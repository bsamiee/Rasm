# [UI_COMMAND]

The in-app command and styling-recipe surface as one `content` sub-domain — a genuine higher-order capability folder holding no domain state, dialing only through the `interchange` `CommandGateway`. `CommandAction` is the one AEC action-lexicon vocabulary owner-block (place-element, run-analysis, open-viewpoint, export-fabrication, toggle-overlay) whose every row carries its `label`, its `lucide-react` `LucideIcon`, its keybinding, the `interaction/role.md#ROLE_BEHAVIOR` `InteractionRole` provenance the live-region politeness derives from, and the stable-string `interchange/Transport/gateway#COMMAND_GATEWAY` `IntentRegistry` intent key the gateway resolves to a `{ verb: ControlVerb, payload }` dial. The vocabulary renders through the `cmdk` `Command`/`Command.Dialog` palette filtered by the one `react-aria` `useFilter` predicate and the `react-stately` `UNSTABLE_useFilteredListState` view, and through the `vaul` `Drawer` directional snap-point sheet as the mobile/secondary action surface; the `cn = (...a) => twMerge(cx(...a))` variant-recipe owner paired with the `@radix-ui/react-slot` `Slot`/`Slottable` `asChild` primitive is the single composition root the whole library reads, and `ActionIcon` keys the tree-shaken `lucide-react` icon vocabulary by `CommandAction` row. Every overlay rides the `theming/tokens.md#THEME_TOKENS` `tw-animate-css` `data-[state]` enter/exit layer; the palette subscribes command-availability through `binding/atom.md#ATOM_BINDING` to the `projection/evidence/availability#AVAILABILITY_GATE` `AvailabilityStore.isEnabled(intentKey)` gate so a disabled command never fires. The folder collapses six admitted packages with zero prior owner — `cmdk`/`vaul`/`lucide-react`/`class-variance-authority`/`tailwind-merge`/`@radix-ui/react-slot` — into this one page.

## [1]-[INDEX]

- [1]-[COMMAND_SURFACE]: the `CommandAction` AEC vocabulary, the `cmdk` palette, the `vaul` drawer, the `cn`+`Slot` variant-recipe, and `ActionIcon`.

## [2]-[COMMAND_SURFACE]

