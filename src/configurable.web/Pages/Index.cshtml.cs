using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace configurable.web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public Dictionary<string, List<string>> ConfigItemsBySource { get; set; }

        public IndexModel(IConfiguration config, ILogger<IndexModel> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void OnGet()
        {
            var root = _config as IConfigurationRoot;
            var raw = root.GetDebugView();
            _logger.LogDebug(raw);
            var entries = raw.Split(Environment.NewLine);
            var itemsBySource = new Dictionary<string, List<string>>()
            {
                { "env", new List<string>() }
            };
            var nest = string.Empty;
            foreach (var entry in entries)
            {
                if (entry.Trim().EndsWith(":"))
                {
                    // e.g. Logging:
                    nest += entry.Trim();
                    continue;
                }
                else if (!entry.StartsWith(" "))
                {
                    // preserve nesting for e.g. 
                    //   Defaut = Information
                    nest = string.Empty;
                }
                if (entry.Contains(Providers.Env))
                {
                    // e.g. ALLUSERSPROFILE=C:\ProgramData (EnvironmentVariablesConfigurationProvider)
                    var item = entry.Substring(0, entry.IndexOf(Providers.Env) - 2).Trim();
                    itemsBySource["env"].Add(nest + item);
                }
                else if (entry.Contains(Providers.Json))
                {
                    // e.g. AllowedHosts=* (JsonConfigurationProvider for 'appsettings.json' (Optional))
                    var sourceStart = entry.IndexOf("for '") + 5;
                    var sourceEnd = entry.LastIndexOf("'");
                    var source = entry.Substring(sourceStart, sourceEnd - sourceStart);
                    if (!itemsBySource.ContainsKey(source))
                    {
                        itemsBySource.Add(source, new List<string>());
                    }
                    var item = entry.Substring(0, entry.IndexOf(Providers.Json) - 2).Trim();
                    itemsBySource[source].Add(nest + item);
                }
            }
            ConfigItemsBySource = itemsBySource;
        }

        private struct Providers
        {
            public const string Env = "EnvironmentVariablesConfigurationProvider";
            public const string Json = "JsonConfigurationProvider";
        }
    }
}
