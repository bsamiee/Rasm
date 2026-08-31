namespace Lab.F20;

internal static class Agent {
    public static Conduit<M, M> Inbox<M>() => Conduit.make(Buffer<M>.Unbounded);

    public static IO<ForkIO<S>> Start<S, M>(Conduit<M, M> inbox, S initialState, Func<S, M, S> process) =>
        inbox.Reduce(initialState, (state, message) => Reduced.ContinueIO(process(state, message))).Fork();

    public static IO<ForkIO<S>> Start<S, M>(Conduit<M, M> inbox, S initialState, Func<S, M, IO<S>> process) =>
        inbox.Reduce(initialState, (state, message) => process(state, message).Map(Reduced.Continue)).Fork();
}
