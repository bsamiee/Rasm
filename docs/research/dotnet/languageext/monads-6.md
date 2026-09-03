<!-- Ideas integrated into .claude/skills/dotnet-languageext/references/traits-and-transformers.md on the real types, the toy Maybe, List, and MaybeT code enters no skill
# [MONADS]

## [01]-[MOTIVATION]

Pure functional programming rests on: functions are values, everything is an expression, and expressions are pure or behave with referential transparency. Pure expressions are values or combine pure expressions and produce no side effects. Referentially transparent expressions can use imperative implementations, from the outside each call is replaceable by its value.

Expression-oriented programs still need readable, line-by-line sequencing. In an ML-style expression,

```text
let x = doSomething () in
let y = doSomethingElse () in
x + y
```

the apparent statements are nested function applications. They keep expression scoping and compose and reduce as expressions.

Purity creates a second need: useful programs read clocks, files, and other external state. `DateTime.Now` cannot be replaced with one evaluated value, it is not pure. The effect must be represented without being performed while the expression is constructed. One impure operation compromises the purity of the expression that contains it, effects stay explicit and controlled.

Monads answer both needs:
- They represent effects as values, pure code composes effectful computations
- They sequence computations when a later computation depends on an earlier result
- With LINQ, a pure expression reads like a series of imperative statements

Monads are a pattern for sequencing computations in a context. Its central operation is `Bind`:

```text
Bind : M<A> -> (A -> M<B>) -> M<B>
```

The function receives a value from one contextual computation and returns the next computation in the same context. The `Bind` implementation determines what happens between the steps.

## [02]-[IO_AS_A_COMPUTATION]

Reading the clock yields a time snapshot. Capturing the read as a function represents the computation itself:

```csharp
public record IO<A>(Func<A> runIO);

public static class DateTimeIO
{
    public static readonly IO<DateTime> Now =
        new(() => DateTime.Now);
    public static readonly IO<DateTime> Today =
        new(() => DateTime.Today);
    public static readonly IO<DateTime> Tomorrow =
        new(() => DateTime.Today.AddDays(1));
}
```

`DateTimeIO.Now` constructs a data representation of a computation, it does not read the clock. Each run of that representation reads the clock again, instead of preserving the first observed time.

For the higher-kinded trait system, `IO<A>` implements the `IO` witness and gains the downcast and the interpreter:

```csharp
public record IO<A>(Func<A> runIO) : K<IO, A>;

public static class IOExtensions
{
    public static IO<A> As<A>(this K<IO, A> ma) => (IO<A>)ma;
    public static A Run<A>(this K<IO, A> ma) => ma.As().runIO();
}
```

## [03]-[FROM_APPLICATIVE_TO_MONAD]

`Map` works within the lifted `IO` space. It runs the existing computation only when the returned computation is interpreted, transforms its result, and wraps the operation in another `IO`:

```csharp
public class IO : Functor<IO>
{
    public static K<IO, B> Map<A, B>(
        Func<A, B> f,
        K<IO, A> ma) =>
        new IO<B>(() => f(ma.Run()));
}
```

This builds a computation for tomorrow's current time without reading the clock:

```csharp
var thisTimeTomorrow = DateTimeIO.Now.Map(now => now.AddDays(1));
```

Applicatives add `Pure` and `Apply`:

```csharp
public class IO : Applicative<IO>
{
    public static K<IO, B> Map<A, B>(
        Func<A, B> f,
        K<IO, A> ma) =>
        new IO<B>(() => f(ma.Run()));
    public static K<IO, A> Pure<A>(A value) =>
        new IO<A>(() => value);
    public static K<IO, B> Apply<A, B>(
        K<IO, Func<A, B>> mf,
        K<IO, A> ma) =>
        mf.Map(f => f(ma.Run()));
}
```

The monad adds `Bind`:

