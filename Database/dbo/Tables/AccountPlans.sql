-- Tabela AccountPlans
CREATE TABLE AccountPlans (
    Id INT PRIMARY KEY IDENTITY,
    GroupId INT NOT NULL,
    CompanyId INT  NULL,
    SubCompanyId INT  NULL,
    SourceMode INT NOT NULL CONSTRAINT DF_AccountPlans_SourceMode DEFAULT 1,

	CONSTRAINT FK_AccountPlans_Group FOREIGN KEY (GroupId) REFERENCES Groups(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AccountPlans_Company FOREIGN KEY (CompanyId) REFERENCES Companies(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AccountPlans_SubCompany FOREIGN KEY (SubCompanyId) REFERENCES SubCompanies(Id) ON DELETE NO ACTION
);
