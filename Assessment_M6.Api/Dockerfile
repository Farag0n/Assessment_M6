FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Assessment_M6.Api/Assessment_M6.Api.csproj", "Assessment_M6.Api/"]
COPY ["Assessment_M6.Application/Assessment_M6.Application.csproj", "Assessment_M6.Application/"]
COPY ["Assessment_M6.Domain/Assessment_M6.Domain.csproj", "Assessment_M6.Domain/"]
COPY ["Assessment_M6.Infrastructure/Assessment_M6.Infrastructure.csproj", "Assessment_M6.Infrastructure/"]
RUN dotnet restore "Assessment_M6.Api/Assessment_M6.Api.csproj"
COPY . .
WORKDIR "/src/Assessment_M6.Api"
RUN dotnet build "./Assessment_M6.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Assessment_M6.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Assessment_M6.Api.dll"]
