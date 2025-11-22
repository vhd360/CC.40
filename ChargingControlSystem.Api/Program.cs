using ChargingControlSystem.Api.Services;
using ChargingControlSystem.Api.Middleware;
using ChargingControlSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent circular reference errors
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Add SignalR for real-time notifications
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // For debugging
});

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with detailed documentation
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.0",
        Title = "ChargingControl System API",
        Description = @"**Charging Station Management System** mit OCPP 1.6 Support

## Features
- 🔋 **Multi-Tenant Ladeverwaltung**
- ⚡ **OCPP 1.6 Integration**
- 👥 **Benutzer- & Gruppenverwaltung**
- 🚗 **Fahrzeugverwaltung**
        
- 💳 **Abrechnung & Billing**
- 📊 **Dashboard & Analytics**
- 🔐 **JWT-Authentifizierung**
- 🏷️ **QR-Code Management**

## Authentifizierung
Die API verwendet **JWT Bearer Tokens**. Erhalten Sie einen Token über:
```
POST /api/auth/login
```

Fügen Sie den Token in jeden Request ein:
```
Authorization: Bearer <your_token>
```",
        Contact = new OpenApiContact
        {
            Name = "ChargingControl Team",
            Email = "support@chargingcontrol.de"
        },
        License = new OpenApiLicense
        {
            Name = "Proprietary",
        }
    });

    // JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization Header mit Bearer Schema.  
**Beispiel:** 'Bearer <your_token>'  

Erhalten Sie einen Token über den `/api/auth/login` Endpoint."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Enable XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Enable annotations
    options.EnableAnnotations();

    // Use method names as operation IDs for better readability
    options.CustomOperationIds(apiDesc =>
    {
        var actionDescriptor = apiDesc.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        return actionDescriptor?.ActionName;
    });

    // Group by tags
    options.TagActionsBy(api => new[]
    {
        api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Default"
    });
    
    options.DocInclusionPredicate((name, api) => true);
});

// Configure routing options
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
        
        // Support JWT authentication for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var path = context.HttpContext.Request.Path;
                
                // SignalR sends token in different ways depending on transport
                if (path.StartsWithSegments("/hubs"))
                {
                    // Try to get token from query string first (WebSocket transport)
                    var accessToken = context.Request.Query["access_token"];
                    
                    // If not in query string, check Authorization header (Long Polling, Server-Sent Events)
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            accessToken = authHeader.Substring("Bearer ".Length).Trim();
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }
                }
                
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// CORS - speziell für SignalR konfiguriert
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => 
            {
                // In Entwicklung: Erlaube localhost auf allen Ports
                var uri = new Uri(origin);
                return uri.Host == "localhost" || uri.Host == "127.0.0.1";
            })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

// Custom Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChargingService, ChargingService>();
builder.Services.AddScoped<ITariffService, TariffService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();

// OCPP Services
// Register a simple factory that creates DbContext instances
builder.Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new SimpleDbContextFactory(connectionString);
});
builder.Services.AddTransient<ChargingControlSystem.OCPP.Handlers.IOcppMessageHandler, ChargingControlSystem.OCPP.Handlers.OcppMessageHandler>();
builder.Services.AddSingleton<ChargingControlSystem.OCPP.Server.OcppWebSocketServer>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<ChargingControlSystem.OCPP.Server.OcppWebSocketServer>>();
    var ocppUrl = builder.Configuration["Ocpp:ServerUrl"] ?? "http://localhost:9000/ocpp/";
    return new ChargingControlSystem.OCPP.Server.OcppWebSocketServer(sp, logger, ocppUrl);
});
builder.Services.AddHostedService<ChargingControlSystem.OCPP.Services.OcppHostedService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IInvoicePdfService, InvoicePdfService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ChargingControl API v1");
        options.DocumentTitle = "ChargingControl System API Documentation";
        options.RoutePrefix = "swagger";
        
        // Enable deep linking
        options.EnableDeepLinking();
        
        // Enable filter box
        options.EnableFilter();
        
        // Display request duration
        options.DisplayRequestDuration();
        
        // Persist authorization data
        options.EnablePersistAuthorization();
        
        // Use DocExpansion to control default expansion
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        
        // Default models expansion
        options.DefaultModelsExpandDepth(2);
    });
}

// Migration beim Start ausführen – vor Middleware-Aufbau
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Middleware - WICHTIG: Reihenfolge beachten!
app.UseHttpsRedirection();

// Static Files für Logo-Uploads
app.UseStaticFiles();

// Routing MUSS als erstes kommen
app.UseRouting();

// CORS nach Routing
app.UseCors("AllowFrontend");

// Authentication
app.UseAuthentication();
app.UseAuthorization();

// Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();

// TenantMiddleware - wird für SignalR übersprungen da SignalR keinen Tenant-Context in diesem Weg nutzt
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/hubs"), appBuilder =>
{
    appBuilder.UseMiddleware<TenantMiddleware>();
});

// Endpoints (müssen nach UseAuthorization kommen)
app.MapControllers();

// Health Check Endpoint (für Docker/Kubernetes)
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    environment = builder.Environment.EnvironmentName
})).AllowAnonymous();

// SignalR Hub für Echtzeit-Benachrichtigungen (mit CORS Policy)
app.MapHub<ChargingControlSystem.Api.Hubs.NotificationHub>("/hubs/notifications")
    .RequireCors("AllowFrontend");

app.Run();

// Simple DbContext Factory for OCPP
class SimpleDbContextFactory : IDbContextFactory<ApplicationDbContext>
{
    private readonly string _connectionString;

    public SimpleDbContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public ApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(_connectionString);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
