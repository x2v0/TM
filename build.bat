::  Author: Valeriy Onuchin 02.04.2021

@ECHO OFF
chcp 1251

PUSHD %~dp0
PowerShell -NoProfile -ExecutionPolicy Bypass -Command ".\build.ps1 %*; exit $LastExitCode;"
POPD

pause