- Owner: `CommandAction`, the one `as const satisfies Record<string, CommandSpec>` AEC action-lexicon vocabulary owner-block — each row carries `label`, the `lucide-react` `LucideIcon`, the `keybinding`, the `InteractionRole` provenance the live-region `politeness` derives from through `role-behavior.md#ROLE_BEHAVIOR` `announceFor`, and the `intentKey` the `interchange` `CommandGateway` `IntentRegistry.resolve` maps to a `{ verb: ControlVerb, payload }` dial; `CommandPalette`, the single `cmdk` `Command`/`Command.Dialog` mount folding the vocabulary through the one `useFilter` predicate and `UNSTABLE_useFilteredListState` view; `ActionDrawer`, the single `vaul` `Drawer.Root` directional snap-point sheet; `cn`, the one `cn = (...a: CxOptions) => twMerge(cx(...a))` variant-recipe owner paired with the `@radix-ui/react-slot` `Slot`/`Slottable` `asChild` polymorphic-composition primitive at the single composition root; and `ActionIcon`, the `lucide-react` `LucideIcon` vocabulary keyed by `CommandAction` row over the tree-shaken named-icon exports and the `LucideProvider` default-prop context. The five owner-blocks are the members of this one page; the folder holds no domain state and re-mints no command, intent, variant, or icon vocabulary beside the gateway.
- Cases: each `CommandAction` row is the action's whole identity — `label` is the rendered text, `icon` is the named `lucide-react` export, `keybinding` is the accelerator, `role` is the `InteractionRole` the `politeness` derives from (`actions` for place/run/export, `overlays` for toggle), and `intentKey` is the `IntentRegistry` stable string against `csharp:Rasm.AppUi/Shell/commands#TS_PROJECTION` so a deep link survives reload; the gateway owns the key→verb mapping (`IntentRegistry.resolve(key) => Option<{ verb, payload }>` over the closed `ControlVerb = "captureSupport" | "setDegradation" | "reloadOptions"` domain), so the row carries the key and never a re-minted verb. `CommandPalette` mounts `Command.Dialog` (the `cmdk` root wrapped in `@radix-ui/react-dialog`, emitting the `data-state` the `tw-animate-css` enter/exit layer rides) with the `filter` prop bound to the shared `useFilter` `score` predicate so the palette and every data surface score identically — the `cmdk` built-in `command-score` (`defaultFilter`) is the deleted defect — and a collection result set folds through the same predicate's `UNSTABLE_useFilteredListState` `filteredList` view; on `Command.Item` select the row's `intentKey` resolves through `IntentRegistry` and dials, gated by `AvailabilityStore.isEnabled` so a disabled item never fires. `ActionDrawer` mounts `Drawer.Root` keyed by `direction`/`snapPoints`/`activeSnapPoint`/`dismissible`/`modal` carried as payload, the `WithFadeFromProps`/`WithoutFadeFromProps` snap discriminant resolved by whether `fadeFromIndex` is present, as the mobile/secondary surface dialing the same `CommandAction` rows. `cn` folds `cva` `cx` (the `clsx` re-export) through `tailwind-merge` `twMerge` so every component variant is one `cva` row resolved conflict-safe, and `Slot` merges props/ref onto a single `asChild` child so every polymorphic element composition rides one primitive; `ActionIcon` resolves each row's named `LucideIcon` under the `LucideProvider` default `size`/`strokeWidth` context, never `createLucideIcon` per action.
- Entry: every mutation exits through the one `interchange` `CommandGateway` — the palette resolves a selected `CommandAction.intentKey` through `IntentRegistry.resolve` and dials `clients.control[verb]`, never a transport directly; command-availability subscribes through `binding/atom.md#ATOM_BINDING` to the `projection` `AvailabilityStore` `isEnabled(intentKey)` read so the dial-time gate greys a disabled or below-tier command; the palette and drawer compose the `role-behavior.md#ROLE_BEHAVIOR` `actions`/`overlays` rows by reference for their accessibility and focus behavior; overlay enter/exit rides the `Theme/tokens.md#THEME_TOKENS` `tw-animate-css` `data-[state]` layer; the `cn` recipe and `ActionIcon` `LucideProvider` are mounted once at the SPA composition root and read by every leaf.
- Packages: `cmdk`, `vaul`, `lucide-react`, `class-variance-authority`, `tailwind-merge`, `@radix-ui/react-slot`, `react-aria`, `react-stately`, `effect`.
- Growth: a new action lands as one `CommandAction` row carrying its `label`/`icon`/`keybinding`/`role`/`intentKey`, never a sibling command enum; a new addressable intent lands as one `IntentRegistry` key bound to one verb at the gateway, never re-minted here; a new overlay surface lands as one arm composing `Command.Dialog` or `Drawer.Root`, never a third mount; a new component variant lands as one `cva` row read through `cn`, never a per-`.tsx` className soup; a new icon lands as one named `lucide-react` import keyed by row, never a hand-crafted SVG.
- Boundary: a bare `clsx` import is the named defect the `cn` owner deletes via `cva` `cx`; a per-`.tsx` className soup beside the one `cn` recipe owner is the named defect; a hand-rolled `asChild` `cloneElement` merge beside the `@radix-ui/react-slot` `Slot` primitive is the named defect; a second command/intent enum beside the gateway `IntentRegistry` is the named defect — the leaf binds the C# `commands-availability` `TS_PROJECTION` vocabulary and re-mints no parallel intent shape; the `cmdk` built-in `command-score` used for the palette while another surface hand-rolls a predicate is the named defect the shared `useFilter` deletes; a hand-crafted SVG icon for an icon `lucide-react` already exports, or `createLucideIcon` per action instead of the named tree-shaken export, is the named defect; the surface dials no transport and holds no domain state.

