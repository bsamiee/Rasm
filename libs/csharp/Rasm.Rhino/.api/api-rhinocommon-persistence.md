# [RASM_RHINO_API_RHINOCOMMON_PERSISTENCE]

`ArchivableDictionary` is the string-keyed serializable value store, `PersistentSettings` the per-plugin/app settings tree, `UserData` the attach-to-object custody family, and `StringTable` with `ObjectAttributes` the document and per-object user text. Value writes admit a wider type vocabulary than typed reads, so every value lacking a typed getter reads back as a boxed `object` through the untyped indexer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon` persistence surface
- package: `RhinoCommon`
- assembly: `RhinoCommon.dll` — in-process managed host runtime
- namespaces: `Rhino`, `Rhino.Collections`, `Rhino.DocObjects`, `Rhino.DocObjects.Custom`, `Rhino.DocObjects.Tables`
- rail: custody-boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serialization dictionary and settings tree

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :--------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `ArchivableDictionary` | class         | string-keyed serializable value store; typed write/read over 48 setters |
|  [02]   | `PersistentSettings`   | class         | per-plugin/app settings tree with child nodes, defaults, and validators |

[PUBLIC_TYPE_SCOPE]: attached custom-data and user text

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [CAPABILITY]                                                        |
| :-----: | :----------------- | :------------- | :------------------------------------------------------------------ |
|  [01]   | `UserData`         | abstract class | attach-to-object custody base with serialize and duplicate seams    |
|  [02]   | `UserDictionary`   | class          | `UserData` carrying one attached `ArchivableDictionary`             |
|  [03]   | `UserDataList`     | class          | per-object user-data collection; add, find by type, purge           |
|  [04]   | `UnknownUserData`  | class          | opaque carrier for user data of unrecognized origin                 |
|  [05]   | `StringTable`      | sealed class   | document user text as flat keys and section/entry pairs             |
|  [06]   | `ObjectAttributes` | class          | per-object user-string slice; document/tables catalogs own the rest |

[PUBLIC_TYPE_SCOPE]: settings codec and event payloads

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]  | [CAPABILITY]                                                                |
| :-----: | :--------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `PersistentSettingsConverter`      | static class   | string⇄list/dictionary/enum/double codec behind every settings value kind   |
|  [02]   | `PersistentSettingsEventArgs`      | abstract class | validator payload base; sole member `Cancel : bool` (default false)         |
|  [03]   | `PersistentSettingsEventArgs<T>`   | class          | `CurrentValue : T` (settable) + `NewValue : T`; the typed validator payload |
|  [04]   | `PersistentSettingsSavedEventArgs` | class          | `SavedByThisRhino : bool` + `PlugInSettings` + `CommandSettings(string)`    |

`PersistentSettingsConverter` carries `IsStringList` / `IsStringDictionary` / `TryParseStringList(out string[])` / `TryParseStringDictionary(out KeyValuePair<string,string>[])` / `ToString(string[])` / `ToString(KeyValuePair<string,string>[])` / `ToString(double)` / `TryParseDouble(out double)` / `TryParseEnum(Type, string, out string)` / `TryParseEnum(Type, string, out int)`. Saved notification rides `Rhino.PlugIns.PlugIn.SettingsSaved : EventHandler<PersistentSettingsSavedEventArgs>`; no standalone saved-delegate type exists.

`ArchivableDictionary.ItemType` (value discriminant) and `ArchivableDictionary.DictionaryItem` (item carrier) are `private`; the Rasm value union reconstructs the discriminant from the public `Set`/`Get` overload roster. `SharedUserDictionary` is `internal` and never crosses the SDK boundary.

## [03]-[ENTRYPOINTS]

[DICTIONARY_VALUE_WRITE]:
- `ArchivableDictionary.Set(string, T) : bool` over primitives {`bool`, `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `float`, `double`, `Guid`, `string`} — upsert; false on null/empty key, null value, or undefined type.
- `Set(string, T) : bool` over drawing types {`Color`, `Point`, `PointF`, `Rectangle`, `RectangleF`, `Size`, `SizeF`, `Font`} — same upsert contract.
- `Set(string, T) : bool` over geometry values {`Interval`, `Point2d`, `Point3d`, `Point4d`, `Vector2d`, `Vector3d`, `BoundingBox`, `Ray3d`, `Transform`, `Plane`, `Line`, `Point3f`, `Vector3f`} — every geometry value is a struct copied by value on store, unlike the reference-aliasing carriers below.
- `Set(string, T) : bool` over carriers {nested `ArchivableDictionary`, `MeshingParameters`, `GeometryBase`, `ObjRef`} — stores the caller reference VERBATIM and `TryGetValue` returns that stored reference, so reference payloads alias both ways; caller-side freezing rides `GeometryBase.Duplicate()` and the copy constructors `new MeshingParameters(MeshingParameters)` / `new ObjRef(ObjRef)`, since neither carrier implements `ICloneable`.
- `Set(string, IEnumerable<T>) : bool` over {`bool`, `byte`, `sbyte`, `short`, `int`, `float`, `double`, `Guid`, `string`, `ObjRef`, `GeometryBase`} — lands as one array item; no `uint`/`long`/`ushort` enumerable overload exists.
- `SetEnumValue<T>(T)` / `SetEnumValue<T>(string, T)` where `T : struct, IConvertible` — the keyless overload derives the key from `typeof(T).Name`; storage is the invariant-culture name string as an `ItemType.String` item, so an enum entry is `GetString`-readable and indistinguishable from a plain string.
- `ItemType.PlaneEquation` (38) has no public `Set` and no typed getter — a loaded archive's `double[]` plane-equation entry reaches only through the boxed indexer. No `DateTime` value kind exists in either direction.

