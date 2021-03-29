#include <userint.h>
#include <utility.h>
#include <ansi_c.h>
#include <tcpsupp.h>
#include "lnklb.h"

#define UMACRO	1

typedef void (* DISCONF)(int sid);
typedef void (* RESERRF)(int sid,int errcod);

static DISCONF DisCon[RS_SERV] = {0};
static RESERRF RecErr[RS_SERV] = {0};
static unsigned hConver[RS_SERV] = {0};
//static int sSocket[RS_SERV] ={0};

#define MINUS1	(~0)

#define OSERCMX	0x10000000
static int oSerc = 1;
static int oSern = OSERCMX;
static int TimerID=-1;
static int TimerPn=0;
static int TimerEn=0;

#if LIMCRQ
int cidCnt = 0;
#endif

typedef struct crq {
#if LIMCRQ
	int cid;
#endif
	struct crq *nxt;
	struct crq *prv;
	unsigned int seq;
	short fnq;
	short rqc;
	char *pkd;				// Pointer to Packet data region
	int ppn;				// Put index in fragmented data
	int act;				// Active flag , reseted on cancel
	int (* cbf)(PKT *);		// CallBack Function
	void *cbd;				// CallBackData
} CRQ;

typedef struct {			// structure to use with Deferred Call
	int (* cbf)(PKT *);
	void *cbd;
	int act;
	PKT pk;		
} DCD;

#define R_ACTIV	1		// Req Canceled
#define R_QUICK	2		// Req must be executed by quick method (no defer)
#define R_CDATA	4		// CallBack must be called with CallBackData
#define R_ALLOC	8		// There was no memory while allocating it for CallBack

static CRQ *Hd = NULL;		// Queue for sended packet that require answer 
static CRQ *Fr = NULL;		// Queue of free elements
//static CRQ *Ex = NULL;		// Queue of need to make CallBack answers

static CRQ *WrHd = NULL;	// TCP output Queue Head
static CRQ *WrTl = NULL;	// TCP output Queue Tail

//static PKT *PkFr = NULL;	// Free Packet Queue

#if UMACRO
#define sIDr(rqc)	((rqc>>RS_SHFT) & RS_MASK)
#define sIDq(crq)	((crq->rqc>>RS_SHFT) & RS_MASK)
#else
static int sIDr(short rqc) {	// Get Server ID by request code
	return (rqc>>RS_SHFT) & RS_MASK;
}

static int sIDq(CRQ *rq) {		// Get Server ID by Queued element
	return (rq->rqc>>RS_SHFT) & RS_MASK;
}
#endif

static int sIDh(unsigned hConvers) {	// Get Server ID by Conversation handle
int sid;
	for(sid=0;sid<RS_SERV;sid++)
		if(hConver[sid] == hConvers)
			return sid;
	return -1;
}			

static void EnQueueTl(CRQ *tmp) {
	tmp->nxt = NULL;
	if(WrHd == NULL)
		WrHd = tmp;
	else
		WrTl->nxt = tmp;
	WrTl = tmp;
}

#if 0
static void EnQueueHd(CRQ *tmp) {
	tmp->nxt = NULL;
	if(WrHd == NULL)
		WrTl = tmp;
	else
		tmp->nxt = WrHd;
	WrHd = tmp;
}
#endif

static void FrCrqEl(CRQ *tm) {	
	if(tm->pkd != NULL) {
		free(tm->pkd);
		tm->pkd = NULL;
	}
#if LIMCRQ
	if(tm->cid >= LIMCRQ) {
		free(tm);
		cidCnt--;	
		return;
	}
#endif
	tm->act = 0;			// Inactive !
	tm->nxt = Fr;
	Fr = tm;
}

static void DeQueueRQ(CRQ *tm) {	// DeQueue tm from hd queue
	if(tm->prv == NULL) {
		if( (Hd = tm->nxt) != NULL)
			Hd->prv = NULL;
	} else {
		if((tm->prv->nxt = tm->nxt) != NULL)
			tm->nxt->prv = tm->prv;
	}
	FrCrqEl(tm);
}

