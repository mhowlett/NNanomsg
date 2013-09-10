## NNanomsg

This is a very lightweight wrapper around the Nanomsg C API which makes it callable from .NET languages.

It works (without recompiling) on Windows and (using mono) on Linux. 

So far, I have only implemented the subset of functionality that I need, however it should be relatively straight forward and not take very long to implement the rest of it (I may do this myself, at least a little more very soon).

The advantages of wrapping the C API rather than implementing it entirely in a .NET language are:
 1. It is much less work. This means that:
 2. I have written very little code in which to introduce bugs. All functionality is provided by the C implementation which should be well tested and tuned. This is an important point and it is something that worries me about using a library that offer API's in a large number of different languages - I wonder about the quality of all these implementations!

The disadvantages are:
 1. It forces .NET applications that use the library to be platform dependent. 
 2. It meens you have to bugger about with C compilers / native linking which depending on your luck and level of expertise has the potential to cause headaches.
 3. I'm not sure which method is more efficient. Possibly there are gc isses related to pinned memory with this type of implementation. In my applications, I've never run into any problems however.


# Example

A simple example is included in the source distribution.


# Building Notes

Build nanomsg.dll and/or libnanomsg.so as instructed by the <a href="http://nanomsg.org">nanomsg.org</a> and make sure they can be found by the .NET runtime when you execute your application. I tend to include these libraries in my project and have them coppied automatically into the application directory when built.
