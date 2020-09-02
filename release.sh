#!/bin/bash
set -e

cp ./appsettings.json.example ./Dockerize/web/appsettings.json

cd Dockerize
zip dockerize.zip -r ./*
mv dockerize.zip ../
