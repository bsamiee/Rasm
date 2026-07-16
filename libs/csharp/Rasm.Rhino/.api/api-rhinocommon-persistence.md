# [RASM_RHINO_API_RHINOCOMMON_PERSISTENCE]

This catalog owns the typed-serialization and data-custody spine: `ArchivableDictionary` as the string-keyed serializable value store, `PersistentSettings` as the per-plugin/app settings tree, the `UserData` attach-to-object custody family, and document plus per-object user text. `SessionSource.Configured` (session.md) consumes `ArchivableDictionary` as the headless-open option payload, tables.md reads per-object user strings inside `TablePredicate.Tag` and owns table mutation, events.md owns `UserStringChanged` observation, and the document-scoped saved-state preset tables live in api-rhinocommon-document-state.md. Value writes carry a wider type vocabulary than typed reads, so every value that lacks a typed getter returns as a boxed object through the untyped indexer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon persistence surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino`, `Rhino.Collections`, `Rhino.DocObjects`, `Rhino.DocObjects.Custom`, `Rhino.DocObjects.Tables`
- kernel: `Rasm` (host-agnostic vocabularies and numeric owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: custody-boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serialization dictionary and settings tree
- rail: custody-boundary

| [INDEX] | [SYMBOL]               | [KIND] | [CAPABILITY]                                                               |
| :-----: | :--------------------- | :----- | :------------------------------------------------------------------------- |
|  [01]   | `ArchivableDictionary` | class  | string-keyed serializable value store; typed write and read over ~48 types |
|  [02]   | `PersistentSettings`   | class  | per-plugin/app settings tree with child nodes, defaults, and validators    |

[PUBLIC_TYPE_SCOPE]: attached custom-data and user text
- rail: custody-boundary

| [INDEX] | [SYMBOL]           | [KIND]         | [CAPABILITY]                                                        |
| :-----: | :----------------- | :------------- | :------------------------------------------------------------------ |
|  [01]   | `UserData`         | abstract class | attach-to-object custody base with serialize and duplicate seams    |
|  [02]   | `UserDictionary`   | class          | `UserData` carrying one attached `ArchivableDictionary`             |
|  [03]   | `UserDataList`     | class          | per-object user-data collection; add, find by type, purge           |
|  [04]   | `UnknownUserData`  | class          | opaque carrier for user data of unrecognized origin                 |
|  [05]   | `StringTable`      | sealed class   | document user text as flat keys and section/entry pairs             |
|  [06]   | `ObjectAttributes` | class          | per-object user-string slice; document/tables catalogs own the rest |

[PUBLIC_TYPE_SCOPE]: settings codec and event payloads
- rail: custody-boundary

| [INDEX] | [SYMBOL]                           | [KIND]         | [CAPABILITY]                                                                |
| :-----: | :--------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `PersistentSettingsConverter`      | static class   | string⇄list/dictionary/enum/double codec behind every settings value kind   |
|  [02]   | `PersistentSettingsEventArgs`      | abstract class | validator payload base; sole member `Cancel : bool` (default false)         |
|  [03]   | `PersistentSettingsEventArgs<T>`   | class          | `CurrentValue : T` (settable) + `NewValue : T`; the typed validator payload |
|  [04]   | `PersistentSettingsSavedEventArgs` | class          | `SavedByThisRhino : bool` + `PlugInSettings` + `CommandSettings(string)`    |

`PersistentSettingsConverter` members: `IsStringList(string) : bool` / `IsStringDictionary(string) : bool` / `TryParseStringList(string, out string[]) : bool` / `TryParseStringDictionary(string, out KeyValuePair<string,string>[]) : bool` / `ToString(string[])` / `ToString(KeyValuePair<string,string>[])` / `ToString(double)` / `TryParseDouble(string, out double) : bool` / `TryParseEnum(Type, string, out string) : bool` / `TryParseEnum(Type, string, out int) : bool`. Saved notification rides `Rhino.PlugIns.PlugIn.SettingsSaved : EventHandler<PersistentSettingsSavedEventArgs>`; no standalone saved-delegate type exists.

Private internals: `ArchivableDictionary.ItemType` (value-type discriminant) and `ArchivableDictionary.DictionaryItem` (item carrier) are `private`; the Rasm value union reconstructs the discriminant from the public `Set`/`Get` overload roster. `SharedUserDictionary` is `internal` and never crosses the SDK boundary.

## [03]-[ENTRYPOINTS]

[DICTIONARY_VALUE_WRITE]:
- `Rhino.Collections.ArchivableDictionary.Set(string key, T val) : bool` over primitives {`bool`, `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `float`, `double`, `Guid`, `string`} — upsert; returns false on null/empty key, null value, or undefined type.
- `ArchivableDictionary.Set(string key, T val) : bool` over drawing types {`Color`, `Point`, `PointF`, `Rectangle`, `RectangleF`, `Size`, `SizeF`, `Font`} — same upsert contract.
- `ArchivableDictionary.Set(string key, T val) : bool` over geometry values {`Interval`, `Point2d`, `Point3d`, `Point4d`, `Vector2d`, `Vector3d`, `BoundingBox`, `Ray3d`, `Transform`, `Plane`, `Line`, `Point3f`, `Vector3f`} — same upsert contract.
- `ArchivableDictionary.Set(string key, T val) : bool` over carriers {nested `ArchivableDictionary`, `MeshingParameters`, `GeometryBase`, `ObjRef`} — every `Set` stores the caller's reference VERBATIM (`m_items[key] = new DictionaryItem(it, val)`) and `TryGetValue` returns the stored reference, so reference payloads alias both ways; caller-side freezing rides `GeometryBase.Duplicate()` and the copy constructors `new MeshingParameters(MeshingParameters source)` / `new ObjRef(ObjRef other)`, because neither carrier implements `ICloneable`.
- `ArchivableDictionary.Set(string key, IEnumerable<T> val) : bool` over {`bool`, `byte`, `sbyte`, `short`, `int`, `float`, `double`, `Guid`, `string`, `ObjRef`, `GeometryBase`} — enumerables land as one array item; no `uint`/`long`/`ushort` enumerable overload exists. Exact public `Set` census is 48: 12 scalars, 11 enumerables, 8 drawing, 13 geometry, 4 carriers.
- `ArchivableDictionary.SetEnumValue<T>(T enumValue) : bool` / `SetEnumValue<T>(string key, T enumValue) : bool` — keyless overload derives the key from `typeof(T).Name`; `T : struct, IConvertible`; storage is the invariant-culture name string as an `ItemType.String` item, so an enum entry is `GetString`-readable and indistinguishable from a plain string.
- `ItemType.PlaneEquation` (38) has no public `Set` and no typed getter — a loaded archive can carry a `double[]` plane-equation entry reachable only through the boxed indexer. No `DateTime` value kind exists in either direction.

