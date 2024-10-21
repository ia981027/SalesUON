using SalesUON.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", builder =>
    {
        builder.WithOrigins("http://localhost:5245") // Replace with your Razor Pages app URL
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient("SalesAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5245"); // Replace with your API's base URL
});
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SalesUONDBConnection")));
builder.Logging.ClearProviders()
           .AddConsole(configure => configure.IncludeScopes = true)
           .AddDebug();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // TODO remove
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();
app.UseCors("AllowLocalhost");
app.Run();
