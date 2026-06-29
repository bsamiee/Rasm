# python-xmp-toolkit

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| XMP packet object | `libxmp.XMPMeta`, `XMPMeta(xmp_str=...)` | `.planning/exchange/metadata.md` |
| XMP property read/write | `XMPMeta.get_property`, `XMPMeta.set_property` | `.planning/exchange/metadata.md` |
| XMP serialization | `XMPMeta.serialize_to_str` | `.planning/exchange/metadata.md` |
| XMP file packet access | `libxmp.XMPFiles` | `.planning/exchange/metadata.md` |
| XMP namespace constants | `libxmp.consts` | `.planning/exchange/metadata.md` |

## Integration Rules

- `exchange/metadata` owns compound XMP packet semantics; `pikepdf.open_metadata()` remains the PDF scalar XMP path.
- XMP namespace and property spellings collapse into local `FieldKey` and `MetaFacts` projections at the boundary.
- Provider packet objects never cross into document, export, or receipt owners.
