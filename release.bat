@echo off
pushd %~dp0

IF "%1"=="" (
	SET /p PROJECT_DIR=input project dir:
) ELSE (
	SET PROJECT_DIR=%1
)

IF EXIST %PROJECT_DIR% (
	echo copying athena and scripts to %PROJECT_DIR%
	echo.
	xcopy /Y Athena\bin\Release\Athena.exe %PROJECT_DIR%
	xcopy /E /Y /I Scripts %PROJECT_DIR%\Scripts
	echo.
) ELSE (
	echo %PROJECT_DIR% is not valid path
)

popd
pause