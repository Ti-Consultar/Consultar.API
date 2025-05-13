using _2___Application.Base;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System;

namespace _4_Application._1_Services
{
    public class BalanceteService : BaseService
    {
        private readonly BalanceteRepository _balanceteRepository;

        public BalanceteService(
            BalanceteRepository balanceteRepository,
            IAppSettings appSettings) : base(appSettings)
        {
            _balanceteRepository = balanceteRepository;
        }

        #region Métodos

        #endregion
    }
}
