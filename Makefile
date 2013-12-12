all: Modelica_ResultCompare/Properties/AssemblyInfo.cs
	xbuild /p:Configuration=Release
Modelica_ResultCompare/Properties/AssemblyInfo.cs: Modelica_ResultCompare/Properties/AssemblyInfo.cs.template
	sed 's/$$WCREV[$$]'"/`svn info | grep '^Revision:' | cut -d\  -f2`/" $< > $@.tmp
	mv $@.tmp $@
