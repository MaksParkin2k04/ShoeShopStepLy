FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ShoeShop.API.csproj", "."]
COPY ["../ShoeShop.Shared/ShoeShop.Shared.csproj", "../ShoeShop.Shared/"]
RUN dotnet restore "ShoeShop.API.csproj"
COPY . .
COPY ../ShoeShop.Shared ../ShoeShop.Shared
WORKDIR "/src"
RUN dotnet build "ShoeShop.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShoeShop.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShoeShop.API.dll"]