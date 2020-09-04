#!/usr/bin/env bash
set -e

function usage() {
  echo "Usage: ./release.sh [env|webapp|worker|package] [tag]"
}

case "$1" in
"env")
  echo "Building judge1-build-env:latest..."
  CHANGE_SOURCE=true docker build --force-rm --no-cache -f Dockerfile.env -t judge1-build-env:latest .
  ;;
"webapp" | "worker")
  if [ -z "$2" ]; then
    usage
    exit 1
  fi
  echo "Building ccr.ccs.tencentyun.com/doowzs/judge1-$1:$2..."
  CHANGE_SOURCE=true docker build --force-rm --no-cache -f Dockerfile.$1 -t ccr.ccs.tencentyun.com/doowzs/judge1-$1:"$2" .
  ;;
"*")
  usage
  exit 1
  ;;
esac
