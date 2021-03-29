// Sdev.c	v 2.0.2
// 21.05.2015	redirrect to slave command used now in SetMBPScur(), GetRedirrectToSlaveSts() function added

#include <ansi_c.h>
#include "lnklb.h"
#include "Sdev.h"

#include "RemReq.h"
#if 0
#include "RemRe2.h"
#define RQRSDEV	R2_RSDEV
#else
#define RQRSDEV	RQ_RSDEV
#endif


static int CmdToMasterX(int adr,int fnc,int cmd,int data,int cnt,int rcn,int tmo,int (*sdevCB)(PKT *) ) {
	SDEV dp;
	char *datp = (char *) &data;
	dp.seg = adr>>8;
	dp.tmo = tmo;
	dp.rcn = rcn;									// Max Answer Size
	dp.wcn = 0;										// bytes to write counter
	dp.udt[dp.wcn++] = (unsigned char) adr;			// Master address
	dp.udt[dp.wcn++] = (unsigned char) cmd;			// Get ADC data Command for DynPS Slave controller
	if(cnt > 4)
		cnt = 4;
	while (cnt-- > 0)
		dp.udt[dp.wcn++] = *datp++;
	return PktWrC(RQRSDEV|FN_ANS|fnc, &dp,sizeof(dp), sdevCB);
}

static int CmdToSlaveX(int adr,int fnc,int slave_ch,int cmd,int data,int cnt,int rcn,int tmo,int (*sdevCB)(PKT *) ) {
	SDEV dp;
	char *datp = (char *) &data;
	dp.seg = adr>>8;
	dp.tmo = tmo;
	dp.rcn = rcn;									// Max Answer Size
	dp.wcn = 0;										// bytes to write counter
	dp.udt[dp.wcn++] = (unsigned char) adr;			// Master address
	dp.udt[dp.wcn++] = 0x6;							// Command: Redirect to slave
	dp.udt[dp.wcn++] = (unsigned char) slave_ch;	// Channel
	dp.udt[dp.wcn++] = (unsigned char) cmd;			// Get ADC data Command for DynPS Slave controller
	if(cnt > 4)
		cnt = 4;
	while (cnt-- > 0)
		dp.udt[dp.wcn++] = *datp++;
	return PktWrC(RQRSDEV|FN_ANS|fnc, &dp,sizeof(dp), sdevCB);
}


int GetSDevAdr(PKT *pk) {
	SDEV *dp = (SDEV *) &pk->d;
	return (((unsigned char) dp->seg)<<8) + (unsigned char) dp->udt[0];
}

static double GetSDevCh(char *udt,int ch) {
	double dat = * ((unsigned int *) &udt[7+ch*8]);
	unsigned short scn = * (unsigned short *) &udt[5+ch*8];
	if( scn )
		dat /= scn;
	else
		dat = 0.;
	return dat;
}

int GetMSsts(PKT *pk,int *adr,int *ch,int opch) {
SDEV *dp = (SDEV *) &pk->d;
	if(opch < 1) 
		opch = 1;
	*adr = (((unsigned char) dp->seg)<<8) + (unsigned char) dp->udt[0];
	if(pk->sts) {
		*ch =  ((unsigned char) dp->udt[2]);	// use data from innitial command format in case of error
		if(dp->udt[1] == 0x11)
			*ch /= opch;
	}else{
		*ch =  ((unsigned char) dp->udt[3]);
		if(dp->udt[1] ==0x11)
			*ch /= opch;
	}	
	return pk->sts;
}


int GetMBPSsts(PKT *pk,int *adr,int *ch) {	// Return Status 0 == ok;
	return GetMSsts(pk,adr,ch,2);
}


int GetRedirrectToSlaveSts(PKT *pk,int *adr,int *ch) {	// status, address, channel for redirect to slave answer
SDEV *dp = (SDEV *) &pk->d;
	*adr = (((unsigned char) dp->seg)<<8) + (unsigned char) dp->udt[0];	// dp->udt[1(cmd)] == 6
	*ch =  ((unsigned char) dp->udt[2]);					// slave channel
	if(pk->sts)
		return pk->sts;			// communication error, address and channel information is valid !
	if(dp->udt[1] & 0x80)			// MSB in command byte means error 
		return 0xff + (dp->udt[2]<<8);	// master return error, error code in dp->udt[2]
	return 0;
}

