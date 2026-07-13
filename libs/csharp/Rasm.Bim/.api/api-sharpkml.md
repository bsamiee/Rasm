# [RASM_BIM_API_SHARPKML]

`SharpKml.Core` is the full managed OGC-KML / (+ Google `gx:` extension) object model and
KMZ archive engine: a strongly-typed `Dom` element tree (`Kml`/`Document`/`Folder`/`Placemark`
over a `Point`/`LineString`/`LinearRing`/`Polygon`/`MultipleGeometry` geometry hierarchy with
`Style`/`StyleMap`/`ExtendedData`/`Schema`/`GroundOverlay`/`NetworkLink`/`gx:Tour`/`gx:Track`),
the `KmlFile`/`KmzFile` serialization+archive engine, and the `Serializer`/`Parser`/`KmlFactory`
low-level XML round-trip. It is the KML presentation+authoring leg of the
`Semantics/geospatial#GEOSPATIAL_SEAM` — the `GeoFeature` site-context rows project to styled
`Placemark`/`GroundOverlay` elements for Google Earth and web-globe delivery, the presentation
model the `MaxRev.Gdal.Core` geometry-only OGR `KML` driver (`.api/api-maxrev-gdal`) cannot
reach. SharpKml carries its OWN geographic geometry (`Vector` lat/lon/alt, `CoordinateCollection`)
distinct from the NTS `Geometry` algebra; the seam is a projection bridge, not a shared type.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SharpKml.Core`
- package: `SharpKml.Core`
- license: MIT
- assembly: `SharpKml.Core`
- namespace: `SharpKml.Dom` (the KML element tree — features, geometry, styles, overlays, extended data)
- namespace: `SharpKml.Dom.GX` (the Google `gx:` extension — `Tour`/`Track`/`MultipleTrack`/`FlyTo`/`Playlist`/`LatLonQuad`)
- namespace: `SharpKml.Dom.Atom` / `SharpKml.Dom.Xal` (the embedded Atom syndication and xAL address vocabularies)
- namespace: `SharpKml.Engine` (`KmlFile`, `KmzFile`, the `Feature`/`Geometry` extension folds, `StyleResolver`, `BoundingBox`)
- namespace: `SharpKml.Base` (`Serializer`, `Parser`, `KmlFactory`, `Vector`, `Angle`, the attribute/element metadata)
- asset: netstandard2.0 single managed AnyCPU assembly (multi-targets net462/netstandard1.2 — the net10.0 consumer binds `lib/netstandard2.0`); ships the `SharpKml.Core.xml` doc set
- asset: IL-only, no P/Invoke; the `.NETStandard2.0` dependency group is EMPTY (zero transitive packages — KMZ uses the BCL `System.IO.Compression`)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the feature and document tree
- package: `SharpKml.Core`
- namespace: `SharpKml.Dom`
- rail: geometry

| [INDEX] | [SYMBOL]    | [CAPABILITY]                                                                                                       |
| :-----: | :---------- | :----------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Kml`       | document root element; members in `[01]-[KML]`                                                                     |
|  [02]   | `Feature`   | abstract base of every placemark/container/overlay; members in `[02]-[FEATURE]`                                    |
|  [03]   | `Container` | feature-with-children (`Document`/`Folder`); `AddFeature(Feature)`, `FindFeature(string id)`, `RemoveFeature(...)` |
|  [04]   | `Document`  | top container owning shared `Style`/`Schema`; `AddSchema(Schema)` + inherited `AddFeature`/`AddStyle`              |
|  [05]   | `Folder`    | a nested grouping `Container`; the sub-tree a layer/discipline groups under                                        |
|  [06]   | `Placemark` | a feature with geometry; `Geometry Geometry`; the styled point/line/polygon a `GeoFeature` projects onto           |

