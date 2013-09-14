// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the NANOMSGX_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// NANOMSGX_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.

#if defined _WIN32
#   if defined NANOMSGX_EXPORTS
#       define NANOMSGX_API __declspec(dllexport)
#   else
#       define NANOMSGX_API __declspec(dllexport)
#   endif
#else
#   if defined __SUNPRO_C
#       define NANOMSGX_API __global
#   elif (defined __GNUC__ && __GNUC__ >= 4) || \
          defined __INTEL_COMPILER || defined __clang__
#       define NANOMSGX_API __attribute__ ((visibility("default")))
#   else
#       define NANOMSGX_API
#   endif
#endif

//NANOMSGX_API int nn_poll (int s, int events, int timeout);
