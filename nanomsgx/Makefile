# nanomsgx makefile
# Copyright (C) 2011 Matt Howlett <matt dot howlett at gmail dot com>
# This file is released under an MIT license, see the LICENSE.txt file.

NANOMSG_PATH=/opt/nanomsg-0.2-alpha/src
NANOMSG_LIB_PATH=/usr/local/lib

all: libnanomsgx

libnanomsgx: nanomsgx
	gcc -shared -Wl,-soname,libnanomsgx.so.1 -o libnanomsgx.so nanomsgx.o $(NANOMSG_LIB_PATH)/libnanomsg.a

nanomsgx: nanomsgx.c nanomsgx.h
	gcc -I$(NANOMSG_PATH) -w -c nanomsgx.c

clean:
	rm -rf *.o
	rm -rf *~
	rm -rf libnanomsgx.so