[DICTIONARY_VALUE_READ]:
- `ArchivableDictionary.TryGet<Kind>(string key, out T value) : bool` plus `Get<Kind>(string key) : T` and `Get<Kind>(string key, T defaultValue) : T` over {`String`, `Dictionary`, `Bytes`, `Bool`, `Float`, `Double`, `Integer`, `Point3f`, `Point3d`, `Vector3d`, `Guid`, `Plane`} — typed read vocabulary is narrower than write: `Color`, `Size`, `Rectangle`, `Point`, `Interval`, and `Transform` write typed but read back only through the boxed indexer.
- `Get<Kind>(string key)` throws `KeyNotFoundException` on a missing key and `NotSupportedException` on a type mismatch; the `Get<Kind>(key, defaultValue)` sibling swallows both and returns the fallback — opposite failure contracts on one name.
- `ArchivableDictionary.GetEnumValue<T>() : T` / `GetEnumValue<T>(string key) : T` / `TryGetEnumValue<T>(string key, out T) : bool` / `RemoveEnumValue<T>() : bool` — `T : struct, IConvertible`; reads `Enum.Parse(..., ignoreCase: true)` over the stored name string, `GetEnumValue<T>()` throws `KeyNotFoundException`/`FormatException`, all throw `ArgumentException` for a non-enum `T`, and no `GetEnumValue<T>(key, default)` overload exists (PersistentSettings-only).
- `ArchivableDictionary[string key] : object` — the getter throws `KeyNotFoundException` on a missing key; the setter accepts only {`int`, `long`, `bool`, `double`, `string`, `GeometryBase`, `IEnumerable<GeometryBase>`} and throws `NotSupportedException` otherwise; `TryGetValue(string key, out object value) : bool` is the non-throwing boxed read.
- `ArchivableDictionary.ContainsKey(string key) : bool` / `Keys : string[]` / `Values : object[]` / `Count : int` — membership and enumeration surface.
- `ArchivableDictionary.Getint(string key, int defaultValue) : int` — legacy lowercase alias of `GetInteger`; the canonical name is `GetInteger`.

