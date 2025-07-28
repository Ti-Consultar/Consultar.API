
select * from Users;
GO

select * from Companies;
GO

select * from SubCompanies;
GO

select * from CompanyUsers;
GO

select * from InvitationToCompany;
GO

INSERT INTO Classification (Name, TypeOrder, TypeClassification) VALUES 
(N'Caixa e Equivalente de Caixa', 1, 1),
(N'Aplicação Financeira', 2, 1),
(N'Clientes', 3, 1),
(N'Estoques', 4, 1),
(N'Outros Ativos Operacionais', 5, 1),
(N'Ativo Não Circulante Financeiro', 6, 1),
(N'Ativo Não Circulante Operacional', 7, 1),
(N'Investimentos', 8, 1),
(N'Imobilizado', 9, 1),
(N'( - ) Depreciação Acumuladas', 10, 1),
(N'Intangível / Diferido', 11, 1),
(N'( - ) Amortização Acumuladas', 12, 1),
(N'Ativo Compensado', 13, 1),
(N'Contas Transitórias', 14, 1),
(N'Empréstimos e Financiamentos', 15, 2),
(N'Fornecedores', 16, 2),
(N'Obrigações Trabalhistas', 17, 2),
(N'Obrigações Tributárias', 18, 2),
(N'Outros Passivos Operacionais', 19, 2),
(N'Passivo Não Circulante Financeiro', 20, 2),
(N'Passivo Não Circulante Operacional', 21, 2),
(N'Capital Social', 22, 2),
(N'Reservas De Capital', 23, 2),
(N'Lucros Ou Prejuizos Acumulados', 24, 2),
(N'Resultado Do Exercício', 25, 2),
(N'Distribuição De Lucro', 26, 2),
(N'Passivo Compensado', 27, 2),
(N'Contas Transitórias Passivo', 28, 2),
(N'Apuração e Encerramento', 29, 2),
(N'Custo com Depreciação', 30, 2),
(N'Vendas de Produtos', 31, 3),
(N'Vendas de Mercadorias', 32, 3),
(N'Prestação de Serviço', 33, 3),
(N'(-) Devoluções de Vendas', 34, 3),
(N'(-) Abatimentos', 35, 3),
(N'(-) Impostos e Contribuições', 36, 3),
(N'(-) Custos das Mercadorias e Serviços', 37, 3),
(N'Despesas com Vendas', 38, 3),
(N'Despesas Com Pessoal e Encargos', 39, 3),
(N'Despesas Administrativas e Gerais', 40, 3),
(N'Outros Resultados Operacionais', 41, 3),
(N'Ganhos e Perdas de Capital', 42, 3),
(N'Outros Resultados não Operacionais', 43, 3),
(N'Receitas Financeiras', 44, 3),
(N'Despesas Financeiras', 45, 3),
(N'Provisão para CSLL', 46, 3),
(N'Provisão para IRPJ', 47, 3),
(N'Despesas com Depreciação', 48, 3),
(N'Custo com Depreciação', 49, 3);
GO

INSERT INTO Classification (Name, TypeOrder, TypeClassification) VALUES 
(N'Caixa e Equivalente de Caixa', 1, 1),
(N'Aplicação Financeira', 2, 1),
(N'Clientes', 3, 1),
(N'Estoques', 4, 1),
(N'Outros Ativos Operacionais', 5, 1),
(N'Ativo Não Circulante Financeiro', 6, 1),
(N'Ativo Não Circulante Operacional', 7, 1),
(N'Investimentos', 8, 1),
(N'Imobilizado', 9, 1),
(N'( - ) Depreciação Acumuladas', 10, 1),
(N'Intangível / Diferido', 11, 1),
(N'( - ) Amortização Acumuladas', 12, 1),
(N'Ativo Compensado', 13, 1),
(N'Contas Transitórias', 14, 1),
(N'Empréstimos e Financiamentos', 15, 2),
(N'Fornecedores', 16, 2),
(N'Obrigações Trabalhistas', 17, 2),
(N'Obrigações Tributárias', 18, 2),
(N'Outros Passivos Operacionais', 19, 2),
(N'Passivo Não Circulante Financeiro', 20, 2),
(N'Passivo Não Circulante Operacional', 21, 2),
(N'Capital Social', 22, 2),
(N'Reservas De Capital', 23, 2),
(N'Lucros Ou Prejuizos Acumulados', 24, 2),
(N'Resultado Do Exercício', 25, 2),
(N'Distribuição De Lucro', 26, 2),
(N'Passivo Compensado', 27, 2),
(N'Contas Transitórias Passivo', 28, 2),
(N'Apuração e Encerramento', 29, 2),
(N'Custo com Depreciação', 30, 2),
(N'Vendas de Produtos', 31, 3),
(N'Vendas de Mercadorias', 32, 3),
(N'Prestação de Serviço', 33, 3),
(N'(-) Devoluções de Vendas', 34, 3),
(N'(-) Abatimentos', 35, 3),
(N'(-) Impostos e Contribuições', 36, 3),
(N'(-) Custos das Mercadorias e Serviços', 37, 3),
(N'Despesas com Vendas', 38, 3),
(N'Despesas Com Pessoal e Encargos', 39, 3),
(N'Despesas Administrativas e Gerais', 40, 3),
(N'Outros Resultados Operacionais', 41, 3),
(N'Ganhos e Perdas de Capital', 42, 3),
(N'Outros Resultados não Operacionais', 43, 3),
(N'Receitas Financeiras', 44, 3),
(N'Despesas Financeiras', 45, 3),
(N'Provisão para CSLL', 46, 3),
(N'Provisão para IRPJ', 47, 3),
(N'Despesas com Depreciação', 48, 3),
(N'Custo com Depreciação', 49, 3);
GO

Select * from AccountPlans;
GO

select * from Classification;
GO

select * from Reclassification;
GO

Select * from AccountPlanClassification
GO