static CRQ * gtCrqEl(void) {
CRQ *tm;
	if((tm = Fr) == NULL) {
		tm = malloc(sizeof(CRQ));	 //memory leaks  ????
#if LIMCRQ
		if(tm != NULL) {
			memset(&tm->nxt,0,sizeof(CRQ)-sizeof(tm->cid));
			tm->cid = cidCnt++;			
		}
		return tm;
#endif
	} else
		Fr = tm->nxt;
	if(tm != NULL)
#if LIMCRQ	
		memset(&tm->nxt,0,sizeof(CRQ)-sizeof(tm->cid));
#else
		memset(tm,0,sizeof(CRQ));
#endif		
	return tm;
}

static void DoOuP(void) {
	CRQ *tm;
	int sts;
	while(WrHd != NULL) {
		if((sts=ClientTCPWrite(hConver[sIDq(WrHd)],WrHd->pkd + WrHd->ppn,sizeof(PKT) - WrHd->ppn,1)) <= 0)
   			break;
    	if(sts != sizeof(PKT) - WrHd->ppn) {
    		WrHd->ppn += sts;
    		break;
    	}
    	tm = WrHd;
    	WrHd = WrHd ->nxt;
    	FrCrqEl(tm);
	}
	if(WrHd != NULL) {
		if(!TimerEn)
			SetCtrlAttribute(TimerPn,TimerID,ATTR_ENABLED,1);
		TimerEn |= 1;
//		OuMsg("&");
		
	} else {
		if(TimerEn == 1) {
			SetCtrlAttribute(TimerPn,TimerID,ATTR_ENABLED,0);	
//			OuMsg("*");
		}
		TimerEn &= ~1;
	}
}

static int OuPKT(PKH *ph,void *pd,int sz) {
CRQ *tm;
	if(hConver[sIDr(ph->rqc)] == MINUS1)
		return LE_NCON;				// No Connection ! (-103)
	if((tm = gtCrqEl()) == NULL)
		return LE_NCRQ;				// No free CRQ element
	if( (tm->pkd = malloc(sizeof(PKT))) == NULL) {
		FrCrqEl(tm);
		return LE_NMEM;				// No memory for packet (-102)
	}
	ph->mrk = PK_MARK;
	memcpy(tm->pkd,ph,sizeof(PKH));
	if( pd != NULL) {
		if(sz > sizeof(PKT) - sizeof(PKH))
			sz = (int) (sizeof(PKT) - sizeof(PKH));
		memcpy(((PKT * ) tm->pkd)->d ,pd,sz);
	}
	tm->rqc = ph->rqc;		//????
	EnQueueTl(tm);
	DoOuP();
	return 0;	// Success
}

static int PktWrHC(PKH *ph,void *pd,int sz,void *cbfnq,void *cbdata) {
CRQ *tm;
int sts;
    if(cbfnq != NULL ) {				// 080214 added cbfnq != NULL
    	if(!(ph->fnq & (F_ANS|F_NFY)))
    		ph->fnq |= F_ANS;			// F_ANS by default if not specified
		if((tm = gtCrqEl()) == NULL)
			return LE_NCRQ;				// No Free Queue element (-101)
		if(ph->fnq & F_QCK) {
			tm->act |= R_QUICK;			// Mark request not to defer
			ph->fnq &= ~F_QCK;
		}
		ph->seq = oSerc;
		tm->seq = oSerc;
		tm->fnq = ph->fnq;
		tm->rqc = ph->rqc;
		tm->cbf = cbfnq;
		tm->cbd = cbdata;
		if(ph->mrk == 12345)
			tm->act |= R_CDATA;			// CallBack with Data
		if(Hd != NULL)
			Hd->prv = tm;
		tm->nxt = Hd;
		Hd = tm;
		tm->act |= R_ACTIV;				// Activate 
		if( (sts=OuPKT(ph,pd,sz)) != 0 ) {
   			DeQueueRQ(tm);
   			return sts;
   		}
	    if(++oSerc >= OSERCMX)
   			oSerc = 1;
   		for(tm = Hd; tm != NULL;)
   			if(tm->seq == oSerc) {
   				if(++oSerc >= OSERCMX)
   					oSerc = 1;
   				tm = Hd;
   			} else
   				tm = tm->nxt;
   		return ph->seq;
	}
	
// Request without CallBack	

	if(ph->fnq & F_NFY)
		return LE_NCBF;	// Error - No CallBack Function  for Notify request
		
	ph->fnq &= ~F_ANS;	// Answer not needed if no CallBack function.
	ph->seq = oSern;
	if( (sts=OuPKT(ph,pd,sz)) != 0 )
    	return sts;
    if(++oSern < 0)
    	oSern = OSERCMX;
    return ph->seq;
}

