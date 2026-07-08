# [RASM_RHINO_API_RHINOCOMMON_FILEIO]

Catalog scope: the archive and interchange surface — `File3dm` full read/write/partial-read, the complete format-engine roster, PDF authoring, publish/print capture, and geo-reference.

[NAMESPACES]:
- `Rhino.FileIO` archive — `File3dm` (read/write/byte-array/log variants, partial reads: revision history, application data, archive version, notes, page views; tables: manifest/objects/layers/materials/groups/linetypes/dimstyles/hatch patterns/named cplanes/named views/views/plug-in data/render content/embedded files/strings; preview image; `TableTypeFilter`/`ObjectTypeFilter`), `File3dmObject`, `File3dmSettings`, `File3dmStringTable`, `File3dmViewTable`, `File3dmEmbeddedFile`, `File3dmRenderContent`/`RenderEnvironment`/`RenderMaterial`/`RenderTexture`, `File3dmWriteOptions`, `File3dmPlugInData`, `FileReference`.
- `Rhino.FileIO` format engines — the native/direct-read/direct-write pairs and their option types: `File3ds`/`3mf`/`Ai`/`Amf`/`Obj`/`Ply`/`Cd`/`Dgn`/`Dst`/`Dwg`/`Eps`/`Stl`/`Stp`/`Fbx`/`GHS`/`Gts`/`Igs`/`Lwo`/`Nwd`/`Pov`/`Sat`/`Skp`/`Slc`/`SW`/`Udo`/`Vda`/`Vrml`/`X3dv`/`Xaml`/`X_T`/`Raw`/`Txt`/`Csv`/`Gltf`/`Usd`/`Pdf`/`Svg`, `WriteFileResult`, nested option enums (OBJ geometry/name modes, DWG export-surface/color modes, FBX object types, EPS/AI/PDF unit regimes, USD block handling).
- `Rhino.FileIO` PDF — `FilePdf` (`Create`/`AddPage`/draw text/polyline/line/bitmap, optional-content layer groups, `PreWrite` stamping hook, `FilePdfEventArgs`).
- `Rhino.DocObjects` — `EarthAnchorPoint` (earth/model basepoints, KML orientation, anchor plane/compass/transform), per-viewport layer override family, `ObjectMaterialSource`/`ObjectLinetypeSource`.
- `Rhino.Render` — `Sun` (latitude/longitude/north).
- `Rhino.Geometry` — `LengthUnit`, `ScaleValue`/`LengthValue`, `Transform` plane-to-plane/scale/translation families.
