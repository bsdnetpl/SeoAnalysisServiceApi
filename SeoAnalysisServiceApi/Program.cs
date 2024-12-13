using Microsoft.AspNetCore.Builder;
using SeoAnalysisServiceApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Dodanie Swagger do kontenera us³ug
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(); // Dodanie kontrolerów do kontenera us³ug
builder.Services.AddScoped<ISeoAnalysisService, SeoAnalysisService>();

var app = builder.Build();

// Konfiguracja Swagger w œrodowisku deweloperskim
if (app.Environment.IsDevelopment())
    {
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        options.RoutePrefix = string.Empty; // Ustawienie Swaggera jako strony g³ównej
    });
    }

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