int PktWrCBD(int rqc_fnq,void *pd,int sz,int (* cbdfnq)(PKT *,void *),void *cbdata) {
PKH ph;
	memset(&ph,0,sizeof(PKH));
	if(!(ph.fnq = (unsigned char) (rqc_fnq>>16)))
		ph.fnq = F_IMD;
	ph.mrk = 12345;
	ph.rqc = (short) rqc_fnq;
 	return PktWrHC(&ph,pd,sz,cbdfnq,cbdata);
}

int PktWrC(int rqc_fnq,void *pd,int sz,int (* cbfnq)(PKT *)) {
PKH ph;
	memset(&ph,0,sizeof(PKH));
	if(!(ph.fnq = (unsigned char) (rqc_fnq>>16)))
		ph.fnq = F_IMD;
	ph.rqc = (short) rqc_fnq;
 	return PktWrHC(&ph,pd,sz,cbfnq,NULL);
}

int PktWr(int rqc_fnq,void *pd,int sz) {
 	return PktWrC(rqc_fnq,pd,sz,NULL);
}

typedef struct _iosrd {
	struct _iosrd *nxt;
	int srn;
	int sts;
	PKH *pkh;
	void *pdi;
	int szi;
} IOR;

static IOR *QioH = NULL;

static int IoRdCb(PKT *pk) {
	IOR *iot = QioH;
	while(iot != NULL) {
		if(pk->seq == iot->srn) {
			if(pk->per > iot->szi)
				pk->per = iot->szi;
			memcpy(iot->pkh,pk,sizeof(PKH));
			memcpy(iot->pdi,&pk->d,pk->per);
			iot->sts = 0;
			break;
		}
		iot = iot->nxt;
	}
	return 0;
}

// Send Request to IO Server, wait for answer, return accepted data
int IOSrvRd(int rqc_fnq,void *pdo,int szo,PKH *ph,void *pdi,int szi) {
#define IOWAIT	20
#if IOWAIT	
	double stt;
#endif	
	IOR ior;
	int sts = 0;
	ior.sts = 1;
	if((ior.srn = PktWrC(rqc_fnq|FN_QCK,pdo,szo,IoRdCb)) < 0)
		return ior.srn;		// return error code
		
	ior.pkh = ph;
	ior.pdi = pdi;
	ior.szi = szi;
	ior.nxt = QioH;
	QioH = &ior;
#if IOWAIT
	for(stt = Timer() + IOWAIT; ior.sts;) {
		ProcessSystemEvents();
		if(Timer() > stt) {
			sts = LE_WTMO;
			break;
		}
	}
#else
	while(ior.sts)		// Timeout ???
		ProcessSystemEvents();
#endif
	if(QioH == &ior)
		QioH = ior.nxt;
	else {
		IOR *iob;
		for(iob = QioH; iob->nxt != NULL; iob = iob->nxt)
			if(iob->nxt == &ior) {
				iob->nxt = ior.nxt;
				break;
			}
	}
	return sts;		
}

