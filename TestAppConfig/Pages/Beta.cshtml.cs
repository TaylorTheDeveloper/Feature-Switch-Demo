using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace TestAppConfig.Pages
{
    /// <summary>
    /// _features is the api object we would use in WebAPI and AzureFunction
    /// </summary>
    [FeatureGate("Beta")]
    public class BetaModel : PageModel
    {
        private readonly IFeatureManagerSnapshot _features;

        public bool WeatherCheckEnabled { get; private set; }
        public int? TemperatureF { get; private set; }

        public BetaModel(IFeatureManagerSnapshot features)
        {
            _features = features;
        }

        public async Task OnGet()
        {
            WeatherCheckEnabled = await _features.IsEnabledAsync("Feature_WeatherCheck");
            if (WeatherCheckEnabled)
            {
                // Random 40–99°F just for demo
                TemperatureF = Random.Shared.Next(40, 100);
            }
        }
    }
}
