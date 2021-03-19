// Patient Fixation System (PFS) utilities file	v 1.0.1
// 28.05.2013 (v 1.0.0) Started
// 31.05.2013 (v 1.0.1) Wait for end of moving added

#include <ansi_c.h>
#include "lnklb.h"
#include "PFS.h"

#include "RemReq.h"
#include "RemRe2.h"

#define RQRSDEV	R2_RSDEV

extern int PFSadr = 0;

typedef struct {
	int enci;
	int ence;
	unsigned short ips;
	char ops;
	char al;
	char err;
	char bf;
} SERVO;

typedef struct {
	char dn;
	char cmd;
	char sts;
	char cid;
	char pid;
	char new;
	char eaf;
	char svd[3][14];
} SERVS;

typedef struct {
	char dn;
	char cmd;
	char sts;
	char rs;
	char ecnt;
	char ecod[16];
} CFSERR;


int rPFSerr(int (*pfspCB)(PKT *)) {
	SDEV dp;
	dp.seg = (short)PFSadr>>8;
	dp.tmo = 2;							// Additional time-out to wait
	dp.wcn = 2;							// Number of data bytes to send
	dp.rcn = 40;						// Answer must be less equal .. bytes
	dp.udt[0] = (unsigned char) PFSadr;	// Address
	dp.udt[1] = 0x4;					// Command: read errors
	return PktWrC(RQRSDEV|FN_ANS|FN_WEV,&dp,(int)sizeof(dp),pfspCB);
}

static int  PFSerrCB(PKT *pk) {
	int mprntf(char *fmt, ...);
	int i;
	
	SDEV *sd = (SDEV *) pk->d;
	CFSERR *ce = (CFSERR *) sd->udt;
	if(ce->ecnt) {
		mprntf("\n ERCNT=%d",ce->ecnt);
		for(i=0;i<sd->rcn-5;i++)
			mprntf(" %02x",ce->ecod[i]&0xff);
	}
	return 0;
}

int dcdPFSerr(PKT *pk) {

	SDEV *sd = (SDEV *) pk->d;
  	SERVS *ses = (SERVS *) sd->udt;
  	if(pk->sts)
  		return(pk->sts);		// Error during communication with ArmCh
  	if(ses->cmd & 0x80)
  		return(ses->cmd << 8);	// PFS return error
  		
  	if(ses->sts & 0x80) {			// if errors in CFS	(Controller Fixation System)
  		rPFSerr(PFSerrCB);
//  		return ses->sts << 16;
  	}
  	return 0;
}

int ckk4Busy(PKT *pk) {
//	SDEV *sd = (SDEV *) pk->d;
//	SERVS *ses = (SERVS *) sd->udt;
// 	SERVS *ses = ((SERVS *) ((SDEV *) pk->d)->udt);
  	return (((SERVS *) ((SDEV *) pk->d)->udt))->sts & PFS_BUSY;
}


static int PFSwaitCB(PKT *pk,void *cbdata) {
int (*pfswCB)(PKT *) = cbdata;
//	SDEV *sd = (SDEV *) pk->d;
// 	SERVS *ses = (SERVS *) sd->udt;
 	SERVS *ses = ((SERVS *) ((SDEV *) pk->d)->udt);
  	if(pk->sts || (ses->cmd & 0x80) || (ses->sts & (PFS_ERR*000000 |PFS_MODE))) {
		pfswCB(pk);	
  		return 0;
  	}
  	
  	pfswCB(pk);

  	if(ses->sts & PFS_BUSY) {
		SDEV dp;
		dp.seg = (short)PFSadr>>8;
		dp.tmo = 2;							// Additional time-out to wait
		dp.wcn = 4;							// Number of data bytes to send
		dp.rcn = 80;						// Answer must be less equal .. bytes
		dp.udt[0] = (unsigned char) PFSadr;	// Address
		dp.udt[1] = 0x11;					// Command: read
		dp.udt[2] = 0x1;					// Read Servo's Status
		dp.udt[3] = 0x07;					// Read All
		PktWrCBD(RQRSDEV|FN_ANS|FN_WEV,&dp,(int)sizeof(dp),PFSwaitCB,cbdata);
  	}
	return 0;
}

