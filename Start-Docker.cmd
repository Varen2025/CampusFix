@echo off
title Start CampusFix with Docker
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Start-Docker.ps1"
pause
