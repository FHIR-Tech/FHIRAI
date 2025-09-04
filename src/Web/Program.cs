using FHIRAI.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();

// Configure Swagger/OpenAPI first, before static files
app.UseOpenApi();
app.UseSwaggerUi();

// Configure static files after Swagger
app.UseStaticFiles();

app.UseExceptionHandler(options => { });

// Redirect root to Swagger
app.Map("/", () => Results.Redirect("/swagger"));

app.MapEndpoints();

app.Run();

public partial class Program { }
