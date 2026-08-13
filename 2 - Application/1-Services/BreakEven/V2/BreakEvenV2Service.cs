using _2___Application._1_Services.Scope;
using _2___Application._2_Dto_s.BreakEven.V2;

namespace _2___Application._1_Services.BreakEven.V2;

public sealed class BreakEvenV2Service
{
    private readonly IBreakEvenScopeService _scopeService;
    private readonly IBreakEvenDreProvider _dreProvider;
    private readonly BreakEvenCalculator _calculator;

    public BreakEvenV2Service(
        IBreakEvenScopeService scopeService,
        IBreakEvenDreProvider dreProvider,
        BreakEvenCalculator calculator)
    {
        _scopeService = scopeService;
        _dreProvider = dreProvider;
        _calculator = calculator;
    }

    public Task<BreakEvenV2Response> GetAsync(
        EntityScopeRequest scope,
        int year,
        int month,
        int userId,
        CancellationToken cancellationToken) =>
        CalculateAsync(
            scope,
            year,
            month,
            0m,
            Array.Empty<BreakEvenLineSimulationDto>(),
            userId,
            cancellationToken);

    public Task<BreakEvenV2Response> SimulateAsync(
        BreakEvenSimulationRequest request,
        int userId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CalculateAsync(
            CreateScope(request.GroupId, request.CompanyId, request.SubCompanyId),
            request.Year,
            request.Month,
            request.Factor,
            request.Simulations,
            userId,
            cancellationToken);
    }

    private async Task<BreakEvenV2Response> CalculateAsync(
        EntityScopeRequest scope,
        int year,
        int month,
        decimal factor,
        IReadOnlyList<BreakEvenLineSimulationDto> simulations,
        int userId,
        CancellationToken cancellationToken)
    {
        ValidatePeriod(year, month);
        var accountPlanId = await _scopeService.ResolveAuthorizedAccountPlanIdAsync(
            scope,
            userId,
            cancellationToken);
        var sourceRows = await _dreProvider.GetMonthlyRowsAsync(
            accountPlanId,
            year,
            month,
            cancellationToken);
        var result = _calculator.Calculate(sourceRows, factor, simulations);

        return new BreakEvenV2Response
        {
            Data = new BreakEvenV2DataDto
            {
                Scope = new BreakEvenScopeDto
                {
                    GroupId = scope.GroupId,
                    CompanyId = scope.CompanyId,
                    SubCompanyId = scope.SubCompanyId
                },
                Period = new BreakEvenPeriodDto { Year = year, Month = month },
                Factor = factor,
                Summary = result.Summary,
                Rows = result.Rows
            }
        };
    }

    private static EntityScopeRequest CreateScope(
        int groupId,
        int? companyId,
        int? subCompanyId) =>
        new()
        {
            GroupId = groupId,
            CompanyId = companyId,
            SubCompanyId = subCompanyId,
            IncludeChildren = false
        };

    private static void ValidatePeriod(int year, int month)
    {
        if (year <= 0)
            throw new BreakEvenValidationException("year deve ser maior que zero.");
        if (month is < 1 or > 12)
            throw new BreakEvenValidationException("month deve estar entre 1 e 12.");
    }
}
