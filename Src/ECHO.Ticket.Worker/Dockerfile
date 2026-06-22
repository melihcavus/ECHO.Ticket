# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Build context = repo root (.) olduğu için Src/ ile başlıyoruz
COPY ["Src/ECHO.Ticket.Worker/ECHO.Ticket.Worker.csproj",        "Src/ECHO.Ticket.Worker/"]
COPY ["Src/ECHO.Ticket.Business/ECHO.Ticket.Business.csproj",    "Src/ECHO.Ticket.Business/"]
COPY ["Src/ECHO.Ticket.Core/ECHO.Ticket.Core.csproj",            "Src/ECHO.Ticket.Core/"]
COPY ["Src/ECHO.Ticket.DataAccess/ECHO.Ticket.DataAccess.csproj","Src/ECHO.Ticket.DataAccess/"]
COPY ["ECHO.Ticket.sln", "."]

RUN dotnet restore "Src/ECHO.Ticket.Worker/ECHO.Ticket.Worker.csproj"

COPY . .

RUN dotnet publish "Src/ECHO.Ticket.Worker/ECHO.Ticket.Worker.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ECHO.Ticket.Worker.dll"]

