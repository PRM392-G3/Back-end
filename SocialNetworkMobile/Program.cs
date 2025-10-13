using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SocialNetworkMobile.Repository;
using SocialNetworkMobile.Repository.Basic;
using SocialNetworkMobile.Repository.Context;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Services;
using SocialNetworkMobile.Services.Services.Authentication;
using Supabase;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================= CẤU HÌNH DB =================
builder.Services.AddDbContext<SocialNetworkDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(30);
    }));

// ================= CẤU HÌNH AUTHENTICATION =================
ConfigureAuthentication(builder.Services, builder.Configuration);

// ================= FIREBASE CONFIGURATION =================
try
{
    if (FirebaseApp.DefaultInstance == null)
    {
        string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, builder.Configuration.GetSection("Firebase:CredentialsPath").Value ?? "");
        if (File.Exists(jsonPath))
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(jsonPath)
            });
            Console.WriteLine("FirebaseApp khởi tạo thành công.");
        }
        else
        {
            Console.WriteLine("Firebase credentials file not found. Firebase features will be disabled.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Lỗi khi khởi tạo Firebase: {ex.Message}");
}

// ================= ĐĂNG KÝ REPOSITORY & SERVICE =================
// Repository Layer
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<SocialNetworkDbContext>();

// Service Layer
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Authentication Services
builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
builder.Services.AddScoped<IPasswordEncryptionService, PasswordEncryptionService>();

// ================= CẤU HÌNH SUPABASE =================
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseAnonKey = builder.Configuration["Supabase:AnonKey"];
var supabaseServiceKey = builder.Configuration["Supabase:ServiceRoleKey"];

if (!string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseServiceKey))
{
    builder.Services.AddSingleton<Supabase.Client>(provider => 
    {
        var options = new Supabase.SupabaseOptions
        {
            AutoConnectRealtime = false,
            AutoRefreshToken = false,
            Headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {supabaseServiceKey}" }
            }
        };
        return new Supabase.Client(supabaseUrl, supabaseServiceKey, options);
    });
    Console.WriteLine("Supabase client registered successfully with ServiceRoleKey.");
}
else
{
    Console.WriteLine("Supabase configuration is missing. Storage functionality will not be available.");
}

// ================= CẤU HÌNH CORS =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ================= CẤU HÌNH SWAGGER =================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Social Network API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Vui lòng nhập Bearer Token (VD: Bearer eyJhbGciOi...)",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// ================= CẤU HÌNH CONTROLLERS =================
builder.Services.AddControllers();
builder.Services.AddHttpClient();

var app = builder.Build();

// ================= MIDDLEWARE =================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Social Network API V1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

// ================= AUTHENTICATION CONFIGURATION FUNCTION =================
void ConfigureAuthentication(IServiceCollection services, IConfiguration config)
{
    // Retrieve JWT key from configuration
    var jwtKey = config["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException("JWT Key is missing in the configuration.");
    }

    // JWT Authentication Configuration
    services.AddAuthentication(options =>
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
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("JWT Token validated successfully.");
                return Task.CompletedTask;
            }
        };
    });

    // Google Authentication
    services.AddAuthentication()
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = config["Authentication:Google:ClientId"] ?? "";
            googleOptions.ClientSecret = config["Authentication:Google:ClientSecret"] ?? "";
            googleOptions.SaveTokens = true;
            googleOptions.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
            googleOptions.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");
        });

    // Authorization policies
    services.AddAuthorization();
}