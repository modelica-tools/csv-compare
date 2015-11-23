FROM mono:4.2-onbuild
ENTRYPOINT [ "mono",  "/usr/src/app/build/Compare.exe" ]
