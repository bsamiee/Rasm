# [RESULTS]

Worked flows for `Fin` workflows and recursive union folds.

## [01]-[WORKFLOWS]

A workflow joins its independent checks under tuple `Apply`, whose failure holds one leaf per violated rule, and binds each step that reads an earlier result, the first failure skipping every later step. `let` names a value derived between steps, and a limit the caller varies is a defaulted parameter:

```csharp
internal sealed record Request(string Key, decimal Amount);

internal static class Withdrawals {
    public static Fin<decimal> Remaining(Request request, Map<string, decimal> balances, decimal floor = 0m) =>
        from balance in (
                Fault.Unless(request.Amount > 0m, Reason.Range, request.Amount),
                balances.Find(request.Key).ToFin(Fault.Of(Reason.Missing, request.Key)))
            .Apply(static (_, found) => found).As()
        let remaining = balance - request.Amount
        from _ in Fault.Unless(remaining >= floor, Reason.Range, (remaining, floor))
        select remaining;
}
```

- `Apply` takes 2 to 10 operands and one uncurried function, a value object field joins the tuple through its `Validated` factory
- Rule sets held as data, `Seq<Func<T, Fin<Unit>>>`, join through `Traverse`, and `TraverseM` stops at the first failure, so cheap structural rules go before an expensive one
- Protocol adapters render each `Fault` leaf of a final expected `Fail` (`error.Filter<Fault>().AsIterable()`) as one problem entry

## [02]-[UNION_FOLDS]

Cases holding the union itself form a tree, and each case passes its children to the base constructor, so recursion sits once in `Fold` and every reduction is one step over a node and its already-folded children:

```csharp
[Union]
internal abstract partial record Block {
    private Block(Seq<Block> children) => Children = children;

    public Seq<Block> Children { get; }

    internal sealed record Paragraph(string Text) : Block([]);
    internal sealed record Figure(Uri Source) : Block([]);
    internal sealed record Section(string Title, Seq<Block> Children) : Block(Children);
}

internal static class Blocks {
    public static R Fold<R>(Block block, Func<Block, Seq<R>, R> step) => step(block, block.Children.Map(child => Fold(child, step)));
    public static int Depth(Block block) => Fold<int>(block, static (_, depths) => 1 + depths.Max(0));
    public static Seq<Uri> Sources(Block block) => Fold<Seq<Uri>>(block, static (node, nested) => node is Block.Figure figure ? [figure.Source] : nested.Bind(static sources => sources));
}
```

- Steps read a case's data by matching the node, a rewrite keeping each case's shape calls `Switch` with one arm per case
- Lookups into one case return `Option<A>` and answer `None` for every other case, an operation defined for one case takes that case type