static int wPFSposCB(PKT *pk,void *cbdata) {	// CallBack for wPFSpos function
	SDEV dp;
	int (*pfswCB)(PKT *) = cbdata;
 	SERVS *ses = ((SERVS *) ((SDEV *) pk->d)->udt);
 	
  	if(pk->sts || (ses->cmd & 0x80) || (ses->sts & (PFS_ERR |PFS_MODE|PFS_BUSY))) {
  		pk->sts |= 0x80;
		pfswCB(pk);	
  		return 0;
  	}
// Call check position
	dp.seg = (short)PFSadr>>8;
	dp.tmo = 2;							// Additional time-out to wait
	dp.wcn = 4;							// Number of data bytes to send
	dp.rcn = 100;						// Answer must be less equal .. bytes
	dp.udt[0] = (unsigned char) PFSadr;	// Address
	dp.udt[1] = 0x11;					// Command: read
	dp.udt[2] = 0x1;					// Read Servo's Status
	dp.udt[3] = 0x07;					// Read All
	PktWrCBD(RQRSDEV|FN_ANS|FN_WEV,&dp,(int)sizeof(dp),PFSwaitCB,cbdata);
	return 0;
}

int wPFSpos(int pid,float par,int (*pfswCB)(PKT *)) {	// Post request to set PFS position 
	SDEV dp;
	int pos=0;
	if(pid < 0 || pid > 3)
		return -1;
	switch(pid) {
		case 0:
			pos = par * 173360. /180.;	// Angle
			break;
		case 1:
			pos = par * 381537. / 66.1;	// Height (cm)
			break;
		case 2:
			pos = par ?  89950 :  3000;	// 3000 - down, 89950 - up (detector up/down)
			break;
	}
	dp.seg = (short)PFSadr>>8;
	dp.tmo = 2;							// Additional time-out to wait
	dp.wcn = 9;							// Number of data bytes to send
	dp.rcn = 40;						// Answer must be less equal .. bytes
	dp.udt[0] = (unsigned char) PFSadr;	// Address
	dp.udt[1] = 0x10;					// Command: read
	dp.udt[2] = 0x2;					// CID 
	dp.udt[3] = (char)pid;					// subsystem id
	dp.udt[4] = 0;						// 0 - absolute , 1 - relative
	* (int *) &dp.udt[5] = pos;
	return PktWrCBD(RQRSDEV|FN_ANS|FN_WEV,&dp,(int)sizeof(dp),wPFSposCB,pfswCB);
}

int rPFSpos(int (*pfspCB)(PKT *)) {		// Post request to Get PFS status & position
	SDEV dp;
	dp.seg = (short)PFSadr>>8;
	dp.tmo = 2;							// Additional time-out to wait
	dp.wcn = 4;							// Number of data bytes to send
	dp.rcn = 100;						// Answer must be less equal .. bytes
	dp.udt[0] = (unsigned char) PFSadr;	// Address
	dp.udt[1] = 0x11;					// Command: read
	dp.udt[2] = 0x1;					// Read Servo's Status
	dp.udt[3] = 0x7;					// Read Angle,Height,Detector
	return PktWrC(RQRSDEV|FN_ANS|FN_WEV,&dp,(int)sizeof(dp),pfspCB);
}

int dcdPFSpos(PKT *pk,float *angl,float *hght,float *dtct) {
	int sts;
 	SERVS *ses = ((SERVS *) ((SDEV *) pk->d)->udt);
  	
  	if((sts = dcdPFSerr(pk)) != 0)
  		return sts;
  		
	*angl = ((SERVO *) ses->svd[0])->enci /173360. * 180.;	// degree
	*hght = ((SERVO *) ses->svd[1])->enci /381537. * 66.1;	// cm
	*dtct = (((SERVO *) ses->svd[2])->enci - 3000)/89950.;	// 0/1
	
	return 0;
}
