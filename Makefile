BIN=Modelica_ResultCompare/bin/
OBJ=Modelica_ResultCompare/obj/
BUILD_CMD=xbuild
DBG_ARG=/p:Configuration=Debug
REL_ARG=/p:Configuration=Release
ARGS=/verbosity:quiet /filelogger /flp:logfile=build.log;verbosity=diagnostic

all: clean debug release

debug:
        $(BUILD_CMD) $(DBG_ARG) $(ARGS)
release:
        $(BUILD_CMD) $(REL_ARG) $(ARGS)
publish: clean
        ./deploy.sh
clean:
        rm -rf $(BIN)
        rm -rf $(OBJ)