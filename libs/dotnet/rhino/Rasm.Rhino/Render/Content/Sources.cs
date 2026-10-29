using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Render;

namespace Rasm.Rhino.Render.Content;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Contents {
    // --- [KINDS]
    public static TResult ByKind<TResult>(RenderContent content, Func<RenderMaterial, TResult> material, Func<RenderEnvironment, TResult> environment, Func<RenderTexture, TResult> texture) =>
        content switch {
            RenderMaterial found => material(found),
            RenderEnvironment found => environment(found),
            RenderTexture found => texture(found),
            _ => throw new UnreachableException(),
        };

    public static RenderContentKind KindOf(RenderContent content) =>
        ByKind(content, static _ => RenderContentKind.Material, static _ => RenderContentKind.Environment, static _ => RenderContentKind.Texture);

    // --- [ROWS]
    public static IO<Seq<RenderContent>> Rows(RhinoDoc doc, RenderContentKind kinds) =>
        IO.lift(() => Seq<(RenderContentKind Kind, IEnumerable<RenderContent> Table)>(
                (RenderContentKind.Material, doc.RenderMaterials), (RenderContentKind.Environment, doc.RenderEnvironments), (RenderContentKind.Texture, doc.RenderTextures))
            .Filter(row => kinds.HasFlag(row.Kind))
            .Bind(static row => toSeq(row.Table))
            .Strict());

    public static IO<RenderContent> Resolve(RhinoDoc doc, RenderContentKind kinds, ComponentRef<RenderContent> row, Seq<string> slots) =>
        from top in row.Switch(
                (Doc: doc, Kinds: kinds),
                byId: static (state, byId) => IO.lift(() => Optional(RenderContent.FromId(state.Doc, byId.Id)).Filter(found => state.Kinds.HasFlag(KindOf(found)))),
                byIndex: static (state, byIndex) => Rows(state.Doc, state.Kinds).Map(rows => rows.At(byIndex.Index)),
                byName: static (state, byName) => Rows(state.Doc, state.Kinds)
                    .Map(rows => rows.Filter(content => string.Equals(content.Name, byName.Name, StringComparison.Ordinal)))
                    .Bind(named => named.Count > 1
                        ? IO.fail<Option<RenderContent>>(new RhinoFault($"render content name {byName.Name}", RhinoReason.Repeated))
                        : IO.pure(named.Head)))
            .Bind(found => IO.lift(found.ToFin(new RhinoFault($"render content {row}", RhinoReason.Missing))))
        from child in IO.lift(() => slots.Fold(Fin.Succ(top), static (parent, slot) => parent.Bind(content =>
            Optional(content.FindChild(slot)).ToFin(new RhinoFault($"slot {slot} of {content.Id}", RhinoReason.Missing)))))
        select child;

    // --- [LIFETIME]
    public static RenderContent? FromXml(string xml, RhinoDoc? doc) =>
        RenderContent.FromXml(xml, doc) switch {
            null => null,
            var floating => Floating(floating),
        };

    private static RenderContent Floating(RenderContent content) {
        Internals.AutoDelete(content, autoDelete: true);
        return content;
    }

    // --- [TABLES]
    public static IO<Unit> Attach(RhinoDoc doc, RenderContent content) =>
        Products.Accepted(() => ByKind(content, doc.RenderMaterials.Add, doc.RenderEnvironments.Add, doc.RenderTextures.Add));

    public static IO<RenderContent> Detach(RhinoDoc doc, RenderContent content) =>
        Products.Accepted(() => ByKind(content, doc.RenderMaterials.Remove, doc.RenderEnvironments.Remove, doc.RenderTextures.Remove)) >> use(() => content);

    public static IO<Seq<Guid>> Purge(RhinoDoc doc, RenderContentKind kinds) =>
        Rows(doc, kinds).Bind(rows => Products.Each(rows
            .Filter(static content => content.UseCount() is 0 && !content.IsDefaultInstance && !content.IsHiddenByAutoDelete)
            .Map(content => Detach(doc, content).Map(static detached => detached.Id).Bracket())));

    public static IO<T> WithinTableChange<T>(RhinoDoc doc, RenderContent.ChangeContexts cc, IO<T> body) =>
        IO.lift(() => doc.RenderMaterials.BeginChange(cc)) >> body.Finally(IO.lift(doc.RenderMaterials.EndChange));

    // --- [EDITS]
    public static Stage<RenderContent> Changing(RenderContent.ChangeContexts cc) =>
        new(
            Copy: live => IO.lift(() => live.BeginChange(cc)) >> Products.Released(live.EndChange) >> IO.pure(live),
            Unchanged: static (_, _) => false,
            Commit: static (_, staged) => IO.pure(staged));

    public static IO<RenderContent> Edit(RenderContent content, RenderContent.ChangeContexts cc, Seq<Step<RenderContent>> steps) =>
        Edits.Apply(Changing(cc), content, steps).Bracket();

    public static Step<RenderContent> Child(Func<RenderContent?> make, string slot, [CallerArgumentExpression(nameof(make))] string member = "") =>
        new(target => Products.Transfer(make, child => Products.Accepted(() => target.SetChild(child, slot), member)).Map(_ => target), member);

    // --- [CHOOSERS]
    public static IO<Guid> ShowContentInstanceBrowser(RhinoDoc doc, Option<Guid> selected, RenderContentKind kinds, RenderContent.ContentInstanceBrowserButtons buttons) =>
        IO.lift(() => {
            Guid chosen = Conversions.Unset(selected);
            return RenderContent.ShowContentInstanceBrowser(doc, ref chosen, kinds, buttons) ? Fin.Succ(chosen) : Errors.Cancelled;
        });

    public static IO<(Utilities.ContentNewExistingResults Choice, Seq<Guid> Contents)> ShowContentNewExistingBrowser(
        RhinoDoc doc, Option<Guid> defaultType, Option<Guid> defaultInstance, RenderContentKind kinds, Utilities.ContentNewExistingFlags flags,
        Option<string> presetCategory, Seq<string> categories, Seq<Guid> types) =>
        IO.lift(() => Utilities.ShowContentNewExistingBrowser(
                doc, Conversions.Unset(defaultType), Conversions.Unset(defaultInstance), kinds, flags, Conversions.Unset(presetCategory), categories, types, out Guid[] contents)
            is not Utilities.ContentNewExistingResults.None and var choice ? Fin.Succ((Choice: choice, Contents: toSeq(contents))) : Errors.Cancelled);
}
