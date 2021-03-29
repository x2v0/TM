#ifndef REMREQ_H
#define REMREQ_H

#ifndef WTIMER_STRUCT
#define WTIMER_STRUCT
// Sync Types dependence
#define RIS_RIS	0x00
#define RIS_FAL	0x01
#define FAL_RIS	0x02
#define FAL_FAL 0x03

typedef struct {
	int ne;				// Number of channel to programm
	float delay[10];	// in mks
	float width[10];	// in mks
	char chan[10];		// Channel to set 0...15. if chan == -1 - Set CYCLE parameters (Cycle Delay & Cycle Width)
	char sync[10];		// sync with channel 0..15, or -1 - No sync; if chn == -1  (sync == 0) Timer Off (sync == 1) Timer On
	char type[10];		// sync type 
} WTIM;
#endif

typedef struct {	// SrvOut Synthesizer parameters
	unsigned rgm;			// Regim of SrvOut (The same as for AccSrv)
	double fr1;				// Frequency1 in MHz
	double ph;				// Phase in grad
	double amp;				// RF Amplitude in volts
	double fr2;				// Frequency2 in MHz
	double dfw;				// Delta Frequency Word (Frequency Rate - in MHz per Update tick)
	double efm;				// Frequency multiplier Fex = efm*Frev + efa
	double efa;				// Frequency addition
	int rrt;				// Synthesizer Rate parameter
	unsigned short exst;	// Excitation Start tick
	unsigned short pwpr;	// Piecewice Perioid in ticks
} FPA2;

#define AouRange	80.		// Max output amp in ?? - corresponds to 0xfff DDS Amplitude Code
#define RFoutAmplCoef	(4095./AouRange)

typedef struct {	// Extraction Intensity Feedback parameters
	float fbcf;		// FeedBack coefficient (P from PID)
	float grcf;		// coefficient for post exitation RF amplitude calculation
	short ntck;		// Number of ticks in averaging interval
	short izer;		// Zero value in desired Intensity	
	short rsdl;		// Duration of preexcitation. rsdl > 0 - Maximum amplitude rsdl ticks 
					// then PostExAmp = grcf*ides
					// rsdl < 0 - Maximum Amplitude till disired value reached,then PostExAmp
	short fldl;
	short chnl;		// bit0..bit3 - Adc channel to use for feedback
					// bit4(0x10) - invers ADC data flag
					// bit5(0x20) - Use BPM delta Beam Intensity intstead of ADC channel 08-apr-2009
					// bit6..bit7 - Hardware FeedBack channel selector
					// bit8..bit11 - Additional ADC channel if bit12(0x1000) is set.
	short fdsr;		// Full Dose Rate (corresponds to 20ma current in Beam current control signal)
	float orsh;		// Orbit shift at Disable/Enable Beam extraction in mm
	float eamx;		// Maximum possible Excitation Amplitide in Volts
	float incf;		// Integral coefficient (I from PID)
	float dfcf;		// Differential coefficient (D from PID)
	float eam0;		// Amplitude of first axitation in Volts
	short appn;		// Approximation point for fft filter
	short harm;		// Harmonics to left
} EXFB;				// Intensity table has IDAC format

typedef struct {	// Target amplitude correction
	int ne;						// Table size
	unsigned short bint[20];	// BPM Intensity (internal codes)
	float tam[20];				// Target amplitude multiplier
} TACR;

typedef struct {
	unsigned short dt;
	short du;
} DTDS;

typedef struct {			// Interpolation DAC control structure
	unsigned char stn;		// Station number
	unsigned char cmd;		// Command - see enum idac_cmnds
	unsigned char svec;		// Start vector number
	unsigned char nvec;		// number of vectors in DtDu array
	DTDS dtdu[31];			// Initial value must be in dtdu[0].du
} IDAC;

enum idac_cmnds {
IDAC_OFF,			// Table interpolation OFF
IDAC_ON,			// Table interpolation ON
IDAC_IVL,			// Set Initial value
IDAC_TBL,			// Write DtDu table fragment
IDAC_EXEC = 0x80	// Execute Flag (with IDAC_TBL) - Write whole table to Device
};

