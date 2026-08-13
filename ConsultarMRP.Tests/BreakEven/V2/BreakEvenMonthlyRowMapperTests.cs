using _2___Application._1_Services.BreakEven.V2;
using _2___Application._2_Dto_s.DRE.V2;
using Xunit;

namespace ConsultarMRP.Tests.BreakEven.V2;

public sealed class BreakEvenMonthlyRowMapperTests
{
    [Fact]
    public void Selects_only_the_requested_realized_month()
    {
        var response = new DreV2Response
        {
            Data = new DreV2DataDto
            {
                Scenarios = new[] { new DreScenarioDto { Key = "realizado" } },
                Periods = new[]
                {
                    new DrePeriodDto { Key = "2026-01", Year = 2026, Month = 1, Type = "month" },
                    new DrePeriodDto { Key = "2026-08", Year = 2026, Month = 8, Type = "month" },
                    new DrePeriodDto { Key = "accumulated", Year = 2026, Type = "accumulated" }
                },
                Rows = new[]
                {
                    new DreRowDto
                    {
                        Code = "GROSS_REVENUE",
                        Name = "Receita Operacional Bruta",
                        Values = new Dictionary<string, Dictionary<string, decimal?>>
                        {
                            ["realizado"] = new()
                            {
                                ["2026-01"] = 10m,
                                ["2026-08"] = 80m,
                                ["accumulated"] = 90m
                            },
                            ["orcado"] = new() { ["2026-08"] = 999m }
                        }
                    }
                }
            }
        };

        var rows = BreakEvenMonthlyRowMapper.Map(response, 2026, 8);

        Assert.Single(rows);
        Assert.Equal(80m, rows[0].Value);
    }

    [Fact]
    public void Rejects_a_month_without_dre_data()
    {
        var response = new DreV2Response
        {
            Data = new DreV2DataDto
            {
                Scenarios = new[] { new DreScenarioDto { Key = "realizado" } },
                Periods = new[]
                {
                    new DrePeriodDto { Key = "2026-07", Year = 2026, Month = 7, Type = "month" }
                }
            }
        };

        Assert.Throws<BreakEvenNotFoundException>(() =>
            BreakEvenMonthlyRowMapper.Map(response, 2026, 8));
    }
}
