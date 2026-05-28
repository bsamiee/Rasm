# [H1][RASM_RHINO_CONSTRUCTION_ROADMAP]
>**Dictum:** *Construction turns design intent into valid Rhino geometry without caller ceremony.*

<br>

[IMPORTANT] `Construction` owns 2D and 3D Rhino geometry creation for `Rasm.Rhino`. It lets downstream plugins/apps describe primitives, generated geometry, fitted forms, transforms, framed bounds, annotation output, and document/block/preview-ready projection without remembering every RhinoCommon factory overload or validity sentinel.

`Construction` is the name for this concern because `Drawing` already means UI/display drawing and `Geometry` is already a domain-level concept in `Rasm`. The folder stays Rhino-first: RhinoCommon owns geometry validity, topology, transforms, and tolerances; Rasm adds typed construction intent, policy, diagnostics, projection, and integration with Blocks, Commands, UI, and Camera.

---
## [1][SOURCE_TRUTH]
>**Dictum:** *Construction APIs must be current WIP members, not remembered examples.*

<br>

| [INDEX] | [SOURCE] | [STATUS] | [USE] |
| :-----: | -------- | -------- | ----- |
| **[1]** | `uv run python -m tools.quality api doctor` | Verified RhinoWIP `9.0.26132.12306`. | Host version and API tooling. |
| **[2]** | `RhinoCommon.xml` | Primary. | Geometry type, factory, transform, and document insertion APIs. |
| **[3]** | `uv run python -m tools.quality api decompile rhino-common <type>` | Required for ambiguity. | Obsolete, hidden, internal, sentinel, and expert-only behavior. |
| **[4]** | `Rasm.Domain`, `Rasm.Vectors`, `Rasm.Rhino` source | Required. | Existing normalization, vector/numeric rails, UI/camera/document ownership. |

[VERIFY] Re-run source checks before implementation. Every named Rhino member requires local XML or decompile proof before code claims capability.

---
## [2][API_MAP]
>**Dictum:** *RhinoCommon remains the construction engine.*

<br>

