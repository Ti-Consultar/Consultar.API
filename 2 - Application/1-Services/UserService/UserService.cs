

using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using _2___Application._2_Dto_s.UserDto.Request;
using _2___Application._2_Dto_s.UserDto.Response;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_JWT;
using _2___Application.Base;
using _4_InfraData._3_Utils.Email;
using _2___Application._2_Dto_s.Group;
using _3_Domain._2_Enum_s;
using _4_InfraData._5_ConfigEnum;
using _4_InfraData._2_AppSettings;

namespace _2___Application._1_Services.User
{
    public class UserService : BaseService
    {
        #region Construtor
        private readonly UserRepository _repository;
        private readonly CompanyRepository _companyRepository;
        private readonly EmailService _emailService;
        public int _currentUserId;

        public UserService(UserRepository repository, EmailService emailService, IAppSettings appSettings) : base(appSettings)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _emailService = emailService;
            _currentUserId = GetCurrentUserId();

        }

        #endregion

        #region Metodos
        public async Task<_2_Dto_s.UserDto.Response.LoginResponse> Login(LoginDto request)
        {
            var user = await _repository.Get(request.Email.ToLower(), request.Password.EncryptPassword());

            if (!IsUserValid(request, user))
            {
                return CreateUserResponseInvalid(request.Email.ToLower());
            }

            return CreateUserResponseAuthorized(user);
        }
        public async Task<object> InsertUser(InsertDto request)
        {
            try
            {
                var userExists = await _repository.GetByEmail(request.Email.ToLower());
                if (userExists != null)
                    return UserLoginMessage.EmailExists;

                var newPassword = GenerateNewPassword();

                var user = new UserModel(
                    request.Name,
                    request.Email.ToLower(),
                    request?.Contact,
                    request?.Role,
                    newPassword.EncryptPassword()
                );


                await _repository.AddUser(user);

                await _emailService.SendUserWelcomeAsync(request.Email, request.Name, newPassword);

                return Message.Success;
            }
            catch (Exception ex)
            {
                return UserLoginMessage.Error + ex;
            }
        }

        public async Task<object> InsertSimpleUser(InsertSimpleDto request)
        {
            try
            {
                var userExists = await _repository.GetByEmail(request.Email.ToLower());
                if (userExists != null)
                    return UserLoginMessage.EmailExists;

                var newPassword = GenerateNewPassword();

                var user = new UserModel(
                    request.Name,
                    request.Email.ToLower(),
                    request?.Contact,
                    request?.Role,
                    request.Senha.EncryptPassword()
                );

      
                await _repository.AddUser(user);

                return Message.Success;
            }
            catch (Exception ex)
            {
                return UserLoginMessage.Error + ex;
            }
        }


        public async Task<object> UpdateUser(UpdateUser request)
        {
            try
            {
                var user = await GetCurrentUserAsync();


                if (user == null)
                    return Message.NotFound;

                // Verifica se o e-mail está sendo alterado para um que já existe em outro usuário
                if (user.Email.ToLower() != request.Email.ToLower())
                {
                    var emailExists = await _repository.GetByEmail(request.Email.ToLower());
                    if (emailExists != null && emailExists.Id != user.Id)
                        return UserLoginMessage.EmailExists;
                }

                // Atualiza os campos
                user.Name = request.Name;
                user.Email = request.Email.ToLower();
                user.Contact = request.Contact;

                await _repository.UpdateUser(user);
                return Message.Success;
            }
            catch (Exception ex)
            {
                return UserLoginMessage.Error + ex;
            }
        }


        public async Task<object> UpdateRoleUser(UpdateUserByGestor request)
        {
            try
            {
                var user = await GetCurrentUserAsync();


                if (user.Role != ERole.Gestor.ToString())
                    return Message.NotFound;

               

                var userToUpdate = await _repository.GetById(request.UserId);

                // Atualiza os campos
                userToUpdate.Role = request.Role;

                await _repository.UpdateUser(userToUpdate);
                return Message.Success;
            }
            catch (Exception ex)
            {
                return UserLoginMessage.Error + ex;
            }
        }

        public async Task<object> GetUserByEmailOrContact(string find)
        {
            var user = await _repository.GetUserByEmailOrContactAsync(find);


            var response =  new UserSimpleResponse
            {
                Id = user.Id,
                Email = user.Email.ToLower(),
                Name = user.Name,
                Contact = user.Contact,
                Role = user.Role
                
            };

            return response;
        }


