# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar csproj do Web e do SnapLink.api para restaurar pacotes
COPY Web/Web.csproj ./Web/
COPY SnapLink.api/SnapLink.api.csproj ./SnapLink.api/
RUN dotnet restore ./Web/Web.csproj

# Copiar código-fonte dos dois projetos
COPY Web ./Web
COPY SnapLink.api ./SnapLink.api

# Publicar somente o Web, ignorando appsettings da API
WORKDIR /app/Web
RUN dotnet publish -c Release -o out /p:ExcludeFilesFromPublish="..\SnapLink.api\appsettings*.json"

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/Web/out .

# Definir URL da API como variável de ambiente

EXPOSE 5001
ENTRYPOINT ["dotnet", "Web.dll"]
