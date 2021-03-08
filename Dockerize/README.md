# DotOJ on Docker

Docker files for running dotOJ service.

## Configuration

Rename `env-example` to `.env` and change configs in these files:

- `caddy/Caddyfile`: Reverse proxy settings.
- `webapp/appsettings.json`: Web application settings.
- `worker/appsettings.json`: Judging service settings.

## Deployment

Dependencies: docker, docker-compose.

Optional dependencies: openssl.

1. Run `web/cert.sh` to create a signing certificate for web service or provide with an existing one.
2. Start DB and MQ services with `docker-compose up -d mariadb rabbitmq` and wait them to initialize.
3. Start all the rest services with `docker-compose up -d`.

## References

Some part of files in this repository has referred to the below project(s):

- [Laradock](https://github.com/laradock/laradock)
