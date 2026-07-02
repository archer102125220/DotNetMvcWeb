using Scalar.AspNetCore;

using Microsoft.EntityFrameworkCore;
using DotNetMvcWeb.Services.Interfaces;
using DotNetMvcWeb.Services.Implements;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DotNetMvcWeb.Models.IProductRepository, DotNetMvcWeb.Models.ProductRepository>();

// [教學註解] 註冊自訂的 Services
// AddScoped 表示「每一個 HTTP 請求 (Request)」都會產生一個新的 Service 實例。
// 這樣可以確保同一個 Request 中的資料庫連線與狀態是共用且安全的。
builder.Services.AddScoped<IMssqlDemoItemService, MssqlDemoItemService>();
builder.Services.AddScoped<IMssqlDemoCategoryService, MssqlDemoCategoryService>();

builder.Services.AddDbContext<DotNetMvcWeb.Data.AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleDemoConnection")));

builder.Services.AddDbContext<DotNetMvcWeb.Data.MysqlDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("MysqlDemoConnection")!));

builder.Services.AddDbContext<DotNetMvcWeb.Data.PostgresDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDemoConnection")!));

builder.Services.AddDbContext<DotNetMvcWeb.Data.MssqlDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MssqlDemoConnection")!));
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
        
        DotNetMvcWeb.Data.MysqlDbContext mysqlContext = services.GetRequiredService<DotNetMvcWeb.Data.MysqlDbContext>();
        DotNetMvcWeb.Seeders.MysqlDbInitializer.Initialize(mysqlContext);
        
        DotNetMvcWeb.Data.PostgresDbContext postgresContext = services.GetRequiredService<DotNetMvcWeb.Data.PostgresDbContext>();
        DotNetMvcWeb.Seeders.PostgresDbInitializer.Initialize(postgresContext);

        DotNetMvcWeb.Data.MssqlDbContext mssqlContext = services.GetRequiredService<DotNetMvcWeb.Data.MssqlDbContext>();
        DotNetMvcWeb.Seeders.MssqlDbInitializer.Initialize(mssqlContext);
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
