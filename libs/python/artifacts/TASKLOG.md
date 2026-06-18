# [PY_ARTIFACTS_TASKLOG]

The folder's open and closed work, distilled from `IDEAS.md`. Open tasks are cards in `[1]-[OPEN]` with a `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` leader; closed tasks move to `[2]-[CLOSED]` with `[COMPLETE]`/`[DROPPED]`. Each task names the exact sub-domain or file it lands in.

## [1]-[OPEN]

[ACTIVE] Host-free chart-export floor on `charts/chart-spec.md`
- Build the `_export_host_free` fold so `ChartExport` routes vega through vl-convert-python `vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf` (primary, cp315-core) and matplotlib through the gated-band `_matplotlib_savefig` worker, both host-free.
- Integrate `vl-convert-python` and `kaleido` on the cp315 core; `matplotlib` is gated `python_version<'3.15'`, so its `Figure.savefig` Agg/PDF/SVG arm runs on the runtime subprocess seam; the plotly arm rides `_plotly_via_chrome`, engaged only when host Chrome is detected (requires plotly.py >=6.1.1 for the kaleido v1 Choreographer driver).
- Wire internal: `charts/chart-spec#EXPORT` keys outputs by the runtime content key, contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Chart`, and dispatches the gated matplotlib arm onto `runtime` `anyio.to_process.run_sync` (the runtime subprocess lane); no browser, display, or sibling coupling.
- Key consideration: kaleido v1's host-Chrome dependency is the host-coupling defect to gate behind detection; vl-convert is the reproducible default; the cp315-core process imports no gated distribution, so the matplotlib arm crosses the subprocess seam; the export format set is SVG/PNG/PDF.

[QUEUED] Offscreen software-GL floor on `scene3d/scene.md`
- Build the gated-band `_render_scene` worker over an offscreen `pyvista.Plotter(off_screen=True)` rendering PNG via `screenshot` and exporting glTF/VRML via `export_gltf`/`export_vrml`, surface-extracting `UnstructuredGrid` to PolyData before glTF; `Scene3d.render` dispatches the whole render onto the subprocess seam.
- Integrate `pyvista` and `vtk` on the gated `python_version<'3.13'` sub-3.13 companion floor; select the osmesa/EGL software-GL backend for the host-free path; the cp315-core owner imports neither and crosses the seam.
- Wire internal: `scene3d/scene#SCENE` consumes data/compute arrays as inputs, draws colormaps from `color-management/colorimetry#COLOR`, dispatches the render onto `runtime` `anyio.to_process.run_sync` (the runtime subprocess lane), keys by the content key, and contributes `ArtifactReceipt.Scene`; owns no mesh-file interchange.
- Key consideration: floor-gated planned capability, not a blocked spike — the sub-3.13 companion floor hosts the worker, and the cp315-core process never imports pyvista/vtk; the VTK glTF exporter handles only PolyData.

[QUEUED] Reproducible notebook report on `reporting/report-plan.md`
- Extend `ReportPlan` with the `NOTEBOOK` kind executing a parameterized notebook headlessly through papermill on the nbclient engine into an executed-notebook archive, then rendering it into the DocumentPlan tree.
- Integrate `papermill` (`execute_notebook`) and `nbclient` (`NotebookClient`); the rendered tree hands to `documents/document-plan#DOCUMENT`.
- Wire internal: `reporting/report-plan#REPORT` binds figure content keys from `charts`/`tables`/`scene3d`, keys the executed-notebook archive and the rendered tree by the content key, and contributes `ArtifactReceipt.Report`.
- Key consideration: the executed notebook is the audit/reproducibility artifact; the jinja2 `TEMPLATE` kind cannot execute computation, only the notebook kind can.

[QUEUED] Publication-table owner on `tables/table-plan.md`
- Build `TablePlan` over great-tables `GT` producing styled tables (spanners, value formats, data-driven coloring) exported to HTML/LaTeX/PDF; admit great-tables into the Python manifest.
- Integrate `great-tables` (`GT`/`fmt_*`/`tab_spanner`/`data_color`/`as_raw_html`/`as_latex`/`save`); author its folder `.api` catalogue on admission.
- Wire internal: `tables/table-plan#TABLE` consumes a settled polars/pandas dataframe, draws fills from `color-management/colorimetry#COLOR`, feeds `reporting`/`documents`, keys by the content key, and contributes `ArtifactReceipt.Table`.
- Key consideration: the great-tables PNG path rides host-coupled Selenium — gate it optional and keep HTML/LaTeX/PDF as the host-free default.

[QUEUED] PDF conformance and signing on `typography/conformance.md`
- Build `Conformance` closing a document-axis PDF: fonttools `subset.Subsetter`/`varLib.instancer` subsets and instances embedded fonts, pyhanko applies PAdES B-B/B-T/B-LT/B-LTA signatures with timestamp and validation material.
- Integrate `fonttools` and `pyhanko`; author their folder `.api` catalogues; engage the uharfbuzz repacker when present.
- Wire internal: `typography/conformance#CONFORM` consumes a PDF from `documents/document-plan#DOCUMENT`, keys the signed output by the content key, and contributes `ArtifactReceipt.Pdf`.
- Key consideration: pyhanko enforces no PDF/A or PDF/UA structural conformance — structure is authored upstream at the document axis; signing only adds the cryptographic layer.

[QUEUED] Color-managed visual pipeline on `color-management/colorimetry.md`
- Build `Colorimetry` over colour-science providing color-space conversion, spectral-to-display, CIECAM02/CAM16 appearance correlates, and perceptually-uniform colorblind-safe palette derivation.
- Integrate `colour-science` (`convert`/`sd_to_XYZ`/`XYZ_to_sRGB`/`XYZ_to_CAM16`) on its NumPy backing; author its folder `.api` catalogue.
- Wire internal: `color-management/colorimetry#COLOR` emits color arrays and palettes consumed by `charts`/`scene3d`/`tables` and the document output; emits no rendered artifact, so it carries no receipt case.
- Key consideration: one color owner replaces ad-hoc per-engine color so every visual artifact is perceptually consistent and reproducible; pure-Python and host-free.

[QUEUED] Imaging scientific-transform deepening on `imaging/preview.md`
- Add the scikit-image `TRANSFORM` arm (scientific transforms, segmentation, measurement) to the gated-band `_gated_raster` worker alongside the pillow thumbnail/convert/montage arms; the cp315-core `QR` (qrcode) arm and python-magic media detection stay in-process.
- Integrate `scikit-image` and `pillow` on the gated `python_version<'3.15'` band (run on the subprocess seam), `qrcode` and `python-magic` on the cp315 core; author the scikit-image folder `.api` catalogue.
- Wire internal: `imaging/preview#PREVIEW` keys outputs by the content key, dispatches the gated raster arms onto `runtime` `anyio.to_process.run_sync` (the runtime subprocess lane), and contributes `ArtifactReceipt.Preview`; owns no live viewer.
- Key consideration: the cp315-core process imports neither pillow nor scikit-image, so the raster arms cross the subprocess seam onto the gated band; the `QR` and media-detection arms stay in-process.

## [2]-[CLOSED]

No closed tasks.
