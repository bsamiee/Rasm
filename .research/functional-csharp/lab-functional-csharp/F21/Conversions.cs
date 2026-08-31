namespace Lab.F21;

internal static class Conversions {
    public static Fin<int> Required(Option<int> value) => value.ToFin(new NotFound());

    public static Validation<Error, int> Admitted(Option<int> value) => value.ToValidation<Error>(new NotFound());

    public static Option<Age> Present(Fin<Age> age) => age.ToOption();

    public static Either<Error, Age> Split(Fin<Age> age) => age.ToEither();

    public static Seq<int> Items(Option<int> value) => value.ToSeq();

    public static Fin<Person> Exit(Validation<Error, Person> form) => form.ToFin();

    public static Validation<Error, Age> Widen(Fin<Age> age) => age.ToValidation();

    public static Fin<int> Captured(Try<int> attempt) => attempt.Run();

    public static Fin<Person> Ran(IO<Person> effect) => effect.RunSafe();

    public static Fin<DateTimeOffset> Stamped(Runtime runtime) => Concerns.Stamp<Runtime>().Run(runtime);
}
