using _2___Application._2_Dto_s.TotalizerClassification;

namespace _2___Application._1_Services.TotalizerClassification;

public static class ReclassifiedBalanceSheetAggregation
{
    public static Dictionary<string, TotalizerParentRespone> BuildTotalizerMap(
        IEnumerable<TotalizerParentRespone> totalizers) =>
        totalizers
            .Where(totalizer => !string.IsNullOrWhiteSpace(totalizer.Name))
            .GroupBy(totalizer => totalizer.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var first = group.First();
                    return new TotalizerParentRespone
                    {
                        Id = first.Id,
                        Name = first.Name,
                        TypeOrder = first.TypeOrder,
                        TotalValue = group.Sum(totalizer => totalizer.TotalValue),
                        Classifications = group
                            .SelectMany(totalizer =>
                                totalizer.Classifications ?? new List<ClassificationRespone>())
                            .ToList()
                    };
                },
                StringComparer.OrdinalIgnoreCase);

    public static Dictionary<string, ClassificationRespone> BuildClassificationMap(
        IEnumerable<TotalizerParentRespone> totalizers) =>
        totalizers
            .SelectMany(totalizer =>
                totalizer.Classifications ?? new List<ClassificationRespone>())
            .Where(classification => !string.IsNullOrWhiteSpace(classification.Name))
            .GroupBy(classification => classification.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var first = group.First();
                    return new ClassificationRespone
                    {
                        Id = first.Id,
                        Name = first.Name,
                        TypeOrder = first.TypeOrder,
                        Value = group.Sum(classification => classification.Value),
                        Datas = group
                            .SelectMany(classification =>
                                classification.Datas ?? new List<BalanceteDataResponse>())
                            .ToList()
                    };
                },
                StringComparer.OrdinalIgnoreCase);

    public static decimal RecalculateTotalValue(TotalizerParentRespone totalizer)
    {
        totalizer.TotalValue = (totalizer.Classifications ?? new List<ClassificationRespone>())
            .Sum(classification => classification.Value);
        return totalizer.TotalValue;
    }
}
