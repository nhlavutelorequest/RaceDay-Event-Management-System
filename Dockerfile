# ---- Build stage: restore, compile and publish the MVC app ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first so the NuGet layer is cached until the project file changes
COPY RaceDay.Api/RaceDay.Web/RaceDay.Web.csproj RaceDay.Api/RaceDay.Web/
RUN dotnet restore RaceDay.Api/RaceDay.Web/RaceDay.Web.csproj

COPY RaceDay.Api/RaceDay.Web/ RaceDay.Api/RaceDay.Web/
RUN dotnet publish RaceDay.Api/RaceDay.Web/RaceDay.Web.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---- Runtime stage: small image with only the ASP.NET runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Run as the built-in non-root user
USER $APP_UID

ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]
