# [RASM_RHINO_API_ETO_PLATFORM]

The Rhino host resolves one `Eto` platform for its loaded `Eto.dll`, and this boundary binds it rather than seating it. What this partition owns is the theme-transition seam beneath that root: `Widget.Style` selects a named handler-level style, `Control.TriggerStyleChanged` re-applies it, `Control.AttachNative`/`DetachNative` move an Eto control under an external native parent, and `Rhino.UI.EtoExtensions.Get` is the host notifier that fires when Rhino flips light or dark.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto` theme-transition and native-parent seam
- package: `Eto` (host-provided; bound in-place from the Rhino-loaded `Eto.dll`, never a second NuGet admission) (BSD-3-Clause)
- assembly: `Eto.dll` (Rhino `RhCore` framework); the theme notifier is `Rhino.UI.dll`
- namespace: `Eto.Forms`, `Rhino.UI`
- asset: one platform handler the Rhino process resolves for its loaded `Eto.dll`
- rail: platform-handler

## [02]-[PUBLIC_TYPES]

- Registers the `Eto` platform-handler root (`libs/csharp/.api/api-eto-platform.md`): `Platform` identity and capability discovery, the handler-registration map with `Create`/`CreateShared`/`Find`/`Add`, the `HandlerCreated`/`WidgetCreated` mint events, the boot, context, and marshal surfaces, the `WidgetHandler` family, the `Style` registry, and `NativeControlHost`/`CreateNativeControlArgs`/`IControlObjectSource` carry their algebra there; the rows below are the theme-transition and native-parent seam this boundary adds beyond it.

[PUBLIC_TYPE_SCOPE]: theme-transition and native-parent seam

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :------------------------------ | :------------ | :--------------------------------------------------- |
|  [01]   | `Widget.Style`                  | property      | the style key applied to a widget                    |
|  [02]   | `Control.TriggerStyleChanged()` | member        | re-applies style handlers on a theme change          |
|  [03]   | `Control.AttachNative()`        | member        | attaches the control under an external native parent |
|  [04]   | `Control.DetachNative()`        | member        | detaches from the native parent                      |
|  [05]   | `Rhino.UI.EtoExtensions.Get`    | seam          | Rhino style and theme notifier for an Eto control    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: theme-transition notification and native-parent lifecycle

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `Rhino.UI.EtoExtensions.Get(Control) -> object` | static   | resolve a control's Rhino theme notifier     |
|  [02]   | `Control.TriggerStyleChanged()`                 | instance | re-apply style handlers on a host theme flip |
|  [03]   | `Control.AttachNative()`                        | instance | attach to an external native parent          |
|  [04]   | `Control.DetachNative()`                        | instance | detach from the native parent                |
|  [05]   | `Widget.Style { get; set; }`                    | property | select the registered style key              |

- The notifier fires on host light and dark transitions, so a docked control re-styles on the edge rather than polling appearance state.
- `AttachNative`/`DetachNative` are the seam a Rhino-hosted panel uses to dock Eto content under a host-owned native parent, and they pair — a detach without its attach leaks the parent link.

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_TOPOLOGY]:
- This boundary binds the platform the Rhino process already resolved and never calls the registered initialize surface against the host thread; a worker thread touching Eto scopes the platform through the registered thread-start surface.
- Restyling is three steps in one direction: a named style registers on the branch registry, `Widget.Style` selects it, and `TriggerStyleChanged` re-applies it — a control never subclasses to change appearance.
- The theme notifier is the edge, not a state read: a re-style runs on the notification and never on a polled appearance query.

[STACKING]:
- `api-eto-platform`(`../../.api/api-eto-platform.md`): the registered handler root this seam sits beneath — identity, capability gating, handler resolution, the style registry, and native hosting all read there, and this boundary re-tables none of it.
- `api-rhino-ui`(`api-rhino-ui.md`): `EtoExtensions` native styling and the Rhino native windowing surface dock and style the Eto content the handler places, and the host bridge owns the document-owned window this seam attaches into.
- `api-eto-forms`(`api-eto-forms.md`): the control tree the style key applies to and the `Themed*Handler` backend classes that register through the branch registry.
- `api-macos-native`(`api-macos-native.md`): on the macOS backend the registered native host bridges to an AppKit view, where native pacing composes rather than in this seam.
- `LanguageExt.Core`(`../../.api/api-languageext.md`): `Eff<A>` scopes the attach and detach pair for deterministic release, and the theme notifier feeds a `Fin<A>`-railed re-style.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): the style key binds as a `[ValueObject<string>]` so a registered style is selected by a validated owner rather than a hand-spelled literal.

[LOCAL_ADMISSION]:
- Feature discovery, handler resolution, and native hosting take the registered branch surface; this boundary composes them and re-mints none.
- The theme seam stays behind the Rasm.Rhino UI owner; `Eto.*` platform types never leak past it.

[RAIL_LAW]:
- Partition: `Eto` Rhino host boundary — the theme-transition seam and the external-native-parent attach and detach pair
- Owns: style-key selection, style re-application on a host theme flip, the Rhino theme notifier, and the native-parent lifecycle
- Accept: host theme-change notification, named-style re-application, docking an Eto control under a host-owned native parent
- Reject: a re-tabling of the branch handler root, widget construction and layout (`api-eto-forms.md`), immediate 2D painting (`api-eto-drawing.md`), Rhino document windowing and panel registration (`api-rhino-ui.md`), macOS native pacing (`api-macos-native.md`), a control subclass where a style delegate carries the change, and re-initializing the ambient platform the host owns
