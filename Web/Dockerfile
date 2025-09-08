# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar os projetos referenciados e o Web.csproj
COPY ../SnapLink.api ./SnapLink.api
COPY *.csproj ./

# Restaurar dependências
RUN dotnet restore

# Copiar o restante do projeto Web
COPY . .

# Publicar Web
RUN dotnet publish -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 5001
ENTRYPOINT ["dotnet", "Web.dll"]
