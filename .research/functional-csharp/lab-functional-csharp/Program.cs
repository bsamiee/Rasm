namespace Lab;

internal static class Program {
    private static readonly Seq<(string Name, Func<Fin<Unit>> Sample)> Samples = [
        ("F00", F00.Verify.Run),
        ("F21", F21.Samples.Run),
        ("F03", F03.Samples.Run),
        ("F22", F22.Samples.Run),
        ("F11", F11.Samples.Run),
        ("F08", F08.Samples.Run),
        ("F12", F12.Samples.Run),
        ("F18", F18.Samples.Run),
        ("F14", F14.Samples.Run),
        ("F09", F09.Samples.Run),
        ("F15", F15.Samples.Run),
        ("F17", F17.Samples.Run),
        ("F16", F16.Samples.Run),
        ("F20", F20.Samples.Run),
        ("F05", F05.Samples.Run),
        ("F13", F13.Samples.Run),
        ("F10", F10.Samples.Run),
        ("F06", F06.Samples.Run),
        ("F02", F02.Samples.Run),
        ("F07", F07.Samples.Run),
        ("F04", F04.Samples.Run),
        ("F19", F19.Samples.Run),
        ("F01", F01.Samples.Run),
        ("F24", F24.Samples.Run),
        ("F23", F23.Samples.Run),
        ("T01", T01.Samples.Run),
        ("T02", T02.Samples.Run),
        ("T03", T03.Samples.Run),
        ("T04", T04.Samples.Run),
        ("T05", T05.Samples.Run),
        ("T06", T06.Samples.Run),
        ("T07", T07.Samples.Run),
    ];

    private static int Main() {
        Seq<(string Name, Error Error)> failures = Samples.Choose(static sample =>
            sample.Sample().Match(
                Succ: static _ => Option<(string Name, Error Error)>.None,
                Fail: error => Some((sample.Name, error))));
        _ = failures.Iter(static failure => Console.Error.WriteLine($"{failure.Name}: {failure.Error}"));
        return failures.Count;
    }
}