- [01]-[KML]: `Kml` — `Feature Feature` (the single root feature), `NetworkLinkControl NetworkLinkControl`, `void AddNamespacePrefix(prefix, ns)`; the `Element` a `KmlFile` wraps.
- [02]-[FEATURE]: `Feature` — `string Name`/`Address`/`PhoneNumber`, `bool? Visibility`/`Open`, `Description Description`, `ExtendedData ExtendedData`, `Region Region`, `TimePrimitive Time`, `AbstractView Viewpoint` (the `Camera`/`LookAt`), `Uri StyleUrl`, `void AddStyle(StyleSelector)`/`ClearStyles()`; the shared metadata every KML object carries.

[PUBLIC_TYPE_SCOPE]: the KML geometry hierarchy
- package: `SharpKml.Core`
- namespace: `SharpKml.Dom`, `SharpKml.Base`
- rail: geometry

| [INDEX] | [SYMBOL]               | [CAPABILITY]                                                                                                  |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Geometry`             | abstract base of the KML geometry leaves (distinct from NTS `Geometry` — this is the KML DOM type)            |
|  [02]   | `Point`                | `Vector Coordinate`, `AltitudeMode? AltitudeMode`, `bool? Extrude`; a single placemark location               |
|  [03]   | `LineString`           | `CoordinateCollection Coordinates`, `AltitudeMode?`, `bool? Tessellate`/`Extrude`, `int? GXDrawOrder`; a path |
|  [04]   | `LinearRing`           | `CoordinateCollection Coordinates`; the closed ring used as a polygon boundary                                |
|  [05]   | `Polygon`              | filled area with holes (each boundary wraps a `LinearRing`); members in `[05]-[POLYGON]`                      |
|  [06]   | `MultipleGeometry`     | `IReadOnlyCollection<Geometry> Geometry`, `void AddGeometry(Geometry)`; the KML `MultiGeometry`               |
|  [07]   | `CoordinateCollection` | `ICollection<Vector>` `<coordinates>` list; members in `[07]-[COORDS]`                                        |
|  [08]   | `Vector`               | the KML coordinate value (`SharpKml.Base`); members and coordinate-order law in `[08]-[VECTOR]`               |

- [05]-[POLYGON]: `Polygon` — `OuterBoundary OuterBoundary`, `IReadOnlyCollection<InnerBoundary> InnerBoundary`, `void AddInnerBoundary(InnerBoundary)`, `bool? Tessellate`/`Extrude`; each boundary wraps a `LinearRing`.
- [07]-[COORDS]: `CoordinateCollection` — `ICollection<Vector>`; ctors `()`/`(IEnumerable<Vector>)`, `Add`/`Remove`/`Contains`, `static string Delimiter`; the `<coordinates>` list a `LineString`/`LinearRing` carries.
- [08]-[VECTOR]: `Vector` — `double Latitude`/`Longitude`, `double? Altitude`, value equality + `+`/`-`/`*` operators; the bridge target an NTS `Coordinate` maps onto — KML order is (lat, lon[, alt]), the inverse of an NTS `Coordinate(X=lon, Y=lat)`.

[PUBLIC_TYPE_SCOPE]: style, overlay, extended-data, schema
- package: `SharpKml.Core`
- namespace: `SharpKml.Dom`
- rail: geometry

| [INDEX] | [SYMBOL]             | [CAPABILITY]                                                                                    |
| :-----: | :------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `Style`              | a shared style (`StyleSelector`); per-geometry-kind symbology, members in `[01]-[STYLE]`        |
|  [02]   | `StyleMapCollection` | a `normal`↔`highlight` style-state map (`StyleSelector`, `ICollection<Pair>`); `void Add(Pair)` |
|  [03]   | `ExtendedData`       | typed attributes on a feature; members in `[03]-[EXTDATA]`                                      |
|  [04]   | `Data`               | one untyped attribute: `Name`/`DisplayName`/`Value`                                             |
|  [05]   | `SimpleData`         | one schema-typed attribute: `Name`/`Value`                                                      |
|  [06]   | `Schema`             | the typed-attribute schema; `AddField(SimpleField)` with `Name`/`FieldType`/`DisplayName`       |
|  [07]   | `SchemaData`         | per-feature instance of a `Schema` — the strongly-typed `ExtendedData` form                     |
|  [08]   | `SimpleField`        | one `Schema` field declaration (`Name`/`FieldType`/`DisplayName`)                               |
|  [09]   | `GroundOverlay`      | georeferenced image draped on terrain (`Overlay`); members in `[09]-[GROVERLAY]`                |
|  [10]   | `NetworkLink`        | lazily-fetched remote KML (`Feature`); `Link Link`, `bool? FlyToView`/`RefreshVisibility`       |

- [01]-[STYLE]: `Style` — `IconStyle Icon`, `LabelStyle Label`, `LineStyle Line`, `PolygonStyle Polygon`, `BalloonStyle Balloon`, `ListStyle List`; the per-geometry-kind symbology.
- [03]-[EXTDATA]: `ExtendedData` — `IReadOnlyCollection<Data> Data`, `IReadOnlyCollection<SchemaData> SchemaData`, `void AddData(Data)`/`AddSchemaData(SchemaData)`; the carrier a `BimElement` `PropertySet` projects onto.
- [09]-[GROVERLAY]: `GroundOverlay` — `Icon Icon` (the image link), `LatLonBox Bounds` (N/S/E/W + `Rotation`), `LatLonQuad GXLatLonQuad` (the rotated-quad placement), `double? Altitude`, `Color32? Color`, `int? DrawOrder`; the ortho/site-plan raster overlay.

[PUBLIC_TYPE_SCOPE]: the `gx:` tour and track extension
- package: `SharpKml.Core`
- namespace: `SharpKml.Dom.GX`
- rail: geometry

| [INDEX] | [SYMBOL]     | [CAPABILITY]                                                                                                     |
| :-----: | :----------- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Tour`       | camera-animation feature; `Playlist Playlist` of ordered `FlyTo`/`Wait`/`AnimatedUpdate`/`SoundCue` primitives   |
|  [02]   | `Track`      | time-stamped moving geometry (`when`+`coord`+`angles`); construction/survey path; `MultipleTrack` groups several |
|  [03]   | `LatLonQuad` | the four-corner ground-overlay placement (a rotated/sheared image footprint)                                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the file and archive engine (`SharpKml.Engine`)
- package: `SharpKml.Core`
- namespace: `SharpKml.Engine`
- owners: `[01]`-`[04]` on `KmlFile`; `[05]`-`[11]` on `KmzFile`
- rail: geometry