enum cdac_cmd {
	Cdac_AdV = 1,	// Add Vectors
	Cdac_Off = 2,	// Disable Generation
	Cdac_On	 = 3,	// Enable Generation
	Cdac_Lev = 4,	// Set Initial Level 
	Cdac_RPs = 5,	// Read position
	Cdac_Inc = 6,	// Set increment value for dU (1 <= da[0].du <= 127)
	Cdac_Mod = 7,	// Set Integrator Mode  da[0].du = 1 - Discharge, 0 - Work Mode 
	Cdac_Cur = 8,	// Test Current da[0].du = 1 - On, 0 -Off
	Cdac_Rng = 9	// Set Integrator Range da[0].du = 0 - 10pF, 1 - 100 pF
};

typedef struct {		// Serial Device control structure
	short seg;			// Segment number
	short tmo;			// Additional wait time (msec)
	unsigned char wcn;	// Number of data bytes to write
	unsigned char rcn;	// Max Number of data bytes to read
	char udt[122];		// User data
} SDEV;


// *** AccSrv ***
enum ReqAccSrv {
RQ_ZERO = 0,
RQ_RINFO,	// Read Information Events (Status,Errors mesages) 
RQ_EMPTY,	// Empty request, only for CallBack ?
RQ_RSTAT,	// Read state of ???
RQ_RCORI,	// Read DAC code values from 16 correctors
RQ_WCORI,	// Write DAC code values to correctors 
RQ_WECOR,	// Energy correction
RQ_WREGM,	// Write Regim
RQ_RSFPA,	// Read Frequency, phase & Amplitude of RF at injection
RQ_WSFPA,	// Write Start RF Frequency,Phase & Amplitude
RQ_WACPR,	// Write Acceleration parameters
RQ_RIDAC,	// Read data from Magnet Control DAC
RQ_WIDAC,	// Write Data to  Magnet Control DAC
RQ_WFDBK,	// Write Feeadback parameters
RQ_WPEDS,	// Write ped[16 floats]
RQ_RWOSL,	// Read WaveForms for Offset&Size list for FROM->TO update points 
RQ_RDTDF,	// Read  Time&Frq Table
RQ_WDTDF,	// Write Time&Frequncy Table
RQ_RDTDC,	// Read  Time&DelatCorrection Table
RQ_WDTDC,	// Write Time&DelatCorrection Table
RQ_WDTDA,	// Write RF Amplitude Table
RQ_WMISP,	// Write Magnetic Injection Stabilization Parameters
RQ_RDTDA,	// Read RF Amplitude Table
RQ_RCCPR,	// Read Corrector Control Parameters
RQ_WCCPR,	// Write Corrector Control Parameters
RQ_WCAPT,	// Write Capture Table
RQ_RCAPT,	// Read Capture Table
RQ_WBPMG,	// Write BPM Gain
RQ_RSDEV,	// Serial Devices
RQ_RTIMR,	// Read Timer Channels data
RQ_WTIMR,	// Write Timer Channels data
RQ_RCYCD,
// Excitation service
RQ_RFPA2,	// Read Excitation Synthesizer parameters
RQ_WFPA2,	// Write Excitation Synthesizer parameters
RQ_RDTA2,	// Read RF DeltaTime-DeltaAmplitude table for Excitation synthesizer
RQ_WDTA2,	// Write RF DeltaTime-DeltaAmplitude table
RQ_REXFB,	// Read Extraction FeedBack parameters
RQ_WEXFB,	// Write Extraction FeedBack parameters
RQ_REXTB,	// Read Extraction Table
RQ_WEXTB,	// Write Extraction Table
RQ_RDTDO,	// Read Data for PCI Interpolation DAC
RQ_WDTDO,	// PCI Interpolation DACs control (IDAC structure)
// Added 04.02.2010
RQ_RHSPR,	// Read Hall sensors parameters
RQ_WHSPR,	// Write Hall sensors parameters
RQ_RTACR,	// Read Target amplitude correction structure
RQ_WTACR,	// Write it
RQ_LAST};

//#define AmpRange	117.				// Max amp in Volts - corresponds to 0xfff DDS Amplitude Code
//#define RFamplCoef	(4095./AmpRange)
#define PhaseRange	360.				// Max phase in degr - corresponds to 0xfff DDS phase Code
#define RFphaseCoef	(16384./PhaseRange)

enum info_types {
	I_VERSION = 0,
	I_OSLITEM
};

typedef struct {
	int what;		// About What is request
	int size;		// returned number of items ?
	char data[4];
} INFO;