[DICTIONARY_VALUE_READ]:
- `TryGet<Kind>(string, out T)` / `Get<Kind>(string) : T` / `Get<Kind>(string, T default) : T` over {`String`, `Dictionary`, `Bytes`, `Bool`, `Float`, `Double`, `Integer`, `Point3f`, `Point3d`, `Vector3d`, `Guid`, `Plane`} — typed read is narrower than write: `Color`, `Size`, `Rectangle`, `Point`, `Interval`, and `Transform` write typed but read back only through the boxed indexer.
- `Get<Kind>(string)` throws `KeyNotFoundException` on a missing key and `NotSupportedException` on a type mismatch; the `Get<Kind>(key, default)` sibling swallows both and returns the fallback — opposite failure contracts on one name.
- `GetEnumValue<T>()` / `GetEnumValue<T>(string)` / `TryGetEnumValue<T>(string, out T)` / `RemoveEnumValue<T>()` where `T : struct, IConvertible` — reads `Enum.Parse(ignoreCase: true)` over the stored name; `GetEnumValue<T>()` throws `KeyNotFoundException`/`FormatException`, a non-enum `T` throws `ArgumentException`, and no `(key, default)` overload exists here.
- `this[string] : object` — getter throws `KeyNotFoundException` on a missing key; the setter accepts only {`int`, `long`, `bool`, `double`, `string`, `GeometryBase`, `IEnumerable<GeometryBase>`} and throws `NotSupportedException` otherwise; `TryGetValue(string, out object)` is the non-throwing boxed read.
- `ContainsKey(string)` / `Keys : string[]` / `Values : object[]` / `Count : int` — membership and enumeration.