| [INDEX] | [SURFACE]                  | [CAPABILITY]                                                                                             |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Create`                   | `(Element root, bool duplicates)` → `KmlFile`; wraps an authored tree, `duplicates` = id-dup tolerance   |
|  [02]   | `Load`                     | `(Stream)` / `(TextReader)` → `KmlFile`; parses a `.kml` document into the typed `Dom` tree              |
|  [03]   | `Save`                     | `(Stream stream)`; serializes the tree to `.kml` XML                                                     |
|  [04]   | `FindObject` / `FindStyle` | `(string id)` → `KmlObject` / `StyleSelector`; resolves a `#id`-referenced object / shared style         |
|  [05]   | `Create`                   | `(KmlFile)` / `(KmlFile, Stream targetStream)` → `KmzFile`; builds a `.kmz` archive around `doc.kml`     |
|  [06]   | `Open`                     | `(Stream[, Stream targetStream])` → `KmzFile`; opens an existing `.kmz` (`IDisposable`)                  |
|  [07]   | `AddFile`                  | `(string path, byte[] data)` / `(string path, Stream)`; adds a referenced resource (icon, overlay image) |
|  [08]   | `ReadFile`                 | `(string path)` → `byte[]`; reads an archived resource                                                   |
|  [09]   | `ReadKml`                  | `()` → `string`; reads the `doc.kml` text                                                                |
|  [10]   | `GetDefaultKmlFile`        | `()` → `KmlFile`; the parsed default KML                                                                 |
|  [11]   | `Save`                     | `(Stream stream)` + `UpdateFile`/`RemoveFile`/`Flush`; writes the `.kmz` archive                         |

