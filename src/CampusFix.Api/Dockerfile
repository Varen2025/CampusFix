FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source
COPY Directory.Build.props ./
COPY src/CampusFix.Contracts/CampusFix.Contracts.csproj src/CampusFix.Contracts/
COPY src/CampusFix.Api/CampusFix.Api.csproj src/CampusFix.Api/
RUN dotnet restore src/CampusFix.Api/CampusFix.Api.csproj
COPY src/CampusFix.Contracts/ src/CampusFix.Contracts/
COPY src/CampusFix.Api/ src/CampusFix.Api/
RUN dotnet publish src/CampusFix.Api/CampusFix.Api.csproj -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "CampusFix.Api.dll"]
