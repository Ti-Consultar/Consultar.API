# Visualizador de balancete V2

## Endpoint

`GET /api/v2/trial-balances/{trialBalanceId}/viewer/rows`

O endpoint usa a mesma autenticação JWT e os mesmos papéis do endpoint legado de
dados do balancete. Além disso, valida o vínculo do usuário em `CompanyUsers` para o
grupo, empresa ou filial do plano de contas. Balancetes inexistentes e fora desse
escopo retornam `404` sem revelar qual das duas situações ocorreu.

Parâmetros:

- `offset`: padrão `0`, mínimo `0`.
- `limit`: padrão `200`, mínimo `1`, máximo `500`.
- `search`: procura em `CostCenter` (conta) ou `Name` (descrição).
- `levels`: níveis de `1` a `5`, separados por vírgula; duplicados são ignorados.
- `onlyWithMovement`: considera `Debit != 0 || Credit != 0`.
- `sort`: `id`, `sourceOrder`, `accountCode`, `description`, `level`,
  `previousBalance`, `debit`, `credit` ou `finalBalance`.
- `direction`: `asc` ou `desc`.

O `total` é contado depois dos filtros e antes de `Skip`/`Take`. A consulta faz no
máximo uma ida ao banco para existência/acesso + contagem e outra para o bloco.

## Mapeamento do modelo legado

| Contrato V2 | Campo atual |
| --- | --- |
| `accountCode` | `BalanceteData.CostCenter` |
| `description` | `BalanceteData.Name` |
| `previousBalance` | `BalanceteData.InitialValue` |
| `debit` | `BalanceteData.Debit` |
| `credit` | `BalanceteData.Credit` |
| `finalBalance` | `BalanceteData.FinalValue` |
| `sourceOrder` | `BalanceteData.Id` |

O schema não possui `SourceOrder`, `Level` ou `HasChildren`. As importações montam
a lista na ordem do arquivo e a inserem nessa mesma ordem; por isso o `Id` identity
é hoje a ordem persistida e também o desempate único. `level` é calculado pelo número
de segmentos de `CostCenter`, e `hasChildren` por `EXISTS` de uma conta com o prefixo
`conta.` no mesmo balancete. Nenhum saldo é recalculado.

Foi incluído o índice `IX_BalanceteData_BalanceteId_Id`, cobrindo o recorte por
balancete e a ordem padrão. Para bancos existentes, aplique manualmente o script
`Database/Scripts/AddBalanceteDataViewerV2Index.sql`; nenhuma migration é executada
automaticamente.

## Linha de cabeçalho importada como dado

O visualizador não remove silenciosamente linhas pelo texto da descrição. A origem
desse problema é a configuração `StartRow` da importação dinâmica: ela define a
primeira linha lida, enquanto os importadores fixos já pulam o cabeçalho. Corrigir ou
reimportar a configuração/dado na origem é mais seguro do que uma heurística no GET,
que poderia ocultar uma conta legítima. Uma validação explícita do mapeamento durante
a importação é uma melhoria futura e ficou fora deste endpoint.

## AG Grid Community

Use `startRow` como `offset`, `endRow - startRow` como `limit`, o primeiro item de
`sortModel` como `sort`/`direction`, e chame `successCallback(items, pagination.total)`.
Esta primeira versão aceita apenas ordenação simples.
