# [RASM_BIM_API_SHARPKML]

`SharpKml.Core` owns the full managed OGC-KML (+ Google `gx:`) object model and the `KmlFile`/`KmzFile` serialize-and-archive engine: a strongly-typed `SharpKml.Dom` element tree over styled features, its own geographic geometry, overlays, extended data, and camera tours, round-tripped through the `Serializer`/`Parser`/`KmlFactory` XML layer.

It is the KML presentation-and-authoring leg of the geospatial seam — `GeoFeature` site-context rows project to styled `Placemark`/`GroundOverlay`/`gx:Tour` output for Google Earth and web-globe delivery, the styled model the geometry-only GDAL `KML` driver cannot reach; its `Vector`/`CoordinateCollection` geometry bridges to the NTS `Geometry` algebra at the seam, never shared as a type.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SharpKml.Core`
- package: `SharpKml.Core` (MIT)
- assembly: `SharpKml.Core`
- namespace: `SharpKml.Dom` — the KML element tree (features, geometry, styles, overlays, extended data)
- namespace: `SharpKml.Dom.GX` — the Google `gx:` extension (`Tour`/`Track`/`MultipleTrack`/`FlyTo`/`Playlist`/`LatLonQuad`)
- namespace: `SharpKml.Dom.Atom` / `SharpKml.Dom.Xal` — the embedded Atom syndication and xAL address vocabularies
- namespace: `SharpKml.Engine` — `KmlFile`, `KmzFile`, the `Feature`/`Geometry` extension folds, `StyleResolver`, `BoundingBox`
- namespace: `SharpKml.Base` — `Serializer`, `Parser`, `KmlFactory`, `Vector`, `Angle`, the attribute/element metadata
- asset: netstandard2.0 single managed AnyCPU assembly bound by the net10.0 consumer; IL-only, no P/Invoke, zero transitive packages — KMZ compression rides the BCL `System.IO.Compression`
- consumer: `libs/csharp/Rasm.Bim` (geospatial site-context KML/KMZ authoring)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the feature and document tree (`SharpKml.Dom`)

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :---------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `Kml`       | class         | document root wrapping the single root `Feature`, `NetworkLinkControl`          |
|  [02]   | `Feature`   | class         | abstract base of every placemark, container, and overlay                        |
|  [03]   | `Container` | class         | abstract feature-with-children base; `AddFeature`/`FindFeature`/`RemoveFeature` |
|  [04]   | `Document`  | class         | top container owning shared style and schema; `AddSchema`/`AddStyle`            |
|  [05]   | `Folder`    | class         | a nested grouping container a layer or discipline groups under                  |
|  [06]   | `Placemark` | class         | a feature carrying `Geometry`; the styled shape a `GeoFeature` projects onto    |

[Kml]: `Feature` `NetworkLinkControl` `AddNamespacePrefix(string, string)`
[Feature]: `Name` `Address` `PhoneNumber` `Visibility` `Open` `Description` `ExtendedData` `Region` `Time` `Viewpoint` `StyleUrl` `AddStyle(StyleSelector)` `ClearStyles()`

[PUBLIC_TYPE_SCOPE]: the KML geometry hierarchy (`SharpKml.Dom`, `SharpKml.Base`)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :--------------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `Geometry`             | class         | abstract base of the KML geometry leaves (the DOM type, not NTS `Geometry`) |
|  [02]   | `Point`                | class         | one location; `Coordinate` `Vector`, `AltitudeMode?`, `Extrude?`            |
|  [03]   | `LineString`           | class         | a path; `Coordinates`, `Tessellate?`/`Extrude?`, `GXDrawOrder?`             |
|  [04]   | `LinearRing`           | class         | the closed ring used as a polygon boundary; `Coordinates`                   |
|  [05]   | `Polygon`              | class         | filled area with holes, each boundary a `LinearRing`                        |
|  [06]   | `MultipleGeometry`     | class         | the KML `MultiGeometry`; `Geometry` collection, `AddGeometry`               |
|  [07]   | `CoordinateCollection` | class         | the `<coordinates>` `ICollection<Vector>` a line or ring carries            |
|  [08]   | `Vector`               | class         | the KML coordinate value in `SharpKml.Base`                                 |

[Polygon]: `OuterBoundary` `InnerBoundary` `AddInnerBoundary(InnerBoundary)` `Tessellate` `Extrude`
[CoordinateCollection]: `Add(Vector)` `Remove(Vector)` `Contains(Vector)` `Delimiter`
[Vector]: `Latitude` `Longitude` `Altitude` `+` `-` `*` — the KML ordinate order is `(lat, lon[, alt])`

[PUBLIC_TYPE_SCOPE]: style, overlay, extended-data, and schema (`SharpKml.Dom`)

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `Style`              | class         | a shared `StyleSelector`; per-geometry-kind symbology               |
|  [02]   | `StyleMapCollection` | class         | a `normal`↔`highlight` state map (`ICollection<Pair>`); `Add(Pair)` |
|  [03]   | `ExtendedData`       | class         | typed and untyped attributes on a feature                           |
|  [04]   | `Data`               | class         | one untyped attribute (`Name`/`DisplayName`/`Value`)                |
|  [05]   | `SimpleData`         | class         | one schema-typed attribute (`Name`/`Value`)                         |
|  [06]   | `Schema`             | class         | the typed-attribute schema; `AddField(SimpleField)`                 |
|  [07]   | `SchemaData`         | class         | a per-feature `Schema` instance, the strongly-typed `ExtendedData`  |
|  [08]   | `SimpleField`        | class         | one `Schema` field (`Name`/`FieldType`/`DisplayName`)               |
|  [09]   | `GroundOverlay`      | class         | a georeferenced image draped on terrain (`Overlay`)                 |
|  [10]   | `NetworkLink`        | class         | lazily-fetched remote KML (`Feature`); `Link`, `FlyToView?`         |

[Style]: `Icon` `Label` `Line` `Polygon` `Balloon` `List`
[ExtendedData]: `Data` `SchemaData` `AddData(Data)` `AddSchemaData(SchemaData)` — the carrier a `BimElement` `PropertySet` projects onto
[GroundOverlay]: `Icon` `Bounds` `GXLatLonQuad` `Altitude` `Color` `DrawOrder`

[PUBLIC_TYPE_SCOPE]: the `gx:` tour and track extension (`SharpKml.Dom.GX`)

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                                           |
| :-----: | :----------- | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `Tour`       | class         | camera-animation feature; `Playlist` of `FlyTo`/`Wait`/`AnimatedUpdate`/`SoundCue`     |
|  [02]   | `Track`      | class         | time-stamped moving geometry (`when`+`coord`+`angles`); `MultipleTrack` groups several |
|  [03]   | `LatLonQuad` | class         | the four-corner ground-overlay placement (a rotated or sheared footprint)              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the file and archive engine — `[01]`-`[04]` on `KmlFile`, `[05]`-`[11]` on `KmzFile` (`SharpKml.Engine`)

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :---------------------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | `Create(Element, bool) -> KmlFile`                    | factory  | wrap an authored tree; the `bool` is id-duplicate tolerance |
|  [02]   | `Load(Stream)` / `Load(TextReader)`                   | factory  | parse a `.kml` document into the typed `Dom` tree           |
|  [03]   | `Save(Stream)`                                        | instance | serialize the tree to `.kml` XML                            |
|  [04]   | `FindObject(string)` / `FindStyle(string)`            | instance | resolve a `#id`-referenced object or shared style           |
|  [05]   | `Create(KmlFile[, Stream]) -> KmzFile`                | factory  | build a `.kmz` archive around `doc.kml`                     |
|  [06]   | `Open(Stream[, Stream]) -> KmzFile`                   | factory  | open an existing `.kmz` (`IDisposable`)                     |
|  [07]   | `AddFile(string, byte[])` / `AddFile(string, Stream)` | instance | add a referenced resource (icon, overlay image)             |
|  [08]   | `ReadFile(string) -> byte[]`                          | instance | read an archived resource                                   |
|  [09]   | `ReadKml() -> string`                                 | instance | read the `doc.kml` text                                     |
|  [10]   | `GetDefaultKmlFile() -> KmlFile`                      | instance | the parsed default KML                                      |
|  [11]   | `Save(Stream)` + `UpdateFile`/`RemoveFile`/`Flush`    | instance | write the `.kmz` archive                                    |

