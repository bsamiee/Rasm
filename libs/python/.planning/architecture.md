# [PYTHON_ARCHITECTURE]

`libs/python` is a root-manifest Python library suite with four first-wave package folders. Package folders carry README, `.planning`, `.api`, and future source directly under the package root.

## [1]-[TOPOLOGY]

The codemap shows the active no-source campaign layout and the admitted future source roots.

```text codemap
libs/python/
├── .planning/
│   ├── README.md
│   ├── architecture.md
│   ├── campaign-method.md
│   ├── api-catalogues.md
│   ├── FEATURES.md
│   ├── TASKLOG.md
│   └── region-map/
├── runtime/
│   ├── README.md
│   ├── .planning/
│   └── .api/
├── data/
│   ├── README.md
│   ├── .planning/
│   └── .api/
├── compute/
│   ├── README.md
│   ├── .planning/
│   └── .api/
└── artifacts/
    ├── README.md
    ├── .planning/
    └── .api/
```

Text equivalent: `libs/python/.planning` owns suite law, each package owns its local `.planning` and `.api` evidence, and future Python source lands directly in `libs/python/<package>` rather than behind a nested source namespace.

## [2]-[DEPENDENCY_DIRECTION]

- `runtime` imports no first-wave Python package.
- `data` consumes runtime contracts when source exists and rejects compute/artifacts interiors.
- `artifacts` consumes runtime contracts when source exists and accepts data/compute output only as file or spec bundles.
- `compute` consumes runtime, data bundle shapes, and artifact bundle shapes when source exists.
- No Python package imports C# interiors, product host lifecycles, durable store repositories, bridge lifecycle code, or TypeScript UI state.

## [3]-[MODULE_RULES]

- Package folders are direct future source roots.
- Empty package marker files are absent.
- Public symbols live in the file that owns implementation.
- Heavy scientific, document, visualization, and native packages enter through owner-named boundary adapters only after `.api` evidence names their relevant surfaces.
- The root `pyproject.toml` owns dependency admission; package-local manifests stay absent.