int CancelRQ(int *rqhndl) {	// Cancel Queue request
CRQ *tm;
	if(*rqhndl > 0)
		for(tm=Hd; tm != NULL; tm = tm->nxt)
			if(tm->seq == *rqhndl) {
				PKH rq;
				int sts;
				memset(&rq,0,sizeof(rq));
				rq.seq = tm->seq;
				rq.fnq = (unsigned char) (tm->fnq | F_CAN);
				rq.rqc = tm->rqc;
	    		if( (sts=OuPKT(&rq,NULL,0)) != 0 )
	   				return sts;
    			DeQueueRQ(tm);
				*rqhndl = -1;
				return 0;	// Successful operation		
			}
	return 1;
}

static void DelSrvRQs(int sid) {	// Cancel all request for server with sid
CRQ *tm = Hd;
	while(tm != NULL) {
		CRQ *td = tm;
		tm = tm->nxt;
		if(sIDq(td) == sid)
   			DeQueueRQ(td);
	}
}

static DCD *ReadPacket(unsigned handle,int sid) {
static char *gBf[RS_SERV]={0};
static int rSz[RS_SERV] = {0};
static int tSz[RS_SERV];
static DCD dcB[RS_SERV];
char *odc;
int dSz;
	if((odc = gBf[sid]) == NULL) {
		dSz = ClientTCPRead(handle,((char *) &dcB[sid].pk) + rSz[sid] ,sizeof(PKT) - rSz[sid],1);
		if(dSz < 0){
			if(dSz != -kTCP_TimeOutErr) {
				if(RecErr[sid] != NULL)
					RecErr[sid](sid,dSz);
				rSz[sid] = 0;
			}
			return NULL;
		}
		if((rSz[sid] += dSz) != sizeof(PKT))
			return NULL;
		rSz[sid] = 0;
		if(dcB[sid].pk.per <= sizeof(dcB[sid].pk.d))
			return &dcB[sid];
// First part of Big Packet has arrived
		dSz = (int) (((dcB[sid].pk.per + sizeof(PKH) - 1)/sizeof(PKT)) * sizeof(PKT));
		if((gBf[sid] = odc = malloc(dSz + sizeof(DCD))) == NULL)
			gBf[sid] = odc = (char *) &dcB[sid];
		else
			memcpy(odc,&dcB[sid],sizeof(DCD));	// Copy First Packet + CallBack pointer
		rSz[sid] = (int) sizeof(DCD);
		tSz[sid] = (int) (dSz + sizeof(DCD));
	}
// Continue reading Big Packet
	if(odc == (char *) &dcB[sid]) {	// Check if we need to skip data
		int dSw;
		do {
			dSw = (int) (tSz[sid] - rSz[sid] > sizeof(DCD)  ? sizeof(DCD) : (tSz[sid] - rSz[sid]));
			if((dSz = ClientTCPRead(handle,odc ,dSw, 100)) < 0) {
				if(dSz != -kTCP_TimeOutErr) {
					if(RecErr[sid] != NULL)
						RecErr[sid](sid,dSz);
					gBf[sid] = NULL;
					rSz[sid] = 0;
				}
				return NULL;
			}
			if((rSz[sid] += dSz) >= tSz[sid]) {
				gBf[sid] = NULL;
				rSz[sid] = 0;
				return NULL;
			}
		} while(dSw == dSz);
		return NULL;
	}
	if((dSz = ClientTCPRead(handle,odc + rSz[sid] ,tSz[sid] - rSz[sid],100)) < 0) {
		if(dSz != -kTCP_TimeOutErr) {
			if(RecErr[sid] != NULL)
				RecErr[sid](sid,dSz);
			free(odc);
			gBf[sid] = NULL;
			rSz[sid] = 0;
		}
		return NULL;
	}
	if((rSz[sid] += dSz) < tSz[sid])
		return NULL;
	gBf[sid] = NULL;
	rSz[sid] = 0;
	return (DCD *) odc;
}

