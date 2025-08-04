using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using react.core.Server.Data;
using react.core.Server.Repositories;
using react.core.Server.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));


//JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JwtSettings:ValidIssuer"],
        ValidAudience = builder.Configuration["JwtSettings:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? "")),
        ValidateIssuerSigningKey = true
    };
});


//MAPSTER
builder.Services.AddMapster();


//DBCONTEXT - INMEMORY
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("SGKInMemoryDb"));

//DBCONTEXT - POSTGRESQL
//builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
//{
//    options.UseNpgsql(builder.Configuration.GetConnectionString("sgk.app"));
//});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();// so that we can see the open api documentation in developmet mode, remember this one uses server port not the client port.
}

//MIDDLEWARES
app.UseExceptionHandler("/error");
app.UseDefaultFiles();//if there is no file in the request then it will look for default files like index.html
app.MapStaticAssets();//this one serves static files like css js and so on. they are in wwwroot folder and we put it the using vite and npm run build command
app.UseStaticFiles();
app.UseMiddleware<JwtExtractor>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowReactApp");
app.MapControllers();
app.MapFallbackToFile("/index.html");
app.Run();