[ENTRYPOINT_SCOPE]: bounds, look-at, low-level XML round-trip
- package: `SharpKml.Core`
- namespace: `SharpKml.Engine`, `SharpKml.Base`
- rail: geometry

| [INDEX] | [SURFACE]                            | [CAPABILITY]                                                                                     |
| :-----: | :----------------------------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `FeatureExtensions.CalculateBounds`  | `(this Feature)` → `BoundingBox`; feature-subtree envelope (`Region`/`LookAt` source)            |
|  [02]   | `FeatureExtensions.CalculateLookAt`  | `(this Feature)` → `LookAt`; camera framing the feature (default document viewpoint)             |
|  [03]   | `GeometryExtensions.CalculateBounds` | `(this Geometry)` → `BoundingBox`; the envelope of one geometry                                  |
|  [04]   | `Serializer.Serialize`               | `(Element root)` → `string Xml`, or `Serialize(root, Stream)`; raw XML emit under `KmlFile.Save` |
|  [05]   | `Parser.Parse` / `ParseString`       | raw XML→element parse under `KmlFile.Load`; details in `[05]-[PARSER]`                           |
|  [06]   | `KmlFactory.Register`                | `<T>(XmlComponent xml)`; registers a custom element type                                         |
|  [07]   | `KmlFactory.RegisterExtension`       | `<TElement, TExtension>(XmlComponent xml)`; registers a bespoke extension the round-trip handles |

- [05]-[PARSER]: `Parser.Parse` / `ParseString` — `(Stream input, bool namespaces = true)` / `(string xml, bool namespaces)` + `Element Root`; the raw XML→element parse (the engine under `KmlFile.Load`), with the `ElementAdded` event for streaming inspection.

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- a single managed `SharpKml.Core.dll` (netstandard2.0 binds forward under net10.0); no P/Invoke, zero transitive packages — KMZ compression rides the BCL `System.IO.Compression`. The whole library is the in-memory DOM plus the XML/ZIP round-trip; it carries no rendering and no projection.
- the KML geometry model is SharpKml's OWN (`Vector`/`CoordinateCollection`/`Point`/`LineString`/`Polygon`), NOT the NTS `Geometry` algebra. There is no shared geometry type with `NetTopologySuite` — a `GeoFeature` NTS geometry is BRIDGED to a SharpKml `Placemark` by walking the `Coordinate` ordinates into `Vector`s, never by a cast.
- coordinate-order law: a KML `Vector` is `(Latitude, Longitude[, Altitude])`; an NTS `Coordinate` is `(X = longitude, Y = latitude)`. The bridge MUST swap — `new Vector(coord.Y, coord.X, coord.Z)` — and the geometry MUST already be WGS84/EPSG:4326 (KML is always geographic), so the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET`/OSR leg reprojects to 4326 before the projection.

[AUTHORING_MODEL]:
- author top-down: `var doc = new Document { Name = … }`, add shared `Style`s (`doc.AddStyle(style)`), add `Placemark`s/`Folder`s (`doc.AddFeature(placemark)`), wrap in `new Kml { Feature = doc }`, then `KmlFile.Create(kml, duplicates: false)` and `Save(stream)`. A `Placemark` references a shared style by `StyleUrl = new Uri("#styleId", UriKind.Relative)`.
- a `Polygon` is built from `OuterBoundary { LinearRing = new LinearRing { Coordinates = ring } }` plus `AddInnerBoundary(...)` per hole; a `Point`/`LineString` carries its `Vector`/`CoordinateCollection` directly.
- typed attributes ride `ExtendedData` — `feature.ExtendedData.AddData(new Data { Name = key, Value = value })` for untyped, or a `Schema`+`SchemaData`+`SimpleField` set for typed columns the renderer can format.

[STACK_INTEGRATION]:
- geospatial seam: the `Semantics/geospatial#GEOSPATIAL_SEAM` `GeoFeature` (NTS `Geometry` + `AttributesTable`) projects to a styled `Placemark` — the geometry bridge walks the NTS `Coordinate[]` into a SharpKml `CoordinateCollection`/`Point` (with the lat/lon swap), the `AttributesTable` `GetNames`/`GetValues` fold onto `ExtendedData` `Data`/`SchemaData`, and a `GeoClassifier`/`IfcClass`-keyed `Style` gives the symbology. The whole site model is one `Document`, the standard "BIM site context as a Google-Earth overlay" emit the GDAL geometry-only `KML` driver cannot produce (it loses style and extended data).
- raster overlay seam: the `MaxRev.Gdal.Core` COG/ortho the `GeoRaster` leg produces drapes as a `GroundOverlay` (`Icon` = the image link, `LatLonBox` = the raster `Envelope` N/S/E/W, or `GXLatLonQuad` for a rotated footprint) packaged into a `KmzFile.AddFile` archive alongside the `doc.kml` — the image plus its georeference in one `.kmz`.
- 4D seam: the `Planning/schedule#CRITICAL_PATH` activity network projects to a `gx:Tour` (a `Playlist` of `FlyTo`+`AnimatedUpdate` keyed on the task `Interval`s) or a per-element `gx:Track` (the time-stamped construction sequence) — the 4D playback as a self-contained KML tour, distinct from the `csharp:Rasm.AppUi/Charts` live render.
- distinct legs: KML is the PRESENTATION/authoring model (style, balloon, tour, overlay) for Earth/web-globe delivery; the `MaxRev.Gdal.Core` OGR `KML` driver reads the same format but only its geometry (no style/extended-data round-trip), so SharpKml owns the authoring leg and GDAL owns the universal-geometry-ingest leg — the two coexist. The MVT vector-tile pyramid (`NetTopologySuite.IO.VectorTiles[.Mapbox]`) and the 3D-Tiles glTF tileset (`subtree`) are the orthogonal web-delivery stacks.

