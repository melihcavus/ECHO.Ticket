# 1. Aşama: Kodları derleme ortamı (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# GitHub deposundaki her şeyi (Tüm root'u) içeri kopyala
COPY . .

# Paketleri yükle ve Src altındaki Worker projesini yayına hazırla
RUN dotnet restore "Src/ECHO.Ticket.Worker/ECHO.Ticket.Worker.csproj"
RUN dotnet publish "Src/ECHO.Ticket.Worker/ECHO.Ticket.Worker.csproj" -c Release -o /app/publish

# 2. Aşama: Çalıştırma ortamı (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Render Free Web Service için port tanımı
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Derlenmiş dosyaları al
COPY --from=build /app/publish .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "ECHO.Ticket.Worker.dll"]