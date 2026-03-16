using _2___Application._2_Dto_s.ConfigPrincipal;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;

public class ConfigService : BaseService
{
    private readonly ConfigPrincipalRepository _configRepository;

    public ConfigService(
        ConfigPrincipalRepository configRepository,
        IAppSettings appSettings
    ) : base(appSettings)
    {
        _configRepository = configRepository;
    }

    /* =========================
       MENU
    ========================= */
    public async Task<ResultValue> CreateViewConfig(InsertViewConfigDto dto)
    {
        try
        {
            var viewConfig = new ViewConfig
            {
                AccountPlanId = dto.AccountPlanId,
                ConfigPrincipalId = dto.ConfigPrincipalId,
                SonConfigId = dto.SonConfigId
            };

            await _configRepository.AddViewConfig(viewConfig);

            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> GetConfigPrincipals()
    {
        try
        {
            var configs = await _configRepository.GetConfigPrincipals();

            return SuccessResponse(configs);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> GetMenuByAccountPlan(int accountPlanId)
    {
        try
        {
            var menu = await _configRepository.GetMenuOptimized(accountPlanId);

            if (menu == null || !menu.Any())
                return SuccessResponse(new List<MenuResponse>());

            return SuccessResponse(menu);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetMenuByAccountPlanId(int accountPlanId)
    {
        try
        {
            var menu = await _configRepository
                .GetMenuByAccountPlan(accountPlanId);

            return SuccessResponse(menu);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> ExistsViewConfig(int accountPlanId)
    {
        try
        {
            var exists = await _configRepository
                .ExistsViewConfigForAccountPlan(accountPlanId);

            return SuccessResponse(new
            {
                accountPlanId,
                exists
            });
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    /* =========================
       CONFIG PRINCIPAL
    ========================= */

    public async Task<ResultValue> GetConfigPrincipalTree(int id)
    {
        try
        {
            var config = await _configRepository.GetConfigPrincipalTree(id);

            if (config == null)
                return SuccessResponse(Message.NotFound);

            return SuccessResponse(config);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    /* =========================
       SON CONFIG
    ========================= */
  
    public async Task<ResultValue> GetSonConfigsByPrincipal(int configPrincipalId)
    {
        try
        {
            var sons = await _configRepository.GetSonConfigsByPrincipal(configPrincipalId);

            return SuccessResponse(sons);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetSonConfigById(int id)
    {
        try
        {
            var son = await _configRepository.GetSonConfigById(id);

            if (son == null)
                return SuccessResponse(Message.NotFound);

            return SuccessResponse(son);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    /* =========================
       VIEW CONFIG
    ========================= */

    public async Task<ResultValue> GetViewConfigsBySonConfig(int sonConfigId)
    {
        try
        {
            var views = await _configRepository.GetViewConfigsBySonConfig(sonConfigId);

            return SuccessResponse(views);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetViewConfigsByPrincipal(int configPrincipalId)
    {
        try
        {
            var views = await _configRepository.GetViewConfigsByPrincipal(configPrincipalId);

            return SuccessResponse(views);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetViewConfigById(int id)
    {
        try
        {
            var view = await _configRepository.GetViewConfigById(id);

            if (view == null)
                return SuccessResponse(Message.NotFound);

            return SuccessResponse(view);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
}