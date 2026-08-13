# Ponto de Equilíbrio V2

## Visão geral

O Ponto de Equilíbrio V2 é um cálculo mensal, stateless e somente leitura. A fonte dos
valores monetários é a DRE realizada existente. O cliente informa apenas escopo, período,
fator e percentuais de simulação; nenhuma simulação altera balancetes, classificações ou a
DRE persistida.

Fluxo do backend:

`escopo autorizado -> DRE realizada mensal -> simulações -> projetado -> margem de contribuição -> PE base -> fator -> DRE no cenário PE`

O plano de contas é resolvido internamente a partir de `groupId`, `companyId?` e
`subCompanyId?`. Ele não é aceito como substituto do escopo financeiro.

## Endpoints

### GET `/api/v2/break-even`

Parâmetros:

- `groupId` (obrigatório)
- `companyId` (opcional)
- `subCompanyId` (opcional; exige `companyId`)
- `year` (obrigatório)
- `month` (obrigatório, de 1 a 12)

Equivale a fator zero e nenhuma simulação.

Exemplo:

```http
GET /api/v2/break-even?groupId=10&companyId=20&year=2026&month=8
Authorization: Bearer <token>
```

### POST `/api/v2/break-even/simulate`

Percentuais usam sempre fração decimal: `0.10` representa 10% e `-0.05` representa -5%.

```json
{
  "groupId": 10,
  "companyId": 20,
  "subCompanyId": null,
  "year": 2026,
  "month": 8,
  "factor": 0.10,
  "simulations": [
    {
      "rowCode": "SALES_EXPENSES",
      "percentage": -0.05
    }
  ]
}
```

O `rowCode` é a identidade estável devolvida em `rows[].code`. O backend define
`canSimulate`; totalizadores, receitas brutas e quaisquer linhas não autorizadas pelo
catálogo do PE são rejeitados.

`simulationSignRule` orienta a apresentação no frontend: `negative` para despesas,
`free` para linhas neutras que podem assumir valores positivos ou negativos e `null`
para linhas não simuláveis. As linhas livres são Outros Resultados Operacionais, Outras
Receitas e Ganhos, Ganhos e Perdas de Capital e Receitas Financeiras.

Resposta resumida:

```json
{
  "data": {
    "scope": {
      "groupId": 10,
      "companyId": 20,
      "subCompanyId": null
    },
    "period": {
      "year": 2026,
      "month": 8
    },
    "factor": 0.10,
    "summary": {
      "projectedGrossOperatingRevenue": 18000.00,
      "contributionMargin": 1600.00,
      "contributionMarginPercentage": 0.0888888888888889,
      "fixedResult": -1794.00,
      "fixedAmountToCover": 1794.00,
      "baseBreakEven": 20182.50,
      "finalBreakEven": 22200.75
    },
    "rows": [
      {
        "code": "SALES_EXPENSES",
        "name": "Despesas com Vendas",
        "displayOrder": 210,
        "level": 1,
        "parentCode": "OPERATING_EXPENSES",
        "hasChildren": false,
        "isTotalizer": false,
        "canSimulate": true,
        "simulationPercentage": -0.05,
        "simulationSignRule": "negative",
        "behavior": "FIXED",
        "baseValue": -80.00,
        "projectedValue": -76.00,
        "breakEvenValue": -76.00
      }
    ]
  }
}
```

## Regras de cálculo

- `projectedValue = baseValue * (1 + percentage)`.
- A receita e as linhas variáveis do PE preservam sua proporção sobre a Receita
  Operacional Bruta projetada.
- Linhas fixas preservam o valor projetado no cenário PE.
- Totalizadores e margens são reconstruídos a partir das dependências.
- `contributionMarginPercentage = contributionMargin / projectedGrossOperatingRevenue`.
- `fixedAmountToCover = ABS(fixedResult)`; o absoluto é aplicado apenas depois da soma.
- `baseBreakEven = fixedAmountToCover / contributionMarginPercentage`.
- `finalBreakEven = baseBreakEven * (1 + factor)`.
- Com fator zero, o Resultado Antes dos Impostos do cenário PE tende a zero. Com fator
  positivo, o resultado passa a ser superior ao equilíbrio.

Os cálculos usam `decimal` e não arredondam valores intermediários. Arredondamento e
formatação monetária ficam a cargo da apresentação.

## Validações e erros

- autenticação ausente ou usuário inválido: `401`;
- usuário sem acesso ao escopo: `403`;
- plano de contas ou DRE mensal inexistente: `404`;
- mês fora de 1 a 12, IDs de escopo inválidos, fator negativo, linha duplicada,
  inexistente ou não simulável, e percentual inferior a -100%: `400`;
- Receita Operacional Bruta zero ou Margem de Contribuição não positiva: `400`.

Números JSON inválidos, inclusive valores não representáveis por `decimal`, são rejeitados
pela desserialização do ASP.NET Core antes do cálculo.

## Compatibilidade com a DRE atual

Os valores base monetários e IDs internos vêm da DRE V2. Duas diferenças são deliberadas e
restritas ao cenário analítico do PE:

1. A DRE atual apresenta a Margem de Contribuição percentual sobre a Receita Líquida; o PE
   usa Receita Operacional Bruta, conforme a especificação desta funcionalidade.
2. `Outros Resultados Operacionais` e `Ganhos e Perdas de Capital` são reposicionados
   conceitualmente nas dependências do PE. A DRE existente não é modificada.

A DRE V2 também apresenta hoje o código estável `COST_OF_SERVICES` com o rótulo
`Custos Operacionais`, embora sua origem seja a classificação de custos dos serviços
prestados. O PE conserva o contrato atual e usa o código/ID da origem, não o texto, para o
mapeamento financeiro.

A classificação existente `(-) Custos Variáveis`, quando disponível, permanece variável e
é incorporada ao Lucro Bruto como já ocorre na DRE atual, mas não é simulável. A classificação
extra `Outras Receitas não Operacionais` permanece fixa e integra o resultado fixo para que o
cenário PE reconcilie com a DRE vigente.
