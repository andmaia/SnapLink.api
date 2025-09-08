# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar csproj para restaurar
COPY Web/Web.csproj Web/
COPY SnapLink.api/SnapLink.api.csproj SnapLink.api/

# Restaurar Web (irá restaurar também API como dependência)
RUN dotnet restore Web/Web.csproj

# Copiar código-fonte
COPY Web Web/
COPY SnapLink.api SnapLink.api/

# Publicar Web e remover appsettings da API
WORKDIR /app/Web
RUN dotnet publish -c Release -o out \
    && rm -f out/appsettings*.json

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/Web/out .

EXPOSE 5001
ENTRYPOINT ["dotnet", "Web.dll"]