```csharp
public class IO : Monad<IO>
{
    public static K<IO, B> Bind<A, B>(
        K<IO, A> ma,
        Func<A, K<IO, B>> f) =>
        new IO<B>(() => f(ma.Run()).Run());
    public static K<IO, B> Map<A, B>(
        Func<A, B> f,
        K<IO, A> ma) =>
        Bind(ma, x => Pure(f(x)));
    public static K<IO, A> Pure<A>(A value) =>
        new IO<A>(() => value);
    public static K<IO, B> Apply<A, B>(
        K<IO, Func<A, B>> mf,
        K<IO, A> ma) =>
        mf.Bind(ma.Map);
}
```

The critical expression is:

```csharp
f(ma.Run()).Run()
```

When the composed `IO` is interpreted, it runs `ma`, gives the resulting `A` to `f`, then runs the returned `IO<B>`. The second computation depends on the first result.

`Map` and `Bind` differ in the function they accept:

```text
Map  : (A -> B)    -> M<A> -> M<B>
Bind : (A -> M<B>) -> M<A> -> M<B>
```

`Map` is `Bind` followed by `Pure`: its function produces a plain value and ends the chain. `Apply` derives from `Bind` and `Map`. Those definitions are starting points, a monad can provide a bespoke `Apply`.

## [04]-[LINQ_SEQUENCING]

Nested `Bind` calls form a pyramid:

```csharp
var diff = DateTimeIO.Today.Bind(
    today => DateTimeIO.Tomorrow.Bind(
        tomorrow => IO.Pure(tomorrow - today)));
```

C# query syntax expresses the same dependent composition without the nesting:

```csharp
var diff =
    from today in DateTimeIO.Today
    from tomorrow in DateTimeIO.Tomorrow
    select tomorrow - today;
```

LINQ is C#'s first-class syntax for monadic sequencing, the analogue of Haskell's `do` notation. With the higher-kinded traits, implementing `Monad<M>` makes a type work with LINQ without a separate `Select` and `SelectMany` per concrete monad. Monads chain computations serially, read `Bind` as "and then".

## [05]-[BIND_ACROSS_KINDS]

`Monad<M>` extends `Applicative<M>` with one operation:

```csharp
public interface Monad<M> : Applicative<M>
    where M : Monad<M>
{
    public static abstract K<M, B> Bind<A, B>(
        K<M, A> ma,
        Func<A, K<M, B>> f);
}
```

`Bind` moves from `M<A>` to its contained `A` under rules the monad knows, then applies `A -> M<B>`. The composition stays lifted:

```text
M<A> -> A -> M<B>
```

No general operation has the shape `M<A> -> A`. Contexts can contain no `A`: `Option<A>` can be `None`. Lowering a discriminated union to a plain value is type-specific and needs pattern matching or a default. `Bind` still produces `M<B>`, because the monad knows how to preserve its no-value case. This monad-specific logic between computations is the "programmable semicolon": for `IO` it runs a deferred computation, for `Option` it decides whether the next computation runs.

## [06]-[OPTION_SHORT_CIRCUIT]

`Option<A>` continues from `Some` and preserves `None` without invoking the continuation:

```csharp
static K<Option, B> Bind<A, B>(
    K<Option, A> ma,
    Func<A, K<Option, B>> f) =>
    ma.Match(
        Some: x => f(x),
        None: Option<B>.None);
```

Failure propagation becomes part of composition:

```csharp
var res1 =
    from x in parseInt("100")
    from y in parseInt("200")
    select x + y;

var res2 =
    from x in parseInt("100")
    from y in parseInt("NUL")
    from z in parseInt("200")
    select x + y + z;

// res1 == Some(300)
// res2 == None; the z step and final select do not run
```

When parsing returns `None`, no value exists for the next dependent step, and the rest of the expression stops. The type and its `Bind` make this control flow unavoidable, unlike a convention of scattered null checks.

## [07]-[DECLARED_EFFECTS]

The monad in a return type marks the whole expression with a behavior. `IO<A>` declares that the expression performs IO, `Option<A>` declares that it can produce no value. Visible contexts separate effectful from non-effectful code and preserve composition. Different monads implement very different sequencing behavior through the same `Bind` shape (IO, absence, state, configuration, logging, validation, collection iteration, stream processing, resource tracking). In every case `Bind` is function composition with the external result `M<A> -> M<B>`, the chosen `M` supplies the behavior between the steps.
-->
