# 1. Aşama: Kodları derleme ortamı (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Projedeki tüm dosyaları (API, Worker, Business vb.) yan yana kopyala
COPY . .

# Paketleri yükle ve Worker projesini yayına hazırla (Publish)
RUN dotnet restore "ECHO.Ticket.Worker/ECHO.Ticket.Worker.csproj"
RUN dotnet publish "ECHO.Ticket.Worker/ECHO.Ticket.Worker.csproj" -c Release -o /app/publish

# 2. Aşama: Çalıştırma ortamı (Runtime - Daha hafif)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Render'ın (Free Web Service) dinleyeceği portu belirtiyoruz
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Derlenmiş dosyaları build aşamasından al
COPY --from=build /app/publish .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "ECHO.Ticket.Worker.dll"]