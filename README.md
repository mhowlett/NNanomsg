## NNanomsg

This is a lightweight wrapper around the <a href="http://nanomsg.org">nanomsg</a> C API which makes
it callable from .NET languages.

The .NET library works without recompiling on both Windows and Linux (the latter using mono), however at 
runtime, it needs to be able to locate the native nanomsg library for your platform as well as nanomsgx, a 
small library written in C that is part of this distribution that implements simple polling functionality.

The advantages of wrapping the C API rather than implementing it entirely in a .NET language are:
 1. It is much less work. This means that:
 2. I have written very little code in which to introduce bugs. All functionality is provided by the C 
    implementation which should be well tested and tuned. This is an important point and it is something that
    worries me about using a library that offers API's in a large number of different languages - I wonder 
    about the quality of all these implementations!

The disadvantages are:
 1. .NET applications that use the library cannot be run on different platforms without acquiring/building nanomsg
    and nanomsgx for those platforms.
 2. It means you might have to bugger about with C compilers / native linking which depending on your luck and level 
    of expertise has the potential to cause headaches. Note: I've included x86 Windows and Linux binaries in git, so
    if you are on one of those platforms, you are in luck.
 3. I'm not sure which method is more efficient. 

### Example

A simple C# example is included in the source distribution and you might also want to look at the Test project.


### Development Status

Tested against nanomsg version 0.2-alpha only.

sendmsg and recvmsg not yet implemented (but send and recv are). 

Polling functionality could likely be implemented completely in managed code (get rid of the extra native library),
and more properly integrated with the .NET runtime / ThreadPool. However elements of this are quit difficult, and 
the present implementation works :-).

I'd recommend checking back reltively frequently as I've yet to give it a good work out, but will be doing so over
the coming weeks.


### Building Notes

Build nanomsg.dll and/or libnanomsg.so as instructed by the nanomsg.org website and make sure they can be found 
by the .NET runtime when you execute your application. I tend to include these libraries in my project and have 
them coppied automatically into the application directory when built.

You will need to set the 'Platform Target' to x86 in any project that references NNanomsg.

To build nanomsx, which will be referenced if you try to use the polling functionality, you will need to specify
the path to the nanomsg source directory as follows:

On Windows, in Visual Studio:
  1. Select View/Other Windows/Property Manager
  2. double click Microsoft.Cpp.Win32.user
  3. Under C/C++ / General, edit "Additional Include Directories"
  4. Under Linker / General, edit "Additional Library Directories"

On Linux:
  1. Edit the -I directive in the Makefile to specify the location of the root nanomsg source directory.

I've included a Vagrantfile that builds a VM with nanomsg and mono installed which can be used to run the test 
project.
