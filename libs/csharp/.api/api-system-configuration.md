# [RASM_API_SYSTEM_CONFIGURATION]

`System.Configuration.ConfigurationManager` owns the XML configuration document model end to end: one `Configuration` opened per file map and user level, the section, element, and property algebra its bodies declare, the settings-provider persistence rail, and the protected write-back that closes it. `Microsoft.Extensions.Configuration` owns runtime host configuration, so this surface enters only where a caller reads or writes an XML configuration document.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Configuration.ConfigurationManager`
- package: `System.Configuration.ConfigurationManager` (MIT, Microsoft)
- assembly: `System.Configuration.ConfigurationManager.dll`
- namespace: `System.Configuration`, `System.Configuration.Provider`, `System.Configuration.Internal`
- rail: XML configuration document codec behind every `app.config` and `web.config` read or write

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: opened document, its section tree, and the open inputs

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `ConfigurationManager`         | class         | static entry opening every configuration document |
|  [02]   | `Configuration`                | class         | one opened configuration file and its save rail   |
|  [03]   | `ConfigurationSection`         | class         | typed section body a consumer subclasses          |
|  [04]   | `ConfigurationSectionGroup`    | class         | named group holding sections and child groups     |
|  [05]   | `SectionInformation`           | class         | per-section policy, lock, and protection state    |
|  [06]   | `ContextInformation`           | class         | host context a section evaluates against          |
|  [07]   | `ConfigurationFileMap`         | class         | machine configuration file-name input             |
|  [08]   | `ExeConfigurationFileMap`      | class         | exe, roaming-user, and local-user file-name input |
|  [09]   | `ConfigurationUserLevel`       | enum          | open level selecting the per-user files           |
|  [10]   | `ConfigurationSaveMode`        | enum          | modified, minimal, or full write policy           |
|  [11]   | `ConfigurationErrorsException` | class         | failure rail carrying file name and line number   |

[DOCUMENT_INDEX]: `ConfigurationSectionCollection` `ConfigurationSectionGroupCollection` `ConfigurationLocation` `ConfigurationLocationCollection` `ExeContext`
[SECTION_POLICY]: `ConfigurationAllowDefinition` `ConfigurationAllowExeDefinition` `OverrideMode`

[PUBLIC_TYPE_SCOPE]: element and property algebra every custom section composes

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `ConfigurationElement`               | class         | property-bag base every section and row body extends    |
|  [02]   | `ConfigurationElementCollection`     | class         | keyed row collection base with add-remove-clear merge   |
|  [03]   | `ConfigurationElementCollectionType` | enum          | merge and inherited-sort shape a collection declares    |
|  [04]   | `ConfigurationProperty`              | class         | one declaration binding type, default, converter, guard |
|  [05]   | `ConfigurationPropertyCollection`    | class         | declared property table of an element                   |
|  [06]   | `ConfigurationPropertyOptions`       | enum          | required, key, default-collection, and transform flags  |
|  [07]   | `ConfigurationPropertyAttribute`     | class         | declarative property binding on a CLR property          |
|  [08]   | `ConfigurationCollectionAttribute`   | class         | declarative collection shape on a property              |
|  [09]   | `ConfigurationLockCollection`        | class         | attribute and element lock set of one element           |
|  [10]   | `ElementInformation`                 | class         | runtime source, lock, and validity state of an element  |

[PROPERTY_STATE]: `PropertyInformation` `PropertyInformationCollection` `PropertyValueOrigin` `ConfigurationElementProperty`

[PUBLIC_TYPE_SCOPE]: built-in section classes and their row elements

| [INDEX] | [SYMBOL]                             | [CAPABILITY]                                        |
| :-----: | :----------------------------------- | :-------------------------------------------------- |
|  [01]   | `AppSettingsSection`                 | `<appSettings>` body with an optional external file |
|  [02]   | `ConnectionStringsSection`           | `<connectionStrings>` body                          |
|  [03]   | `KeyValueConfigurationCollection`    | app-setting rows keyed by `key`                     |
|  [04]   | `KeyValueConfigurationElement`       | one `key`/`value` app-setting row                   |
|  [05]   | `ConnectionStringSettings`           | one named connection string with its provider name  |
|  [06]   | `ConnectionStringSettingsCollection` | connection strings keyed by name and by index       |
|  [07]   | `ProviderSettings`                   | one named provider declaration with its parameters  |
|  [08]   | `ClientSettingsSection`              | serialized client settings of one settings group    |

[SECTION_BODY]: `ProtectedConfigurationSection` `IgnoreSection` `DefaultSection` `NameValueConfigurationCollection` `NameValueConfigurationElement` `ProviderSettingsCollection` `SettingElement` `SettingElementCollection` `SettingValueElement` `CommaDelimitedStringCollection`

[PUBLIC_TYPE_SCOPE]: settings-provider persistence rail

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :----------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `ApplicationSettingsBase`      | class         | typed settings base raising change and save events   |
|  [02]   | `SettingsBase`                 | class         | property-bag base bound to a provider collection     |
|  [03]   | `SettingsProvider`             | class         | settings load and store contract                     |
|  [04]   | `LocalFileSettingsProvider`    | class         | user-scoped local-file provider with version upgrade |
|  [05]   | `SettingsProperty`             | class         | one declared setting with default and serialization  |
|  [06]   | `SettingsPropertyValue`        | class         | serialized and deserialized value of one setting     |
|  [07]   | `SettingsSerializeAs`          | enum          | string, XML, or provider-specific serialization      |
|  [08]   | `IApplicationSettingsProvider` | interface     | upgrade, reset, and previous-version contract        |

[SETTINGS_STORE]: `SettingsContext` `SettingsPropertyCollection` `SettingsPropertyValueCollection` `SettingsProviderCollection` `ISettingsProviderService` `IPersistComponentSettings` `SettingsManageability` `SpecialSetting`
[SETTING_ATTRIBUTE]: `SettingAttribute` `ApplicationScopedSettingAttribute` `UserScopedSettingAttribute` `DefaultSettingValueAttribute` `SettingsProviderAttribute` `SettingsSerializeAsAttribute` `SettingsManageabilityAttribute` `SpecialSettingAttribute` `SettingsDescriptionAttribute` `SettingsGroupNameAttribute` `SettingsGroupDescriptionAttribute` `NoSettingsVersionUpgradeAttribute`
[SETTINGS_EVENT]: `SettingChangingEventHandler` `SettingChangingEventArgs` `SettingsLoadedEventHandler` `SettingsLoadedEventArgs` `SettingsSavingEventHandler`

[PUBLIC_TYPE_SCOPE]: validation, value conversion, and protected configuration

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `ConfigurationValidatorBase`     | class         | validator contract a property declaration binds |
|  [02]   | `ValidatorCallback`              | delegate      | validation body a `CallbackValidator` wraps     |
|  [03]   | `ConfigurationConverterBase`     | class         | type-converter base for configuration values    |
|  [04]   | `ProtectedConfiguration`         | class         | provider registry and default-provider lookup   |
|  [05]   | `ProtectedConfigurationProvider` | class         | XML node encrypt and decrypt contract           |
|  [06]   | `ProviderBase`                   | class         | named provider initialization base              |

[VALIDATOR]: `IntegerValidator` `LongValidator` `StringValidator` `RegexStringValidator` `TimeSpanValidator` `PositiveTimeSpanValidator` `SubclassTypeValidator` `CallbackValidator` `DefaultValidator`
[VALIDATOR_ATTRIBUTE]: `ConfigurationValidatorAttribute` `IntegerValidatorAttribute` `LongValidatorAttribute` `StringValidatorAttribute` `RegexStringValidatorAttribute` `TimeSpanValidatorAttribute` `PositiveTimeSpanValidatorAttribute` `SubclassTypeValidatorAttribute` `CallbackValidatorAttribute`
[VALUE_CONVERTER]: `GenericEnumConverter` `TypeNameConverter` `InfiniteIntConverter` `InfiniteTimeSpanConverter` `TimeSpanMinutesConverter` `TimeSpanMinutesOrInfiniteConverter` `TimeSpanSecondsConverter` `TimeSpanSecondsOrInfiniteConverter` `WhiteSpaceTrimStringConverter` `CommaDelimitedStringCollectionConverter`
[PROTECTION_PROVIDER]: `RsaProtectedConfigurationProvider` `DpapiProtectedConfigurationProvider` `ProtectedConfigurationProviderCollection` `ProviderCollection` `ProviderException`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ConfigurationManager` — process-default reads and every mapped open

