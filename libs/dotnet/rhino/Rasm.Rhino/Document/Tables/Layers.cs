using System.Collections.Generic;
using Rasm.Drafting;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record LayerRef {
    public sealed record Row(ComponentRef<Layer> Address) : LayerRef;

    public sealed record Path(string FullPath) : LayerRef;

    public sealed record Current() : LayerRef;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlotWeight {
    public sealed record Default() : PlotWeight;

    public sealed record NoPlot() : PlotWeight;

    public sealed record Width : PlotWeight {
        internal Width(Length value) => Value = value;

        public Length Value { get; }
    }

    public static PlotWeight Of(LineWidth width) => new Width(width.Width);

    public static PlotWeight FromHost(double millimeters) =>
        millimeters switch {
            > 0.0 => new Width(Length.FromMillimeters(Quantities.Exact(millimeters))),
            < 0.0 => new NoPlot(),
            _ => new Default(),
        };

    public static Option<PlotWeight> FromInheritable(double millimeters) =>
        millimeters < -1.0 ? None : Some(FromHost(millimeters));

    public static double ToInheritable(Option<PlotWeight> weight) =>
        weight.Match(Some: static some => some.ToHost(), None: static () => -10.0);

    public double ToHost() =>
        Switch(@default: static _ => 0.0, noPlot: static _ => -1.0, width: static width => width.Value.Millimeters.ToDouble());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeRef {
    public sealed record Continuous() : LinetypeRef;

    public sealed record Row(ComponentRef<Linetype> Address) : LinetypeRef;

    public static Option<LinetypeRef> FromHost(int index) =>
        index switch {
            -1 => Some<LinetypeRef>(new Continuous()),
            >= 0 => Some<LinetypeRef>(new Row(new ComponentRef<Linetype>.ByIndex(index))),
            _ => Option<LinetypeRef>.None,
        };

    public IO<int> Resolve(RhinoDoc doc) =>
        Switch(
            doc,
            continuous: static (_, _) => IO.pure(-1),
            row: static (document, row) => Rows.Find(document.Linetypes, row.Address).Map(static found => found.Index));
}

// --- [TABLES] --------------------------------------------------------------------------
public static class Layers {
    public static TableKind<Layer> Kind { get; } = new(
        static doc => doc.Layers,
        Layer.GetDefaultLayerProperties,
        static live => {
            Layer staged = new();
            staged.CopyAttributesFrom(live);
            return staged;
        },
        static (doc, row) => doc.Layers.Add(row),
        static (doc, row, index) => doc.Layers.Modify(row, index, quiet: true),
        static doc => doc.Layers.GetUnusedLayerName()) {
        Live = static doc => toSeq(doc.Layers.GetSorted()).Map(index => doc.Layers[index]).Filter(static row => !row.IsDeleted).Strict(),
        Key = static row => row.FullPath,
        Names = StringComparer.OrdinalIgnoreCase,
        Parent = static row => Conversions.Present(row.ParentLayerId),
        Arrange = Preorder,
        Place = Placed,
        Purge = Some<Func<RhinoDoc, Layer, bool>>(static (doc, row) => doc.Layers.Purge(row.Index, quiet: true)),
        Sort = Some<Action<RhinoDoc, Seq<int>>>(static (doc, indices) => doc.Layers.Sort(indices)),
    };

    // --- [RESOLUTION]
    public static IO<Layer> Resolve(RhinoDoc doc, LayerRef address) =>
        address.Switch(
            doc,
            row: static (document, row) => Rows.Find(document.Layers, row.Address),
            path: static (document, path) => IO.lift(() =>
                Conversions.Present(document.Layers.FindByFullPath(path.FullPath, RhinoMath.UnsetIntIndex)).Bind(index => Optional(document.Layers.FindIndex(index)))
                    .ToFin(new RhinoFault(path.FullPath, RhinoReason.Missing))),
            current: static (document, _) => IO.lift(() => document.Layers.CurrentLayer));

    public static IO<Option<Seq<Guid>>> Selected(RhinoDoc doc) =>
        IO.lift(() => Conversions.Present(doc.Layers.GetSelected(out List<int> indices), toSeq(indices).Map(index => doc.Layers[index].Id).Strict()));

    // --- [MERGE]
    public static IO<Unit> Merge(RhinoDoc doc, LayerRef source, LayerRef target) =>
        from merged in Resolve(doc, source)
        from kept in Resolve(doc, target)
        from acyclic in IO.lift(() => RhinoFault.Unless(merged.Id != kept.Id && !Below(merged, kept.Id), RhinoReason.Invalid, (merged.FullPath, kept.FullPath)))
        from moved in new ObjectTarget.Lookup(table => table.GetObjectList(new ObjectEnumeratorSettings { HiddenObjects = true, IncludeLights = true, LayerIndexFilter = merged.Index }))
            .Each(doc, found => {
                using ObjectAttributes staged = found.Attributes.Duplicate();
                staged.LayerIndex = kept.Index;
                return doc.Objects.ModifyAttributes(found, staged, quiet: true);
            })
        from adopted in Conversions.Rows(merged.GetChildren(allChildren: false))
            .TraverseM(child => Rows.Edit(doc, Kind, new ComponentRef<Layer>.ById(child.Id), [Rows.Set<Layer, Guid>(static row => row.ParentLayerId, static (row, id) => row.ParentLayerId = id, kept.Id)]))
            .As()
        from removed in Rows.Remove(doc, Kind, merged, Prune.Delete)
        select unit;

    // --- [TREE]
    private static Seq<string> Preorder(Seq<string> keys) {
        Seq<string> paths = toSeq(keys.Bind(Lineage).Distinct(StringComparer.OrdinalIgnoreCase));
        ILookup<string, string> children = paths.ToLookup(Layer.GetParentName, StringComparer.OrdinalIgnoreCase);
        Seq<string> Beneath(string parent) => toSeq(children[parent]).Bind(child => child.Cons(Beneath(child)));
        return Beneath("");
    }

    private static Seq<string> Lineage(string key) =>
        (Layer.GetParentName(key) is { Length: > 0 } parent ? Lineage(parent) : []).Add(key);

    private static Fin<Seq<RowEdit<Layer>>> Placed(RhinoDoc doc, Option<Layer> live, string key) =>
        (Layer.GetLeafName(key), Layer.GetParentName(key)) switch {
            var (leaf, parent) =>
                from named in RhinoFault.Unless(ModelComponent.IsValidComponentName(leaf), RhinoReason.Unqualified, leaf)
                from above in parent.Length is 0
                    ? Option<Layer>.None
                    : Conversions.Present(doc.Layers.FindByFullPath(parent, RhinoMath.UnsetIntIndex)).Bind(index => Optional(doc.Layers.FindIndex(index)))
                        .ToFin(new RhinoFault(parent, RhinoReason.Missing))
                        .Map(Some)
                from acyclic in RhinoFault.Unless(!above.Exists(held => live.Exists(row => row.Id == held.Id || Below(row, held.Id))), RhinoReason.Invalid, key)
                select Seq(
                    Rows.Set<Layer, string>(static row => row.Name, static (row, name) => row.Name = name, leaf),
                    Rows.Set<Layer, Guid>(static row => row.ParentLayerId, static (row, id) => row.ParentLayerId = id, Conversions.Unset(above.Map(static held => held.Id)))),
        };

    private static bool Below(Layer layer, Guid candidate) =>
        Conversions.Rows(layer.GetChildren(allChildren: true)).Exists(child => child.Id == candidate);
}
