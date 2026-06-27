using AsuGenerator.Web.Components;
using AsuGenerator.Web.Services;
using AsuGenerator.Web.Services.Strategies.Implementations;
using AsuGenerator.Web.Services.Strategies;
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
builder.Services.AddScoped<CabinetStrategyFactory>();
builder.Services.AddSingleton<ShuvConfigLoader>();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<UniversalCalculationEngine>();
builder.Services.AddScoped<PlcCalculationService>();
// ИСПРАВЛЕНО: Регистрируем сервис авторасчета крейтов и барьеров REGUL R500 в контейнере зависимостей
builder.Services.AddScoped<AsuGenerator.Web.Services.RegulCalculationService>();


// Регистрация b2b фабрики управления шкафами

// Регистрация стратегий конкретных шкафов
builder.Services.AddScoped<ICabinetStrategy, ShuvStrategy>();
builder.Services.AddScoped<ICabinetStrategy, ShueStrategy>();

// Когда добавите ШУН (насосы) — просто допишете ниже:
// builder.Services.AddScoped<ICabinetStrategy, ShunStrategy>();

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