[ENTRYPOINT_SCOPE]: bounds, camera framing, and low-level XML round-trip (`SharpKml.Engine`, `SharpKml.Base`)

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `FeatureExtensions.CalculateBounds -> BoundingBox`                 | static   | a feature-subtree envelope (`Region`/`LookAt` source) |
|  [02]   | `FeatureExtensions.CalculateLookAt -> LookAt`                      | static   | camera framing a feature (default document viewpoint) |
|  [03]   | `GeometryExtensions.CalculateBounds -> BoundingBox`                | static   | the envelope of one geometry                          |
|  [04]   | `Serializer.Serialize(Element) -> string`                          | instance | raw XML emit under `KmlFile.Save`, or to a `Stream`   |
|  [05]   | `Parser.Parse(Stream)` / `ParseString(string)`                     | instance | raw XML→element parse under `KmlFile.Load`            |
|  [06]   | `KmlFactory.Register<T>(XmlComponent)`                             | static   | register a custom element type for the round-trip     |
|  [07]   | `KmlFactory.RegisterExtension<TElement, TExtension>(XmlComponent)` | static   | register a bespoke extension the round-trip handles   |

- `Parser.Parse`: `(Stream, bool namespaces = true)` / `(string, bool)`, exposing `Root` and the `ElementAdded` event for streaming inspection during parse.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- SharpKml is one managed `SharpKml.Core.dll`: the in-memory KML DOM with its XML/ZIP round-trip, carrying no rendering and no projection; KMZ compression rides the BCL `System.IO.Compression`.
- KML geometry is SharpKml's OWN model (`Vector`/`CoordinateCollection`/`Point`/`LineString`/`Polygon`), never the NTS `Geometry` algebra — an NTS geometry bridges to a `Placemark` by walking each `Coordinate` into a `Vector`, never by a cast.
- coordinate order swaps at the bridge: a KML `Vector` is `(Latitude, Longitude[, Altitude])` against an NTS `Coordinate` `(X=longitude, Y=latitude)`, so the bridge writes `new Vector(coord.Y, coord.X, coord.Z)`; the geometry must already be WGS84/EPSG:4326 (KML is always geographic), the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET`/OSR leg reprojecting first.
- author top-down: `new Document` → `AddStyle` shared `Style`s → `AddFeature` `Placemark`/`Folder` → `new Kml { Feature = doc }` → `KmlFile.Create(kml, duplicates: false)` → `Save`; a `Placemark` binds a shared style by `StyleUrl = new Uri("#id", UriKind.Relative)`, a `Polygon` builds from `OuterBoundary` with per-hole `AddInnerBoundary`, and typed attributes ride `ExtendedData` (`Data` untyped, a `Schema`+`SchemaData`+`SimpleField` set typed).

[STACKING]:
- `MaxRev.Gdal.Core`(`.api/api-maxrev-gdal`): the OGR `KML` driver reads coordinates but drops style and extended data, so SharpKml owns the styled authoring leg and GDAL the universal-geometry-ingest leg; a `MaxRev.Gdal.Core` COG/ortho drapes as a `GroundOverlay` (`Icon` + `LatLonBox` `Bounds`, or `GXLatLonQuad` for a rotated footprint) packaged with `doc.kml` through `KmzFile.AddFile`.
- `ProjNET`(`.api/api-projnet`): a projected-frame geometry reprojects to EPSG:4326 through the geodetic transform BEFORE the coordinate bridge runs, KML being geographic by construction.
- within-lib: the `Semantics/geospatial#GEOSPATIAL_SEAM` `GeoFeature` (NTS `Geometry` + `AttributesTable`) projects to a styled `Placemark` — the coordinate bridge walks `Coordinate[]` into a `CoordinateCollection`/`Point`, `AttributesTable` `GetNames`/`GetValues` fold onto `ExtendedData` `Data`/`SchemaData`, and a `GeoClassifier`/`IfcClass`-keyed `Style` supplies symbology, the whole site model one `Document`.
- within-lib: the `Planning/schedule#CRITICAL_PATH` activity network projects to a `gx:Tour` (`Playlist` of `FlyTo`+`AnimatedUpdate` keyed on task `Interval`s) or a per-element `gx:Track`, the time-stamped construction sequence as a self-contained playback.

