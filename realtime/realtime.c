#include <fcntl.h>
#include <math.h>
#include <pthread.h>
#include <sched.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/time.h>
#include <time.h>
#include <unistd.h>

//#include "b64/cencode.h"
#include "b64encoder.c"
#include "ethercat.h"
#include "ethercattype.h"
#include "json-c/json.h"
#include "json-c/json_object.h"
#include "servo-utils.c"

#define NSEC_PER_SEC 1000000000

#define RUN_STATE_EXIT 0
#define RUN_PRE_OP 2
#define RUN_RT_BYPASS 2
#define RUN_RT_PROCESS 3

pthread_cond_t cond = PTHREAD_COND_INITIALIZER;
pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
struct sched_param schedp;
int doRun = 0;

int expectedWKC = 0;

char IOmap[4096];

output_CTBServo_RxPDO_t *out_ctb;
input_CTBServo_TxPDO_t *in_ctb;

/* RT EtherCAT thread */
void realTimeThread(void *ptr) {
    struct timespec wakeUpTime;
    int64 timeOffset = 0;
    int cyclecount = 0;
    int64 nsCycleTime = *(int *)ptr * 1000; /* cycletime in ns */
    
    boolean isPositionSet = FALSE;
    int32 pos; 
    while (doRun > RUN_STATE_EXIT) {
        clock_gettime(CLOCK_MONOTONIC, &wakeUpTime);          // Each cycle calculation
        add_timespec(&wakeUpTime, nsCycleTime + timeOffset);  // set sleep cycle
        clock_nanosleep(CLOCK_MONOTONIC, 1, &wakeUpTime, NULL);

        if (doRun >= RUN_RT_BYPASS) {
            int send = ec_send_processdata();
            int wkc = ec_receive_processdata(200000);

            if ((wkc == expectedWKC) && (doRun >= RUN_RT_PROCESS)) {
                if (!isPositionSet){
                    isPositionSet = TRUE;
                    pos = in_ctb->PositionAV;
                }
                pos += 10000;
                  
                out_ctb->ModesOfOperation = OP_MODE_CYCLIC_SYNC_POSITION;  //
                              
                ec_slavet curslave = ec_slave[1];
                out_ctb->ControlWord = 0x0F;
                out_ctb->TargetPosition = pos;

                cyclecount++;
            }

            ec_sync(ec_DCtime, nsCycleTime,  &timeOffset);
        }
    }
}

#define BUFFSIZE 2048
char JSONBuffer[BUFFSIZE];
char sdobuff[50];
struct json_object *response;

int JsonInterface() {
    int rtCommandPipe;
    int rtStatusPipe;

    struct json_object *json_input;
    json_tokener *tok;
    tok = json_tokener_new();
    // base64_encodestate b64State;
    char *cmd, *request_id;
    char *b64Payload;
    char *encoded = (char *)malloc(2 * BUFFSIZE);
    char *jsonOutput = (char *)malloc(3 * BUFFSIZE);

       

    while (doRun > RUN_STATE_EXIT) {
        rtStatusPipe = open("../socket/rt_status", O_WRONLY);
        rtCommandPipe = open("../socket/rt_command", O_RDONLY);

        printf("ConnectedPipes:\n");
        size_t dataLength = 0;
        while ((dataLength = read(rtCommandPipe, JSONBuffer, BUFFSIZE))) {
            json_input = json_tokener_parse_ex(tok, JSONBuffer, dataLength);
            cmd = (char *)json_object_get_string(json_object_object_get(json_input, "cmd"));
            request_id = (char *)json_object_get_string(json_object_object_get(json_input, "request_id"));

            if (strcmp(cmd, "sdo_read") == 0) {
                uint16 slave = json_object_get_uint64(json_object_object_get(json_input, "slave"));
                uint16 index = json_object_get_uint64(json_object_object_get(json_input, "index"));
                uint8 subindex = json_object_get_uint64(json_object_object_get(json_input, "subIndex"));
                boolean ca = json_object_get_boolean(json_object_object_get(json_input, "CA"));
                int readsize = 1024;
                int wkC = ec_SDOread(slave, index, subindex, ca, &readsize, &sdobuff, 500000000);

                b64_encode(sdobuff, readsize, encoded);

                response = json_object_new_object();
                json_object_object_add(response, "request_id", json_object_new_string(request_id));
                json_object_object_add(response, "payload", json_object_new_string(encoded));
                json_object_to_fd(rtStatusPipe, response, JSON_C_TO_STRING_PLAIN);
                json_object_put(response);
            }

            if (strcmp(cmd, "sdo_write") == 0) {
                uint16 slave = json_object_get_uint64(json_object_object_get(json_input, "slave"));
                uint16 index = json_object_get_uint64(json_object_object_get(json_input, "index"));
                uint8 subindex = json_object_get_uint64(json_object_object_get(json_input, "subIndex"));
                boolean ca = json_object_get_boolean(json_object_object_get(json_input, "CA"));
                char *pld64 = (char *)json_object_get_string(json_object_object_get(json_input, "pld64"));
                int decLength = b64_decode(pld64, sdobuff, (size_t)2048);
                int wkC = ec_SDOwrite(slave, index, subindex, ca, decLength, &sdobuff, 500000000);

                response = json_object_new_object();
                json_object_object_add(response, "request_id", json_object_new_string(request_id));
                json_object_to_fd(rtStatusPipe, response, JSON_C_TO_STRING_PLAIN);
                json_object_put(response);
            }

            if (strcmp(cmd, "ec_status") == 0) {
                response = json_object_new_object();
                json_object_object_add(response, "request_id", json_object_new_string(request_id));
                json_object_object_add(response, "ec_dctime", json_object_new_int64(ec_DCtime));
                json_object_object_add(response, "slave_count", json_object_new_int(ec_slavecount));                
                json_object_to_fd(rtStatusPipe, response, JSON_C_TO_STRING_PLAIN);
                json_object_put(response);
            }


        }

        close(rtCommandPipe);
        close(rtStatusPipe);
    }
}

