<!-- Ideas integrated into .claude/skills/dotnet-coding-languageext/references/traits-and-transformers.md on the real types, the toy Maybe, List, and MaybeT code enters no skill
# [APPLICATIVES]

Applicative functors extend a functor with contextual function application. They combine values in a context (`Maybe`, `Either`, `Option`, `Seq`, `IO`) without the sequential dependencies of monadic composition.

Important uses are:
- Evaluating independent effectful computations in parallel
- Collecting multiple validation errors

## [01]-[FROM_MAP_TO_APPLY]

`Map` lifts a unary function over a contextual value:

```text
Func<A, B>  ->  Func<F<A>, F<B>>
```

That covers a unary operation:

```csharp
static bool Not(bool x) => !x;

var xs = new List<bool>([true, false, true]);
var ys = xs.Map(Not);

var mx = new Just<bool>(true);
var my = mx.Map(Not);
```

Currying converts a multi-argument function into unary stages:

```csharp
static int Add(int x, int y) => x + y;
static int Multiply(int x, int y) => x * y;

var add = curry<int, int, int>(Add);
var multiply = curry<int, int, int>(Multiply);

var add10 = add(10);      // Func<int, int>
var result = add10(20);   // 30
```

Partial application lets arguments arrive one at a time. When the function passed to `Map` returns another contextual value, `Map` wraps that result in the outer context and creates nesting:

```csharp
var mw = new Just<int>(1);
var mx = new Just<int>(2);
var my = new Just<int>(3);
var mz = new Just<int>(4);

K<Maybe, Func<int, int>> r1 = mw.Map(multiply);
var r2 = r1.Map(f => mx.Map(f));
// K<Maybe, K<Maybe, int>>
```

Calculating `1 * 2 + 3 * 4` this way compounds the nesting:

```csharp
var lhs = mw.Map(multiply).Map(f => mx.Map(f));
var rhs = my.Map(multiply).Map(f => mz.Map(f));
var res = lhs.Map(l => l.Map(add).Map(f => rhs.Map(r => r.Map(f))));

// K<Maybe, K<Maybe, K<Maybe, K<Maybe, int>>>>
```

`Apply` combines a contextual function with a contextual argument and preserves a single contextual layer.

## [02]-[TRAIT]

```csharp
public interface Applicative<F> : Functor<F>
    where F : Applicative<F>
{
    public static abstract K<F, A> Pure<A>(A value);
    public static abstract K<F, B> Apply<A, B>(
        K<F, Func<A, B>> mf,
        K<F, A> ma);
}
```

Compare `Apply` with `Map`:

```csharp
public interface Functor<F>
    where F : Functor<F>
{
    public static abstract K<F, B> Map<A, B>(
        Func<A, B> f,
        K<F, A> ma);
}
```

The difference is the function. `Map` receives an ordinary `Func<A, B>`, `Apply` receives that function inside the same `K<F, ...>` context as its argument. This `Map` signature uses LanguageExt's function-first ordering. The trait also declares `Pure`, the `Maybe` instance below constructs `Just`.

## [03]-[MAYBE_APPLICATIVE]

```csharp
public class Maybe :
    Foldable<Maybe>,
    Applicative<Maybe>
{
    public static K<Maybe, A> Pure<A>(A value) => new Just<A>(value);
    public static K<Maybe, B> Apply<A, B>(
        K<Maybe, Func<A, B>> mf,
        K<Maybe, A> ma) =>
        mf switch
        {
            Just<Func<A, B>>(var f) => ma switch
            {
                Just<A>(var a) => new Just<B>(f(a)),
                Nothing<A> => new Nothing<B>()
            },
            Nothing<Func<A, B>> => new Nothing<B>()
        };
    public static K<Maybe, B> Map<A, B>(
        Func<A, B> f,
        K<Maybe, A> ma) =>
        ma switch
        {
            Just<A>(var a) => new Just<B>(f(a)),
            Nothing<A> => new Nothing<B>()
        };
    public static S FoldWhile<A, S>(
        Func<A, Func<S, S>> f,
        Func<(S State, A Value), bool> predicate,
        S state,
        K<Maybe, A> ta) =>
        ta switch
        {
            Just<A>(var a) when predicate((state, a)) => f(a)(state),
            _ => state
        };
    public static S FoldBackWhile<A, S>(
        Func<S, Func<A, S>> f,
        Func<(S State, A Value), bool> predicate,
        S state,
        K<Maybe, A> ta) =>
        Fold(s => a => f(a)(s), state, ta);
}
```

