#ifndef REMREC_H
#define REMREC_H

// *** SrvCam ***
enum ReqSrvCam {
R4_ZERO = 0xC0,
R4_RINFO,	// Read Information Events (Status,Errors mesages) 
R4_EMPTY,	// Empty request, only for CallBack ?
R4_RCAMR,	// Read Camera Data
R4_WCAMP,	// Write Camera parameters
R4_RCAMP,	// Read Camera parameters
R4_RCAMO,	// Read Camera Options 
R4_RCAMS,	// Read Serial numbers of Camera System (Close&Open camera_interface)
R4_LAST};

typedef struct {
	int camn;		// Camera Number 0...31 to read
	int cams;		// if camn == -1 then cams required camera serial number
} CAMR;

typedef struct {
	int camn;		// Camera Number 0...31
	int cams;		// Camera Serial number (Returned)
	int sts;		// Status (returned)
	int exp;		// Exposition in microseconds
	int gain;		// Master Gain : 1-640 ?
	int encoding;	// Regim: 0 - 8bpp, 1 - 12 bpp
	int binning;	// 0 - 1x1 , 1 - 2x2 
	int h_start;	// начало зоны ROI Horizontal
	int v_start;	// начало зоны ROI Vertical
	int h_size;		// ROI Horizontal size
	int v_size;		// ROI Vertical size
	int trigger;	// Programm or External start
	int gsts;
} CAMP;

typedef struct {
	int camn;		// Camera Number 0...31 (To be filled on request)
	int cams;		// Camera Serial number (Returned) 
	int min_exp;	// Min Exposition
	int max_exp;	// Max Exposition;
	int max_gain;	// Max Gain
	int opt_gain;	// Optimal master gain (recommended min)
	int min_period;
	int width;		// Sensor width
	int height;		// Sensor height
	int color_id;	// 0 - BW, 1 - RGGB ,2 - GRBG
	int gsts;		// Status returned by camera_open function
} CAMO;

#define TRIGMODE_EXTERNAL		1	// External start 
#define TRIGMODE_SOFTSTART		2	// Program start

typedef struct {
	unsigned nCam;				// Number of Cameras attached to system 
	unsigned short SerN[32];	// Array with Cameras serial Numbers 
} CAMS;

typedef struct {		// Returned Picture Header on R4_RCAMR request
	int camn;			// Camera Number
	int cams;			// Camera Serial Number
	unsigned sts;		// returned status 0 - OK
	unsigned bpp;		// bit per pixel 0 - 8, 1 - 12
	unsigned hsz;		// Horizontal size
	unsigned vsz;		// Vertical size
	unsigned psz;		// Picture size in bytes
} PICH;
#endif