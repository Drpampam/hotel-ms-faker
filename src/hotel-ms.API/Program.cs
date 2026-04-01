using Asp.Versioning;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using hotelier_core_app.API.Controllers;
using hotelier_core_app.API.Extensions;
using hotelier_core_app.API.Helpers;
using hotelier_core_app.Core.AutofacModule;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Enums;
using hotelier_core_app.Core.Interceptors;
using hotelier_core_app.Domain.AutofacModule;
using hotelier_core_app.Migrations;
using hotelier_core_app.Model;
using hotelier_core_app.Model.Configs;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.AutofacModule;
using hotelier_core_app.Service.AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Support Render.com (and other PaaS) PORT environment variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

/// <summary>
/// Entry point for the hotel-ms API application. Configures services, middleware, and application startup.
/// </summary>
// Register TenantProvider for multi-tenancy
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// Add services to the container.

builder.Services.AddControllers()
    .AddNewtonsoftJson(option =>
                option.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            )
    .AddJsonOptions(option =>
    {
        option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        option.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    .ConfigureApiBehaviorOptions(o =>
    {
        o.InvalidModelStateResponseFactory = context =>
        {
            return new ValidationFailedResult(context.ModelState);
        };
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnectionString"))
           .ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>());

builder.Services.AddApiVersioning(config =>
{
    config.DefaultApiVersion = new ApiVersion(1, 0);
    config.AssumeDefaultVersionWhenUnspecified = true;
    config.ReportApiVersions = true;
})
    .AddMvc(option =>
    {
        var latestApiVersion = new ApiVersion(GlobalConstant.LatestApiVersion, 0);
        option.Conventions.Controller<UserController>().HasApiVersion(latestApiVersion);
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.Configure<RequestLocalizationOptions>(opt =>
{
    // Sets up culture settings for date and time bind
    opt.DefaultRequestCulture = new RequestCulture("en-GB");
    opt.SupportedCultures = new List<CultureInfo> { new CultureInfo("en-GB"), new CultureInfo("en-US") };
    opt.RequestCultureProviders.Clear();
});

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "hotel-ms.API", Version = "v1" });
    option.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Scheme = "bearer"
    });
    option.OperationFilter<SwaggerHeaderFilter>(Array.Empty<object>());
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ModelsAssemblyReference>();
builder.Services.AddTransient<IValidatorInterceptor, RequestModelValidatorInterceptor>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.User.RequireUniqueEmail = true;
})
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtTokenSettings:TokenIssuer"],
        ValidAudience = builder.Configuration["JwtTokenSettings:TokenIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtTokenSettings:TokenKey"] ?? string.Empty))
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminPolicy", policy => policy.RequireRole(UserRole.Admin.ToString()))
    .AddPolicy("DeveloperPolicy", policy => policy.RequireRole(UserRole.Developer.ToString()));

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy());
}

builder.Services.AddMvc()
    .AddMvcOptions(options =>
    {
        options.MaxModelValidationErrors = 999999;
    });

builder.Services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtTokenSettings"));

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureContainer<ContainerBuilder>(builder =>
        {
            builder.RegisterModule(new AutofacCoreContainerModule());
            builder.RegisterModule(new AutofacServiceContainerModule());
            builder.RegisterModule(new AutofacRepositoryContainerModule());
        });

var app = builder.Build();

DatabaseSeeder.Seeder(app.Services).Wait();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRateLimiter();
app.ConfigureExceptionHandler();
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithOrigins(allowedOrigins.Length > 0 ? allowedOrigins : new[] { "http://localhost:3000", "http://localhost:4200", "http://localhost:5173" })
    .AllowCredentials());
var supportedCultures = new[] { new CultureInfo("en-GB"), new CultureInfo("en-US") };

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-GB"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});


// Use TenantMiddleware before authentication/authorization
app.UseTenantMiddleware();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (!app.Environment.IsDevelopment())
{
    app.MapHealthChecks("/healthz");
}
app.Run();
