// #include <math.h>
// #include <pthread.h>
// #include <sched.h>
// #include <stdio.h>
// #include <stdlib.h>
// #include <string.h>
// #include <sys/time.h>
// #include <time.h>
// #include <unistd.h>

#include <fcntl.h>
#include <unistd.h>
#include <stdio.h>

// #include <sys/types.h>
// #include <sys/stat.h>
// #include <string.h>


#include "ethercat.h"
#include "json-c/json.h"
#include "json-c/json_object.h"

int fp;

#define BUFFSIZE 2048
char buff[BUFFSIZE];
int main() {
    struct json_object *parsed_json;   
    json_tokener *tok;
    tok = json_tokener_new();
    fp = open("../socket/rt_command", O_RDONLY);

    size_t dataLength = read(fp,buff, BUFFSIZE);
    printf("INPUT:  %s", buff);
    parsed_json  = json_tokener_parse_ex(tok,buff,dataLength);
    
    struct json_object *json_cmd;
    json_cmd = json_object_object_get(parsed_json,"cmd");
    printf("CMD: %s \n ", json_object_get_string(json_cmd));
}