static void CVICALLBACK DeferdPktCB(void *cbData) {
//	OuMsg("d");
//	static int qqq=0;
	
	if( ((DCD *) cbData)->act & R_CDATA ) {
typedef int (* CBDFUN) (PKT *,void *);
		if(! ((CBDFUN) ((DCD *) cbData)->cbf) ( &((DCD *) cbData)->pk, ((DCD *) cbData)->cbd) ) {
			if(((DCD *) cbData)->act & R_ALLOC)
				free(cbData);
#if 1
		}
#else		
			else
				qqq++;
		} 
		else
			qqq++;
#endif
	} else {
		if(! ((DCD *) cbData)->cbf ( &((DCD *) cbData)->pk) ) {
			if(((DCD *) cbData)->act & R_ALLOC)
				free(cbData);
#if 1
		}
#else
			else
				qqq++;
		} else
			qqq++;
#endif			
	}
}

void PktFree(PKT **cbPkt) {
	if(*cbPkt != NULL) {
		DCD *dc = (DCD *) (((char *) *cbPkt) - sizeof(DCD) + sizeof(PKT));
		if(dc->act & R_ALLOC)
			free(dc);
		*cbPkt = NULL;
	}
}

static int ParsePacket(DCD *dc) {
	CRQ *tm;
	int seq = dc->pk.seq;
	for(tm = Hd;tm != NULL;tm = tm->nxt)
		if(tm->seq == seq) {					// Request found
			if(dc->pk.per > sizeof(dc->pk.d)) {	
// Big Packet
				dc->cbf = tm->cbf;
				dc->cbd = tm->cbd;
				dc->act = tm->act|R_ALLOC;
				if(PostDeferredCall(DeferdPktCB,dc)) {	// Defer CallBack with large data packet
					OuMsg("@");
					DeferdPktCB(dc);		// Execute CallBack without defer
					if(!(tm->act&R_ACTIV))
						break;				// Request was canceled during CallBack
				}
			} else {			
// Unfragmented packet arrived
				DCD *tmd;
				if((tmd = malloc(sizeof(DCD))) == NULL) {
					DeferdPktCB(dc);	// Call with data in static memory
					if(!(tm->act&R_ACTIV))
						break;			// Request was canceled during CallBack
				} else {
					tmd->cbf = tm->cbf;
					tmd->cbd = tm->cbd;
					tmd->act = tm->act|R_ALLOC;
					memcpy(&tmd->pk,&dc->pk,sizeof(PKT));
					if( !(tm->act&R_CDATA) || PostDeferredCall(DeferdPktCB,tmd) ) {
						static int ccc;
						ccc = tmd->act;
						DeferdPktCB(tmd);	// Execute CallBack without defer
						if(!(tm->act&R_ACTIV))
							break;			// Request was canceled during CallBack
					}
				}
            }
			if(!(tm->fnq & (F_CYC|F_CEV|F_NFY)))
				DeQueueRQ(tm);
        	break;
        }
/*	if(tm == NULL)
		OuMsg(" PKT Skipped ");/**/
//	OuMsg("p");
	return 0;
}

static int CVICALLBACK ClientTCPCB (unsigned handle, int event, int error, void *cbD) {
int sid;
DCD *pk;
//	OuMsg("a");
    switch(event) {
        case TCP_DATAREADY:
#if 1
        	do {
        		if((pk = ReadPacket(handle,((int) cbD))) == NULL)
        			break;
			} while(ParsePacket(pk));
#else
       		while((pk = ReadPacket(handle,(int) cbD)) != NULL)
       			ParsePacket(pk);
#endif
            break;
            
        case TCP_DISCONNECT:
        	if((sid = sIDh(handle)) >= 0) {
		    	hConver[sid] = MINUS1;
		    	RecErr[sid] = NULL;
		    	DelSrvRQs(sid);			// 080202
    	        if(DisCon[sid] != NULL)
        	    	DisCon[sid](sid);	// CallBack on disconnect wiht sid as argument
        	    DisCon[sid] = NULL;
			}
            break;
    }
//	OuMsg("b");
	return 0;
}

