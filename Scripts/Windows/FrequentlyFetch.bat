@echo off
pushd %~dp0
CALL .\FindEnginePath.cmd projdir-only
cd %PROJECT_DIR%

:DO_FETCH
echo.
echo.
echo %TIME%
git fetch
git lfs fetch --recent
SET INTERVAL=1800
SET HOUR=%time:~0,2%
REM if later than 22:00, schdule the next update in 11 hours
if %HOUR% GTR 21 SET INTERVAL=39600
TIMEOUT /T %INTERVAL%

GOTO :DO_FETCH

popd
pause

