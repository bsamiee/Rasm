<!-- Prose of [01] to [04] integrated into dotnet-languageext [05], [05] into dotnet-coding, the code blocks remain for dotnet-languageext/references/traits-and-transformers.md -->
# [LANGUAGEEXT_TRAITS_AND_TRANSFORMERS]

Traits are interfaces with static abstract members. Witness types implement the trait for one type constructor. Generic code names the witness in a constraint and calls the trait members through it.

<!-- Integrated into .claude/skills/dotnet-languageext/SKILL.md
## [01]-[HIGHER_KINDS]

`K<F, A>` is an empty interface with two type arguments. `F` is the witness for the type constructor and `A` is the element type. `Option<A>` implements `K<Option, A>`, and `Seq<A>` implements `K<Seq, A>`. Functions constrained by `F : Functor<F>` can call `Map` on any `K<F, A>`. `.As()` restores the concrete type at the API boundary.

```csharp
internal sealed record Line(string Sku, decimal Price);

internal static class HigherKinds {
    public static K<F, decimal> Prices<F>(K<F, Line> lines) where F : Functor<F> => lines.Map(static line => line.Price);
    public static Option<decimal> OptionPrice(Option<Line> line) => Prices(line).As();
    public static Seq<decimal> SeqPrices(Seq<Line> lines) => Prices(lines).As();
}
```
-->

<!-- Integrated into .claude/skills/dotnet-languageext/SKILL.md
## [02]-[TRAITS]

The table lists the members used in the code block.

| [INDEX] | [TRAIT]            | [MEMBERS]                                    |
| :-----: | :----------------- | :------------------------------------------- |
|  [01]   | `Functor<F>`       | `Map`                                        |
|  [02]   | `Applicative<F>`   | `Pure`, `Apply`, tuple `Apply`               |
|  [03]   | `Monad<M>`         | `Bind`, LINQ query syntax                    |
|  [04]   | `Foldable<T>`      | `Fold`, `FoldBack`, `Exists`, `ForAll`, `At` |
|  [05]   | `Traversable<T>`   | `Traverse`, `TraverseM`                      |
|  [06]   | `Fallible<E, F>`   | `Fail`, `Catch`                              |
|  [07]   | `Readable<M, Env>` | `ask`, `asks`, `local`                       |
|  [08]   | `Stateful<M, S>`   | `get`, `put`, `modify`, `state`, `local`     |
|  [09]   | `Writable<M, W>`   | `tell`                                       |
|  [10]   | `Alternative<F>`   | `Empty`, `Choose`, the alternative operator  |

The witness is the concrete type without its last type argument. The samples run on `Option`, `Seq`, `Fin`, `IO`, and `Validation<Error>`. The environment, state, and output samples run on `Reader<Settings>`, `ReaderT<Settings, IO>`, `State<int>`, `StateT<int, IO>`, `Writer<Seq<string>>`, and `WriterT<Seq<string>, IO>`. `Map`, `Bind`, `Fold`, `FoldBack`, `Exists`, `ForAll`, `At`, and `Catch` are extension methods that the constraint makes available. The tuple `Apply` and LINQ query syntax come from the same constraint. `F.Pure`, `F.Apply`, `F.Fail`, `F.Empty`, `F.Choose`, `T.Traverse`, and `T.TraverseM` are calls on the witness. `Readable.ask`, `Stateful.get`, and `Writable.tell` are module functions that take the witness as a type argument.

`Fallible<E, F>` defines `Fail` and a `Catch` overload that selects errors with a predicate. `Fallible<F>` fixes `E` to `Error`. The `Catch` overloads that select by code and by error value are extensions on the same constraint. The `Recovered` method works with both `Fin` and `IO` through the error predicate. `Alternative<F>` extends `Choice<F>`, making `Choose` the generic form of the operator that `Chosen` shows on `Option`.
-->

