# [PYTHON_FEATURES]

This atlas maps high-order Python capability to package owners. Rows name capability, not implementation state.

| [FEATURE] | [OWNER] | [CONSUMERS] | [BOUNDARY] |
| :-------- | :------ | :---------- | :--------- |
| Python context admission | runtime | all Python packages, Assay internals | receives host facts, never starts hosts |
| boundary rails | runtime | all Python packages | exceptions convert at boundary only |
| resource roots | runtime | data, artifacts, tools | paths come from caller-owned roots |
| API catalogue evidence | runtime | all package `.api` folders | names package surfaces before source |
| columnar scan plans | data | Persistence imports, AppUi/TS data feeds, tools | no durable store writes |
| Arrow and Parquet bundles | data | Persistence, compute, artifacts | interchange, not app schema |
| geospatial/AEC exchange | data | Rhino/GH import flows, analysis tools | file records only, no host mutation |
| graph and network payloads | data | studies, diagnostics | graph algorithms, not product state |
| array admission | compute | studies, numeric experiments | metadata admission before backend execution |
| solver comparison | compute | future C# kernel work | offline study, not product runtime |
| units and uncertainty | compute | numeric studies | evidence propagation only |
| model asset preparation | compute | C# Compute | validates assets, never runs production sessions |
| PDF and Office control | artifacts | support bundles, reports, documentation tools | file artifacts, not UI |
| image and preview export | artifacts | AppUi/TS consumers | generated previews, not render surfaces |
| high-quality visualization export | artifacts | reports, app assets, study output | static/spec artifacts, not dashboard runtime |
| compression and bundle identity | artifacts | data, compute, support bundles | content-addressed files |
| visuals candidate | artifacts lane | downstream apps/plugins | package split requires visual grammar pressure |
| exchange candidate | data lane | AEC and file import tools | package split requires independent interchange volume |
| studies candidate | compute lane | scientific campaigns | package split requires reusable experiment orchestration |
| services candidate | deferred | sidecars and app roots | named app consumer required |
| automation candidate | deferred | controlled acquisition tools | browser, scrape, and desktop automation hazards require explicit consumer |