typedef struct {
	char name[32];
//	char unit[4];
	unsigned int osl;
	float gain;
	float offs;
} OSLI;

typedef struct {	// RQ_RSTAT request
	int	SrvState;	// Bit 1 set means that pedestal were sent
	double tStep;	// Synthesizer time quantum in microseconds
	double fCd2MHz;	// from internal frequency representation to Mhz
	double hCd2Gs;	// internal h code to Gs
	double rCd2mm;	// internal orbit shift code to mm
	double raCd2V;	// RF amplitude Code to Volts
	double eaCd2V;	// Exitation amplitude Code to Volts
} STAT;

typedef struct {			// hand = PktWrC(RQRCYCD|FN_CYC|FN_ANS,&cycr,sizeof(CYCR),callback);
	int rq_type;			// rq_type = 0 - only Hall sensor data, 1 - Oscilloscope data & HS
	short tm_scale;			// 0 - 100 MHz, 1 - 50 MHz, 2 - 25 MHZ
	short smp_freq;			// F/2^smp_freq  smp_freq 0..7
	char chn_list[16];		// rq_type = 3 - use chn_list
} CYCR;						// rq_type |= 0x100 - don't change Oscilloscope settings

typedef struct {
	int sts;
	int ost;
	short hst[16];			// Hall sensor temperature
	unsigned short pwm[16];	// Hall sensors Puls-Width Modulation code
	short osc[2];			// 16 x 1024 elements
} CYCD;

typedef struct {
	int hmsk;		// Hall sensors Mask 
	int hcmd;		// Hall sensors command 0x20 - Use Table, 0x30 Use Plane codes
	short hped[16];	// Hall sensors pedestals (will be substracted from measured values)
} HSPR;

typedef struct {
	int	onof;	// On=1 or Off=0 switch for stabilization
	int tick;	// Start tick number for H field average
	float hfil;	// H field desired injection level
	float kh2c;	// H field to corrector DAC code multiplier (Cd/Gs)
	float A2cd;	// Ampere to corrector DAC code multiplier
	float mxac;	// Maximum sum of all correctors current when stabilization is allowed
	int nave;	// number of ticks for magnetic field averaging
} MISP;

typedef struct {
	short dac[16];	// immediate DAC code
} CORI;

typedef struct {			// Corrector Control parameters
	int strt;				// Update tick to start magnet correctors auto control 
	int stpt;				// Update tick to stop 
	int rest;				// Update tick to return correctors to initial state
} CCPR;

typedef struct {			// Energy correction
	int tcor;				// Update tick to make corrrection
	int tres;				// Update tick to return back to initial level
	int ilvl;				// Initial level
	float dfrq;				// desired frequency at tcor
} ECOR;

#define FBTBLSZ	10
typedef struct {			// Orbit-Shift Feedback parameters
	unsigned char bmsk;		// BPM Mask. Whitch BPM include into orbit shif averaging
	char  nint;				// Number of points in timt
	short ntck;				// Number of ticks in averaging interval
	short ntcs;				// Number of garmonics 
	short apnt;				// Approximation point  0...ntck;
	short drmx;				// in 0.005 mm
	float timt[FBTBLSZ];
	float rdes[FBTBLSZ];	// Desired Orbit shift at this point
	float cffb[FBTBLSZ];	// FeedBack Coeficient
} FDBK;

typedef struct {			// Start parameters
	double frq;				// Frequency in MHz
	double ph;				// Phase in degrees
	double p4;				// Start phase for quadro synthesizer
	double amp;				// RF Amplitude in volts
	double ta;				// Start of additional frequency sweeping & alternation in ms
	double dta;				// Sweep Time in ms
	double dfa;				// sweep delta frequency in MHz
	double fs3;				// Frequency shift in MHz for third synthesizer
	double ph3;				// Start phase for third synthesizer
	double am3;				// Amplitude for third synthesizer
} SFPA;

typedef struct {
	unsigned rgm;			// look at -> enum wrgm_rgm
} WRGM;

