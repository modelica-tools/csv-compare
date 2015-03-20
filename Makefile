BIN=Modelica_ResultCompare/bin/
OBJ=Modelica_ResultCompare/obj/
BUILD_CMD=xbuild
DBG_ARG=/p:Configuration=Debug
REL_ARG=/p:Configuration=Release

all: clean debug release
	
debug:
	$(BUILD_CMD) $(DBG_ARG)
release:
	$(BUILD_CMD) $(REL_ARG)
clean:
	rm -rf $(BIN)	
	rm -rf $(OBJ)