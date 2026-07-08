# [PY_ARTIFACTS_API_COLOUR_CXF]

`colour_cxf` is the categorical-best CxF3 (Color Exchange Format, version 3) read/write wire for the artifacts color/print plane: an `xsdata`-generated dataclass binding of the published `CxF3-core` XSD whose `cxf3.CxF` document root models the full spot/spectral/device interchange graph (measured reflectance/transmittance/emissive spectra, CIE Lab/LCH/XYZ/xyY/Luv, CMYK and CMYK+N spot, Pantone Hexachrome, ink recipes, ISO 13655 measurement geometry, ISO 5-3 density, ICC profile references), and three module-level functions — `read_cxf`, `read_cxf_from_file`, `write_cxf` — that round-trip that graph against UTF-8 XML bytes through the re-exported `xsdata` `XmlParser`/`XmlSerializer`. It is the exchange skin only: it decodes a print partner's `.cxf` spot library into the typed graph and encodes a derived palette back out; it carries no colorimetric math. The wavelength axis it parses (`ReflectanceSpectrum.value: list[float]` + `WavelengthRange.start_wl`/`increment`) feeds the `colour-science` `SpectralDistribution` -> `sd_to_XYZ` rail that owns spectral truth, and the device CMYK/spot rows feed the `graphic/color/managed#MANAGED` ICC/separations egress. The owner never re-implements an SPD integration, a CIE transform, an ink-recipe solve, or a CxF XML parser by hand — every node is a typed `cxf3` dataclass and every read/write is one `read_cxf`/`write_cxf` call.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `colour_cxf`
- package: `colour-cxf`
- import: `colour_cxf`
- owner: `artifacts`
- rail: color
- version: `0.1.1`
- license: `BSD-3-Clause`
- target: pure-Python (xsdata-generated dataclasses, no native extension, abi-agnostic)
- floor: runs on cp315 — verified imported under `3.15.0b2`; upstream `Requires-Python: >=3.10,<3.14` is an advisory metadata cap, NOT an ABI gate (no compiled artifact), so no `python_version` marker is needed and `uv lock` resolves the universal wheel on this interpreter
- depends: `xsdata` (XML bind/parse/serialize runtime), `typing-extensions`
- entry points: none (library only; the project also ships an `[docs]` extra pulling `sphinx`/`xsdata[cli]` — not admitted)
- capability: parse a CxF3 document from bytes or a file path into a typed `cxf3.CxF` graph, mutate or build that graph programmatically as dataclasses, serialize it back to UTF-8 XML bytes; the `cxf3` model package binds the complete `CxF3-core` type system — file metadata, object/color-specification/profile resource collections, the colorimetric+spectral `ColorValues` union, the device `DeviceColorValues` union (CMYK / CMYK+N spot / RGB / HSL / Pantone Hexachrome / ink recipe), measurement geometry (illuminant/observer/method, sphere/single-angle/multi-angle BRDF, wavelength range, density status/filter), and physical attributes

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document read/write rail (`colour_cxf` top-level)
- rail: color

The entire I/O surface is three functions over `cxf3.CxF` <-> `bytes`. `read_cxf`/`read_cxf_from_file` parse via an internal `XmlContext()` + `XmlParser`; `write_cxf` renders via an `XmlSerializer` and encodes UTF-8. There is no streaming, partial-tree, or validating variant — one parse, one render.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `read_cxf(doc)` -> `cxf3.CxF` | `read_cxf(doc: bytes)` | parse CxF3 XML bytes into the typed `CxF` document graph |
| [02] | `read_cxf_from_file(source_path)` -> `cxf3.CxF` | `read_cxf_from_file(str \| PathLike)` | open a `.cxf` path `"rb"` and parse it (thin wrapper over `read_cxf`) |
| [03] | `write_cxf(cxf)` -> `bytes` | `write_cxf(cxf: cxf3.CxF)` | serialize a `CxF` graph to UTF-8-encoded CxF3 XML bytes |

