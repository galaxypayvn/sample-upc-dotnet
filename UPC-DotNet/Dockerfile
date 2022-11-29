FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 9000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_14.x | bash - \
    && apt-get install -y \
        nodejs \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY ["UPC-DotNet/UPC-DotNet.csproj", "UPC-DotNet/"]
COPY ["UPC-Api/UPC-Api.csproj", "UPC-Api/"]
RUN dotnet restore "UPC-DotNet/UPC-DotNet.csproj"
COPY . .
WORKDIR "/src/UPC-DotNet"
RUN dotnet build "UPC-DotNet.csproj" -c Release -o /app/build

FROM build AS publish
ENV TZ="Asia/Ho_Chi_Minh"
RUN dotnet publish "UPC-DotNet.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV TZ="Asia/Ho_Chi_Minh"
ENV ASPNETCORE_ENVIRONMENT Staging
ENTRYPOINT ["dotnet", "UPC.DotNet.dll"]