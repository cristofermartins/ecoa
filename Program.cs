using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

using Ecoa.Core.Ports;
using Ecoa.Core.Services;
using Ecoa.Infrastructure.Data;
using Ecoa.Infrastructure.Stellar;
using Ecoa.Infrastructure.Utils;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel((context, serverOptions) =>
    {
        serverOptions.Listen(System.Net.IPAddress.Loopback, 5270);
    });
}

builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "client_app/dist";
});

var mySQLConnectionString = builder.Configuration["MySQLConnectionString"];

var efLogPath = Path.Combine(builder.Environment.ContentRootPath, "logs", "ef-core.log");
Directory.CreateDirectory(Path.GetDirectoryName(efLogPath)!);
var efLogStream = new StreamWriter(efLogPath, append: true) { AutoFlush = true };

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySQLConnectionString, ServerVersion.AutoDetect(mySQLConnectionString))
        .LogTo(log => { lock (efLogStream) { efLogStream.WriteLine(log); } }, LogLevel.Information)
);

builder.Services.AddSingleton<IStellarService>(sp =>
{
    var networkUrl = builder.Configuration["Stellar:NetworkUrl"] ?? "https://soroban-testnet.stellar.org";
    var ecoaTokenContractId = builder.Configuration["Stellar:EcoaTokenContractId"] ?? "";
    var incentivePoolContractId = builder.Configuration["Stellar:IncentivePoolContractId"] ?? "";
    var oracleContractId = builder.Configuration["Stellar:OracleContractId"] ?? "";
    var adminSecretKey = builder.Configuration["Stellar:AdminSecretKey"] ?? "";
    return new StellarService(networkUrl, ecoaTokenContractId, incentivePoolContractId, oracleContractId, adminSecretKey);
});

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IActionRepository, ActionRepository>();
builder.Services.AddScoped<IValidationRepository, ValidationRepository>();
builder.Services.AddScoped<IPoolRepository, PoolRepository>();
builder.Services.AddScoped<ITokenTransactionRepository, TokenTransactionRepository>();
builder.Services.AddScoped<IRideRepository, RideRepository>();
builder.Services.AddScoped<ICyclePathRepository, CyclePathRepository>();

builder.Services.AddHttpClient();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ActionService>();
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PoolService>();
builder.Services.AddScoped<RideService>();

builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

{
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
            };
        });
}

var app = builder.Build();

app.UseCors(builder => builder
     .AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        using (var context = scope.ServiceProvider.GetService<AppDbContext>())
        {
            context!.Database.EnsureCreated();
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    app.MapWhen(context => !context.Request.Path.StartsWithSegments("/api"), builder =>
    {
        builder.UseSpaStaticFiles();
        builder.UseSpa(spa =>
        {
            spa.Options.SourcePath = "client_app";
            spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ResponseHeaders headers = ctx.Context.Response.GetTypedHeaders();
                    headers.CacheControl = new CacheControlHeaderValue
                    {
                        NoCache = true,
                        NoStore = true,
                        MustRevalidate = true
                    };
                }
            };
        });
    });
}
else
{
    app.MapWhen(context => !context.Request.Path.StartsWithSegments("/api"),
        builder =>
        {
            builder.UseSpa(spa => spa.UseProxyToSpaDevelopmentServer("http://localhost:5170"));
        }
    );
}

app.Run();
