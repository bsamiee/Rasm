# [PY_ARTIFACTS_ARCHITECTURE]

The professional-domain map of `artifacts`: every concern is a sub-domain owning one polymorphic surface, every artifact keys by the runtime content key, and every receipt is one kind-discriminated family. The map fuels the ideas and tasks. The codemap tree below names the full sub-domain structure, each with a one-line charter; one sub-domain folder mirrors one eventual source sub-tree.

## [1]-[DOMAIN_MAP]

```text codemap
artifacts/
├── documents/          # one DocumentPlan mode axis: PDF author/render/raster/assemble/repair + Office + structured-text on a backend-per-mode table
├── reporting/          # the ReportPlan composition axis binding figures/sections into a document tree over jinja2 templating + papermill/nbclient notebook execution
├── charts/             # the 2D chart union (altair/plotly/matplotlib) with host-free static export (vl-convert primary, kaleido gated)
├── scene3d/            # pyvista/VTK offscreen 3D scientific render + glTF/VRML scene export on the gated native floor
├── tables/             # the great-tables publication-table owner exporting HTML/LaTeX/PDF
├── imaging/            # the raster/preview owner over pillow + scikit-image + qrcode + python-magic
├── color-management/   # the colour-science colorimetry/spectral/CAM16/palette owner feeding the visual sub-domains
├── typography/         # the fonttools subset/instance + pyhanko PAdES-signing PDF conformance close
├── compression/        # the algorithm-row compression/bundle owner over zstandard/lz4/brotli/py7zr
└── receipt/            # the shared kind-discriminated ArtifactReceipt family across every production mode
```

`receipt` is the shared owner every visual and document sub-domain contributes one case to, never a parallel per-producer receipt rail; `color-management` is the one upstream color source the visual sub-domains pull palettes from rather than each engine picking color ad hoc. The host-free posture is the structural axis cutting every sub-domain: vl-convert-python is the primary chart engine, matplotlib and pyvista render offscreen on a software-GL floor, and the kaleido host-Chrome and great-tables Selenium paths are gated optional, never the default.
