# [TS_UI_API_PERSPECTIVE_DEV_VIEWER_CHARTS]

`@perspective-dev/viewer-charts` paints the declared-chart half of `<perspective-viewer>`: importing the root registers the bundled WebGL chart roster against the viewer's plugin registry, each chart selected through the config's `plugin` field, driven by its column assignments, and tuned through `plugin_config` inside the one config value.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@perspective-dev/viewer-charts`
- package: `@perspective-dev/viewer-charts` (Apache-2.0)
- module: ESM only — `.` exports `register` and the event-detail types; `./src/*`/`./dist/*` passthroughs
- runtime: browser custom element over a bundled WebGL renderer; evaluating the root import registers the chart roster
- rail: view/chart — `chart.md` composes the bare side-effect import beside the viewer boot

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the chart interaction event details

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                      |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `PerspectiveSelectDetail` | interface     | `perspective-select` detail, re-exported from the viewer surface                  |
|  [02]   | `PerspectiveClickDetail`  | interface     | `perspective-click` detail — `row`, `column_names`, `config` restore-filter patch |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: roster registration at import

| [INDEX] | [SURFACE]               | [SHAPE] | [CAPABILITY]                                                                |
| :-----: | :---------------------- | :------ | :-------------------------------------------------------------------------- |
|  [01]   | `register(...string[])` | static  | narrow registration to the named charts; the bare root import registers all |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every chart is one plugin registered at import and selected through the config's `plugin` field; the config's column assignments drive its channels, and interaction returns as element events with typed `detail`.

[STACKING]:
- `@perspective-dev/viewer`(`.api/perspective-dev-viewer.md`): the side-effect import registers these charts against the viewer's plugin registry; `restore({ plugin, plugin_config })` selects a chart and its options and `save()` round-trips them, so this surface paints while the viewer owns the config value.
- within-lib: `chart.md` evaluates the bare import once beside the viewer boot and reads `perspective-click`/`perspective-select` through the typed `detail`, never chart internals.

[LOCAL_ADMISSION]:
- Admission pairs this chart plugin with `@perspective-dev/viewer`; the two release lockstep.

[RAIL_LAW]:
- Package: `@perspective-dev/viewer-charts`
- Owns: every declared-chart presentation of the viewer — plugin registration at import, channel assignment from the config's columns, chart interaction as element events with typed `detail`.
- Accept: the bare root import evaluated once at chart-plane module scope; `register(names)` only where a build narrows the roster; chart options under `plugin_config` on the config value.
- Reject: `viewer-d3fc` or `viewer-openlayers` beside it; reading chart internals over the typed event details; the chart family standing in for Plot-regime declared statistical graphics.
