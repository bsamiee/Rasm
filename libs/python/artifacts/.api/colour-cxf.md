# [PY_ARTIFACTS_API_COLOUR_CXF]

`colour_cxf` owns the CxF3 (Color Exchange Format v3) document wire for the artifacts color/print plane: an `xsdata`-generated dataclass binding of the `CxF3-core` XSD whose `cxf3.CxF` root models the spot/spectral/device interchange graph, round-tripped against UTF-8 XML bytes through `read_cxf`/`read_cxf_from_file`/`write_cxf`. It is the exchange skin only: it decodes a partner's `.cxf` and encodes a derived palette, carrying no colorimetric math. Parsed spectra feed the `colour-science` `sd_to_XYZ` rail and device CMYK/spot rows feed the `graphic/color/managed` separations egress.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `colour_cxf`
- package: `colour-cxf`
- import: `colour_cxf`
- owner: `artifacts`
- rail: color
- license: `BSD-3-Clause`
- target: pure-Python (`xsdata`-generated dataclasses, abi-agnostic, no native extension, no `python_version` marker)
- depends: `xsdata` (XML bind/parse/serialize runtime), `typing-extensions`
- entry points: none (library only)
- capability: parse a CxF3 document from bytes or path into the typed `cxf3.CxF` graph, build or mutate it as plain dataclasses, and serialize back to UTF-8 XML bytes; the `cxf3` package binds the complete CxF3-core type system across the `ColorValues` (spectral + colorimetric) and `DeviceColorValues` (CMYK / spot / recipe) unions, measurement specification, and physical attributes

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document read/write rail (`colour_cxf` top-level)
- rail: color

Three functions span the whole I/O surface over `cxf3.CxF` <-> `bytes`: `read_cxf`/`read_cxf_from_file` parse through an internal `XmlContext()` + `XmlParser`, `write_cxf` renders via `XmlSerializer` and encodes UTF-8. One parse, one render — no streaming, partial-tree, or validating variant.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                        | [CAPABILITY]                                              |
| :-----: | :------------------- | :-------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `read_cxf`           | `read_cxf(doc: bytes)` -> `cxf3.CxF`                | parse CxF3 XML bytes into the typed `CxF` graph           |
|  [02]   | `read_cxf_from_file` | `read_cxf_from_file(str \| PathLike)` -> `cxf3.CxF` | open a `.cxf` path `"rb"` and parse it (wraps `read_cxf`) |
|  [03]   | `write_cxf`          | `write_cxf(cxf: cxf3.CxF)` -> `bytes`               | serialize a `CxF` graph to UTF-8 CxF3 XML bytes           |

[ENTRYPOINT_SCOPE]: re-exported `xsdata` bind/serialize runtime (`colour_cxf` top-level)
- rail: color

`colour_cxf` re-exports the three `xsdata` runtime objects its functions use, so a consumer needing custom parse/serialize config (pretty-print via `SerializerConfig`, a shared `XmlContext` cache across many documents, strict/lenient fail modes) reaches them through this import rather than importing `xsdata` separately. Provenance is `xsdata.formats.dataclass.` except `PathLike` (`os`).

| [INDEX] | [SYMBOL]        | [PROVENANCE]                | [CAPABILITY]                                                           |
| :-----: | :-------------- | :-------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `XmlContext`    | `context.XmlContext`        | metadata cache for the `cxf3` dataclass graph; share one across reads  |
|  [02]   | `XmlParser`     | `parsers.XmlParser`         | `from_bytes(doc, cxf3.CxF)` / `from_string` / `parse` deserializer     |
|  [03]   | `XmlSerializer` | `serializers.XmlSerializer` | `render(cxf)` -> `str`; accepts a `SerializerConfig` for pretty/indent |
|  [04]   | `PathLike`      | `os.PathLike`               | the accepted `source_path` shape for `read_cxf_from_file`              |

## [03]-[DOCUMENT_GRAPH]

[MODEL_SCOPE]: document spine — `cxf3.CxF` root and resource collections
- rail: color

