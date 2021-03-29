// SDev.h	v 2.0.2
// 21.05.2015	GetRedirrectToSlaveSts(PKT *pk,int *adr,int *ch) added

void SetMBPScur(int adr, int ch, int dat, int (*mbpsCB)(PKT *));
int ReadMBPS(int adr,int ch ,int (*mbpsCB)(PKT *));					// Return Handle
int GetMBPScur(PKT *pk,int *adr,int *ch,unsigned short *dat);		// Get array with data & Return status
int GetMBPSsts(PKT *pk,int *adr,int *ch);

int GetRedirrectToSlaveSts(PKT *pk,int *adr,int *ch);	// status, address, channel for redirect to slave answer

void SetMUVScur(int adr, int ch, int dat, int (*mbpsCB)(PKT *));
int ReadMUVS(int adr,int ch ,int (*mbpsCB)(PKT *));					// Return Handle
int GetMUVScur(PKT *pk,int *adr,int *ch,unsigned short *dat);		// Return status
int GetMUVSsts(PKT *pk,int *adr,int *ch);

void SetSerDac(int adr,int dat, int (*mbpsCB)(PKT *));
int ReadD16dac(int fnc,int adr,int (*d16CB)(PKT *));
int GetD16dac(PKT *pk,int *adr,unsigned short *dat);
int GetD16sts(PKT *pk, int *adr);

//void SetD4A4sync(int adr, int delay, int OnOff);		// delay in terms of 64 mcsec 
void SetD4A4do(int adr,int ss);							// Start & Stop ss=1 Start ss=0 Stop
int ReadD4A4adc(int adr,int (*d4a4CB)(PKT *));			// Initiate Read request(s)
int SetADCPrms(int adr,float average_time,float synch_tmo,float synch_delay, int Nch);

// Extract array of up to Nch doubles from answer. 4 External ADC & 4 Differential internal ADC in Volts
// *cnt - average counter. if *cnt == 0 - ADC average Time must be reinitialzed !
// *ust LSByte contain 8 Digital In bits
int GetD4A4adc(PKT *pk,int *adr,int *adcsts,int *ust,double *dat, int Nch);
//int GetD4A4Vacadc(PKT *pk,int *adr,int *ust,int *cnt,double *dat, int Nch);

int ReadHALLsns(int fnc,int adr,int (*hallCB)(PKT *));		// Send command & install call back for hall sensor
int GetHALLsns(PKT *pk,int *adr,int *cnt,double *dat);		// Extract hall sensor data from answer

//void SetVacTime(int adr,int tm);			// Set time for Vacuum reading [ms]
//void SetDeflTime(int adr,int tm);							// Set time for deflector reading [ms]
void SetDynPSOnOff(int adr, int chan, int sts);				// On/Off dynamic power supply (10.A)
int ReadDynPSstat(int adr, int chan, int (*deflCB)(PKT *));	// Read dynamic power supply status
int ReadDynPScur(int adr, int chan, int (*deflCB)(PKT *));	// Read dynamic power supply current
void SetDeflOnOff(int adr,int OnOff);
int ReadDeflVolt(int adr, int (*deflCB)(PKT *));			// Read deflector voltage
int GetDeflData(PKT *pk,int *adr, short *cur, short *vlt);
int ReadMasterStat(int adr, int (*deflCB)(PKT *));	// Read shutter status

int ReadSDevID(int adr, int (*getIdCB)(PKT *));
int GetSDevID(PKT *pk, int *adr, int *id);
int ReadSDevVersion(int adr, int (*getvrsCB)(PKT *));
int GetSDevVersion(PKT *pk, int *adr, char **vrs);
int rSDversion(int adr,char *vrs,int sz);
int rSDid(int adr,int *id);

enum bps10a_status_bits {	// Status byte for DynPS 
	DYNPS_LOCAL = 0x01,		// 0 - Remote, 1- Local
	DYNPS_OVRHT = 0x08,		// Overheat
	DYNPS_ONOFF = 0x10,		// 1-On, 0-Off
	DYNPS_STRTS = 0x20,		// 1 - Start signal present
	DYNPS_SYNCR = 0x40,		// 1 - "synchro" mode On
	DYNPS_ERROR = 0x80		// 1 - Internal error
};

int DynPS_On_Off(int adr,int ch,int onoff,int (*bpsCB)(PKT *));	// Switch On/Off DynPS
int DynPS_GetSts(int adr,int ch,int (*bpsCB)(PKT *));
int DynPS_Sts(PKT *pk);	// Parse answer. Return status byte if >= 0 otherwise error
int DynPS_GetADC(int adr,int ch,int ch_adc,int (*bpsCB)(PKT *));	// Send command to get DynPS ADC data
int DynPS_ADCval(PKT *pk, double *Val);		// Parse Answer to get ADC value (Only one channel !!!) 
int SetADCPrmsSlave(int adr,int slave_ch,float average_time,float synch_tmo,float synch_delay, int Nch);
int GetSlaveCh(PKT *pk);
int GetMasterAdr(PKT *pk);
int SetVBPMPScur(int adr, char data, int (*mbpsCB)(PKT *));
int SetVBPMPSmode(int adr, char data, int (*mbpsCB)(PKT *));
