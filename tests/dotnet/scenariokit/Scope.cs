using System.Globalization;
using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Framework;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Rasm.Bridge.Contract;
using Rhino;
using Rhino.DocObjects;
using GhDocument = Grasshopper2.Doc.Document;
using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.ScenarioKit;

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<Guid>]
public sealed partial class Gh2Object {
    public static readonly Gh2Object Addition = new(Guid.Parse("00000000-a9e2-4ef6-a4d8-a8f87f93757f"));
    public static readonly Gh2Object Multiplication = new(Guid.Parse("00000000-e4fe-4f60-b2b5-28a52b3047ca"));
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Gh2Solve(SolutionPhase Phase, int Computable, Seq<string> Errors) {
    public bool Completed => Phase == SolutionPhase.Completed && Errors.IsEmpty;
}

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class Gh2Document {
    private Gh2Document(GhDocument native) => Native = native;

    public GhDocument Native { get; }
    public int ObjectCount => Native.Objects.Count;
    public int WireCount => Native.Objects.AllWires.Count();

    public static Fin<Gh2Document> Isolated() =>
        ScenarioContext.CaptureHost<Gh2Document>(static () => new Gh2Document(GhDocument.NewInactiveDocument()));

    public Fin<IDocumentObject> Place(Gh2Object item, PointF at) {
        ArgumentNullException.ThrowIfNull(item);
        return ScenarioContext.CaptureHost(Fin<IDocumentObject> () => ObjectProxies.FindById(item.Key) switch {
            null => Error.New($"no GH2 library object registered for io-id {item.Key:D} ({ObjectProxies.Count} proxies registered)"),
            { } proxy => proxy.Emit() is IDocumentObject emitted && Native.Objects.Add(emitted, at)
                ? Fin.Succ(emitted)
                : Error.New($"'{proxy.Nomen.Name}' did not emit into the document"),
        });
    }

    public Fin<Component> Node(Gh2Object item, PointF at) =>
        Place(item, at).Bind(static Fin<Component> (placed) => placed is Component component
            ? component
            : Error.New($"'{placed.Nomen.Name}' emitted a {placed.GetType().Name}, not a component"));

    public Fin<NumberParameter> Number(PointF at, params double[] values) =>
        ScenarioContext.CaptureHost<NumberParameter>(() => {
            NumberParameter number = new();
            _ = number.Set(values);
            return Native.Objects.Add(number, at) ? number : Error.New("NumberParameter did not add to the document");
        });

    public static Fin<Unit> Wire(IParameter source, IParameter target) =>
        ScenarioContext.CaptureHost<Unit>(() => Connections.Connect(source, target, undo: null)
            ? unit
            : Error.New($"wire refused: {source.Nomen.Name} -> {target.Nomen.Name}"));

    public Fin<Gh2Solve> Solve() =>
        ScenarioContext.CaptureHost<Gh2Solve>(() => {
            using CancellationTokenSource cancellation = new();
            Solution solution = Native.Solution.StartWait(cancellation, SolutionMode.Headless);
            Seq<string> errors = toSeq(Native.Objects.Forwards
                .SelectMany(static item => item.State.Data?.Messages.Errors ?? [])
                .Select(static message => message.Text));
            Gh2Solve solve = new(solution.Phase, solution.ComputableCount, errors);
            return solve.Completed ? solve : Error.New(string.Create(CultureInfo.InvariantCulture, $"solution {solve.Phase} with {errors.Count} error(s): {string.Join("; ", errors)}"));
        });

    public static Fin<T[]> Output<T>(IParameter parameter) {
        ArgumentNullException.ThrowIfNull(parameter);
        return ScenarioContext.CaptureHost<T[]>(() => {
            if (parameter.State.Data is not { } data) {
                return Error.New($"'{parameter.Nomen.Name}' carries no solution data");
            }
            object?[] items = [.. data.Tree().AllItems];
            return items.All(static item => item is T)
                ? items.Cast<T>().ToArray()
                : Error.New($"'{parameter.Nomen.Name}' carries a {items.First(static item => item is not T)?.GetType().Name ?? "null"} item, not {typeof(T).Name}");
        });
    }

    internal void Close() {
        Native.Unmodify();
        Native.Close();
    }
}

public sealed class RhinoDocumentScope : ScenarioContext.IScope {
    private static readonly ObjectEnumeratorSettings ActiveContent = new() {
        NormalObjects = true, LockedObjects = true, HiddenObjects = true, ActiveObjects = true,
        ReferenceObjects = false, IdefObjects = false, DeletedObjects = false,
        IncludeLights = true, IncludeGrips = false, IncludePhantoms = true,
    };

    private readonly ScenarioContext ctx;
    private readonly FrozenSet<Guid> baseline;
    private readonly bool modifiedBefore;

    private RhinoDocumentScope(ScenarioContext ctx, FrozenSet<Guid> baseline, bool modifiedBefore) {
        this.ctx = ctx;
        this.baseline = baseline;
        this.modifiedBefore = modifiedBefore;
    }