enum wrgm_rgm {				// Content of WRGM.rgm
	DynFrq = 0x0001,		// Enable dynamic frequency bit
	DynCor = 0x0002,		// Enable dynamic correctors tuning
	DynAmp = 0x0004,		// Enable RF amplitude control
	DynAvg = 0x0008,		// Dynamic averaging
	RgmFdb = 0x0010,		// Enable Orbit shift feedback
	RgmTbF = 0x0020,		// Enable Frqequency table (DynFrq == 0)
	RgmTbC = 0x0040,		// Enable DeltaTime-DeltaCorrector table processing (DynCor == 0)
	RgmTbA = 0x0080,		// Enable RF Amplitude table
	RgmCap = 0x0100,		// Enable Capture table
	RgmFbI = 0x0200,		// Extract Intens Feedback
	RgmEcr = 0x0400,		// Enable Energy correction by adjustment of main field
	RgmEFC = 0x0800,		// Enable Exc. Freq Calculation
	RgmRAp = 0x1000,		// Enable Running averaging method for Orbit feedback
	RgmOFD = 0x2000,		// Enable Orbit Feedback Debug mode (only calculation of ddf)
	RgmACC1= 0x4000,		// (FPA2.Rgm only) Set ACC1 bit in Excitation Synthesizer.
	RgmBis = 0x01000000,	// Set selected bits in RgmWord
	RgmBic = 0x02000000,	// Clear selected bits in RgmWord
	RgmXor = 0x03000000,	// Xor selected bits in RgmWord
	RgmWrP = 0x40000000		// Calculate & Write Parameters depending on r0,s0
};

typedef struct {			// Write Acceleration Parameters
	float r0;				// Ro = 228.(150.) Magnetic Radius 
	float s0;				// So=L = 1434.5 cm
	float dfmx;				// Maximum allowed frequency change (kHz per tick)
	float kdf1;				// Coefficient for df on first time interval
	float kdf2;				// Coefficient for df on second time interval
	unsigned short nHa;		// interval in ticks for magnetic field averaging
} ACPR;

typedef struct {
	short int scell;		// Start vector number
	short int execf;		// Execute flag  1 - Write all loaded table to device
	int ncell;				// number of vectors
	struct {
		unsigned short dt;	// Delta Update Numbers
		short ph;			// 16384 - 360 degr
		float df;			// Delta Frequency Mhz
	} tf[15];				// Data Array of DT & DU
} DTDF;

typedef struct {			// R/W part of Time-Delta Corrector Table (Max 3 vectors)
	short int scell;		// Start vector number
	short int execf;		// Execute flag  1 - Write all loaded table to device
	int ncell;				// number of vectors
	struct {
		int dt;				// Delta Update Numbers
		short dc[16];		// Delta Correctors Array
	} tc[3];
} DTDC;

typedef struct {
	short int scell;		// Start vector number
	short int execf;		// Execute flag  1 - Write all loaded table to device
	int ncell;				// number of vectors
	struct {
		unsigned short dt;	// Delta Update Numbers
		short da;			// Delta Amplitude (max amp 0xfff(4095) - 150 volt)
	} ta[30];
} DTDA;

typedef struct {			// R/W part of Time-Delta Corrector Table (Max 3 vectors)
	short int scell;		// Start vector number
	short int execf;		// Execute flag  1 - Write all loaded table to device
	int ncell;				// number of vectors
	struct {
		float dt;			// Delta Time in mksec
		float fr;			// Frequency in MHz
		short ph;			// Codes: Phase 4096 - 360 gr
		short am;			// Codes: Amplitude 4095 - AmpRange
	} ct[10];
} CAPT;

typedef struct {			// Write peds for Desired frequency. Range: 0.8 .. 16 MHz , Step 0.05 MHz
	float frq;
	float ped[16];			// 06.09.06  findex moved to rq.per field, number of peds == 32
} WPED;

typedef struct {
	float gain[8];
	unsigned short ctrl[8];
} BPMG;

typedef struct {	// Read Waveforms for Offset&Size List
	int fr;					// Start point 
	int to;					// Stop point (inclusive)
	int nit;				// Number of items in Offset&Size List
	union {
		struct {
			unsigned char siz;	// Offset in RBFM structure
			unsigned char ofs;	// Size in bytes
		};
		unsigned short osi;		// Offset and Size combined in one word
	} itl[58];
} RWOL;						

#define O_CYCL 0x0002			// Cycle NUMBER
#define O_INTC (O_CYCL+0x200)	// Interrupt count
#define O_RFFR (O_INTC+0x202)

