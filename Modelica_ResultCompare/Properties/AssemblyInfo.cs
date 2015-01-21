using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommandLine;
using CommandLine.Text;

// General information about the assembly
// Change these values to modify to the assembly metadata
[assembly: AssemblyTitle("CSV File Comparison Tool")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ITI GmbH")]
[assembly: AssemblyProduct("compare")]
[assembly: AssemblyCopyright("Copyright ©  2015 - ITI GmbH")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyInformationalVersionAttribute("2.0.0.1")]

// If you need to access this assembly in COM, enable this attribute
[assembly: ComVisible(false)]

// The GUID used if you use COM
[assembly: Guid("5ead2e40-fd60-4095-b96c-4f5a6473f43b")]

[assembly: AssemblyVersion("2.0.0.1")]
[assembly: AssemblyFileVersion("2.0.0.1")]

[assembly: AssemblyLicense("\nThis software is free software and released under The BSD 3-Clause License <http://opensource.org/licenses/BSD-3-Clause>.\n")]
[assembly: AssemblyUsage(@"Example Usage:
 -m csvtreecompare -v 1 -r ""C:\test\report"" ""C:\test\compare\"" ""C:\test\base""

The tool returns 0 if all results were valid and no errors occured during the validation, 1 if there were invalid results and 2 if there were exceptions or errors during the program run.

")]
[assembly:CLSCompliant(true)]
