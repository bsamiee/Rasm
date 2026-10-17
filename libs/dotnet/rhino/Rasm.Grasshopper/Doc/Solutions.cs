using Grasshopper2.Doc;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;

namespace Rasm.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Solutions {
    private static readonly Error Declined = new Refused(nameof(SolutionServer.Start), Refused.Declined);

    private static Fin<Solution> Culminated(Solution solution, Option<Error> fault, CancellationToken token) =>
        solution.Phase is SolutionPhase.Completed ? solution
        : token.IsCancellationRequested ? Errors.Cancelled
        : new Expected($"solution {solution.Id} ended {solution.Phase}", 0, fault);

    private static IO<Solution> Watched(SolutionServer server, Func<CancellationToken, Action<IDisposable>, IO<Solution>> start) =>
        from token in cancelToken
        from solution in DisposalOps.Within<IDisposable, Solution>(retain =>
            from faults in atomIO(HashMap<SolutionId, Error>())
            from attached in Subscriptions.Attach<EventHandler<SolutionExceptionEventArgs>>(
                handler => server.SolutionFaulted += handler,
                handler => server.SolutionFaulted -= handler,
                (_, args) => _ = faults.Swap(held => held.AddOrUpdate(args.Solution.Id, Error.New(args.Exception))), retain)
            from started in start(token, retain)
            from culminated in IO.lift(Culminated(started, faults.Value.Find(started.Id), token))
            select culminated, DisposalOps.Release)
        select solution;

    public static IO<Solution> Start(SolutionServer server, CancellationTokenSource source, SolutionMode mode) =>
        Watched(server, (token, _) =>
            from completed in IO.liftAsync(() => server.Start(source, mode))
                .Catch(static error => error.Is(Errors.Cancelled), _ => IO.fail<Solution>(token.IsCancellationRequested ? Errors.Cancelled : Declined))
            from started in IO.lift(completed.Token == source.Token ? Fin.Succ(completed) : Declined)
            select started);

    public static IO<Solution> StartWait(SolutionServer server, SolutionMode mode) =>
        Watched(server, (token, retain) =>
            from source in DisposalOps.Acquire(IO.lift(() => CancellationTokenSource.CreateLinkedTokenSource(token)), retain)
            from solution in IO.lift(() => server.StartWait(source, mode))
            select solution);

    public static IO<A> Paused<A>(IO<A> body) =>
        new ProcessSetting<bool>(
            IO.lift(static () => SolutionServer.EnableSolutions),
            static enabled => IO.lift(() => { SolutionServer.EnableSolutions = enabled; }))
            .Within(static _ => false, body);
}
