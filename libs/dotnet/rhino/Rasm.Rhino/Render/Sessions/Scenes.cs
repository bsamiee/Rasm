using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SceneBatch {
    public static SceneBatch Empty { get; } = new();

    public Option<ViewportInfo> View { get; init; }
    public Option<LinearWorkflow> Workflow { get; init; }
    public HashMap<uint, Transform> Transforms { get; init; } = HashMap<uint, Transform>();
    public HashMap<Guid, Light> DynamicLights { get; init; } = HashMap<Guid, Light>();
    public Option<RenderSettings> Settings { get; init; }
    public HashMap<RenderSettings.EnvironmentUsage, Option<Guid>> Environments { get; init; } = HashMap<RenderSettings.EnvironmentUsage, Option<Guid>>();
    public Option<SceneSkylight> Skylight { get; init; }
    public Option<Light> Sun { get; init; }
    public HashMap<Guid, Option<SceneLight>> Lights { get; init; } = HashMap<Guid, Option<SceneLight>>();
    public HashMap<uint, uint> Materials { get; init; } = HashMap<uint, uint>();
    public HashMap<Guid, Option<SceneMesh>> Meshes { get; init; } = HashMap<Guid, Option<SceneMesh>>();
    public HashMap<uint, Option<SceneInstance>> Instances { get; init; } = HashMap<uint, Option<SceneInstance>>();
    public Option<SceneGround> Ground { get; init; }
    public HashMap<Guid, Option<SceneClip>> ClippingPlanes { get; init; } = HashMap<Guid, Option<SceneClip>>();
    public Option<SceneDisplay> Display { get; init; }

    public SceneBatch Combine(SceneBatch later) =>
        new() {
            View = later.View | View,
            Workflow = later.Workflow | Workflow,
            Transforms = Transforms.AddOrUpdateRange(later.Transforms.AsIterable()),
            DynamicLights = DynamicLights.AddOrUpdateRange(later.DynamicLights.AsIterable()),
            Settings = later.Settings | Settings,
            Environments = Environments.AddOrUpdateRange(later.Environments.AsIterable()),
            Skylight = later.Skylight | Skylight,
            Sun = later.Sun | Sun,
            Lights = Lights.AddOrUpdateRange(later.Lights.AsIterable()),
            Materials = Materials.AddOrUpdateRange(later.Materials.AsIterable()),
            Meshes = Meshes.AddOrUpdateRange(later.Meshes.AsIterable()),
            Instances = Instances.AddOrUpdateRange(later.Instances.AsIterable()),
            Ground = later.Ground | Ground,
            ClippingPlanes = ClippingPlanes.AddOrUpdateRange(later.ClippingPlanes.AsIterable()),
            Display = later.Display | Display,
        };
}

public sealed record SceneLight(Light Data, Option<uint> MaterialId);

public sealed record SceneMesh(Seq<Mesh> Meshes, Seq<SceneMapping> Mappings, Transform OcsTransform, Option<ObjectAttributes> Attributes, Option<GeometryBase> Original);

public sealed record SceneMapping(int Channel, Transform Local, TextureMapping Mapping);

public sealed record SceneInstance(Guid MeshId, uint MaterialId, Transform Transform, Option<Guid> RootId, Option<Guid> ParentId) {
    public Option<ObjectAttributes> Attributes { get; init; }
    public Seq<SceneAncestor> Ancestry { get; init; }
}

public sealed record SceneAncestor(Guid ReferenceId, Guid DefinitionId, Option<ObjectAttributes> ReferenceAttributes, Transform Transform);

public sealed record SceneGround(bool Enabled, double Altitude, bool IsShadowOnly, bool ShowUnderside, Option<uint> MaterialId, Vector2d TextureScale, Vector2d TextureOffset, double TextureRotation);

public sealed record SceneSkylight(bool Enabled, bool UsesCustomEnvironment, double ShadowIntensity);

public sealed record SceneClip(bool IsEnabled, Plane Plane, Seq<Guid> ViewIds, Option<ObjectAttributes> Attributes);

public sealed record SceneDisplay(Guid Id, int RealtimeRenderPasses);
