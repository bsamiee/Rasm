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

| [INDEX] | [SYMBOL]    | [RAIL]   | [CAPABILITY]                                                                                                                                                                                                                                                                                                                                                                        |
| :-----: | :---------- | :------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Kml`       | geometry | the document root element; `Feature Feature` (the single root feature), `NetworkLinkControl NetworkLinkControl`, `void AddNamespacePrefix(prefix, ns)`. The `Element` a `KmlFile` wraps                                                                                                                                                                                             |
|  [02]   | `Feature`   | geometry | abstract base of every placemark/container/overlay; `string Name`/`Address`/`PhoneNumber`, `bool? Visibility`/`Open`, `Description Description`, `ExtendedData ExtendedData`, `Region Region`, `TimePrimitive Time`, `AbstractView Viewpoint` (the `Camera`/`LookAt`), `Uri StyleUrl`, `void AddStyle(StyleSelector)`/`ClearStyles()`. The shared metadata every KML object carries |
|  [03]   | `Container` | geometry | abstract feature-with-children (`Document`/`Folder`); `void AddFeature(Feature)`, `Feature FindFeature(string id)`, `bool RemoveFeature(...)` — the tree node                                                                                                                                                                                                                       |
|  [04]   | `Document`  | geometry | the top container that can own shared `Style`/`Schema`; `void AddSchema(Schema)` (+ the inherited `AddFeature`/`AddStyle`) — the standard root of an authored KML                                                                                                                                                                                                                   |
|  [05]   | `Folder`    | geometry | a nested grouping `Container` — the sub-tree a layer/discipline groups under                                                                                                                                                                                                                                                                                                        |
|  [06]   | `Placemark` | geometry | a feature with geometry; `Geometry Geometry`. The styled point/line/polygon a `GeoFeature` projects onto                                                                                                                                                                                                                                                                            |

[PUBLIC_TYPE_SCOPE]: the KML geometry hierarchy
- package: `SharpKml.Core`
- namespace: `SharpKml.Dom`, `SharpKml.Base`
- rail: geometry

| [INDEX] | [SYMBOL]               | [RAIL]   | [CAPABILITY]                                                                                                                                                                                                                                                                    |
| :-----: | :--------------------- | :------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Geometry`             | geometry | abstract base of the KML geometry leaves (distinct from NTS `Geometry` — this is the KML DOM type)                                                                                                                                                                              |
|  [02]   | `Point`                | geometry | `Vector Coordinate`, `AltitudeMode? AltitudeMode`, `bool? Extrude` — a single placemark location                                                                                                                                                                                |
|  [03]   | `LineString`           | geometry | `CoordinateCollection Coordinates`, `AltitudeMode?`, `bool? Tessellate`/`Extrude`, `int? GXDrawOrder` — a path                                                                                                                                                                  |
|  [04]   | `LinearRing`           | geometry | `CoordinateCollection Coordinates` — the closed ring used as a polygon boundary                                                                                                                                                                                                 |
|  [05]   | `Polygon`              | geometry | `OuterBoundary OuterBoundary`, `IReadOnlyCollection<InnerBoundary> InnerBoundary`, `void AddInnerBoundary(InnerBoundary)`, `bool? Tessellate`/`Extrude` — a filled area with holes (each boundary wraps a `LinearRing`)                                                         |
|  [06]   | `MultipleGeometry`     | geometry | `IReadOnlyCollection<Geometry> Geometry`, `void AddGeometry(Geometry)` — the heterogeneous geometry collection (the KML `MultiGeometry`)                                                                                                                                        |
|  [07]   | `CoordinateCollection` | geometry | `ICollection<Vector>`; ctors `()`/`(IEnumerable<Vector>)`, `Add`/`Remove`/`Contains`, `static string Delimiter` — the `<coordinates>` list a `LineString`/`LinearRing` carries                                                                                                  |
|  [08]   | `Vector`               | geometry | the KML coordinate value (`SharpKml.Base`); `double Latitude`/`Longitude`, `double? Altitude`, value equality + `+`/`-`/`*` operators. The bridge target an NTS `Coordinate` maps onto — NOTE: KML order is (lat, lon[, alt]), the inverse of an NTS `Coordinate(X=lon, Y=lat)` |

