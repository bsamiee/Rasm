using System.Drawing;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ReferenceScope {
    public static readonly ReferenceScope TopLevel = new(0);
    public static readonly ReferenceScope TopLevelAndNested = new(1);
    public static readonly ReferenceScope FromOtherDefinitions = new(2);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Hyperlink(string Address, Option<string> Tag) {
    public Option<string> Tag { get; } = Tag.Bind(Conversions.Present);
}

public sealed record DefinitionMetadata(string Name, Option<string> Description, Option<Hyperlink> Link) {
    public Option<string> Description { get; } = Description.Bind(Conversions.Present);

    public Option<Hyperlink> Link { get; } = Link.Filter(static link => link.Address.Length > 0);

    public static DefinitionMetadata Of(InstanceDefinitionGeometry definition) =>
        new(definition.Name, Optional(definition.Description), Optional(definition.Url).Map(address => new Hyperlink(address, Optional(definition.UrlDescription))));

    internal string Url => Conversions.Unset(Link.Map(static link => link.Address));

    internal string UrlTag => Conversions.Unset(Link.Bind(static link => link.Tag));

    internal IO<Unit> Written(InstanceDefinitionTable table, InstanceDefinition definition) =>
        Products.Accepted(() => table.Modify(definition, Name, Conversions.Unset(Description), Url, UrlTag, quiet: true));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Definitions {
    // --- [STATE]
    private const string ClippingDrawingKey = "SectionTools";

    private static readonly Ref<bool> Watching = Ref(value: false);

    private static readonly Ref<HashMap<(uint Document, int Definition, Guid Mode, DefinedViewportProjection Projection, IsometricCamera Camera, bool Decorations, Size Pixels), Bitmap>> Previews =
        Ref(HashMap<(uint Document, int Definition, Guid Mode, DefinedViewportProjection Projection, IsometricCamera Camera, bool Decorations, Size Pixels), Bitmap>());

    // --- [FACTS]
    extension(InstanceDefinitionArchiveFileStatus status) {
        public bool Outdated => status is InstanceDefinitionArchiveFileStatus.LinkedFileIsNewer or InstanceDefinitionArchiveFileStatus.LinkedFileIsOlder or InstanceDefinitionArchiveFileStatus.LinkedFileIsDifferent;
    }

    extension(InstanceDefinition definition) {
        public bool Hidden => definition.Name.StartsWith('*');

        public bool ClippingDrawing => Conversions.Present(definition.GetUserString(ClippingDrawingKey)).IsSome;

        public (int TopLevel, int Nested) Usage {
            get {
                _ = definition.UseCount(out int topLevel, out int nested);
                return (topLevel, nested);
            }
        }

        public Seq<InstanceObject> References(ReferenceScope scope) => Conversions.Rows(definition.GetReferences(scope.Key));

        public Option<int> Nesting(InstanceDefinition inner) =>
            definition.Index == inner.Index ? None : Some(definition.UsesDefinition(inner.Index)).Filter(static level => level > 0);

        public IO<ContentKey> Key() =>
            from members in IO.lift(() => Conversions.Rows(definition.GetObjects()).Map(static member => member.Geometry).Strict())
            from archive in IO.lift(() => definition.IsLinkedType ? ContentHash.CreateFromFile(definition.SourceArchive).Sha1ContentHash : [])
            let metadata = DefinitionMetadata.Of(definition)
            select ContentKey.Of<InstanceDefinition>(stream => stream
                .Text(metadata.Name)
                .Rows(metadata.Description.ToSeq(), static (row, text) => row.Text(text))
                .Rows(metadata.Link.ToSeq(), static (row, link) => row.Text(link.Address).Rows(link.Tag.ToSeq(), static (tag, text) => tag.Text(text)))
                .Integer((int)definition.UpdateType)
                .Integer((int)definition.LayerStyle)
                .Integer(Convert.ToInt32(definition.SkipNestedLinkedDefinitions))
                .Integer((int)definition.UnitSystem)
                .Integer(members.Fold(0u, static (remainder, member) => member.DataCRC(remainder)))
                .Bytes(archive));

        // --- [PREVIEW]
        public IO<Bitmap> Preview(RhinoDoc doc, Guid displayMode, DefinedViewportProjection projection, IsometricCamera camera, bool decorations, Size pixels) =>
            (doc.RuntimeSerialNumber, definition.Index, displayMode, projection, camera, decorations, pixels) switch {
                var key =>
                    from watched in Watched
                    from cached in IO.lift(() => Previews.Value.Find(key))
                    from bitmap in cached.Match(
                        Some: IO.pure,
                        None: () =>
                            from fresh in IO.lift(() => Conversions.Required(definition.CreatePreviewBitmap(displayMode, projection, camera, decorations, pixels, applyDpiScaling: true)))
                            from settled in atomicIO(() => Previews.Value.Find(key).Match(
                                Some: held => (Kept: held, Spare: Some(fresh)),
                                None: () => {
                                    Previews.Value = Previews.Value.Add(key, fresh);
                                    return (Kept: fresh, Spare: Option<Bitmap>.None);
                                }))
                            from spared in IO.lift(() => settled.Spare.Iter(static spare => spare.Dispose()))
                            select settled.Kept)
                    select bitmap,
            };
    }

    private static IO<Unit> Watched =>
        atomicIO(static () => {
            bool watching = Watching.Value;
            Watching.Value = true;
            return watching;
        }).Bind(static watching => unless(watching,
            (Products.Released(static () => Evicted(static _ => true))
             >> Subscriptions.Inline<InstanceDefinitionTableEventArgs>(static h => RhinoDoc.InstanceDefinitionTableEvent += h, static h => RhinoDoc.InstanceDefinitionTableEvent -= h)
                 .Attach(static args => IO.lift(() => Evicted(key => key.Document == args.Document.RuntimeSerialNumber && key.Definition == args.InstanceDefinitionIndex))))
            .Scoped(Session.Scope)).As());

    private static Unit Evicted(Func<(uint Document, int Definition, Guid Mode, DefinedViewportProjection Projection, IsometricCamera Camera, bool Decorations, Size Pixels), bool> stale) =>
        atomic(() => {
            HashMap<(uint Document, int Definition, Guid Mode, DefinedViewportProjection Projection, IsometricCamera Camera, bool Decorations, Size Pixels), Bitmap> held = Previews.Value;
            Previews.Value = held.Filter((key, _) => !stale(key));
            return held.Filter((key, _) => stale(key));
        }).Values.Iter(static bitmap => bitmap.Dispose());
}
