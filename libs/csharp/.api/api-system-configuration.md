# [RASM_API_SYSTEM_CONFIGURATION]

`System.Configuration.ConfigurationManager` is the .NET XML configuration package for: `app.config`, `web.config`, app settings, connection strings, configuration sections, settings providers, validators, protected configuration, and file-map based load/save. It is centrally pinned as a host compile surface and transitive closure package. It is not the default configuration model for new host code; new runtime configuration composes `Microsoft.Extensions.Configuration`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Configuration.ConfigurationManager`
- package: `System.Configuration.ConfigurationManager`
- assembly: `System.Configuration.ConfigurationManager`
- bound asset: `lib/net10.0` for `net10.0` consumers
- namespaces: `System.Configuration`, `System.Configuration.Provider`, `System.Configuration.Internal`
- admission: central host compile surface; no direct active `.csproj` owner
- role: XML `app.config` / `web.config` model and transitive package closure; active host configuration composes `Microsoft.Extensions.Configuration`
- rail: host-configuration-xml

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: root configuration surfaces
- rail: host-configuration-xml

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------ |:------------------ |:------------------------------------------------ |
| [01] | `ConfigurationManager` | static facade | app settings, connection strings, section lookup, section refresh, configuration open operations |
| [02] | `Configuration` | configuration file | section/group access plus save/save-as operations |
| [03] | `ConfigurationSection` | section base | section serialization, deserialization, and section metadata |
| [04] | `ConfigurationSectionGroup` | section group | grouped section ownership |
| [05] | `ConfigurationElement` | element base | property-backed configuration element |
| [06] | `ConfigurationElementCollection` | element collection | collection of configuration elements |
| [07] | `ConfigurationProperty` | property metadata | declared configuration property |
| [08] | `ConfigurationPropertyAttribute` | property attribute | maps a CLR property to a configuration property |
| [09] | `ConfigurationPropertyCollection` | property collection | section/element property set |
| [10] | `SectionInformation` | section metadata | protection, source, declaration, and override metadata |
| [11] | `ContextInformation` | context metadata | hosting/context metadata for configuration |

[PUBLIC_TYPE_SCOPE]: app settings and connection strings
- rail: host-configuration-xml

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------ |:------------------ |:------------------------------------------------ |
| [01] | `AppSettingsSection` | built-in section | XML app settings section |
| [02] | `ConnectionStringsSection` | built-in section | XML connection strings section |
| [03] | `KeyValueConfigurationCollection` | app setting rows | keyed app-setting collection |
| [04] | `KeyValueConfigurationElement` | app setting row | one keyed app-setting element |
| [05] | `ConnectionStringSettings` | connection row | one named connection string with provider name |
| [06] | `ConnectionStringSettingsCollection` | connection rows | named connection string collection |

[PUBLIC_TYPE_SCOPE]: file maps, save modes, and section collections
- rail: host-configuration-xml

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------ |:------------------ |:------------------------------------------------ |
| [01] | `ConfigurationFileMap` | file map | machine configuration file-map input |
| [02] | `ExeConfigurationFileMap` | file map | executable configuration file-map input |
| [03] | `ConfigurationUserLevel` | open policy enum | per-user vs no-user executable configuration level |
| [04] | `ConfigurationSaveMode` | save policy enum | modified/full/minimal save policy |
| [05] | `ConfigurationSectionCollection` | collection | named section collection |
| [06] | `ConfigurationSectionGroupCollection` | collection | named section-group collection |
| [07] | `ConfigurationErrorsException` | error rail | XML/configuration failure exception |

[PUBLIC_TYPE_SCOPE]: application settings provider model
- rail: host-configuration-xml

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------ |:------------------ |:------------------------------------------------ |
| [01] | `ApplicationSettingsBase` | settings base | strongly typed application/user settings base |
| [02] | `SettingsBase` | settings base | common settings persistence base |
| [03] | `SettingsProvider` | provider base | custom settings provider contract |
| [04] | `LocalFileSettingsProvider` | provider | local file-backed settings provider |
| [05] | `UserScopedSettingAttribute` | setting attribute | user-scoped setting marker |
| [06] | `ApplicationScopedSettingAttribute` | setting attribute | application-scoped setting marker |
| [07] | `DefaultSettingValueAttribute` | setting attribute | default setting value marker |
| [08] | `SettingsProviderAttribute` | setting attribute | settings provider binding |
| [09] | `SettingsSerializeAsAttribute` | setting attribute | setting serialization mode marker |

[PUBLIC_TYPE_SCOPE]: validators and protected configuration
- rail: host-configuration-xml

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------ |:------------------ |:------------------------------------------------ |
| [01] | `ConfigurationValidatorBase` | validator base | custom configuration value validator base |
| [02] | `IntegerValidator` / `LongValidator` | numeric validators | integer range validators |
| [03] | `StringValidator` / `RegexStringValidator` | string validators | string length and regex validators |
| [04] | `TimeSpanValidator` | time validator | time-span range validator |
| [05] | `ProtectedConfiguration` | protection facade | protected configuration provider lookup |
| [06] | `ProtectedConfigurationProvider` | provider base | protected configuration provider contract |
| [07] | `ProtectedConfigurationProviderCollection` | provider collection | named protected configuration providers |
| [08] | `RsaProtectedConfigurationProvider` | provider | RSA protected configuration provider |
| [09] | `DpapiProtectedConfigurationProvider` | provider | DPAPI protected configuration provider |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ConfigurationManager` facade
- rail: host-configuration-xml

