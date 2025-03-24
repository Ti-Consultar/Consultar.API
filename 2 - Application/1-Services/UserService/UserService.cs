

using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using _2___Application._2_Dto_s.UserDto.Request;
using _2___Application._2_Dto_s.UserDto.Response;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_JWT;
using _4_InfraData._3_Utils.Base;
using _4_InfraData._3_Utils.Email;

namespace _2___Application._1_Services.User
{
    public class UserService
    {
        #region Construtor
        private readonly UserRepository _repository;
        private readonly CompanyRepository _companyRepository;
        private readonly EmailService _emailService;

        public UserService(UserRepository repository, EmailService emailService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _emailService = emailService;
        }

        #endregion

        #region Metodos
        public async Task<_2_Dto_s.UserDto.Response.LoginResponse> Login(LoginDto request)
        {
            var user = await _repository.Get(request.Email, request.Password.EncryptPassword());

            if (!IsUserValid(request, user))
            {
                return CreateUserResponseInvalid(request.Email);
            }

            return CreateUserResponseAuthorized(user);
        }

        public async Task<object> InsertUser(InsertDto request)
        {
            try
            {
                var userExists = await _repository.GetByEmail(request.Email);

                if (userExists != null)
                {
                    return UserLoginMessage.EmailExists;
                }

                var user = new UserModel(request.Name, request.Email, request.Password.EncryptPassword());
                await _repository.AddUser(user);

                return Message.Success;
            }
            catch (Exception ex)
            {
                return UserLoginMessage.Error + ex;
            }
        }

        public async Task<object> GetUser(int userId)
        {
            try
            {
                // Busca o usuário
                var user = await _repository.GetByUserId(userId);

                // Cria o objeto de resposta
                var response = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Companies = user?.CompanyUsers
                        .Select(a => new CompanyDto
                        {
                            Id = a.Company.Id,
                            Name = a.Company.Name,
                            DateCreate = a.Company.DateCreate
                            
                        }).ToList()
                };
                
                // Retorna o response com as informações do usuário e empresas
                return response;
            }
            catch (Exception ex)
            {
                return UserLoginMessage.Error + ex.Message; // Ajuste para mostrar a mensagem de erro
            }
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
        public async Task<ResetPasswordResponse> ResetPassword(int userId, UpdatePasswordDto request)
        {

            try

            {
                var user = await _repository.GetByEmail(request.Email);

                if (user == null && user.Id != userId)
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
                Email = user.Email,
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

        private bool IsUserValid(LoginDto request, UserModel user) => user != null && request.Email == user.Email && request.Password.EncryptPassword() == user.Password;

        private LoginResponse CreateUserResponseInvalid(string email)
        {
            return new LoginResponse
            {
                Email = email,
                Message = UserLoginMessage.InvalidCredentials
            };
        }

        #endregion


    }
}