[DICTIONARY_LIFECYCLE]:
- `ArchivableDictionary.AddContentsFrom(ArchivableDictionary source) : bool` — reflects `GetMethod("Set", {string, value.GetType()})` with exact-type match, no assignability: every array entry (runtime `int[]` versus parameter `IEnumerable<int>`) and every `GeometryBase` subtype or `ObjRef` entry finds no overload and throws `ArgumentException`; only scalar entries whose runtime type is exactly a `Set` parameter type survive. Throws `ArgumentNullException` on null source.
- `ArchivableDictionary.ReplaceContentsWith(ArchivableDictionary source) : bool` — clears, then `AddContentsFrom`; same throw surface.
- `ArchivableDictionary.Clone() : ArchivableDictionary` — preserves `Version`, `Name`, and `ChangeSerialNumber`; a value clones only when it implements `ICloneable` (nested `ArchivableDictionary` and arrays clone; `GeometryBase`, `MeshingParameters`, and `ObjRef` values share by reference), so the clone is not an unconditional deep copy and no public `CloneValue`/`DeepCloneValue` exists.
- `ArchivableDictionary.Remove(string key) : bool` / `Clear() : void` — `IDictionary<string,object>.Add(key,value)` always throws `NotSupportedException`, so mutation enters only through `Set`.
- `ArchivableDictionary.Version : int` (get/set) / `Name : string` (get/set) / `ChangeSerialNumber : uint` — the serial increments only on `Set`, never on remove or clear.
- `ArchivableDictionary.ParentUserData : UserData` — the owning user data, null when detached.
- `ArchivableDictionary(int version, string name)` / `ArchivableDictionary(int version)` / `ArchivableDictionary(UserData parentUserData)` / `ArchivableDictionary()` — construction variants; the `int version` overloads stamp the archive schema version.
- `[Serializable] : ISerializable` — `protected ArchivableDictionary(SerializationInfo, StreamingContext)` + `GetObjectData` round-trip the dictionary as a native ON buffer under `"version"`/`"archive3dm"`/`"opennurbs"`/`"data"` entries.

[SETTINGS_TREE]:
- `Rhino.PersistentSettings.FromPlugInId(Guid pluginId) : PersistentSettings` — the plugin settings root, throwing `InvalidEnumArgumentException` on `Guid.Empty`; `PersistentSettings.RhinoAppSettings : PersistentSettings` — a static property lazily resolving `FromPlugInId(RhinoApp.CurrentRhinoId)`, not a type.
- `PersistentSettings.GetChild(string key) : PersistentSettings` / `TryGetChild(string key, out PersistentSettings) : bool` / `AddChild(string key) : PersistentSettings` / `DeleteChild(string key) : void` — the child-settings hierarchy.
- `PersistentSettings.Keys : ICollection<string>` / `ChildKeys : ICollection<string>` — value keys and child-node keys of the current node.
- `PersistentSettings.HiddenFromUserInterface : bool` (get/set) — the whole-node visibility flag, hiding every value on this node from the options UI, distinct from the per-key `HideSettingFromUserInterface`.