[LOCAL_ADMISSION]:
- KML authoring enters through the `SharpKml.Dom` tree wrapped by `KmlFile.Create` and archived by `KmzFile`; parse enters through `KmlFile.Load`/`KmzFile.Open`, the raw `Serializer`/`Parser` reserved for a standalone sub-tree round-trip or a streaming `ElementAdded` inspection.

[RAIL_LAW]:
- Package: `SharpKml.Core`
- Owns: the OGC-KML (+ `gx:`) object model — features, geometry, styles, overlays, extended data, tours and tracks — the `KmlFile`/`KmzFile` serialize-and-archive engine, and the `Serializer`/`Parser`/`KmlFactory` XML round-trip with extension registration
- Accept: authoring and parsing styled KML/KMZ, projecting `GeoFeature`/raster/schedule rows to `Placemark`/`GroundOverlay`/`gx:Tour`, packaging KML plus referenced resources into a `.kmz`
- Reject: the NTS planar `Geometry` algebra and `Feature` shape (`NetTopologySuite` owns them, KML geometry is a bridged model), the geodetic reprojection to EPSG:4326 (`ProjNET`/OSR runs first), universal vector/raster ingest (`MaxRev.Gdal.Core` owns the OGR/GDAL drivers), the MVT vector-tile pyramid and 3D-Tiles tileset (`NetTopologySuite.IO.VectorTiles[.Mapbox]` and `subtree` own them), and a hand-rolled KML XML string builder
