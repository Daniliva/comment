FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
RUN apt-get update && \
    apt-get install -y --no-install-recommends fonts-dejavu-core && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Comments.API/Comments.API.csproj", "Comments.API/"]
COPY ["Comments.Application/Comments.Application.csproj", "Comments.Core/"]
COPY ["Comments.Core/Comments.Core.csproj", "Comments.Core/"]
COPY ["Comments.Infrastructure/Comments.Infrastructure.csproj", "Comments.Infrastructure/"]
RUN dotnet restore "Comments.API/Comments.API.csproj"
COPY . .
WORKDIR "/src/Comments.API"
RUN dotnet build "Comments.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Comments.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Comments.API.dll"]