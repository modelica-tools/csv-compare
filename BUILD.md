# Build Instructions
## Windows
To build the tool you need to have:
 * A C# compiler that supports .NET 4.0
 * .NET Framework 4.0
 
Open the Solution (Modelica_ResultCompare.sln) in Visual Studio,
build the Project in the desired configuration (Debug|Release)
and run the "compare.exe" in the output dir.

## Linux
Install mono and git from your repository. If the mono version shipped with ubuntu is too old, use this guide to receive a current version:
http://www.mono-project.com/docs/getting-started/install/linux/#debian-ubuntu-and-derivatives

Tested mono version is:
```bash
$ git clone https://github.com/modelica-tools/csv-compare.git
$ cd csv-compare
$ make
```

After the build you can use `Modelica_ResultCompare/bin/Release/Compare.exe` on Linux