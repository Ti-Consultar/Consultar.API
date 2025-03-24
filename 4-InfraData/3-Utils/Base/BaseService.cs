using _4_InfraData._2_AppSettings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4_InfraData._3_Utils.Base
{
    public class BaseService : IDisposable
    {
        #region [ Fields ]
        public void Dispose()
        {

        }


        public IAppSettings _appSettings;
        public int _currentUserId;
        #endregion

        #region [ Constructor ]

        public BaseService(IAppSettings appSettings) //int currentUserId)
        {
            _appSettings = appSettings;
            // _currentUserId = currentUserId;
        }
        #endregion

        #region [ Public Methods ]
        public string GetEnvironmentName()
        {
            return _appSettings.GetHostingEnvironment() == null ? "dev" : _appSettings.GetHostingEnvironment().EnvironmentName;
        }
        public int GetCurrentUserId() => _currentUserId;
        #endregion

        #region [ Private Methods ]
        private string GetRouterName()
        {
            if (_appSettings.GetHttpContext()?.HttpContext == null) return "";
            var http = _appSettings.GetHttpContext().HttpContext;

            var routeTemplate = $"{http?.Request.Host}-{http?.Request.Method}-{http?.Request.Path}";

            if (string.IsNullOrWhiteSpace(routeTemplate))
                return "";

            return routeTemplate;
        }
        private int GetTokenValidTotalMinutesTo()
        {
            if (_appSettings.GetHttpContext()?.HttpContext == null) return 0;
            var http = _appSettings.GetHttpContext().HttpContext;
            if (http == null) return 0;

            var headerAuthorization = http.Request.Headers.FirstOrDefault(p => p.Key.Equals("Authorization")).Value.ToString();
            if (string.IsNullOrWhiteSpace(headerAuthorization)) return 0;

            var jwtToken = new JwtSecurityToken(jwtEncodedString: headerAuthorization.Substring(7));
            var tokenExp = jwtToken.Claims.FirstOrDefault(p => p.Type.ToLower().Equals("exp"))?.Value;
            if (string.IsNullOrWhiteSpace(tokenExp)) return 0;

            var tokenExpMinutes = int.Parse(tokenExp);

            return tokenExpMinutes;
        }
        #endregion

        #region [ Protected Methods ]

        protected ResultValue SuccessResponse<T>(T data)
        {
            return new ResultValue<T>
            {
                Data = data,
                Success = true,
                Router = GetRouterName(),
                Environment = GetEnvironmentName(),
                TokenValidTotalMinutesTo = GetTokenValidTotalMinutesTo()
            };
        }
        protected ResultValue SuccessResponse()
        {
            return new ResultValue
            {
                Success = true,
                Router = GetRouterName(),
                Environment = GetEnvironmentName(),
                TokenValidTotalMinutesTo = GetTokenValidTotalMinutesTo()
            };
        }
        protected ResultValue SuccessResponse(string name, string message)
        {
            return new ResultValue
            {
                Success = true,
                Message = $"{name} {message}",
                Router = GetRouterName(),
                Environment = GetEnvironmentName(),
                TokenValidTotalMinutesTo = GetTokenValidTotalMinutesTo()
            };
        }
        protected ResultValue SuccessResponse<T>(T data, string name, string message)
        {
            return new ResultValue<T>
            {
                Data = data,
                Success = true,
                Message = $"{name} {message}",
                Router = GetRouterName(),
                Environment = GetEnvironmentName(),
                TokenValidTotalMinutesTo = GetTokenValidTotalMinutesTo()
            };
        }
        protected ResultValue ErrorResponse(Exception ex)
        {


            var messageError = new List<string>() { "INTERNAL_SERVER_ERROR" };
            if (!GetEnvironmentName().Equals("prod"))
            {
                messageError.Add(ex.Message);
                messageError.Add(ex.StackTrace);
            }

            return new ResultValue
            {
                Success = false,
                Router = GetRouterName(),
                Environment = GetEnvironmentName(),
                ErrorMessage = messageError,
                TokenValidTotalMinutesTo = GetTokenValidTotalMinutesTo()
            };
        }
        protected ResultValue ErrorResponse(string message)
        {
            return new ResultValue
            {
                Success = false,
                Message = message,
                Router = GetRouterName(),
                Environment = GetEnvironmentName(),
                TokenValidTotalMinutesTo = GetTokenValidTotalMinutesTo(),
            };
        }
        #endregion
    }

    public class ResultValue
    {

        public bool Success { get; set; }


        public string Message { get; set; } = string.Empty;

        public DateTime Time { get; private set; } = DateTime.UtcNow;

        public string Environment { get; set; }

        public string Router { get; set; }

        public int TokenValidTotalMinutesTo { get; set; }

        public List<string> ErrorMessage { get; set; } = new List<string>();
    }
    public class ResultValue<T> : ResultValue
    {

        public T Data { get; set; }
    }
}
