#!/usr/bin/env bash
set -e

case "$1" in
  "env")
    CHANGE_SOURCE=true docker build -f Dockerfile.env -t judge1-build-env .
    ;;
  "webapp")
    CHANGE_SOURCE=true docker build -f Dockerfile.webapp -t ccr.ccs.tencentyun.com/doowzs/judge1-webapp:"$2" .
    ;;
  "worker")
    CHANGE_SOURCE=true docker build -f Dockerfile.worker -t ccr.ccs.tencentyun.com/doowzs/judge1-worker:"$2" .
    ;;
  "all")
    CHANGE_SOURCE=true docker build -f Dockerfile.env -t judge1-build-env .
    CHANGE_SOURCE=true docker build -f Dockerfile.webapp -t ccr.ccs.tencentyun.com/doowzs/judge1-webapp:"$2" .
    CHANGE_SOURCE=true docker build -f Dockerfile.worker -t ccr.ccs.tencentyun.com/doowzs/judge1-worker:"$2" .
    ;;
  "*")
    echo "Usage: ./release.sh [env|webapp|worker|all] tag"
    exit 1
    ;;
esac
