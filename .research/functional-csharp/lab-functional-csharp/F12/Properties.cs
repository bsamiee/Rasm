namespace Lab.F12;

internal static class Properties {
    public static Fin<Unit> Probe() {
        Func<int, int, int> multiply = static (x, y) => x * y;
        Option<Func<int, int, int>> lifted = Pure(multiply);
        Gen<Option<int>> option = Gen.OneOf(Gen.Int[-1000, 1000].Select(static x => Some(x)), Gen.Const(Option<int>.None));
        Fin<Unit> equivalence = Try.lift(() => {
            option.Select(option).Sample((a, b) =>
                multiply.Map(a).Apply(b).As() == lifted.Apply(a).Apply(b).As());
            return unit;
        }).Run();
        return Samples.Check(
            nameof(Properties),
            ("equivalence.IsSucc", equivalence.IsSucc));
    }
}
