using Microsoft.AspNetCore.SignalR;
using SignalRWebpack.Hubs;
using SignalRWebpack.Database;
using SignalRWebpack.Services;
using SignalRWebpack.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR()
    .AddHubOptions<ChatHub>(opt =>
    {
        opt.AddFilter<AuthStateFilter>();
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>();

builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

app.UseRouting();
app.UseCors();

app.MapHub<ChatHub>("/hub");
ApiEndpoints.Map(app);

app.Run();
