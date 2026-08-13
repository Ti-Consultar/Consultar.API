using _2___Application._2_Dto_s.Breadcrumb;
using _4_InfraData._1_Context;
using _4_InfraData._1_Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConsultarMRP.Tests.Breadcrumb;

public sealed class BreadcrumbServiceTests
{
    [Theory]
    [InlineData(null, null, null, "/ponto-equilibrio")]
    [InlineData(10, null, null, "/grupos/10/ponto-equilibrio")]
    [InlineData(10, 20, null, "/grupos/10/empresas/20/ponto-equilibrio")]
    [InlineData(10, 20, 30, "/grupos/10/empresas/20/filiais/30/ponto-equilibrio")]
    public async Task Resolves_break_even_route_for_every_financial_scope(
        int? groupId,
        int? companyId,
        int? subCompanyId,
        string expectedPath)
    {
        await using var context = CreateContext();
        var service = new BreadcrumbService(
            new GroupRepository(context),
            new CompanyRepository(context),
            null!);

        var result = await service.ResolveBreadcrumbAsync(new BreadcrumbResolveQueryDto
        {
            RouteKey = "break-even",
            GroupId = groupId,
            CompanyId = companyId,
            SubCompanyId = subCompanyId
        });

        var screen = Assert.Single(result, item => item.Type == "screen");
        Assert.Equal("Ponto de Equilíbrio", screen.Label);
        Assert.Equal(expectedPath, screen.Path);
        Assert.Equal("break-even", screen.RouteKey);
        Assert.DoesNotContain(result, item => item.Type == "section");
    }

    private static CoreServiceDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CoreServiceDbContext>()
            .UseInMemoryDatabase($"breadcrumb-{Guid.NewGuid():N}")
            .Options;

        return new CoreServiceDbContext(options);
    }
}
