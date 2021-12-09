#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["POSSystems.Api/POSSystems.Api.csproj", "POSSystems.Api/"]
COPY ["POSSystems.Persistence/POSSystems.Persistence.csproj", "POSSystems.Persistence/"]
COPY ["POSSystems.Core/POSSystems.Core.csproj", "POSSystems.Core/"]
COPY ["POSSystems.Helpers/POSSystems.Helpers.csproj", "POSSystems.Helpers/"]
RUN dotnet restore "POSSystems.Api/POSSystems.Api.csproj"
COPY . .
WORKDIR "/src/POSSystems.Api"
RUN dotnet build "POSSystems.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "POSSystems.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY POSSystems.Api/wait-for-it.sh .
RUN chmod +x wait-for-it.sh


