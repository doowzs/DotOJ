# Judge1

Docker files for running Judge0 service.

## Configuration

Rename `env-example` to `.env` and change configs in these files:

- `caddy/Caddyfile`: Reverse proxy settings.
- `web/appsettings.json`: Web settings and judger connections.

## Deployment

Dependencies: docker, docker-compose.

Optional dependencies: openssl.

1. Start MariaDB Server with `docker-compose up -d mariadb` and wait it to initialize.
2. Run `web/cert.sh` to create a signing certificate for web service or provide with an existing one.
3. Start all the rest services with `docker-compose up -d`.

## References

Some part of files in this repository has referred to the below project(s):

- [Laradock](https://github.com/laradock/laradock)
