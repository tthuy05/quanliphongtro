FROM node:22-alpine AS assets
WORKDIR /src
COPY package.json package-lock.json tailwind.config.js ./
COPY Styles ./Styles
COPY scripts ./scripts
COPY Views ./Views
COPY wwwroot/js ./wwwroot/js
RUN npm ci --ignore-scripts && npm run build:assets

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY TroiSinhVien.csproj ./
RUN dotnet restore TroiSinhVien.csproj
COPY . .
COPY --from=assets /src/wwwroot/css/tailwind.generated.css ./wwwroot/css/tailwind.generated.css
COPY --from=assets /src/wwwroot/vendor ./wwwroot/vendor
RUN dotnet publish TroiSinhVien.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM build AS migrations
RUN dotnet tool restore
ENTRYPOINT ["dotnet", "ef", "database", "update", "--configuration", "Release", "--no-build", "--project", "TroiSinhVien.csproj"]

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
RUN addgroup --system app && adduser --system --ingroup app app \
    && mkdir -p /app/App_Data/uploads /app/keys \
    && chown -R app:app /app
COPY --from=build --chown=app:app /app/publish .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "TroiSinhVien.dll"]
