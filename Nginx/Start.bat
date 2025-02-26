@echo off
TITLE Nginx Live Server
ECHO Nginx RTMP server running.....
nginx.exe -c conf\nginx.conf
PAUSE