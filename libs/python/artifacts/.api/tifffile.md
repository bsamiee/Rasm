# tifffile

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| TIFF image write | `tifffile.imwrite` | `.planning/export/layered.md` |
| Incremental TIFF writer | `tifffile.TiffWriter.write` | `.planning/export/layered.md` |
| TIFF extratags | `imwrite(..., extratags=...)`, `TiffWriter.write(..., extratags=...)` | `.planning/export/layered.md` |
| TIFF metadata and ICC fields | `description`, `metadata`, `iccprofile`, `photometric`, `compression`, `byteorder`, `bigtiff` | `.planning/export/layered.md`, `.planning/exchange/metadata.md` |

## Integration Rules

- `export/layered` owns TIFF authoring; `exchange/metadata` may read/write scalar TIFF metadata only through local metadata facts.
- Layered TIFF export writes merged RGBA image data plus `psdtags` extratags; raw `TiffFile` or `TiffPage` objects do not enter downstream owners.
- Use `metadata=None` for Photoshop-layered TIFF paths so tifffile-specific JSON metadata does not compete with `psdtags` image resources.