`cxf3.CxF` is the parse/render root both `read_cxf` and `write_cxf` bind; every node namespaces to `http://colorexchangeformat.com/CxF3-core`. Each type exposes both its CapWords class (`ColorCielab`) and a snake_case module alias (`color_cielab`) — the CapWords class is the public symbol.

| [INDEX] | [SYMBOL]                       | [CAPABILITY]                                                                      |
| :-----: | :----------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `CxF`                          | document root; `resources` holds the payload, `custom_resources` the escape hatch |
|  [02]   | `FileInformation`              | provenance header — creator/date/description + `Tag` pairs                        |
|  [03]   | `Resources`                    | the three resource collections under the root                                     |
|  [04]   | `ObjectCollection`             | the list of measured/named color `Object` entries                                 |
|  [05]   | `ColorSpecificationCollection` | the measurement-context registry rows reference by `Id`                           |
|  [06]   | `ProfileCollection`            | embedded/referenced ICC profile entries                                           |
|  [07]   | `CustomResources`              | open `xsd:any` wildcard for vendor extensions                                     |

[MODEL_SCOPE]: color object and measurement specification
- rail: color

`Object` is the CxF spot/sample unit — a GUID-keyed entry carrying its measured `ColorValues`, `DeviceColorValues`, color-difference values, and physical attributes, each leaf color row bound to a `ColorSpecification` through the `color_specification` IDREF.

| [INDEX] | [SYMBOL]             | [CAPABILITY]                                                                             |
| :-----: | :------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Object`             | one measured/named color entry keyed by `ObjectType`/`Name`/`Id`, optional `GUID`        |
|  [02]   | `ColorSpecification` | the measurement context every color row references by `@id`                              |
|  [03]   | `TristimulusSpec`    | illuminant + observer + ASTM E308 table that define tristimulus computation              |
|  [04]   | `MeasurementSpec`    | full ISO 13655 measurement geometry — type, geometry, sampling grid, M-series conditions |
|  [05]   | `Illuminant`         | named CIE illuminant (or `Custom` with explicit XYZ)                                     |
|  [06]   | `Observer`           | 2°/10°/custom standard observer                                                          |
|  [07]   | `Method`             | ASTM E308 weighting table (`E308_Table5`/`Table6`/`1nm`)                                 |
|  [08]   | `PhysicalAttributes` | substrate/finish/target + dimensional + gloss/opacity sample metadata                    |
|  [09]   | `Profile`            | an ICC profile reference/embed with a transform `direction`                              |

[MODEL_SCOPE]: colorimetric + spectral color values — `ColorValues` union members
- rail: color

`Object.color_values` is a `ColorValues` whose `choice: list[...]` is the closed `xsd:choice` over every spectral and colorimetric encoding — the half that crosses into `colour-science` for SPD/XYZ resolution. Each spectrum carries samples as `value: list[float]` with a `@start_wl` attribute; `CustomSpectrum`/`PrivateSpectrum`/`PrivateColorValues`/`CustomColorSpace` are the vendor escape hatches outside the closed set.

| [INDEX] | [SYMBOL]                      | [CAPABILITY]                                                                              |
| :-----: | :---------------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `ColorValues`                 | the closed colorimetric+spectral `xsd:choice` over rows [02]–[11] plus the escape hatches |
|  [02]   | `ReflectanceSpectrum`         | measured reflectance SPD — the primary spot-color spectral payload                        |
|  [03]   | `TransmittanceSpectrum`       | measured (total)transmittance SPD                                                         |
|  [04]   | `EmissiveSpectrum`            | measured emissive SPD (display/light source)                                              |
|  [05]   | `ColorCielab`                 | CIE L*a*b* tristimulus — `(l*, a*, b*)`                                                   |
|  [06]   | `ColorCielch`                 | CIE LCH cylindrical — `(l, c, h)`                                                         |
|  [07]   | `ColorCiexyz`                 | CIE 1931 XYZ tristimulus — `(x*, y*, z*)`                                                 |
|  [08]   | `ColorCiexyY`                 | CIE xyY chromaticity + luminance — `(x*, y*, Y*)`                                         |
|  [09]   | `ColorCieluv`                 | CIE L*u*v* tristimulus — `(l, u, v)`                                                      |
|  [10]   | `ColorSrgb` / `ColorAdobeRgb` | integer sRGB / Adobe RGB — `(max_range, r*, g*, b*)`                                      |
|  [11]   | `ColorDensity`                | ISO 5-3 optical density — `(density*, status*, filter*, base_offset)`                     |

[MODEL_SCOPE]: device + spot color values — `DeviceColorValues` union members
- rail: color

`Object.device_color_values` is a `DeviceColorValues` whose `choice: list[...]` is the device/separations half feeding `graphic/color/managed`. `ColorCmykplusN` is the headline spot node — process CMYK with a list of named `SpotColorType` channels at percentage coverage; `ColorRecipe` carries the ink-mixing formula (substrate, process, weighted `Colorant` list).

| [INDEX] | [SYMBOL]                                                | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------------------ | :------------------------------------------------------------------ |
|  [01]   | `DeviceColorValues`                                     | the closed device-color `xsd:choice` over rows [02]–[08]            |
|  [02]   | `ColorCmyk`                                             | process CMYK coverage with optional ICC profile reference           |
|  [03]   | `ColorCmykplusN`                                        | CMYK + N named spot channels — the N-color separations payload      |
|  [04]   | `SpotColor` / `SpotColorType`                           | one named spot channel at a coverage percentage                     |
|  [05]   | `ColorPantoneHexachrome`                                | the 6-color CMYKOG Hexachrome device encoding                       |
|  [06]   | `ColorRgb` / `ColorHsl` / `ColorHtml` / `ColorNotation` | RGB / HSL / `#hex` / free-notation additive/web encodings           |
|  [07]   | `ColorRecipe`                                           | ink-mixing recipe — substrate + process + weighted colorant formula |
|  [08]   | `Colorant`                                              | one ink/base in a recipe at a mixing value, base-ink flag           |

