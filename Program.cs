using SignalRWebpack.Hubs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

builder.Services.AddSignalR();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<ChatHub>("/hub");

app.Run();
