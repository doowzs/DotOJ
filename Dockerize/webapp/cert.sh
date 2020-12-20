#!/bin/bash
set -e

# Certificate cannot be included twice in pfx file.
# See https://github.com/dotnet/runtime/issues/44535

openssl req -x509 -days 365 -newkey rsa:4096 -sha256 -nodes \
  -subj "/CN=Judge1" -keyout identity.key -out identity.crt
openssl pkcs12 -export -out identity.pfx -password pass:identity \
  -inkey identity.key -in identity.crt
