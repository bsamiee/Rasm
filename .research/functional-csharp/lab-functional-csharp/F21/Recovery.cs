namespace Lab.F21;

internal static class Recovery {
    public static Fin<Age> ByCode(Fin<Age> age) => age.Catch(Codes.InvalidAge, static _ => Age.From(0)).As();

    public static Fin<Age> ByValue(Fin<Age> age) => age.Catch(new InvalidAge(), static _ => Age.From(0)).As();

    public static Fin<Age> ByPredicate(Fin<Age> age) => age.Catch(static error => error.IsExpected, static _ => Age.From(0)).As();

    public static IO<Person> Cached(IO<Person> load, Person cached) => load.Catch(Codes.NotFound, _ => IO.pure(cached)).As();

    public static IO<Person> Fallback(IO<Person> primary, IO<Person> secondary) => primary | secondary;

    public static Fin<Age> Rebound(Fin<Age> age) => age.BindFail(static error => error.HasCode(Codes.InvalidAge) ? Age.From(0) : error);

    public static Fin<Age> WithContext(Fin<Age> age) => age.MapFail(static error => Error.New("registration", error));

    public static int AtHost(Fin<int> result) => result.IfFail(static _ => -1);
}
