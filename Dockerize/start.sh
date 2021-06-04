#!/usr/bin/env bash

SCALE=$1
if [ "$#" -lt 1 ]; then
    SCALE=1
fi

echo "Starting backend services..."
docker-compose up -d mariadb rabbitmq

echo "Waiting for 15 seconds..."
sleep 15

echo "Starting judge server and worker..."
docker-compose up -d --scale worker=$SCALE
