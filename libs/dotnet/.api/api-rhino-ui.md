# [RASM_API_RHINO_UI]

`Rhino.UI` carries the two host-UI bridges every Rhino-hosted boundary crosses: the `EtoExtensions` bridge that binds an Eto window to a `RhinoDoc`, stamps native chrome onto a control, presents a semi-modal dialog, and persists a window's screen slot keyed by a caller `Type`; and the `Dialogs` native value-prompt fast lane that settles a string or a number without a full Eto dialog. The namespace spans two host assemblies. This branch catalogue owns the shared bridges; each host-boundary folder registers it and tables only the subsystem its own boundary reaches.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the shared host-bridge statics

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :-------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `EtoExtensions` | static class  | native styling, document window binding, and position persistence |
|  [02]   | `Dialogs`       | static class  | Rhino-native value prompts                                        |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `EtoExtensions` — native styling and document window binding

Members are declared static on `EtoExtensions` and spell as extensions on the receiver; a boundary may write either form and both resolve to the same member.

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------ | :------ | :------------------------------ |
|  [01]   | `EtoExtensions.UseRhinoStyle(Control)`                              | static  | apply Rhino chrome to a control |
|  [02]   | `EtoExtensions.Show(Form, RhinoDoc)`                                | static  | show a form bound to a document |
|  [03]   | `EtoExtensions.GetRhinoDoc(Form) -> RhinoDoc`                       | static  | resolve the owning document     |
|  [04]   | `EtoExtensions.ShowSemiModal<T>(Dialog<T>, RhinoDoc, Control) -> T` | static  | typed semi-modal dialog         |
|  [05]   | `EtoExtensions.ShowSemiModal(Dialog, RhinoDoc, Control)`            | static  | untyped semi-modal dialog       |
|  [06]   | `EtoExtensions.WindowsFromDocument<T>(RhinoDoc) -> IEnumerable<T>`  | static  | document-scoped window roster   |
|  [07]   | `EtoExtensions.SavePosition(Window, Type)`                          | static  | persist the screen position     |
|  [08]   | `EtoExtensions.RestorePosition(Window, Type) -> bool`               | static  | restore the screen position     |
|  [09]   | `EtoExtensions.LocalizeAndRestore(Window, Type)`                    | static  | localize and restore the layout |

- Position persistence keys on a caller `Type`, so a window restores its own screen slot across sessions and two windows never contend for one slot.
- `Show` pairs the form with a `RhinoDoc` and `GetRhinoDoc` resolves it back; the association is the bridge's, never a boundary-side map beside it.

[ENTRYPOINT_SCOPE]: `Dialogs` — native value prompts

Each prompt settles a value through the Rhino-native fast lane, the accepted-versus-dismissed verdict on the `bool` return and the value on an `out`/`ref` channel.

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]            |
| :-----: | :-------------------------------------------------------------------------- | :------ | :---------------------- |
|  [01]   | `Dialogs.ShowEditBox(string, string, string, bool, out string) -> bool`     | static  | native edit-text prompt |
|  [02]   | `Dialogs.ShowNumberBox(string, string, ref double) -> bool`                 | static  | unbounded number prompt |
|  [03]   | `Dialogs.ShowNumberBox(string, string, ref double, double, double) -> bool` | static  | bounded number prompt   |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every Eto surface reaches a Rhino window through this bridge alone: `UseRhinoStyle` stamps native chrome in place, `Show`/`ShowSemiModal` present against a document, and the control tree itself is authored through the Eto catalogues and never re-implemented here.
- The value prompts and a full Eto dialog are two tiers of one decision: a single string or number settles on the native fast lane, and anything carrying layout, validation, or more than one field is an Eto dialog presented through `ShowSemiModal`.

[STACKING]:
- `api-eto-forms`(`.api/api-eto-forms.md`): `Form`, `Dialog`, `Dialog<T>`, `Window`, and `Control` are the carriers every member here receives; this bridge supplies the document ownership, native styling, and semi-modal presentation the construction surface lacks.
- `api-rhinocommon`(`.api/api-rhinocommon.md`) and the boundary `RhinoDoc` handle: the document handle a window binds to is the host's own, and a boundary reaches it through its own active-model owner rather than a second lookup here.
- `LanguageExt.Core`(`.api/api-languageext.md`): `UseRhinoStyle` and `Show` lower onto side-effecting `Eff` calls, `GetRhinoDoc` null-gates through `Optional(...)` into `Option<RhinoDoc>`, a `WindowsFromDocument<T>` roster carries as `Seq<T>`, `RestorePosition`'s `bool` folds to `Fin<Unit>`, and a prompt's `bool` return with its `out`/`ref` channel lifts to `Fin<string>`/`Fin<double>` where `false` maps to an invalid-result fault.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): the caller `Type` a position keys on binds as a `[ValueObject]` slot identity so two windows never collide on a hand-spelled key.

[LOCAL_ADMISSION]:
- Native styling enters through `UseRhinoStyle`; a hand-rolled Rhino-chrome stamp is the deleted form.
- Form-to-document binding is `Show`/`GetRhinoDoc`, and a boundary owner composes these rather than re-deriving the association.
- A single-value input takes `ShowEditBox`/`ShowNumberBox`; a hand-built one-field Eto dialog beside them is the deleted form.
