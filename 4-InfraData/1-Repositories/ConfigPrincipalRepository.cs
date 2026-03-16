using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;

namespace _4_InfraData._1_Repositories
{
    public class ConfigPrincipalRepository : GenericRepository<ConfigPrincipal>
    {
        private readonly CoreServiceDbContext _context;

        public ConfigPrincipalRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /* =========================
           MENU POR ACCOUNT PLAN
        ========================= */
        public async Task AddViewConfig(ViewConfig viewConfig)
        {
            await _context.ViewConfig.AddAsync(viewConfig);
            await _context.SaveChangesAsync();
        }
        public class MenuPrincipalDto
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public List<MenuSonDto> Sons { get; set; }
        }
        public class MenuSonDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
        public async Task<List<MenuPrincipalDto>> GetMenuByAccountPlanId(int accountPlanId)
        {
            var result = await _context.ViewConfig
                .Where(v => v.AccountPlanId == accountPlanId)
                .Include(v => v.ConfigPrincipal)
                .Include(v => v.SonConfig)
                .ToListAsync();

            var menu = result
                .GroupBy(v => new
                {
                    v.ConfigPrincipal.Id,
                    v.ConfigPrincipal.Name
                })
                .Select(g => new MenuPrincipalDto
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,

                    Sons = g
                        .Where(x => x.SonConfig != null)
                        .Select(x => new MenuSonDto
                        {
                            Id = x.SonConfig.Id,
                            Name = x.SonConfig.Name
                        })
                        .Distinct()
                        .ToList()
                })
                .ToList();

            return menu;
        }
        public async Task<bool> ExistsViewConfigForAccountPlan(int accountPlanId)
        {
            return await _context.ViewConfig
                .AnyAsync(x => x.AccountPlanId == accountPlanId);
        }
        public async Task<List<ConfigPrincipal>> GetMenuByAccountPlan(int accountPlanId)
        {
            return await _context.ConfigPrincipal
                .AsNoTracking()
                .Include(c => c.SonConfigs)
                    .ThenInclude(s => s.ViewConfigs)
                .Where(c => c.ViewConfigs
                    .Any(v => v.AccountPlanId == accountPlanId))
                .ToListAsync();
        }
        public async Task<List<ConfigPrincipal>> GetConfigPrincipals()
        {
            return await _context.ConfigPrincipal
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
        public async Task<List<SonConfig>> GetSonConfigsByPrincipal(int configPrincipalId)
        {
            return await _context.SonConfig
                .AsNoTracking()
                .Where(x => x.ConfigPrincipalId == configPrincipalId)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
        /* =========================
           CONFIG PRINCIPAL
        ========================= */

        public async Task<List<ConfigPrincipal>> GetByAccountPlanId(int accountPlanId)
        {
            return await _context.ConfigPrincipal
                .AsNoTracking()
                .Where(c => c.ViewConfigs
                    .Any(v => v.AccountPlanId == accountPlanId))
                .ToListAsync();
        }

        public class MenuResponse
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public List<MenuSonResponse> Sons { get; set; }
        }
        public class MenuSonResponse
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
        public async Task<List<MenuResponse>> GetMenuOptimized(int accountPlanId)
        {
            return await _context.ConfigPrincipal
                .AsNoTracking()
                .Where(c => c.ViewConfigs
                    .Any(v => v.AccountPlanId == accountPlanId))
                .Select(c => new MenuResponse
                {
                    Id = c.Id,
                    Name = c.Name,

                    Sons = c.SonConfigs
                        .Where(s => s.ViewConfigs
                            .Any(v => v.AccountPlanId == accountPlanId))
                        .Select(s => new MenuSonResponse
                        {
                            Id = s.Id,
                            Name = s.Name
                        })
                        .ToList()
                })
                .ToListAsync();
        }
        public async Task<List<ConfigPrincipal>> GetConfigPrincipalTree()
        {
            var data = await _context.ConfigPrincipal
      .AsNoTracking()
      .Include(c => c.SonConfigs)
          .ThenInclude(s => s.ViewConfigs)
      .OrderBy(c => c.Id)
      .ToListAsync();

            foreach (var item in data)
            {
                item.SonConfigs = item.SonConfigs
                    .OrderBy(s => s.Id)
                    .ToList();
            }

            return data;
        }

        public async Task<List<ConfigPrincipal>> GetFullTreeByAccountPlan(int accountPlanId)
        {
            return await _context.ConfigPrincipal
                .AsNoTracking()
                .Include(c => c.SonConfigs)
                    .ThenInclude(s => s.ViewConfigs)
                .Where(c => c.ViewConfigs
                    .Any(v => v.AccountPlanId == accountPlanId))
                .ToListAsync();
        }

        /* =========================
           SON CONFIG
        ========================= */

      

        public async Task<SonConfig> GetSonConfigById(int id)
        {
            return await _context.SonConfig
                .AsNoTracking()
                .Include(x => x.ViewConfigs)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        /* =========================
           VIEW CONFIG
        ========================= */

        public async Task<List<ViewConfig>> GetViewConfigsBySonConfig(int sonConfigId)
        {
            return await _context.ViewConfig
                .AsNoTracking()
                .Where(x => x.SonConfigId == sonConfigId)
                .ToListAsync();
        }

        public async Task<List<ViewConfig>> GetViewConfigsByPrincipal(int configPrincipalId)
        {
            return await _context.ViewConfig
                .AsNoTracking()
                .Where(x => x.ConfigPrincipalId == configPrincipalId)
                .ToListAsync();
        }

        public async Task<List<ViewConfig>> GetViewConfigsByAccountPlan(int accountPlanId)
        {
            return await _context.ViewConfig
                .AsNoTracking()
                .Where(x => x.AccountPlanId == accountPlanId)
                .ToListAsync();
        }

        public async Task<ViewConfig> GetViewConfigById(int id)
        {
            return await _context.ViewConfig
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}