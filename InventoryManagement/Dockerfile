# =========================
# GIAI DOAN BUILD
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["InventoryManagement.csproj", "./"]

RUN dotnet restore "InventoryManagement.csproj"

COPY . .

RUN dotnet publish "InventoryManagement.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# =========================
# GIAI DOAN CHAY
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# Chay ung dung bang user khong phai root
USER app

ENTRYPOINT ["dotnet", "InventoryManagement.dll"]
