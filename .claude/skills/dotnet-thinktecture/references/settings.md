# [SETTINGS]

Attribute settings of value objects, smart enums, ad hoc unions, and regular unions with their defaults and effects, and the generator's MSBuild properties.

## [01]-[VALUE_OBJECTS]

| [INDEX] | [SETTING]                                  | [DEFAULT]     | [EFFECT]                                                      |
| :-----: | :----------------------------------------- | :------------ | :------------------------------------------------------------ |
|   [01]  | `ConversionToKeyMemberType`                | `Implicit`    | `ConversionOperatorsGeneration.None` removes the operator     |
|   [02]  | `ConversionFromKeyMemberType`              | `Explicit`    | Calls `Create`, `None` removes it                             |
|   [03]  | `UnsafeConversionToKeyMemberType`          | `Explicit`    | Class to value-type key, throws on `null`                     |
|   [04]  | `EqualityComparisonOperators`              | `Default`     | `None` or `DefaultWithKeyTypeOverloads`                       |
|   [05]  | `ComparisonOperators`                      | `Default`     | Same values, must match the equality setting (105)            |
|   [06]  | `AdditionOperators` and the 3 siblings     | `Default`     | Present when the key supports it, results go through `Create` |
|   [07]  | `SkipToString` and the 4 `SkipI*` settings | `false`       | Remove the member, `SkipIParsable` also skips `ISpanParsable` |
|   [08]  | `SkipEqualityComparison`                   | `false`       | Removes equality members and both operator settings           |
|   [09]  | `SkipFactoryMethods`                       | `false`       | Removes factories, `TypeConverter`, key conversion, parsing, converters, arithmetic |
|   [10]  | `ConstructorAccessModifier`                | `Private`     | Constructor accessibility                                     |
|   [11]  | `CreateFactoryMethodName`                  | `Create`      | Factory name, `CreateCore` follows the rename                 |
|   [12]  | `TryCreateFactoryMethodName`               | `TryCreate`   | Factory name                                                  |
|   [13]  | `NullInFactoryMethodsYieldsNull`           | `false`       | Class factories return `null` for `null` input                |
|   [14]  | `EmptyStringInFactoryMethodsYieldsNull`    | `false`       | Extends that to blank strings and implies row 13              |
|   [15]  | `AllowDefaultStructs`                      | `false`       | Accepts `default` and emits `public static readonly T Empty`  |
|   [16]  | `DefaultInstancePropertyName`              | `Empty`       | Name of that default instance                                 |
|   [17]  | `KeyMember*` settings                      | Private field | `KeyMemberName`, `KeyMemberAccessModifier`, `KeyMemberKind`   |
|   [18]  | `SkipKeyMember`                            | `false`       | Leaves the key member to the hand-written part                |
|   [19]  | `SerializationFrameworks`                  | `All`         | Flags that select which converter attributes the generator emits |

## [02]-[SMART_ENUMS]

| [INDEX] | [SETTING]                          | [DEFAULT]              | [EFFECT]                                                                   |
| :-----: | :--------------------------------- | :--------------------- | :------------------------------------------------------------------------- |
|   [01]  | `KeyMember*` settings              | `Key`, public property | `KeyMemberName`, `KeyMemberAccessModifier`, `KeyMemberKind`                |
|   [02]  | `SkipI*` interface settings        | `false`                | Drop `IComparable`, `IParsable` (with `ISpanParsable`), or `ISpanParsable` |
|   [03]  | `SkipIFormattable`, `SkipToString` | `false`                | Drop `IFormattable`, keep a hand-written `ToString`                        |
|   [04]  | Operator settings                  | `Default`              | `ComparisonOperators`, `EqualityComparisonOperators`, must match (105)     |
|   [05]  | `ConversionToKeyMemberType`        | `Implicit`             | `ConversionOperatorsGeneration.None` removes it                            |
|   [06]  | `ConversionFromKeyMemberType`      | `Explicit`             | `None` removes it                                                          |
|   [07]  | `SwitchMethods`, `MapMethods`      | `Default`              | `None` or `DefaultWithPartialOverloads`                                    |
|   [08]  | `SerializationFrameworks`          | `All`                  | Flags: `SystemTextJson`, `NewtonsoftJson`, `Json`, `MessagePack`, `None`   |
|   [09]  | `DisableSpanBasedJsonConversion`   | `false`                | String keys only, other keys ignore it                                     |

## [03]-[AD_HOC_UNIONS]

