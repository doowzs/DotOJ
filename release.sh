#!/usr/bin/env sh
set -e

# Usage: ./release.sh [env|webapp|worker|package] name tag1 [tag2]
# If tag2 is present, it will be used to tag; otherwise tag1 is used.

if [ "$1" != "env" ] && [ "$#" -lt 3 ]; then
  ACTION="FATAL"
else
  ACTION=$1
fi

NAME=$2

if [ "$#" -eq 3 ]; then
  TAG="$3"
else
  TAG="$4"
fi

case "$ACTION" in
"env" | "webapp" | "worker")
  echo "Building $ACTION as $NAME:$TAG..."
  CHANGE_SOURCE=true docker build --force-rm --no-cache -f "Dockerfile.$ACTION" -t "$NAME:$TAG" .
  docker image push "$NAME:$TAG"
  ;;
"package")
  cd Dockerize
  sed -i '1s/$/:'"$TAG"'/' webapp/Dockerfile worker/Dockerfile
  zip "$NAME" -r ./*
  mv "$NAME" ../
  cd -
  ;;
"*")
  exit 1
  ;;
esac
