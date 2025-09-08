# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar solução e projeto Web
COPY *.sln .
COPY Web/*.csproj ./Web/
RUN dotnet restore

# Copiar tudo e publicar
COPY . .
WORKDIR /app/Web
RUN dotnet publish -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/Web/out .
EXPOSE 5001
ENTRYPOINT ["dotnet", "Web.dll"]
