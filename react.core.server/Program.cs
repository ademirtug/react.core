using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using react.core.Server.Data;
using react.core.Server.Repositories;
using react.core.Server.Services;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//CORS
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.AllowAnyOrigin()
			   .AllowAnyHeader()
			   .AllowAnyMethod();
	});
});

//SERVICES
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));


//JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidIssuer = builder.Configuration["JwtSettings:ValidIssuer"],
		ValidAudience = builder.Configuration["JwtSettings:ValidAudience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? "")),
		ValidateIssuerSigningKey = true
	};
});

//DBCONTEXT - IN MEMORY
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("ReactCoreInMemoryDb"));


//DBCONTEXT - POSTGRESQL
//builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
//{
//	options.UseNpgsql(builder.Configuration.GetConnectionString("react.core"));
//});


//REVERSE PROXY HEADERS
//bool useForwardedHeaders = builder.Configuration.GetValue<bool>("UseForwardedHeaders");
//if (useForwardedHeaders)
//{
//	builder.Services.Configure<ForwardedHeadersOptions>(options =>
//	{
//		options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
//		options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
//	});
//}

//RATE LIMIT
builder.Services.AddRateLimiter(options =>
{
	options.AddPolicy<string>("react.core", context =>
	{
		//BEWARE CGNAT!!!!!
		var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

		return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
		{
			AutoReplenishment = true,
			PermitLimit = 1000,
			Window = TimeSpan.FromMinutes(1),
			SegmentsPerWindow = 1,
			QueueLimit = 100
		});
	});
});

//AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();// so that we can see the open api documentation in developmet mode, remember this one uses server port not the client port.
}

//using (var scope = app.Services.CreateScope())
//{
//	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//	db.Database.Migrate(); // applies any pending migrations OR creates db
//}

//MIDDLEWARES
app.UseExceptionHandler("/error");
app.UseDefaultFiles();//if there is no file in the request then it will look for default files like index.html
app.MapStaticAssets();//this one serves static files like css js and so on. they are in wwwroot folder and we put it the using vite and npm run build command
app.UseStaticFiles();
app.UseMiddleware<JwtExtractor>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");
app.Run();
