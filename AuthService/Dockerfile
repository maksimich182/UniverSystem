#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

ENV HTTP_PORT=5004
ENV GRPC_PORT=5005
ENV ASPNETCORE_URLS=http://+:${HTTP_PORT};http://+:${GRPC_PORT}

EXPOSE ${HTTP_PORT} ${GRPC_PORT}

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AuthService/AuthService.csproj", "AuthService/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore "AuthService/AuthService.csproj"
RUN dotnet restore "Infrastructure/Infrastructure.csproj"

COPY . .

WORKDIR /src/AuthService
RUN dotnet build "./AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.dll"]