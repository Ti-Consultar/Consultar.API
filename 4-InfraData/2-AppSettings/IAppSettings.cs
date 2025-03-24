using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace _4_InfraData._2_AppSettings
{
    public interface IAppSettings : IDisposable
    {

        IConfiguration GetConfiguration();
        IHttpContextAccessor GetHttpContext();
        IWebHostEnvironment GetHostingEnvironment();
    }
}
