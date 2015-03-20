@echo off
rem This script creates a delivery package
echo Setting environment variables ...
if exist "%ProgramFiles%\Microsoft Visual Studio 10.0\VC\bin" goto x86
echo Setting environment variables for msbuild (64bit system)
call  "%ProgramFiles(x86)%\Microsoft Visual Studio 10.0\VC\bin\vcvars32.bat"
goto build

:x86
echo Setting environment variables for msbuild (32bit system)
call  "%ProgramFiles%\Microsoft Visual Studio 10.0\VC\bin\vcvars32.bat"

:build
REM Build solutions
echo "Building solutions ..."
msbuild %~dp0\Modelica_ResultCompare.sln /target:Rebuild /p:Configuration=Release;Platform=x86 /noconsolelogger /maxcpucount:5 /filelogger /fileloggerparameters:logfile=%~dp0\deliver.msbuild.win32.err
msbuild %~dp0\Modelica_ResultCompare.sln /target:Rebuild /p:Configuration=Release;Platform=x64 /noconsolelogger /maxcpucount:5 /filelogger /fileloggerparameters:logfile=%~dp0\deliver.msbuild.win64.err