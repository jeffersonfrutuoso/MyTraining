using Microsoft.EntityFrameworkCore;
   using MyTrainingApi.Data;
   using Microsoft.AspNetCore.Authentication.JwtBearer;
   using Microsoft.IdentityModel.Tokens;
   using System.Text;
   using DotNetEnv;

   // Load .env file
   Env.Load();

   var builder = WebApplication.CreateBuilder(args);

   // Add services to the container.
   builder.Services.AddControllers();
   builder.Services.AddDbContext<AppDbContext>(options =>
       options.UseMySql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"),
           ServerVersion.AutoDetect(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"))));

   // Configure JWT Authentication
   var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
   if (string.IsNullOrEmpty(jwtKey))
   {
       throw new InvalidOperationException("JWT Key is missing in environment variables.");
   }

   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
               ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
           };
       });

   builder.Services.AddAuthorization();

   var app = builder.Build();

   // Configure the HTTP request pipeline.
   if (!app.Environment.IsDevelopment())
   {
       app.UseHttpsRedirection();
   }
   app.UseAuthentication();
   app.UseAuthorization();

   // Health check endpoint
   app.MapGet("/", () => "API is running!");

   app.MapControllers();

   app.Run();