```csharp
public static NameValueCollection ConfigurationManager.AppSettings { get; }
public static ConnectionStringSettingsCollection ConfigurationManager.ConnectionStrings { get; }
public static object ConfigurationManager.GetSection(string sectionName);
public static void ConfigurationManager.RefreshSection(string sectionName);
public static Configuration ConfigurationManager.OpenMachineConfiguration();
public static Configuration ConfigurationManager.OpenMappedMachineConfiguration(ConfigurationFileMap fileMap);
public static Configuration ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel userLevel);
public static Configuration ConfigurationManager.OpenExeConfiguration(string exePath);
public static Configuration ConfigurationManager.OpenMappedExeConfiguration(ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel);
public static Configuration ConfigurationManager.OpenMappedExeConfiguration(ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel, bool preLoad);
```

[ENTRYPOINT_SCOPE]: `Configuration` file model
- rail: host-configuration-xml

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------------------- |:----------------- |:------------------------------------------------ |
| [01] | `Configuration.AppSettings` | property | app settings section |
| [02] | `Configuration.ConnectionStrings` | property | connection strings section |
| [03] | `Configuration.FilePath` / `Configuration.HasFile` | properties | file identity and presence |
| [04] | `Configuration.Sections` / `Configuration.SectionGroups` | collections | section and group discovery |
| [05] | `Configuration.GetSection(string)` | lookup | named section lookup |
| [06] | `Configuration.GetSectionGroup(string)` | lookup | named section-group lookup |
| [07] | `Configuration.Save()` | save | save modified configuration |
| [08] | `Configuration.Save(ConfigurationSaveMode)` | save | save with explicit mode |
| [09] | `Configuration.Save(ConfigurationSaveMode, bool)` | save | save with mode and force-save flag |
| [10] | `Configuration.SaveAs(...)` | save-as | write configuration to another file |

## [04]-[IMPLEMENTATION_LAW]

[XML_CONFIGURATION_BOUNDARY]:
- New host configuration code composes `Microsoft.Extensions.Configuration`. This package owns XML configuration APIs and transitive host-compile closure.
- A direct `PackageReference` is added only if a real owner consumes `System.Configuration.*` APIs.
- Do not present `ConfigurationManager` as the workspace default configuration abstraction.
- Do not claim Windows-only behavior unless the exact resolved member/doc surface proves that behavior for the described API.

[LOCAL_ADMISSION]:
- The central manifest pin keeps transitive XML-configuration consumers on the bound package surface without making every project reference it directly.
- File-map APIs are the explicit route when a caller must load a non-default XML configuration file.
- App settings, connection strings, settings providers, validators, and protected configuration are XML configuration surfaces. New application settings must route through the active host configuration owner.

[RAIL_LAW]:
- Package: `System.Configuration.ConfigurationManager`
- Owns: XML configuration files, app settings, connection strings, section/group/element model, settings provider model, validators, protected configuration providers, and file-map open/save operations.
- Accept: central host-compile/transitive pinning; direct use only where `System.Configuration.*` APIs are a real source dependency.
- Reject: universal injection through `Directory.Build.props`, new-code configuration ownership, direct package references with no `System.Configuration.*` source consumer, or platform claims not supported by the resolved API surface.