[SETTINGS_TYPED_READ]:
- `PersistentSettings.TryGet<Kind>(string key, out T value) : bool` plus `Get<Kind>(string key) : T` and `Get<Kind>(string key, T defaultValue) : T` over {`Guid`, `Bool`, `Byte`, `Integer`, `UnsignedInteger`, `Double`, `Char`, `String`, `StringList`(`string[]`), `StringDictionary`(`KeyValuePair<string,string>[]`), `Date`(`DateTime`), `Color`, `Color?`, `Point`(`System.Drawing.Point`), `Point3d`, `Size`, `Rectangle`} — each read overload has an `IEnumerable<string> legacyKeyList` sibling that falls back across renamed keys.
- `PersistentSettings.GetInteger(string key, int defaultValue, int bound, bool boundIsLower) : int` / `GetInteger(string key, int defaultValue, int lowerBound, int upperBound) : int` — clamp the stored integer to a one-sided or two-sided bound; `int` is the only kind carrying clamp overloads.
- `PersistentSettings.GetEnumValue<T>(string key) : T` / `GetEnumValue<T>(string key, T defaultValue) : T` / `GetEnumValue<T>(T defaultValue) : T` / `TryGetEnumValue<T>(string key, out T) : bool` — `T : struct, IConvertible`; `TryGetEnumValue` always assigns `enumValue` (`default(T)` on failure), so only the returned bool proves presence.
- Every defaulted `Get<Kind>(key, defaultValue)` read MUTATES the tree: on hit it stamps the default layer, on miss it registers the default AND materializes the key set to it, marking the node changed — a defaulted read is never a pure lookup.
- `Get<Kind>(string key)` (no-default) throws `KeyNotFoundException` on a missing key and `NotSupportedException` on a type mismatch.

[SETTINGS_TYPED_WRITE]:
- `PersistentSettings.Set<Kind>(string key, T value) : void` as the named methods `SetGuid`/`SetBool`/`SetByte`/`SetInteger`/`SetUnsignedInteger`/`SetDouble`/`SetChar`/`SetString`/`SetStringList`/`SetStringDictionary`/`SetDate`/`SetColor`(over `Color` and `Color?`)/`SetPoint3d`/`SetRectangle`/`SetSize`/`SetPoint`/`SetEnumValue<T>` — one typed writer per value kind.
- `PersistentSettings.DeleteItem(string key) : void` — removes one stored value.

[SETTINGS_DEFAULTS]:
- `PersistentSettings.SetDefault(string key, T value) : void` over 16 kinds {`bool`, `byte`, `int`, `double`, `char`, `string`, `string[]`, `KeyValuePair<string,string>[]`, `DateTime`, `Color`, `Color?`, `Rectangle`, `Size`, `Point`, `Point3d`, `Guid`} — the fallback layer read when no explicit value is stored; no `SetDefault(uint)` exists despite `SetUnsignedInteger`.
- `PersistentSettings.TryGetDefault(string key, out T value) : bool` over 12 kinds {`bool`, `byte`, `int`, `double`, `char`, `string`, `string[]`, `DateTime`, `Color`, `Point3d`, `Size`, `Rectangle`} — reads the default layer without the resolved value; Guid, Point, `Color?`, StringDictionary, and uint have no default probe.
- Each `SettingValue` holds one explicit value beside one typed default per key, so the two layers resolve independently.