## [04]-[VOCABULARIES]

[ENUM_SCOPE]: closed measurement + sample vocabularies (`cxf3`, exact CxF3-core cardinality)
- rail: color

These closed `StrEnum` vocabularies the measurement/physical nodes select from are the registry-key set a consuming page maps onto the `colour-science` registries (`SDS_ILLUMINANTS`, `MSDS_CMFS`), read as the exact key set, never an approximation.

- [01]-[ESPECTRUMTYPE]: `Spectrum_Reflectance` `Spectrum_Transmittance` `Spectrum_TotalTransmittance` `Spectrum_Emissive` `Colorimetric_Reflectance` `Colorimetric_Transmittance` `Colorimetric_Emissive` `Spectrum_Custom`
- [02]-[EILLUMINANTTYPE]: `A` `B` `C` `D50` `D55` `D60` `D65` `D75` `E` `F2` `F3` `F7` `F9` `F10` `F11` `F12` `9300` `TL84` `TL83` `UL30` `UL35` `UL50` `Custom`
- [03]-[EOBSERVERTYPE]: `2_Degree` `10_Degree` `Custom_CMF`
- [04]-[EASTMTABLETYPE]: `E308_Table5` `E308_Table6` `E308_1nm` `unknown`
- [05]-[EEMISSIVEMODETYPE]: `EmissiveMode_Diffuser` `EmissiveMode_Reflected` `EmissiveMode_Other`
- [06]-[EDENSITYSTATUSTYPE]: `A` `E` `I` `M` `T` `SpectralX` `Spectral` `HiFi` `Hex` `Txp` `Ex` `DIN` `DIN-NB` `PD` `APD`
- [07]-[EDENSITYFILTERTYPE]: `Visual` `Cyan` `Magenta` `Yellow` `Black` `Red` `Green` `Blue` `A` `B`
- [08]-[ESPHERETYPE]: `Specular_Included` `Specular_Excluded` `Diffuse`
- [09]-[EFINISHTYPE]: `Coated` `Uncoated` `Matte` `Polished` `Glossy` `Satin` `Other`
- [10]-[ESUBSTRATETYPE]: `Film` `Leather` `Metal` `Paint` `Paper` `Plastic` `Textile` `Tile` `Vinyl` `Wood` `Other`
- [11]-[ETARGETTYPE]: `IT8.7/1` `IT8.7/2` `IT8.7/3` `IT8.7/4` `ECI2002` `Other`
- [12]-[EFILTERTYPE]: `Filter_None` `Filter_UVExcluded` `Filter_UVD65` `Filter_Partial` `Filter_Custom`

