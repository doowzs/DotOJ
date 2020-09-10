# Judge1 on Docker

Docker files for running Judge1 service.

## Configuration

Rename `env-example` to `.env` and change configs in these files:

- `caddy/Caddyfile`: Reverse proxy settings.
- `webapp/appsettings.json`: Frontend settings.
- `worker/appsettings.json`: Judge worker settings.

## Deployment

Dependencies: docker, docker-compose.

Optional dependencies: openssl.

1. Run `web/cert.sh` to create a signing certificate for web service or provide with an existing one.
2. Start DB service with `docker-compose up -d mariadb` and wait it to initialize.
3. Start all the rest services with `docker-compose up -d`.

## References

Some part of files in this repository has referred to the below project(s):

- [Laradock](https://github.com/laradock/laradock)
