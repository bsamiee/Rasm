# [PY_ARTIFACTS_IDEAS]

The folder's forward pool of higher-order concepts, each grounded in artifact production and the host-free companion charter. Open ideas are cards in `[1]-[OPEN]`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition. Each idea drives one or more `TASKLOG.md` tasks.

## [1]-[OPEN]

[HOST_FREE_RENDER_FLOOR]
- Make vl-convert-python the primary chart/Vega export engine and the offscreen software-GL path (osmesa/EGL) the floor for pyvista, so every visual artifact renders with zero browser, display, or GPU; kaleido is a host-Chrome-gated degraded path, never the default. The gated render engines (matplotlib, pyvista/vtk) never resolve in the cp315-core process and run on the runtime subprocess seam onto their gated band.
- Unlocks a genuinely host-free, reproducible, server/CI-safe visual rail — charts and 3D scenes render bit-deterministically and key by content, matching the companion charter and the reproducibility law.
- Draws on kaleido v1 dropping its bundled Chrome for a host Chrome via its Choreographer driver (a host-coupling defect), vl-convert's embedded V8 + bundled Vega emitting SVG/PNG/PDF browser-free, VTK's offscreen software-GL support, and the runtime `anyio.to_process.run_sync` seam isolating the gated-band render workers from the cp315 core.

[REPRODUCIBLE_NOTEBOOK_REPORT]
- A reporting sub-domain where papermill/nbclient execute a parameterized notebook headlessly into an executed-notebook artifact, then render it into the DocumentPlan tree as a signed, content-keyed report.
- Unlocks parameter-driven, archival, audit-grade analytical reports from data and compute studies — the executed notebook is the reproducibility receipt and the rendered PDF/HTML is the deliverable, both content-keyed.
- Draws on papermill + nbclient admitted with no page home and the current jinja2-only ReportPlan being unable to execute computation; parameters-tagged notebook execution is the standard reproducible-report pattern.

[PUBLICATION_TABLE_OWNER]
- A tables sub-domain over great-tables (Bring-Your-Own-DataFrame, polars/pandas) producing publication-quality styled tables — spanners, value formatting, data-driven coloring, inline sparklines — exported to HTML/LaTeX/PDF and embedded into reports and documents.
- Unlocks first-class tabular artifacts, the missing third pillar beside documents and charts, so numeric study results render as styled formatted tables rather than raw dumps, feeding the report tree directly.
- Draws on no tabular owner existing in the folder and great-tables not yet admitted; great-tables is the publication-table standard from polars/pandas and is pure-Python except its optional Selenium PNG path.

[PDF_CONFORMANCE_AND_SIGNING]
- A typography sub-domain closing a PDF emitted by documents into a conformant, font-embedded, digitally-signed archival artifact: fonttools subsets/instances embedded fonts, pyhanko applies PAdES B-LT/B-LTA signatures with timestamp and validation material.
- Unlocks archival, legally-signable, long-term-validatable PDF deliverables with a minimal embedded-font footprint — a production-grade close on the document rail.
- Draws on fonttools + pyhanko admitted with no page home and the document rail emitting raw PDF with no subsetting, signing, or archival step; pyhanko supplies the full PAdES baseline family but enforces no PDF/A or PDF/UA structural conformance, so structure is authored upstream.

[COLOR_MANAGED_VISUAL_PIPELINE]
- A color-management sub-domain over colour-science providing one CIE colorimetry / spectral / CIECAM02-CAM16 appearance owner that hands consistent, perceptually-uniform palettes and color-space conversions to charts, scene3d, tables, and PDF output.
- Unlocks perceptually-correct, reproducible color across every visual artifact — spectral-to-display conversion for scientific and material data, colorblind-safe CAM16-derived palettes shared by 2D charts and 3D scenes.
- Draws on colour-science admitted with no page home and each visual engine currently picking color ad hoc; colour-science is the NumFOCUS-affiliated standard for Python colorimetry, pure-Python and host-free.

[SCIENTIFIC_IMAGE_ANALYSIS]
- Deepen the imaging sub-domain past pillow raster I/O into the scikit-image scientific layer: geometric transforms, segmentation, region measurement, and feature extraction over the array image, so a scan or render output is not just thumbnailed but measured and annotated.
- Unlocks quantitative image artifacts — segmented overlays, measured region tables, feature-annotated previews — that feed the report tree and the table owner, turning a raster preview into an analytical deliverable rather than a decoration.
- Draws on scikit-image being the SciPy-ecosystem image-analysis standard (NumPy-array-native, host-free) and the current `Preview` owner stopping at pillow raster operations with no measurement or segmentation arm; scikit-image rides the gated `python_version<'3.15'` band and runs on the runtime subprocess seam, never resolving in the cp315 core.

## [2]-[CLOSED]

No closed ideas.