| [INDEX] | [FAMILY] | [RHINO WIP SURFACE] | [CONSTRUCTION RAIL IMPLICATION] |
| :-----: | -------- | ------------------- | ------------------------------- |
| **[1]** | 2D values | `Point2d`, `Vector2d`, `Point2f`, `Vector2f`. | `Point2d`/`Vector2d` are public model-space 2D; `Point2f`/`Vector2f` stay internal display/interoperability. |
| **[2]** | Plane-backed intent | `Plane.PointAt`, `Rectangle3d`, `LineCurve(Point2d, Point2d)`, `Hatch.Create`. | `Plane + Point2d` means model-plane coordinates; surface UV and screen/window input stay separate. |
| **[3]** | Points/vectors | `Point3d`, `Vector3d`, `PointCloud`, validity and transform members. | Admit coordinates, clouds, sampling, and point/vector output through native validity. |
| **[4]** | Primitive curves | `Line`, `Polyline`, `Circle`, `Arc`, `Ellipse`, `Rectangle3d`. | Preserve value primitives; convert to curves only when output projection requires `GeometryBase`. |
| **[5]** | Curves | `Curve`, `LineCurve`, `PolylineCurve`, `ArcCurve`, `BezierCurve`, `NurbsCurve`. | Join, blend, fillet, tween, project, pull, interpolate, control-point, fit, offset, split, simplify, and boolean curves. |
| **[6]** | Text curves | `Curve.CreateTextOutlines`, `TextEntity` curve/surface/extrusion projection. | Treat text outlines and glyph projections as construction outputs, not UI drawing. |
| **[7]** | NURBS curves | `NurbsCurve.Create`, primitive conversions, fit points, H-spline, SubD-friendly, spiral, parabola, rail frames. | Use NURBS as output mode or advanced target, not universal internal format. |
| **[8]** | Surfaces | `Surface.CreateExtrusion`, `CreateExtrusionToPoint`, fillet/tween/periodic/soft-edit, `ToBrep`, `ToNurbsSurface`, `IsExtrusion`, `TryGetExtrusion`. | Keep operations native and preserve recoverable extrusion identity. |
| **[9]** | NURBS surfaces | `NurbsSurface.Create`, primitive conversions, corners, points, ruled, rail-revolved, `CreateNetworkSurface`. | Carry network-surface error result as diagnostics. |
| **[10]** | Breps | `Brep.TryConvertBrep`, primitive Breps, planar/trimmed surfaces, patch, pipe, sweep, loft, booleans, joins, splits, offsets, repairs, `Brep.FilletSurfaceSettings`, `CreateFilletSurface`, `CreateFilletEdges`. | Prefer current tolerance/settings overloads and route native metadata into diagnostics. |
| **[11]** | Extrusions | `Extrusion.Create`, box/cylinder/pipe extrusion, profile/wall/wireframe members, `ToBrep(bool splitKinkyFaces)`. | Preserve `Extrusion` until requested projection requires `Brep`. |
| **[12]** | Meshes | Primitive factories, planar boundary with tolerance, tessellation, Brep/Surface/SubD/Extrusion conversion, `MeshBooleanOptions`, `Mesh.Check`, `MeshCheckParameters`, cleanup, reduce, QuadRemesh. | Own native mesh construction; preserve `Tolerance`, `TextLog`, `CancellationToken`, `ProgressReporter`, out `Rhino.Commands.Result`, and before/after mesh checks; delegate vector/field algorithms to `Rasm.Vectors`. |
| **[13]** | SubD | `SubD.CreateFromMesh`, `CreateFromSurface`, sphere/cylinder/loft/sweep factories, `ToBrep`, `Mesh.CreateFromSubD`. | Treat SubD as native geometry, not mesh-only fallback. |
| **[14]** | Transforms | `Transform.Identity`, translation, scale, rotation, mirror, `PlaneToPlane`, `ChangeBasis`, `ProjectAlong`, shear, decompose/check members. | Separate object orientation from coordinate-description conversion; invalid `ProjectAlong` identity is a sentinel. |
| **[15]** | Bounds/frames | `GeometryBase.GetBoundingBox`, plane/transformed overloads, `BoundingBox.Transform`, `Box(Plane, GeometryBase)`. | Centralize framed bounds for Camera, UI, Gumball, previews, and block placement. |
| **[16]** | Document add | `AddRectangle`, `AddBox`, `AddSphere`, `AddText`, `AddLeader`, `AddTextDot`, `AddHatch`, `AddPointCloud`, `AddClippingPlane`, `AddClippingPlaneSurface`, `AddLinearDimension`, `AddAngularDimension`, `AddRadialDimension`, `AddOrdinateDimension`, `AddCentermark`. | Typed add APIs serve value structs, annotations, hatches, and clipping planes. |
| **[17]** | Document replace | `Replace` overloads for points/primitive curves, `TextEntity`, `Leader`, `TextDot`, `Hatch`, `Surface`, `Brep`, `Extrusion`, `Mesh`, `SubD`, `PointCloud`, `GeometryBase`. | Add and replace consume one construction projection path without mirrored local adapters. |
| **[18]** | Construction planes/history | `ConstructionPlane`, `NamedConstructionPlaneTable`, viewport cplane APIs, `HistoryRecord`. | Construction may produce plane/grid specs; Camera and document resources apply or persist them; `HistoryRecord` stays deferred and expert-only. |

