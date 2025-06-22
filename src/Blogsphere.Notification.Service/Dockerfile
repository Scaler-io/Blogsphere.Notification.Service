FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Blogsphere.Notification.Service/Blogsphere.Notification.Service.csproj", "Blogsphere.Notification.Service/"]
RUN dotnet restore "Blogsphere.Notification.Service/Blogsphere.Notification.Service.csproj"
COPY . .
WORKDIR "/src/Blogsphere.Notification.Service"
RUN dotnet build "Blogsphere.Notification.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Blogsphere.Notification.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "Blogsphere.Notification.Service.dll" ]