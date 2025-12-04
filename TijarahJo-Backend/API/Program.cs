using Services.PersonServices;

var builder = WebApplication.CreateBuilder(args);

// Dependency Injection setup can be added here
builder.Services.AddScoped<PersonService>();



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthentication(); // JWT
app.UseAuthorization();

app.MapControllers();

app.Run();