| [INDEX] | [SURFACE]                                                                           | [SHAPE] | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `AppSettings -> NameValueCollection`                                                | static  | process-default app settings          |
|  [02]   | `ConnectionStrings`                                                                 | static  | process-default connection strings    |
|  [03]   | `GetSection(string) -> object`                                                      | static  | named section of the default document |
|  [04]   | `RefreshSection(string)`                                                            | static  | drop the cache before the next read   |
|  [05]   | `OpenMachineConfiguration()`                                                        | static  | open `machine.config`                 |
|  [06]   | `OpenMappedMachineConfiguration(ConfigurationFileMap)`                              | static  | open a mapped machine file            |
|  [07]   | `OpenExeConfiguration(ConfigurationUserLevel)`                                      | static  | open this exe at a user level         |
|  [08]   | `OpenExeConfiguration(string)`                                                      | static  | open another executable's document    |
|  [09]   | `OpenMappedExeConfiguration(ExeConfigurationFileMap, ConfigurationUserLevel)`       | static  | open mapped exe and user files        |
|  [10]   | `OpenMappedExeConfiguration(ExeConfigurationFileMap, ConfigurationUserLevel, bool)` | static  | mapped open, preloading sections      |

[ENTRYPOINT_SCOPE]: `Configuration` — one opened document a caller reads, mutates, and writes