[PUBLIC_TYPE_SCOPE]: style, overlay, extended-data, schema
- package: `SharpKml.Core`
- namespace: `SharpKml.Dom`
- rail: geometry

| [INDEX] | [SYMBOL]                                | [RAIL]   | [CAPABILITY]                                                                                                                                                                                                                                                                      |
| :-----: | :-------------------------------------- | :------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Style`                                 | geometry | a shared style (`StyleSelector`); `IconStyle Icon`, `LabelStyle Label`, `LineStyle Line`, `PolygonStyle Polygon`, `BalloonStyle Balloon`, `ListStyle List` — the per-geometry-kind symbology                                                                                      |
|  [02]   | `StyleMapCollection`                    | geometry | a `normal`↔`highlight` style-state map (`StyleSelector`, `ICollection<Pair>`); `void Add(Pair)` — the rollover style map                                                                                                                                                          |
|  [03]   | `ExtendedData`                          | geometry | typed attributes on a feature; `IReadOnlyCollection<Data> Data`, `IReadOnlyCollection<SchemaData> SchemaData`, `void AddData(Data)`/`AddSchemaData(SchemaData)` — the carrier a `BimElement` `PropertySet` projects onto                                                          |
|  [04]   | `Data` / `SimpleData`                   | geometry | one untyped (`Data`: `Name`/`DisplayName`/`Value`) / schema-typed (`SimpleData`: `Name`/`Value`) attribute                                                                                                                                                                        |
|  [05]   | `Schema` / `SchemaData` / `SimpleField` | geometry | the typed-attribute schema (`Schema.AddField(SimpleField)` with `Name`/`FieldType`/`DisplayName`) and its per-feature `SchemaData` instance — the strongly-typed `ExtendedData` form                                                                                              |
|  [06]   | `GroundOverlay`                         | geometry | a georeferenced image draped on terrain (`Overlay`); `Icon Icon` (the image link), `LatLonBox Bounds` (N/S/E/W + `Rotation`), `LatLonQuad GXLatLonQuad` (the rotated-quad placement), `double? Altitude`, `Color32? Color`, `int? DrawOrder` — the ortho/site-plan raster overlay |
|  [07]   | `NetworkLink`                           | geometry | a lazily-fetched remote KML (`Feature`); `Link Link`, `bool? FlyToView`/`RefreshVisibility` — the super-overlay tile/region streaming root                                                                                                                                        |

[PUBLIC_TYPE_SCOPE]: the `gx:` tour and track extension
- package: `SharpKml.Core`
- namespace: `SharpKml.Dom.GX`
- rail: geometry

| [INDEX] | [SYMBOL]     | [RAIL]   | [CAPABILITY]                                                                                                                                         |
| :-----: | :----------- | :------- | :--------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Tour`       | geometry | a camera-animation feature; `Playlist Playlist` (the ordered `FlyTo`/`Wait`/`AnimatedUpdate`/`SoundCue` primitives) — the guided 4D fly-through      |
|  [02]   | `Track`      | geometry | a time-stamped moving geometry (`when`+`coord`+`angles` series) — the construction-equipment / survey path with time; `MultipleTrack` groups several |
|  [03]   | `LatLonQuad` | geometry | the four-corner ground-overlay placement (a rotated/sheared image footprint)                                                                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the file and archive engine (`SharpKml.Engine`)
- package: `SharpKml.Core`
- namespace: `SharpKml.Engine`
- rail: geometry

