// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the NANOMSGX_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// NANOMSGX_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef NANOMSGX_EXPORTS
#define NANOMSGX_API __declspec(dllexport)
#else
#define NANOMSGX_API __declspec(dllimport)
#endif

NANOMSGX_API int fnnanomsgx(void);

NANOMSGX_API int getevents (int s, int events, int timeout);
