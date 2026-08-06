using _2___Application._2_Dto_s.AccountPlan.Balancete.V2;
using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;

namespace _2___Application._1_Services.AccountPlans.Balancete.V2;

public sealed class TrialBalanceViewerNotFoundException : KeyNotFoundException
{
    public TrialBalanceViewerNotFoundException(int trialBalanceId)
        : base($"Balancete {trialBalanceId} não encontrado.")
    {
    }
}

public sealed class TrialBalanceViewerV2Service
{
    public const int DefaultLimit = 200;
    public const int MaximumLimit = 500;

    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id",
        "sourceOrder",
        "accountCode",
        "description",
        "level",
        "previousBalance",
        "debit",
        "credit",
        "finalBalance"
    };

    private readonly CoreServiceDbContext _context;

    public TrialBalanceViewerV2Service(CoreServiceDbContext context)
    {
        _context = context;
    }

    public async Task<TrialBalanceViewerRowsResponse> GetRowsAsync(
        int trialBalanceId,
        int userId,
        TrialBalanceViewerRowsRequest request,
        CancellationToken cancellationToken)
    {
        var options = ParseAndValidate(trialBalanceId, userId, request);

        var filteredRows = ApplyFilters(
            _context.BalanceteData.AsNoTracking().Where(row => row.BalanceteId == trialBalanceId),
            options);

        // A existência/acesso e a contagem filtrada compartilham a mesma ida ao banco.
        // Assim, um balancete acessível sem linhas retorna 200, enquanto inexistente ou
        // fora do escopo retorna 404, sem exceder duas consultas no total.
        var total = await _context.Balancete
            .AsNoTracking()
            .Where(trialBalance =>
                trialBalance.Id == trialBalanceId &&
                _context.CompanyUsers.Any(scope =>
                    scope.UserId == userId &&
                    scope.GroupId == trialBalance.AccountPlans.GroupId &&
                    (
                        scope.CompanyId == null ||
                        (
                            scope.CompanyId == trialBalance.AccountPlans.CompanyId &&
                            (
                                scope.SubCompanyId == null ||
                                scope.SubCompanyId == trialBalance.AccountPlans.SubCompanyId
                            )
                        )
                    )))
            .Select(_ => (int?)filteredRows.Count())
            .SingleOrDefaultAsync(cancellationToken);

        if (!total.HasValue)
            throw new TrialBalanceViewerNotFoundException(trialBalanceId);

        var orderedRows = ApplyOrdering(filteredRows, options.Sort, options.Descending);

        var items = await orderedRows
            .Skip(options.Offset)
            .Take(options.Limit)
            .Select(row => new TrialBalanceViewerRowDto
            {
                Id = row.Id,
                // O schema legado guarda a ordem original no identity Id: ambas as rotas
                // de importação inserem a lista na ordem do arquivo e não reordenam depois.
                SourceOrder = row.Id,
                AccountCode = row.CostCenter ?? string.Empty,
                Description = row.Name ?? string.Empty,
                Level = string.IsNullOrEmpty(row.CostCenter)
                    ? 1
                    : row.CostCenter.Length - row.CostCenter.Replace(".", string.Empty).Length + 1,
                HasChildren = _context.BalanceteData.Any(child =>
                    child.BalanceteId == row.BalanceteId &&
                    child.Id != row.Id &&
                    row.CostCenter != null &&
                    child.CostCenter != null &&
                    child.CostCenter.StartsWith(row.CostCenter + ".")),
                PreviousBalance = row.InitialValue,
                Debit = row.Debit,
                Credit = row.Credit,
                FinalBalance = row.FinalValue
            })
            .ToListAsync(cancellationToken);

        return new TrialBalanceViewerRowsResponse
        {
            TrialBalanceId = trialBalanceId,
            Items = items,
            Pagination = new TrialBalanceViewerPaginationDto
            {
                Offset = options.Offset,
                Limit = options.Limit,
                Returned = items.Count,
                Total = total.Value,
                HasMore = (long)options.Offset + items.Count < total.Value
            }
        };
    }

    private static IQueryable<BalanceteDataModel> ApplyFilters(
        IQueryable<BalanceteDataModel> query,
        ViewerQueryOptions options)
    {
        if (options.Search is not null)
        {
            var search = options.Search;
            query = query.Where(row =>
                (row.CostCenter != null && row.CostCenter.Contains(search)) ||
                (row.Name != null && row.Name.Contains(search)));
        }

        if (options.Levels.Count > 0)
            query = query.Where(row => options.Levels.Contains(
                string.IsNullOrEmpty(row.CostCenter)
                    ? 1
                    : row.CostCenter.Length - row.CostCenter.Replace(".", string.Empty).Length + 1));

        if (options.OnlyWithMovement)
            query = query.Where(row => row.Debit != 0m || row.Credit != 0m);

        return query;
    }

    private static IOrderedQueryable<BalanceteDataModel> ApplyOrdering(
        IQueryable<BalanceteDataModel> query,
        string sort,
        bool descending)
    {
        IOrderedQueryable<BalanceteDataModel> ordered = (sort.ToLowerInvariant(), descending) switch
        {
            ("id", false) or ("sourceorder", false) => query.OrderBy(row => row.Id),
            ("id", true) or ("sourceorder", true) => query.OrderByDescending(row => row.Id),
            ("accountcode", false) => query.OrderBy(row => row.CostCenter),
            ("accountcode", true) => query.OrderByDescending(row => row.CostCenter),
            ("description", false) => query.OrderBy(row => row.Name),
            ("description", true) => query.OrderByDescending(row => row.Name),
            ("level", false) => query.OrderBy(row =>
                string.IsNullOrEmpty(row.CostCenter)
                    ? 1
                    : row.CostCenter.Length - row.CostCenter.Replace(".", string.Empty).Length + 1),
            ("level", true) => query.OrderByDescending(row =>
                string.IsNullOrEmpty(row.CostCenter)
                    ? 1
                    : row.CostCenter.Length - row.CostCenter.Replace(".", string.Empty).Length + 1),
            ("previousbalance", false) => query.OrderBy(row => row.InitialValue),
            ("previousbalance", true) => query.OrderByDescending(row => row.InitialValue),
            ("debit", false) => query.OrderBy(row => row.Debit),
            ("debit", true) => query.OrderByDescending(row => row.Debit),
            ("credit", false) => query.OrderBy(row => row.Credit),
            ("credit", true) => query.OrderByDescending(row => row.Credit),
            ("finalbalance", false) => query.OrderBy(row => row.FinalValue),
            ("finalbalance", true) => query.OrderByDescending(row => row.FinalValue),
            _ => throw new InvalidOperationException("Ordenação validada não foi mapeada.")
        };

        // Id representa simultaneamente SourceOrder e o identificador único no schema
        // atual. Portanto, este desempate cobre ambos e estabiliza blocos consecutivos.
        return sort.Equals("id", StringComparison.OrdinalIgnoreCase) ||
               sort.Equals("sourceOrder", StringComparison.OrdinalIgnoreCase)
            ? ordered
            : ordered.ThenBy(row => row.Id);
    }

    private static ViewerQueryOptions ParseAndValidate(
        int trialBalanceId,
        int userId,
        TrialBalanceViewerRowsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (trialBalanceId <= 0)
            throw new ArgumentException("trialBalanceId deve ser maior que zero.");

        if (userId <= 0)
            throw new ArgumentException("Usuário autenticado inválido.");

        if (request.Offset < 0)
            throw new ArgumentException("offset deve ser maior ou igual a zero.");

        if (request.Limit < 1 || request.Limit > MaximumLimit)
            throw new ArgumentException($"limit deve estar entre 1 e {MaximumLimit}.");

        var sort = string.IsNullOrWhiteSpace(request.Sort) ? "sourceOrder" : request.Sort.Trim();
        if (!AllowedSortFields.Contains(sort))
            throw new ArgumentException($"sort inválido. Valores permitidos: {string.Join(", ", AllowedSortFields.Order())}.");

        var direction = string.IsNullOrWhiteSpace(request.Direction) ? "asc" : request.Direction.Trim();
        if (!direction.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
            !direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("direction deve ser 'asc' ou 'desc'.");
        }

        var levels = ParseLevels(request.Levels);

        return new ViewerQueryOptions(
            request.Offset,
            request.Limit,
            string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
            levels,
            request.OnlyWithMovement,
            sort,
            direction.Equals("desc", StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyCollection<int> ParseLevels(string? rawLevels)
    {
        if (string.IsNullOrWhiteSpace(rawLevels))
            return Array.Empty<int>();

        var levels = new HashSet<int>();
        foreach (var value in rawLevels.Split(',', StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(value, out var level) || level is < 1 or > 5)
                throw new ArgumentException("levels deve conter apenas valores inteiros entre 1 e 5, separados por vírgula.");

            levels.Add(level);
        }

        return levels;
    }

    private sealed record ViewerQueryOptions(
        int Offset,
        int Limit,
        string? Search,
        IReadOnlyCollection<int> Levels,
        bool OnlyWithMovement,
        string Sort,
        bool Descending);
}
