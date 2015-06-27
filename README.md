## NNanomsg

[![Join the chat at https://gitter.im/mhowlett/NNanomsg](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/mhowlett/NNanomsg?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

NNanomsg is a .NET binding for <a href="http://nanomsg.org">nanomsg</a>. It can be used without recompiling
on any platform where .NET is available.

At runtime NNanomsg needs to be able to locate the native nanomsg library for your platform. The following 
paths are searched (in order) until the native library is found:

     Windows:   [Application Directory]\bin\[x86|x64]
                [Application Directory]\[x86|x64]
                [Application Directory]
                [NNanomsg.dll Directory]\bin\[x86|x64]
                [NNanomsg.dll Directory]\[x86|x64]
                [NNanomsg.dll Directory]

     Posix:     [Application Directory]/bin/[x86|x64]
                [Application Directory]/[x86|x64]
                [Application Directory]
                /usr/local/lib
                /usr/lib

For convenience, we've included nanomsg binaries for Windows (x86 and x64) and Linux (x86 and x64) in the git repository.

### Example

A simple C# example is included in the source distribution and you might also want to look at the Test project.

### Usage

NNanomsg actually exposes two API's:

 1. Static methods in the NN class. These match those of the C API very closely.
 2. NanomsgSocket and associated classes which provide a higher level idiomatic .NET interface.

For most applications, the higher level interface should be used.


### Development Status

Alpha quality. We're still debating the best way to structure some functionality and parts of the API will likely change.

The current version of NNanomsg requires nanomsg-0.3-beta or greater.

Only tested on linux/mono and Windows/Microsoft CLR.


### Building Notes

A Vagrantfile that builds a VM with nanomsg and mono installed which can be used to run the example project 
is included.

Procedure used for generating shared nanomsg libraries: 

#### Windows x86

     cmake.exe . -G "Visual Studio 12"

then Release build in Visual Studio 2013

#### Windows x64

     cmake.exe . -G "Visual Studio 12 Win64"

then Release build in Visual Studio 2013

#### Linux x86/x64

     autoreconf -fi
     ./configure --enable-shared
     make
     make install

.so library is in /usr/local/lib

#### NuGet package

1. do a release build of NNanomsg in Visual Studio.
2. execute the script nugetpkg_make.bat in the NNanomsg project directory
3. cd nugetpkg
4. NuGet.exe pack


### Primary Contributors

  * [Matt Howlett](https://www.matthowlett.com)
  * Kyle Patrick (kwpatrick)