int SetVBPMPSmode(int adr, char data, int (*mbpsCB)(PKT *)) {
	SDEV dp;
	int cnt=1;								// data size (bytes)
	char *datp = &data;						// pointer to data
	dp.seg = adr>>8;
	dp.tmo = 10;
	dp.rcn = 5;								// Max Answer Size
	dp.wcn = 0;								// bytes to write counter
	dp.udt[dp.wcn++] = (unsigned char) adr;	// Master address
	dp.udt[dp.wcn++] = (unsigned char) 0x12;// Set mode
	while (cnt-- > 0)
		dp.udt[dp.wcn++] = *datp++;
	return PktWrC(RQRSDEV|FN_ANS|FN_WEV, &dp,sizeof(dp), mbpsCB);
}

int SetVBPMPScur(int adr, char data, int (*mbpsCB)(PKT *)) {
	SDEV dp;
	int cnt=1;								// data size (bytes)
	char *datp = &data;						// pointer to data
	dp.seg = adr>>8;
	dp.tmo = 10;
	dp.rcn = 5;								// Max Answer Size
	dp.wcn = 0;								// bytes to write counter
	dp.udt[dp.wcn++] = (unsigned char) adr;	// Master address
	dp.udt[dp.wcn++] = (unsigned char) 0x10;// Activate PS
//	if(cnt > 4)
//		cnt = 4;
	while (cnt-- > 0)
		dp.udt[dp.wcn++] = *datp++;
	return PktWrC(RQRSDEV|FN_ANS|FN_WEV, &dp,sizeof(dp), mbpsCB);
}
void SetMBPScur(int adr, int ch, int dat, int (*mbpsCB)(PKT *)) {
#if 0
	SDEV dp;
	dp.seg = adr>>8;
	dp.tmo = 0;
	dp.rcn = 20;								// Answer must be less equal .. bytes
	dp.wcn = 0;									// Number of bytes to write
	dp.udt[dp.wcn++] = (unsigned char) adr;			// Master address
	dp.udt[dp.wcn++] = (unsigned char) 0x10;		// Command
	dp.udt[dp.wcn++] = (unsigned char) ch;
	dp.udt[dp.wcn++] = (unsigned char) dat;			// LSBYTE of current
	dp.udt[dp.wcn++] = (unsigned char) (dat>>8);	// MSBYTE of current
	PktWrC(RQRSDEV|FN_WEV|FN_ANS,&dp,sizeof(dp),mbpsCB);
#else
/////	CmdToMasterX(adr,FN_WEV,0x10,(ch&0xff)|(dat<<8),3,20,0,mbpsCB);	// use master to set DAC in slave module
	CmdToSlaveX(adr,FN_WEV,ch,0x10,dat<<8,3,20,3,mbpsCB);		// use master to redirrect command to slave module
#endif
}

int ReadMS(int adr,int ch ,int (*mbpsCB)(PKT *)) {	// Send command to read data and set CallBack
// Read Channel data
	return CmdToMasterX(adr,FN_WEV,0x11,ch,1,20,10,mbpsCB);
}

int ReadMBPS(int adr,int ch ,int (*mbpsCB)(PKT *)) {
	return ReadMS(adr,ch*2 ,mbpsCB);
}


int GetMBPScur(PKT *pk,int *adr,int *ch,unsigned short *dat) {	// Return Status 0 == ok;
	if( GetMBPSsts(pk,adr,ch) )
		return pk->sts;
	if(((SDEV *) &pk->d)->rcn < 10)
		return -1;
	*dat =  ((unsigned short) GetSDevCh(((SDEV *) &pk->d)->udt,0)) << 6;
	return 0;
}

void SetMUVScur(int adr, int ch, int dat, int (*muvsCB)(PKT *)) {
	SetMBPScur(adr,ch,dat,muvsCB);			// Control format the same as in MBPS
}

int GetMUVSsts(PKT *pk,int *adr,int *ch) {	// Return Status 0 == ok;
	return GetMSsts(pk,adr,ch,3);
}

int ReadMUVS(int adr,int ch ,int (*mbpsCB)(PKT *)) {
	return ReadMS(adr,ch*3,mbpsCB);
}

int GetMUVScur(PKT *pk,int *adr,int *ch,unsigned short *dat) {	// Return Status 0 == ok;
	if( GetMUVSsts(pk,adr,ch) )
		return pk->sts;
	if(((SDEV *) &pk->d)->rcn < 10)
		return -1;
	*dat =  ((unsigned short) GetSDevCh(((SDEV *) &pk->d)->udt,0)) << 6;
	return 0;
}

void SetDynPSOnOff(int adr, int chan, int sts) {	// On/Off dynamic power supply (10.A)
	CmdToSlaveX(adr,FN_WEV,chan,0x14,sts,1,10,0,NULL );
}

