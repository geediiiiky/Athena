@echo off
pushd %~dp0

call release ..\OASIS\Gauntlet
call release ..\OASIS\OASIS
call release ..\AshVsEvilDead
call release ..\themachines-client-lfs

popd
