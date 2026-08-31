# [APPLICATIVES]

An applicative functor extends a functor with contextual function application. It can combine values inside a context such as `Maybe`, `Either`, `Option`, `Seq`, or `IO` without imposing the sequential dependencies of monadic composition.

Two important uses in language-ext are:
- evaluating independent effectful computations in parallel;
- collecting multiple validation errors.

Applicative behavior is not limited to those cases. Higher-kinded types, including collection types, can provide applicative instances.

## [01]-[FROM_MAP_TO_APPLY]

A functor's `Map` lifts a unary function over a contextual value:

```text
Func<A, B>  ->  Func<F<A>, F<B>>
```

That is sufficient for a unary operation:

```csharp
static bool Not(bool x) => !x;

var xs = new List<bool>([true, false, true]);
var ys = xs.Map(Not);

var mx = new Just<bool>(true);
var my = mx.Map(Not);
```

Multi-argument functions can be converted into unary stages by currying:

```csharp
static int Add(int x, int y) => x + y;
static int Multiply(int x, int y) => x * y;

var add = curry<int, int, int>(Add);
var multiply = curry<int, int, int>(Multiply);

var add10 = add(10);      // Func<int, int>
var result = add10(20);   // 30
```

Partial application lets arguments arrive one at a time. But when the function passed to `Map` returns another contextual value, `Map` wraps that result in the outer context and creates nesting:

```csharp
var mw = new Just<int>(1);
var mx = new Just<int>(2);
var my = new Just<int>(3);
var mz = new Just<int>(4);

K<Maybe, Func<int, int>> r1 = mw.Map(multiply);
var r2 = r1.Map(f => mx.Map(f));
// K<Maybe, K<Maybe, int>>
```

Trying to calculate `1 * 2 + 3 * 4` this way compounds the problem:

```csharp
var lhs = mw.Map(multiply).Map(f => mx.Map(f));
var rhs = my.Map(multiply).Map(f => mz.Map(f));
var res = lhs.Map(l => l.Map(add).Map(f => rhs.Map(r => r.Map(f))));

// K<Maybe, K<Maybe, K<Maybe, K<Maybe, int>>>>
```

`Apply` combines a contextual function with a contextual argument while preserving a single contextual layer.

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

The difference is the function. `Map` receives an ordinary `Func<A, B>`; `Apply` receives that function inside the same `K<F, ...>` context as its argument. The `Map` signature shown here uses language-ext's function-first ordering.

The trait also declares `Pure`. In the `Maybe` instance below, `Pure` constructs `Just`.

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

`Apply` extracts both the function and its argument. If either is `Nothing`, the result is `Nothing`. If both are `Just`, it invokes the function and wraps the result in `Just`.

The `Foldable` trait requires `FoldWhile` and `FoldBackWhile`, which support optimized defaults for operations such as `Exists`, `ForAll`, and `IsEmpty`.

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

`Map` supplies the first argument to the curried function, and each `Apply` supplies another. Longer functions can continue the chain.

The same operation uses applicative operators in Haskell:

```haskell
let r = multiply <$> mw <*> mx
```

C# operators cannot be parametrically polymorphic, so the generic operation uses fluent methods instead.

The full arithmetic expression preserves its two independent multiplication branches:

```csharp
var lhs = multiply.Map(mw).Apply(mx);
var rhs = multiply.Map(my).Apply(mz);
var res = add.Map(lhs).Apply(rhs);
```

The equivalent monadic query is often easier to read in C# and does not require explicit currying:

```csharp
var res = from w in mw
          from x in mx
          from y in my
          from z in mz
          select w * x + y * z;
```

The distinction is evaluation structure. A monadic expression is sequential: each operand is obtained in order, and a failure can stop the remaining operations. An applicative expression exposes that its operands do not depend on one another, so an applicative instance may evaluate independent branches concurrently.

## [05]-[PARALLEL_IO]

A simplified applicative `Apply` for `IO` forks both contextual operands and waits for both results:

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

With more arguments, chained `Apply` calls allow each argument computation to run in parallel. The final function runs after all arguments have been acquired:

```csharp
var io1 = liftIO(() => File.ReadAllTextAsync(path1));
var io2 = liftIO(() => File.ReadAllTextAsync(path2));
var io3 = liftIO(() => File.ReadAllTextAsync(path3));

var concat = (string txt1, string txt2, string txt3) => txt1 + txt2 + txt3;

IO<string> res = concat.Map(io1).Apply(io2).Apply(io3);
```

language-ext provides multi-argument `Map` and `Apply` overloads that curry delegates automatically. Because `Map` is called on the ordinary `concat` function, only the three `IO` arguments are forked.

The real `IO` implementation is more complex than the simplified fork-and-await model. It represents `IO<A>` as a DSL, avoids `async` where possible, unpacks underlying `Task` values, and coordinates them with `Task.WhenAll`. The applicative meaning remains automatic concurrency for independent `IO` operands.

Applicatives are most useful when their independent structure provides a capability that sequential monadic composition does not, or when the fluent expression is clearer. In C#, the monadic form may otherwise be easier to read.
