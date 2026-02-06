# Этот этап используется при запуске из VS в быстром режиме
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TreeNodeWebApi/TreeNodeWebApi.csproj", "TreeNodeWebApi/"]
RUN dotnet restore "./TreeNodeWebApi/TreeNodeWebApi.csproj"
COPY . .
WORKDIR "/src/TreeNodeWebApi"
RUN dotnet build "./TreeNodeWebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TreeNodeWebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный этап для production
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/data && chmod 777 /app/data
ENTRYPOINT ["dotnet", "TreeNodeWebApi.dll"]