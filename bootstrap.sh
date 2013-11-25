#!/usr/bin/env bash

# in the case a machine has already been provisioned, running this script shouldn't be a problem.

# quick and dirty test to see if update has been run yet.
if [ ! -d /opt ]
  then
    apt-get update
fi

apt-get install -y mono-devel
apt-get install -y mono-xbuild
apt-get install -y g++
apt-get install -y make
apt-get install -y libtool
apt-get install -y autoconf
apt-get install -y automake
apt-get install -y uuid-dev
apt-get install -y git
apt-get install -y unzip
apt-get install -y libcurl-openssl-dev
apt-get install -y libcurl4-openssl-dev
apt-get install -y wget
apt-get install -y screen
apt-get install -y libc6-dev-i386

if [ ! -d /opt ]
  then
    mkdir /opt
fi

# acquire and install nanomsg
if [ ! -f /usr/local/lib/libnanomsg.a ]
  then
    cd /opt
    
	# wget http://download.nanomsg.org/nanomsg-0.2-alpha.tar.gz
    # tar xvf nanomsg-0.2-alpha.tar.gz
    # cd nanomsg-0.2-alpha
    
    git clone https://github.com/nanomsg/nanomsg.git
    cd nanomsg
    autoreconf --install
    automake
    
    ./configure
    make
    make install
    ldconfig
fi
