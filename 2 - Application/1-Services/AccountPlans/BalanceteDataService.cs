using _2___Application.Base;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System;

namespace _4_Application._1_Services
{
    public class BalanceteDataService : BaseService
    {
        private readonly BalanceteDataRepository _balanceteDataRepository;

        public BalanceteDataService(
            BalanceteDataRepository balanceteDataRepository,
            IAppSettings appSettings) : base(appSettings)
        {
            _balanceteDataRepository = balanceteDataRepository;
        }

        #region Métodos

        #endregion
    }
}
