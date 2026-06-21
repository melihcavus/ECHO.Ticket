# 1. Aşama: Kodları derleme ortamı (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Projedeki tüm dosyaları kopyala
COPY . .

# Paketleri yükle ve projeyi yayına hazırla (Publish)
RUN dotnet restore "ECHO.Ticket.API/ECHO.Ticket.API.csproj"
RUN dotnet publish "ECHO.Ticket.API/ECHO.Ticket.API.csproj" -c Release -o /app/publish

# 2. Aşama: Çalıştırma ortamı (Runtime - Daha hafif)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Render'ın dinleyeceği portu belirliyoruz
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Derlenmiş dosyaları build aşamasından al
COPY --from=build /app/publish .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "ECHO.Ticket.API.dll"]