@echo off
chcp 65001 >nul
title Duplicar Conversación Histórica a FanGest

node "%~dp0..\..\..\Scripts\duplicar_conversacion_fangest.cjs"

echo.
pause
