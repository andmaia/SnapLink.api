# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar apenas o csproj do Web
COPY Web/Web.csproj ./Web/

# Restaurar dependências do Web
RUN dotnet restore ./Web/Web.csproj

# Copiar apenas o projeto Web
COPY Web ./Web

# Publicar apenas o Web
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
