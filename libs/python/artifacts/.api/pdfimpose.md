# pdfimpose

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| Saddle-stitch imposition | `pdfimpose.schema.saddle.impose` | `.planning/composition/imposition.md` |
| Wire binding imposition | `pdfimpose.schema.wire.impose` | `.planning/composition/imposition.md` |
| Cards and cut/fold imposition | `pdfimpose.schema.cards.impose`, `pdfimpose.schema.copycutfold.impose`, `pdfimpose.schema.cutstackfold.impose` | `.planning/composition/imposition.md` |
| Shared imposition substrate | `pdfimpose.schema.AbstractImpositor`, `pdfimpose.schema.Matrix`, `pdfimpose.schema.Page`, `pdfimpose.schema.Margins`, `pdfimpose.schema.nocreep` | `.planning/composition/imposition.md` |
| Hardcover schema | `pdfimpose.schema.hardcover.impose` | `.planning/composition/imposition.md` |

## Integration Rules

- `composition/imposition` owns every `pdfimpose` call; document egress delegates to the composition owner for imposed PDF output.
- `pdfimpose` page-order and sheet matrices normalize into local `Placement`, `ImposedPlan`, `Layer`, and `ArtifactReceipt` facts before any document or export layer consumes them.
- Provider-specific schema names do not cross the owner boundary; downstream callers select local `Scheme`, `Geometry`, and `ImpositionEngine` values.
- `impose(files: Sequence[str | pathlib.Path | io.BytesIO], output: str | pathlib.Path | io.BytesIO, *, ...)` is the stable schema-wrapper shape across `saddle`, `wire`, `cards`, `copycutfold`, `cutstackfold`, `hardcover`, and `onepagezine`; local code spills bytes to `io.BytesIO` only inside the worker/provider wrapper.