[DOCUMENT_READ]: `AppSettings` `ConnectionStrings` `FilePath` `HasFile` `Sections` `SectionGroups` `RootSectionGroup` `Locations` `EvaluationContext` `NamespaceDeclared` `TargetFramework`

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :-------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `GetSection(string) -> ConfigurationSection`  | instance | one section by name                     |
|  [02]   | `GetSectionGroup(string)`                     | instance | one section group by name               |
|  [03]   | `Save()`                                      | instance | write modified sections back            |
|  [04]   | `Save(ConfigurationSaveMode)`                 | instance | write under an explicit save policy     |
|  [05]   | `Save(ConfigurationSaveMode, bool)`           | instance | write, optionally forcing every section |
|  [06]   | `SaveAs(string)`                              | instance | write modified sections to another file |
|  [07]   | `SaveAs(string, ConfigurationSaveMode)`       | instance | write to another file under a policy    |
|  [08]   | `SaveAs(string, ConfigurationSaveMode, bool)` | instance | write to another file, forcing all      |

[ENTRYPOINT_SCOPE]: `ConfigurationElement` — property-bag body every custom section extends

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `this[ConfigurationProperty]`                           | property | slot of one declared property            |
|  [02]   | `this[string]`                                          | property | slot by declared property name           |
|  [03]   | `Properties`                                            | property | declared property table this body owns   |
|  [04]   | `ElementInformation`                                    | property | runtime source, lock, and validity state |
|  [05]   | `CurrentConfiguration`                                  | property | document this element was loaded from    |
|  [06]   | `OnDeserializeUnrecognizedAttribute(string, string)`    | instance | absorb an undeclared attribute           |
|  [07]   | `OnDeserializeUnrecognizedElement(string, XmlReader)`   | instance | absorb an undeclared child element       |
|  [08]   | `OnRequiredPropertyNotFound(string) -> object`          | instance | supply a missing required value          |
|  [09]   | `PostDeserialize()`                                     | instance | cross-property check after load          |
|  [10]   | `PreSerialize(XmlWriter)`                               | instance | hook before this body serializes         |
|  [11]   | `SerializeElement(XmlWriter, bool) -> bool`             | instance | write this body's XML                    |
|  [12]   | `SetPropertyValue(ConfigurationProperty, object, bool)` | instance | set a slot, optionally ignoring locks    |

