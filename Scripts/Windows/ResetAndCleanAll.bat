pushd %~dp0
CALL .\FindEnginePath.cmd projdir-only
cd %PROJECT_DIR%

git reset --hard
git clean -dxf

popd
pause