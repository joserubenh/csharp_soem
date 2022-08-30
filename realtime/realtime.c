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

#define NSEC_PER_SEC 1000000000

#define RUN_EXIT 0
#define RUN_RT_BYPASS 1
#define RUN_RT_PROCESS 2

pthread_cond_t cond = PTHREAD_COND_INITIALIZER;
pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
struct sched_param schedp;
int doRun = 0;

char IOmap[4096];

void add_timespec(struct timespec *ts, int64 addTimeNanoseconds) {
    int64 sec, nsec;

    nsec = addTimeNanoseconds % NSEC_PER_SEC;
    sec = (addTimeNanoseconds - nsec) / NSEC_PER_SEC;
    ts->tv_sec += sec;
    ts->tv_nsec += nsec;
    if (ts->tv_nsec > NSEC_PER_SEC) {
        nsec = ts->tv_nsec % NSEC_PER_SEC;
        ts->tv_sec += (ts->tv_nsec - nsec) / NSEC_PER_SEC;
        ts->tv_nsec = nsec;
    }
}

void set_timespec(struct timespec *ts, int64 setNanoseconds) {
    ts->tv_nsec = 0;
    ts->tv_sec = 0;
    add_timespec(ts, setNanoseconds);
}

int64 integral = 0;
void ec_sync(int64 reftime, int64 cycletime, int64 *offsettime) {
    /* PI calculation to get linux time synced to DC time */
    /* set linux sync point 50us later than DC sync, just as example */
    int64 delta = (reftime - 50000) % cycletime;
    if (delta > (cycletime / 2)) delta = delta - cycletime;
    if (delta > 0) integral++;
    if (delta < 0) integral--;
    *offsettime = -(delta / 100) - (integral / 20);
}

typedef struct PACKED {
    uint16 ControlWord;          // 0x6040 10	16	UINT	ControlWorld
    uint8 ModesOfOperation;      // 0x6060 08	8	USINT	Modes of operation
    int32 TargetVelocity;        // 0x60ff 20	32	DINT	Target velocity
    int32 TargetPosition;        // 0x607a 20	32	DINT	Target position
    int16 TargetTorque;          // 0x6071 10	16	INT	    Target torque
    uint32 DigitalOutputs;       // 0x60fe 20	32	UDINT	Digital outputs
    uint16 TouchProbeFunction;   // 0x60b8 10	16	UINT	Touch probe function
    int16 Analog1OutputControl;  // 0x6100 10	16	INT	    Analog1 Output Control
    int16 Analog2OutputControl;  // 0x6101 10	16	INT	    Analog2 Output Control
} output_CTBServo_RxPDO_t;

typedef struct PACKED {
    uint16 ErrorCode;                     // 0x603f0 010	16	UINT	Error code
    uint16 StatusWord;                    // 0x60410 010	16	UINT	statusword
    uint8 ModesOfOperationDisplay;        // 0x60610 008	8	USINT	Modes of operation display
    int32 VelocityAV;                     // 0x606c0 020	32	DINT	Velocity AV
    int32 PositionAV;                     // 0x60640 020	32	DINT	Position AV
    int16 TorqueAV;                       // 0x60770 010	16	INT	    Torque AV
    int16 CurrentAV;                      // 0x60780 010	16	INT	    Current AV
    int32 FollowingErrorAV;               // 0x60f40 020	32	DINT	Following error AV
    uint32 IgitalInputs;                  // 0x60fd0 020	32	UDINT	Digital inputs
    uint16 TouchProbeStatus;              // 0x60b90 010	16	UINT	Tourch probe status
    int32 TouchProbePos1PosValue;         // 0x60ba0 020	32	DINT	Touch Probe pos1 pos value
    int32 TourchProbePos1NegValue;        // 0x60bb0 020	32	DINT	Touch probe pos1 neg value
    int32 TouchProbePos2PosValue;         // 0x60bc0 020	32	DINT	touch probe pos2 pos value
    int32 TouchProbePos2NegValue;         // 0x60bd0 020	32	DINT	Touch probe pos2 neg value
    int16 AnalogInputMonitoring;          // 0x62000 010	16	INT	    Analog Input Monitoring
    int16 Analog2InputMonitoring;         // 0x62010 010	16	INT	    Analog2 Input Monitoring
    int32 T4VelocitySensorActualValue;    // 0x60690 020	32	DINT	T4 velocity sensor actual value
    int32 T4PositionActualInternalValue;  // 0x60630 020	32	DINT	T4 position actual internal value
} input_CTBServo_TxPDO_t;

output_CTBServo_RxPDO_t *out_ctb;
input_CTBServo_TxPDO_t *in_ctb;