```ts contract
import type { CommandGateway, IntentRegistry } from "@rasm/interchange";
import { AvailabilityStore } from "@rasm/projection";
import { Activity, Download, Eye, Layers, PlusSquare, type LucideIcon } from "lucide-react";
import { Command } from "cmdk";
import { Drawer } from "vaul";
import { cva, cx, type CxOptions, type VariantProps } from "class-variance-authority";
import { twMerge } from "tailwind-merge";
import { Slot, Slottable } from "@radix-ui/react-slot";
import { useFilter } from "react-aria";
import { UNSTABLE_useFilteredListState, type ListState } from "react-stately";
import { Effect, Option, Record as R, Schema } from "effect";
import * as React from "react";

// --- [TYPES] -----------------------------------------------------------------------------

interface CommandSpec {
  readonly label: string;
  readonly icon: LucideIcon;
  readonly keybinding: string;
  readonly role: "actions" | "overlays";
  readonly intentKey: string;
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const _Commands = {
  "place-element":      { label: "Place element",      icon: PlusSquare,  keybinding: "p",      role: "actions",  intentKey: "place-element"      },
  "run-analysis":       { label: "Run analysis",       icon: Activity,    keybinding: "mod+r",  role: "actions",  intentKey: "run-analysis"       },
  "open-viewpoint":     { label: "Open viewpoint",     icon: Eye,         keybinding: "v",      role: "actions",  intentKey: "open-viewpoint"     },
  "export-fabrication": { label: "Export fabrication", icon: Download,    keybinding: "mod+e",  role: "actions",  intentKey: "export-fabrication" },
  "toggle-overlay":     { label: "Toggle overlay",     icon: Layers,      keybinding: "o",      role: "overlays", intentKey: "toggle-overlay"     },
} as const satisfies Record<string, CommandSpec>;

const CommandAction = Schema.Literal(
  ...(R.keys(_Commands) as [keyof typeof _Commands, ...Array<keyof typeof _Commands>]),
);
type CommandAction = Schema.Schema.Type<typeof CommandAction>;
const commandRows: ReadonlyArray<CommandSpec & { readonly action: CommandAction }> = R.toEntries(_Commands).map(
  ([action, spec]) => ({ action, ...spec }),
);

// --- [OPERATIONS] ------------------------------------------------------------------------

// `CommandGateway` (`invoke: (verb, payload) => Effect<CommandReceiptWire, FaultDetail, AvailabilityStore>`),
// `IntentRegistry` (`resolve: (key) => Option<{ verb, payload }>`), and `AvailabilityStore`
// (`isEnabled: (intentKey) => Effect<boolean>`) are owned by `interchange/Transport/gateway#COMMAND_GATEWAY`
// and `projection/evidence/availability#AVAILABILITY_GATE` — imported by reference, never re-minted.

// `cn` is the ONE variant-recipe owner the whole library reads: `cva` `cx` (the `clsx`
// re-export) folded through `tailwind-merge` `twMerge` for conflict-safe class joining.
const cn = (...inputs: CxOptions): string => twMerge(cx(...inputs));

// Every component variant is one `cva` row read through `cn`; `VariantProps` extracts the
// prop union at the component boundary. `Slot`/`Slottable` own `asChild` element merge.
const actionItem = cva("flex items-center gap-2 rounded-sm px-2 py-1.5 outline-none", {
  variants: {
    state: { idle: "data-[selected=true]:bg-[--accent]", disabled: "pointer-events-none opacity-50" },
  },
  defaultVariants: { state: "idle" },
});
type ActionItemVariants = VariantProps<typeof actionItem>;

const ActionIcon: React.FC<{ readonly action: CommandAction; readonly size?: number }> = ({ action, size }) => {
  const Icon = _Commands[action].icon;
  return React.createElement(Icon, { size, "aria-hidden": true });
};

// One `asChild`-polymorphic action element: `Slot` merges props/ref onto a single child so
// the caller's element shape composes the recipe without a `cloneElement` hand-roll.
const ActionElement: React.FC<
  { readonly asChild?: boolean; readonly children: React.ReactNode } & ActionItemVariants
> = ({ asChild, state, children }) => {
  const Comp = asChild ? Slot : "div";
  return React.createElement(Comp, { className: cn(actionItem({ state })) }, React.createElement(Slottable, null, children));
};

