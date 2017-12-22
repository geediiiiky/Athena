@echo off
pushd %~dp0

CALL .\FindEnginePath.cmd

:PATH_FOUND
set "SUB_PATH=Engine\Binaries\Win64\UnrealFrontend.exe"
cd %PROJECT_DIR%
start %ENGINE_PATH%\%SUB_PATH%

popd
