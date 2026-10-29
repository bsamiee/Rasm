using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Render;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class MeshModifier {
    // --- [KINDS]
    public static readonly MeshModifier Displacement = new(static modifiers => modifiers.Displacement is not null);
    public static readonly MeshModifier EdgeSoftening = new(static modifiers => modifiers.EdgeSoftening is not null);
    public static readonly MeshModifier Thickening = new(static modifiers => modifiers.Thickening is not null);
    public static readonly MeshModifier CurvePiping = new(static modifiers => modifiers.CurvePiping is not null);
    public static readonly MeshModifier ShutLining = new(static modifiers => modifiers.ShutLining is not null);

    // --- [PRESENCE]
    [UseDelegateFromConstructor]
    public partial bool IsAttached(File3dmMeshModifiers modifiers);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AttributeEdits {
    // --- [STAGE]
    public static Stage<ObjectAttributes> Stage(RhinoDoc doc, RhinoObject target, bool quiet) =>
        new(
            Copy: static live => Products.Of(() => live.Duplicate()),
            Unchanged: static (_, _) => false,
            Commit: (_, staged) => Products.Accepted(() => doc.Objects.ModifyAttributes(target, staged, quiet)).Map(_ => staged));

    public static IO<Seq<Guid>> Modify(RhinoDoc doc, Seq<RhinoObject> objects, Func<RhinoObject, Seq<Step<ObjectAttributes>>> steps, bool quiet) =>
        objects.Map(target => (Target: target, Steps: steps(target))).Filter(static row => !row.Steps.IsEmpty)
            .Map((row, index) => Edits.Apply(Stage(doc, row.Target, quiet), row.Target.Attributes, row.Steps).Map(_ => row.Target.Id).Bracket()
                .MapFail(error => new RhinoFault(row.Target.Id.ToString(), RhinoReason.Declined, index, error)))
            .TraverseM(static edit => edit).As();

    // --- [STEPS]
    public static Seq<Step<ObjectAttributes>> UserStrings(HashMap<string, Option<string>> edits) =>
        toSeq<(string Key, Option<string> Value)>(edits).Map(static edit => Steps.Of<ObjectAttributes>(staged => Conversions.Present(staged.GetUserString(edit.Key)) == edit.Value.Bind(Conversions.Present)
            || edit.Value.Match(Some: text => staged.SetUserString(edit.Key, text), None: () => staged.DeleteUserString(edit.Key)))).Strict();

    public static Step<ObjectAttributes> HiddenInDetails(Seq<Guid> details) =>
        Steps.Of<ObjectAttributes>(staged => {
            HashSet<Guid> held = toHashSet(staged.GetHideInDetailOverrides());
            HashSet<Guid> wanted = toHashSet(details);
            return toSeq(held - wanted).ForAll(staged.RemoveHideInDetailOverride) && toSeq(wanted - held).ForAll(staged.AddHideInDetailOverride);
        });

    public static Step<ObjectAttributes> AddDecal(DecalCreateParams decal) =>
        Steps.Of<ObjectAttributes>(staged => {
            using Decal? created = Decal.Create(decal);
            return created is not null && staged.Decals.Add(created) is not 0u;
        });

    public static Step<ObjectAttributes> AddMaterialRef(Guid plugIn, ObjectMaterialSource source, Guid frontId, int frontIndex, Guid backId, int backIndex) =>
        Steps.Of<ObjectAttributes>(staged => {
            using MaterialRef? created = staged.MaterialRefs.Create(new MaterialRefCreateParams {
                PlugInId = plugIn, MaterialSource = source, FrontFaceMaterialId = backId, FrontFaceMaterialIndex = backIndex, BackFaceMaterialId = frontId, BackFaceMaterialIndex = frontIndex,
            });
            if (created is null) return false;
            staged.MaterialRefs.Add(plugIn, created);
            return true;
        });

    // --- [FACTS]
    extension(ObjectAttributes attributes) {
        public bool HasCustomLinetype {
            get {
                using Linetype? held = attributes.GetCustomLinetype();
                return held is not null;
            }
        }

        public bool HasCustomSectionStyle {
            get {
                using SectionStyle? held = attributes.GetCustomSectionStyle();
                return held is not null;
            }
        }

        public bool HasCustomMeshing {
            get {
                using MeshingParameters? held = attributes.CustomMeshingParameters;
                return held is not null;
            }
        }

        public Seq<MeshModifier> MeshModifiers => toSeq(MeshModifier.Items).Filter(modifier => modifier.IsAttached(attributes.File3dmMeshModifiers)).Strict();

        public Option<(Seq<Guid> Viewports, bool Active)> ActiveInViewports =>
            Conversions.Present(attributes.GetActiveInViewportOverrides(out Guid[] ids, out bool active), (Viewports: Conversions.Rows(ids), Active: active));

        public Option<bool> ActiveInViewport(Guid viewport) => Conversions.Present(attributes.HasActiveInViewportOverride(viewport, out bool active), active);
    }

    extension(Decal decal) {
        public Interval HorizontalSweep {
            get {
                decal.HorzSweep(out double start, out double end);
                return new(start, end);
            }
        }

        public Interval VerticalSweep {
            get {
                decal.VertSweep(out double start, out double end);
                return new(start, end);
            }
        }

        public (Interval U, Interval V) UvBounds {
            get {
                decal.GetUVBounds(out double minU, out double minV, out double maxU, out double maxV);
                return (new(minU, maxU), new(minV, maxV));
            }
        }
    }
}
