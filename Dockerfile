# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar apenas os arquivos .csproj dos dois projetos
COPY Web/Web.csproj ./Web/
COPY SnapLink.api/SnapLink.api.csproj ./SnapLink.api/

# Restaurar dependências do projeto Web
RUN dotnet restore ./Web/Web.csproj

# Copiar os códigos-fonte dos dois projetos
COPY Web ./Web
COPY SnapLink.api ./SnapLink.api

# Publicar somente o Web (o .NET compila a API como dependência)
WORKDIR /app/Web
RUN dotnet publish -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiar os arquivos publicados do Web
COPY --from=build /app/Web/out .

# Definir variável de ambiente para a URL da API
ENV ApiSettings__BaseUrl=http://localhost:5000

EXPOSE 5001
ENTRYPOINT ["dotnet", "Web.dll"]