[DICTIONARY_LIFECYCLE]:
- `AddContentsFrom(ArchivableDictionary) : bool` — reflects `GetMethod("Set", {string, value.GetType()})` with exact-type match and no assignability: every array entry (runtime `int[]` versus parameter `IEnumerable<int>`) and every `GeometryBase` subtype or `ObjRef` entry finds no overload and throws `ArgumentException`; only exact-type scalar entries survive. Throws `ArgumentNullException` on null source.
- `ReplaceContentsWith(ArchivableDictionary) : bool` — clears, then `AddContentsFrom`; same throw surface.
- `Clone() : ArchivableDictionary` — preserves `Version`, `Name`, and `ChangeSerialNumber`; a value clones only when it implements `ICloneable` (nested `ArchivableDictionary` and arrays clone; `GeometryBase`, `MeshingParameters`, and `ObjRef` values share by reference), so the clone is not deep and no `CloneValue`/`DeepCloneValue` exists.
- `Remove(string)` / `Clear()` — `IDictionary<string,object>.Add(key,value)` always throws `NotSupportedException`, so mutation enters only through `Set`.
- `Version : int` (get/set) / `Name : string` (get/set) / `ChangeSerialNumber : uint` — the serial increments only on `Set`, never on remove or clear.
- `ParentUserData : UserData` — owning user data, null when detached.
- `ArchivableDictionary(int version, string name)` / `(int version)` / `(UserData)` / `()` — the `int version` overloads stamp the archive schema version.
- `[Serializable] : ISerializable` — `protected ArchivableDictionary(SerializationInfo, StreamingContext)` + `GetObjectData` round-trip the dictionary as a native ON buffer under `"version"`/`"archive3dm"`/`"opennurbs"`/`"data"` entries.

[SETTINGS_TREE]:
- `Rhino.PersistentSettings.FromPlugInId(Guid) : PersistentSettings` — plugin settings root, throwing `InvalidEnumArgumentException` on `Guid.Empty`; `RhinoAppSettings : PersistentSettings` — a static property lazily resolving `FromPlugInId(RhinoApp.CurrentRhinoId)`.
- `GetChild(string)` / `TryGetChild(string, out PersistentSettings)` / `AddChild(string) : PersistentSettings` / `DeleteChild(string)` — the child-settings hierarchy.
- `Keys : ICollection<string>` / `ChildKeys : ICollection<string>` — value keys and child-node keys of the current node.
- `HiddenFromUserInterface : bool` (get/set) — the whole-node visibility flag, distinct from the per-key `HideSettingFromUserInterface`.

[SETTINGS_TYPED_READ]:
- `TryGet<Kind>(string, out T)` / `Get<Kind>(string) : T` / `Get<Kind>(string, T default) : T` over {`Guid`, `Bool`, `Byte`, `Integer`, `UnsignedInteger`, `Double`, `Char`, `String`, `StringList`(`string[]`), `StringDictionary`(`KeyValuePair<string,string>[]`), `Date`(`DateTime`), `Color`, `Color?`, `Point`, `Point3d`, `Size`, `Rectangle`} — each read overload has an `IEnumerable<string> legacyKeyList` sibling that falls back across renamed keys.
- `GetInteger(string, int default, int bound, bool boundIsLower)` / `GetInteger(string, int default, int lowerBound, int upperBound)` — clamp the stored integer to a one-sided or two-sided bound; `int` is the only kind carrying clamp overloads.
- `GetEnumValue<T>(string)` / `GetEnumValue<T>(string, T default)` / `GetEnumValue<T>(T default)` / `TryGetEnumValue<T>(string, out T)` where `T : struct, IConvertible` — `TryGetEnumValue` always assigns `enumValue` (`default(T)` on failure), so only the returned bool proves presence.
- Every defaulted `Get<Kind>(key, default)` MUTATES the tree: on hit it stamps the default layer, on miss it registers the default AND materializes the key to it, marking the node changed; the no-default `Get<Kind>(string)` instead throws `KeyNotFoundException` on a missing key and `NotSupportedException` on a type mismatch.

[SETTINGS_TYPED_WRITE]:
- `Set<Kind>(string, T)` as the named methods `SetGuid`/`SetBool`/`SetByte`/`SetInteger`/`SetUnsignedInteger`/`SetDouble`/`SetChar`/`SetString`/`SetStringList`/`SetStringDictionary`/`SetDate`/`SetColor`(over `Color` and `Color?`)/`SetPoint3d`/`SetRectangle`/`SetSize`/`SetPoint`/`SetEnumValue<T>` — one typed writer per value kind.
- `DeleteItem(string)` — removes one stored value.