int ReadDynPSstat(int adr, int chan, int (*sdevCB)(PKT *)) {// Read dynamic power supply status
	return CmdToSlaveX(adr,FN_WEV,chan,0x16,0,0,10,0,sdevCB);
}

int ReadDynPScur(int adr, int chan, int (*sdevCB)(PKT *)) {	
// Read dynamic power supply current (Slave ADC channel 0)
	return CmdToSlaveX(adr,FN_CYC,chan,0x11,0,0,10,0,sdevCB);
}

void SetD4A4do(int adr,int ss) {	// Start & Stop
	CmdToMasterX(adr, FN_WEV, 0x14 ,ss & 1, 1, 8,0,NULL );
}

// Set ADC parameters in ms (Real Time unit is 0.4 ms), synch_delay in milliseconds. synch_tmo in seconds	// 14.02.2014
// if synch_tmo == 0 - asynchronious else synch.
int SetADCPrms(int adr,float average_time,float synch_tmo,float synch_delay, int Nch) {
	SDEV dp;
	int sdl = (int) (synch_delay/0.064 + 0.5);;
	dp.seg = adr>>8;
	dp.tmo = 0;
	dp.rcn = 8;								// Max Answer size
	dp.wcn = 0;								// Bytes to write
	dp.udt[dp.wcn++] = (unsigned char) adr;	// Device address
	dp.udt[dp.wcn++] = 0x12;				// Command: Set ADC parameters
	dp.udt[dp.wcn++] = (unsigned char) (synch_tmo+0.5);				// ssto time out for start 0 - asynch in sec (default = 20 sec)
	dp.udt[dp.wcn++] = (unsigned char) (sdl&0xff);					// ssdl start delay in 64 mks
	dp.udt[dp.wcn++] = (unsigned char) ((sdl>>8)&0xff);				// ssdh 
	dp.udt[dp.wcn++] = (unsigned char) (average_time/0.4 + 0.5);	// Averaging time in 0,4 ms quantums
	
	for(sdl = 0;sdl < Nch; sdl++)
		dp.udt[dp.wcn++] = 0;				// Average counter for each channel (0 == 256)
	
	PktWr(RQRSDEV|FN_WEV,&dp,sizeof(dp));
	return 0;
}

// Install CallBack & Post request to Get measured ADC data (All channels)
int ReadD4A4adc(int adr, int (*d4a4CB)(PKT *)) {
	return CmdToMasterX(adr, FN_CYC, 0x11 ,0xff,1, 120,0,d4a4CB );
}

int GetD4A4adc(PKT *pk,int *adr,int *adcsts,int *usts,double *dat, int Nch) {	// Return Status 0 == ok;
	int i,cn;
	SDEV *dp = (SDEV *) &pk->d;
	*adr = GetSDevAdr(pk);
	if(pk->sts)
		return pk->sts;
	if(dp->rcn < 10)
		return -1;
#if 0		// 1 - OLD	
	*usts = (unsigned char) dp->udt[2];	// Asynch/synch regim bit 0x40, Clocks on = 0x20, 0x80 - Error
	*adcsts = 0x40;
#else
	*adcsts = (unsigned char) dp->udt[2];	// Asynch/synch regim bit 0x40, Clocks on = 0x20, 0x80 - Error
	if((dp->rcn-3)%8 == 1)
		*usts = dp->udt[dp->rcn - 1];			// Last byte in packet is USG status
	else 
		*usts = 0;
#endif
	
	cn = (dp->rcn - 3) / 8;				// Number of channels arrived
//	*cnt = 1;
	memset(dat,0,sizeof(*dat)*Nch); 	// Clear output buffer
	for(i = 0; i < cn; i++) {
		if(i < Nch)
			dat[i] = GetSDevCh(dp->udt,i);
	}
	return 0;
}

void SetSerDac(int adr,int dat, int (*mbpsCB)(PKT *)) {
#if 0
	SDEV dp;
	dp.seg = adr>>8;
	dp.tmo = 0;
	dp.rcn = 8;										// Answer must be less equal .. bytes
	dp.wcn = 0;
	dp.udt[dp.wcn++] = (unsigned char) adr;			// Master address
	dp.udt[dp.wcn++] = 0x10;						// Command group	 0x13
	dp.udt[dp.wcn++] = (unsigned char) 0;
	dp.udt[dp.wcn++] = (unsigned char) (dat);		// LSBYTE of voltage
	dp.udt[dp.wcn++] = (unsigned char) (dat>>8);	// MSBYTE of voltage
	PktWrC(RQRSDEV|FN_WEV|FN_ANS,&dp,sizeof(dp),mbpsCB);
#else
	CmdToMasterX(adr,FN_WEV, 0x10 ,dat<<8,3, 8,0,mbpsCB);
#endif
}

