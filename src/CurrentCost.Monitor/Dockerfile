#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["CurrentCost.Monitor/CurrentCost.Monitor.csproj", "CurrentCost.Monitor/"]
RUN dotnet restore "CurrentCost.Monitor/CurrentCost.Monitor.csproj"
COPY . .
WORKDIR "/src/CurrentCost.Monitor"
RUN dotnet build "CurrentCost.Monitor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CurrentCost.Monitor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CurrentCost.Monitor.dll"]
