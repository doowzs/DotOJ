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
  docker build --force-rm --no-cache -f "Dockerfile.$ACTION" --build-arg VERSION="$TAG" -t "$NAME:$TAG" .
  docker image push "$NAME:$TAG"
  ;;
"package")
  cd Dockerize
  cp ../WebApp/appsettings.json.example ./webapp/appsettings.json
  cp ../Worker/appsettings.json.example ./worker/appsettings.json
  cp env-example .env
  sed -i 's/WEBAPP_VERSION=/WEBAPP_VERSION='"$TAG"'/' .env
  sed -i 's/WORKER_VERSION=/WORKER_VERSION='"$TAG"'/' .env
  zip "$NAME" -r ./*
  mv "$NAME" ../
  cd -
  ;;
"*")
  exit 1
  ;;
esac
