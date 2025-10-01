using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace TestAppConfig.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeatureController : ControllerBase
    {
        private readonly IFeatureManagerSnapshot _features;

        public FeatureController(IFeatureManagerSnapshot features)
        {
            _features = features;
        }

        /// <summary>
        /// Returns all feature flags and whether they’re enabled.
        /// </summary>
        [HttpGet("GetFeatureSwitches")]
        public async Task<IActionResult> GetFeatureSwitches()
        {
            // Internal Flag Registry should link to work item in metadata for tracking
            var flags = new Dictionary<string, bool>
            {
                { "Beta", await _features.IsEnabledAsync("Beta") },
                { "Journal", await _features.IsEnabledAsync("Feature_Journal") } 
            };

            return Ok(flags);
        }

        /// <summary>
        /// Returns journal info depending on Beta + Journal flags.
        /// </summary>
        [HttpGet("GetJournalInfo")]
        public async Task<IActionResult> GetJournalInfo()
        {
            bool beta = await _features.IsEnabledAsync("Beta");
            bool journal = await _features.IsEnabledAsync("Feature_Journal");

            if (beta && journal)
            {
                return Ok("you’re enabled for journal data");
            }

            return Ok("you’re not enabled for journal");
        }

        /// <summary>
        /// Returns beta info depending on Beta flag.
        /// </summary>
        [HttpGet("GetBetaInfo")]
        public async Task<IActionResult> GetBetaInfo()
        {
            bool beta = await _features.IsEnabledAsync("Beta");

            return Ok(beta ? "you’re in beta" : "you’re not in beta");
        }
    }
}
