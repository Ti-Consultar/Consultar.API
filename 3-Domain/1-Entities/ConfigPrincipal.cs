using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class ConfigPrincipal
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // Navigation
        public ICollection<SonConfig> SonConfigs { get; set; }

        public ICollection<ViewConfig> ViewConfigs { get; set; }
    }
    public class SonConfig
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ConfigPrincipalId { get; set; }

        [JsonIgnore]
        public ConfigPrincipal ConfigPrincipal { get; set; }

        public ICollection<ViewConfig> ViewConfigs { get; set; } = new List<ViewConfig>();
    }
    public class ViewConfig
    {
        public int Id { get; set; }

        public int? AccountPlanId { get; set; }

        public int? ConfigPrincipalId { get; set; }

        public int? SonConfigId { get; set; }

        public AccountPlansModel AccountPlan { get; set; }

        [JsonIgnore]
        public ConfigPrincipal ConfigPrincipal { get; set; }

        [JsonIgnore]
        public SonConfig SonConfig { get; set; }
    }
}