[SETTINGS_METADATA]:
- `PersistentSettings.GetSettingType(string key) : Type` / `TryGetSettingType(string key, out Type) : bool` — the stored value's runtime type.
- `PersistentSettings.GetSettingIsReadOnly(string key) : bool` / `TryGetSettingIsReadOnly(string key, out bool) : bool` — the read-only flag.
- `PersistentSettings.HideSettingFromUserInterface(string key) : void` / `GetSettingIsHiddenFromUserInterface(string key) : bool` / `TryGetSettingIsHiddenFromUserInterface(string key, out bool) : bool` — visibility flag; both readers carry a `legacyKeyList` sibling.
- `PersistentSettings.RegisterSettingsValidator<T>(string key, EventHandler<PersistentSettingsEventArgs<T>> validator) : void` / `GetValidator<T>(string key) : EventHandler<PersistentSettingsEventArgs<T>>` — per-key typed validator seam; registration OVERWRITES an existing validator unconditionally, `GetValidator<T>` returns null when none is installed and throws `InvalidCastException` when `T` mismatches the registered specialization, and a registered validator fires on every typed set AND every default stamp, its args carrying settable `Cancel` and `CurrentValue`.
- `PersistentSettings.ContainsChangedValues() : bool` / `ClearChangedFlag() : void` / `ContainsModifiedValues(PersistentSettings allUserSettings) : bool` — change tracking against the persisted baseline; no public per-change event exists — change notification is the validator handler plus `PlugIn.SettingsSaved`.
- `PersistentSettings.StringListRootKey : string` — a sentinel list ELEMENT: placed inside a `SetStringList` array it splices the all-users ProgramData list at that position on `GetStringList`.
- `[Serializable] : ISerializable` — the protected deserialization constructor has an empty body (a hollow instance), and `GetObjectData` delegates to the live tree.

[USERDATA_CUSTODY]:
- `Rhino.DocObjects.Custom.UserData.Description : string` (virtual, default literal `"RhinoCommon UserData"`) / `ShouldWrite : bool` (virtual, default false) — serialization identity and opt-in; the protected `Write`/`Read` defaults return false, so a `ShouldWrite => true` override without both IO overrides serializes as a silent no-op.
- `UserData.Transform : Transform` — the accumulated object transform, read from native per call and set through the protected setter; an `OnTransform` override that skips the base call leaves it stale.
- `UserData.Dispose() : void` — releases the native handle; the finalizer mirrors it.
- `UserData.Copy(CommonObject source, CommonObject destination) : void` (static) / `MoveUserDataFrom(CommonObject objectWithUserData) : Guid` (static, `Guid.Empty` when nothing moved) / `MoveUserDataTo(CommonObject objectToGetUserData, Guid id, bool append) : void` (static, no-op on `Guid.Empty`) — cross-object custody transfer.
- `UserData.Write(BinaryArchiveWriter) : bool` / `Read(BinaryArchiveReader) : bool` / `OnTransform(Transform) : void` / `OnDuplicate(UserData source) : void` / `Dispose(bool disposing) : void` — the protected override seams a derived class implements.
- `Rhino.DocObjects.Custom.ClassIdAttribute` — `ctor(string)` + `Id : Guid`; binds a legacy class id onto a `UserData` subclass so pre-rename archives keep resolving it.
- `UserDictionary : UserData` — `Dictionary : ArchivableDictionary` lazily parented to the user data; overrides: `Description` is `"RhinoCommon UserDictionary"`, `ShouldWrite` returns true only when `Dictionary.Count > 0`, `Read` is `archive.ReadDictionary()` and `Write` is `archive.WriteDictionary(Dictionary)` — the concrete proof the custody spine rides the archive dictionary pair.
- `Rhino.Runtime.CommonObject.UserData : UserDataList` — lazy list accessor allocating the roster on first read; `CommonObject.UserDictionary : ArchivableDictionary` — lazy MUTATING accessor: finds the internal `SharedUserDictionary`, constructs and attaches one through `UserData.Add` when absent, and returns null when that `Add` fails.
- `UserDataList : IEnumerable<UserData>` — `Add(UserData) : bool` throws `ArgumentException` unless the type is a public class with a parameterless constructor (the internal `SharedUserDictionary` bypasses that gate, and `Add` detaches an entry from a prior roster before attaching) / `Remove(UserData) : bool` detaches WITHOUT disposing / `Find(Type userdataType) : UserData` / `Contains(Guid userdataId) : bool` / `Purge() : void` / `this[int index] : UserData` / `Count : int` / `GetEnumerator() : IEnumerator<UserData>` — the per-object user-data roster.
- `UnknownUserData(nint pointerNativeUserData)` — the sole public member; carries opaque native user data across a round-trip without decoding it.