/* RT EtherCAT thread */
void ecatthread(void *ptr) {
    struct timespec wakeUpTime;
    int64 timeOffset = 0;
    int cyclecount = 0;
    int64 nsCycleTime = *(int *)ptr * 1000; /* cycletime in ns */
    // clock_gettime(CLOCK_MONOTONIC, &wakeUpTime);
    // int32 DebugPos = 0;

    while (doRun > RUN_EXIT) {
        clock_gettime(CLOCK_MONOTONIC, &wakeUpTime);          // Each cycle calculation
        add_timespec(&wakeUpTime, nsCycleTime + timeOffset);  // set sleep cycle
        clock_nanosleep(CLOCK_MONOTONIC, 1, &wakeUpTime, NULL);
        // printf(".");

        if (doRun >= RUN_RT_PROCESS) {
            ec_send_processdata();
            ec_receive_processdata(EC_TIMEOUTRET);

            // DebugPos += 2000;
            // in_ctb->ModesOfOperation = 3; //ProfileVelocity
            out_ctb->ModesOfOperation = 8;  // CSP (Cyclic synchronous position)

            out_ctb->ControlWord = 0x0F;
            // in_ctb->TargetVelocity = 100;
            out_ctb->TargetPosition = in_ctb->PositionAV;
            ;

            cyclecount++;

            ec_sync(ec_DCtime, nsCycleTime, &timeOffset);
        }
    }
}

int errorExit(char *error, int errorCode) {
    printf("ERROR EXIT:>> %s \n", error);
    return errorCode;
}

int GoOperational() {
    ec_config_map(&IOmap);
    ec_configdc();
   printf("DC capable : %d\n",ec_configdc());

    // MAPPING FOR DEBUGGING
    out_ctb = (output_CTBServo_RxPDO_t *)ec_slave[1].outputs;
    in_ctb = (input_CTBServo_TxPDO_t *)ec_slave[1].inputs;

    ec_dcsync0(1, TRUE, 5 * 1000, 0);  // Given in naoseconds

    ec_slave[0].state = EC_STATE_SAFE_OP;
    ec_writestate(0);
    if (!ec_statecheck(0, EC_STATE_SAFE_OP, EC_TIMEOUTSTATE)) return errorExit("Not all slaves reached EC_STATE_SAFE_OP", 1);

    ec_readstate();
    /* send one processdata cycle to init SM in slaves */
    ec_send_processdata();
    ec_receive_processdata(EC_TIMEOUTRET);

    // ec_dcsync0(1, TRUE, 5*1000, 0); //Given in naoseconds

    ec_slave[0].state = EC_STATE_OPERATIONAL;
    ec_writestate(0);
    if (!ec_statecheck(0, EC_STATE_OPERATIONAL, EC_TIMEOUTSTATE)) return errorExit("Not all slaves reached EC_STATE_OPERATIONAL", 1);
    doRun = RUN_RT_PROCESS;

    printf("ALL SLAVES REACHED OPERATIONAL STATE\n");
}

#define BUFFSIZE 2048
char buff[BUFFSIZE];
char sdobuff[50];
struct json_object *response;

int jsonCommandInterface() {
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

    while (doRun > RUN_EXIT) {
        rtStatusPipe = open("../socket/rt_status", O_WRONLY);
        rtCommandPipe = open("../socket/rt_command", O_RDONLY);

        printf("ConnectedPipes:\n");
        size_t dataLength = 0;
        while ((dataLength = read(rtCommandPipe, buff, BUFFSIZE))) {
            json_input = json_tokener_parse_ex(tok, buff, dataLength);
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
        }

        close(rtCommandPipe);
        close(rtStatusPipe);
    }
}

int main() {
    int cycleTimeUS = 5000;  // cycleTime in MICROSECONDS
    // cycleTimeUS = 1 * 1000 * 1000;  // set to one full second

    struct sched_param param;
    int policy = SCHED_OTHER;
    doRun = RUN_RT_BYPASS;
    memset(&param, 0, sizeof(param));

    if (!ec_init("enp2s0")) return errorExit("No socket connection, execute as root", 1);
    if (!(ec_config_init(FALSE) > 0)) return errorExit("No slaves found", 1);

    // GoOperational();  // DELETE WHEN GOING RT
    // commandListener();

    // commandListenerThread
    pthread_t curThread;
    pthread_create(&curThread, NULL, (void *)&jsonCommandInterface, NULL);
    param.sched_priority = 50;
    pthread_setschedparam(curThread, policy, &param);

    pthread_create(&curThread, NULL, (void *)&ecatthread, (void *)&cycleTimeUS);
    param.sched_priority = 80;
    pthread_setschedparam(curThread, policy, &param);

    // usleep(500 * 1000);

    GoOperational();
    while (doRun > RUN_EXIT) usleep(1000);

    schedp.sched_priority = 0;
    sched_setscheduler(0, SCHED_OTHER, &schedp);
    printf("End program\n");
    return (0);

    // ec_SDOwrite(1,0x1c12,01,FALSE,os,&ob2,EC_TIMEOUTRXM);
}
