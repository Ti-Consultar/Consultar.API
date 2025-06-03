

namespace _2___Application.Base
{
    public class Message
    {

        public const string Success = "Inserido com Sucesso";
        public const string DeletedSuccess = "Removido com Sucesso";
        public const string RestoreSuccess = "Restaurado com Sucesso";
        public const string DeleteSuccess = "Removido com Sucesso";
        public const string MessageError = "Dados Inválidos";
        public const string NotFound = "Não encontrado";
        public const string Unauthorized = "Sem permissão para Realizar esta ação";
        public const string CNPJAlreadyRegistered = "Já existe um cadastro com este CNPJ.";
        public const string InvalidInvitationType = "Tipo de convite inválido ou incompleto.";
        public const string ExistsAccountPlans = "Já existe um plano de contas para esse nível";
        public const string ExistsInvitation = "Já existe um Convite";
        public const string RejectSucess = "Convite Rejeitado";

    }

    public static class UserLoginMessage
    {
        public const string Authorized = "Autorizado";
        public const string InvalidCredentials = "Login ou Senha Inválidos";
        public const string InvalidPassword = "As senhas não conferem";
        public const string EmailExists = "Já existe uma conta com este email cadastrado";
        public const string PasswordFailed = "Falha ao redefinir a senha. Por favor, tente novamente mais tarde.";
        public const string PasswordSuccess = "Senha Alterada com Sucesso!";
        public const string UserNotFound = "Usuário não encontrado!";
        public const string Error = "Erro!";
    }
}

