# Judge1

Docker files for running Judge0 service.

## Configuration

Rename `env-example` to `.env` and change configs in these files:

- `caddy/Caddyfile`: Reverse proxy settings.
- `web/appsettings.json`: Web settings and judger connections.

## Deployment

Dependencies: docker, docker-compose.

Optional dependencies: T-SQL client, openssl.

1. Start SQL Server with `docker-compose up -d mssql` and wait it to initialize.
2. Execute T-SQL commands in `mssql/initdb.sql` to prepare DB environment.
3. Run `web/cert.sh` to create a signing certificate for web service or provide with an existing one.
4. Start all the rest services with `docker-compose up -d`.

## References

Some part of files in this repository has referred to the below project(s):

- [Laradock](https://github.com/laradock/laradock)
