namespace Lab.F12;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        Applicatives.ApplyProbe,
        Applicatives.LiftProbe,
        Applicatives.LawsProbe,
        QueriesProbe.Run,
        PhoneNumbersProbe.Run,
        ValidatorsProbe.Run,
        Properties.Probe,
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