Rejected or missing local APIs:
- `Rhino.DocObjects.ObjectInstance`: absent; use `InstanceObject`.
- Public `Line2d`, `Circle2d`, `Arc2d`, `Rectangle2d`: absent as Rhino geometry types.
- `GetBaseClass.Line2d()` and `Rectangle2d()` are command input screen/window result names.
- `Mesh.CreateFromClosedPolylinesAndPoints`: XML lists it, decompile shows `internal static`.
- `Line.Create`, `Circle.Create`, `Arc.Create`, `Ellipse.Create`, `EllipseCurve`, `Surface.TryConvertBrep`: false or suspect in local WIP.
- No-tolerance curve boolean/tween and planar-boundary style overloads are obsolete or rejected; use explicit tolerance/settings overloads. Older Brep fillet/chamfer signatures are separately obsolete.

---
## [3][TARGET_SHAPE]
>**Dictum:** *One construction algebra feeds blocks, documents, previews, and cameras.*

<br>

### [3.1][PUBLIC_RAIL]

`RhinoConstruction` is the public owner for construction projection. It exposes `RhinoConstruction.Project<TOut>(ConstructionOp op, Context context)`, uses `Context.Absolute.Value` and `Context.Angle.Value` for tolerance-bearing native calls, and keeps direct Rhino factory calls internal. Native null, bool, empty-array, unset, obsolete, and invalid results project into `Fin<T>`.

Operation families:
- Primitives: points, vectors, clouds, lines, polylines, circles, arcs, ellipses, rectangles, and plane-backed 2D intent.
- Curves/text: interpolate, control, fit, join, blend, fillet, boolean, project/pull, offset, split, simplify, text outlines.
- Surfaces/Breps/extrusions: extrusion, sweep, loft, patch, planar, trimmed, primitive solid, shell, offset, boolean, repair.
- Mesh/SubD: primitive mesh, tessellation, conversion, boolean, cleanup, reduce, QuadRemesh, SubD create/convert.
- Transforms/frames: orient, basis conversion, projection, mirror, scale, decompose, generated arrays, framed bounds.
- Output: native typed result, block-ready members, document-ready payload, preview geometry, bounds, transform, or diagnostics.

---
### [3.2][VALUE_ADD]

| [INDEX] | [CAPABILITY] | [VALUE] |
| :-----: | ------------ | ------- |
| **[1]** | Policy | Centralize tolerance, angle, unit, fractional, loose projection, manifold-only, split-kinky, setback, preserve-extrusion, mesh, and output ownership choices. |
| **[2]** | Native preservation | Keep primitives, `Extrusion`, `Mesh`, and `SubD` native until projection requires conversion. |
| **[3]** | Diagnostics | Preserve `NullResult`, `EmptyResult`, `FalseResult`, `InvalidGeometry`, `NativeErrorCode`, `ToleranceRaised`, `IndexMap`, diagnostic points, naked/non-manifold edges, offset blends/walls, `TextLog`, `Rhino.Commands.Result`, cancellation, and progress. |
| **[4]** | Batch/generation | Model arrays, repeated transforms, generated frames, preview batches, and per-item failures as first-class output. |
| **[5]** | Projection | Produce block-ready members, document-ready payloads, preview geometry, framed bounds, transforms, and native typed results. |
| **[6]** | Annotation output | Project text, leaders, dimensions, dots, hatches, and clipping planes without moving UI drawing or document mutation. |
| **[7]** | Numeric fitting | Use MathNet only after explicit Rhino coordinate/unit/tolerance projection; keep storage internal. |

---
### [3.3][FILE_ARCHITECTURE]

| [INDEX] | [FILE] | [OWNERSHIP] |
| :-----: | ------ | ----------- |
| **[1]** | `Construction.cs` | `RhinoConstruction` public owner and one `Project<TOut>` rail. |
| **[2]** | `Operations.cs` | `ConstructionOp` union for primitive, curve, surface/Brep/extrusion, mesh/SubD, transform/frame, annotation, bounds, and output projection operations. |
| **[3]** | `State.cs` | Compact specs, policies, output descriptors, ownership markers, and diagnostics. |