[ENTRYPOINT_SCOPE]: `ConfigurationElementCollection` — keyed rows over the element property table

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `CreateNewElement()`                            | instance | mint one row of the collection         |
|  [02]   | `CreateNewElement(string)`                      | instance | mint one row by element name           |
|  [03]   | `GetElementKey(ConfigurationElement) -> object` | instance | key a row for merge and lookup         |
|  [04]   | `CollectionType`                                | property | merge and inherited-sort shape         |
|  [05]   | `BaseAdd(ConfigurationElement, bool)`           | instance | add a row, rejecting duplicates or not |
|  [06]   | `BaseGet(object) -> ConfigurationElement`       | instance | one row by key                         |
|  [07]   | `BaseGetAllKeys() -> object[]`                  | instance | every row key in document order        |
|  [08]   | `BaseRemove(object)`                            | instance | remove one row by key                  |
|  [09]   | `BaseRemoveAt(int)`                             | instance | remove one row by position             |
|  [10]   | `BaseClear()`                                   | instance | drop every row                         |

[ROW_LOOKUP]: `BaseGet(int)` `BaseGetKey(int)` `BaseIndexOf(ConfigurationElement)` `BaseIsRemoved(object)`
[PROPERTY_CTOR]: `ConfigurationProperty(string, Type)` `ConfigurationProperty(string, Type, object)` `ConfigurationProperty(string, Type, object, ConfigurationPropertyOptions)` `ConfigurationProperty(string, Type, object, TypeConverter, ConfigurationValidatorBase, ConfigurationPropertyOptions)`

[ENTRYPOINT_SCOPE]: `SectionInformation` — per-section policy, protection, and raw XML

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `ProtectSection(string)`                     | instance | bind a protection provider by name         |
|  [02]   | `UnprotectSection()`                         | instance | drop protection from the section           |
|  [03]   | `ForceDeclaration(bool)`                     | instance | force or drop the `<configSections>` entry |
|  [04]   | `GetRawXml() -> string`                      | instance | read the section's XML verbatim            |
|  [05]   | `SetRawXml(string)`                          | instance | replace the section's XML verbatim         |
|  [06]   | `RevertToParent()`                           | instance | discard local values, inherit the parent   |
|  [07]   | `GetParentSection() -> ConfigurationSection` | instance | the section this one inherits              |

[POLICY_SLOT]: `AllowDefinition` `AllowExeDefinition` `AllowLocation` `AllowOverride` `ConfigSource` `ForceSave` `InheritInChildApplications` `IsDeclarationRequired` `IsDeclared` `IsLocked` `IsProtected` `OverrideMode` `OverrideModeDefault` `OverrideModeEffective` `ProtectionProvider` `RequirePermission` `RestartOnExternalChanges` `SectionName` `Type`

