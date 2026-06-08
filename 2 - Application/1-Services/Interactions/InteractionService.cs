using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.Parameter;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System;

namespace _4_Application._1_Services
{
    public class InteractionService : BaseService
    {
        private readonly InteractionRepository _interactionRepository;

        public InteractionService(
            InteractionRepository interactionRepository,
            IAppSettings appSettings) : base(appSettings)
        {
            _interactionRepository = interactionRepository;
        }

        #region Métodos
       
        #endregion
    }
}