[SETTINGS_DEFAULTS]:
- `SetDefault(string, T)` over 16 kinds {`bool`, `byte`, `int`, `double`, `char`, `string`, `string[]`, `KeyValuePair<string,string>[]`, `DateTime`, `Color`, `Color?`, `Rectangle`, `Size`, `Point`, `Point3d`, `Guid`} — the fallback layer read when no explicit value is stored; no `SetDefault(uint)` exists despite `SetUnsignedInteger`.
- `TryGetDefault(string, out T)` over 12 kinds {`bool`, `byte`, `int`, `double`, `char`, `string`, `string[]`, `DateTime`, `Color`, `Point3d`, `Size`, `Rectangle`} — reads the default layer without the resolved value; Guid, Point, `Color?`, StringDictionary, and uint have no default probe.
- Each `SettingValue` holds one explicit value beside one typed default per key, so the two layers resolve independently.

[SETTINGS_METADATA]:
- `GetSettingType(string) : Type` / `TryGetSettingType(string, out Type)` — the stored value's runtime type.
- `GetSettingIsReadOnly(string) : bool` / `TryGetSettingIsReadOnly(string, out bool)` — the read-only flag.
- `HideSettingFromUserInterface(string)` / `GetSettingIsHiddenFromUserInterface(string) : bool` / `TryGetSettingIsHiddenFromUserInterface(string, out bool)` — visibility flag; both readers carry a `legacyKeyList` sibling.
- `RegisterSettingsValidator<T>(string, EventHandler<PersistentSettingsEventArgs<T>>)` / `GetValidator<T>(string)` — the per-key typed validator seam; registration OVERWRITES an existing validator unconditionally, `GetValidator<T>` returns null when none is installed and throws `InvalidCastException` when `T` mismatches the registered specialization, and a registered validator fires on every typed set AND every default stamp, its args carrying settable `Cancel` and `CurrentValue`.
- `ContainsChangedValues() : bool` / `ClearChangedFlag()` / `ContainsModifiedValues(PersistentSettings)` — change tracking against the persisted baseline; change notification is the validator handler and `PlugIn.SettingsSaved`, never a per-change event.
- `StringListRootKey : string` — a sentinel list ELEMENT: placed inside a `SetStringList` array it splices the all-users ProgramData list at that position on `GetStringList`.
- `[Serializable] : ISerializable` — `GetObjectData` delegates to the live tree, and the protected deserialization constructor has an empty body (a hollow instance).

[USERDATA_CUSTODY]:
- `UserData.Description : string` (virtual, default `"RhinoCommon UserData"`) / `ShouldWrite : bool` (virtual, default false) — serialization identity and opt-in; the protected `Write`/`Read` defaults return false, so a `ShouldWrite => true` override without both IO overrides serializes as a silent no-op.
- `UserData.Transform : Transform` — accumulated object transform, read from native per call and set through the protected setter; an `OnTransform` override that skips the base call leaves it stale.
- `UserData.Dispose()` — releases the native handle; the finalizer mirrors it.
- `UserData.Copy(CommonObject, CommonObject)` (static) / `MoveUserDataFrom(CommonObject) : Guid` (static, `Guid.Empty` when nothing moved) / `MoveUserDataTo(CommonObject, Guid, bool append)` (static, no-op on `Guid.Empty`) — cross-object custody transfer.
- `UserData.Write(BinaryArchiveWriter)` / `Read(BinaryArchiveReader)` / `OnTransform(Transform)` / `OnDuplicate(UserData)` / `Dispose(bool)` — the protected override seams a derived class implements.
- `Rhino.DocObjects.Custom.ClassIdAttribute` — `ctor(string)` + `Id : Guid`; pins a stable class id onto a `UserData` subclass so archives written under a prior class name keep resolving it.
- `UserDictionary : UserData` — `Dictionary : ArchivableDictionary` lazily parented to the user data; `Description` is `"RhinoCommon UserDictionary"`, `ShouldWrite` returns true only when `Dictionary.Count > 0`, `Read` is `archive.ReadDictionary()` and `Write` is `archive.WriteDictionary(Dictionary)`.
- `Rhino.Runtime.CommonObject.UserData : UserDataList` — lazy list accessor allocating the roster on first read; `CommonObject.UserDictionary : ArchivableDictionary` — lazy MUTATING accessor: finds the internal `SharedUserDictionary`, constructs and attaches one through `UserData.Add` when absent, and returns null when that `Add` fails.
- `UserDataList : IEnumerable<UserData>` — `Add(UserData) : bool` throws `ArgumentException` unless the type is a public class with a parameterless constructor (the internal `SharedUserDictionary` bypasses that gate, and `Add` detaches an entry from a prior roster before attaching) / `Remove(UserData) : bool` detaches WITHOUT disposing / `Find(Type) : UserData` / `Contains(Guid) : bool` / `Purge()` / `this[int] : UserData` / `Count : int` / `GetEnumerator()`.
- `UnknownUserData(nint pointerNativeUserData)` — sole public member; carries opaque native user data across a round-trip without decoding it.