Deferred split thresholds:
- Create `Kernels.cs` only after native factory dispatch spans at least three operation families.
- Create `Frames.cs` only after frame/bounds logic has callers outside Construction.
- Create `Outputs.cs` only after document/block/preview output adaptation has at least two real consumers.

Do not create one file per Rhino geometry type.

---
### [3.4][EXTERNAL_STACK]

| [INDEX] | [OWNER] | [USE] |
| :-----: | ------- | ----- |
| **[1]** | RhinoCommon | Geometry creation, validity, topology, transforms, tolerance semantics, document add/replace surfaces. |
| **[2]** | `Rasm.Domain` | `GeometryKernel`, `Requirement`, `Context`, `Op`, acceptance, normalization, and reusable ownership semantics. |
| **[3]** | `Rasm.Analysis` | Bounds, oriented boxes, principal frames, metrics, and analysis-owned projections. |
| **[4]** | `Rasm.Vectors` | Mesh/vector/field algorithms, sparse/dense solve rails, spectral/geodesic/DEC concerns. |
| **[5]** | LanguageExt | `Fin`, `Option`, `Validation`, `Seq`, `TraverseM`, LINQ composition. |
| **[6]** | Thinktecture | `ConstructionOp`, value objects, smart enums, and generated dispatch. |
| **[7]** | MathNet | Fitting, solving, optimization, and statistics after explicit Rhino projection. |

---
## [4][CENTRALIZATION]
>**Dictum:** *Construction removes repeated shape classification from sibling folders.*

<br>

| [INDEX] | [CURRENT LOCATION] | [MOVE OR DELEGATE] | [REASON] |
| :-----: | ------------------ | ------------------ | -------- |
| **[1]** | `Commands/Document.cs` `GeometrySource.From`. | Delegate primitive/value/annotation adaptation to construction projection. | Current adapter covers only `GeometryBase`, `Point3d`, `Line`, `Circle`, `Arc`, `Ellipse`, and `Polyline`. |
| **[2]** | `Commands/Document.cs` `ReplaceGeometry`. | Delegate to same projection rail. | Add and replace require one owned/borrowed adaptation path. |
| **[3]** | `Commands/Input.cs` point constraints and 2D result modes. | Preserve; consume construction specs where useful. | Commands own live `GetPoint` and screen/window input. |
| **[4]** | `UI/Overlay.cs` `UiPreviewStyle.Draw`. | Preserve; consume preview-ready geometry and bounds. | UI owns drawing. |
| **[5]** | `UI/Overlay.cs` bounds and gumball specs. | Delegate framed bounds projection after parity. | Overlay, gumball, camera, and block placement need one projection truth. |
| **[6]** | `Camera` cplane and viewport operations. | Preserve; consume cplane/frame/bounds specs. | Camera owns live viewport and cplane application. |
| **[7]** | `Blocks` future definition creation. | Consume block-ready construction members. | Blocks should not recreate shape factories. |

[IMPORTANT] Do not route construction into a facade layer that only renames Rhino factories. Value comes from canonical intent, validity, policy, diagnostics, projection, and reusable output.

---
## [5][VALIDATION]
>**Dictum:** *Construction implementation proves every Rhino factory claim locally.*

<br>

Docs-only refinement gate:
- `uv run python -m tools.quality api doctor`.
- Targeted `uv run python -m tools.quality api xml rhino-common "<symbol>"` for every newly named Rhino member.
- `uv run python -m tools.quality api decompile rhino-common "<type>"` where XML is missing or ambiguous.
- Exact XML/decompile checks for rejected 2D types and hidden/internal mesh members.
- `git diff --check -- libs/csharp/Rasm.Rhino/Construction/ROADMAP.md`.

Future implementation gate:
- `dotnet build libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj --no-restore`.
- `uv run python -m tools.quality static check`.

Runtime Rhino verification is required only when construction behavior needs native document/runtime mutation beyond static compile proof.
