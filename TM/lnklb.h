#ifndef LNKLB_H
#define LNKLB_H

#include "link.h"

#define RS_BITS	2	// Number of bits for Remote server numbering (nServ = (1<<RS_BITS))
#define LIMCRQ	0	// LIMIT number of CRQ in FreeQueue	(0 - disable limit) //20

#define FN_IMD	(F_IMD<<16)
#define FN_CYC	(F_CYC<<16)
#define FN_WEV	(F_WEV<<16)
#define FN_CEV	(F_CEV<<16)
#define FN_QCK	(F_QCK<<16)
#define FN_NFY	(F_NFY<<16)
#define FN_ANS	(F_ANS<<16)

#define RS_SERV	(1<<RS_BITS)	// Maximum Number of Remote Servers
#define RS_SHFT	(8-RS_BITS)
#define RS_MASK	(RS_SERV-1)

enum lnklb_errors {
LE_WTMO = -104,		// TimeOut during IOSrvRd()
LE_NCON,			// No connection -103
LE_NMEM,			// No memory for packet data
LE_NCRQ,			// No Free Queue element (CRQ)
LE_NCBF		  		// No CallBack Function specified for Notify request
};

int IOSrvRd(int rqc_fnq,void *pdo,int szo,PKH *ph,void *pdi,int szi); // Read From IO Srv
//int PktWrHC(PKH *ph,void *pd,int sz,void *cbfnq,void *cbdata);
int PktWrCBD(int rqc_fnq,void *pd,int sz,int (* cbdfnq)(PKT *,void *),void *cbdata);
int PktWrC(int rqc_fnq,void *pd,int sz,int (* cbfnq)(PKT *));
int PktWr(int rqc_fnq,void *pd,int sz);
int CancelRQ(int *rqhndl);					// Cancel Queue request
int ConToSrv(int panel,short rqc,char *srvNam, int port,void (* discon)(int),void (* recerr)(int,int));
void DisFrSrv(short rqc);					// Disconnect from server by requst code
void DisFrSrvS(void);						// Disconnect from all servers
void DisFrSid(int sid);

void PktFree(PKT **cbData);		// Free PKT data that CallBack(PktWrCx) require
								// not to free automatically (return nonzero)

// This function must be defined by User to display messages
// from lnklib.c somewhere in his program
void OuMsg(char *msg);		


unsigned int getHConver(int sid);

#endif
