// Patient Fixation System (PFS) header file	v 1.0.2
// 28.05.2013 (v 1.0.0)	Started.
// 29.05.2013 (v 1.0.1) dcdPFSerr prototype added
// 30.05.2013 (v 1.0.2)	Adr parameters moved to PFSadr variable


extern int	PFSadr;	// PFS address on serial bus

enum PFS_pids {
PID_ANGLE  = 0,		// Rotation
PID_HEIGHT = 1,		// Moving patient up/down
PID_DETECTOR = 2,	// Moving Detector up/down
PID_SHIFT = 3		// Shifting PFS
};


enum PFS_status_bits {
PFS_ERR	= 0x80,
PFS_BUSY = 0x40,
PFS_MODE = 0x20,
PFS_MOVE = 0x10,
PFS_SSTS = 0x08,
PFS_FSTS = 0x04,
PFS_SOK = 0x02,
PFS_FOK = 0x01
};


int ckk4Busy(PKT *pk);
int dcdPFSerr(PKT *pk);										// check & decode erros
int wPFSpos(int pid,float par,int (*armwCB)(PKT *));
int rPFSpos(int (*armcCB)(PKT *));							// Post request to Get PFS position & status
int dcdPFSpos(PKT *pk,float *angl,float *hght,float *dtct);	// Decode position data
