
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ../*.sln ./

COPY ../Web/*.csproj ./Web/
COPY ../SnapLink.api/*.csproj ./SnapLink.api/

RUN dotnet restore ./Web/Web.csproj

COPY ../ .

WORKDIR /src/Web
RUN dotnet publish -c Release -o /app/out


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .


EXPOSE 5001
ENTRYPOINT ["dotnet", "Web.dll"]