[ARCHIVE_IO]:
- `BinaryArchiveWriter.WriteDictionary(ArchivableDictionary)` / `BinaryArchiveReader.ReadDictionary() : ArchivableDictionary` — the custody-spine pair every `UserData.Write`/`Read` body composes.
- `Write3dmChunkVersion(int major, int minor)` / `Read3dmChunkVersion(out int major, out int minor)` — payload schema framing; `BeginWrite3dmChunk`/`EndWrite3dmChunk` pair with `BeginRead3dmChunk`/`EndRead3dmChunk(bool)` and `BeginReadDictionaryEntry(out int, out string)` for entry-level walks.
- `WriteErrorOccured : bool` / `ReadErrorOccured : bool` / `EnableCRCCalculation(bool)` / `WriteEmptyCheckSum` / `ReadCheckSum` / `Archive3dmVersion : int` — archive error and integrity state read after each IO leg.
- `AtEnd : bool` / `CurrentPosition : ulong` / `SeekFromStart(ulong)` / `SeekFromCurrentPosition` — reader positioning.
- Typed `Write*`/`Read*` pairs: Bool, Byte, SByte, Short, UShort, Int, UInt, Int64, Single, Double, String, Utf8String, Guid, Color, Font, the `System.Drawing` Point/PointF/Rectangle/RectangleF/Size/SizeF set, Point2d/Point3d/Point3f/Point4d, Vector2d/Vector3d/Vector3f, Interval, Line, Plane, Ray3d, BoundingBox, Transform, `Geometry(GeometryBase)`, MeshingParameters, ObjRef, RenderSettings, `*Array` variants (Bool/Byte/SByte/Short/Single/Double/Int/Guid/String/Geometry/ObjRef), and `Write/ReadCompressedBuffer`.

