using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace _4_InfraData._2_AppSettings
{
    public class AppSettings : IAppSettings
    {
        #region [ Fields ]
        private readonly ILogger<AppSettings> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region [ Constructor ]
        public AppSettings(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AppSettings> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        #endregion

        #region [ Public Methods ]
        public IConfiguration GetConfiguration() => _configuration;
        public IHttpContextAccessor GetHttpContext() => _httpContextAccessor;
        public IWebHostEnvironment GetHostingEnvironment() => _environment;
        public void Dispose() { }
        #endregion
    }
}