    public RhinoDoc Doc => ctx.Doc;
    public bool ViewportRealized => Doc.Views.ActiveView is not null;
    internal bool IsLive { get; private set; } = true;
    bool ScenarioContext.IScope.IsLive => IsLive;

    internal static Fin<RhinoDocumentScope> Open(ScenarioContext ctx) =>
        ScenarioContext.CaptureHost<RhinoDocumentScope>(() => {
            FrozenSet<Guid> existing = ObjectIds(ctx.Doc);
            RhinoDocumentScope scope = new(ctx, existing, ctx.Doc.Modified);
            ctx.Fact(EvidenceName.Create("rhino.document.before.objects"), existing.Count);
            ctx.Register(scope);
            return scope;
        });

    internal static FrozenSet<Guid> ObjectIds(RhinoDoc doc) => doc.Objects.GetObjectIdList(ActiveContent).ToFrozenSet();

    Fin<Unit> ScenarioContext.IScope.Complete() => Close("completed");
    public void Dispose() => _ = Close("disposed");

    private Fin<Unit> Close(string reason) {
        if (!IsLive) {
            return unit;
        }
        IsLive = false;
        return ScenarioContext.CaptureHost<Unit>(() => {
            FrozenSet<Guid> current = ObjectIds(Doc);
            Guid[] added = [.. current.Where(id => !baseline.Contains(id) && Doc.Objects.FindId(id) is not null)];
            int missing = baseline.Count(id => Doc.Objects.FindId(id) is null);
            _ = Doc.Objects.Delete(added, quiet: true);
            int residual = added.Count(id => Doc.Objects.FindId(id) is not null);
            int removed = added.Length - residual;
            if (!modifiedBefore) {
                Doc.Modified = false;
            }
            ctx.Fact(EvidenceName.Create("rhino.document.after.objects"), current.Count);
            ctx.Fact(EvidenceName.Create("rhino.document.cleanup.reason"), reason);
            ctx.Fact(EvidenceName.Create("rhino.document.cleanup.removed"), removed);
            ctx.Fact(EvidenceName.Create("rhino.document.cleanup.missing"), missing);
            ctx.Fact(EvidenceName.Create("rhino.document.cleanup.residual"), residual);
            return missing == 0 && residual == 0
                ? unit
                : Error.New(string.Create(CultureInfo.InvariantCulture, $"Rhino document cleanup breached its baseline: missing={missing};residual={residual}"));
        });
    }
}

public sealed class GrasshopperDocumentScope : ScenarioContext.IScope {
    private readonly ScenarioContext ctx;
    private readonly GhEditor editor;
    private readonly GhDocument prior;

    private GrasshopperDocumentScope(ScenarioContext ctx, GhEditor editor, GhDocument prior, Gh2Document doc) {
        this.ctx = ctx;
        this.editor = editor;
        this.prior = prior;
        Doc = doc;
    }

    public Gh2Document Doc { get; }
    internal bool IsLive { get; private set; } = true;
    bool ScenarioContext.IScope.IsLive => IsLive;

    internal static Fin<GrasshopperDocumentScope> Open(ScenarioContext ctx) =>
        ScenarioContext.CaptureHost<GhEditor>(static () => GhEditor.Instance is { } live
                ? live
                : Error.New("GH2 editor is unavailable; require the gh2.editor capability before opening a GH2 document"))
            .Bind(live => Gh2Document.Isolated().Bind(doc => ScenarioContext.CaptureHost<GrasshopperDocumentScope>(() => {
                GhDocument previous = live.Canvas.Document;
                live.Canvas.Document = doc.Native;
                GrasshopperDocumentScope scope = new(ctx, live, previous, doc);
                ctx.Fact(EvidenceName.Create("gh2.document.before.objects"), doc.ObjectCount);
                ctx.Register(scope);
                return scope;
            })));

    Fin<Unit> ScenarioContext.IScope.Complete() => Close("completed");
    public void Dispose() => _ = Close("disposed");

    private Fin<Unit> Close(string reason) {
        if (!IsLive) {
            return unit;
        }
        IsLive = false;
        return ScenarioContext.CaptureHost<Unit>(() => {
            int observed = Doc.ObjectCount;
            if (ReferenceEquals(editor.Canvas.Document, Doc.Native)) {
                editor.Canvas.Document = prior;
            }
            Doc.Close();
            bool residual = GhDocument.AllDocuments.Contains(Doc.Native, ReferenceEqualityComparer.Instance);
            ctx.Fact(EvidenceName.Create("gh2.document.after.objects"), observed);
            ctx.Fact(EvidenceName.Create("gh2.document.cleanup.reason"), reason);
            ctx.Fact(EvidenceName.Create("gh2.document.cleanup.closed"), !residual);
            return residual ? Error.New("GH2 document remained registered after Close()") : unit;
        });
    }
}
