namespace Lab.F17;

internal static class States {
    public static Seq<Session> Trace(Session initial, Func<Session, Session> advance) =>
        toSeq(LanguageExt.List.unfold(initial, state => Step(state, advance)));

    private static Option<(Session, Session)> Step(Session state, Func<Session, Session> advance) =>
        state.HasExited ? None : Emit(advance(state));

    private static Option<(Session, Session)> Emit(Session next) => Some((next, next));
}
