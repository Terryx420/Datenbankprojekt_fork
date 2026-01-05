var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddHttpClient<RocketMoonApp.Server.Services.LaunchService>();
builder.Services.AddHttpClient<RocketMoonApp.Server.Services.MoonService>();
builder.Services.AddScoped<RocketMoonApp.Server.Services.AnalysisService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalDev", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:5285",
                "https://localhost:5285")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.MapStaticAssets();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowLocalDev");
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
