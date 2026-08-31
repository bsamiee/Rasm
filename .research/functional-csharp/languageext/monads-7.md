# Monads

## Why monads matter in pure functional C#

Pure functional programming rests on three ideas:
1. Functions are values.
2. Everything is an expression.
3. Expressions are pure or behave with referential transparency.

A pure expression is a value or is built entirely from pure expressions and produces no side effects. A pure function has a pure expression as its body and depends only on its arguments. A referentially transparent expression may use an imperative implementation internally, but from the outside it can still be replaced by its value without changing the program's behavior.

An expression-oriented program still needs readable, line-by-line sequencing. In an ML-style expression,

```text
let x = doSomething () in
let y = doSomethingElse () in
x + y
```

the apparent statements are nested function applications. They retain expression scoping and can be composed and reduced as expressions.

Purity creates a second problem: useful programs read clocks, files, and other external state. `DateTime.Now`, for example, cannot be replaced permanently with one evaluated value, so it is not pure. The effect must be represented without performing it while the surrounding expression is being constructed.

That discipline matters because pure functions are easier to reason about and test, simpler to compose, safer to optimize and parallelize, and less prone to unintended interactions when code changes. One impure operation compromises the purity of the expression that contains it, so effects must remain explicit and controlled.

Monads address both needs:
- They represent effects as values so effectful computations can be composed by pure code.
- They sequence computations when a later computation depends on an earlier result.
- With LINQ, they let a pure C# expression read like a series of imperative statements.

A monad is therefore a design pattern for sequencing computations in a context. Its central operation is `Bind`:

```text
Bind : M<A> -> (A -> M<B>) -> M<B>
```

The function receives a value from one contextual computation and returns the next computation in the same context. The particular implementation of `Bind` determines what happens between those steps.

## Representing IO as a computation

Reading the clock immediately yields a time snapshot. Capturing the read as a function instead represents the computation itself:

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

`DateTimeIO.Now` constructs a data representation of a computation. It does not read the clock. Reusing that representation evaluates the clock each time the computation is run rather than preserving the first observed time.

To use the higher-kinded trait system, make `IO<A>` implement the `IO` witness and provide the downcast and interpreter:

```csharp
public record IO<A>(Func<A> runIO) : K<IO, A>;

public static class IOExtensions
{
    public static IO<A> As<A>(this K<IO, A> ma) => (IO<A>)ma;
    public static A Run<A>(this K<IO, A> ma) => ma.As().runIO();
}
```

## From functor and applicative to monad

`Map` works within the lifted `IO` space. It runs the existing computation only when the returned computation is interpreted, transforms its result, and wraps the whole operation in another `IO`:

```csharp
public class IO : Functor<IO>
{
    public static K<IO, B> Map<A, B>(
        Func<A, B> f,
        K<IO, A> ma) =>
        new IO<B>(() => f(ma.Run()));
}
```

For example, this builds a computation for tomorrow's current time without reading the clock yet:

```csharp
var thisTimeTomorrow = DateTimeIO.Now.Map(now => now.AddDays(1));
```

An applicative adds `Pure` and `Apply`:

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

When the composed `IO` is interpreted, it runs `ma`, gives the resulting `A` to `f`, and then runs the returned `IO<B>`. The second computation can therefore depend on the first result.

`Map` and `Bind` differ in the function they accept:

```text
Map  : (A -> B)    -> M<A> -> M<B>
Bind : (A -> M<B>) -> M<A> -> M<B>
```

`Map` can be expressed as `Bind` followed by `Pure`: its supplied function produces a plain value and ends that chain. `Apply` can similarly be implemented from `Bind` and `Map`. Those definitions are useful starting points, although a monad may provide a bespoke `Apply`.

## LINQ flattens dependent sequencing

Directly nested `Bind` calls form a pyramid:

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

LINQ is C#'s first-class syntax for monadic sequencing, analogous to Haskell's `do` notation. With LanguageExt's higher-kinded traits, implementing `Monad<M>` makes the type work with LINQ without separately implementing `Select` and `SelectMany` for each concrete monad.

The conventional names are useful shared vocabulary. As an intuition aid, a monad can be thought of as "chainable" and `Bind` as "and then": computations are chained serially.

## `Bind` composes across kinds

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

Conceptually, `Bind` moves from the higher-kinded `M<A>` to its contained `A` according to rules known by that particular monad, then applies `A -> M<B>`. The complete composition stays lifted:

```text
M<A> -> A -> M<B>
```

There is no general operation `M<A> -> A`. A context may contain no `A` at all: `Option<A>` may be `None`. Lowering a discriminated union to a plain value is necessarily type-specific and usually needs pattern matching or a default. `Bind` can still produce `M<B>` because the monad knows how to preserve its no-value case.

This monad-specific logic between computations motivates the phrase "programmable semicolon." For `IO`, the logic runs a deferred computation. For `Option`, it decides whether the following computation may run.

## `Option` short-circuits dependent work

`Option<A>` continues from `Some` and preserves `None` without invoking the continuation:

```csharp
static K<Option, B> Bind<A, B>(
    K<Option, A> ma,
    Func<A, K<Option, B>> f) =>
    ma.Match(
        Some: x => f(x),
        None: Option<B>.None);
```

That policy makes failure propagation part of composition:

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

When parsing returns `None`, there is no value for the next dependent step, so the rest of the expression stops. Unlike a convention of scattered null checks, the type and its `Bind` implementation make this control flow unavoidable.

## Total functions make expected failure explicit

Expected failure belongs in the return type rather than an exception. A total function maps every allowed input to a value in its return type. Two techniques make this possible:
1. Constrain input types so out-of-range values cannot be constructed.
2. When valid inputs can still fail to produce the ordinary result, augment the return type with a failure or absence case.

For parsing, the type communicates the whole outcome:

```csharp
Option<int> parseInt(string value);
```

`Some<int>` represents a parsed value and `None` represents expected inability to parse. Exceptions remain for exceptional events from which the program cannot recover.

## Stay in the declared effect

The monad in a return type marks a whole expression with a particular behavior. `IO<A>` declares that the expression performs IO; `Option<A>` declares that it may produce no value. Keeping those contexts visible encourages separation between effectful and non-effectful code and preserves composition.

Do not call `Run` throughout an IO expression. Internal operations such as `Map` and `Bind` use it to define composition, but application code should remain in `IO` until an owning boundary such as `Main` or a web-request handler interprets the completed computation. Collapsing `IO<A>` to `A` earlier reintroduces the effect into otherwise pure code.

Different monads implement very different sequencing behavior through the same `Bind` shape. The mechanism can represent IO, absence, state, configuration, logging, validation, collection iteration, stream processing, resource tracking, and other behaviors. In every case, `Bind` is a form of function composition whose external result is `M<A> -> M<B>`; the chosen `M` supplies the behavior between the steps.
