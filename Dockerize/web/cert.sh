#!/bin/bash
set -e

openssl req -x509 -newkey rsa:4096 -sha256 -nodes \
  -subj "/CN=Judge1" -keyout identity.key -out identity.crt
openssl pkcs12 -export -out identity.pfx -password pass:identity \
  -inkey identity.key -in identity.crt -certfile identity.crt
