using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<IStoreBackendClient, StoreBackendClient>();
builder.Services.AddMemoryCache();
builder.Services.AddApplicationMonitoring();

// reconfigure code to make requests to Dapr sidecar
var baseURL = (Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost") + ":" + (Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500");
builder.Services.AddHttpClient("Products", (httpClient) =>
{
    httpClient.BaseAddress = new Uri(baseURL);
    httpClient.DefaultRequestHeaders.Add("dapr-app-id", "Products");
});

builder.Services.AddHttpClient("Inventory", (httpClient) =>
{
    httpClient.BaseAddress = new Uri(baseURL);
    httpClient.DefaultRequestHeaders.Add("dapr-app-id", "Inventory");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

public class Product
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
}

public interface IStoreBackendClient
{
    [Get("/products")]
    Task<List<Product>> GetProducts();

    [Get("/inventory/{productId}")]
    Task<int> GetInventory(string productId);
}

public class StoreBackendClient : IStoreBackendClient
{
    IHttpClientFactory _httpClientFactory;

    public StoreBackendClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<int> GetInventory(string productId)
    {
        var client = _httpClientFactory.CreateClient("Inventory");
        return await RestService.For<IStoreBackendClient>(client).GetInventory(productId);
    }

    public async Task<List<Product>> GetProducts()
    {
        var client = _httpClientFactory.CreateClient("Products");
        return await RestService.For<IStoreBackendClient>(client).GetProducts();
    }
}
