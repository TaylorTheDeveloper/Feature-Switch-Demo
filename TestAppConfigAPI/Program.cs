using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using Microsoft.Azure.AppConfiguration.AspNetCore;
using Azure.Core;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Pull App Configuration endpoint from settings
string endpoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")
    ?? throw new InvalidOperationException("The setting `Endpoints:AppConfiguration` was not found.");

TokenCredential cred;

// Use Azure CLI locally, Managed Identity in Azure
if (builder.Environment.IsDevelopment())
{
    cred = new AzureCliCredential();
}
else
{
    cred = new ManagedIdentityCredential();
    // In Azure App Service, make sure System Assigned Managed Identity is enabled
}

// Hook in Azure App Configuration + Feature Flags
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri(endpoint), cred)
           .Select("TestApp:*", LabelFilter.Null)
           .ConfigureRefresh(refreshOptions =>
               refreshOptions.RegisterAll())
           .UseFeatureFlags(); // load feature flags
});

// Register services
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();
builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseAzureAppConfiguration();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
