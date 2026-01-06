using Microsoft.EntityFrameworkCore;
using PriceParser.Web.Data;
using PriceParser.Web.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorPages();


builder.Services.AddHttpClient();


builder.Services.Configure<ParsingOptions>(builder.Configuration.GetSection("Parsing"));


builder.Services.AddSingleton<HtmlFetcher>();
builder.Services.AddSingleton<GenericPriceExtractor>();
builder.Services.AddScoped<PriceParserService>();
builder.Services.AddHostedService<ParsingHostedService>();


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
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
