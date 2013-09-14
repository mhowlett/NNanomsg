## NNanomsg

This is a lightweight wrapper around the <a href="http://nanomsg.org">nanomsg</a> C API which makes
it callable from .NET languages.

The .NET library works without recompiling on both Windows and Linux (the latter using mono), however at 
runtime, it needs to be able to locate the native nanomsg library for your platform as well as nanomsgx, a 
small extras library written in C that is part of this distribution which implements simple polling 
functionality.

NNanomsg is not quite complete, however it is useful for many use cases and it should be relatively straight 
forward to implement the parts of the API that I've yet to complete (I'll probably do this at some point
relatively soon).

The advantages of wrapping the C API rather than implementing it entirely in a .NET language are:
 1. It is much less work. This means that:
 2. I have written very little code in which to introduce bugs. All functionality is provided by the C 
    implementation which should be well tested and tuned. This is an important point and it is something that
    worries me about using a library that offers API's in a large number of different languages - I wonder 
	about the quality of all these implementations!

The disadvantages are:
 1. It forces .NET applications that use the library to be platform dependent. 
 2. It means you have to bugger about with C compilers / native linking which depending on your luck and level 
    of expertise has the potential to cause headaches.
 3. I'm not sure which method is more efficient. Possibly there are GC isses related to pinned memory with this
    type of implementation. In my applications, I've never run into any practical problems however.


### Example

A simple C# example is included in the source distribution and you might also want to look at the Test project.


### Development Status

Partial Implementation of interface only. 

Tested against nanomsg version 0.1alpha only.

Early days, alpha quality. I'd recommend coming back in a few days after I've incorporated it into my own
project and given it a bit more testing...


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