```csharp
internal sealed record Rejected() : Expected("value rejected", 2301);
internal sealed record Settings(int Factor);

internal static class Traits {
    public static K<F, int> Doubled<F>(K<F, int> values) where F : Functor<F> => values.Map(static v => v * 2);
    public static K<F, int> Lifted<F>(int value) where F : Applicative<F> => F.Pure(value);
    public static K<F, int> Incremented<F>(K<F, int> value) where F : Applicative<F> => F.Apply(F.Pure<Func<int, int>>(static v => v + 1), value);
    public static K<F, int> Summed<F>(K<F, int> left, K<F, int> right) where F : Applicative<F> => (left, right).Apply(static (a, b) => a + b);
    public static K<M, int> Halved<M>(K<M, int> value, Func<int, K<M, int>> halve) where M : Monad<M> => value.Bind(halve);
    public static K<M, int> Total<M>(K<M, int> first, K<M, int> second) where M : Monad<M> =>
        from a in first
        from b in second
        select a + b;
    public static int Sum<T>(K<T, int> values) where T : Foldable<T> => values.Fold(0, static (total, v) => total + v);
    public static Seq<int> Reversed<T>(K<T, int> values) where T : Foldable<T> => values.FoldBack(Seq<int>(), static (acc, v) => acc.Add(v));
    public static bool AnyNegative<T>(K<T, int> values) where T : Foldable<T> => values.Exists(static v => v < 0);
    public static bool AllPositive<T>(K<T, int> values) where T : Foldable<T> => values.ForAll(static v => v > 0);
    public static Option<int> Second<T>(K<T, int> values) where T : Foldable<T> => values.At(1);
    public static K<Option, K<T, int>> ParseAll<T>(K<T, string> values) where T : Traversable<T> => T.Traverse(static s => parseInt(s), values);
    public static K<IO, K<T, int>> ReadAll<T>(K<T, string> values, Func<string, IO<int>> read) where T : Traversable<T> => T.TraverseM(read, values);
    public static K<F, int> Reject<F>() where F : Fallible<F> => F.Fail<int>(new Rejected());
    public static K<F, int> Recovered<F>(K<F, int> value) where F : Fallible<F>, Applicative<F> =>
        value.Catch(static error => error.HasCode(2301), static _ => F.Pure(0));
    public static K<M, int> Factor<M>() where M : Readable<M, Settings>, Monad<M> => Readable.ask<M, Settings>().Map(static s => s.Factor);
    public static K<M, int> Scaled<M>(int value) where M : Readable<M, Settings> => Readable.asks<M, Settings, int>(s => s.Factor * value);
    public static K<M, int> ScaledTwice<M>(K<M, int> operation) where M : Readable<M, Settings> =>
        Readable.local<M, Settings, int>(static s => s with { Factor = s.Factor * 2 }, operation);
    public static K<M, int> Tick<M>() where M : Stateful<M, int>, Monad<M> =>
        from current in Stateful.get<M, int>()
        from _ in Stateful.put<M, int>(current + 1)
        select current;
    public static K<M, Unit> Reset<M>() where M : Stateful<M, int> => Stateful.modify<M, int>(static _ => 0);
    public static K<M, int> Counted<M>() where M : Stateful<M, int>, Monad<M> => Stateful.state<M, int, int>(static n => (n, n + 1));
    public static K<M, int> Isolated<M>(K<M, int> operation) where M : Stateful<M, int>, Monad<M> => Stateful.local<M, int, int>(static n => n + 100, operation);
    public static K<M, Unit> Note<M>(string message) where M : Writable<M, Seq<string>> => Writable.tell<M, Seq<string>>(Seq(message));
    public static K<F, int> Nothing<F>() where F : Alternative<F> => F.Empty<int>();
    public static K<F, int> FirstOf<F>(K<F, int> first, K<F, int> second) where F : Alternative<F> => F.Choose(first, second);
    public static Option<int> Chosen(Option<int> first, Option<int> second) => first | second;
}
```

<!-- Integrated into .claude/skills/dotnet-languageext/SKILL.md
## [03]-[LAWS]

`FunctorLaw<F>.validate(fa)`, `ApplicativeLaw<F>.validate()`, and `MonadLaw<F>.validate()` return `Validation<Error, Unit>`. Failed laws contain an accumulated `Error`, and `IsSuccess` indicates the result. The checks hold for `Option` and `Fin`. `MonadLaw<IO>.validate()` throws inside the library and is not run.
-->

```csharp
internal static class Laws {
    public static Validation<Error, Unit> OptionFunctor => FunctorLaw<Option>.validate(Some(1));
    public static Validation<Error, Unit> OptionApplicative => ApplicativeLaw<Option>.validate();
    public static Validation<Error, Unit> OptionMonad => MonadLaw<Option>.validate();
    public static Validation<Error, Unit> FinFunctor => FunctorLaw<Fin>.validate(Pure(1).ToFin());
    public static Validation<Error, Unit> FinApplicative => ApplicativeLaw<Fin>.validate();
    public static Validation<Error, Unit> FinMonad => MonadLaw<Fin>.validate();
}
```

<!-- Integrated into .claude/skills/dotnet-languageext/SKILL.md
## [04]-[TRANSFORMERS]

