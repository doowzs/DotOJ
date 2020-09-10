#!/usr/bin/env bash
set -e

# https://bugs.debian.org/cgi-bin/bugreport.cgi?bug=863199
mkdir -p /usr/share/man/man1

wget -qO - https://adoptopenjdk.jfrog.io/adoptopenjdk/api/gpg/key/public | apt-key add -
echo "deb http://mirrors.tuna.tsinghua.edu.cn/AdoptOpenJDK/deb buster main" >> /etc/apt/sources.list.d/AdoptOpenJDK.list

apt update -yqq
apt install -y adoptopenjdk-11-hotspot
