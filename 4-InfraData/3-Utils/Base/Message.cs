

namespace _4_InfraData._3_Utils.Base
{
    public class Message
    {

        public const string Success = "Inserido com Sucesso";
        public const string MessageError = "Dados Inválidos";
        public const string NotFound = "Não encontrado";

    }

    public static class UserLoginMessage
    {
        public const string Authorized = "Autorizado";
        public const string InvalidCredentials = "Login ou Senha Inválidos";
        public const string InvalidPassword = "As senhas não conferem";
        public const string EmailExists = "Já existe uma conta com este email cadastrado";
        public const string PasswordFailed = "Falha ao redefinir a senha. Por favor, tente novamente mais tarde.";
        public const string PasswordSuccess = "Senha Alterada com Sucesso!";
        public const string Error = "Erro!";
    }
}
