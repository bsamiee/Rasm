namespace Lab.F19;

internal static class Sources {
    private static Action<string> onMessage = static _ => { };

    private static readonly Event<string> Messages = Event.from(ref onMessage);

    public static Source<string> OneValue => Source.pure("ready");

    public static Source<int> FiniteValues => Source.lift(Seq(1, 2, 3));

    public static IO<string> FirstMessage =>
        from messages in Messages.Subscribe()
        from _ in IO.lift(static () => onMessage("hello"))
        from head in messages.Take(1).Last()
        select head;
}