`ColorDensity.status` selects `EdensityStatusType` and `.filter` selects `EdensityFilterType`.

[MODEL_SCOPE]: measurement geometry choice — sphere / single-angle / multi-angle
- rail: color

`MeasurementSpec.geometry_choice` is a `GeometryChoice` whose `choice` discriminates the geometry — emissive mode, sphere, a fixed single illumination/measurement angle pair, or a multi-angle BRDF set — the context deciding whether a spot's spectrum is flat or angle-dependent (effect pigment).

| [INDEX] | [SYMBOL]          | [CAPABILITY]                                           |
| :-----: | :---------------- | :----------------------------------------------------- |
|  [01]   | `GeometryChoice`  | the measurement-geometry discriminant                  |
|  [02]   | `SingleAngleType` | one fixed illumination/measurement angle pair (45°:0°) |
|  [03]   | `MultiAngleType`  | multi-angle BRDF geometry for effect/metallic pigments |
|  [04]   | `WavelengthRange` | the global SPD sampling grid all spectra share         |
|  [05]   | `SpectralPoint`   | a single (wavelength, value) irregular-grid sample     |

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- generation: every `cxf3` type is an `xsdata`-generated `@dataclass` with `Meta.namespace = "http://colorexchangeformat.com/CxF3-core"`; field-to-XML binding lives in each field's `metadata`, and the graph is bound, parsed, and serialized by the `xsdata` runtime, never hand-walked. `MeasurementSpec.measurement_type` wraps the spectrum token as `MeasurementType(EspectrumType | None)`; a bare `EspectrumType` in the slot is the mis-shape.
- root: `cxf3.CxF` is the only type passed to `read_cxf`/`write_cxf`; `CxF.resources.{object_collection, color_specification_collection, profile_collection}` is the payload spine.
- union discrimination: `ColorValues.choice` and `DeviceColorValues.choice` are `xsd:choice` lists, not tagged unions — a consumer discriminates members by `isinstance` against the closed set, the Python type being the discriminant with no runtime tag field.
- IDREF resolution: every leaf color row carries `color_specification: str` (an XML IDREF) keying a `ColorSpecification.id`; a spectrum/Lab/CMYK value is interpretable only against its referenced spec's `MeasurementSpec` (illuminant/observer/geometry/wavelength grid), resolved once and threaded, never treated as context-free.
- spectral grid: a `ReflectanceSpectrum.value: list[float]` is a uniform SPD starting at `@start_wl` and stepping by the referenced `MeasurementSpec.wavelength_range.increment`, so a spectral row's `ColorSpecification` carries a non-`None` `MeasurementSpec`; colorimetric-only rows (`ColorCielab`/`ColorCiexyz`/`ColorDensity`) admit its omission. `SpectralPoint(value, @wl)` carries irregular pairs; the grid is reconstructed, never assumed.
- custom escape: `CustomResources` (`xsd:any` + `any_attributes`), `CustomSpectrum`, `PrivateSpectrum`, `PrivateColorValues`, `CustomColorSpace`, and `ColorCustom` carry vendor extensions — a reader tolerates them, an authoring path emits only closed CxF3-core nodes unless a partner requires a private block.

