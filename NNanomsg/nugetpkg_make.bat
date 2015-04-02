md nugetpkg
md nugetpkg\tools
md nugetpkg\tools\net40
md nugetpkg\content
md nugetpkg\content\net40
md nugetpkg\content\net40\x86
md nugetpkg\content\net40\x64
md nugetpkg\lib
md nugetpkg\lib\net40
copy NNanomsg.nuspec nugetpkg
copy install.ps1 nugetpkg\tools\net40
copy x86\* nugetpkg\content\net40\x86
copy x64\* nugetpkg\content\net40\x64
copy bin\Release\NNanomsg.dll nugetpkg\lib\net40
