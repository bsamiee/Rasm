namespace Lab.F22;

internal static class Exits {
    public static Fin<int> Safe => Construction.Failed.RunSafe();

    public static Fin<int> Thrown => Try.lift(Construction.Failed.Run).Run();

    public static Fin<int> Recovered => Construction.Failed.Catch(503, static _ => IO.pure(9)).As().RunSafe();

    public static Fin<int> Alternative => (Construction.Failed | IO.pure(8)).RunSafe();

    public static Fin<int> Cancelled() {
        using EnvIO env = EnvIO.New(token: new CancellationToken(canceled: true));
        return Try.lift(() => Construction.TokenAware.Run(env)).Run();
    }
}
