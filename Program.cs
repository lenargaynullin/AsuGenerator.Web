using AsuGenerator.Web.Components;
using AsuGenerator.Web.Models;
using AsuGenerator.Web.Services;
using AsuGenerator.Web.Services.Strategies;
using AsuGenerator.Web.Services.Strategies.Implementations;
using MudBlazor.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Оптимизация интерактивных компонентов SignalR под плохую и нестабильную мобильную связь
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); // Оставляем вызов чистым, как было в вашей рабочей версии

// ИСПРАВЛЕНО: Настраиваем таймауты удержания сессии при обрывах связи напрямую через HubOptions
builder.Services.Configure<Microsoft.AspNetCore.SignalR.HubOptions>(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // Ждать ответа от мобильного телефона 60 секунд вместо 30
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);    // Проверять канал каждые 15 секунд для удержания сессии
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);     // Увеличить время первичного рукопожатия при слабом сигнале 3G/E
    options.MaximumReceiveMessageSize = 1024 * 1024;         // Лимит пакета в 1 МБ для защиты от разрывов при передаче JSON
});


builder.Services.AddScoped<EquipmentDatabase>();
builder.Services.AddScoped<PowerCabinetCalculator>();
builder.Services.AddScoped<PowerCabinetCatalog>();
builder.Services.AddScoped<AsuGenerator.Web.Services.EquipmentDatabase>();
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
builder.Services.AddScoped<AsuGenerator.Web.Services.RegulCalculationService>();
builder.Services.AddScoped<AsuGenerator.Web.Services.PlcComparisonEngine>();
builder.Services.AddScoped<UniversalCalculationService>();
builder.Services.AddSingleton(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    string jsonPath = Path.Combine(env.WebRootPath, "Configs", "plc-base.json");
    string jsonText = File.ReadAllText(jsonPath);
    return JsonSerializer.Deserialize<PlcBaseRoot>(jsonText) ?? new PlcBaseRoot();
});

// Каталог IEK и движок расчёта конфигуратора силовых шкафов (ВРУ, ЩС, ЩО)
builder.Services.AddSingleton(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    return new PowerCabinetCatalogLoader().Load(env);
});
builder.Services.AddScoped<PowerCabinetCalculator>();

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
