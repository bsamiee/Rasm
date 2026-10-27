using Grasshopper2.Doc;

namespace Rasm.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Solutions {
    // --- [RUNS]
    private static Fin<Solution> Culminated(Solution solution, Option<Error> fault, CancellationToken token) =>
        solution.Phase is SolutionPhase.Completed ? solution
        : token.IsCancellationRequested ? Errors.Cancelled
        : new GrasshopperFault($"solution {solution.Id} in {solution.Phase}", GrasshopperReason.Unfinished, Inner: fault);

    private static IO<Solution> Watched(SolutionServer server, IGrasshopperPlugin owner, Func<CancellationToken, IO<Solution>> start) =>
        (from token in cancelToken
         from faults in atomIO(HashMap<SolutionId, Error>())
         from attached in Hooks.On<SolutionExceptionEventArgs>(
             h => server.SolutionFaulted += h, h => server.SolutionFaulted -= h,
             args => faults.SwapIO(held => held.AddOrUpdate(args.Solution.Id, Error.New(args.Exception))).Map(unit), owner)
         from started in start(token)
         from culminated in IO.lift(() => Culminated(started, faults.Value.Find(started.Id), token))
         select culminated).Bracket();

    public static IO<Solution> Start(SolutionServer server, CancellationTokenSource source, SolutionMode mode, IGrasshopperPlugin owner) =>
        Watched(server, owner, token =>
            from completed in IO.liftAsync(() => server.Start(source, mode)).Map(static solution => Some(solution))
                .Catch(static error => error.Is(Errors.Cancelled), _ => token.IsCancellationRequested ? IO.fail<Option<Solution>>(Errors.Cancelled) : IO.pure(Option<Solution>.None))
            from started in IO.lift(() => Hosts.Found(completed.Filter(solution => solution.Token == source.Token)))
            select started);

    public static IO<Solution> StartWait(SolutionServer server, SolutionMode mode, IGrasshopperPlugin owner) =>
        Watched(server, owner, token =>
            from source in use(() => CancellationTokenSource.CreateLinkedTokenSource(token))
            from solution in IO.lift(() => server.StartWait(source, mode))
            select solution);

    // --- [PAUSES]
    public static IO<A> Paused<A>(IO<A> body) =>
        IO.lift(static () => SolutionServer.EnableSolutions).Bind(prior =>
            IO.lift(static () => { SolutionServer.EnableSolutions = false; }) >> body.Finally(IO.lift(() => { SolutionServer.EnableSolutions = prior; })));
}
