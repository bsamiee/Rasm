using Rasm.Rhino.Render.Content;
using Rasm.Rhino.UI.Styling;
using Rhino.Render;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Views;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class EngineScope {
    public static readonly EngineScope Own = new(alwaysShow: false, static () => Session.PlugIn.Id);
    public static readonly EngineScope Every = new(alwaysShow: true, static () => new Guid(0x99999999, 0x9999, 0x9999, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99));

    public bool AlwaysShow { get; }

    [UseDelegateFromConstructor]
    public partial Guid TabEngineId();
}

[Union]
public abstract partial record ContentScope {
    public bool Covers(RenderContent content) =>
        Switch(
            content,
            kinds: static (held, scope) => scope.Mask.HasFlag(Contents.KindOf(held)),
            types: static (held, scope) => scope.TypeIds.Contains(held.TypeId));

    public sealed record Kinds(RenderContentKind Mask) : ContentScope;

    public sealed record Types(HashSet<Guid> TypeIds) : ContentScope;
}

public sealed record ContentMenuRow(
    string Caption,
    IconResource Icon,
    ContentScope Scope,
    HashSet<RenderContentMenu.Context> Contexts,
    RenderContentMenu.SeparatorStyle Separator,
    bool TopLevel,
    Func<Seq<RenderContent>, IO<Unit>> Run);
