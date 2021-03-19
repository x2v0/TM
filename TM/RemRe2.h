#ifndef REMRE2_H
#define REMRE2_H

/* *** AuxIOS *** */
enum ReqSrvOut { 
R2_ZERO = 0x80,
R2_RINFO,				//1 Read Information Events (Status,Errors mesages) 
R2_EMPTY,				//2 Empty request, only for CallBack ?
R2_RPERI,				//3 Get Cycle Period
R2_WPERI,				//4 Set Cycle Period
R2_RIDAC,				//5
R2_WIDAC,				//6
R2_RSDEV,				//7	Serial Devices
R2_WROTP = R2_ZERO+12,	// Write rotation parameters
R2_WROTC,				// Rotation control
R2_RKRES,				// Read Patient Armchair parameters
R2_WKRES,				// Write Patient Armchair parameters
R2_LAST2};

#endif