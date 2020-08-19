#!/bin/bash
set -e

cp ./initdb.sql ./Dockerize/mssql/initdb.sql

cp ./appsettings.json ./Dockerize/web/appsettings.json
set -i 's/localhost\\Express/mssql/g' ./Dockerize/web/appsettings.json

cd Dockerize
zip release.zip -r ./*
