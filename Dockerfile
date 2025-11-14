FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "HttpServer/HttpServer.csproj"

RUN dotnet publish "HttpServer/HttpServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 1234

ENTRYPOINT ["dotnet", "HttpServer.dll"]
