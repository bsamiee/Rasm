# iptcinfo3

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| IPTC image metadata object | `iptcinfo3.IPTCInfo` | `.planning/exchange/metadata.md` |
| IPTC field read/write | `IPTCInfo[...]` | `.planning/exchange/metadata.md` |
| IPTC byte/file persistence | `IPTCInfo.save`, `IPTCInfo.save_as` | `.planning/exchange/metadata.md` |

## Integration Rules

- `exchange/metadata` owns IPTC semantics and maps provider fields into local descriptive, rights, capture, place, and history facts.
- IPTC read/write is a raster metadata carrier path; raster processing never authors descriptive semantics.
- Missing provider fields become local absence values, not `None`-driven interior dispatch.
