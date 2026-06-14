# [GRASSHOPPER_HOST]

Grasshopper 2 host behavior comes from local GH2 XML, decompile confirmation when XML is silent, the API rail, owner-owned bridge scenarios, and the `Rasm.Grasshopper` boundary.

## [1]-[READ_ROUTE]

- Member availability: local GH2 XML and the API rail.
- Missing XML semantics: decompile confirmation or runtime bridge scenario.
- Boundary design: `Rasm.Grasshopper` and architecture routes.
- Runtime confirmation: owner-owned typed `[RhinoScenario]` sources through the bridge rail.

## [2]-[BOUNDARIES]

Grasshopper docs choose confirmation routes; `Rasm.Grasshopper` owns public boundary shape. Keep data access, tree paths, canvas behavior, diagnostics, subscriptions, and paint hooks inside GH2 boundary rails before exposing project concepts.

## [3]-[BOUNDARIES]

- GH2 member catalogs stay in generated or API-owned surfaces.
- Tree/path behavior stays tree-shaped.
- Scenario confirmation uses the bridge-owned route when GH2 execution requires it.
