using Microsoft.EntityFrameworkCore;
using PriceParser.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.Configure<PriceParser.Web.Services.ParsingOptions>(builder.Configuration.GetSection("Parsing"));
builder.Services.AddSingleton<PriceParser.Web.Services.GenericPriceExtractor>();
builder.Services.AddScoped<PriceParser.Web.Services.PriceParserService>();
builder.Services.AddHostedService<PriceParser.Web.Services.ParsingHostedService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

app.Run();
