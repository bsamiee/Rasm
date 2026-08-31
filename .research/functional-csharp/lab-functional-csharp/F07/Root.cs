namespace Lab.F07;

internal sealed record OwnerNotFound() : Expected("The transfer owner is unknown", 101);

internal sealed record Runtime(ConnectionIO Connection, Func<DateTime> Clock) : Has<Eff<Runtime>, ConnectionIO> {
    static K<Eff<Runtime>, ConnectionIO> Has<Eff<Runtime>, ConnectionIO>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Connection);
}

internal static class Workflow {
    public static Eff<RT, Employee> Book<RT>(
        Func<BookTransfer, Validation<Error, BookTransfer>> validate, Func<Guid, Eff<RT, Option<Employee>>> lookup,
        Func<BookTransfer, IO<Unit>> save, BookTransfer command) =>
        from valid in Eff<RT, BookTransfer>.Lift(() => validate(command).ToFin())
        from owner in lookup(valid.OwnerId)
        from found in Eff<RT, Employee>.Lift(() => owner.ToFin(new OwnerNotFound()))
        from _ in save(valid)
        select found;
}

internal static class Host {
    public static Fin<Employee> Book(Runtime runtime, Func<BookTransfer, IO<Unit>> save, BookTransfer command) =>
        Workflow.Book(Validators.DateNotPast(runtime.Clock), Lookups<Runtime>.LookupEmployee, save, command).Run(runtime);
}