#define O_CORM	((O_RFFR&0xFF00)+0x420)
#define O_COR0	((O_CORM&0xFF00)+0x02)
#define O_COR1	(O_COR0+0x200)
#define O_COR2	(O_COR1+0x200)
#define O_COR3	(O_COR2+0x200)
#define O_COR4	(O_COR3+0x200)
#define O_COR5	(O_COR4+0x200)
#define O_COR6	(O_COR5+0x200)
#define O_COR7	(O_COR6+0x200)
#define O_COR8	(O_COR7+0x200)
#define O_COR9	(O_COR8+0x200)
#define O_CORA	(O_COR9+0x200)
#define O_CORB	(O_CORA+0x200)
#define O_CORC	(O_CORB+0x200)
#define O_CORD	(O_CORC+0x200)
#define O_CORE	(O_CORD+0x200)
#define O_CORF	(O_CORE+0x200)

#define O_MGSM	(O_CORF+0x21d)	// All 16 magnet sensors

#define O_MGS0  (O_CORF+0x200)	// Magnet Sensor 0
#define O_MGS1  (O_MGS0+0x200)	// Magnet Sensor 1
#define O_MGS2  (O_MGS1+0x200)	// Magnet Sensor 2
#define O_MGS3  (O_MGS2+0x200)	// Magnet Sensor 3
#define O_MGS4  (O_MGS3+0x200)	// Magnet Sensor 4
#define O_MGS5  (O_MGS4+0x200)	// Magnet Sensor 5
#define O_MGS6  (O_MGS5+0x200)	// Magnet Sensor 6
#define O_MGS7  (O_MGS6+0x200)	// Magnet Sensor 7
#define O_MGS8  (O_MGS7+0x200)	// Magnet Sensor 8
#define O_MGS9  (O_MGS8+0x200)	// Magnet Sensor 9
#define O_MGSA  (O_MGS9+0x200)	// Magnet Sensor 10
#define O_MGSB  (O_MGSA+0x200)	// Magnet Sensor 11
#define O_MGSC  (O_MGSB+0x200)	// Magnet Sensor 12
#define O_MGSD  (O_MGSC+0x200)	// Magnet Sensor 13
#define O_MGSE  (O_MGSD+0x200)	// Magnet Sensor 14
#define O_MGSF  (O_MGSE+0x200)	// Magnet Sensor 15
#define O_MGSS  (O_MGSF+0x200)	// Average of Magnet Sensors 

#define O_RFAM	(O_MGSS+0x200)	// Rf Amplitude

#define O_BPMA ((O_RFAM&0xFF00)+0x220)
#if 1
#define O_B1V0 ((O_BPMA&0xFF00)+0x01)
#define O_B1V1 (O_B1V0+0x100)
#define O_B1V2 (O_B1V0+0x200)
#define O_B1V3 (O_B1V0+0x300)
#define O_B2V0 (O_B1V0+0x400)
#define O_B2V1 (O_B2V0+0x100)
#define O_B2V2 (O_B2V0+0x200)
#define O_B2V3 (O_B2V0+0x300)
#define O_B3V0 (O_B2V0+0x400)
#define O_B3V1 (O_B3V0+0x100)
#define O_B3V2 (O_B3V0+0x200)
#define O_B3V3 (O_B3V0+0x300)
#define O_B4V0 (O_B3V0+0x400)
#define O_B4V1 (O_B4V0+0x100)
#define O_B4V2 (O_B4V0+0x200)
#define O_B4V3 (O_B4V0+0x300)

#define O_B5V0 (O_B4V0+0x400)
#define O_B5V1 (O_B5V0+0x100)
#define O_B5V2 (O_B5V0+0x200)
#define O_B5V3 (O_B5V0+0x300)
#define O_B6V0 (O_B5V0+0x400)
#define O_B6V1 (O_B6V0+0x100)
#define O_B6V2 (O_B6V0+0x200)
#define O_B6V3 (O_B6V0+0x300)
#define O_B7V0 (O_B6V0+0x400)
#define O_B7V1 (O_B7V0+0x100)
#define O_B7V2 (O_B7V0+0x200)
#define O_B7V3 (O_B7V0+0x300)
#define O_B8V0 (O_B7V0+0x400)
#define O_B8V1 (O_B8V0+0x100)
#define O_B8V2 (O_B8V0+0x200)
#define O_B8V3 (O_B8V0+0x300)
#endif
// WORD Values
#define O_B1U0 ((O_BPMA&0xFF00)+0x02)
#define O_B1U1 (O_B1U0+0x200)
#define O_B2U0 (O_B1U1+0x200)
#define O_B2U1 (O_B2U0+0x200)
#define O_B3U0 (O_B2U1+0x200)
#define O_B3U1 (O_B3U0+0x200)
#define O_B4U0 (O_B3U1+0x200)
#define O_B4U1 (O_B4U0+0x200)
#define O_B5U0 (O_B4U1+0x200)
#define O_B5U1 (O_B5U0+0x200)
#define O_B6U0 (O_B5U1+0x200)
#define O_B6U1 (O_B6U0+0x200)
#define O_B7U0 (O_B6U1+0x200)
#define O_B7U1 (O_B7U0+0x200)
#define O_B8U0 (O_B7U1+0x200)
#define O_B8U1 (O_B8U0+0x200)

