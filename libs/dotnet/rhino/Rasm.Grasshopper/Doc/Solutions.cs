using Grasshopper2.Doc;
using Rasm.Rhino;
using Rasm.Rhino.Persistence.Settings;

namespace Rasm.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Solutions {
    private static Fin<Solution> Culminated(Solution solution, CancellationToken token) =>
        solution.Phase is SolutionPhase.Completed ? solution
        : token.IsCancellationRequested ? Errors.Cancelled
        : new SolutionUnfinished(solution.Id, solution.Phase);

    public static IO<Solution> Start(SolutionServer server, CancellationTokenSource source, SolutionMode mode) =>
        from token in cancelToken
        let requested = source.Token
        let refused = Fin.Fail<Solution>(new Refused(nameof(SolutionServer.Start)))
        from solution in IO.liftAsync(() => server.Start(source, mode))
            .Catch(static error => error.Is(Errors.Cancelled), _ => IO.lift(token.IsCancellationRequested ? Fin.Fail<Solution>(Errors.Cancelled) : refused))
            .Bind(completed => IO.lift(completed.Token == requested ? Culminated(completed, token) : refused))
        select solution;

    public static IO<Solution> StartWait(SolutionServer server, SolutionMode mode) =>
        from token in cancelToken
        from solution in DisposalOps.Within(() => CancellationTokenSource.CreateLinkedTokenSource(token),
            source => IO.lift(() => Culminated(server.StartWait(source, mode), token)),
            static source => IO.lift(source.Dispose))
        select solution;

    public static IO<A> Paused<A>(IO<A> body) =>
        new ProcessSetting<bool>(
            IO.lift(static () => SolutionServer.EnableSolutions),
            static enabled => IO.lift(() => { SolutionServer.EnableSolutions = enabled; }))
            .Within(static _ => false, body);
}
