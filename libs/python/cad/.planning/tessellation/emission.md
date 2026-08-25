# [PY_CAD_EMISSION]

`emitted` is the provider's one glTF writer and the byte gate standing behind it: it drives `RWGltf_CafWriter.Perform` over an already-meshed XCAF document onto a call-owned path, then re-reads that path's extent and admits it against the call's artifact ceiling. Emission returns the admitted extent and nothing else — the GLB body never crosses the `anyio.to_process` pipe, and the parent that owns the path is the only holder of the bytes.

Byte admission here is EVIDENCE, never preemption. OCCT's bound filename writer exposes no bounded output callback and no incremental stream seam, so a runaway document is already fully written by the time `stat()` reads it; confining that write is the app root's job through the worker's own filesystem and memory quotas, which `service/spool#SPOOL` composes as the call-owned path lifetime. `tessellation/mesh#MESH` sequences this owner between preflight and census, and `metrology/census#CENSUS` re-opens exactly the bytes written here.

## [01]-[INDEX]

- [02]-[EMISSION]: `Perform` under its required metadata and progress arguments, the two-verdict extent gate, and the absent-callback boundary.

## [02]-[EMISSION]

- Owner: `emitted` — one writer call per admitted document, returning the byte extent the artifact gate admitted so the publish confirms against a measured number.
- Law: `RWGltf_CafWriter(TCollection_AsciiString(path), True)` writes the binary container; the boolean is the GLB-versus-glTF discriminant and this provider emits GLB alone, so no caller carries a container knob.
- Law: `Perform` demands `fileInfo` and `progress` positionally — neither is optional, so passing an empty map and an empty range is argument satisfaction rather than a declared capability left unread.
- Law: `TColStd_IndexedDataMapOfStringString` goes in EMPTY, so no glTF asset metadata is emitted: product identity, schema pin, and source digest reach a consumer through the artifact receipt alone, never through the container.
- Law: `Message_ProgressRange()` is constructed empty and never read, so a long native fold reports nothing and a deadline cannot observe its progress; cancellation stays the lane's scope-level concern at `service/lane#LANE`.
- Law: `Perform` returning false refuses on `EMIT_WRITE` — OCCT publishes no typed failure reason for the writer, so the coordinate names the call and the partial file is the call-owned path's own cleanup.
- Law: a zero extent and an over-ceiling extent are DIFFERENT refusals — zero means the writer reported success over bytes it never produced and grades `EMIT_WRITE`, while over-ceiling is an admission verdict on real output and grades `EMIT_EXTENT`.
- Law: the old single `artifact-bytes:{n}` coordinate collapsed both onto one row, so a caller reading the receipt had no way to tell a silent write failure from a source that legitimately outgrew its budget.
- Law: `stat()` raising is a host failure on a path the writer just claimed to have filled, so it grades `EMIT_WRITE` beside the writer's own false verdict rather than minting a third disposition.
- Boundary: post-write extent is ADMISSION EVIDENCE, never a filesystem-growth bound — the bound OCCT filename writer exposes no bounded output callback, so the disk is already committed when the gate reads it, and confining that write belongs to the app root's worker quotas and the call-owned spool.
- Boundary: this owner writes bytes and admits their extent. Counting what those bytes decode to belongs to `metrology/census#CENSUS`, and publishing them to `service/spool#SPOOL`.

```python signature
from pathlib import Path

from OCP.Message import Message_ProgressRange
from OCP.RWGltf import RWGltf_CafWriter
from OCP.TColStd import TColStd_IndexedDataMapOfStringString
from OCP.TCollection import TCollection_AsciiString
from OCP.TDocStd import TDocStd_Document
from expression import Error, Ok

from rasm.cad.faults import EMIT_EXTENT, EMIT_WRITE, CadRail

# --- [OPERATIONS] -----------------------------------------------------------------------


def _written(document: TDocStd_Document, glb_path: Path, /) -> CadRail[Path]:
    writer = RWGltf_CafWriter(TCollection_AsciiString(str(glb_path)), True)
    performed = writer.Perform(document, TColStd_IndexedDataMapOfStringString(), Message_ProgressRange())
    return Ok(glb_path) if performed else Error(EMIT_WRITE.at("RWGltf_CafWriter.Perform"))


def _extent(glb_path: Path, ceiling: int, /) -> CadRail[int]:
    try:
        artifact_bytes = glb_path.stat().st_size
    except OSError as cause:
        return Error(EMIT_WRITE.at(f"artifact.stat:{cause.errno}"))
    return (
        Error(EMIT_WRITE.at("artifact-bytes:0"))
        if artifact_bytes == 0
        else Ok(artifact_bytes)
        if artifact_bytes <= ceiling
        else Error(EMIT_EXTENT.at(f"artifact-bytes:{artifact_bytes}>{ceiling}"))
    )


def emitted(document: TDocStd_Document, glb_path: Path, ceiling: int, /) -> CadRail[int]:
    return _written(document, glb_path).bind(lambda written: _extent(written, ceiling))
```

## [03]-[RESEARCH]

- [GLTF_METADATA]-[OPEN]: does `RWGltf_CafWriter` surface `fileInfo` entries into the emitted container's `asset.extras` where a decoder reads them back, and which key spellings survive the round trip; write one document with a probe map, re-read it through `trimesh.load_scene`, then either grow the map from the admitted `exchange/identity#CANONICAL` fields or delete the argument's ambition and say so.
- [PROGRESS_ARMING]-[OPEN]: does `Message_ProgressRange` arm from a `Message_ProgressIndicator` so a long `Perform` reports progress a deadline scope observes; probe the installed `OCP.Message` surface, land the member roster on `.api/cadquery-ocp.md`, and route the cancellation consequence to `service/lane#LANE`.
