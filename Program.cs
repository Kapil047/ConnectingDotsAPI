using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services.AwsService;
using ConnectingDotsAPI.Services.AzureService;
using ConnectingDotsAPI.Services.CacheService;
using ConnectingDotsAPI.Services.CustomerService;
using ConnectingDotsAPI.Services.ErrorLoggingService;
using ConnectingDotsAPI.Services.FileService;
using ConnectingDotsAPI.Services.FormService;
using ConnectingDotsAPI.Services.HelperService;
using ConnectingDotsAPI.Services.ProductService;
using ConnectingDotsAPI.Services.PropertyService;
using ConnectingDotsAPI.Services.SettingsService;
using ConnectingDotsAPI.Services.TopicsService;
using ConnectingDotsAPI.Services.traceabilityService;
using ConnectingDotsAPI.Services.UserService;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using ConnectingDotsAPI.Services.OrderService;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add auth headers
builder.Services.AddAuthorizationBuilder()
    // Add services to the container.
    .AddPolicy("Bearer", new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
    };
});

builder.Services.AddDbContext<ConnectingDotsDbContext>(
    options => options.UseMySql(
        configuration.GetConnectionString("CS"),
        new MySqlServerVersion(new Version(8, 0, 0))
    ));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<Dictionary<string, string>>(); // Registering the Dictionary

builder.Services.AddHttpContextAccessor();

builder.Services.AddOutputCache();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IErrorLoggingService, ErrorLoggingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAzureService, AzureService>();
builder.Services.AddScoped<IAWSService, AWSService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHelperService, HelperService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<ITopicsService, TopicsService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITraceabilityService, TraceabilityService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowCors",
                      builder =>
                      {
                          builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                      });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowCors");
app.UseOutputCache();

app.MapControllers();
//app.UseMiddleware<TokenValidationMiddleware>();

app.Run();
