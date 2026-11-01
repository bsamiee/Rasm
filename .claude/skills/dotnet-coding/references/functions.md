# [FUNCTIONS]

Covers functions as values, from composition and functions held as data to the composition root that wires a request's effects.

## [01]-[COMPOSITION]

`g << f` states `f >> g` right to left. Step factories take each value a step varies in as a parameter with a default where one value dominates, one composed field states a fixed pipeline, and `Fold` from `identity` composes a pipeline whose steps arrive as data, an empty one returning its input:

```csharp
internal static class Passes {
    public static Func<double, double> Offset(double by) => x => x + by;
    public static Func<double, double> Gain(double by) => x => x * by;
    public static Func<double, double> Clamp(double low = -1.0, double high = 1.0) => x => Math.Clamp(x, low, high);

    public static readonly Func<double, double> Centered = Offset(-0.5) >> Gain(2.0) >> Clamp();

    public static Func<double, double> Chain(Seq<Func<double, double>> passes) => passes.Fold(fun<double, double>(identity), static (all, next) => all >> next);
}
```

A static field holds a specialization fixed at type load, a getter-only property closes over instance state and allocates a delegate per read, and a factory method alone takes a type parameter. `flip` and `par` adapt a two-argument function value the code does not declare (a dependency, a package delegate): `flip` reorders its parameters and `par` fixes the leading one.

## [02]-[FUNCTIONS_AS_DATA]

A record of the operations targets differ in takes the store as an argument, so each target is one static instance overriding only the `init` members that differ and serving every store. A host type the code does not own takes no interface, and the record stands where an interface would need one implementation per target:

```csharp
internal sealed record Layer(string FullPath, bool IsDeleted);
internal sealed record Material(string Name);
internal sealed record Store(Seq<Layer> Layers, Seq<Material> Materials);

internal sealed record Table<T>(Func<Store, Seq<T>> Rows, Func<T, string> Key) {
    public Func<T, bool> Live { get; init; } = static _ => true;
    public StringComparer Names { get; init; } = StringComparer.OrdinalIgnoreCase;

    public Option<T> Named(Store store, string name) => Rows(store).Find(row => Live(row) && Names.Equals(Key(row), name));
}

internal static class Tables {
    public static readonly Table<Layer> Layers = new(static store => store.Layers, static layer => layer.FullPath) { Live = static layer => !layer.IsDeleted };
    public static readonly Table<Material> Materials = new(static store => store.Materials, static material => material.Name) { Names = StringComparer.Ordinal };
}
```

A first-match decision fixed in code is a switch expression with relational patterns. Rows that arrive as configuration are data a factory reads: each row holds only the values its arms differ in, `Find` returns the first matching row as an `Option`, row order is precedence, and the result past the last row is a parameter that keeps the returned function total:

```csharp
internal static class Tariffs {
    public static Func<decimal, decimal> Priced(Seq<(decimal UpTo, decimal Rate)> bands, decimal beyond) =>
        weight => weight * bands.Find(band => weight <= band.UpTo).Map(static band => band.Rate).IfNone(beyond);
}
```

## [03]-[COMPOSITION_ROOT]

A returned `map.Find` hands on one lookup without the map, and a test passes a lambda where a mock would stand. A handler is a factory whose dependencies lead and whose returned function takes the request. The composition root wires by function application with no container: it adapts infrastructure once, returns the handler the host runs per request, and reads each request-scoped value (the date) inside the returned effect so no validator outlives its snapshot:

```csharp
internal sealed record Command(Guid Id, LocalDate Date);

internal static class Bookings {
    public static Func<Command, IO<Unit>> Handler(Func<Command, Fin<Command>> validate, Func<Command, IO<Unit>> save, Func<Command, IO<Unit>> notify) =>
        command =>
            from valid in IO.lift(validate(command))
            from _ in save(valid)
            from __ in notify(valid)
            select unit;

    public static Func<Command, IO<Unit>> Root(TimeProvider time, AtomHashMap<Guid, Command> store, Func<Command, IO<Unit>> notify) =>
        command => IO.lift(() => time.ToZonedClock().GetCurrentDate())
            .Bind(today => Handler(Validators.NotPast(today), booked => IO.lift(() => store.AddOrUpdate(booked.Id, booked)), notify)(command));
}
```

Composition orders effects without making them atomic: `notify` runs only after `save` succeeds, and a failure between them leaves a saved booking no one hears of. A flow across systems writes its own state and the pending notice in one commit, then delivers the notice until delivery succeeds, each delivery idempotent.
