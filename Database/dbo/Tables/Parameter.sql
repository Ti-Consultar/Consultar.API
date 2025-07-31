CREATE TABLE Parameter
(
    Id INT IDENTITY (1, 1) PRIMARY KEY NOT NULL,
    AccountPlansId INT NOT NULL,
    ParameterValue Decimal(18, 2) NULL,
    Name NVARCHAR(200) NOT NULL,
    ParameterYear INT NOT NULL,
   
    
    CONSTRAINT FK_Parameter_Accountplan 
        FOREIGN KEY (AccountPlansId) REFERENCES AccountPlans(Id) ON DELETE CASCADE,

);