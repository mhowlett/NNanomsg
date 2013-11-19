# nanomsgx makefile
# Copyright (C) 2011 Matt Howlett <matt dot howlett at gmail dot com>
# This file is released under an MIT license, see the LICENSE.txt file.

NANOMSG_PATH=/opt/nanomsg-0.2-alpha/src
NANOMSG_LIB_PATH=/usr/local/lib

all: libnanomsgx

libnanomsgx: nanomsgx
	gcc nanomsgx.o $(NANOMSG_LIB_PATH)/libnanomsg.a -shared -o libnanomsgx.so

nanomsgx: nanomsgx.c nanomsgx.h
	gcc -I$(NANOMSG_PATH) -Wall -c -fPIC nanomsgx.c -o nanomsgx.o

clean:
	rm -rf *.o
	rm -rf *~
	rm -rf libnanomsgx.so
