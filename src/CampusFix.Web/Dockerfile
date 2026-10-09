FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source
COPY Directory.Build.props ./
COPY src/CampusFix.Contracts/CampusFix.Contracts.csproj src/CampusFix.Contracts/
COPY src/CampusFix.Web/CampusFix.Web.csproj src/CampusFix.Web/
RUN dotnet restore src/CampusFix.Web/CampusFix.Web.csproj
COPY src/CampusFix.Contracts/ src/CampusFix.Contracts/
COPY src/CampusFix.Web/ src/CampusFix.Web/
RUN dotnet publish src/CampusFix.Web/CampusFix.Web.csproj -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
RUN mkdir -p /app/App_Data && chown "$APP_UID:$APP_UID" /app/App_Data
USER $APP_UID
ENTRYPOINT ["dotnet", "CampusFix.Web.dll"]
