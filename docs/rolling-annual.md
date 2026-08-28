# Rolling anual — DRE e Fluxo de Caixa

## Regra

Rolling é a projeção anual composta pelos valores realizados até o último período
realizado efetivamente disponível e pelos valores orçados disponíveis nos períodos
posteriores. A presença do período, e não o valor financeiro, define o corte; zero é
um valor válido.

O Orçado anual é a soma dos valores orçados disponíveis no exercício. A Variação
mantém a convenção atual dos relatórios: Rolling menos Orçado.

## DRE

Endpoint: `GET /api/v2/dre?accountPlanId={id}&year={ano}`.

`data.periods` mantém os meses e o `accumulated` e adiciona, ao final:

```json
{
  "key": "annual-rolling",
  "label": "2026",
  "year": 2026,
  "month": null,
  "type": "rolling",
  "displayOrder": 14,
  "columns": [
    { "key": "orcado", "label": "Orçado", "type": "budget", "displayOrder": 1 },
    { "key": "rolling", "label": "Rolling", "type": "rolling", "displayOrder": 2 },
    { "key": "variacao", "label": "Variação", "type": "variation", "displayOrder": 3 }
  ]
}
```

Cada linha expõe os valores anuais em
`values.orcado["annual-rolling"]`, `values.rolling["annual-rolling"]` e
`values.variacao["annual-rolling"]`. Totais e subtotais usam os resultados mensais
já calculados pela DRE; margens percentuais são recalculadas pela fórmula consolidada
da linha, sem somar percentuais mensais.

`rolling` não integra a lista global `data.scenarios` e não aparece nas colunas
mensais. O front deve montar cada agrupamento a partir de `periods[].columns`:
meses e acumulado usam `Orçado | Realizado | Variação`; somente o período
`annual-rolling` usa `Orçado | Rolling | Variação`.

## Fluxo de Caixa

Endpoints:

- `GET /rolling?accountPlanId={id}&year={ano}`;
- `GET /api/CashFlow/scope/rolling?groupId={id}&year={ano}` e filtros opcionais de empresa/filial.

A resposta preserva os painéis mensais existentes e adiciona `annual`:

```json
{
  "annual": {
    "year": 2026,
    "type": "rolling",
    "displayOrder": 14,
    "columns": [
      { "key": "orcado", "label": "Orçado", "type": "budget", "displayOrder": 1, "value": {} },
      { "key": "rolling", "label": "Rolling", "type": "rolling", "displayOrder": 2, "value": {} },
      { "key": "variacao", "label": "Variação", "type": "variation", "displayOrder": 3, "value": {} }
    ]
  }
}
```

Os movimentos são acumulados. `disponibilidadeInicioDoPeriodo` usa o primeiro
período selecionado e `disponibilidadeFinalDoPeriodo` usa o último, preservando a
natureza não aditiva desses saldos. O painel legado `rolling` contém o mesmo valor
anual para compatibilidade.
