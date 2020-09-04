#!/usr/bin/env bash
set -e

function usage() {
  echo "Usage: ./release.sh [env|webapp|worker|package] name tag1 [tag2]"
  echo "If tag2 is present, it will be used to tag; otherwise tag1 is used."
}

if [ "$1" != "env" ] && [ "$#" -lt 3 ]; then
  usage
  exit 1
fi

ACTION=$1
NAME=$2
if [ "$#" -eq 3 ]; then
  TAG="$3"
else
  TAG="$4"
fi

case "$ACTION" in
"env")
  echo "Building judge1-build-env:latest..."
  CHANGE_SOURCE=true docker build --force-rm --no-cache -f Dockerfile.env -t judge1-build-env:latest .
  ;;
"webapp" | "worker")
  echo "Building $ACTION as $NAME:$TAG..."
  CHANGE_SOURCE=true docker build --force-rm --no-cache -f "Dockerfile.$ACTION" -t "$NAME:$TAG" .
  ;;
"package")
  cd Dockerize
  sed -i '1s/$/:'"$TAG"'/' webapp/Dockerfile worker/Dockerfile
  zip "$NAME" -r ./*
  mv "$NAME" ../
  cd -
  ;;
"*")
  usage
  exit 1
  ;;
esac
