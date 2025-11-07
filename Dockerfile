########### UI build stage ###########
FROM node:22-alpine AS ui-build
WORKDIR /ui
COPY src/frontend/ClientApp/package*.json ./
RUN npm ci --no-audit --no-fund
COPY src/frontend/ClientApp ./
# Build UI into /ui/dist by overriding Vite outDir
ENV VITE_OUT_DIR=dist
RUN npm run build

########### .NET restore/build stage ###########
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy project for restore
COPY src/backend/ShoppingBasket/ShoppingBasket.csproj src/backend/ShoppingBasket/
RUN dotnet restore src/backend/ShoppingBasket/ShoppingBasket.csproj

# copy source and UI assets; then publish
COPY src/ ./src/
# Copy built UI into backend wwwroot before publish
COPY --from=ui-build /ui/dist/ ./src/backend/ShoppingBasket/wwwroot/
RUN dotnet publish src/backend/ShoppingBasket/ShoppingBasket.csproj -c Release -o /app/publish /p:UseAppHost=false

########### runtime ###########
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# app data volume for sqlite
VOLUME /app/data
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0

COPY --from=build /app/publish .

# ensure sqlite db under /app/data if connectionstring not overridden
ENV ConnectionStrings__Default=Data Source=/app/data/app.db

EXPOSE 8080
ENTRYPOINT ["dotnet", "ShoppingBasket.dll"]