`Apply` extracts both the function and its argument. Either side `Nothing` makes the result `Nothing`, both `Just` invokes the function and wraps the result in `Just`. The `Foldable` trait requires `FoldWhile` and `FoldBackWhile`, which support optimized defaults for `Exists`, `ForAll`, and `IsEmpty`.

## [04]-[FLUENT_COMPOSITION]

Function-first `Map` and applicative `Apply` extensions enable a left-to-right form:

```csharp
public static K<F, B> Map<F, A, B>(
    this Func<A, B> f,
    K<F, A> ma)
    where F : Functor<F> =>
    F.Map<A, B>(f, ma);

public static K<F, B> Apply<F, A, B>(
    this K<F, Func<A, B>> mf,
    K<F, A> ma)
    where F : Applicative<F> =>
    F.Apply<A, B>(mf, ma);
```

The nested `Map` expression becomes:

```csharp
var r = multiply.Map(mw).Apply(mx); // K<Maybe, int>
```

`Map` supplies the first argument to the curried function, each `Apply` supplies another. Haskell writes the same operation with the `<$>` and `<*>` operators. C# operators cannot be parametrically polymorphic, the generic operation uses fluent methods:

```haskell
let r = multiply <$> mw <*> mx
```

The full arithmetic expression preserves its two independent multiplication branches:

```csharp
var lhs = multiply.Map(mw).Apply(mx);
var rhs = multiply.Map(my).Apply(mz);
var res = add.Map(lhs).Apply(rhs);
```

The equivalent monadic query needs no explicit currying:

```csharp
var res = from w in mw
          from x in mx
          from y in my
          from z in mz
          select w * x + y * z;
```

The distinction is evaluation structure. Monadic expressions are sequential: each operand arrives in order, and failure stops the remaining operations. Applicative expressions expose that their operands do not depend on one another, an applicative instance can evaluate independent branches concurrently.

## [05]-[PARALLEL_IO]

Simplified applicative `Apply` for `IO` forks both operands and awaits both results:

```csharp
public static IO<B> Apply<A, B>(
    this IO<Func<A, B>> ff,
    IO<A> fa) =>
    from tf in ff.Fork()
    from ta in fa.Fork()
    from f in tf.Await
    from a in ta.Await
    select f(a);
```

Chained `Apply` calls let each argument computation run in parallel, the final function runs after all arguments have been acquired:

```csharp
var io1 = liftIO(() => File.ReadAllTextAsync(path1));
var io2 = liftIO(() => File.ReadAllTextAsync(path2));
var io3 = liftIO(() => File.ReadAllTextAsync(path3));

var concat = (string txt1, string txt2, string txt3) => txt1 + txt2 + txt3;

IO<string> res = concat.Map(io1).Apply(io2).Apply(io3);
```

LanguageExt provides multi-argument `Map` and `Apply` overloads that curry delegates for the caller. Because `Map` is called on the ordinary `concat` function, only the `IO` arguments are forked.

The production `IO` represents `IO<A>` as a DSL, avoids `async` where possible, unpacks underlying `Task` values, and coordinates them with `Task.WhenAll`. The applicative meaning stands: automatic concurrency for independent `IO` operands.

Use applicatives where the independent structure provides a capability that sequential monadic composition does not, or where the fluent expression is clearer, otherwise the monadic form reads better in C#.
-->