int ReadD16dac(int fnc,int adr,int (*d16CB)(PKT *)) {
// Read Channel 1 - This is ADC from Analog HV
	return CmdToMasterX(adr,fnc, 0x11 ,1,1, 20,0,d16CB );
}

int GetD16sts(PKT *pk, int *adr) {		// Return Status 0 == ok;
	*adr = GetSDevAdr(pk);
	return pk->sts;
}

int GetD16dac(PKT *pk,int *adr,unsigned short *dat) {	// Return Status 0 == ok;
	SDEV *dp = (SDEV *) &pk->d;
	*adr = GetSDevAdr(pk);
	if(pk->sts)
		return pk->sts;
	if(dp->rcn < 11)
		return -1;
	*dat = (unsigned short) (GetSDevCh(dp->udt,0)*64);
	return 0;
}

// Install CallBack & Post request to Get measured HALL sensor data
int ReadHALLsns(int fnc,int adr,int (*hallCB)(PKT *)) {
	return CmdToMasterX(adr,fnc, 0x11 ,0,1, 20,2,hallCB );	// Channel 0 - MFS
}

static double gdHALLsns(SDEV *dp) {
	int cn;
	double dat = 0.;
	if( (cn = *(unsigned short *) &dp->udt[5]) <= 0)
		return dat;
	dat = (double) (* ((int *) &dp->udt[7])) / cn;
	return dat-8192.;
}

// Extract double with hall sensor data from packet. 
// *cnt - average counter.
int GetHALLsns(PKT *pk,int *adr,int *cnt,double *dat) {	// Return Status 0 == ok;
	SDEV *dp = (SDEV *) &pk->d;
	*adr = GetSDevAdr(pk);
	if(pk->sts)
		return pk->sts;
	if(dp->rcn < 8)
		return -1;
	*cnt = *(unsigned short *) &dp->udt[5];
	*dat = gdHALLsns(dp);
	return 0;
}

void SetDeflOnOff(int adr,int OnOff) {
	SetD4A4do(adr,OnOff);					// Bit.1 ? On : Off
}

int ReadDeflVolt(int adr, int (*deflCB)(PKT *)) {	// Read deflector voltage
	return CmdToMasterX(adr,FN_CYC,0x11,0xff, 1,100,0,deflCB);	// 0xff - all channels
}

int GetDeflData(PKT *pk,int *adr, short *cur, short *vlt) {	// Return Status 0 == ok;
	SDEV *dp = (SDEV *) &pk->d;
	*adr = GetSDevAdr(pk);
	if(pk->sts)
		return pk->sts;
	*cur = (short) (GetSDevCh(dp->udt,0) + 0.5);
	*vlt = (short) (GetSDevCh(dp->udt,1) + 0.5);
	return 0;
}

int ReadMasterStat(int adr, int (*deflCB)(PKT *)) {	// Read shutter status
	return CmdToMasterX(adr,FN_WEV,0x16,0,0,10,0,deflCB);
}

int ReadSDevID(int adr, int (*getIdCB)(PKT *)) {
	return CmdToMasterX(adr,FN_WEV,0x00,0,0,4,0,getIdCB);	// 0x00  - Command: Read ID
}

int GetSDevID(PKT *pk, int *adr, int *id) {
	SDEV *dp = (SDEV *) &pk->d;
	*adr = GetSDevAdr(pk);
	if(pk->sts)
		return pk->sts;
	*id = *((unsigned short *) &dp->udt[2]);
	return 0;
}

int ReadSDevVersion(int adr, int (*getvrsCB)(PKT *)) {
	SDEV dp;
	return CmdToMasterX(adr,FN_WEV,0x01,0,0,(int) sizeof(dp.udt),0,getvrsCB);	// 0x01  - Command: Read Version
}

int GetSDevVersion(PKT *pk, int *adr, char **vrs) {
	SDEV *dp = (SDEV *) &pk->d;
	*adr = GetSDevAdr(pk);
	if(pk->sts)
		return pk->sts;
	if(dp->rcn < 3)
		return -1;
	if(dp->rcn > sizeof(dp->udt))
		dp->udt[sizeof(dp->udt)-1] = 0;
	else
		dp->udt[dp->rcn] = 0;
	*vrs = (char *) &dp->udt[2];
	return 0;
}

