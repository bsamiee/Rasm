# exif

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| EXIF image object | `exif.Image` | `.planning/exchange/metadata.md` |
| EXIF presence and tag access | `Image.has_exif`, `Image.get`, `Image.get_all`, dynamic EXIF tag attributes | `.planning/exchange/metadata.md` |
| EXIF field write | `Image.set`, `Image.delete_all` | `.planning/exchange/metadata.md` |
| EXIF byte egress | `Image.get_file` | `.planning/exchange/metadata.md` |

## Integration Rules

- `exchange/metadata` owns EXIF read/write and projects provider tag attributes into local `MetaFacts`.
- Raster owners may preserve bytes and ICC facts, but descriptive EXIF semantics belong only to metadata.
- Dynamic EXIF attributes are admitted at the boundary into bounded local fields; provider attribute names do not leak inward.
