# [TRAVERSABLES]

## [01]-[OPERATION]

`Map` preserves the nesting produced by an effectful transformation:

```csharp
Seq<Option<int>> mapped =
    Seq("1", "2", "X").Map(parseInt);
// [Some(1), Some(2), None]
```

`Traverse` transforms every value and flips the two structures:

```csharp
Option<Seq<int>> parsed =
    Seq("1", "2", "X")
        .Traverse(parseInt)
        .As();
// None
```

The general transformation is:

```text
Traverse : (A -> F<B>) -> T<A> -> F<T<B>>
```

- `T` is the traversed structure.
- `F` is the effect produced for each value.
- The behavior that combines results comes from `F`.

Use it whenever `Map` leaves effects inside a structure and the next operation needs one effect containing the complete structure. A traversal exposes no accumulator: the applicative's own behavior performs the accumulation while the structures flip.

## [02]-[OUTER_APPLICATIVE_SEMANTICS]

`Traverse` defines no universal failure or evaluation policy. It combines values through the selected `Applicative<F>`:

| [INDEX] | [TYPE]       | [BEHAVIOR]                                                                                  |
| :-----: | :----------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `Option`     | Any `None` makes the whole result `None`.                                                   |
|  [02]   | `Validation` | All failures are collected.                                                                 |
|  [03]   | `Eff`        | File-reading effects in the example run in parallel; one failure fails the whole traversal. |

Match the transformation to the dependency structure:

```csharp
Seq<string> paths = Seq(
    @"C:\input\a.txt",
    @"C:\input\b.txt",
    @"C:\input\c.txt");

// For this Eff example, Traverse evaluates the reads in parallel.
var parallel = paths.Traverse(File<Runtime>.readAllText);

// Seq's monadic variant evaluates them serially.
var serial = paths.TraverseM(File<Runtime>.readAllText);
```

## [03]-[TRAVERSE_AND_SEQUENCE]

`Sequence` is `Traverse` with the identity function, for input that is already nested:

```text
Traverse(f, T<A>)  -> F<T<B>>    where f : A -> F<B>
Sequence(T<F<A>>)  -> F<T<A>>
```

The trait supplies the derived operations:

```csharp
public static virtual K<F, K<T, A>> Sequence<F, A>(
    K<T, K<F, A>> ta)
    where F : Applicative<F> =>
    Traversable.traverse(x => x, ta);

public static virtual K<M, K<T, B>> TraverseM<M, A, B>(
    Func<A, K<M, B>> f,
    K<T, A> ta)
    where M : Monad<M> =>
    Traversable.traverse(f, ta);

public static virtual K<M, K<T, A>> SequenceM<M, A>(
    K<T, K<M, A>> ta)
    where M : Monad<M> =>
    Traversable.traverseM(x => x, ta);
```

`Sequence` and `SequenceM` perform the same structural flip; their constraints select applicative or monadic composition. A type overrides the defaults where it provides materially better semantics or performance; `Seq<A>` overrides `TraverseM` to guarantee serial evaluation.

### [03.1]-[PREFER_TRAVERSE]

C# cannot convert a concrete nested value such as `Seq<Option<int>>` to the nested higher-kinded form `K<Seq, K<Option, int>>`, so this fails at compile time:

```csharp
var values = Seq(Some(1), Some(2), None);
// values.Sequence();
```

The practical equivalent keeps the inner conversion visible to inference:

```csharp
Option<Seq<int>> result = values.Traverse(x => x).As();
```

Use `Sequence` when the input is already represented as `K<T, K<F, A>>`; otherwise use `Traverse(x => x)`.

## [04]-[HIGHER_KINDED_CONTRACT]

```csharp
public interface Traversable<T> : Functor<T>, Foldable<T>
    where T : Traversable<T>, Functor<T>, Foldable<T>
{
    static abstract K<F, K<T, B>> Traverse<F, A, B>(
        Func<A, K<F, B>> f,
        K<T, A> ta)
        where F : Applicative<F>;
}
```

Two independent type constructors participate:
- `T` maps over values and folds its shape.
- `F` lifts values with `Pure` and combines independent computations with `Apply`.
- `K<T, A>` is the traversed value in higher-kinded form.
- `K<F, K<T, B>>` is the transformed `T<B>` nested inside the chosen effect.

One `Traverse` implementation for a type composes with every applicative the trait system supports.

## [05]-[COLLECTION_TRAVERSABLE]

A sequence implementation folds from the back, starts with an empty sequence lifted into `F`, and applicatively prepends each transformed item:

```csharp
static K<F, K<Seq, B>> Traverse<F, A, B>(
    Func<A, K<F, B>> f,
    K<Seq, A> ta)
    where F : Applicative<F>
{
    K<F, Seq<B>> folded = Foldable.foldBack(
        cons,
        F.Pure(empty<B>()),
        ta);

    return F.Map<Seq<B>, K<Seq, B>>(xs => xs, folded);

    K<F, Seq<B>> cons(K<F, Seq<B>> xs, A x) =>
        Applicative.lift(Prelude.Cons, f(x), xs);
}
```

- `foldBack` uses `Cons` to prepend each mapped item to the accumulated sequence.
- `F.Pure(empty<B>())` is the result for an empty input.
- `Applicative.lift` combines `f(x)` and the accumulated sequence through `F.Apply`; failure accumulation, short-circuiting, or concurrency enters here.
- The final `F.Map` widens the concrete inner `Seq<B>` to `K<Seq, B>` explicitly.

Keeping `Seq<B>` concrete during the fold avoids an `.As()` and `.Kind()` call per item; one outer `Map` costs less than repeated boxing of a value-type collection. Prefer the explicit widening over variance tricks that compile and then fail through nested runtime casts.

## [06]-[ALTERNATIVE_TRAVERSABLE]

For a type with success and failure cases, traverse only the case that contains a transformable value:

```csharp
static K<F, K<Either<L>, B>> Traverse<F, A, B>(
    Func<A, K<F, B>> f,
    K<Either<L>, A> value)
    where F : Applicative<F> =>
    value switch
    {
        Either.Right<L, A>(var right) =>
            F.Map<B, K<Either<L>, B>>(
                x => Either<L, B>.Right(x),
                f(right)),

        Either.Left<L, A>(var left) =>
            F.Pure<K<Either<L>, B>>(Either<L, B>.Left(left)),

        _ => throw new NotSupportedException()
    };
```

The `Left` branch does not call `f`; it preserves the existing structure and lifts it into `F`. The `Right` branch runs `f` once and maps the result back into `Either`.

## [07]-[CONCRETE_RETURN_TYPES]

The generic extension returns two nested `K` interfaces. Concrete member methods on a traversable type resolve before the generic extensions, so only the outer layer stays abstract:

```csharp
public readonly struct Seq<A>
{
    public K<F, Seq<B>> Traverse<F, B>(Func<A, K<F, B>> f)
        where F : Applicative<F> =>
        F.Map(x => x.As(), Traversable.traverse(f, this));

    public K<M, Seq<B>> TraverseM<M, B>(Func<A, K<M, B>> f)
        where M : Monad<M> =>
        M.Map(x => x.As(), Traversable.traverseM(f, this));
}
```

Call `.As()` once more where the outer concrete type is needed:

```csharp
Option<Seq<int>> parsed =
    Seq("10", "20", "30")
        .Traverse(parseInt)
        .As();
```

The repeated member pattern answers C#'s handling of nested generic interfaces; it does not change traversal semantics. Operations defined for `K`, such as `Map` and `Apply`, stay available before that conversion. A domain type implements `Traverse` once and composes with the available foldable and applicative types, instead of one method per pairing.