static int CVICALLBACK TcpTimerCB (int panel, int control, int event, void *cbData, int eD1,  int eD2) {
	if(event == EVENT_TIMER_TICK)
		if(TimerEn & 1)
			DoOuP();
		else {
		}
	return 0;
}

int ConToSrv(int panel,short sRq,char *srvNam, int port,void (* discon)(int),void (* recerr)(int,int)) {
	int errcod;
	int sid = sIDr(sRq);
	if(TimerID < 0) {
 		if((TimerID = NewCtrl(TimerPn = panel,CTRL_TIMER,"", 0,0)) < 0)
			return -kTCP_DisconnectPending - 2;
			
	 	if((errcod = InstallCtrlCallback (TimerPn,TimerID, TcpTimerCB, NULL)) < 0) {
			DiscardCtrl(TimerPn,TimerID);
			TimerID = -1;
			return -kTCP_DisconnectPending - 3;
 		}
 			
		SetCtrlAttribute(TimerPn,TimerID,ATTR_INTERVAL,0.1);
		SetCtrlAttribute(TimerPn,TimerID,ATTR_ENABLED,TimerEn=0);
			
		for(errcod=0;errcod<RS_SERV;errcod++) {
			hConver[errcod] = MINUS1;
			DisCon[errcod] = NULL;
			RecErr[errcod] = NULL;
		}
		oSerc = 1;
		oSern = OSERCMX;
#if LIMCRQ
		{
// Reserve LIMCRQ CRQ elements in Free queue
			CRQ	*tm,*head = NULL;
			for(errcod = 0; errcod < LIMCRQ; errcod++) {
				if( (tm = gtCrqEl()) == NULL )
					break;
				tm->nxt = head;
				head = tm;
			}
			
			while((tm = head) != NULL) {
				head = tm->nxt;
				FrCrqEl(tm);
			}
			
		}
#endif			
	}
	
	if(hConver[sid] != MINUS1)
		return -kTCP_DisconnectPending - 1;				// Already connected !
    DisableBreakOnLibraryErrors();
    
    SetWaitCursor(1);
	errcod=ConnectToTCPServer(&hConver[sid],port,srvNam,ClientTCPCB,(void *) sid,3000);
 	SetWaitCursor(0);
 	if(errcod < 0) {
 		hConver[sid] = MINUS1;
        return errcod;
	}
	
	DisCon[sid] = discon;			// Disconnect CallBack 
	RecErr[sid] = recerr;			// Receive Error CallBack 

//	errcod = GetHostTCPSocketHandle(hConver[sid], &sSocket[sid]);

	return 0;
}

void DisFrSid(int sid) {	/* Disconnect from the TCP server by sid (Server ID) */
	DelSrvRQs(sid);			// 080202
	if(hConver[sid] != MINUS1) {
		DisconnectFromTCPServer (hConver[sid]);
/*		{
			int i;
			for(i=0;i<10;i++)
				ProcessSystemEvents();
		}/**/
		hConver[sid] = MINUS1;
	}
	DisCon[sid] = NULL;
	RecErr[sid] = NULL;		
	for(sid=0;sid<RS_SERV;sid++)
		if(hConver[sid] != MINUS1)
			return;
	if(TimerID >= 0) {
		DiscardCtrl(TimerPn,TimerID);
		TimerID = -1;
	}
}

void DisFrSrv(short sRq) {	/* Disconnect from the TCP server by request */
	DisFrSid(sIDr(sRq));
}

void DisFrSrvS(void) {		/* Disconnect from all TCP servers */
	int sid;
	for(sid=0;sid<RS_SERV;sid++)
		DisFrSid(sid);
}
//===========================================
unsigned int getHConver(int sid)
{
	return hConver[sid];
}
