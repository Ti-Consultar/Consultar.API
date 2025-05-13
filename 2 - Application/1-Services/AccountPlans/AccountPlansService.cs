using _2___Application.Base;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services.AccountPlans
{
    public class AccountPlansService : BaseService
    {
        private readonly GroupRepository _groupRepository;
        private readonly CompanyRepository _companyRepository;
        private readonly UserRepository _userRepository;


        public AccountPlansService(
            GroupRepository groupRepository,
            CompanyRepository companyRepository,
            UserRepository userRepository,


            IAppSettings appSettings) : base(appSettings)
        {
            _groupRepository = groupRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;


        }
        #region Métodos



        #endregion


    }
}