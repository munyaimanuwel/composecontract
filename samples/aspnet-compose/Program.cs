using ComposeContract.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseComposeContractKeyCheck(new[]
{
    "ConnectionStrings__Default",
    "ASPNETCORE_ENVIRONMENT"
});
var app = builder.Build();
app.MapGet("/", () => "ok");
app.Run();

public partial class Program { }