#define O_B1PS  ((O_B8V3&0xFF00)+0x102)	// BPM1 Position
#define O_B2PS  (O_B1PS+0x200)
#define O_B3PS  (O_B2PS+0x200)
#define O_B4PS  (O_B3PS+0x200)
#define O_B5PS  (O_B4PS+0x200)
#define O_B6PS  (O_B5PS+0x200)
#define O_B7PS  (O_B6PS+0x200)
#define O_B8PS  (O_B7PS+0x200)

#define O_B1AM  (O_B8PS+0x200)	// BPM1 Amplitude
#define O_B2AM  (O_B1AM+0x200)
#define O_B3AM  (O_B2AM+0x200)
#define O_B4AM  (O_B3AM+0x200)
#define O_B5AM  (O_B4AM+0x200)
#define O_B6AM  (O_B5AM+0x200)
#define O_B7AM  (O_B6AM+0x200)
#define O_B8AM  (O_B7AM+0x200)

#define O_DRAV  (O_B8AM+0x200)
#define O_MGS  (O_DRAV+0x200)
#define O_MGA  (O_MGS+0x200)
#define O_UNKN  (O_MGA+0x200)
#define O_DFRQ  (O_UNKN+0x202)
#define O_DDFR  (O_DFRQ+0x400)
#define O_BOFS	(((O_DDFR+0x400)&0XFF00)+0x02)
#define O_BINT  (O_BOFS+0x200)	// Beam Intensity from BPM (BPM MASK used)
#define O_ODES  (O_BINT+0x200)	// Desired orbit position for feedback
#define O_OERR  (O_ODES+0x200)	// = O_DRAV - O_ODES
#define O_DTSC	(O_OERR+0x200)	// Time Stamp delta counter
#define O_EXAM	(O_DTSC+0x200)	// Excitation amplitude
#define O_EXFR	(O_EXAM+0x202)	// Excitation frequency
#define O_VAL0	(O_EXAM+0x600)	// 8 channels measured by pci9114 ADC
#define O_VAL1	(O_VAL0+0x200)
#define O_VAL2	(O_VAL1+0x200)
#define O_VAL3	(O_VAL2+0x200)
#define O_VAL4	(O_VAL3+0x200)
#define O_VAL5	(O_VAL4+0x200)
#define O_VAL6	(O_VAL5+0x200)
#define O_VAL7	(O_VAL6+0x200)

#define O_DINT  (O_VAL7+0x200)	// Desired intensity (short)
#define O_OIAV  (O_DINT+0x200)	// Averaged Output intensity (short)	
#define O_CINT  (O_OIAV+0x200)	// Output Intensity	

enum ptc_project_error_list {
PE_OK = 0,	// Status - success
PE_COMOPE,	// Com - Open COM Port Error
PE_COMISG,	// Com - Invalid Segment
PE_COMCMD,	// Com - Command not supported
PE_COMRER,	// Com - Port Read Error
PE_COMTMO,	// Com - TimeOut - there was no record terminator in time
PE_COMOVR,	// Com - Too many bytes in answer
PE_COMCHR,	// Com - Bad Chars in Answer
PE_COMODD,	// Com - Odd byte counter in Answer
PE_COMLDR,	// Com - no Lider in Answer
PE_COMTER,	// Com - no Terminator in Answer
PE_COMCSM,	// Com - Bad CheckSumm
PE_COMECD,	// Com - Device return Error Code
PE_LAST};

#ifndef SIZ
#define SIZ(A)	(sizeof(A)/sizeof(A[0]))	
#endif

#endif
