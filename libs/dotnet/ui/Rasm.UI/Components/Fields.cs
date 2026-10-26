using Rasm.UI.Styling;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FieldRow(Key Key, Seq<Span> Caption, Node Value) {
    private static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));

    public LanguageExt.HashSet<Condition> Conditions { get; init; }
    public Option<Node> Trail { get; init; }

    private Node.Text Captioned => new(Caption) { Role = Role.Label, Conditions = Conditions };

    private Node Valued =>
        new Node.Flex(Orientation.Horizontal, (Value with { Accessible = Value.Accessible.Combine(new Accessible { Label = Captioned.Content }) }).Cons(Trail.ToSeq())) {
            Key = Key,
            Width = Grown,
            Conditions = Conditions,
        };

    public static Node Drawn(Seq<FieldRow> rows) =>
        new Node.Choice(Orientation.Horizontal, [
            new Node.Grid(new Tracks.Explicit([Sizing.Fit.Content, Grown]), rows.Map(static row => Seq(row.Captioned, row.Valued))),
            new Node.Grid(new Tracks.Explicit([Grown]), rows.Bind(static row => Seq(Seq<Node>(row.Captioned), Seq(row.Valued)))),
        ]);
}
