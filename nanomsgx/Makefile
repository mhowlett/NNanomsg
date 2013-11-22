# nanomsgx makefile
# Copyright (C) 2013 Matt Howlett <matt dot howlett at gmail dot com>
# This file is released under an MIT license, see the LICENSE.txt file.
# -anl

all: libnanomsgx

libnanomsgx: nanomsgx
	gcc -shared -Wl,-soname,libnanomsgx.so.1 -o libnanomsgx.so nanomsgx.o

nanomsgx: nanomsgx.c nanomsgx.h
	gcc -fPIC -g -c -Wall nanomsgx.c

clean:
	rm -rf *.o
	rm -rf *~
	rm -rf libnanomsgx.so