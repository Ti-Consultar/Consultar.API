CREATE TABLE BusinessEntity (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NomeFantasia NVARCHAR(255) NULL,
    RazaoSocial NVARCHAR(255) NULL,
    Cnpj CHAR(14) NOT NULL, -- sem pontuação
    Logradouro NVARCHAR(255) NULL,
    Numero NVARCHAR(20) NULL,
    Bairro NVARCHAR(100) NULL,
    Municipio NVARCHAR(100) NULL,
    Uf CHAR(2) NULL,
    Cep CHAR(8) NULL,
    Telefone NVARCHAR(20) NULL,
    Email NVARCHAR(255) NULL
);