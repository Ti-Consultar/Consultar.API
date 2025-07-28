using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace _2___Application.Base
{
    public class BaseService : IDisposable
    {
        public IAppSettings _appSettings;
        public int _currentUserId;
     

        public BaseService(IAppSettings appSettings)
        {
            _appSettings = appSettings;
         
        }
        public static DateTime GetBrasilDateTime()
        {
            var timeZoneId = OperatingSystem.IsWindows()
                ? "E. South America Standard Time"
                : "America/Sao_Paulo";

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        }

        // Método para liberar recursos (se necessário)
        public void Dispose()
        {
            // Libere recursos não gerenciados aqui, se necessário.
        }
        public decimal ParseDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            // Remove espaços
            input = input.Trim();

            // Trata ponto como separador de milhar e vírgula como decimal
            if (Regex.IsMatch(input, @"\.\d{3},\d{2}$"))
            {
                input = input.Replace(".", "").Replace(",", ".");
            }
            // Trata vírgula como milhar e ponto como decimal (formato inverso)
            else if (Regex.IsMatch(input, @",\d{3}\.\d{2}$"))
            {
                input = input.Replace(",", "").Replace(".", ",");
            }
            else
            {
                // Caso seja só vírgula ou só ponto, tenta ajustar
                input = input.Replace(".", "").Replace(",", ".");
            }

            decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
            return result;
        }


        public decimal? SafeDivide(decimal numerator, decimal denominator)
        {
            if (denominator == 0) return null;
            return numerator / denominator * 100;
        }

        public bool ParseBoolean(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim().ToLower();

            return value switch
            {
                "true" or "1" or "sim" or "s" => true,
                _ => false
            };
        }


        public string GetEnvironmentName() =>
            _appSettings.GetHostingEnvironment()?.EnvironmentName ?? "dev";

        // Método atualizado para obter o ID do usuário autenticado via JWT
        public int GetCurrentUserId()
        {
            var http = _appSettings.GetHttpContext()?.HttpContext;
            var headerAuthorization = http?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(headerAuthorization) || !headerAuthorization.StartsWith("Bearer "))
                return 0;

            var token = headerAuthorization.Substring(7); // Remover "Bearer " do início
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "sub" || c.Type == "userId" || c.Type == "nameid");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    _currentUserId = userId;
                    return userId;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }
        
       

        private string GetRouterName()
        {
            var http = _appSettings.GetHttpContext()?.HttpContext;
            return http == null ? string.Empty : $"{http.Request.Host}-{http.Request.Method}-{http.Request.Path}";
        }

        private int GetTokenValidTotalMinutesTo()
        {
            var http = _appSettings.GetHttpContext()?.HttpContext;
            var headerAuthorization = http?.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(headerAuthorization)) return 0;

            var jwtToken = new JwtSecurityToken(jwtEncodedString: headerAuthorization.Substring(7));
            var tokenExp = jwtToken.Claims.FirstOrDefault(p => p.Type.ToLower() == "exp")?.Value;
            return int.TryParse(tokenExp, out var tokenExpMinutes) ? tokenExpMinutes : 0;
        }

        // Método comum para criar Response de sucesso
        protected ResultValue CreateSuccessResponse<T>(T data = default, string message = "", string name = "")
        {
            return new ResultValue<T>
            {
                Success = true,
                Data = data,
                Message = string.IsNullOrWhiteSpace(message) ? $"{name} {message}" : message,
                Router = GetRouterName(),
                Environment = GetEnvironmentName(),
                TokenValidTotalMinutesTo = GetTokenValidTotalMinutesTo()
            };
        }

        // Método para resposta de erro
        protected ResultValue CreateErrorResponse(string errorMessage, Exception ex = null)
        {
            var errorDetails = new List<string> { "INTERNAL_SERVER_ERROR" };

            if (!GetEnvironmentName().Equals("prod") && ex != null)
            {
                errorDetails.Add(ex.Message);
                errorDetails.Add(ex.StackTrace);
            }

            return new ResultValue
            {
                Success = false,
                ErrorMessage = errorDetails,
                Router = GetRouterName(),
                Environment = GetEnvironmentName(),
                TokenValidTotalMinutesTo = GetTokenValidTotalMinutesTo()
            };
        }

        #region Response Methods
        protected ResultValue SuccessResponse<T>(T data) => CreateSuccessResponse(data);

        
        protected ResultValue SuccessResponse() => CreateSuccessResponse<object>();
        protected ResultValue SuccessResponse(string name, string message) => CreateSuccessResponse<string>(null, message, name);
        protected ResultValue SuccessResponse<T>(T data, string name, string message) => CreateSuccessResponse(data, message, name);

        protected ResultValue ErrorResponse(Exception ex) => CreateErrorResponse("Internal server error", ex);
        protected ResultValue ErrorResponse(string message) => CreateErrorResponse(message);
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
