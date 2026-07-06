# [API_GH2_COMPONENTS]

Catalog scope: the Grasshopper2 component and parameter model — component lifecycle, pin registration, data access, tree/goo data structures, type conversion, and plugin registration.

[NAMESPACES]:
- `Grasshopper2.Components` — `Component` (identity: `CustomValues`, `InstanceId`, `Document`, `Nomen`), `ModularComponent` (`IconInternal`, `AppendToInputPanel`, `CreateAttributes`, `AddInputs`/`AddOutputs`, `BeforeProcess`/`PreProcess`/`Process`/`PostProcess`/`PostProcessTree`), `ModularInputAdder`/`ModularOutputAdder`, `ThreadingState`, well-known `CustomValues` keys (`__Optional`/`__HideByDefault`/`__Category`/`__Colour`).
- `Grasshopper2.Parameters` — `IDataAccess` (get/set pear/twig/tree, tolerance/unit-system/unit-scaling, `TryTransform`, remark/warning/error emission, `SetProgress`), `IParameter` (`CustomValues`, `PersistentDataWeak`, `Presets`), `Access` (Item/Twig/Tree), `Requirement`, `InputAdder`/`OutputAdder` (the full typed `Add*` pin roster, ~45 members), `ComponentParameters`, `Solution.Token`, `FleetingCustomData`.
- `Grasshopper2.Parameters.Standard` — `VectorParameter`, `AngleParameter`, `IntegerParameter`, `CurveParameter`, `SurfaceParameter` (normalization/flip/indexing policies).
- `Grasshopper2.Data` / `Grasshopper2.Data.Meta` — `Twig<T>`/`Tree<T>`/`ITree`/`Leaf<T>`, `Pear<T>`/`IPear`, `Garden` (tree/twig factory families), `Site`, `Path`, `MetaData.FindCommonData`.
- `Grasshopper2.Types.Conversion` — `CurveBroker`, `SurfaceBroker` (+`SurfaceLikeType`), `ConversionServer.Convert`.
- `Grasshopper2.Types.Shapes` — `Region`, `ArcF`, `LineF` (float-precision UI-space geometry).
- `Grasshopper2.Types.Numeric` — `Angle`.
- `Grasshopper2.Extensions` — `IDataAccess.CoverageOut`, `ITree.WithPathPrefix`, `Tree<T>.EnumerateLeaves`, `ArrayEx`.
- `Grasshopper2.Framework` — `Plugin` (author/contact/licence metadata, `SatelliteAssemblies`, `OnLoaded`, `ExportedTypes`).
- `GrasshopperIO` — `IoIdAttribute`.
- `Rhino.Geometry` crossing set — the carrier types component pins bind (`Point3d`, `Vector3d`, `Line`, `Arc`, `Circle`, `Rectangle3d`, `Curve`, `Brep`/`BrepFace`, `Surface`, `SubD`, `Box`, `Sphere`, `Plane`, `Mesh`/`MeshFace`, `Transform`, `Interval`, `Polyline`, `TextDot`).
