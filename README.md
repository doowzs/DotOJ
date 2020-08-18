# Judge1

A modern frontend for Judge0 API.

## Dependencies

Judge1 depends on multiple open-source projects:

- [.NET Core](https://dotnet.microsoft.com/)
- [Ajax.org Cloud9 Editor](https://ace.c9.io/)
- [Angular 10](https://angular.io/)
- [Angular Material](https://material.angular.io/)
- [ASP.NET Core](https://github.com/dotnet/aspnetcore)
- [Entity Framework Core](https://github.com/dotnet/efcore)
- [Hangfire](https://www.hangfire.io/)
- [Identity Server](https://identityserver.io/)
- [Judge0 API](https://github.com/judge0/api)
- [Node.js](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)

## Requirement

The software requires at least 2GiB of RAM on your host.

There are no extra requirements for now.

## Installation

To prepare a development environment, please follow the following steps:

### 1. Install .NET Core SDK

Proceed to [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) and download .NET Core SDK for your platform. Do not download .NET Framework or you will not be able to make a build. After installation, fire `dotnet` in a shell to check it is correctly installed.

**Important**: You MIGHT see a line in output that tells 'Successfully installed the ASP.NET Core HTTPS Development Certificate.' This would be critical in step 5 for development environment.

Now install Entiry Framework Core CLI tools with the following commands ([reference](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet)):

```shell
$ dotnet tool install --global dotnet-ef
$ dotnet add package Microsoft.EntityFrameworkCore.Design
```

You can verify that the EFCore tool is correctly installed with `dotnet ef`.

### 2. Install Node.js

Visit (https://nodejs.org/)[https://nodejs.org/] and download the latest LTS version installer. After installation, you should check `node --version` and `npm --version` to make sure these two tools are working.

We are not using Yarn for this project. If your Internet connection is not smooth, refer to [this page](https://developer.aliyun.com/mirror/NPM) to learn how to change registry for NPM.

### 3. Install dependencies

Clone this repository and open the folder in a shell.

- Run the following command to install .NET packages.
  ```shell
  $ dotnet restore
  ```
- `cd` into directory `/ClientApp` and run the following command to install Node.js dependencies.
  ```shell
  $ npm install
  ```
  During the installation you might get a prompt for telemetering of Angular. After that you should see Angular modules being compiled locally. You should test installation with `ng version`.

### 4. Configure Data Source

We are using SQL Server Express as the data source. There are two flavors of database installation.

- If you are using Windows or GNU/Linux, you can install SQL server on your host. Visit [the download page](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) and download SQL Server Express installer or find other guides. During the configuration step, choose Developer or Express as the edition of SQL server.
  
  For windows users, an extra step of configuration is required. You need to open SQL Server Configuration Manager after installation, then start SQL Server related Windows service and turn on TCP/IP protocol in Internet configuration. Test the setup using an SQL client listed below and connect to `localhost:1443`.

- If you prefer dockerize, refer to [this guide](https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker) to run SQL server with Docker. The docker container by default provides a Developer edition of SQL server, setting `MSSQL_PID=Express` environment variable will tell the container to start Express edition. See [DockerHub page](https://hub.docker.com/_/microsoft-mssql-server) for more details.

After installing DB, you need to configure the users and create databases. You can use the following common utilities:

- [SSMS (Windows only)](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-manage-ssms)
- [Azure Data Studio (Cross Platform)](https://docs.microsoft.com/en-us/sql/azure-data-studio/what-is)
- [Visual Studio Code](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-develop-use-vscode)
- [mssql-cli](https://github.com/dbcli/mssql-cli/blob/master/doc/usage_guide.md)

Rename `appsettings.json.example` to `appsettings.json` and change its contents according to your installation. Execute the following transcation-SQL commands to prepare a database environment:

```SQL
CREATE LOGIN judge1 WITH PASSWORD = 'judge1', CHECK_POLICY = OFF;
CREATE USER judge1 FOR LOGIN judge1;
GO

CREATE DATABASE judge1;
GO

USE judge1;
EXEC sp_changedbowner judge1;
GO

CREATE LOGIN hangfire WITH PASSWORD = 'hangfire', CHECK_POLICY = OFF;
CREATE USER hangfire FOR LOGIN hangfire;
GO

CREATE DATABASE hangfire;
GO

USE hangfire;
EXEC sp_changedbowner hangfire;
GO
```

Then execute the following command to migrate the database for application:

```shell
$ dotnet ef database update
```

Note for this section:

1. Hangfire DB will be installed on the first run. No manual configuration is required.
2. Roles and an admin user specified in `appsettings.json` will be created if they do not exist.

### 5. Start Judge0 API

Start Judge0 API locally and make it listen on port 3000.

TODO: Add more detail about this step.

### 6. Run the application

Before we run the application for the first time, it is CRITICAL to install and trust an HTTPS development certificate on Windows and macOS. Simply run the following command and trust the certificate, or refer to [this manual](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl):

```shell
$ dotnet dev-certs https --trust
```

Now everything is done and we are ready to go. Issue `dotnet run` and after the compilation you should see the web application running at port 5001.

## Deployment

### X.509 Cerfificate

```shell
$ openssl req -x509 -newkey rsa:4096 -sha256 -nodes \
  -subj "/CN=Judge1" -keyout identity.key -out identity.crt
$ openssl pkcs12 -export -out out/identity.pfx -password pass:identity \
  -inkey identity.key -in identity.crt -certfile identity.crt
```
