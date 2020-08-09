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

After installing DB, you need to configure the users and create schemas. You can use the following common utilities:

- [SSMS (Windows only)](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-manage-ssms)
- [Azure Data Studio (Cross Platform)](https://docs.microsoft.com/en-us/sql/azure-data-studio/what-is)
- [Visual Studio Code](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-develop-use-vscode)
- [mssql-cli](https://github.com/dbcli/mssql-cli/blob/master/doc/usage_guide.md)

In this step, you need to:

- Create a schema called `judge1`, create a user called `judge1` identified by `judge1`, and grant all privileges on that schema. The user must use SQL server authentication (i.e. password) instead of Windows authentication.
  ```sql
  CREATE SCHEMA judge1;
  CREATE LOGIN judge1 WITH PASSWORD = 'judge1';
  CREATE USER judge1 FOR LOGIN judge1;
  GRANT ALL PRIVILEGES ON SCHEMA::judge1 TO judge1;
  ```
- Create a schema called `hangfire`, create a user called `hangfire` identified by `hangfire`, and grant all privileges on that schema.
  ```sql
  CREATE SCHEMA hangfire;
  CREATE LOGIN hangfire WITH PASSWORD = 'hangfire';
  CREATE USER hangfire FOR LOGIN hangfire;
  GRANT ALL PRIVILEGES ON SCHEMA::hangfire TO hangfire;
  ```

Ensure that the setup is correct by running this command at root of the repository:

```shell
$ dotnet ef database update
```

TODO: initialize Hangfire DB.

### 5. Run the application

Before we run the application for the first time, it is CRITICAL to install and trust an HTTPS development certificate on Windows and macOS. Simply run the following command and trust the certificate, or refer to [this manual](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl):

```shell
$ dotnet dev-certs https --trust
```

Now everything is done and we are ready to go. Issue `dotnet run` and after the compilation you should see the web application running at port 5001.
