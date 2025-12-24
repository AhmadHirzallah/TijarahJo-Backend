using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TijarahJoDBAPI;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TijarahJoDBAPI",
        Version = "v1",
        Description = "API documentation for TijarahJoDBAPI with JWT Authentication"
    });

    // Enable JWT authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token. Example: Bearer eyJhbGciOiJIUzI1NiIs..."
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


// 
///
/// @brief from appsettings.Development.json 
/// Get - Deserialize section "JWT" to JwtOptions class
/// Dotnet by default by default from appsettings.json and appsettings.{Environment}.json
var jwtOption = builder.Configuration.GetSection("JWT").Get<JwtOptions>();


builder.Services.AddSingleton(jwtOption); // Only one  for all the program lifetime

builder.Services.AddScoped<TokenService>(); // Only one  for all the program lifetime

builder.Services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
       .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
       {

           /// <summary>
           /// Instructs the JWT Bearer authentication handler to store the raw bearer token (from
           /// <c>Authorization: Bearer &lt;token&gt;</c>) into the successful authentication result’s
           /// <see cref="Microsoft.AspNetCore.Authentication.AuthenticationProperties"/> (typically as <c>access_token</c>).
           /// This does not change token validation or claims creation; it only makes the original token retrievable later
           /// in the same request pipeline (e.g., <c>HttpContext.GetTokenAsync("access_token")</c>).
           /// </summary>
           options.SaveToken = true; // Save token in AuthenticationProperties after a successful authorization
           
           options.TokenValidationParameters = new TokenValidationParameters()
           {
               ValidateIssuer = true,
               ValidIssuer = jwtOption!.Issuer,

               ValidateAudience = true,
               ValidAudience = jwtOption!.Audience,

               ValidateIssuerSigningKey = true,
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.SigningKey))
           };
       });



// Done
/// Configure CORS to allow specific origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:3456") // PUT THE FRONTEND URL HERE e.g., "http://localhost:3000"
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Done
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
