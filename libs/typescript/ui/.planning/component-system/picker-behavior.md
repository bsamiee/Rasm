# [UI_PICKER_BEHAVIOR]

The headless perceptual-picker family realizing the `component-system/role-behavior.md#ROLE_BEHAVIOR` `pickers` row's four named-but-empty behaviors as ONE closed `PickerBehavior` `Data.TaggedEnum` — `color`, `date`, `file-preview`, `file-upload` — under one total generated `$match` dispatch so a new picker breaks every site at compile time. The `color` arm projects the picked channel through the `react-stately` `parseColor`/`getColorChannels` surface into the `theming/theme-tokens.md#THEME_TOKENS` OKLCH `colorjs.io` space so a pick is a perceptual `oklch` token and `contrastWith` runs on the pick, never an sRGB hex (the live `useColorPickerState` cell is the consumer's stateful component); the `date` arm selects the `react-aria-components` `RangeCalendar` or `Calendar` widget by `mode`; the `file-preview` and `file-upload` arms select the `react-aria-components` `FileTrigger` ingest, the two role rows kept distinct rather than collapsed to one file arm. Only the headless behavior is owned — the per-component `.tsx` markup stays the consumer's — and the picked value reads and writes through `binding/atom-binding.md#ATOM_BINDING`. The page composes the `role-behavior.md#ROLE_BEHAVIOR` `pickers` `RoleBehavior<Props, State>` contract by reference and holds no domain state.

## [1]-[INDEX]

One cluster: `PICKER_BEHAVIOR` owns the closed `PickerBehavior` family and the total `applyPicker` dispatch over its four arms.

## [2]-[PICKER_BEHAVIOR]

