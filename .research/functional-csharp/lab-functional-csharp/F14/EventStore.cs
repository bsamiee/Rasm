namespace Lab.F14;

internal static class EventStore {
    public static IO<Seq<Event>> Load(Atom<Seq<Event>> store, Guid accountId) =>
        store.ValueIO.Map(events => events.Filter(evt => evt.AccountId == accountId));

    public static IO<Unit> Save(Atom<Seq<Event>> store, Event evt) =>
        store.SwapIO(events => events.Add(evt)).Map(static _ => unit);
}