[ARCHIVE_IO]:
- `Rhino.FileIO.BinaryArchiveWriter.WriteDictionary(ArchivableDictionary) : void` / `BinaryArchiveReader.ReadDictionary() : ArchivableDictionary` — the custody-spine pair every `UserData.Write`/`Read` body composes.
- `BinaryArchiveWriter.Write3dmChunkVersion(int major, int minor) : void` / `BinaryArchiveReader.Read3dmChunkVersion(out int major, out int minor) : void` — payload schema framing; `BeginWrite3dmChunk`/`EndWrite3dmChunk` pair with `BeginRead3dmChunk`/`EndRead3dmChunk(bool)` and `BeginReadDictionaryEntry(out int, out string)` for entry-level walks.
- `BinaryArchiveWriter.WriteErrorOccured : bool` / `BinaryArchiveReader.ReadErrorOccured : bool` / `EnableCRCCalculation(bool)` / `Writer.WriteEmptyCheckSum` / `Reader.ReadCheckSum` / `Archive3dmVersion : int` — archive error and integrity state read after each IO leg.
- `BinaryArchiveReader.AtEnd : bool` / `CurrentPosition : ulong` / `SeekFromStart(ulong)` / `SeekFromCurrentPosition` — reader positioning.
- Typed `Write*`/`Read*` pairs: Bool, Byte, SByte, Short, UShort, Int, UInt, Int64, Single, Double, String, Utf8String, Guid, Color, Font, the `System.Drawing` Point/PointF/Rectangle/RectangleF/Size/SizeF set, Point2d/Point3d/Point3f/Point4d, Vector2d/Vector3d/Vector3f, Interval, Line, Plane, Ray3d, BoundingBox, Transform, `Geometry(GeometryBase)`, MeshingParameters, ObjRef, RenderSettings, the `*Array` variants (Bool/Byte/SByte/Short/Single/Double/Int/Guid/String/Geometry/ObjRef), and `Write/ReadCompressedBuffer`.

