#ifndef LINK_H
#define LINK_H

#define PK_MARK	0x55AA

typedef struct pkh {
	short mrk;				/* Packet Mark */
	short csm;				/* CheckSum for PKH */
	unsigned int seq;		/* Sequence number */
	unsigned char fnq;		/* Function */
	unsigned char sts;		/* Status */
	short rqc;				/* Request code */
	short frg;				/* Fragment number */
	short isz;				/* Item size */
	int per;				/* Real data size */
	int ckl;
} PKH;

typedef struct pkt {
	short mrk;				/* Packet Mark */
	short csm;				/* CheckSum for PKH */
	unsigned int seq;		/* Sequence number */
	unsigned char fnq;		/* Function */
	unsigned char sts;		/* Status */
	short rqc;				/* Request code */
	short frg;				/* Fragment number (unused) */
	short isz;				/* Item size */
	int per;				/* Real data size */
	int ckl;
	char d[128];			/* Data */
} PKT;

#define STS_OK		0x00	/* Status OK */
#define STS_ERROR	0x01
#define STS_NOMEM	0x02	/* Status no memory*/

#define	F_IMD		0x01	/* Quick request*/
#define F_CYC		0x02	/* Cycle function */
#define F_WEV		0x04	/* Wait for some events */
#define F_CEV		0x08	/* Cycle, send data if it changed */
#define F_CAN		0x10	/* Cancel Queued request */
#define F_QCK		0x20	/* Quick manner of CallBack */
#define F_NFY		0x40	/* Notify Flag */
#define F_ANS		0x80	/* Need return data */

#define F_FNC	(F_IMD|F_CYC|F_WEV|F_CEV|F_CAN|F_NFY)	/* MASK for real functions byte */

#endif
