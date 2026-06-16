# [API_EFFECT_ESLINT_PLUGIN]

Dependency catalogue for `@effect/eslint-plugin` (reflected version `0.3.2`). Surface is grounded from the installed `node_modules/@effect/eslint-plugin/dist/dts` declarations via `uv run python -m tools.assay api query --key @effect/eslint-plugin` (`--symbol` / `--full` for member detail), authoritative over any published or remembered surface. This is a build-time lint toolchain package consumed by the flat-config ESLint surface, never imported into a runtime bundle; tier is `neutral` but it is a dev-tool import, not a planning-owner runtime dependency. Runtime dependencies of the package itself: `@dprint/formatter`, `@dprint/typescript`, `prettier-linter-helpers`. Rule typing rides `@typescript-eslint/utils/ts-eslint` (`RuleModule` / `RuleListener`, referenced via inline `import(...)` in every rule/plugin declaration) and `@typescript-eslint/utils` root (`ESLintUtils`, imported by `utils/eslint`); config-schema typing rides `@typescript-eslint/utils/json-schema` (`JSONSchema4`).

The default export (`import plugin from "@effect/eslint-plugin"`) re-exports `./plugin` (`meta`, `rules`, `configs`) and side-effect imports `./configs/dprint`. Nine package entrypoints are exported; each is reflected below with exact spellings.

---

## [.] index (default export)

```typescript
// dist/dts/index.d.ts
import * as plugin from "@effect/eslint-plugin/plugin";
import "@effect/eslint-plugin/configs/dprint";   // side-effect registration
export * from "@effect/eslint-plugin/plugin";     // re-exports meta, rules, configs
export default plugin;
```

The default import is the ESLint flat-config plugin object. `export *` surfaces `meta` / `rules` / `configs` as named re-exports alongside the default.

---

## [./plugin] plugin object

```typescript
// dist/dts/plugin.d.ts
export declare const meta: {
  name: string;
};

export declare const rules: {
  dprint: import("@typescript-eslint/utils/ts-eslint").RuleModule<
    "requireLinebreak" | "extraLinebreak" | "requireWhitespace" | "extraWhitespace"
      | "requireCode" | "extraCode" | "replaceWhitespace" | "replaceCode"
      | "moveCodeToNextLine" | "moveCodeToPrevLine" | "moveCode",
    [{ config: {} }],
    unknown,
    import("@typescript-eslint/utils/ts-eslint").RuleListener
  >;
  "no-import-from-barrel-package": import("@typescript-eslint/utils/ts-eslint").RuleModule<
    "replaceImport",
    [import("@effect/eslint-plugin/rules/no-import-from-barrel-package").Options],
    unknown,
    import("@typescript-eslint/utils/ts-eslint").RuleListener
  >;
};

export declare const configs: {
  dprint: Array<any>;
};
```

`meta.name` is the plugin namespace stamp. `rules` carries exactly two `RuleModule` entries: `dprint` (11 message IDs, single `{ config: {} }` options tuple) and `no-import-from-barrel-package` (one `replaceImport` message ID, `Options` tuple). `configs.dprint` is the flat-config preset array (typed loosely as `Array<any>` on the plugin object; the precise element shapes are reflected under `./configs/dprint`).

---

## [./rules/dprint] dprint rule

```typescript
// dist/dts/rules/dprint.d.ts
export interface Message {
  messageId: keyof (typeof dprint)["meta"]["messages"];
  data: Record<string, string>;
}

export declare const dprint: import("@typescript-eslint/utils/ts-eslint").RuleModule<
  "requireLinebreak" | "extraLinebreak" | "requireWhitespace" | "extraWhitespace"
    | "requireCode" | "extraCode" | "replaceWhitespace" | "replaceCode"
    | "moveCodeToNextLine" | "moveCodeToPrevLine" | "moveCode",
  [{ config: {} }],
  unknown,
  import("@typescript-eslint/utils/ts-eslint").RuleListener
>;
```

The dprint formatter-as-lint rule. The 11 message IDs are the closed report vocabulary; `Message.messageId` is keyed off the rule's own `meta.messages` map. Options is a single-element tuple `[{ config: {} }]` whose `config` is the dprint plugin configuration object.

---

## [./rules/no-import-from-barrel-package] barrel-import rule

```typescript
// dist/dts/rules/no-import-from-barrel-package.d.ts
export type Options = {
  packageNames: Array<string>;
};

export type MessageIds = "replaceImport";

export declare const noImportFromBarrelPackage: import("@typescript-eslint/utils/ts-eslint").RuleModule<
  "replaceImport",
  [Options],
  unknown,
  import("@typescript-eslint/utils/ts-eslint").RuleListener
>;
```

Flags imports from barrel packages named in `Options.packageNames`. The exported const name is `noImportFromBarrelPackage`; it surfaces in `plugin.rules` under the dasherized key `"no-import-from-barrel-package"`. Single message ID `replaceImport`.

---

## [./configs/dprint] dprint flat-config preset