[USER_TEXT]:
- `Rhino.DocObjects.Tables.StringTable.SetString(string section, string entry, string value) : string` / `SetString(string key, string value) : string` — writes and returns the prior value captured through `GetValue` before the write (a string, empty when absent — never a guaranteed null); a null value deletes the entry, and the section overload forms the key `section\entry`.
- `StringTable.GetValue(string section, string entry) : string` / `GetValue(string key) : string` / `GetValue(int i) : string` / `GetKey(int i) : string` — section/entry, flat-key, and positional reads; the section/entry read returns `string.Empty` on an empty key.
- `StringTable.GetSectionNames() : string[]` / `GetEntryNames(string section) : string[]` — the section and entry rosters, sorted and never null.
- `StringTable.Count : int` / `DocumentDataCount : int` / `DocumentUserTextCount : int` — the backslash in the key is the discriminant: `DocumentDataCount` counts keys containing `\` (section\entry), `DocumentUserTextCount` counts keys without one, `Count` totals both.
- `StringTable.Delete(string section, string entry) : void` / `Delete(string key) : void` — a null section deletes every section, a null entry deletes every entry in the section; `Document : RhinoDoc` re-resolves the owner.
- User strings ride INTERNAL `Rhino.Runtime.CommonObject` primitives (`_SetUserString`/`_GetUserString`/`_GetUserStrings`/`_DeleteAllUserStrings`/`_UserStringCount`); `CommonObject` itself exposes NO public user-string member, and the identical public roster is re-declared per type on `ObjectAttributes`, `GeometryBase`, `Layer`, `Material`, `Group`, `Linetype`, `HatchPattern`, `DimensionStyle`, `PageViewGroup`, `InstanceDefinitionGeometry`, and `Rhino.Display.RhinoViewport` — and the attribute and geometry stores on one object are INDEPENDENT.
- `Rhino.DocObjects.ObjectAttributes.SetUserString(string key, string value) : bool` (null value removes the key) / `GetUserString(string key) : string` (null when absent) / `GetUserStrings() : NameValueCollection` (independent copy) / `UserStringCount : int` / `DeleteUserString(string key) : bool` / `DeleteAllUserStrings() : void` — `UserStringChanged` (events.md) observes writes, and `TablePredicate.Tag` (tables.md) reads them; `RhinoViewport.SetUserStrings(IEnumerable<KeyValuePair<string,string>>, bool) : void` is the one bulk-set overload.
- `Rhino.DocObjects.Tables.ObjectTable.FindByUserString(string key, string value, bool caseSensitive) : RhinoObject[]` plus the `(..., bool searchGeometry, bool searchAttributes, ObjectType filter)` and `(..., ObjectEnumeratorSettings filter)` overloads — null when none found; key and value admit `?`/`*` wildcards, and the two search flags address the two distinct stores.
- `Rhino.RhinoDoc.UserStringChangedArgs` — the nested event-args type carrying `Document : RhinoDoc` + `Key : string` behind `RhinoDoc.UserStringChanged`.

## [04]-[IMPLEMENTATION_LAW]

[PERSISTENCE_TOPOLOGY]:
- `ArchivableDictionary` is the serialization primitive — a string-to-boxed-value store whose element kind is the private `ItemType` discriminant; the Rasm value union reconstructs that discriminant as a closed generated case set from the ~48 typed `Set` overloads, and the 12 typed `Get` families plus the boxed indexer read it back, so the write vocabulary is deliberately wider than the typed-read vocabulary.
- `PersistentSettings` is a per-plugin or per-app tree reached through `FromPlugInId` and `RhinoAppSettings`; each node layers an explicit value over a default over a validator, carries per-key type/read-only/hidden metadata, and tracks change against the persisted baseline; the child hierarchy nests through `GetChild`/`AddChild`.
- `UserData` is the attach-to-object custody base, `UserDictionary` carries exactly one `ArchivableDictionary`, and `UserDataList` is the per-object roster; document key/value text lives on `StringTable`, and per-object strings live on `ObjectAttributes`, never duplicated across the two.

[STACKING]:
- `LanguageExt.Core`(`libs/csharp/.api/api-languageext.md`): `TryGet*` out-bool lookups fold to `Fin<A>`/`Option<A>`, boxed indexer reads lift through a typed decode into `Option<A>`, and the throwing `AddContentsFrom`/`ReplaceContentsWith`/`IDictionary.Add` wrap to `Fin<Unit>` at the boundary so exception control flow never enters domain code.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the `ItemType` value roster reconstructs as a generated `[Union]` typed value with total `Set`/`Get` dispatch, and the settings value-kind set wraps as a keyed `SmartEnum` that owns one typed writer and one typed reader per kind.
- `Rasm` kernel: geometry values (`Point3d`, `Plane`, `Transform`, `Interval`, `BoundingBox`) and drawing values compose the kernel numeric and color owners, never re-derived inside the custody layer.

[LOCAL_ADMISSION]:
- a dictionary value enters through `Set`; a settings value enters through a typed `Set<Kind>` writer or `SetDefault`; attached custody enters through a `UserData` subclass or `UserData.Move*`, and settings custody enters through `FromPlugInId`/`RhinoAppSettings`.
- live `UserData`, `UserDictionary`, and `ObjectAttributes` values remain inside the document grant; downstream code receives detached `ArchivableDictionary` snapshots, decoded typed values, or projected receipts.

[RAIL_LAW]:
- Surface: `Rhino.Collections` dictionary + `Rhino` settings + `Rhino.DocObjects.Custom` user data + `Rhino.DocObjects.Tables.StringTable` + `ObjectAttributes` user strings
- Owns: typed serialization values, per-plugin/app settings custody, attached custom-data, and document plus per-object user text.
- Accept: typed value write and read, settings tree navigation with defaults and validators, user-data custody transfer, and user-text mutation projected onto `Fin`/`Option` rails.
- Reject: boxed values escaping without a typed decode, exception-style dictionary and settings outcomes, `internal` custody types, and live document-bound custody objects crossing the session boundary.
