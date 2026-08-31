namespace Lab.F06;

internal sealed record Cache<T>(HashMap<Guid, T> Entries) {
    public T Get(Guid id, Func<T> onMiss) => Entries.Find(id).IfNone(onMiss);
}

internal sealed record EnemyShip(string Type, string WeaponryLevel);

internal sealed record ReportItem(string ColumnOne, string ColumnTwo);

internal sealed record Report(string Title, Seq<ReportItem> Rows);

internal static class Reports {
    public static Report BuildSummary(Seq<EnemyShip> ships, Func<EnemyShip, string> summarizeBy, string title) =>
        new(title, toSeq(ships.GroupBy(summarizeBy, StringComparer.Ordinal)).Map(static g => new ReportItem(g.Key, string.Create(CultureInfo.InvariantCulture, $"{g.Count()}"))));
}

internal static class Summaries {
    public static Report ByType(Seq<EnemyShip> ships) => Reports.BuildSummary(ships, static ship => ship.Type, "Enemy Ship Type");

    public static Report ByWeaponry(Seq<EnemyShip> ships) => Reports.BuildSummary(ships, static ship => ship.WeaponryLevel, "Enemy Ship Weaponry Level");
}
