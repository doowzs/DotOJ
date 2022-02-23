# DotOJ

Also known as: .OJ, NTOJ, OJ Core, etc.

Yet another online judge built on .NET Core tech stack, with ASP.NET and Angular.

> This branch (`2-njucs`) is for teaching use of CS Dept. at Nanjing University only.

## Dependencies

DotOJ depends on multiple open-source projects:

- [.NET Core](https://dotnet.microsoft.com/)
- [Ajax.org Cloud9 Editor](https://ace.c9.io/)
- [Angular 10](https://angular.io/)
- [ASP.NET Core](https://github.com/dotnet/aspnetcore)
- [Bootstrap 4](https://getbootstrap.com/)
- [Entity Framework Core](https://github.com/dotnet/efcore)
- [Identity Server](https://identityserver.io/)
- [Isolate](https://github.com/ioi/isolate)
- [ng-bootstrap](https://ng-bootstrap.github.io/)
- [ngx-markdown](https://github.com/jfcere/ngx-markdown)
- [Node.js](https://nodejs.org/)
- [MariaDB](https://mariadb.org/)
- [Vditor](https://github.com/Vanessa219/vditor)

## Development

**The judging service depends on control group of Linux kernels.**
Judging service relies on Isolate, which requires CGroupV1 (CGroupV2 cannot be used, see deployment).
It is not possible to run workers on Windows or macOS, but the web application is cross-platform.

Follow the steps to prepare a dev environment:

1. Install .NET 5 SDK, Node, MariaDB (MySQL) and RabbitMQ.
2. Edit `Server/appsettings.json` and `Worker/appsettings.json` with database and message queue configuration and edit application information.
3. Restore .NET and Node packages.
   - Run `dotnet restore` to restore all nuget packages.
   - Run `npm install` in `Client` folder to download node dependencies.
4. Run `dotnet run` in `Server` and `Worker` folder to start application. 

## Deployment

### Prerequisite

You need a GNU/Linux operating system with CGroupV1.
For Fedora 31+ and Debian 11+, they use CGroupV2 by default.
You can switch to CGroupV1 by adding `systemd.unified_cgroup_hierarchy=0` to kernel command line.
Besides that, for Debian 8+, you also need to add `cgroup_enable=memory swapaccount=1` to kernel command line.

### Docker Containers

Docker containers are published in registry and namespace `reg.nju.edu.cn/psv/dotoj`. There are six containers to build and run services:

- `sdk`: .NET Core SDK.
- `runtime`: ASP.NET runtime.
- `env`: build environment.
- `node`: node environment.
- `webapp`: web frontend and server.
- `worker`: judge service.

For more information on how to deploy with docker, refer to [Dockerize/README.md](Dockerize/README.md).

### Scaling Application

The application can be easily scaled by adding or removing workers.

However, workers needs to share file system with web app in order to keep judge data updated. On the other hand, if judge data can be made read only, then workers can run on separate environments with connection to the same DB context.

### Create X.509 Cerfificate

```shell
$ openssl req -x509 -newkey rsa:4096 -sha256 -nodes \
  -subj "/CN=DotOJ" -keyout identity.key -out identity.crt
$ openssl pkcs12 -export -out out/identity.pfx -password pass:identity \
  -inkey identity.key -in identity.crt
```
