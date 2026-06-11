using OrderProcessing.Api.Endpoints;
using OrderProcessing.Api.Services;
using OrderProcessing.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

// Adăugăm servicii pentru documentația API (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repozitoriu In-Memory unic
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

// CONSTRUIREA FLUENTĂ A LÂNȚULUI DE VALIDARE (Chain of Responsibility)
builder.Services.AddSingleton<IOrderValidationHandler>(_ =>
{
    var stock = new StockValidationHandler();
    var price = new PriceValidationHandler();
    var fraud = new FraudDetectionHandler();
    var age = new AgeVerificationHandler();

    // Structura: Stock -> Price -> Fraud -> Age
    stock.SetNext(price).SetNext(fraud).SetNext(age);
    return stock;
});

// Înregistrăm orchestratorul
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

// Activăm Swagger în modul de dezvoltare
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Activăm servirea fișierelor statice din wwwroot (HTML, CSS, JS)
app.UseDefaultFiles();
app.UseStaticFiles();

// Mapăm endpoint-urile Minimal API
app.MapOrderEndpoints();

// Dacă userul dă refresh pe o rută necunoscută, trimite-l înapoi la aplicația SPA
app.MapFallbackToFile("index.html");

app.Run();