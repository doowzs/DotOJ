# DotOJ on Docker

Docker files for running dotOJ service.

## Configuration

Rename `env-example` to `.env` and change configs in these files:

- `webapp/appsettings.json`: Web application settings.
- `worker/appsettings.json`: Judging service settings.

## Deployment

Dependencies: docker, docker-compose.

Optional dependencies: openssl.

1. Run `web/cert.sh` to create a signing certificate for web service or provide with an existing one.
2. Start all services with `start.sh` (usage: `./start.sh WORKER_SCALE`, default scale is 1 worker), stop with `stop.sh`.