int errorExit(char *error, int errorCode) {
    printf("ERROR EXIT:>> %s \n", error);
    return errorCode;
}

int main() {
    int cycleTimeUS = 5000;  // cycleTime in MICROSECONDS

    struct sched_param param;
    int policy = SCHED_OTHER;
    doRun = RUN_PRE_OP;
    memset(&param, 0, sizeof(param));

    if (!ec_init("enp2s0")) return errorExit("No socket connection, execute as root", 1);
    if (!(ec_config_init(FALSE) > 0)) return errorExit("No slaves found", 1);
    printf("%d slaves found and configured.\n", ec_slavecount);

    pthread_t curThread;

    // start up the JSON-API
    pthread_create(&curThread, NULL, (void *)&JsonInterface, NULL);
    param.sched_priority = 50;
    pthread_setschedparam(curThread, policy, &param);

    // StartUp the real-time thread.
    pthread_create(&curThread, NULL, (void *)&realTimeThread, (void *)&cycleTimeUS);
    param.sched_priority = 80;
    pthread_setschedparam(curThread, policy, &param);

    ec_config_map(&IOmap);
    ec_configdc();
    ec_dcsync0(1, TRUE, 5000000U, 0);  // Given in naoseconds

    expectedWKC = (ec_group[0].outputsWKC * 2) + ec_group[0].inputsWKC;
    ec_slave[0].state = EC_STATE_SAFE_OP;

    ec_statecheck(0, EC_STATE_SAFE_OP, EC_TIMEOUTSTATE);
    printf("ALL SLAVES REACHED EC_STATE_SAFE_OP\n");
    printf("DC capable : %d\n", ec_configdc());

    out_ctb = (output_CTBServo_RxPDO_t *)ec_slave[1].outputs;
    in_ctb = (input_CTBServo_TxPDO_t *)ec_slave[1].inputs;

    ec_readstate();

    for (int cnt = 1; cnt <= ec_slavecount; cnt++) {
        printf("Slave:%d Name:%s Output size:%3dbits Input size:%3dbits State:%2d delay:%d.%d\n",
               cnt, ec_slave[cnt].name, ec_slave[cnt].Obits, ec_slave[cnt].Ibits,
               ec_slave[cnt].state, (int)ec_slave[cnt].pdelay, ec_slave[cnt].hasdc);
    }

    printf("Request operational state for all slaves\n");

    ec_send_processdata();
    ec_receive_processdata(EC_TIMEOUTRET);

    ec_slave[0].state = EC_STATE_OPERATIONAL;
    ec_writestate(0);

    ec_statecheck(0, EC_STATE_OPERATIONAL, EC_TIMEOUTSTATE);

    if (ec_slave[0].state == EC_STATE_OPERATIONAL) {
        printf("Operational state reached for all slaves.\n");
        doRun = RUN_RT_BYPASS;
        usleep(100 * 1000);               // wait for linux to sync on DC
        ec_dcsync0(1, TRUE, 5000000, 0);  // SYNC0 on slave 1
        usleep(100 * 000);                // wait for linux to sync on DC

        doRun = RUN_RT_PROCESS;
        while (doRun > RUN_STATE_EXIT) usleep(100 * 1000);  // CYCLE THIS THREAD.
    } else {
        doRun = RUN_STATE_EXIT;  // REQUEST OTHER CYCLES TO CLOSE GRACEFULLY
    }

    usleep(100 * 1000);  // Lets wait 100ms to finish.
    schedp.sched_priority = 0;
    sched_setscheduler(0, SCHED_OTHER, &schedp);
    printf("End program\n");
    return (0);

    // ec_SDOwrite(1,0x1c12,01,FALSE,os,&ob2,EC_TIMEOUTRXM);
}

// configure the slave
// map the slave
// configure distributed clock
// go to safe-op
// start pdo data transfer (LRW or LRD/LWR) at the desired DC interval (for example 1ms)
// check for stable DC clock in all slaves (difference timer)
// check for stable master clock (digital PLL locked to reference slave)
// only then request OP