Transformers stack one concern over an inner monad `M`. `OptionT<M, A>` holds `K<M, Option<A>>`. `FinT<M, A>` holds `K<M, Fin<A>>` and exposes it as `runFin`. `EitherT<L, M, A>` holds `K<M, Either<L, A>>`. `ValidationT<Error, IO, A>` accumulates inside an effect and is used only when errors must accumulate inside that effect. `ReaderT<Env, M, A>` holds `Func<Env, K<M, A>>`. `WriterT<W, M, A>` accumulates `W` beside the value, and `tell` appends one item. `RWST<R, W, S, M, A>` combines `ask`, `tell`, `get`, and `put` over one `M`.

`lift` adds a transformer layer to an evaluated value: `Fin<A>`, `Either<L, A>`, `Validation<Error, A>`, or the inner monad's `K<M, A>`. `liftIO` passes an `IO<A>` through every layer to the `IO` at the bottom of the stack. `Run` removes one layer, and the host runs the layers from the outside in. `Priced` shows the order: `Run(settings)` yields `K<OptionT<IO>, int>`, the second `Run()` yields `K<IO, Option<int>>`, and `RunSafe` yields `Fin<Option<int>>`. The concrete transformer determines what `Run` returns: `ReaderT` applies its function, and `OptionT` returns the wrapped value.

`Settled` converts the `Fin` from `runFin` to an `IO` result with `IO.lift(Fin<A>)`, preserving a rejection as a typed `Expected` error. `Both` combines two `ValidationT` values with the tuple `Apply`, and both effects run before the errors accumulate.

`Counter<A>` is a domain wrapper around `StateT<int, IO, A>` and implements `K<Counter, A>`. The witness `Counter` implements `Deriving.Monad<Counter, StateT<int, IO>>` and `Deriving.Stateful<Counter, StateT<int, IO>, int>` with `Transform` and `CoTransform` alone. `Transform` unwraps to the stack and `CoTransform` wraps the result. Functions constrained on `Monad<M>` or `Stateful<M, int>` run on `Counter`, including `Tick<Counter>`. `Deriving.MonadIO` requires the stack to implement `MonadIO`. Because `StateT` does not, the wrapper derives only `Monad` and `Stateful`. The wrapper lifts an effect through `CoTransform` over `StateT.liftIO` because the stack supplies no `LiftIO`.
-->

```csharp
internal sealed record Account(Guid Id, decimal Balance);
internal sealed record Guest(string Name);
internal sealed record Member(int Id);
internal sealed record Overdrawn() : Expected("account overdrawn", 2302);
internal sealed record EmptyName() : Expected("name is empty", 2303);

internal static class Transformers {
    public static OptionT<IO, decimal> Converted(OptionT<IO, Account> lookup, IO<decimal> rate) =>
        from account in lookup
        from factor in OptionT.liftIO<IO, decimal>(rate)
        select account.Balance * factor;
    public static Fin<Account> Charge(Account account, decimal amount) =>
        account.Balance >= amount ? account with { Balance = account.Balance - amount } : new Overdrawn();
    public static FinT<IO, Account> Charged(IO<Account> load, decimal amount) =>
        from account in FinT.liftIO<IO, Account>(load)
        from charged in FinT.lift<IO, Account>(Charge(account, amount))
        select charged;
    public static IO<Account> Settled(FinT<IO, Account> charged) => charged.runFin.As().Bind(static fin => IO.lift(fin));
    public static Either<Guest, Member> Classify(int id, string name) => id > 0 ? Right(new Member(id)) : Left(new Guest(name));
    public static EitherT<Guest, IO, Member> Visitor(IO<int> loadId, string name) =>
        from id in EitherT.liftIO<Guest, IO, int>(loadId)
        from member in EitherT.lift<Guest, IO, Member>(Classify(id, name))
        select member;
    public static Validation<Error, string> ValidName(string name) => string.IsNullOrWhiteSpace(name) ? new EmptyName() : name;
    public static ValidationT<Error, IO, string> Checked(IO<string> loadName) =>
        from name in ValidationT.liftIO<Error, IO, string>(loadName)
        from valid in ValidationT.lift<Error, IO, string>(ValidName(name))
        select valid;
    public static ValidationT<Error, IO, string> Both(IO<string> first, IO<string> second) =>
        (Checked(first), Checked(second)).Apply(static (a, b) => a + b).As();
    public static ReaderT<Settings, OptionT<IO>, int> Priced(Option<int> quantity, IO<int> unitPrice) =>
        from settings in ReaderT.ask<OptionT<IO>, Settings>()
        from count in ReaderT.lift<Settings, OptionT<IO>, int>(OptionT.lift<IO, int>(quantity))
        from price in ReaderT.liftIO<Settings, OptionT<IO>, int>(unitPrice)
        select count * price * settings.Factor;
    public static Fin<Option<int>> Run(ReaderT<Settings, OptionT<IO>, int> priced, Settings settings) =>
        priced.Run(settings).As().Run().As().RunSafe();
    public static WriterT<Seq<string>, IO, int> Audited(IO<int> load) =>
        from value in WriterT.liftIO<Seq<string>, IO, int>(load)
        from _ in WriterT.tell<Seq<string>, IO>(Seq(string.Create(CultureInfo.InvariantCulture, $"loaded {value}")))
        select value;
    public static Fin<(int Value, Seq<string> Output)> Run(WriterT<Seq<string>, IO, int> audited) => audited.Run().As().RunSafe();
    public static RWST<Settings, Seq<string>, int, IO, int> Stepped =>
        from settings in RWST.ask<Settings, Seq<string>, int, IO>()
        from count in RWST.get<Settings, Seq<string>, int, IO>()
        from _ in RWST.put<Settings, Seq<string>, int, IO>(count + 1)
        from __ in RWST.tell<Settings, Seq<string>, int, IO>(Seq("stepped"))
        select count * settings.Factor;
}

internal sealed record Counter<A>(StateT<int, IO, A> Step) : K<Counter, A>;

internal abstract class Counter : Deriving.Monad<Counter, StateT<int, IO>>, Deriving.Stateful<Counter, StateT<int, IO>, int> {
    public static K<StateT<int, IO>, A> Transform<A>(K<Counter, A> fa) => fa.As().Step;
    public static K<Counter, A> CoTransform<A>(K<StateT<int, IO>, A> fa) => new Counter<A>(fa.As());
}

internal static class CounterExtensions {
    public static Counter<A> As<A>(this K<Counter, A> ma) => (Counter<A>)ma;
    public static Fin<(int Value, int State)> Exit(this Counter<int> counter, int seed) => counter.Step.Run(seed).As().RunSafe();
}
internal static class Counters {
    public static Counter<int> Increment =>
        (from current in Stateful.get<Counter, int>()
         from _ in Stateful.put<Counter, int>(current + 1)
         select current).As();
    public static Counter<A> Lifted<A>(IO<A> effect) => Counter.CoTransform(StateT.liftIO<int, IO, A>(effect)).As();
}
```

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [05]-[TRAVERSAL_POLICY]

