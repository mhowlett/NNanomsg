// The nn_poll function below was derived from the getevents method taken from tests/poll.c
// in the main nanomsg library.

#include "nanomsgx.h"

#include "nn.h"
#include "../src/utils/err.c"

#if defined NN_HAVE_WINDOWS
#include "../src/utils/win.h"
#else
#include <sys/select.h>
#endif

#define NN_IN 1
#define NN_OUT 2

NANOMSGX_API void nn_poll(int* s, int slen, int events, int timeout, int* res)
{
    int rc;
    fd_set pollset;
#if defined NN_HAVE_WINDOWS
    SOCKET* rcvfd = (SOCKET *)malloc(slen * sizeof(SOCKET));
    SOCKET* sndfd = (SOCKET *)malloc(slen * sizeof(SOCKET));
#else
    int* rcvfd = (int *)malloc(slen * sizeof(int));
    int* sndfd = (int *)malloc(slen * sizeof(int));
    int maxfd;
#endif
    size_t fdsz;
    struct timeval tv;
    int revents;

	int i;

#if !defined NN_HAVE_WINDOWS
    maxfd = 0;
#endif
    FD_ZERO (&pollset);

    if (events & NN_IN) 
	{
		for (i=0; i<slen; ++i)
		{
			fdsz = sizeof (rcvfd[i]);
			rc = nn_getsockopt (s[i], NN_SOL_SOCKET, NN_RCVFD, (char*) &(rcvfd[i]), &fdsz);
			errno_assert (rc == 0);
			nn_assert (fdsz == sizeof (rcvfd[i]));
			FD_SET (rcvfd[i], &pollset);
#if !defined NN_HAVE_WINDOWS
			if (rcvfd[i] + 1 > maxfd)
			{
				maxfd = rcvfd[i] + 1;
			}
#endif
		}
    }

    if (events & NN_OUT) 
	{
		for (i=0; i<slen; ++i)
		{
			fdsz = sizeof (sndfd[i]);
			rc = nn_getsockopt (s[i], NN_SOL_SOCKET, NN_SNDFD, (char*) &(sndfd[i]), &fdsz);
			errno_assert (rc == 0);
			nn_assert (fdsz == sizeof (sndfd[i]));
			FD_SET (sndfd[i], &pollset);
#if !defined NN_HAVE_WINDOWS
			if (sndfd[i] + 1 > maxfd)
			{
				maxfd = sndfd[i] + 1;
			}
#endif
		}
    }

    if (timeout >= 0) 
	{
        tv.tv_sec = timeout / 1000;
        tv.tv_usec = (timeout % 1000) * 1000;
    }
#if defined NN_HAVE_WINDOWS
    rc = select (0, &pollset, NULL, NULL, timeout < 0 ? NULL : &tv);
    wsa_assert (rc != SOCKET_ERROR);
#else
    rc = select (maxfd, &pollset, NULL, NULL, timeout < 0 ? NULL : &tv);
    errno_assert (rc >= 0);
#endif

	for (i=0; i<slen; ++i)
	{
		res[i] = 0;
		if ((events & NN_IN) && FD_ISSET (rcvfd[i], &pollset))
		{
		    res[i] |= NN_IN;
		}
		if ((events & NN_OUT) && FD_ISSET (sndfd[i], &pollset))
		{
		    res[i] |= NN_OUT;
		}
	}

	free(rcvfd);
	free(sndfd);
}
