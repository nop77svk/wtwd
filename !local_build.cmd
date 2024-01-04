@echo off
pushd wtwd.cli
call global_build.cmd wtwd Release
set l_result=%ERRORLEVEL%
popd
exit /b %l_result%