The dependency structure and the concurrency bound determine the traversal.

| [INDEX] | [SCENARIO]                   | [TRAVERSAL]                             | [BEHAVIOR]                                                                               |
| :-----: | :--------------------------- | :-------------------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | Independent checks           | Instance `Traverse` under `Validation`  | Accumulates every error                                                                  |
|  [02]   | Independent effects          | Instance `Traverse` under `IO`          | Asynchronous effects overlap without a bound, and the traversal fails if an effect fails |
|  [03]   | Dependent or ordered effects | Instance `TraverseM`                    | Serial, short-circuit on the first failure                                               |
|  [04]   | Bounded concurrency          | Chunk, then `TraverseM` over the chunks | One chunk runs at a time, and the chunk width sets the bound                             |
|  [05]   | Best effort                  | `PartitionFallible`, `Succs`, `Fails`   | No short-circuit, both branches returned                                                 |

`PartitionFallible` returns `(Seq<Error> Fails, Seq<A> Succs)` with the failures first. `Succs` keeps the successes and `Fails` keeps the errors. `PartitionFallible`, `Succs`, and `Fails` take a `Seq<K<IO, A>>`. Their projections use `Map<K<IO, A>>` to specify the result type.

```csharp
internal sealed record Job(string Name, IO<int> Work);

internal static class Traversals {
    public static Validation<Error, Seq<int>> Accumulated(Seq<string> values, Func<string, Validation<Error, int>> check) => values.Traverse(check).As();
    public static Validation<Error, Seq<int>> FirstFailure(Seq<string> values, Func<string, Validation<Error, int>> check) => values.TraverseM(check).As();
    public static IO<Seq<int>> Overlapped(Seq<Job> jobs) => jobs.Traverse(static job => job.Work).As();
    public static IO<Seq<int>> Serial(Seq<Job> jobs) => jobs.TraverseM(static job => job.Work).As();
    public static IO<Seq<int>> Bounded(Seq<Job> jobs, int width) => toSeq(jobs.Chunk(width)).Map(toSeq).TraverseM(Overlapped).As().Map(static groups => groups.Flatten());
    public static IO<(Seq<Error> Fails, Seq<int> Succs)> BestEffort(Seq<Job> jobs) => jobs.Map<K<IO, int>>(static job => job.Work).PartitionFallible().As();
    public static IO<Seq<int>> Completed(Seq<Job> jobs) => jobs.Map<K<IO, int>>(static job => job.Work).Succs().As();
    public static IO<Seq<Error>> Failed(Seq<Job> jobs) => jobs.Map<K<IO, int>>(static job => job.Work).Fails().As();
}
```
-->