        public async Task<object> GetUser()
        {
            try
            {

                // Busca o usuário no banco, garantindo que está trazendo as empresas e subempresas
                var user = await GetCurrentUserAsync();

                // Cria o objeto de resposta
                var response = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email.ToLower(),
                    Name = user.Name,
                    Contact = user.Contact,
                    Role = user.Role,
                    Groups = user?.CompanyUsers
                        .Select(a => new GroupDto
                        {
                            Id = a.Group.Id,
                            Name = a.Group.Name,
                            DateCreate = a.Group.DateCreate,
                            
                            // Adiciona a lista de subempresas
                            Companies = a.Group.Companies.Select(company => new CompanyDto
                            {
                                Id = company.Id,
                                Name = company.Name,
                                DateCreate = company.DateCreate,
                                SubCompanies = company.SubCompanies.Select(subCompany => new SubCompanyDto
                                {
                                    Id = subCompany.Id,
                                    Name = subCompany.Name,
                                    DateCreate = subCompany.DateCreate
                                }).ToList()
                            }).ToList()
                        }).ToList()
                };

                return response;
            }
            catch (Exception ex)
            {
                return UserLoginMessage.Error + ex.Message; // Retorna a mensagem de erro
            }
        }

        public async Task<object> GetAllUsers()
        {
             var users = await _repository.GetUsers();

            
                var response = users.Select(user => new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email.ToLower(),
                    Name = user.Name,
                    Contact = user.Contact
                }).ToList();

                return response;
        }
        public async Task<object> GetById()
        {
            var user = await GetCurrentUserAsync();

            var model = await _repository.GetById(user.Id);


            var response = new UserSimpleResponse
            {
                Id = model.Id,
                Email = model.Email.ToLower(),
                Name = model.Name,
                Contact= model.Contact,
                Role = model.Role
            };

            return response;
        }
        #endregion

        public async Task<ResetPasswordResponse> RedefinePassword(ResetPasswordRequest request)
        {
            var user = await _repository.GetByEmail(request.Email);

            if (user == null)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = Message.NotFound
                };
            }

            try
            {
                var newPassword = GenerateNewPassword();
                user.Password = newPassword.EncryptPassword();

                await _repository.ResetPassword(user);

                await _emailService.SendPasswordResetEmailAsync(user.Email, newPassword);

                return new ResetPasswordResponse
                {
                    Success = true,
                    Message = "Uma nova senha foi enviada para o seu email."
                };
            }
            catch (Exception ex)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = $"Falha ao redefinir a senha. Por favor, tente novamente mais tarde + {ex}."
                };
            }
        }
        public async Task<ResetPasswordResponse> ResetPassword( UpdatePasswordDto request)
        {

            try

            {
                var user = await GetCurrentUserAsync();

                if (user == null && user.Id != user.Id)
                {
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = Message.NotFound
                    };
                }
                user.Password = request.Password.EncryptPassword();

                await _repository.ResetPassword(user);



                return new ResetPasswordResponse
                {
                    Success = true,
                    Message = UserLoginMessage.PasswordSuccess
                };
            }
            catch (Exception ex)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = UserLoginMessage.PasswordFailed + ex.Message
                };
            }
        }


        #region Metodos Privados
        private LoginResponse CreateUserResponseAuthorized(UserModel user)
        {
            var token = TokenService.GenerateToken(user);

            return new LoginResponse
            {
                Email = user.Email.ToLower(),
                Token = token,
                Role = user.Role,
                Message = UserLoginMessage.Authorized
            };
        }

        private string GenerateNewPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var newPassword = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return newPassword;
        }

        private bool IsUserValid(LoginDto request, UserModel user) => user != null && request.Email.ToLower() == user.Email.ToLower() && request.Password.EncryptPassword() == user.Password;

        private LoginResponse CreateUserResponseInvalid(string email)
        {
            return new LoginResponse
            {
                Email = email.ToLower(),
                Message = UserLoginMessage.InvalidCredentials
            };
        }
        private async Task<UserModel> GetCurrentUserAsync()
        {
            var user = await _repository.GetByUserId(_currentUserId);
            if (user == null)
                throw new UnauthorizedAccessException(UserLoginMessage.InvalidCredentials);

            return user;
        }
        #endregion


    }
}
