# WriterT: Monoidal Output as State

`WriterT` accumulates output during a monadic expression. Its output type `W` is constrained by `Monoid<W>`, so it has an empty value and can combine output values. This makes the transformer useful for logging multiple outputs or building one aggregate output with pure expressions.

Operationally, `WriterT` is the same as `StateT`: the state type `S` is renamed to the output type `W` and constrained to be a monoid. The distinction remains useful because `WriterT` declares that the threaded value is accumulated output, not arbitrary state.

## The classic Writer design

A direct `Writer` representation returns both an output and a value:

```csharp
public record Writer<W, A>(Func<(W Output, A Value)> runWriter)
    where W : Monoid<W>
{
    public Writer<W, B> Bind<B>(Func<A, Writer<W, B>> f) =>
        new(() =>
        {
            var (output1, value1) = runWriter();
            var (output2, value2) = f(value1).runWriter();
            return (output1.Combine(output2), value2);
        });
}
```

`Bind` runs the first computation, uses its value to select the next one, then combines the two returned outputs. In this representation, `tell` only needs to return the supplied output alongside `Unit`:

```csharp
public static class Writer
{
    public static Writer<W, Unit> tell<W>(W output)
        where W : Monoid<W> =>
        new(() => (output, unit));
}
```

Individual outputs can then be accumulated in a query expression:

```csharp
static Writer<Seq<string>, Unit> example =>
    from _1 in tell(Seq("Hello"))
    from _2 in tell(Seq("World"))
    select unit;
```

## Why combining in Bind is costly

`Bind` is normally called far more often than `tell`, yet the classic design calls `Combine` for every bind. This causes two problems:
- one or both outputs are often empty, so their combination is wasted work;
- non-empty outputs may be expensive to combine.

For example, concatenating two immutable linked lists of 100 items can require a 100-item traversal to build a new list. Repeatedly combining growing immutable outputs can therefore rebuild the same elements many times and become very expensive.

The combination should happen in `tell`, where output is deliberately added, instead of in every `Bind`.

## Thread the output through the computation

Change the runner from a function with no input:

```csharp
Func<(W Output, A Value)> runWriter
```

to a function that receives the accumulated output:

```csharp
Func<W, (W Output, A Value)> runWriter
```

`Bind` can now pass each updated output to the next computation without combining anything:

```csharp
public record Writer<W, A>(Func<W, (W Output, A Value)> runWriter)
{
    public Writer<W, B> Bind<B>(Func<A, Writer<W, B>> f) =>
        new(output0 =>
        {
            var (output1, value1) = runWriter(output0);
            var (output2, value2) = f(value1).runWriter(output1);
            return (output2, value2);
        });
}
```

This is the same mechanism as `State`: it threads a value through a computation and returns the updated value. `tell` now performs the output combination:

```csharp
public static Writer<W, Unit> tell<W>(W value)
    where W : Monoid<W> =>
    new(output => (output.Combine(value), unit));
```

Writer output is commonly a collection, so `tell` usually appends or prepends a single item. That avoids repeatedly concatenating whole accumulated collections, provided the monoid itself combines values efficiently.

The revised `Writer<W, A>` no longer needs a `Monoid<W>` constraint. Only `tell` combines values, so only `tell` needs the constraint.

## Express Writer behavior with StateT

Because the revised implementation is otherwise identical to `State`, a `WriterT` operation can be expressed with `StateT.modify`:

```csharp
public static StateT<W, M, Unit> tell<M, W>(W value)
    where W : Monoid<W>
    where M : Monad<M> =>
    StateT.modify<M, W>(output => output.Combine(value));
```

The operation can also work with any type that implements `Stateful<M, W>`:

```csharp
public static K<M, Unit> tell<M, W>(W value)
    where W : Monoid<W>
    where M : Stateful<M, W> =>
    Stateful.modify<M, W>(output => output.Combine(value));
```

Any such computation can aggregate output using the `Monoid<W>` operation. Dedicated `Writer` and `WriterT` types are still worth keeping because their names communicate the intended role of the threaded value.

## RWST

`ReaderT`, `WriterT`, and `StateT` can be stacked over a base monad `M`:

```csharp
public record RWST<R, W, S, M, A>(
    ReaderT<R, WriterT<W, StateT<S, M>>, A> runRWS)
    : K<RWST<R, W, S, M>, A>
    where M : Monad<M>
    where W : Monoid<W>;
```

The stack combines four behaviors: reading configuration through `ReaderT`, aggregating output through `WriterT`, carrying state through `StateT`, and lifting a base monad such as `IO` or `Option`. This makes it suitable as an application monad that needs all four behaviors.

Its trait witness exposes the capabilities already provided by the wrapped types:

```csharp
public class RWST<R, W, S, M> :
    MonadT<RWST<R, W, S, M>, M>,
    Readable<RWST<R, W, S, M>, R>,
    Writable<RWST<R, W, S, M>, W>,
    Stateful<RWST<R, W, S, M>, S>
    where M : Monad<M>
    where W : Monoid<W>
{
    // Lift each existing behavior into the wrapper-transformer.
}
```

The wrapped types already implement the required behaviors. `RWST` only needs to lift those behaviors into its own wrapper rather than reimplement them.
