using System.Text;
using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Business.RabbitMQ;
using ECHO.Ticket.Business.Security;
using ECHO.Ticket.Business.Services;
using ECHO.Ticket.Business.Services.Security;
using ECHO.Ticket.Business.Validations;
using ECHO.Ticket.DataAccess.Contexts;
using ECHO.Ticket.DataAccess.Interfaces;
using ECHO.Ticket.DataAccess.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ECHO.Ticket.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURATION & JWT SETTINGS ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];  

// --- 2. CORS POLICY (Bulut ortamı için tüm frontend bağlantılarına açıldı) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontends", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Tüm kaynaklara izin ver
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // SignalR WebSocket bağlantısı için zorunlu
    });
});

// --- 3. AUTHENTICATION & AUTHORIZATION ---
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });
builder.Services.AddAuthorization();

// --- 4. CORE SERVICES & SIGNALR ---
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

// --- 5. SWAGGER CONFIGURATION ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Lütfen aşağıdaki kutuya 'Bearer' yazıp boşluk bıraktıktan sonra Token'ınızı yapıştırın.\r\n\r\nÖrnek: \"Bearer eyJhbGci...\""
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }});
});

// --- 6. CACHING (REDIS) ---
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "EchoTicket_"; 
});

// --- 7. DATABASE (POSTGRESQL) ---
builder.Services.AddDbContext<EchoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 8. DEPENDENCY INJECTION (DI) ---
builder.Services.AddScoped<IWorkContext, WorkContext>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

builder.Services.AddValidatorsFromAssemblyContaining<EventValidator>();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPledgeService, PledgeService>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IMessageProducer, RabbitMQProducer>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IEventReviewService, EventReviewService>();

// HttpClients
builder.Services.AddHttpClient<ISentimentAnalysisService, SentimentAnalysisService>();
builder.Services.AddHttpClient<IAiRecommendationService, AiRecommendationService>();

var app = builder.Build();  

// --- 9. MIDDLEWARE PIPELINE ---
// Render'da canlıdayken de Swagger arayüzünü görebilmek için "if" kontrolünü kaldırdık.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// CORS Middleware'i (Sırası çok önemli: Auth'dan önce olmalı)
app.UseCors("AllowFrontends");

app.UseAuthentication();
app.UseAuthorization();

// Hub ve Controller Map'leri
app.MapHub<TicketHub>("/ticketHub");
app.MapControllers();

app.Run();