@echo off
pushd %~dp0

RunAssociatedEngine.cmd -run=DerivedDataCache -fill -log
popd