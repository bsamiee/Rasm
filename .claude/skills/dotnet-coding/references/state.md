# [STATE]

Covers values that depend on earlier values without a mutable variable, from the transition function to the sequence that threads it.

## [01]-[RUNNING_STATE]

A transition `Func<S, A, S>` receives the current state and one input and returns the successor, and the consumer's read selects the operator: `Fold` for the final state, `Scan` for every state, and `FoldM` for a transition `Func<S, A, IO<S>>` whose effects run in input order. Zipping the inputs with their `Scan` pairs each input with the state before it, the extra final state falls off the end, and `Tail` shifts the pairing to the state after each input:

```csharp
internal static class Running {
    public static Seq<(double Width, double Start)> Placed(Seq<double> widths, double origin = 0d, double gap = 0d) =>
        widths.Zip(widths.Scan(origin, (start, width) => start + width + gap), static (width, start) => (Width: width, Start: start));
    public static Option<int> Settled(Seq<int> deltas, int start = 0) =>
        deltas.Scan(start, static (sum, delta) => sum + delta).Tail.Choose(static (at, sum) => sum is 0 ? Some(at) : None).Head;
}
```

`Settled` answers the index of the first input after which the running sum returns to zero, and `Head` stops the `Scan` at that state. The state can be any immutable value: a `Seq<A>` path trimmed with `Take(depth)` and extended with `Add` threads the ancestry of a flattened outline the same way a sum threads an offset.

## [02]-[UNFOLDING]

A state that advances from itself, with no input sequence and a step count no one knows in advance, unfolds from a seed through `LanguageExt.List.unfold`, and the step's return type selects what the sequence yields:
- `Func<S, Option<S>>` yields the seed and every successor, and the first `None` ends the sequence after the last state
- `Func<S, Option<(A, S)>>` yields a value apart from the state and never the seed, and a seed whose step returns `None` yields an empty sequence
- Tuple seeds up to four elements pass the state as separate parameters and return `Option<(A, S1, S2, ...)>`

```csharp
internal static class Unfolded {
    public static Seq<int> Levels(int size) =>
        toSeq(LanguageExt.List.unfold(size, static level => level > 1 ? Some(level / 2) : None));
    public static Seq<int> Digits(int value, int radix = 10) =>
        toSeq(LanguageExt.List.unfold(value, rest => rest is 0 ? None : Some((rest % radix, rest / radix))));
}
```

`Digits` yields the least significant digit first.
