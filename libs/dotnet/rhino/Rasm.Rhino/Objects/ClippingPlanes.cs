using System.Collections.Generic;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Objects;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ClippingPlanes {
    // --- [DOCUMENT]
    public static IO<Guid> Add(RhinoDoc doc, Plane plane, Seq<Step<ClippingPlaneSurface>> steps, Option<ObjectAttributes> attributes, Option<HistoryRecord> history) =>
        (from surface in use(() => new ClippingPlaneSurface(plane))
         from staged in Edits.Apply(Stages.InPlace<ClippingPlaneSurface>(), surface, steps)
         from id in Products.Accepted(() => doc.Objects.AddClippingPlaneSurface(staged, attributes.ValueUnsafe(), history.ValueUnsafe(), reference: false) switch { var added => (added != Guid.Empty, added) })
         select id).Bracket();

    public static IO<Seq<Guid>> Edit(RhinoDoc doc, Seq<ClippingPlaneObject> planes, Func<ClippingPlaneObject, Seq<Step<ClippingPlaneSurface>>> steps) =>
        planes.Map((plane, index) => Edits.Apply(RhinoObjects.Replacement<ClippingPlaneSurface>(doc, plane.Id, ignoreModes: true), plane.ClippingPlaneGeometry, steps(plane)).Map(_ => plane.Id).Bracket()
                .MapFail(error => new RhinoFault(plane.Id.ToString(), RhinoReason.Declined, index, error)))
            .TraverseM(static edit => edit).As();

    public static IO<Seq<Guid>> Detach(RhinoDoc doc, Seq<RhinoViewport> viewports) =>
        IO.lift(() => toSeq(viewports.Bind(viewport => Conversions.Rows(doc.Objects.FindClippingPlanesForViewport(viewport, includeHidden: true))).DistinctBy(static plane => plane.Id)).Strict())
            .Bind(planes => Edit(doc, planes, plane => viewports.Map(static viewport => viewport.Id).Filter(toHashSet(plane.ClippingPlaneGeometry.ViewportIds()).Contains)
                .Map(static id => Steps.Of<ClippingPlaneSurface>(surface => surface.RemoveClipViewportId(id)))));

    // --- [STEPS]
    public static Step<ClippingPlaneSurface> Participation(Seq<Guid> objects, Seq<int> layers, bool exclusion) =>
        Steps.Then<ClippingPlaneSurface>(surface => {
            surface.SetClipParticipation(objects, layers, exclusion);
            surface.ParticipationListsEnabled = true;
            return surface;
        });

    public static Step<ClippingPlaneSurface> Unrestricted { get; } =
        Steps.Then<ClippingPlaneSurface>(static surface => {
            surface.ClearClipParticipationLists();
            surface.ParticipationListsEnabled = false;
            return surface;
        });

    // --- [FILLS]
    public static IO<Seq<(Guid Object, Seq<Brep> Fills)>> Fills(RhinoDoc doc, Seq<ClippingPlaneObject> planes, bool unclippedFills) =>
        IO.lift(() => Conversions.Rows(doc.Objects.GetObjectList(new ObjectEnumeratorSettings { HiddenObjects = false, LockedObjects = true, ReferenceObjects = true }))
                .Filter(found => found is not ClippingPlaneObject && planes.Exists(plane => plane.ObjectParticipates(found.Id))).Strict())
            .Bind(objects => objects.TraverseM(found =>
                Products.Maybe(() => RhinoObject.GetFillSurfaces(found, planes, unclippedFills)).Bind(fills => fills.Filter(static held => held.Length > 0).Match(
                    Some: held => Products.Many(() => held).Map(owned => Some((Object: found.Id, Fills: owned))),
                    None: static () => IO.pure(Option<(Guid Object, Seq<Brep> Fills)>.None)))).As())
            .Map(static rows => rows.Somes());

    // --- [FACTS]
    extension(ClippingPlaneSurface surface) {
        public Option<(Seq<Guid> Objects, Seq<int> Layers, bool Exclusion)> Participants {
            get {
                surface.GetClipParticipation(out IEnumerable<Guid> objects, out IEnumerable<int> layers, out bool exclusion);
                return Conversions.Present(surface.ParticipationListsEnabled, (Objects: toSeq(objects).Strict(), Layers: toSeq(layers).Strict(), Exclusion: exclusion));
            }
        }

        public Option<double> Depth => Conversions.Present(surface.PlaneDepthEnabled, surface.PlaneDepth);
    }
}
