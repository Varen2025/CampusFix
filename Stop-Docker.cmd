@echo off
title Stop CampusFix Docker containers
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Stop-Docker.ps1"
pause