int rSDversion(int adr,char *vrs,int sz) {
	SDEV dp;
	PKH ph;
	int sts;
	
	memset(vrs,0,sz);
	
	dp.seg = adr>>8;
	dp.tmo = 0;
	dp.wcn = 2;
	dp.rcn = (int) sizeof(dp.udt) - 1;		// Answer must be less equal .. bytes
	dp.udt[0] = (unsigned char) adr;	// Address
	dp.udt[1] = 0x01;					// Command
	if((sts = IOSrvRd(RQRSDEV|FN_ANS|FN_WEV,&dp,sizeof(dp),&ph,&dp,sizeof(dp))) != 0)
		return sts;
	if(ph.sts)
		return ph.sts;
	if(dp.rcn - 1 < sz)
		sz = dp.rcn - 1;
	memcpy(vrs,&dp.udt[2],sz);
	return 0;
}

int rSDid(int adr,int *id) {
	SDEV dp;
	PKH ph;
	int sts;
	
	*id = -1;
	
	dp.seg = adr>>8;
	dp.tmo = 0;
	dp.wcn = 2;
	dp.rcn = 4;							// Answer must be less equal .. bytes
	dp.udt[0] = (unsigned char) adr;	// Address
	dp.udt[1] = 0x00;					// Command
	if((sts = IOSrvRd(RQRSDEV|FN_ANS|FN_WEV,&dp,sizeof(dp),&ph,&dp,sizeof(dp))) !=0)
		return sts;
	if(ph.sts)
		return ph.sts;
	if(dp.rcn  < 4)
		return -1;
	*id = *((unsigned short *) &dp.udt[2]);
	return 0;
}

int DynPS_On_Off(int adr,int ch,int onoff,int (*bpsCB)(PKT *)) {
// On/Off Command for DynPS Slave controller
	return CmdToSlaveX(adr,FN_WEV,ch,0x14,onoff,1,10,0,bpsCB);	
}

int DynPS_GetSts(int adr,int ch,int (*bpsCB)(PKT *)) {
// Get Status Command for DynPS Slave controller
	return CmdToSlaveX(adr,FN_WEV,ch,0x16,0,0,10,0,bpsCB);
}

int DynPS_Sts(PKT *pk) {
	if(pk->sts)
		return -1;								// Error
	return ((SDEV *) &pk->d)->udt[4] & 0xff;	// Status byte	(enum in sdev.h)
}			

int DynPS_GetADC(int adr,int ch,int ch_adc,int (*bpsCB)(PKT *)) {
// Get ADC data Command for DynPS Slave controller
	return CmdToSlaveX(adr,FN_WEV,ch,0x11,ch_adc,1,100,0,bpsCB);
}


int GetSlaveCh(PKT *pk) {
	return ((SDEV *) &pk->d)->udt[2];
}

int GetMasterAdr(PKT *pk) {
	return ((SDEV *) &pk->d)->udt[0];
}


int DynPS_ADCval(PKT *pk, double *Val) {		// Parse Answer to get ADC value (Only one channel !!!) 
	*Val = GetSDevCh(((SDEV *) &pk->d)->udt+2,0);
	return pk->sts;
}

// Set ADC parameters in ms (Real Time unit is 0.4 ms), synch_delay in milliseconds. synch_tmo in seconds	// 14.02.2014
// if synch_tmo == 0 - asynchronious else synch.
int SetADCPrmsSlave(int adr,int slave_ch,float average_time,float synch_tmo,float synch_delay, int Nch) {
	SDEV dp;
	int sdl = (int) (synch_delay/0.064 + 0.5);;
	dp.seg = (unsigned char)(adr>>8);
	dp.tmo = 0;
	dp.rcn = 10;									// Max Answer size
	dp.wcn = 0;										// Bytes to write
	dp.udt[dp.wcn++] = (unsigned char) adr;			// Device address
	dp.udt[dp.wcn++] = 0x6;							// Command: Redirect to slave
	dp.udt[dp.wcn++] = (unsigned char) slave_ch;	// Channel
	dp.udt[dp.wcn++] = 0x12;						// Command: Set ADC parameters
	dp.udt[dp.wcn++] = (unsigned char) (synch_tmo+0.5);				// ssto time out for start 0 - asynch in sec (default = 20 sec)
	dp.udt[dp.wcn++] = (unsigned char) (sdl&0xff);					// ssdl start delay in 64 mks
	dp.udt[dp.wcn++] = (unsigned char) ((sdl>>8)&0xff);				// ssdh 
	dp.udt[dp.wcn++] = (unsigned char) (average_time/0.4 + 0.5);	// Averaging time in 0,4 ms quantums
	
	for(sdl = 0;sdl < Nch; sdl++)
		dp.udt[dp.wcn++] = 0;				// Average counter for each channel (0 == 256)
	
	PktWr(RQRSDEV|FN_WEV,&dp,sizeof(dp));
	return 0;
}


