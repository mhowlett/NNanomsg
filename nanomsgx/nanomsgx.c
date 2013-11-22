
#include "nanomsgx.h"

#include <stdio.h>
#include <errno.h>


#if defined NN_HAVE_WINDOWS

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <windows.h>
#include <winsock2.h>
#include <mswsock.h>
#include <process.h>
#include <ws2tcpip.h>
#define ssize_t int

#else

#include <sys/select.h>

#endif


#define NN_IN 1
#define NN_OUT 2


int nn_fd_size()
{
#if defined NN_HAVE_WINDOWS
	return sizeof(SOCKET);
#else
	return sizeof(int);
#endif
}


void nn_poll(void* rcvfds, int slen, int timeout, int* res)
{
    int rc;
    fd_set pollset;
#if defined NN_HAVE_WINDOWS
    SOCKET* rcvfd = (SOCKET *)rcvfds;
#else
    int* rcvfd = (int *)rcvfds;
    int maxfd;
#endif

    struct timeval tv;
	int i;

#if !defined NN_HAVE_WINDOWS
    maxfd = 0;
#endif
    
	FD_ZERO (&pollset);

	for (i=0; i<slen; ++i)
	{
		FD_SET (rcvfd[i], &pollset);
#if !defined NN_HAVE_WINDOWS
		if (rcvfd[i] + 1 > maxfd)
		{
			maxfd = rcvfd[i] + 1;
		}
#endif
	}

    if (timeout >= 0) 
	{
        tv.tv_sec = timeout / 1000;
        tv.tv_usec = (timeout % 1000) * 1000;
    }

	// TODO: improve error handling.
#if defined NN_HAVE_WINDOWS
    rc = select (0, &pollset, NULL, NULL, timeout < 0 ? NULL : &tv);
	if (rc == SOCKET_ERROR)
	{
		for (i=0; i<slen; ++i)
		{
			res[i] = 0;
		}
		return;
	}
#else
    rc = select (maxfd, &pollset, NULL, NULL, timeout < 0 ? NULL : &tv);
	if (rc < 0)
	{
		perror("select failed: ");
		for (i=0; i<slen; ++i)
		{
			res[i] = 0;
		}
		return;
	}
#endif

	for (i=0; i<slen; ++i)
	{
		res[i] = 0;
		if (FD_ISSET (rcvfd[i], &pollset))
		{
		    res[i] |= NN_IN;
		}
	}

}