[ENTRYPOINT_SCOPE]: re-exported `xsdata` bind/serialize runtime (`colour_cxf` top-level)
- rail: color

`colour_cxf` re-exports the three `xsdata` runtime objects its functions use, so a consumer that needs custom parse/serialize config (pretty-print, custom `SerializerConfig`, a shared `XmlContext` cache across many documents, lenient/strict fail modes) reaches them through this package rather than importing `xsdata` separately — the model graph and its codec live behind one import.

| [INDEX] | [SYMBOL] | [PROVENANCE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `XmlContext` | `xsdata.formats.dataclass.context.XmlContext` | metadata cache for the `cxf3` dataclass graph; share one across reads |
| [02] | `XmlParser` | `xsdata.formats.dataclass.parsers.XmlParser` | `from_bytes(doc, cxf3.CxF)` / `from_string` / `parse` deserializer |
| [03] | `XmlSerializer` | `xsdata.formats.dataclass.serializers.XmlSerializer` | `render(cxf)` -> `str`; accepts a `SerializerConfig` for pretty/indent |
| [04] | `PathLike` | `os.PathLike` | the accepted `source_path` shape for `read_cxf_from_file` |

## [03]-[DOCUMENT_GRAPH]

[MODEL_SCOPE]: document spine — `cxf3.CxF` root and resource collections
- rail: color

`cxf3.CxF` is the parse/render root and the single type both `read_cxf` and `write_cxf` bind. Every node namespaces to `http://colorexchangeformat.com/CxF3-core`. The model package exposes each type under both its CapWords class (`ColorCielab`) and a snake_case module alias (`color_cielab`) — the CapWords class is the public symbol; the alias is the generating module re-export.

| [INDEX] | [SYMBOL] | [SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `CxF` | `(file_information, resources, custom_resources)` | document root; `resources` carries the color payload, `custom_resources` a wildcard escape hatch |
| [02] | `FileInformation` | `(creator, creation_date, description, comment, tag: list[Tag])` | provenance header — creator/date/description plus arbitrary `Tag` name/value pairs |
| [03] | `Resources` | `(object_collection, color_specification_collection, profile_collection)` | the three resource collections under the document root |
| [04] | `ObjectCollection` | `(object_value: list[Object])` | the list of measured/named color `Object` entries |
| [05] | `ColorSpecificationCollection` | `(color_specification: list[ColorSpecification])` | the measurement-context registry every color row references by `Id` |
| [06] | `ProfileCollection` | `(profile: list[Profile])` | embedded/referenced ICC profile entries |
| [07] | `CustomResources` | `(other_element: list[object] [xsd:any], any_attributes: dict[str, str])` | open `xsd:any` wildcard for vendor extensions outside CxF3-core |

[MODEL_SCOPE]: color object and measurement specification
- rail: color

`Object` is the unit of a CxF spot/sample: a named, GUID-keyed entry carrying its measured `ColorValues` (spectral + colorimetric), its `DeviceColorValues` (CMYK/spot/recipe), color-difference values, and physical attributes. Every leaf color row carries a `color_specification` attribute (an IDREF `str`) that resolves into a `ColorSpecification` in the collection — the measurement geometry, illuminant, observer, and method under which that color was measured. This IDREF link is the load-bearing seam: a `ReflectanceSpectrum` is only interpretable against the `MeasurementSpec.wavelength_range` of its referenced `ColorSpecification`.

| [INDEX] | [SYMBOL] | [SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `Object` | `(creation_date*, comment, color_values, color_difference_values, device_color_values, tag_collection, physical_attributes, @object_type*, @name*, @id*, @guid)` | one measured/named color entry keyed by `ObjectType`/`Name`/`Id`, optional `GUID` |
| [02] | `ColorSpecification` | `(tristimulus_spec, measurement_spec*, physical_attributes, @id*)` | the measurement context every color row references by `@id` |
| [03] | `TristimulusSpec` | `(illuminant_or_custom_illuminant: Illuminant \| CustomIlluminant, observer*, method*)` | illuminant + observer + ASTM E308 table that define tristimulus computation |
| [04] | `MeasurementSpec` | `(measurement_type*, geometry_choice*, wavelength_range, luminance_units_type, calibration_standard, aperture, backing, bandpass_corrected, device)` | full ISO 13655 measurement geometry — type, geometry, sampling grid, M-series conditions |
| [05] | `Illuminant` | `(value: EilluminantType*, @name, @x, @y, @z)` | named CIE illuminant (or `Custom` with explicit XYZ) |
| [06] | `Observer` | `(value: EobserverType*, @name, @angle, @age)` | 2°/10°/custom standard observer |
| [07] | `Method` | `(value: EastmTableType*)` | ASTM E308 weighting table (`E308_Table5`/`Table6`/`1nm`) |
| [08] | `PhysicalAttributes` | `(target_type, finish_type, substrate_type, quantity, height, width, length, thickness, gloss: list[Gloss], opacity: list[Opacity], custom_attribute_string/value: list[...], image: list[Image])` | substrate/finish/target + dimensional + gloss/opacity sample metadata |
| [09] | `Profile` | `(profile_choice*, parameters: list[ProfileTypeParameters], created, @id*, @direction*)` | an ICC profile reference/embed with a transform `direction` |

[MODEL_SCOPE]: colorimetric + spectral color values — `ColorValues` union members
- rail: color

`Object.color_values` is a `ColorValues` whose single `choice: list[...]` is the closed `xsd:choice` over every spectral and colorimetric encoding. This is the spectral/CIE half of the CxF interchange — the data that crosses into `colour-science` for SPD/XYZ resolution. Each spectrum carries its samples as `value: list[float]` with a `start_wl` attribute; the sampling step lives on the referenced `MeasurementSpec.wavelength_range.increment`.

| [INDEX] | [SYMBOL] | [SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `ColorValues` | `(choice: list[ReflectanceSpectrum \| TransmittanceSpectrum \| EmissiveSpectrum \| CustomSpectrum \| ColorSrgb \| ColorAdobeRgb \| ColorCielab \| ColorCielch \| ColorCiexyz \| ColorEmissiveCiexyz \| ColorCiexyY \| ColorEmissiveCiexyY \| ColorCieluv \| ColorDensity \| PrivateSpectrum \| PrivateColorValues \| CustomColorSpace])` | the closed colorimetric+spectral choice on a measured `Object` |
| [02] | `ReflectanceSpectrum` | `(value: list[float], @name, @measure_date: XmlDateTime, @start_wl: int, @color_specification*)` | measured reflectance SPD — the primary spot-color spectral payload |
| [03] | `TransmittanceSpectrum` | `(value: list[float], @name, @measure_date, @start_wl, @color_specification*)` | measured (total)transmittance SPD |
| [04] | `EmissiveSpectrum` | `(value: list[float], @name, @measure_date, @start_wl, @color_specification*)` | measured emissive SPD (display/light source) |
| [05] | `ColorCielab` | `(l*, a*, b*, @name, @color_specification*)` | CIE L*a*b* tristimulus |
| [06] | `ColorCielch` | `(l, c, h, @name, @color_specification)` | CIE LCH cylindrical |
| [07] | `ColorCiexyz` | `(x*, y*, z*, @name, @color_specification*)` | CIE 1931 XYZ tristimulus |
| [08] | `ColorCiexyY` | `(x*, y*, Y*, @name, @color_specification*)` | CIE xyY chromaticity + luminance |
| [09] | `ColorCieluv` | `(l, u, v, @name, @color_specification)` | CIE L*u*v* tristimulus |
| [10] | `ColorSrgb` / `ColorAdobeRgb` | `(max_range, r*, g*, b*, @name, @color_specification*)` | integer sRGB / Adobe RGB with `MaxRange` scale |
| [11] | `ColorDensity` | `(density*, status*: EdensityStatusType, filter*: EdensityFilterType, base_offset, @name, @color_specification*)` | ISO 5-3 optical density with a status condition and a filter condition |
| [12] | `CustomSpectrum` / `PrivateSpectrum` / `PrivateColorValues` / `CustomColorSpace` | vendor/private spectral + color escape hatches | non-standard spectral/color payloads outside the closed encodings |

[MODEL_SCOPE]: device + spot color values — `DeviceColorValues` union members
- rail: color

`Object.device_color_values` is a `DeviceColorValues` whose `choice: list[...]` is the device/separations half — the data that crosses into `graphic/color/managed#MANAGED` for CMYK/separations egress. `ColorCmykplusN` is the headline spot-separation node: process CMYK plus an arbitrary list of named `SpotColorType` channels at percentage coverage — the N-channel separations model. `ColorRecipe` carries the ink-mixing formula (substrate + process + weighted `Colorant` list).

| [INDEX] | [SYMBOL] | [SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `DeviceColorValues` | `(choice: list[ColorHtml \| ColorNotation \| ColorRgb \| ColorHsl \| ColorCmyk \| ColorCmykplusN \| ColorCustom \| ColorPantoneHexachrome \| ColorRecipe \| PrivateColorValues])` | the closed device-color choice on a measured `Object` |
| [02] | `ColorCmyk` | `(cyan*, magenta*, yellow*, black*, @profile_specification, @name, @color_specification*)` | process CMYK coverage with optional ICC profile reference |
| [03] | `ColorCmykplusN` | `(cyan*, magenta*, yellow*, black*, spot_color: list[SpotColorType], @profile_specification, @name, @color_specification*)` | CMYK + N named spot channels — the N-color separations payload |
| [04] | `SpotColor` / `SpotColorType` | `(name*, percentage*)` | one named spot channel at a coverage percentage |
| [05] | `ColorPantoneHexachrome` | `(cyan*, magenta*, yellow*, black*, orange*, green*, @profile_specification, @name, @color_specification*)` | the 6-color CMYKOG Hexachrome device encoding |
| [06] | `ColorRgb` / `ColorHsl` / `ColorHtml` / `ColorNotation` | RGB / HSL / `#hex` / free-notation device rows | additive/web device-color encodings |
| [07] | `ColorRecipe` | `(creation_date*, tag: list[Tag], substrate, process, colorant: list[Colorant], @units*, @name*, @comments, @color_specification)` | ink-mixing recipe — substrate + process + weighted colorant formula |
| [08] | `Colorant` | `(part_number, density, value*, @name*, @id, @is_base: bool)` | one ink/base in a recipe at a mixing value, base-ink flag |

## [04]-[VOCABULARIES]

[ENUM_SCOPE]: closed measurement + sample enumerations (`cxf3`, exact published cardinalities)
- rail: color

These are the closed `StrEnum`-style vocabularies the measurement/physical nodes select from. They are authored to the exact CxF3-core schema cardinality — a consuming page that maps CxF illuminant/observer/spectrum-type onto `colour-science` registry keys (`SDS_ILLUMINANTS`, `MSDS_CMFS`) reads these as the closed key set, never an approximation.

| [INDEX] | [SYMBOL] | [CARDINALITY] | [MEMBERS] |
| --- | --- | --- | --- |
| [01] | `EspectrumType` | 8 | `Spectrum_Reflectance`, `Spectrum_Transmittance`, `Spectrum_TotalTransmittance`, `Spectrum_Emissive`, `Colorimetric_Reflectance`, `Colorimetric_Transmittance`, `Colorimetric_Emissive`, `Spectrum_Custom` |
| [02] | `EilluminantType` | 23 | `A`, `B`, `C`, `D50`, `D55`, `D60`, `D65`, `D75`, `E`, `F2`, `F3`, `F7`, `F9`, `F10`, `F11`, `F12`, `9300`, `TL84`, `TL83`, `UL30`, `UL35`, `UL50`, `Custom` |
| [03] | `EobserverType` | 3 | `2_Degree`, `10_Degree`, `Custom_CMF` |
| [04] | `EastmTableType` | 4 | `E308_Table5`, `E308_Table6`, `E308_1nm`, `unknown` |
| [05] | `EemissiveModeType` | 3 | `EmissiveMode_Diffuser`, `EmissiveMode_Reflected`, `EmissiveMode_Other` |
| [06] | `EdensityStatusType` | 15 | `A`, `E`, `I`, `M`, `T`, `SpectralX`, `Spectral`, `HiFi`, `Hex`, `Txp`, `Ex`, `DIN`, `DIN-NB`, `PD`, `APD` |
| [07] | `EdensityFilterType` | 10 | `Visual`, `Cyan`, `Magenta`, `Yellow`, `Black`, `Red`, `Green`, `Blue`, `A`, `B` |
| [08] | `EsphereType` | 3 | `Specular_Included`, `Specular_Excluded`, `Diffuse` |
| [09] | `EfinishType` | 7 | `Coated`, `Uncoated`, `Matte`, `Polished`, `Glossy`, `Satin`, `Other` |
| [10] | `EsubstrateType` | 11 | `Film`, `Leather`, `Metal`, `Paint`, `Paper`, `Plastic`, `Textile`, `Tile`, `Vinyl`, `Wood`, `Other` |
| [11] | `EtargetType` | 6 | `IT8.7/1`, `IT8.7/2`, `IT8.7/3`, `IT8.7/4`, `ECI2002`, `Other` |
| [12] | `EfilterType` | 5 | `Filter_None`, `Filter_UVExcluded`, `Filter_UVD65`, `Filter_Partial`, `Filter_Custom` |

[MODEL_SCOPE]: measurement geometry choice — sphere / single-angle / multi-angle
- rail: color

`MeasurementSpec.geometry_choice` is a `GeometryChoice` whose `choice` discriminates the measurement geometry: an emissive mode, a sphere (d:8°/d:0° etc.), a single fixed illumination/measurement angle pair, or a multi-angle BRDF set. This is the geometry context that determines whether a spot's spectrum is interpretable as a flat or angle-dependent (effect-pigment) measurement.

| [INDEX] | [SYMBOL] | [SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `GeometryChoice` | `(choice: EemissiveModeType \| EsphereType \| SingleAngleType \| str \| MultiAngleType)` | the measurement-geometry discriminant |
| [02] | `SingleAngleType` | `(single_angle_configuration*, illumination_angle*, measurement_angle*)` | one fixed illumination/measurement angle pair (e.g. 45°:0°) |
| [03] | `MultiAngleType` | `(brdfangle: list[Brdfangle])` | the multi-angle BRDF geometry for effect/metallic pigments |
| [04] | `WavelengthRange` | `(@start_wl: int*, @increment: int*)` | the global SPD sampling grid (e.g. start 380nm, 10nm step) all spectra share |
| [05] | `SpectralPoint` | `(value: float*, @wl: float*)` | a single (wavelength, value) sample for irregular-grid spectral points |

## [05]-[IMPLEMENTATION_LAW]

[CXF_TOPOLOGY]:
- generation: every `cxf3` type is an `xsdata`-generated `@dataclass` with `Meta.namespace = "http://colorexchangeformat.com/CxF3-core"`; field-to-XML binding lives in each field's `metadata` (`type=Element|Attribute|Elements|Wildcard`, `name`, `required`). The graph is bound, parsed, and serialized entirely by the `xsdata` runtime — never hand-walked.
- root: `cxf3.CxF` is the only type passed to `read_cxf`/`write_cxf`; `CxF.resources.{object_collection, color_specification_collection, profile_collection}` is the payload spine.
- union discrimination: `ColorValues.choice` and `DeviceColorValues.choice` are `xsd:choice` lists, not tagged unions — a consuming page discriminates members by `isinstance` against the closed member set (the two `choice` element-union signatures above ARE the exhaustive case lists), folding each onto its target representation. There is no runtime tag field; the Python type IS the discriminant.
- IDREF resolution: every leaf color row carries `color_specification: str` (an XML IDREF) keying a `ColorSpecification.id` in the collection. A spectrum/Lab/CMYK value is only interpretable against its referenced spec's `MeasurementSpec` (illuminant/observer/geometry/wavelength grid) — the consuming page resolves this reference once and threads the geometry, never treats a spectrum as context-free.
- spectral grid: a `ReflectanceSpectrum.value: list[float]` is a uniform-grid SPD whose start is `@start_wl` (or the row's own attribute) and whose step is the referenced `MeasurementSpec.wavelength_range.increment`; `SpectralPoint(value, @wl)` carries irregular (wavelength, value) pairs. The grid is reconstructed from these, never assumed.
- custom escape: `CustomResources` (`other_element: list[object]` over `xsd:any` + `any_attributes`), `CustomSpectrum`, `PrivateSpectrum`, `PrivateColorValues`, `CustomColorSpace`, and `ColorCustom` carry vendor extensions; a reader tolerates them, an authoring path emits only the closed CxF3-core nodes unless a partner explicitly requires a private block.

[LOCAL_ADMISSION]:
- a CxF document enters via `read_cxf(bytes)` / `read_cxf_from_file(path)` and leaves via `write_cxf(cxf)` -> `bytes`; raw XML is never parsed with `lxml`/`ElementTree` nor templated as strings — the typed `cxf3` graph is the only intermediate.
- a `cxf3.CxF` is built/mutated as plain dataclasses (construct `Object`, append to `ObjectCollection.object_value`, set `color_values.choice`); never assembled as an XML string.
- for bulk reads sharing one metadata cache, a single re-exported `XmlContext()` is constructed once and passed to `XmlParser(context=...)`; pretty/indent output uses an `xsdata` `SerializerConfig` on `XmlSerializer` rather than post-processing the rendered string.
- the byte payload `write_cxf` returns is the artifact body; it keys through the runtime content identity and an `exchange`/`package` egress, never re-encoded.

[INTEGRATION_STACK]:
- `colour_cxf` -> `colour-science` (spectral truth, the stacking partner): the decode seam is the wavelength axis. A parsed `ReflectanceSpectrum`/`TransmittanceSpectrum`/`EmissiveSpectrum` (`value: list[float]` + the referenced `WavelengthRange.start_wl`/`increment`) constructs a `colour.SpectralDistribution(dict(zip(wavelengths, values)))` over a `colour.SpectralShape(start, end, interval)` (the CxF `WavelengthRange.increment` feeding the `colour` `interval` parameter), which `colour.sd_to_XYZ(sd, cmfs=MSDS_CMFS[observer], illuminant=SDS_ILLUMINANTS[illuminant])` resolves to XYZ — the CxF `Observer`/`Illuminant`/`Method` enums map onto the `colour` registry keys and the ASTM E308 method. `colour_cxf` owns the *exchange shape*; `colour-science` owns the *colorimetry*. Neither crosses: `colour_cxf` never integrates an SPD, `colour-science` never parses CxF XML.
- `colour_cxf` -> `coloraide` (CSS/gamut presentation): a CxF `ColorCielab`/`ColorCiexyz` decodes into a `coloraide` `Color(('lab', [l, a, b]))` (the D50-referenced CIELAB space; `'lab-d65'` when the CxF illuminant is D65) / `Color(('xyz-d65', [x, y, z]))` for gamut-mapped CSS egress of an exchanged spot, via the `colour-science` XYZ bridge — `colour_cxf` is never used for CSS parsing.
- `colour_cxf` <-> `graphic/color/derive#DERIVE` (palette source, owning page): inbound, a print partner's `.cxf` spot library decodes into the `Colorimetry` palette source — each `Object`'s spectral/Lab values resolve (via `colour-science`) into the `ColorReceipt` color provenance, so a derived palette is seeded from a measured spot set; outbound, a `derive`-resolved spot palette serializes back to a `CxF` document (`Object` per swatch carrying its `ReflectanceSpectrum` + `ColorCielab`) via `write_cxf` for round-trip exchange. `derive` stays the one upstream palette owner; `colour_cxf` is its CxF wire.
- `colour_cxf` <-> `graphic/color/managed#MANAGED` (ICC/separations egress, owning page): the device half feeds separations — a `ColorCmykplusN` (CMYK + N `SpotColorType` channels) carries the N-color separations intent the `ColorManaged` ICC/LUT egress applies; an exchanged ink `ColorRecipe`/`Colorant` set informs the separations plane. `colour_cxf` carries the *spot/separation declaration*; `managed` (pyvips/`colour-science` ICC) carries the *pixel transform*. Device-link/named-color/N-channel proof transforms route through Pillow `ImageCms`, NOT through `colour_cxf`.
- universal-rail stacking (`libs/python/.api`): a parsed `Object`/`ColorSpecification` projects onto a `msgspec.Struct` spot-library wire model (canonical `ColorReceipt.wired()`-style projection sinking `coords`/`shape` outward) at the `data/tabular` seam; the bounded enum admissions (`@start_wl`, percentage, recipe `value`) refine through a `beartype.vale.Is`-guarded `Annotated` contract at the authoring factory so an out-of-range coverage or wavelength is refused at construction; a malformed-XML `read_cxf` raise crosses the `runtime/reliability/faults` `async_boundary` seam onto the `RuntimeRail`, never an unhandled `xsdata` `ParserError`; a `structlog` event + OpenTelemetry span records the parse/serialize at the exchange boundary; a batch decode of many `.cxf` files runs through the `anyio` structured-concurrency lane with a `CapacityLimiter`, each parse a `to_thread` slot.

[RAIL_LAW]:
- Package: `colour-cxf`
- Owns: CxF3 (Color Exchange Format v3) document read/parse/build/serialize — the typed `cxf3.CxF` graph and its `read_cxf`/`read_cxf_from_file`/`write_cxf` round-trip; the complete CxF3-core type system (spectral reflectance/transmittance/emissive, CIE Lab/LCH/XYZ/xyY/Luv, density, CMYK / CMYK+N spot / Pantone Hexachrome / RGB / HSL device, ink recipe + colorant, measurement geometry/illuminant/observer/method, physical attributes, ICC profile references). The CxF spot/spectral exchange skin for the print/separations plane.
- Accept: `read_cxf`/`read_cxf_from_file` to decode a `.cxf` into the typed graph; `write_cxf` to encode a built/mutated graph; the `cxf3` dataclasses for programmatic construction; the re-exported `XmlContext`/`XmlParser`/`XmlSerializer` for shared-cache or pretty-print config; the closed `Espectrum`/`Eilluminant`/`Eobserver`/`Eastm` vocabularies as the registry-key set bridged onto `colour-science`.
- Reject: parsing CxF XML by hand (`lxml`/`ElementTree`) or templating it as strings where the typed `cxf3` graph is the contract; integrating an SPD, computing a CIE transform, solving an ink recipe, or mapping a gamut inside this rail (those are `colour-science`/`coloraide`/`managed`); using `colour_cxf` for any non-CxF color file; emitting private/custom CxF blocks unless a named partner requires them; treating a spectrum or Lab row as context-free rather than resolving its `color_specification` IDREF to the measurement spec; adding a `python_version` marker (pure-Python, runs on cp315 — the upstream `<3.14` metadata cap is advisory, not an ABI gate).