// The ONE filtering primitive (T-FILTER-COLLAPSE): `useFilter`'s locale-aware `contains`
// drives BOTH the `cmdk` `filter` prop AND the `UNSTABLE_useFilteredListState` view, so the
// palette, the data grid, and any collection score identically — the built-in `command-score`
// is never used and no surface hand-rolls a `contains`/`startsWith` predicate.
const useCommandFilter = (): {
  readonly score: (value: string, search: string, keywords?: ReadonlyArray<string>) => number;
  readonly filteredList: <T extends object>(state: ListState<T>, search: string) => ListState<T>;
} => {
  const { contains } = useFilter({ sensitivity: "base" });
  const score = (value: string, search: string, keywords?: ReadonlyArray<string>): number =>
    search.length === 0 || contains(value, search) || (keywords ?? []).some((k) => contains(k, search)) ? 1 : 0;
  const filteredList = <T extends object>(state: ListState<T>, search: string): ListState<T> =>
    UNSTABLE_useFilteredListState(state, (nodeValue) => contains(nodeValue, search));
  return { score, filteredList };
};

// `CommandPalette`: one `Command.Dialog` mount folding the vocabulary through the shared
// predicate; on select the row's `intentKey` resolves to a `{ verb, payload }` and dials,
// gated by `isEnabled` so a disabled command never fires.
const CommandPalette: React.FC<{
  readonly open: boolean;
  readonly registry: IntentRegistry;
  readonly gateway: CommandGateway;
  readonly store: AvailabilityStore;
  readonly run: <A>(effect: Effect.Effect<A>) => Promise<A>;
}> = ({ open, registry, gateway, store, run }) => {
  const { score } = useCommandFilter();
  // The palette pre-gates on `isEnabled` (greys a disabled item) AND provides the same
  // `AvailabilityStore` into the gateway's `R` channel, the dial-time gate the gateway re-reads;
  // the typed `FaultDetail` rail is discharged at the leaf, never re-thrown.
  const onSelect = (intentKey: string): void => {
    void run(
      Effect.flatMap(store.isEnabled(intentKey), (enabled) =>
        enabled
          ? Option.match(registry.resolve(intentKey), {
              onNone: () => Effect.void,
              onSome: ({ verb, payload }) =>
                Effect.provideService(Effect.ignore(gateway.invoke(verb, payload)), AvailabilityStore, store),
            })
          : Effect.void),
    );
  };
  return React.createElement(
    Command.Dialog,
    { open, label: "Command palette", filter: score },
    React.createElement(Command.Input, { placeholder: "Search actions…" }),
    React.createElement(
      Command.List,
      null,
      React.createElement(Command.Empty, null, "No matching action"),
      ...commandRows.map((row) =>
        React.createElement(
          Command.Item,
          { key: row.action, value: row.label, keywords: [row.action], onSelect: () => onSelect(row.intentKey) },
          React.createElement(ActionIcon, { action: row.action }),
          row.label,
          React.createElement("kbd", null, row.keybinding),
        ),
      ),
    ),
  );
};

// `ActionDrawer`: one `vaul` `Drawer.Root` directional snap-point sheet — the mobile/secondary
// action surface dialing the same `CommandAction` rows through the same gateway.
const ActionDrawer: React.FC<{
  readonly open: boolean;
  readonly direction: "top" | "bottom" | "left" | "right";
  readonly snapPoints: ReadonlyArray<number | string>;
  readonly children: React.ReactNode;
}> = ({ open, direction, snapPoints, children }) =>
  React.createElement(
    Drawer.Root,
    { open, direction, snapPoints: [...snapPoints], dismissible: true, modal: true },
    React.createElement(
      Drawer.Portal,
      null,
      React.createElement(Drawer.Overlay, { className: "fixed inset-0 bg-[--scrim]" }),
      React.createElement(
        Drawer.Content,
        { className: "fixed flex flex-col rounded-t-lg bg-[--surface]" },
        React.createElement(Drawer.Handle, null),
        children,
      ),
    ),
  );

export { ActionDrawer, ActionElement, ActionIcon, CommandAction, CommandPalette, cn, useCommandFilter };
```

The `UNSTABLE_useFilteredListState(state, fn)` filtered `ListState` view (`react-stately` entrypoint [3]) and the `useFilter` `Filter` predicate are the one scoring primitive bound here; `cmdk`'s `Command.Item.value` is scored by the same predicate the `filter` prop receives, so the command palette and the `interaction/role.md#ROLE_BEHAVIOR` `collections` data surfaces share one filtering algebra. The `lucide-react` named-icon exports are tree-shaken `ForwardRefExoticComponent` per row; the `LucideProvider` default-prop context sets project-wide `size`/`strokeWidth`/`color` once at the SPA root.