```typescript
// dist/dts/configs/dprint.d.ts
import * as plugin from "@effect/eslint-plugin/plugin";
declare const _default: ({
  rules: {
    // 78 core/legacy ESLint stylistic rules set to a disable string, e.g.:
    "array-bracket-newline": string;
    "array-bracket-spacing": string;
    "arrow-parens": string;
    curly: string;
    indent: string;
    "max-len": string;
    quotes: string;
    semi: string;
    // ... through "yield-star-spacing"
    // 12 "@typescript-eslint/*" stylistic rules, e.g.:
    "@typescript-eslint/indent": string;
    "@typescript-eslint/member-delimiter-style": string;
    "@typescript-eslint/semi": string;
    // 15 "react/jsx-*" stylistic rules, e.g.:
    "react/jsx-closing-bracket-location": string;
    "react/jsx-indent": string;
    "react/jsx-wrap-multilines": string;
  };
} | {
  plugins: {
    "@effect": typeof plugin;
  };
  rules: {
    "@effect/dprint": string;
  };
})[];
export default _default;
```

Two-element flat-config array: element one disables exactly 105 conflicting stylistic rules (78 core ESLint stylistic + 12 `@typescript-eslint/*` + 15 `react/jsx-*`), each value typed as `string`; element two registers the plugin under the `"@effect"` namespace and enables `"@effect/dprint"`. Importing `./index` runs this side-effect import. The full per-rule key list is reflected verbatim in the `.d.ts`; only representative keys are transcribed here, with the full set captured as a gap below.

---

## [./configs/disable-conflict-rules] conflict-disable preset

```typescript
// dist/dts/configs/disable-conflict-rules.d.ts
declare const _default: {
  rules: {
    // identical 105-key disable map as configs/dprint element one
    "array-bracket-newline": string;
    // ... through "react/jsx-wrap-multilines"
  };
}[];
export default _default;
```

Single-element array carrying only the disable map (no plugin registration). This is the standalone form of `configs/dprint` element one, for consumers that register the dprint rule separately. Same 105 stylistic-rule keys (78 core + 12 `@typescript-eslint/*` + 15 `react/jsx-*`).

---

## [./utils/eslint] rule-authoring utilities

```typescript
// dist/dts/utils/eslint.d.ts
import { ESLintUtils } from "@typescript-eslint/utils";

declare const getParserServices: typeof ESLintUtils.getParserServices;

declare const createRule: <Options extends readonly unknown[], MessageIds extends string>(
  { meta, name, ...rule }: Readonly<ESLintUtils.RuleWithMetaAndName<Options, MessageIds, unknown>>,
) => ESLintUtils.RuleModule<MessageIds, Options, unknown, ESLintUtils.RuleListener>;

export { getParserServices, createRule };
```

Re-exports of `@typescript-eslint/utils` `ESLintUtils` rule-authoring helpers. `getParserServices` forwards directly; `createRule` is the typed `RuleModule` factory binding `meta` + `name`. These are the package-internal authoring primitives, exposed for consumers writing rules against the same typing contract.

---

## [./RegularExpression] whitespace/linebreak predicates

```typescript
// dist/dts/RegularExpression.d.ts
export declare const IS_WHITESPACE_REGEX: RegExp;
export declare const STARTS_WITH_WHITESPACE_REGEX: RegExp;
export declare const HAS_LINE_BREAK_REGEX: RegExp;
export declare const GLOBAL_LINE_BREAK_REGEX: RegExp;

export declare function isWhitespace(s: string): boolean;   // text is whitespace(s)
export declare function hasLineBreak(s: string): boolean;   // text contains line break(s)
```

Pure string predicates used by the dprint rule's source-text inspection. Four exported `RegExp` constants plus two boolean predicates. `isWhitespace` and `hasLineBreak` also surface on the package index.

---

## [./Dprint] config schema

```typescript
// dist/dts/Dprint.d.ts
import type { JSONSchema4 } from "@typescript-eslint/utils/json-schema";
export declare const ConfigSchema: JSONSchema4;
```

The JSON Schema (draft-4) describing the dprint rule's `config` options object. Typed as the opaque `JSONSchema4` from `@typescript-eslint/utils/json-schema`; the runtime schema object's internal property set is not expressed at the type level.

---

## [GAPS]

- `configs/dprint` and `configs/disable-conflict-rules` each declare an identical 105-key stylistic-rule disable map (78 core ESLint stylistic + 12 `@typescript-eslint/*` + 15 `react/jsx-*`), every value typed as `string`. Only representative keys are transcribed; the full key list lives verbatim in the `.d.ts` and is reproducible via `uv run python -m tools.assay api query --key @effect/eslint-plugin --symbol configs --full`. Re-pull before any planning owner that depends on the exact disabled-rule set.
- `ConfigSchema: JSONSchema4` is opaque at the type level — the runtime JSON Schema property tree (the dprint `config` option keys) is not reflectable from `.d.ts` and is not captured.
- `rules.dprint` options `[{ config: {} }]` types `config` as the empty object `{}` at the declaration level; the actual accepted dprint configuration shape is not expressed in types.
- The package's own runtime dependencies (`@dprint/formatter`, `@dprint/typescript`, `prettier-linter-helpers`) are not reflected here; they carry no planning-owner consumer.
