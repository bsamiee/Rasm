# [RASM_MATERIALS_API_UNITSNET]

Full surface and stacking: `libs/csharp/.api/api-unitsnet.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

[UNIT_ALGEBRA]:
- namespace: `UnitsNet`
- quantity root: generated quantity structs
- unit root: `UnitsNet.Units` enum families
- metadata root: `QuantityInfo` and `UnitInfo`
- conversion root: `UnitConverter`, quantity `As`, and quantity `ToUnit`

[PHOTOMETRIC_BASE_UNITS]:
- `Illuminance.BaseUnit` is `IlluminanceUnit.Lux`.
- `Luminance.BaseUnit` is `LuminanceUnit.CandelaPerSquareMeter`.
- `LuminousFlux.BaseUnit` is `LuminousFluxUnit.Lumen`.
- `LuminousIntensity.BaseUnit` is `LuminousIntensityUnit.Candela`.
- `Irradiance.BaseUnit` is `IrradianceUnit.WattPerSquareMeter`.
- The `photometric` author-kernel rescales measured photometric and radiometric inputs to these SI base units.

[THERMAL_BASE_UNITS]:
- `ThermalConductivity.BaseUnit` is `ThermalConductivityUnit.WattPerMeterKelvin`.
- `SpecificEntropy.BaseUnit` is `SpecificEntropyUnit.JoulePerKilogramKelvin`; the quantity carries specific-heat-capacity values at this base.
- `HeatTransferCoefficient.BaseUnit` is `HeatTransferCoefficientUnit.WattPerSquareMeterKelvin`.
- `ThermalConductivity.From`, `SpecificEntropy.From`, and `HeatTransferCoefficient.From` admit a `QuantityValue` plus the unit enum; the `physical-properties` author-kernel rescales measured conductivity, specific-heat, and thermal-transmittance inputs to these SI base units.

[QUANTITY_POLICY]:
- parsing: `QuantityParser` and `UnitParser`
- formatting: `QuantityFormatter` and `UnitFormatter`
- abbreviation policy: `UnitAbbreviationsCache` — every accessor takes an explicit `IFormatProvider?` that defaults to `CurrentCulture` when `null` (the internal invariant `FallbackCulture` is only the per-unit secondary degrade); the `MaterialUnits` boundary passes `CultureInfo.InvariantCulture` explicitly to keep parse/lookup satellite- and `CurrentCulture`-independent
- setup policy: `UnitsNetSetup`
- generic math: `UnitsNet.GenericMath`

[LOCAL_ADMISSION]:
- Compute numeric inputs and receipts carry explicit quantities when units affect meaning.
- Unit conversion is rail policy and cannot be hidden inside numeric helper calls.
- Parsing and formatting are boundary operations, not internal numeric representation; the boundary passes `CultureInfo.InvariantCulture` to every `Parse`/`TryParse`/abbreviation call so admission is deterministic across the host's ambient culture and the loaded satellite set, never `CurrentCulture`-defaulted.
- Quantity metadata informs diagnostics, support output, and receipt projection.

[STACK]:
- in-folder boundary: `UnitsNet` is admitted ONLY through the Materials `MaterialUnits` owner, never crossed into an interior numeric signature (the `BOUNDARY_ADMISSION` law) and never reached DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner (the strata-acyclic AEC-domain owns its own unit boundary). Conversion runs exactly once at admission; interior numerics are raw `double`.
- photometric seam: `photometric#PHOTOMETRIC` `MaterialUnits.Admit(Illuminance.Info, value, unit, …)` gates the illuminance row by `q.Dimensions.Equals(Illuminance.Info.BaseDimensions)`, rescales any sub/multiple to `Illuminance.BaseUnit` (`IlluminanceUnit.Lux`) through `UnitConverter.TryConvert`, and returns `Fin<UnitEvidence>` reading `evidence.CanonicalValue` — the membership gate and the SI-base rescale are one boundary call. The luminous↔radiometric divide (683 lm/W) is author-kernel, not a `UnitsNet` conversion.
- properties seam: `properties#PHYSICAL_PROPERTIES` coerces measured conductivity/specific-heat/thermal-transmittance to `ThermalConductivity.BaseUnit`/`SpecificEntropy.BaseUnit`/`HeatTransferCoefficient.BaseUnit` (the `WattPerMeterKelvin`/`JoulePerKilogramKelvin`/`WattPerSquareMeterKelvin` SI bases) through the same `MaterialUnits.Coerce` rescale; an aggregation over a measured-quantity series (e.g. layered-assembly resistance) folds through `UnitMath.Sum<T>`, never a raw-`double` accumulation that drops the unit.
- wire seam: a quantity that reaches the `interchange#MATERIAL_WIRE` projection serializes as its canonical SI-base scalar plus the unit `Enum` token (resolved through `Quantity.Infos`/`QuantityInfo`), not a localized formatted string — the TS/Python peers decode the SI scalar. The inverse path that admits a measured row from a text abbreviation (e.g. an IFC property string or `Quantity.FromUnitAbbreviation`) resolves the unit through `UnitParser.Default.Parse(abbr, CultureInfo.InvariantCulture)`, never the `CurrentCulture` default, so the same wire byte parses identically on every host regardless of which `*.resources.dll` satellites the runtime loaded.

[RAIL_LAW]:
- Package: `UnitsNet` (MIT-0, multi-TFM, `net10.0` binds `net9.0`)
- Owns: the immutable quantity/unit algebra — quantity structs, `UnitsNet.Units` enums, parsing/formatting, `UnitConverter` rescale, `BaseDimensions` family-membership algebra, `UnitMath`/`GenericMath` typed aggregation, and `UnitSystem` SI policy
- Accept: measured unit-aware execution admitted through the `MaterialUnits` in-folder boundary; the `Dimensions.Equals(Info.BaseDimensions)` family gate plus the `BaseUnit` SI-base rescale
- Reject: raw numeric unit comments; a quantity type crossing into an interior signature; a reach DOWN to the Compute units owner; a raw-`double` reduce where `UnitMath` preserves the unit
