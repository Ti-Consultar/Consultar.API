-- Tabela AccountPlans
CREATE TABLE AccountPlans (
    Id INT PRIMARY KEY IDENTITY,
    GroupId INT NOT NULL,
    CompanyId INT  NULL,
    SubCompanyId INT  NULL,

	CONSTRAINT FK_AccountPlans_Group FOREIGN KEY (GroupId) REFERENCES Groups(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AccountPlans_Company FOREIGN KEY (CompanyId) REFERENCES Companies(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AccountPlans_SubCompany FOREIGN KEY (SubCompanyId) REFERENCES SubCompanies(Id) ON DELETE NO ACTION
);