- Owner: `PickerBehavior`, the one headless `Data.TaggedEnum` over the four `_Roles.pickers` behaviors — `color`/`date`/`filePreview`/`fileUpload` — and `applyPicker`, the total generated `$match` fold mapping each arm to its `react-stately`/`react-aria-components` headless surface. `colorPick` is the `react-stately` `parseColor`/`getColorChannels` channel-projection fold lifting the picked value into the `theme-tokens.md#THEME_TOKENS` OKLCH space (the live `useColorPickerState` `color`/`setColor` cell is the consumer's stateful component, read by reference); the `date` arm selects the `react-aria-components` `RangeCalendar` or `Calendar` widget keyed by `mode`; `filePreview` and `fileUpload` are the `FileTrigger` ingest arms (IFC/GLB), the file-preview-versus-file-upload split mirroring the two role rows. The family is the single closed picker concept; the markup is the consumer's, only the behavior owned.
- Cases: the `color` arm reads the picked channel through the `getColorChannels(colorSpace)` channel set and the `parseColor(value)` `Color`, then projects it into the OKLCH space `theme-tokens.md#THEME_TOKENS` owns — a picked color is one `oklch` token string and `contrastWith` runs on the pick so a material-color pick is contrast-checked and token-emitted in one fold, never an sRGB-hex value beside the token projection (the live `useColorPickerState` `color`/`setColor` cell is the consumer's stateful surface, composed by reference, never re-minted here); the `date` arm selects the `react-aria-components` `RangeCalendar` or `Calendar` widget by `mode` for schedule and milestone surfaces; the `filePreview` and `fileUpload` arms each select a `FileTrigger` `acceptedFileTypes` ingest for IFC/GLB, the preview arm gating a read-only inspection and the upload arm a write ingest — the two rows stay distinct because the role vocabulary carries both. `applyPicker` is total over the four arms under the generated `$match` so a fifth picker without an arm is a compile error.
- Entry: a picker composes one `PickerBehavior` arm and the matching `role-behavior.md#ROLE_BEHAVIOR` `pickers` `RoleBehavior<Props, State>` contract by reference; the picked value reads and writes through `binding/atom-binding.md#ATOM_BINDING`, never a second state binding; the OKLCH conversion is by reference from `theme-tokens.md#THEME_TOKENS` (the `Color` OKLCH space, the picked channel projected to an `oklch` token, `contrastWith` on the pick), never a second `colorjs.io` import path; the per-component `react-aria` `.tsx` markup stays the consumer's.
- Packages: `react-aria`, `react-aria-components`, `react-stately`, `colorjs.io`, `effect`.
- Growth: a new picker behavior lands as one `PickerBehavior` arm breaking the `applyPicker` `$match` at compile time; a new color channel projection lands as one read over `getColorChannels`, never a hand-rolled channel math; a new date surface lands as one `react-aria-components` date widget under the `date` arm; a new ingest kind lands as one `FileTrigger` `acceptedFileTypes` row under the preview or upload arm, never a collapsed single file arm.
- Boundary: an sRGB-hex color value beside the OKLCH token projection is the named defect the `theme-tokens.md#THEME_TOKENS` Boundary forbids, extended here to the picker; a hand-rolled color-channel math beside `getColorChannels`/`parseColor` is the named defect; a per-component react-aria `.tsx` markup owned here is the named defect the `role-behavior.md#ROLE_BEHAVIOR` Boundary forbids — only the headless behavior is owned; collapsing `file-preview`+`file-upload` into one file arm is the named defect — the vocabulary carries both as distinct rows; a second state binding beside the `AtomBinding` is the named defect; a second `colorjs.io` import path beside the `theme-tokens` OKLCH owner is the named defect.

```ts contract
import { Calendar, FileTrigger, RangeCalendar } from "react-aria-components";
import { getColorChannels, parseColor, type Color } from "react-stately";
import Colorjs from "colorjs.io";
import { Data, Option } from "effect";

// --- [TYPES] -----------------------------------------------------------------------------

type PickerBehavior = Data.TaggedEnum<{
  readonly color: { readonly value: string; readonly colorSpace: "rgb" | "hsl" | "hsb" };
  readonly date: { readonly mode: "single" | "range"; readonly value: Option.Option<string> };
  readonly filePreview: { readonly acceptedFileTypes: ReadonlyArray<string> };
  readonly fileUpload: { readonly acceptedFileTypes: ReadonlyArray<string>; readonly multiple: boolean };
}>;
const PickerBehavior = Data.taggedEnum<PickerBehavior>();

interface PickerView {
  readonly aria: Record<string, unknown>;
  readonly render: () => unknown;
}

// --- [OPERATIONS] ------------------------------------------------------------------------

// The picked color is projected through the `theme-tokens.md#THEME_TOKENS` OKLCH space so a
// pick is a perceptual `oklch` token and `contrastWith` runs on it — never an sRGB hex.
const colorPick = (value: string, colorSpace: "rgb" | "hsl" | "hsb"): { readonly token: string; readonly channels: ReadonlyArray<string> } => {
  const color: Color = parseColor(value);
  const channels = getColorChannels(colorSpace).map((channel) => String(color.getChannelValue(channel)));
  const oklch = new Colorjs(color.toString("css")).to("oklch").toString({ format: "oklch" });
  return { token: oklch, channels };
};

// The total dispatch over the four `_Roles.pickers` behaviors; a fifth arm without a branch
// is a compile error under the generated `$match` fold.
const applyPicker: (behavior: PickerBehavior) => PickerView = PickerBehavior.$match({
  color: ({ value, colorSpace }) => ({
    aria: { role: "group", "aria-label": "color picker" },
    render: () => colorPick(value, colorSpace),
  }),
  date: ({ mode }) => ({
    aria: { role: "group", "aria-label": "date picker" },
    render: () => (mode === "range" ? RangeCalendar : Calendar),
  }),
  filePreview: ({ acceptedFileTypes }) => ({
    aria: { role: "group", "aria-label": "file preview" },
    render: () => ({ trigger: FileTrigger, acceptedFileTypes, allowsMultiple: false }),
  }),
  fileUpload: ({ acceptedFileTypes, multiple }) => ({
    aria: { role: "group", "aria-label": "file upload" },
    render: () => ({ trigger: FileTrigger, acceptedFileTypes, allowsMultiple: multiple }),
  }),
});

export { applyPicker, colorPick, PickerBehavior };
```

The `parseColor`/`getColorChannels` channel-projection surface (`react-stately` entrypoints [7]/[8]) and the `useColorPickerState` `color`/`setColor` cell (entrypoint [4], the consumer's stateful color component) are the headless color surface; the `Calendar`/`RangeCalendar`/`DatePicker`/`FileTrigger` widgets (`react-aria-components` date and file entrypoints) are the date and ingest behavior the arms select. The OKLCH projection is the `theme-tokens.md#THEME_TOKENS` `colorjs.io` `Color` space read by reference, the one place a pick becomes a perceptual token.
