#include <osal.h>

/**
 * SOEM EtherCAT CiA402 example
 * Author: Ho Tam - thanhtam.h[at]gmail.com
 *
 * Description: implementation of CiA402 state machine for Servo On
 * Input: current status word read from servo drive
 * Output: next control word or command will be sent to servo drive in next cycle
 * Return value: drive has been in Operation or not
 */

#include "servo_def.h"
#define NSEC_PER_SEC 1000000000

int ServoOn_GetCtrlWrd(uint16_t StatusWord, uint16_t *ControlWord) {
    int _enable = 0;
    if (bit_is_clear(StatusWord, STATUSWORD_OPERATION_ENABLE_BIT)) {            // Not ENABLED yet
        if (bit_is_clear(StatusWord, STATUSWORD_SWITCHED_ON_BIT)) {             // Not SWITCHED ON yet
            if (bit_is_clear(StatusWord, STATUSWORD_READY_TO_SWITCH_ON_BIT)) {  // Not READY to SWITCH ON yet
                if (bit_is_set(StatusWord, STATUSWORD_FAULT_BIT)) {             // FAULT exist
                    (*ControlWord) = 0x80;                                      // =>>FAULT RESET command
                } else {                                                        // NO FAULT
                    (*ControlWord) = 0x06;                                      // =>>SHUTDOWN command (transition#2)
                }
            } else {                    // READY to SWITCH ON
                (*ControlWord) = 0x07;  // =>>SWITCH ON command (transition#3)
            }
        } else {                    // has been SWITCHED ON
            (*ControlWord) = 0x0F;  //=>>ENABLE OPERATION command (transition#4)
            _enable = 1;
        }
    } else {                    // has been ENABLED
        (*ControlWord) = 0x0F;  // =>>maintain OPERATION state
        _enable = 1;
    }
    return _enable;
    ;
}


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
void ec_sync(int64 reftime, int64 cycletime,  int64 *offsettime) {
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