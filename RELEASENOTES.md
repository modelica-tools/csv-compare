## 2019-04-17 Version 2.0.2
* bugfix for meta report for the PlotOnly mode
* bugfix to create unique bitmap file names if the option --bitmap is set
* bugfix for relative paths for the CsvFileCompare mode

## 2018-11-21 Version 2.0.1
* new command line option `--reportnamesep` to specify the name space separator when assembling the report file name from the compare directory structure; only relevant for the CsvTreeCompare mode; the default separator is "."
* improve naming of report file from the structure of the compare directory; only relevant for the CsvTreeCompare mode
* improve meta report to consider untested files
* improve meta report statistics
* update jqPlot to 1.0.9.d96a669
* bugfix for curve compare on corner case CSV files with single or two rows
* bugfix for the creation of the error curve
* bugfix for intersection point search interval
* bugfix for locale independent error values of compare log file
* bugfix for PlotOnly option
* bugfix for reference to metareport directory if there is none
* bugfix for SUCCEEDED table in meta report
* bugfix to not create a temp log file when logging to console
* bugfix to not add charts for missing signals
* bugfix to always export style sheet files independent of bitmap plot option

## 2015-03-23 Version 2.0.0
* new command line option `--inline` to insert scripts and styles in the report html file (easier to exchange reports with others); the default behaviour was to insert the scripts/styles in the code this has been changed to generate scripts and styles only once and reference in all other html files to the generated files
* new command line option `--bitmap` toggles to raster plots in reports to make the files smaller and better to parse in case of very much points to plot (browsers freezed when using vector plots)
* a new tube algorithm has been implemented and is used now, this fixed several bugs
* reports in tree mode are now written per compared csv files, this keeps a lower memory consumption
* a significantly faster csv parsing has been implemented
* locale independence has been ensured
* new option `--separator` can now be set to change the separator used in csv files
* moved repository from modelica.org svn to GitHub

## 2013-06-26 Version 1.0.0.6751
* bugfix error graph (list has not been reset)
* new commandline parameter `-e` for enabling absolute errors (0,1) in error graph; new default behaviour is to show relative differences between error and tube
* added check for resolution of base file and documentation in the readme. Base files should have a higher resolution than the compare files
* use `--nometareport` to skip the generation of an index file for the reports

## 2013-06-14 Version 1.0.0.6717
* added error graph to ease the discovery of errors in the graphs
* stopping recursion instead of throwing exception when calculating tubes for really big files
* better handling of "jumps" during the calculation of tubes

## 2013-06-14 Version 1.0.0.6673
* enabled relative paths to html files in (meta)reports
* catched parsing errors in tree mode skip failed files instead of stopping the test
* documentation extended

## 2013-06-05 Version 1.0.0.6623
* bugfixes for reporting, the directories were wrong if no report directory has been set.
* `--override` has been fixed to be a boolean flag. Default is "no override" now.
* added some color to the console output
* more output on parsing errors has been added
* extrapolation of values if the source time line is shorter than the target timeline is now prevented
* some doumentation has been done in readme and cli usage info

## 2013-05-30 Version 1.0.0.6573 beta
* added option `--delimiter` to set the delimiter of the input csv files
* changed the csv parser algorithm to use regular expressions
* merged CommandLineParser source code to project
* changed project settings to enable build with Mono 2.*
* added x64 platform configuration
* extended build script for windows to build x86 & x64 binaries and zip them

## 2013-05-24 Version 1.0.0.6473 beta
* bugfix for report directories; when given as relative path and with no `--reportdir` the output paths for the html have been set wrong

## 2013-05-17 Version 1.0.0.6381 beta
* initial beta test release
