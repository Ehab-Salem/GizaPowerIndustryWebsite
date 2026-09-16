
// ===> ConfigureServices 
using GizaPowerIndustryWebsite.InfraDB.DapperContext;
using GizaPowerIndustryWebsite.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OfficeOpenXml;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// create scop and run func when app run 
//using var scope = builder.Services.BuildServiceProvider().CreateScope();
//var services = scope.ServiceProvider;
 


// ========== CORE SERVICES ========== //
builder.Services.AddControllersWithViews();
builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();  // Required for session access 
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IMetalsService, MetalsService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
    client.DefaultRequestHeaders.TryAddWithoutValidation("domain-id", "www");
});

//builder.Services.AddHttpClient("ReportAPI", client =>
//{
//    client.Timeout = TimeSpan.FromMinutes(5); // زيادة وقت الانتظار إلى 5 دقائق
//    client.DefaultRequestHeaders.Add("Accept", "application/json");
//}); // required to connect on microservices API 




// ========== SESSION CONFIGURATION ========== //
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(builder.Configuration.GetValue<int>("SessionTime"));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
  
builder.Services.AddTransient<IDbConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;

    var connectionString = configuration.GetConnectionString("GizaPowerConnection");


    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string cannot be null or empty!");
    }

    return new SqlConnection(connectionString);
});



builder.Services.AddScoped<DapperDBContext>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();

    return new DapperDBContext(configuration, httpContextAccessor);
});
     


builder.Services.AddSignalR(); // إضافة خدمات SignalR
// ========== LOGGING CONFIGURATION ========== //
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});


builder.Services.AddAuthorization();
// Add Authorization Services
//builder.Services.AddSingleton<IAuthorizationPolicyProvider, ControllerActionPolicyProvider>();
//builder.Services.AddScoped<IAuthorizationHandler, ControllerActionAuthorizationHandler>();

builder.Services.AddControllersWithViews();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

builder.Services.AddRazorPages();
builder.Services.Configure<SecurityStampValidatorOptions>(options => { options.ValidationInterval = TimeSpan.Zero; });
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddDistributedMemoryCache(); 

// ========== Configure Request TimeOut on Server  ========== //
builder.WebHost.ConfigureKestrel(option =>
{
    option.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(150);
    option.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(150);
    option.Limits.MaxRequestLineSize = 8192;
    // زيادة الحد إلى 32 KB = 32 * 1024 بايت
    option.Limits.MaxRequestHeadersTotalSize = 32 * 1024;
});

// to compress request 
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // اتفعيل الضغط حتي مع https 
    options.Providers.Add<GzipCompressionProvider>(); // مزود ضغط اضافيه 
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] {"application/json", "text/plain", "application/javascript", "text/css", "text/html" });
});


builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});



var app = builder.Build();

// Register PerformanceReport service for aggregating request timings

// Middleware to measure and log request execution time



//using (var scoped = app.Services.CreateScope())
//{
//    var seederService = scoped.ServiceProvider.GetRequiredService<DataSeederFroLogin>();
//    await seederService.SeedAsync("GizaPowerConnection");
//}



if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    //app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Pages/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseResponseCompression();

// ========== CORS (مهم: قبل كل شيء آخر) ========== //
app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31526000";
    }
});




app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None
});
app.UseRouting();
app.UseSession();


// ========== JWT AND AUTHENTICATION MIDDLEWARE ========== //
//app.Use(async (context, next) =>
//{
//    // 1. معالجة JWT من الكوكيز أولاً
//    var token = context.Request.Cookies["access_token"];
//    if (!string.IsNullOrEmpty(token))
//    {
//        context.Request.Headers.Authorization = $"Bearer {token}";
//    }

//    await next();
//});

//app.UseAuthentication();
app.UseAuthorization(); 

// Register Middleware 

app.UseResponseCaching();


 
 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pages}/{action=Home}/{id?}");


app.UseStatusCodePages(async context =>
{
    var httpContext = context.HttpContext;
    var request = httpContext.Request;
    var response = httpContext.Response;

    if (response.StatusCode != 404)
    {
        return;
    }

    // Avoid redirect loops for static files and non-page requests.
    if (request.Path.StartsWithSegments("/assets")
        || request.Path.StartsWithSegments("/lib")
        || request.Path.StartsWithSegments("/Images")
        || request.Path.StartsWithSegments("/Pages/Error"))
    {
        return;
    }

    if (request.Headers.Accept.Any(h => h != null && h.Contains("text/html", StringComparison.OrdinalIgnoreCase)))
    {
        response.Redirect("/Pages/Error");
    }
});
 


app.MapControllers();

app.Run();