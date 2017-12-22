pushd %~dp0
CALL .\FindEnginePath.cmd projdir-only
cd %PROJECT_DIR%
git submodule foreach --recursive git fetch
git submodule foreach --recursive git lfs fetch --recent
git submodule foreach --recursive git pull
popd
pause