[LOCAL_ADMISSION]:
- KML authoring enters through the `SharpKml.Dom` tree (`Document`/`Placemark`/`Style`/`ExtendedData`) wrapped by `KmlFile.Create`, archived by `KmzFile`; the NTS→KML geometry bridge swaps coordinate order and assumes EPSG:4326.
- KML/KMZ parse enters through `KmlFile.Load` / `KmzFile.Open`; the raw `Serializer`/`Parser` are used only for a standalone sub-tree round-trip or a streaming `ElementAdded` inspection.
- a hand-rolled KML XML string builder, or routing KML through the GDAL OGR `KML` driver when style/extended-data/tour authoring is needed, is the rejected form — SharpKml owns the full presentation model.

[RAIL_LAW]:
- Package: `SharpKml.Core`
- Owns: the OGC-KML / (+ `gx:`) object model (features, geometry, styles, overlays, extended data, tours/tracks), the `KmlFile`/`KmzFile` serialize+archive engine, and the `Serializer`/`Parser`/`KmlFactory` XML round-trip and extension registration
- Accept: authoring/parsing styled KML and KMZ, projecting `GeoFeature`/raster/schedule rows to `Placemark`/`GroundOverlay`/`gx:Tour`, packaging KML + referenced resources into a `.kmz`
- Reject: the NTS planar `Geometry` algebra and `Feature` shape (`NetTopologySuite` owns them — KML geometry is a separate bridged model), the geodetic reprojection to EPSG:4326 (the `Semantics/georeference` `ProjNET`/OSR leg runs first), universal vector/raster ingest (`MaxRev.Gdal.Core` owns the OGR/GDAL drivers), the MVT vector-tile pyramid and the 3D-Tiles tileset (the `NetTopologySuite.IO.VectorTiles[.Mapbox]` and `subtree` stacks own them)
