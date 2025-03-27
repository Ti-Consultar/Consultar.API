using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _2___Application.Base;
using _4_InfraData._2_AppSettings;
using _2___Application._2_Dto_s.Permissions;


public class PermissionService : BaseService
{
    private readonly PermissionRepository _permissionRepository;
    private readonly UserRepository _userRepository;

    public PermissionService(PermissionRepository permissionRepository, IAppSettings appSettings)
        : base(appSettings)
    {
        _permissionRepository = permissionRepository;
    }


    #region Companies

    public async Task<ResultValue> GetById(int id)
    {
        try
        {
            var model = await _permissionRepository.GetById(id);
            if (model == null)
                return ErrorResponse(Message.NotFound);

            var response = new PermissionResponse
            {
                Id = model.Id,
                Name = model.Name,
            };

            return SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> GetAll()
    {
        try
        {
            var model = await _permissionRepository.GetAll();
            if (model == null)
                return ErrorResponse(Message.NotFound);

            var response = model.Select(permission => new PermissionResponse
            {
                Id = permission.Id,
                Name = permission.Name,
            }).ToList();

            return SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    #endregion


}