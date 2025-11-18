FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ShoeShop.TelegramBot.csproj", "."]
COPY ["../ShoeShop.Shared/ShoeShop.Shared.csproj", "../ShoeShop.Shared/"]
RUN dotnet restore "ShoeShop.TelegramBot.csproj"
COPY . .
COPY ../ShoeShop.Shared ../ShoeShop.Shared
WORKDIR "/src"
RUN dotnet build "ShoeShop.TelegramBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShoeShop.TelegramBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShoeShop.TelegramBot.dll"]