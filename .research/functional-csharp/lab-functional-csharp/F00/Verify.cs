namespace Lab.F00;

internal static class Verify {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        Style.Probe,
        Results.Probe,
        Effects.Probe,
        Loops.Probe,
        Collections.Probe,
        Streams.Probe,
        Laws.Probe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    public static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }
}
