## NNanomsg

NNanomsg is a .NET binding for <a href="http://nanomsg.org">nanomsg</a>. It can be used without recompiling
on any platform where .NET is available. However, at runtime, it needs to be able to locate the native
nanomsg library for your platform.

The following paths will be searched (in order) until the native library is found:

     Windows:   [Application Directory]\bin\[x86|x64]
                [Application Directory]\[x86|x64]
                [Application Directory]

     Posix:     [Application Directory]/bin/[x86|x64]
                [Application Directory]/[x86|x64]
                [Application Directory]
                /usr/local/lib
                /usr/lib

For convenience, we've included nanomsg binaries for Windows (x86 and x64) and Linux (x86 and x64) in the 
git repository.

### Example

A simple C# example is included in the source distribution and you might also want to look at the Test
project.

Work in progress...

### Usage

NNanomsg actually exposes two API's:

 1. Static methods in the NN class. These match those of the C API very closely.
 2. NanomsgSocket and associated classes which provide a higher level idiomatic .NET interface.

For most applications, the higher level interface should be used.


### Development Status

Alpha quality. 

We're still debating the best way to structure some functionality and parts of the API will likely change.

NNanomsg now requires a version of nanomsg that provides nn_poll. This is not included in the alpha-0.2 releases - you
must get master from github, or use one of the binaries provided in this distribution.

Only tested on linux/mono and Windows/Microsoft CLR.


### Building Notes

A Vagrantfile that builds a VM with nanomsg and mono installed which can be used to run the example project 
is included.


### Primary Contributors

  * Matt Howeltt
  * Mason Of Words
