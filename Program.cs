using Scalar.AspNetCore;

using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DotNetMvcWeb.Models.IProductRepository, DotNetMvcWeb.Models.ProductRepository>();

builder.Services.AddDbContext<DotNetMvcWeb.Data.AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleDemoConnection")));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// 呼叫我們自訂的第二種 Seed Data 方式 (DbInitializer)
using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    try
    {
        DotNetMvcWeb.Data.AppDbContext context = services.GetRequiredService<DotNetMvcWeb.Data.AppDbContext>();
        // 執行外部獨立的 Seed 邏輯
        DotNetMvcWeb.Seeders.DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
