# simpleidml

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| IDML package handle | `simple_idml.idml.IDMLPackage` | `.planning/export/indesign.md` |
| Insert IDML content | `IDMLPackage.insert_idml` | `.planning/export/indesign.md` |
| Add pages from source package | `IDMLPackage.add_pages_from_idml` | `.planning/export/indesign.md` |
| Import and export XML | `IDMLPackage.import_xml`, `IDMLPackage.export_xml` | `.planning/export/indesign.md` |
| Import PDF placeholder content | `IDMLPackage.import_pdf` | `.planning/export/indesign.md` |
| Layer mutation | `IDMLPackage.merge_layers`, `IDMLPackage.remove_layer`, `IDMLPackage.remove_orphan_layers` | `.planning/export/indesign.md` |

## Integration Rules

- `export/indesign` is the only SimpleIDML owner; document emit produces document trees and PDF/Office outputs but does not mutate IDML packages.
- IDML mutations run on a path-backed working copy under an owner-managed temporary scope because the provider operates on package files and XML members.
- SimpleIDML methods normalize into local `IdmlStep`, `IdmlMutation`, and `IdmlFact`; provider package handles never cross the boundary.
