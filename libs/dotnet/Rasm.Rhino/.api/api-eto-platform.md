# [RASM_RHINO_API_ETO_PLATFORM]

The Rhino host resolves one `Eto` platform for its loaded `Eto.dll`, and this boundary binds it rather than seating it. What this partition owns is the theme-transition seam beneath that root: `Widget.Style` selects a named handler-level style, `Control.TriggerStyleChanged` re-applies it, `Control.AttachNative`/`DetachNative` move an Eto control under an external native parent, and `Rhino.UI.EtoExtensions.UseRhinoStyle` applies the Rhino style the host re-applies on a light/dark flip.

## [01]-[BOUNDARY_REACH]

- Registers the `Eto` platform-handler root (`libs/dotnet/.api/api-eto-platform.md`): `Platform` identity and capability discovery, the handler-registration map with `Create`/`CreateShared`/`Find`/`Add`, the `HandlerCreated`/`WidgetCreated` mint events, the boot, context, and marshal surfaces, the `WidgetHandler` family, the `Style` registry, and `NativeControlHost`/`CreateNativeControlArgs`/`IControlObjectSource` carry their algebra there. This boundary binds the platform the Rhino process already resolved, adds no carrier of its own, and states the theme-transition and native-parent law over the registered surface.

| [INDEX] | [BOUNDARY_CONCERN]        | [REGISTERED_MEMBERS]                                               |
| :-----: | :------------------------ | :----------------------------------------------------------------- |
|  [01]   | style-key selection       | `Widget.Style { get; set; }` keyed into the registered `Style` map |
|  [02]   | theme-flip re-application | `Control.TriggerStyleChanged()`                                    |
|  [03]   | host style application    | `Rhino.UI.EtoExtensions.UseRhinoStyle(Control)`                    |
|  [04]   | native-parent attach      | `Control.AttachNative()`                                           |
|  [05]   | native-parent release     | `Control.DetachNative()`                                           |

## [02]-[IMPLEMENTATION_LAW]

[PLATFORM_TOPOLOGY]:
- This boundary binds the platform the Rhino process already resolved and never calls the registered initialize surface against the host thread; a worker thread touching Eto scopes the platform through the registered thread-start surface.
- Restyling is three steps in one direction: a named style registers on the branch registry, `Widget.Style` selects it, and `TriggerStyleChanged` re-applies it — a control never subclasses to change appearance.
- Rhino styling enters through `UseRhinoStyle`, which resolves the host style service and re-applies on a light/dark flip inside `Rhino.UI`; the notifier driving that re-application is a private nested type with no public accessor, so a boundary needing its own re-style calls `TriggerStyleChanged` rather than reaching for a host notification handle.
- `AttachNative`/`DetachNative` are the seam a Rhino-hosted panel uses to dock Eto content under a host-owned native parent, and they pair — a detach without its attach leaks the parent link.

[STACKING]:
- `libs/dotnet/.api/api-eto-platform.md`: the registered handler root this seam sits beneath — identity, capability gating, handler resolution, the style registry, and native hosting all read there, and this boundary re-tables none of it.
- `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md`: `EtoExtensions` native styling and the Rhino native windowing surface dock and style the Eto content the handler places, and the host bridge owns the document-owned window this seam attaches into.
- `libs/dotnet/Rasm.Rhino/.api/api-eto-forms.md`: the control tree the style key applies to and the `Themed*Handler` backend classes that register through the branch registry.
- `libs/dotnet/Rasm.Rhino/.api/api-macos-native.md`: on the macOS backend the registered native host bridges to an AppKit view, where native pacing composes rather than in this seam.
- `LanguageExt.Core`(`libs/dotnet/.api/api-languageext.md`): `Eff<A>` scopes the attach and detach pair for deterministic release, and a host theme flip feeds a `Fin<A>`-railed re-style.
- `Thinktecture.Runtime.Extensions`(`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`): the style key binds as a `[ValueObject<string>]` so a registered style is selected by a validated owner rather than a hand-spelled literal.

[LOCAL_ADMISSION]:
- Feature discovery, handler resolution, and native hosting take the registered branch surface; this boundary composes them and re-mints none.
- The theme seam stays behind the Rasm.Rhino UI owner; `Eto.*` platform types never leak past it.
