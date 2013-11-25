## NNanomsg

This is a lightweight wrapper around the <a href="http://nanomsg.org">nanomsg</a> C API which makes
it callable from .NET languages.

NNanomsg can be used without recompiling on any platform where .NET is available. At runtime, it needs 
to be able to locate the native nanomsg library for your platform. By default, the following paths are
searched (in order):

     Windows:   [Application Directory]\bin\[x86|x64]
                [Application Directory]\[x86|x64]
                [Application Directory]

     Posix:     [Application Directory]/bin/[x86|x64]
                [Application Directory]/[x86|x64]
                [Application Directory]
                /usr/local/lib
                /usr/lib

For convenience, we distribute nanomsg binaries for Windows (x86 and x64) and Linux (x86 and x64).

The advantages of wrapping the C API rather than implementing it entirely in a .NET language are:
 1. It is much less work. This means that:
 2. There is very little code in which to introduce bugs. 

The disadvantages are:
 1. .NET applications that use the library cannot be run on different platforms without acquiring/building
    nanomsg for those platforms.
 3. I'm not sure which method is more efficient. 

### Example

A simple C# example is included in the source distribution and you might also want to look at the Test
project.

Work in progress...

### Usage

NNanomsg actually exposes API's at three levels of abstraction:

 1. Static methods in the NN class. These match those of the C API very closely.
 2. NanomsgSocket, which provides more convenience functionality than the NN methods.
 3. Protocol classes: PairSocket, PublishSocket, SubscribeSocket etc. which restrict the allowed operations to
    those apporpriate to the associated socket type.

For most applications, the protocol classes should be used.


### Development Status

Alpha quality. 

We're still debating the best way to structure some functionality and parts of the API will likely change.

Requires a version of nanomsg that provides nn_poll. This is not included in the alpha-0.2 releases - you
must get master from github, or use one of the binaries provided in this distribution.


### Building Notes

A Vagrantfile that builds a VM with nanomsg and mono installed which can be used to run the example project 
is included.


### Primary Contributors

  * Matt Howeltt
  * Mason Of Words
