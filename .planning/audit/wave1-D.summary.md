# Wave 1 Agent D: Bridge + Radyab Boundary Audit

## (i) Bridge Architecture

The bridge layer (`libs/csharp/grasshopper/`, 297 LOC across three files) translates between the Grasshopper2 component runtime and the analysis layer's `Eff<AnalysisRuntime, Seq<TOut>>` rail. `Bridge.cs` (113 LOC) owns scope resolution, monadic execution via `WithStandardResilience().Run(scope).Match(...)`, and twig writes. `AnalysisComponent.cs` (160 LOC) is a polymorphic `Component<TInput>` base owning input/output declaration plus the `Process(IDataAccess)` execution loop; subclasses declare `Outputs` as `Seq<IBridgeOutput<TInput>>` plus optional `IndexInputSpec`. `PluginBase.cs` (24 LOC) is reflection-driven plugin metadata. The Radyab plugin (136 LOC across 4 files) is two thin Rhino+GH2 plugin shells (33 LOC) plus two concrete `AnalysisComponent<object>` subclasses with hand-rolled 5-output `Seq` literals. The monadic core is ROP-clean (no `if`/`while`/`throw` outside the labelled boundary adapter), but `ParameterFactory`, the `<object>` generic argument, and the hand-rolled output declarations are three separate hand-rolls of capabilities GH2 already provides.

## (ii) ParameterFactory SmartEnum Proposal

Replace the `HashMap<Type, Func<...>>` registry (`AnalysisComponent.cs:55-67`) with Thinktecture `[SmartEnum<GeometryParameterKind>]`:

```csharp
[SmartEnum<GeometryParameterKind>]
public partial class GeometryParameterKind {
    public static readonly GeometryParameterKind Point = new(typeof(Point3d), static (i,n,c,d,a,r) => i.AddPoint(n,c,d,a,r));
    public static readonly GeometryParameterKind Curve = new(typeof(Curve), static (i,n,c,d,a,r) => i.AddCurve(n,c,d,a,r));
    public static readonly GeometryParameterKind Brep  = new(typeof(Brep),  static (i,n,c,d,a,r) => i.AddSurface(n,c,d,a,r));
    public static readonly GeometryParameterKind Index = new(typeof(int),   static (i,n,c,d,a,r) => i.AddIndex(n,c,d,a,r));
    // full coverage of GH2's 50+ standard parameters
    public Type ClrType { get; }
    public Func<InputAdder, string, string, string, Access, Requirement, IParameter> Add { get; }
}
```

Generated `Switch`/`Map` makes the dispatch exhaustive at compile time; a new geometry type without a member fails to type-check. Eliminates `(IParameter)new XParameter(...)` allocations — the typed `AddX` returns the strongly-typed parameter directly. Net LOC delta: -5 to +0 with full GH2 coverage; surface break: none for callers (the bridge keeps `Build(Type)`-shape entry).

## (iii) `<TGeometry : GeometryBase>` vs `<object>`

**Cannot use `<TGeometry : GeometryBase>` directly.** The analysis Query layer accepts value-type primitives (verified in `Extract.cs`/`Locate.cs`/`Measure.cs` switch arms): `Box`, `BoundingBox`, `Line`, `Polyline` (when separate), `Plane`, `Sphere`, `Cylinder`, `Cone`, `Torus`, `Circle`, `Arc` are CLR structs. Correct shape: closed `[Union] RhinoGeometry { Native(GeometryBase), Box, BoundingBox, Line, Plane, Sphere, Cylinder, Cone, Torus, Circle, Arc }`. `AnalysisComponent<TInput> where TInput : RhinoGeometry`. The `Process` switch at `AnalysisComponent.cs:149-153` then collapses to `access.GetItem<TInput>(0, out TInput input) switch { true => ..., false => HandleMissingInput(access) };` — no runtime type-cast at the boundary. Cross-cutting with Agent C (core/Domain owns `RhinoGeometry`).

## (iv) GH2 Framework Features Under-Utilised

Verified via reflection on `Grasshopper2.dll@2.0.9225-wip.14825`: (1) `InputAdder.AddIndex(name, code, info, access, requirement)` exists, hand-rolled as `BuildIndex` + `AddIndexSlot` (D-005, D-014). (2) `InputAdder/OutputAdder.Add{Curve|Brep|Mesh|Point|Vector|Box|Plane|...}` typed methods cover the full 50+ standard set, replicated as `ParameterFactory.Registry` (D-002). (3) `IDataAccess.GetItem<T>(int, out T)` is generic; bridge calls `<object>` then runtime-pattern-matches, bypassing GH2's `TypeAssistant<T>` coercion (D-004). (4) `Plugin.Assembly` auto-resolves the calling assembly — `PluginBase.cs:14` redundantly accepts `Assembly source` (D-010). (5) `IDataAccess.SetTwig<T>(int, T[], MetaData[], bool[])` is the universal twig writer; the `WriteValues` wrapper exists only to allocate metas/nulls arrays (D-009). The bridge does not use `TypeAssistant<T>`/`TypeUnification`/`TypeHierarchy` at all.

## (v) What Is Good

The monadic backbone is correct. `RunOne` uses `query.Apply(geometry).WithStandardResilience().Run(scope).Match(...)` — canonical LanguageExt v5 ROP, no mid-pipeline `.Run()` (CSP0303 clean), no exception flow on success, typed `Error` propagation, declarative resilience via `Schedule.exponential | Schedule.recurs | Schedule.jitter` in `Resilience.cs`. `BridgeOutput<TInput, TValue>` is a clean polymorphic seam: `IBridgeOutput<TInput>` erases the value type while the concrete record retains it. `Outputs.Iter((slot, output) => output.Execute(...))` is a pure fold. `IndexInput.Match` records index in `AnalysisRuntime`'s context rather than threading it through every Query. `AcceptedGeometry` constant makes error messages actionable. `BOUNDARY ADAPTER` markers correctly tag the two non-functional points (`Bridge.cs:79-80`, `Analyze.cs:140-146`).

## Further Considerations

- D-001/D-002/D-003 are co-dependent — the SmartEnum, typed `AddX` migration, and `RhinoGeometry` Union land as one refactor. Wave-2 sequencing: Agent C ships `RhinoGeometry` first; bridge work follows.
- GH2's `TypeAssistant<T>` is the long-term coercion seam — registering a per-Union-case assistant lets Grasshopper trees coerce arbitrary native shapes into the canonical Union, moving dispatch from the bridge into GH2's type system.
- The two plugin Guids (`694d39bb...` Rhino .rhp, `6fbf37d9...` GH2) are intentional and load-bearing — Rhino's `HostUtils.CreatePlugIn` and Grasshopper2.Framework require dual registration. Do not consolidate.
