namespace Rasm.Rhino;

// --- [MODELS] ---------------------------------------------------------------------------
public sealed class CommandSelection : IDisposable {
    private readonly Seq<ObjRef> references;
    private bool disposed;

    private CommandSelection(RhinoDoc document, Seq<ObjRef> references, Seq<Guid> preselected) {
        Document = document;
        this.references = references;
        Items = references.Map(reference => Reference.Of(reference: reference, preselected: preselected.Exists(id => id == reference.ObjectId)));
    }

    public RhinoDoc Document { get; }
    public Seq<Reference> Items { get; }
    public Seq<Guid> ObjectIds => Items.Map(static item => item.ObjectId);

    public TResult Use<TResult>(Func<Seq<ObjRef>, TResult> project) {
        ArgumentNullException.ThrowIfNull(argument: project);
        ObjectDisposedException.ThrowIf(condition: disposed, instance: this);
        return project(arg: references);
    }

    internal static CommandSelection From(RhinoDoc document, Seq<ObjRef> references, Seq<Guid> preselected) {
        ArgumentNullException.ThrowIfNull(argument: document);
        return new(document: document, references: references.Map(static reference => new ObjRef(reference)), preselected: preselected);
    }

    public void Dispose() {
        _ = disposed switch {
            true => unit,
            false => references.Iter(static reference => reference.Dispose()),
        };
        disposed = true;
        GC.SuppressFinalize(obj: this);
    }

    public readonly record struct Reference(Guid ObjectId, uint RuntimeSerialNumber, ComponentIndex Component, bool Preselected) {
        internal static Reference Of(ObjRef reference, bool preselected) =>
            new(
                ObjectId: reference.ObjectId,
                RuntimeSerialNumber: reference.RuntimeSerialNumber,
                Component: reference.GeometryComponentIndex,
                Preselected: preselected);

        public ObjRef ToObjRef(RhinoDoc document) {
            ArgumentNullException.ThrowIfNull(argument: document);
            RhinoObject? found = document.Objects.Find(runtimeSerialNumber: RuntimeSerialNumber);
            return (found, Component.IsSet) switch {
                (RhinoObject native, true) => new ObjRef(doc: document, id: native.Id, ci: Component),
                (RhinoObject native, false) => new ObjRef(rhinoObject: native),
                (_, true) => new ObjRef(doc: document, id: ObjectId, ci: Component),
                _ => new ObjRef(doc: document, id: ObjectId),
            };
        }
    }
}