| [INDEX] | [SURFACE]                                            | [CALL_SHAPE]                                                       | [CAPABILITY]                                                                                                                            |
| :-----: | :--------------------------------------------------- | :----------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `KmlFile.Create`                                     | `(Element root, bool duplicates)` → `KmlFile`                      | wraps an authored `Kml`/`Element` tree (the `duplicates` flag controls id-duplication tolerance) — the in-memory KML ready to serialize |
|  [02]   | `KmlFile.Load`                                       | `(Stream input)` / `(TextReader reader)` → `KmlFile`               | parses a `.kml` document into the typed `Dom` tree                                                                                      |
|  [03]   | `KmlFile.Save`                                       | `(Stream stream)`                                                  | serializes the tree to `.kml` XML                                                                                                       |
|  [04]   | `KmlFile.FindObject` / `FindStyle`                   | `(string id)` → `KmlObject` / `StyleSelector`                      | resolves a `#id`-referenced object / shared style                                                                                       |
|  [05]   | `KmzFile.Create`                                     | `(KmlFile kml)` / `(KmlFile kml, Stream targetStream)` → `KmzFile` | builds a `.kmz` archive around the doc (the `doc.kml` entry)                                                                            |
|  [06]   | `KmzFile.Open`                                       | `(Stream stream[, Stream targetStream])` → `KmzFile`               | opens an existing `.kmz` (`IDisposable`)                                                                                                |
|  [07]   | `KmzFile.AddFile`                                    | `(string path, byte[] data)` / `(string path, Stream stream)`      | adds a referenced resource (an icon PNG, a `GroundOverlay` image) into the archive                                                      |
|  [08]   | `KmzFile.ReadFile` / `ReadKml` / `GetDefaultKmlFile` | `(string path)` → `byte[]` / `()` → `string` / `()` → `KmlFile`    | reads an archived resource / the `doc.kml` text / the parsed default KML                                                                |
|  [09]   | `KmzFile.Save`                                       | `(Stream stream)` (+ `UpdateFile`/`RemoveFile`/`Flush`)            | writes the `.kmz` archive                                                                                                               |

[ENTRYPOINT_SCOPE]: bounds, look-at, low-level XML round-trip
- package: `SharpKml.Core`
- namespace: `SharpKml.Engine`, `SharpKml.Base`
- rail: geometry

| [INDEX] | [SURFACE]                                                            | [CALL_SHAPE]                                                                                | [CAPABILITY]                                                                                                                                  |
| :-----: | :------------------------------------------------------------------- | :------------------------------------------------------------------------------------------ | :-------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `FeatureExtensions.CalculateBounds`                                  | `(this Feature feature)` → `BoundingBox`                                                    | the geographic envelope of a feature subtree — the N/S/E/W the document `Region`/`LookAt` derives from                                        |
|  [02]   | `FeatureExtensions.CalculateLookAt`                                  | `(this Feature feature)` → `LookAt`                                                         | the camera that frames the feature — the default viewpoint authored onto the document                                                         |
|  [03]   | `GeometryExtensions.CalculateBounds`                                 | `(this Geometry geometry)` → `BoundingBox`                                                  | the envelope of one geometry                                                                                                                  |
|  [04]   | `Serializer.Serialize`                                               | `(Element root)` then `string Xml` (or `Serialize(root, Stream)`)                           | the raw element→XML emit (the engine under `KmlFile.Save`) — the form when a sub-tree is serialized standalone                                |
|  [05]   | `Parser.Parse` / `ParseString`                                       | `(Stream input, bool namespaces = true)` / `(string xml, bool namespaces)` + `Element Root` | the raw XML→element parse (the engine under `KmlFile.Load`), with the `ElementAdded` event for streaming inspection                           |
|  [06]   | `KmlFactory.Register<T>` / `RegisterExtension<TElement, TExtension>` | `(XmlComponent xml)`                                                                        | registers a custom element type / extension so the parser/serializer round-trips a bespoke KML extension (the GeometryGym-style schema graft) |

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
