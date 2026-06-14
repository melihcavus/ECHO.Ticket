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

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];  

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

builder.Services.AddControllers();
    
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IWorkContext, WorkContext>();

// SignalR Servisi Eklendi
builder.Services.AddSignalR();

// TEK BİR CORS POLİTİKASI (Hem normal API istekleri hem de SignalR için yeterli)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // SignalR WebSocket bağlantısı için zorunlu
    });
});

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

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "EchoTicket_"; 
});

builder.Services.AddDbContext<EchoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

var app = builder.Build();  

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS Middleware'i sadece bir kez ve Authentication'dan önce çağrılmalı
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

// SignalR Hub Endpoint'i
app.MapHub<TicketHub>("/ticketHub");

app.MapControllers();

app.Run();