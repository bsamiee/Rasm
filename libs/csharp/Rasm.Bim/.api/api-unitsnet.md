# [RASM_BIM_API_UNITSNET]

Full surface and stacking: `libs/csharp/.api/api-unitsnet.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

[QUANTITY_TOPOLOGY]:
- namespaces: `UnitsNet`, `UnitsNet.Units`
- typed quantities: `Length`, `Area`, `Volume`, `Mass`, `Duration` — each a `readonly struct`
- value carrier: `QuantityValue` (implicit from `double`/`decimal`/integral; explicit to `double`/`decimal`)
- unit vocabulary: `LengthUnit`, `AreaUnit`, `VolumeUnit`, `MassUnit`, `DurationUnit` enums
- SI anchors: `Length.BaseUnit` `Meter`, `Area.BaseUnit` `SquareMeter`, `Volume.BaseUnit` `CubicMeter`, `Mass.BaseUnit` `Kilogram`, `Duration.BaseUnit` `Second`
- SI system: `UnitSystem.SI`, `BaseUnits`, `BaseDimensions`
- erased seam: `IQuantity` (`Value`, `Unit`, `Dimensions`, `QuantityInfo`, `As`, `ToUnit`)
- dynamic hub: `Quantity.From`/`Parse`/`TryFrom`/`TryParse`/`Infos`/`ByName`
- aggregation: `UnitMath.Sum`/`Min`/`Max`/`Average`/`Abs`

[LOCAL_ADMISSION]:
- the seam `Rasm.Element/Properties/quantity#MEASURE_VALUE` `MeasureValue` stores the typed quantity, never a bare `double`; the IFC measure unit selects the seam `Dimension` (the retired stringly-keyed `KindOf` axis replaced [H2]), and the carrier holds the SI scalar plus its `Dimension`/`QuantityType`.
- Coercion to the canonical persisted form is `ToUnit(UnitSystem.SI)` (or the typed `FromMeters`/`FromSquareMeters`/`FromCubicMeters`/`FromKilograms`/`FromSeconds` factory), so the persisted scalar is always SI-base.
- Foreign-unit ingest from an IFC `IfcSIUnit`/`IfcConversionBasedUnit` abbreviation routes through `UnitParser.TryParse<TUnit>` then `Quantity.From(value, unit)`; an unparseable unit lowers onto `BimFault.CapabilityMiss`, never an exception.
- Quantity equality and ordering use the struct's `IEqualityOperators`/`IComparisonOperators`; aggregation over an element set uses `UnitMath.Sum`/`Min`/`Max`/`Average` with the explicit SI base unit, never a manual `double` fold.
- The dynamic `Quantity`/`IQuantity` seam is the boundary-only erased path for unit-keyed IFC ingest; the persisted seam `MeasureValue` holds the SI scalar the strongly-typed `Length`/`Area`/`Volume`/`Mass`/`Duration` struct coerced.

[STACKING]:
- with `GeometryGymIFC_Core` (`.api/api-geometrygym-ifc`): the IFC ingest rail surfaces a foreign measure as a raw `(double measure, string unit)` pair off `IfcPhysicalSimpleQuantity.MeasureValue`/`.Unit` and `IfcPropertySingleValue.NominalValue`; `UnitParser.Default.TryParse<TUnit>(unit)` resolves the abbreviation and `Quantity.From(measure, parsedUnit)` constructs the typed quantity, SI-coerced into the seam `MeasureValue` — one rail from foreign string to typed seam value, no stringly-keyed unit switch.
- with `Thinktecture.Runtime.Extensions` (`.api/api-thinktecture-json`): the seam `Rasm.Element/Properties/quantity#MEASURE_VALUE` `MeasureValue` `[ComplexValueObject]` value-carrier (over the `Dimension` exponent vector + the `QuantityType` `[ValueObject<string>]` discriminator) and the seam `Rasm.Element/Properties/property#PROPERTY_VALUE` `PropertyValue.Measure` `[Union]` arm own the discriminant; the UnitsNet typed struct is the payload, and the persisted scalar is always `ToUnit(UnitSystem.SI)`-coerced before it enters the carrier — the `[ComplexValueObject]`/`[Union]` owns shape, UnitsNet owns dimension.
- with `LanguageExt.Core`: an unparseable `IfcSIUnit`/`IfcConversionBasedUnit` abbreviation degrades through `UnitParser.Default.TryParse` to a dimensionless seam `MeasureValue` over `Dimension.Dimensionless` (ingest-tolerant) or lowers onto `BimFault.CapabilityMiss`/`Fin<T>` at a hard boundary — never a thrown `UnitNotFoundException`; same-`Dimension` element-set reduction is the seam `MeasureValue.Sum` reducer lifted from the persisted SI scalar through `Length.FromMeters`/`Area.FromSquareMeters`/…, never a `Seq.Fold` over raw `double`.
- with the kernel `Rasm` geometry: the `Semantics/properties#BASE_QUANTITIES` `QuantityDerivation.Derive` reads the kernel `GeometryMeasures` value-object (`Option<double>` `Length`/`Area`/`Volume`, already SI-base from the kernel/Compute resolved by content key) and wraps each through the matching `From*` factory so the takeoff is a typed quantity SI-coerced into a seam `MeasureValue`, the cross-dimensional operators closing a derivation (e.g. a sectional `Area * Length → Volume`, `NetVolume × density → NetWeight`) without leaving the dimensioned algebra — Bim consumes the measure, never tessellating.

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: dimensioned scalar quantities and SI-base unit coercion
- Accept: the seam `MeasureValue` carries a typed value coerced SI-base, its `Dimension`/`QuantityType` the discriminant
- Reject: bare `double` quantity fields, ad-hoc unit-conversion arithmetic, exception-driven unit parse, a stringly-keyed `KindOf`/`QuantityKind` unit switch (the seam `Dimension` replaced it [H2]), a manual `double` aggregation fold
