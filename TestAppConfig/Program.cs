using Microsoft.Extensions.Configuration;
using Microsoft.Azure.AppConfiguration.AspNetCore;
using Azure.Identity;
using TestAppConfig;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using Azure.Core;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string endpoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")
            ?? throw new InvalidOperationException("The setting `Endpoints:AppConfiguration` was not found.");

        // Issue 1: Publix Web Proxy
        // Issue 2: Getting this shit below to work was hard and I would love to use UAMI. Right now using my accout passthrough CLI. It is unclear why VS cred will not work.
        //var cred = new AzureCliCredential();// new VisualStudioCredential();// new ManagedIdentityCredential();// new DefaultAzureCredential();
        TokenCredential cred;

        if (builder.Environment.IsDevelopment())
        {
            // local dev
            cred = new AzureCliCredential();
        }
        else
        {
            // live site (App Service / VM with MSI)
            cred = new ManagedIdentityCredential();
            // After you setup MSI, you also need to configure:
            // Endpoints:AppConfiguration = "https://tempappconfigtaylor.azconfig.io"
        }

        // Example 1: Remote Configs - Service restart required.
        // Load configuration from Azure App Configuration 
        //builder.Configuration.AddAzureAppConfiguration(options =>
        //{
        //    options.Connect(new Uri(endpoint), cred);
        //});

        // Load configuration from Azure App Configuration 
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            options.Connect(new Uri(endpoint), cred)
                   // Load all keys that start with `TestApp:` and have no label.
                   .Select("TestApp:*", LabelFilter.Null)
                   // Reload configuration if any selected key-values have changed.
                   .ConfigureRefresh(refreshOptions =>
                       refreshOptions.RegisterAll()); // Note we can also register on sentinel

            // Load all feature flags with no label
            options.UseFeatureFlags();  
        });

        builder.Services.AddRazorPages();

        // Add Azure App Configuration middleware to the container of services.
        builder.Services.AddAzureAppConfiguration();
        // Add feature management to the container of services.
        builder.Services.AddFeatureManagement();

        builder.Services.Configure<Settings>(builder.Configuration.GetSection("TestApp:Settings"));

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }


        // Use Azure App Configuration middleware for dynamic configuration refresh.
        app.UseAzureAppConfiguration();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
        app.MapRazorPages();
        app.Run();
    }
}
