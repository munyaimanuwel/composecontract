using StackContract.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseStackContractKeyCheck(new[]
{
    "ConnectionStrings__Default",
    "ASPNETCORE_ENVIRONMENT"
});
var app = builder.Build();
app.MapGet("/", () => "ok");
app.Run();

public partial class Program { }
