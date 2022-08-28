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
#include "ethercat.h"
#include "json-c/json.h"
#include "json-c/json_object.h"
#include "b64encoder.c"

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

/* RT EtherCAT thread */
void ecatthread(void *ptr) {
    struct timespec wakeUpTime;
    int64 timeOffset = 0;
    int cyclecount = 0;

    int64 nsCycleTime = *(int *)ptr * 1000; /* cycletime in ns */
    clock_gettime(CLOCK_MONOTONIC, &wakeUpTime);
    while (doRun > RUN_EXIT) {
        add_timespec(&wakeUpTime, nsCycleTime + timeOffset);  // set sleep cycle
        clock_nanosleep(CLOCK_MONOTONIC, 1, &wakeUpTime, NULL);

        if (doRun >= RUN_RT_PROCESS) {
            // clock_gettime(CLOCK_MONOTONIC, &tp);
            ec_send_processdata();
            ec_receive_processdata(EC_TIMEOUTRET);
            cyclecount++;

            // printf("ZOUP DUDE %d \n", cyclecount);

            /* calulate toff to get linux time and DC synced */
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

    
    ec_slave[0].state = EC_STATE_SAFE_OP;
    ec_writestate(0);
    if (!ec_statecheck(0, EC_STATE_SAFE_OP, EC_TIMEOUTSTATE)) return errorExit("Not all slaves reached EC_STATE_SAFE_OP", 1);

    /* send one processdata cycle to init SM in slaves */
    ec_send_processdata();
    ec_receive_processdata(EC_TIMEOUTRET);



    ec_slave[0].state = EC_STATE_OPERATIONAL;
    ec_writestate(0);
    if (!ec_statecheck(0, EC_STATE_OPERATIONAL, EC_TIMEOUTSTATE)) return errorExit("Not all slaves reached EC_STATE_OPERATIONAL", 1);
    doRun = RUN_RT_PROCESS;
}

#define BUFFSIZE 2048
char buff[BUFFSIZE];
char sdobuff[50];
struct json_object *cmdResponse;



int commandListener() {
    int rtCommandSocket;
    struct json_object *parsed_json;
    json_tokener *tok;
    tok = json_tokener_new();
    //base64_encodestate b64State;  
    char *cmd, *cmdId;
    char *b64Payload;
    char* encoded = (char*)malloc(2*BUFFSIZE);

    while (doRun > RUN_EXIT) {
        rtCommandSocket = open("../socket/rt_command", O_RDONLY);
        //printf("STR READING:\n");
        size_t dataLength = 0;
        while ((dataLength = read(rtCommandSocket, buff, BUFFSIZE))) {
            //printf("JSON: %s\n", buff);
            // continue;

            parsed_json = json_tokener_parse_ex(tok, buff, dataLength);
            cmd = (char *)json_object_get_string(json_object_object_get(parsed_json, "cmd"));
            cmdId = (char *)json_object_get_string(json_object_object_get(parsed_json, "request_id"));

            if (strcmp(cmd, "sdo_read") == 0) {
                uint16 slave = json_object_get_uint64(json_object_object_get(parsed_json, "slave"));
                uint16 index = json_object_get_uint64(json_object_object_get(parsed_json, "index"));
                uint8 subindex = json_object_get_uint64(json_object_object_get(parsed_json, "subIndex"));
                boolean ca = json_object_get_boolean(json_object_object_get(parsed_json, "CA"));
                int readsize = 1024;
                int wkC = ec_SDOread(slave, index, subindex, FALSE, &readsize, &sdobuff, 500000000);
               
                //base64_init_encodestate(&b64State);
                b64_encode(sdobuff,readsize,encoded);
                //int cnt = base64_encode_block(sdobuff, readsize, encoded, &b64State);
                //cnt = base64_encode_blockend(encoded, &b64State);

                cmdResponse = json_object_new_object();
                json_object_object_add(cmdResponse, "type", json_object_new_string((char *)"response_sdo_read"));
                json_object_object_add(cmdResponse, "request_id", json_object_new_string(cmdId));
                json_object_object_add(cmdResponse, "payload", json_object_new_string(encoded));
                json_object_to_file("/root/csharp_soem/socket/rt_status", cmdResponse);
                json_object_put(cmdResponse);
            }
        }
        close(rtCommandSocket);
    }
}

int main() {
    int cycleTimeUS = 5000;  // cycleTime
    // cycleTimeUS = 1 * 1000 * 1000;  // set to one full second

    struct sched_param param;
    int policy = SCHED_OTHER;
    doRun = RUN_RT_BYPASS;
    memset(&param, 0, sizeof(param));

    if (!ec_init("enp2s0")) return errorExit("No socket connection, execute as root", 1);
    if (!(ec_config_init(FALSE) > 0)) return errorExit("No slaves found", 1);

    //GoOperational();  // DELETE WHEN GOING RT
    commandListener();

    // commandListenerThread
    pthread_t curThread;
    pthread_create(&curThread, NULL, (void *)&commandListener, NULL);
    param.sched_priority = 50;
    pthread_setschedparam(curThread, policy, &param);

    pthread_create(&curThread, NULL, (void *)&ecatthread, (void *)&cycleTimeUS);
    param.sched_priority = 80;
    pthread_setschedparam(curThread, policy, &param);

    // usleep(500 * 1000);

    GoOperational();
    while (doRun > RUN_EXIT)
        usleep(1000);

    schedp.sched_priority = 0;
    sched_setscheduler(0, SCHED_OTHER, &schedp);
    printf("End program\n");
    return (0);

    // ec_SDOwrite(1,0x1c12,01,FALSE,os,&ob2,EC_TIMEOUTRXM);
}
