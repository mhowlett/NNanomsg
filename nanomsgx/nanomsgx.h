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

NANOMSGX_API int nn_fd_size();
NANOMSGX_API void nn_poll(void* rcvfds, int slen, int timeout, int* res);