[STACKING]:
- `colour-science`(`.api/colour-science.md`): the wavelength axis is the seam — a parsed `ReflectanceSpectrum`/`TransmittanceSpectrum`/`EmissiveSpectrum` (`value` + the referenced `WavelengthRange.start_wl`/`increment`) constructs a `colour.SpectralDistribution` over a `colour.SpectralShape(start, end, interval)`, which `colour.sd_to_XYZ(sd, cmfs=MSDS_CMFS[observer], illuminant=SDS_ILLUMINANTS[illuminant])` resolves to XYZ; the CxF `Observer`/`Illuminant`/`Method` enums key the `colour` registries and ASTM E308 method. `colour_cxf` owns the exchange shape, `colour-science` the colorimetry — neither re-implements the other.
- `coloraide`(`.api/coloraide.md`): a CxF `ColorCielab`/`ColorCiexyz` decodes into `Color(('lab', [l, a, b]))` (D50; `'lab-d65'` when the CxF illuminant is D65) / `Color(('xyz-d65', [x, y, z]))` for gamut-mapped CSS egress, via the `colour-science` XYZ bridge.
- `graphic/color/derive#DERIVE` (palette source): inbound, a partner's `.cxf` spot library decodes into the `Colorimetry` palette source, each `Object`'s spectral/Lab values resolving through `colour-science` into the `ColorReceipt` provenance; outbound, a `derive`-resolved spot palette serializes back through `write_cxf`. `derive` stays the palette owner, `colour_cxf` its CxF wire.
- `graphic/color/managed#MANAGED` (ICC/separations egress): a `ColorCmykplusN` (CMYK + N `SpotColorType` channels) carries the N-color separations intent the `ColorManaged` egress applies and an ink `ColorRecipe`/`Colorant` set informs the separations plane; `colour_cxf` declares the spot/separation, `managed` runs the pixel transform.
- universal rail (`libs/python/.api`): a parsed `Object`/`ColorSpecification` projects onto a `msgspec.Struct` wire model at `data/tabular`; bounded admissions (`@start_wl`, coverage, recipe `value`) refine through a `beartype.vale.Is` contract; a malformed `read_cxf` crosses the `runtime/reliability/faults` `async_boundary` as a typed fault, not a bare `xsdata` `ParserError`; a `structlog`/OTel span records each parse; a batch decode runs the `anyio` `CapacityLimiter` over `to_thread` slots.

[LOCAL_ADMISSION]:
- a CxF document enters via `read_cxf`/`read_cxf_from_file` and leaves via `write_cxf`; the typed `cxf3` graph is the only intermediate — never `lxml`/`ElementTree` parsing nor string templating.
- a `cxf3.CxF` is built and mutated as plain dataclasses (construct `Object`, append to `ObjectCollection.object_value`, set `color_values.choice`), never assembled as an XML string.
- bulk reads share one re-exported `XmlContext()` passed to `XmlParser(context=...)`; pretty/indent output sets an `xsdata` `SerializerConfig` on `XmlSerializer` rather than post-processing the rendered string.

[RAIL_LAW]:
- Package: `colour-cxf`
- Owns: CxF3 document read/parse/build/serialize — the typed `cxf3.CxF` graph and its `read_cxf`/`read_cxf_from_file`/`write_cxf` round-trip over the complete CxF3-core type system; the CxF spot/spectral exchange skin for the print/separations plane.
- Accept: `read_cxf`/`read_cxf_from_file` to decode a `.cxf`, `write_cxf` to encode a built or mutated graph, the `cxf3` dataclasses for programmatic construction, the re-exported `XmlContext`/`XmlParser`/`XmlSerializer` for shared-cache or pretty-print config, and the closed `Espectrum`/`Eilluminant`/`Eobserver`/`Eastm` vocabularies as the registry-key set bridged onto `colour-science`.
- Reject: parsing CxF XML by hand (`lxml`/`ElementTree`) or templating it as strings; integrating an SPD, computing a CIE transform, solving an ink recipe, or mapping a gamut inside this rail (those are `colour-science`/`coloraide`/`managed`); using `colour_cxf` for any non-CxF color file; emitting private/custom blocks unless a named partner requires them; treating a spectrum or Lab row as context-free rather than resolving its `color_specification` IDREF; a `python_version` marker (pure-Python, abi-agnostic).
