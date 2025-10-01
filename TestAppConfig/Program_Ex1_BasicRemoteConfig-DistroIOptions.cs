using Microsoft.Extensions.Configuration;
using Microsoft.Azure.AppConfiguration.AspNetCore;
using Azure.Identity;
using TestAppConfig;

var builder = WebApplication.CreateBuilder(args);


// Retrieve the endpoint
string endpoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")
    ?? throw new InvalidOperationException("The setting `Endpoints:AppConfiguration` was not found.");

//var cred = new DefaultAzureCredential(new DefaultAzureCredentialOptions
//{
//    // If you're using a *user-assigned* MI; remove this line for system-assigned
//    ManagedIdentityClientId = "db8032e1-f94f-42ca-86a6-4aa98b87d6f6"
//});

//var opts = new DefaultAzureCredentialOptions
//{
//    // flip some of these to true to isolate the problem
//    ExcludeEnvironmentCredential = false,
//    ExcludeVisualStudioCredential = false,
//    ExcludeSharedTokenCacheCredential = false,
//    ExcludeAzureCliCredential = false,
//    ExcludeInteractiveBrowserCredential = false,
//    ExcludeManagedIdentityCredential = false
//};

// Issue 1: Publix Web Proxy
// Issue 2: Getting this shit below to work was hard and I would love to use UAMI. Right now using my accout passthrough CLI. It is unclear why VS cred will not work.
var cred = new AzureCliCredential();// new VisualStudioCredential();// new ManagedIdentityCredential();// new DefaultAzureCredential();

// Example 1: Remote Configs - Service restart required.
// Load configuration from Azure App Configuration 
//builder.Configuration.AddAzureAppConfiguration(options =>
//{
//    options.Connect(new Uri(endpoint), cred);
//});

// Load configuration from Azure App Configuration 
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri(endpoint), cred);
});


// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<Settings>(builder.Configuration.GetSection("TestApp:Settings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
