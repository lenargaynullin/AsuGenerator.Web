using AsuGenerator.Web.Components;
using AsuGenerator.Web.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddScoped<AsuGenerator.Web.Services.ExcelParserService>();
builder.Services.AddScoped<AsuGenerator.Web.Services.CalculationEngine>();
builder.Services.AddScoped<AsuGenerator.Web.Services.DocumentGenerator>();
builder.Services.AddScoped<AsuGenerator.Web.Services.CadGeneratorService>();
builder.Services.AddSingleton<AsuGenerator.Web.Services.DxfBlockManager>();
builder.Services.AddScoped<PriceCalculationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