| [INDEX] | [SETTING]                     | [DEFAULT]              | [EFFECT]                                                              |
| :-----: | :---------------------------- | :--------------------- | :-------------------------------------------------------------------- |
|   [01]  | `T1Name` to `T5Name`          | Type name              | Renames `IsX`, `AsX`, `CreateX`, `NormalizeX`, and the `Switch` arm   |
|   [02]  | `DefaultStringComparison`     | `OrdinalIgnoreCase`    | Comparison for `string` members in `Equals` and `GetHashCode`         |
|   [03]  | `Skip*` settings              | `false`                | `SkipToString`, or `SkipEqualityComparison` for equality members      |
|   [04]  | `ConstructorAccessModifier`   | `Public`               | Constructors and factory methods, never the operators                 |
|   [05]  | Conversion settings           | `Implicit`, `Explicit` | `ConversionFromValue`, `ConversionToValue`, `None` disables one       |
|   [06]  | `T1IsNullableReferenceType`   | `false`                | Types the member as `string?` and allows `null`                       |
|   [07]  | `T1IsStateless`               | `false`                | Stores only the discriminator for that member                         |
|   [08]  | `ValueMember*` settings       | `Public`, `Value`      | Accessibility and name of the raw accessor                            |
|   [09]  | Backing field settings        | `false`, none          | `UseSingleBackingField` boxes, `SingleBackingFieldType` types it      |
|   [10]  | `FactoryMethodGeneration`     | `Default`              | `Always` or `None` overrides the trigger rule for `CreateX`           |
|   [11]  | `DefaultValueHandling`        | `Disallow`             | `MapToFirstMember` makes `default` of a struct union the first member |
|   [12]  | `SwitchMethods`, `MapMethods` | `Default`              | `DefaultWithPartialOverloads` adds partial forms, `None` removes all  |
|   [13]  | `SwitchMapStateParameterName` | `state`                | Name of the state parameter                                           |

## [04]-[REGULAR_UNIONS]

| [INDEX] | [SETTING]                     | [DEFAULT]     | [EFFECT]                                                             |
| :-----: | :---------------------------- | :------------ | :------------------------------------------------------------------- |
|   [01]  | `ConversionFromValue`         | `Implicit`    | Operator from a unique single-constructor-parameter type to the base |
|   [02]  | `NestedUnionParameterNames`   | Parent prefix | `NestedUnionParameterNameGeneration.Simple` drops the parent prefix  |
|   [03]  | `SwitchMethods`, `MapMethods` | `Default`     | `DefaultWithPartialOverloads` adds partial forms, `None` removes all |
|   [04]  | `SwitchMapStateParameterName` | `state`       | Name of the state parameter                                          |

## [05]-[GENERATOR_PROPERTIES]

Project-level MSBuild properties carry the prefix `ThinktectureRuntimeExtensions_SourceGenerator_`, reach the compiler as `build_property.<PropertyName>`, and apply to every generated type in the project:

| [INDEX] | [PROPERTY]                     | [VALUES]                                                                      | [DEFAULT] |
| :-----: | :----------------------------- | :---------------------------------------------------------------------------- | :-------- |
|   [01]  | `LogFilePath`                  | File or folder path, trimmed                                                  | No log    |
|   [02]  | `LogFilePathMustBeUnique`      | `true` or `false`                                                             | `true`    |
|   [03]  | `LogLevel`                     | `Trace`, `Debug`, `Information`, `Warning`, `Error`, `None`, case-insensitive | `Warning` |
|   [04]  | `LogMessageInitialBufferSize`  | Integer of at least 100                                                       | `100`     |
|   [05]  | `GenerateJetBrainsAnnotations` | `disable`, `disabled`, `false`, or `0` turn it off, case-insensitive          | On        |
|   [06]  | `Counter`                      | `enable`, `enabled`, `true`, or `1` turn it on, case-insensitive              | Off       |

- `LogFilePath` gates the other logging properties, must name a folder that exists before the build, and blank disables file logging
- `LogLevel` at `Information` shows the generator run and which serialization generators participate, and only `Information`, `Warning`, and `Error` create a file logger
- `LogFilePathMustBeUnique` at `false` collects every compiler process in one file, and the default `true` names a new file per process with a UTC timestamp and a guid
- The generator skips the annotation file when `JetBrains.Annotations.dll` is referenced, so `GenerateJetBrainsAnnotations` needs no value
- `Counter` serves only to detect regeneration (every emitted file starts with `// COUNTER: <n>`), and it is off before generated files are compared or committed
