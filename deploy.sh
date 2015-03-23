#!/bin/bash
#This script sets assembly infos and builds csv-copmpare and zips it in a properly named tar archive

# stop on errors
set -e

#Variables
root="`dirname \"$0\"`"
output=$root"/Modelica_ResultCompare/Properties/AssemblyInfo.cs"
version=$(git describe --tags)
fileversion=$(echo $version | sed 's/v\(\([0-9]\(\.\)\?\)\{3\}\).*/\1/g')
dt=$(date +%Y)

# check for template
if [ -e $input ]
then
        echo found $input
        sed -i 's/\(Assembly\)\(.*\)\(Version\)(.*/\1\2\3("'$fileversion'")]/' $output
        echo file version set to $fileversion
        sed -i 's/\(AssemblyInformationalVersionAttribute\)(.*/\1("'$version'")]/' $output
        echo informational version set to $version
        sed -i 's/\(AssemblyCopyright\).*/AssemblyCopyright("Copyright © '$dt' ITI GmbH")]/' $output
else
        echo "No template found @"$input!
        exit 1
fi
# get shell
if [[ $(uname -s) == MINGW32* ]]
then
	echo running cmd //c $root/"winbuild.bat"
	cmd //c "winbuild.bat"
	# create zip file
	zipper="/c/Program Files/7-Zip/7z.exe"
	"$zipper" a -tzip $root/csv-compare.windows-$version.x64.zip ./Modelica_ResultCompare/bin/x64/Release/Compare.exe BUILD.md LICENSE README.md RELEASENOTES.md
	echo "Successfully created the file $root/csv-compare.windows-$version.x64.tar.gz"
	"$zipper" a -tzip $root/csv-compare.windows-$version.x86.zip  ./Modelica_ResultCompare/bin/x86/Release/Compare.exe BUILD.md LICENSE README.md RELEASENOTES.md
	echo "Successfully created the file $root/csv-compare.windows-$version.x86.tar.gz"
else
	# build release
	make release
	# create zip file
	tar -czf $root/csv-compare.linux-$version.tar.gz Modelica_ResultCompare/bin/Release/Compare.exe BUILD.md LICENSE README.md RELEASENOTES.md
	echo "Successfully created the file csv-compare.linux-$version.tar.gz with the following content:"
	tar tf $root/csv-compare.linux-$version.tar.gz
fi
echo "resetting AssemblyInfo.cs"
git checkout -- $output