[USER_TEXT]:
- `StringTable.SetString(string section, string entry, string value) : string` / `SetString(string key, string value) : string` — writes and returns the prior value captured through `GetValue` before the write (a string, empty when absent, never a guaranteed null); a null value deletes the entry, and the section overload forms the key `section\entry`.
- `GetValue(string section, string entry)` / `GetValue(string key)` / `GetValue(int)` / `GetKey(int)` — section/entry, flat-key, and positional reads; the section/entry read returns `string.Empty` on an empty key.
- `GetSectionNames() : string[]` / `GetEntryNames(string) : string[]` — the section and entry rosters, sorted and never null.
- `Count : int` / `DocumentDataCount : int` / `DocumentUserTextCount : int` — the backslash in the key is the discriminant: `DocumentDataCount` counts keys containing `\`, `DocumentUserTextCount` counts keys without one, `Count` totals both.
- `Delete(string section, string entry)` / `Delete(string key)` — a null section deletes every section, a null entry deletes every entry in the section; `Document : RhinoDoc` re-resolves the owner.
- User strings ride INTERNAL `CommonObject` primitives (`_SetUserString`/`_GetUserString`/`_GetUserStrings`/`_DeleteAllUserStrings`/`_UserStringCount`); `CommonObject` exposes NO public user-string member, and the identical public roster re-declares per type on `ObjectAttributes`, `GeometryBase`, `Layer`, `Material`, `Group`, `Linetype`, `HatchPattern`, `DimensionStyle`, `PageViewGroup`, `InstanceDefinitionGeometry`, and `Rhino.Display.RhinoViewport` — the attribute and geometry stores on one object are INDEPENDENT.
- `ObjectAttributes.SetUserString(string, string) : bool` (null value removes the key) / `GetUserString(string) : string` (null when absent) / `GetUserStrings() : NameValueCollection` (independent copy) / `UserStringCount : int` / `DeleteUserString(string) : bool` / `DeleteAllUserStrings()` — `RhinoViewport.SetUserStrings(IEnumerable<KeyValuePair<string,string>>, bool)` is the one bulk-set overload.
- `Rhino.DocObjects.Tables.ObjectTable.FindByUserString(string key, string value, bool caseSensitive) : RhinoObject[]` with `(..., bool searchGeometry, bool searchAttributes, ObjectType filter)` and `(..., ObjectEnumeratorSettings filter)` overloads — null when none found; key and value admit `?`/`*` wildcards, and the two search flags address the two distinct stores.
- `Rhino.RhinoDoc.UserStringChangedArgs` — nested event-args type carrying `Document : RhinoDoc` + `Key : string` behind `RhinoDoc.UserStringChanged`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ArchivableDictionary` is the serialization primitive — a string-to-boxed-value store keyed by the private `ItemType` discriminant; the Rasm value union reconstructs that discriminant as a closed generated case set from the typed `Set` overloads, and the typed `Get` families and the boxed indexer read it back, so the write vocabulary is deliberately wider than the typed read.
- `PersistentSettings` is a per-plugin or per-app tree reached through `FromPlugInId`/`RhinoAppSettings`; each node layers an explicit value over a default over a validator, carries per-key type/read-only/hidden metadata, and tracks change against the persisted baseline; the child hierarchy nests through `GetChild`/`AddChild`.
- `UserData` is the attach-to-object custody base, `UserDictionary` carries exactly one `ArchivableDictionary`, and `UserDataList` is the per-object roster; document key/value text lives on `StringTable`, per-object strings on `ObjectAttributes`, never duplicated across the two.

[STACKING]:
- `LanguageExt.Core`(`libs/csharp/.api/api-languageext.md`): `TryGet*` out-bool lookups fold to `Fin<A>`/`Option<A>`, boxed indexer reads lift through a typed decode into `Option<A>`, and the throwing `AddContentsFrom`/`ReplaceContentsWith`/`IDictionary.Add` wrap to `Fin<Unit>` at the boundary so exception control flow never enters domain code.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the `ItemType` value roster reconstructs as a generated `[Union]` typed value with total `Set`/`Get` dispatch, and the settings value-kind set wraps as a keyed `SmartEnum` owning one typed writer and one typed reader per kind.
- `Rasm` kernel: geometry values (`Point3d`, `Plane`, `Transform`, `Interval`, `BoundingBox`) and drawing values compose the kernel numeric and color owners, never re-derived inside the custody layer.

[LOCAL_ADMISSION]:
- a dictionary value enters through `Set`; a settings value through a typed `Set<Kind>` writer or `SetDefault`; attached custody through a `UserData` subclass or `UserData.Move*`; settings custody through `FromPlugInId`/`RhinoAppSettings`.
- live `UserData`, `UserDictionary`, and `ObjectAttributes` values remain inside the document grant; downstream code receives detached `ArchivableDictionary` snapshots, decoded typed values, or projected receipts.

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: typed serialization values, per-plugin/app settings custody, attached custom-data, and document with per-object user text.
- Accept: typed value write and read, settings tree navigation with defaults and validators, user-data custody transfer, and user-text mutation projected onto `Fin`/`Option` rails.
- Reject: boxed values escaping without a typed decode, exception-style dictionary and settings outcomes, `internal` custody types, and live document-bound custody objects crossing the session boundary.
