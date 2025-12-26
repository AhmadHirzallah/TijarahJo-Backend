using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TijarahJoDBAPI;
using TijarahJoDBAPI.Services;
using TijarahJoDBAPI.Services.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TijarahJoDBAPI",
        Version = "v1",
        Description = "TijarahJo Marketplace API with JWT Authentication and Role-Based Authorization"
    });

    // Enable JWT authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

// JWT Configuration
var jwtOptions = builder.Configuration.GetSection("JWT").Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing");

builder.Services.AddSingleton(jwtOptions);
builder.Services.AddScoped<TokenService>();

// Register Security Services (Password Hashing)
builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();

// Register HttpContextAccessor for ImageUploadService
builder.Services.AddHttpContextAccessor();

// Register ImageUploadService for handling file uploads
builder.Services.AddScoped<ImageUploadService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.SaveToken = true;
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No tolerance for token expiration
    };

    // Custom response for authentication failures
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Skip default response
            context.HandleResponse();
            
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = "Unauthorized",
                status = 401,
                detail = "You are not authorized to access this resource. Please provide a valid JWT token."
            });
            
            return context.Response.WriteAsync(result);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = "Forbidden",
                status = 403,
                detail = "You do not have permission to access this resource."
            });
            
            return context.Response.WriteAsync(result);
        }
    };
});

// Add Authorization policies
builder.Services.AddAuthorization(options =>
{
    // Default policy requires authenticated user
    options.FallbackPolicy = null; // Allow anonymous by default
    
    // Admin-only policy
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    
    // Admin or Moderator policy
    options.AddPolicy("AdminOrModerator", policy =>
        policy.RequireRole("Admin", "Moderator"));
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Token-Expired"); // Expose custom header to frontend
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable serving static files from wwwroot (for uploaded images)
app.UseStaticFiles();

// CORS must be before Authentication
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