[ENTRYPOINT_SCOPE]: protection providers, settings persistence, and validation

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `ProtectedConfiguration.Providers`                             | static   | registered protection providers         |
|  [02]   | `ProtectedConfiguration.DefaultProvider`                       | static   | provider a bare `ProtectSection` binds  |
|  [03]   | `ProtectedConfigurationProvider.Encrypt(XmlNode) -> XmlNode`   | instance | encrypt a section's XML node            |
|  [04]   | `ProtectedConfigurationProvider.Decrypt(XmlNode) -> XmlNode`   | instance | decrypt a section's XML node            |
|  [05]   | `ApplicationSettingsBase.Save()`                               | instance | store every dirty setting               |
|  [06]   | `ApplicationSettingsBase.Reload()`                             | instance | re-read every setting from its provider |
|  [07]   | `ApplicationSettingsBase.Reset()`                              | instance | restore declared defaults               |
|  [08]   | `ApplicationSettingsBase.Upgrade()`                            | instance | carry values forward from a prior build |
|  [09]   | `ApplicationSettingsBase.GetPreviousVersion(string) -> object` | instance | one setting's prior-version value       |
|  [10]   | `ConfigurationValidatorBase.CanValidate(Type) -> bool`         | instance | accept a value type                     |
|  [11]   | `ConfigurationValidatorBase.Validate(object)`                  | instance | throw on an inadmissible value          |

[PROVIDER_OP]: `SettingsProvider.GetPropertyValues(SettingsContext, SettingsPropertyCollection) -> SettingsPropertyValueCollection` `SettingsProvider.SetPropertyValues(SettingsContext, SettingsPropertyValueCollection)` `SettingsBase.Initialize(SettingsContext, SettingsPropertyCollection, SettingsProviderCollection)`
[FILE_MAP_SLOT]: `ConfigurationFileMap.MachineConfigFilename` `ExeConfigurationFileMap.ExeConfigFilename` `ExeConfigurationFileMap.RoamingUserConfigFilename` `ExeConfigurationFileMap.LocalUserConfigFilename`
[VALIDATOR_CTOR]: `IntegerValidator(int, int, bool, int)` `LongValidator(long, long, bool, long)` `StringValidator(int, int, string)` `RegexStringValidator(string)` `TimeSpanValidator(TimeSpan, TimeSpan, bool, long)` `SubclassTypeValidator(Type)` `CallbackValidator(Type, ValidatorCallback)`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ConfigurationManager` opens one `Configuration` per file map and user level; every section read, element mutation, and save below it addresses that handle, never ambient process state.
- A custom section declares its shape once: each `ConfigurationProperty` binds type, default, `TypeConverter`, and `ConfigurationValidatorBase` together, and deserialization, validation, and serialization fold over the resulting `ConfigurationPropertyCollection`.
- `SectionInformation` carries the policy a section enforces on child documents — definition level, override mode, lock state, protection provider — so inheritance is section metadata rather than caller branching.

[STACKING]:
- `System.Xml`(`libs/csharp/.api/api-bcl-xml.md`): `ProtectedConfigurationProvider.Encrypt(XmlNode)` and `Decrypt(XmlNode)` exchange DOM nodes, so section protection composes the owned XML surface instead of a private parser.
- `Microsoft.Extensions.Configuration`(`Rasm.AppHost/.api/api-config.md`): `ConfigurationManager.AppSettings` and `Configuration.ConnectionStrings` project to key-value rows that `AddInMemoryCollection` mounts as one ordered source on the host configuration root.
- `ConfigurationElementCollection` extends the element property table to keyed rows through `CreateNewElement` and `GetElementKey`, so a row family inherits merge, lock, validation, and save from one declaration with no per-collection parser.
- `SectionInformation.ProtectSection` encrypts whatever that declaration serializes, so protection composes over any section body without a second write path.

[LOCAL_ADMISSION]:
- A non-default XML configuration document opens through a file map; the running executable's own opens through `ConfigurationManager.OpenExeConfiguration`.
- New application settings route through the active host configuration owner.

[RAIL_LAW]:
- Package: `System.Configuration.ConfigurationManager`
- Owns: XML configuration document model end to end — open, section algebra, element and property declaration, settings persistence, validation, protected write-back
- Accept: documents opened through a file map or user level, `ConfigurationSection` subclasses declaring their own property table, `SettingsProvider` persistence, `ProtectedConfigurationProvider` encryption
- Reject: a hand-parsed `app.config` or a bespoke XML settings reader beside this surface
