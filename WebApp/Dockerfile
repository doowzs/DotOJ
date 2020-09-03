FROM ccr.ccs.tencentyun.com/doowzs/dotnet-core-sdk:3.1 AS build-env
WORKDIR /app

# Change source of APT for a smoother installation
ARG CHANGE_SOURCE=true
RUN if [ ${CHANGE_SOURCE} = true ]; then \
  sed -i 's/deb.debian.org/mirrors.aliyun.com/' /etc/apt/sources.list; \
  sed -i 's/security.debian.org/mirrors.aliyun.com/' /etc/apt/sources.list \
;fi

# Install node for building SPA
ENV APT_KEY_DONT_WARN_ON_DANGEROUS_USAGE=1
RUN apt-get update -yqq
RUN apt-get install curl gnupg -yqq
RUN apt-get install build-essential -yqq
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash -
RUN apt-get install -y nodejs
RUN if [ ${CHANGE_SOURCE} = true ]; then \
  npm config set registry https://registry.npm.taobao.org \
;fi

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM ccr.ccs.tencentyun.com/doowzs/dotnet-core-aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Judge1.dll"]
