# [TS_TESTS_API_AXE_CORE_PLAYWRIGHT]

[PACKAGE_SURFACE]:
- package: `@axe-core/playwright` · version `4.12.1` · license `MPL-2.0` · depends on `axe-core@4.12.1`, peer `playwright-core >= 1.0.0`
- module: dual CJS/ESM (`dist/index.js` + `dist/index.mjs`) with one `.` export; `AxeBuilder` ships named AND as default — the named import is the house spelling.
- asset: bundles the `axe-core` rules engine source and injects it into the page (and every child frame) at `analyze()`; no browser binary, no server, no network.
- runtime: node `>=18` driving a live `playwright-core` `Page` — the audit executes inside the page, the receipt returns to node.
- plane: `plane:dev` — the accessibility half of the `tests/typescript/e2e` visual-and-aria gauge, beside `@playwright/test`; `tests/typescript/_architecture` fences it off every runtime graph.
- rail: wcag-conformance gauge over a live page.

`@axe-core/playwright` is one builder class over one live `Page`: chain scope (`include`/`exclude`), rule selection (`withTags`/`withRules`/`disableRules`), and engine options (`options`), then `analyze()` injects axe into every frame and folds the audit into a single typed `AxeResults` receipt. It is the rules-engine complement to the two golden gauges — `toMatchAriaSnapshot` freezes one accessibility tree, `toHaveScreenshot` freezes pixels, axe audits CONFORMANCE against the wcag rule catalog with zero goldens to mint. Kit fixtures compose it as one row whose tag set is the policy value; a spec asserts on `violations` and never re-learns the engine.

## [01]-[BUILDER_SURFACE]

[ENTRYPOINT_SCOPE]: one class, fluent rows, one terminal verb.

| [INDEX] | [SYMBOL]                   | [TYPE]                          | [CAPABILITY]                                                          |
| :-----: | :------------------------- | :------------------------------ | :-------------------------------------------------------------------- |
|  [01]   | `new AxeBuilder({ page })` | `(params: AxePlaywrightParams)` | binds one live `Page`; `axeSource` swaps the injected engine build    |
|  [02]   | `include(selector)`        | `(SerialFrameSelector) => this` | scope IN: repeatable; frame-piercing via selector arrays              |
|  [03]   | `exclude(selector)`        | `(SerialFrameSelector) => this` | scope OUT: subtracts subtrees from the audit                          |
|  [04]   | `withTags(tags)`           | `(string \| string[]) => this`  | rule selection by tag — `wcag2a`/`wcag2aa`/`wcag21aa`/`best-practice` |
|  [05]   | `withRules(rules)`         | `(string \| string[]) => this`  | rule selection by id — additive                                       |
|  [06]   | `disableRules(rules)`      | `(string \| string[]) => this`  | rule id removal; wins over any selection                              |
|  [07]   | `options(options)`         | `(RunOptions) => this`          | raw engine pass-through; `withTags`/`withRules` are its typed face    |
|  [08]   | `setLegacyMode(on?)`       | `(boolean?) => this`            | pre-4.3 frame injection; never used — modern frame audit is default   |
|  [09]   | `analyze()`                | `() => Promise<AxeResults>`     | inject, run, fold every frame; the one receipt                        |

```ts signature
// dist/index.d.mts — the whole public surface; AxeBuilder is also the default export.
interface AxePlaywrightParams { page: Page; axeSource?: string }
declare class AxeBuilder {
  constructor({ page, axeSource }: AxePlaywrightParams)
  include(selector: SerialFrameSelector): this
  exclude(selector: SerialFrameSelector): this
  options(options: RunOptions): this
  withRules(rules: string | string[]): this
  withTags(tags: string | string[]): this
  disableRules(rules: string | string[]): this
  setLegacyMode(legacyMode?: boolean): this
  analyze(): Promise<AxeResults>
}
export { AxeBuilder, AxeBuilder as default }
```

## [02]-[RESULT_RECEIPT]

`AxeResults` is the audit as data: four disjoint result groups over one `Result` row shape; gauge verdict is `violations` — an empty array is conformance, and each violation row carries the rule id, impact, wcag tags, and the offending nodes with their selector ancestry.

```ts signature
// axe-core axe.d.ts — the receipt the builder resolves.
interface AxeResults extends EnvironmentData { toolOptions: RunOptions; passes: Result[]; violations: Result[]; incomplete: IncompleteResult[]; inapplicable: Result[] }
interface Result { description: string; help: string; helpUrl: string; id: string; impact?: ImpactValue; tags: TagValue[]; nodes: NodeResult[] }
interface NodeResult { html: string; impact?: ImpactValue; target: UnlabelledFrameSelector; any: CheckResult[]; all: CheckResult[]; none: CheckResult[]; failureSummary?: string; element?: HTMLElement; xpath?: string[]; ancestry?: UnlabelledFrameSelector }
interface RunOptions { runOnly?: RunOnly | TagValue[] | string[] | string; rules?: RuleObject; resultTypes?: resultGroups[]; selectors?: boolean; ancestry?: boolean; xpath?: boolean; iframes?: boolean; frameWaitTime?: number; preload?: boolean | PreloadOptions }
```

## [03]-[INTEGRATION]

[STACK: `@axe-core/playwright` + the e2e fixture tower] — the kit exposes one `a11y` fixture row: `new AxeBuilder({ page }).withTags([...wcagTags])` with an optional `include` scope, projecting `violations` as the verdict. Tag set is the fixture's policy value, so every target project — hermetic corpus or served product — inherits one conformance floor; a spec asserts `toEqual([])` and its falsification twin perturbs the DOM (`label` removed, `lang` stripped) to prove the audit draws the named rule id.

[STACK: axe beside the aria golden] — `toMatchAriaSnapshot` proves the tree the page EXPOSES matches its committed contract; axe proves the page CONFORMS to rules no golden encodes (contrast, labeling, landmark structure); the two never substitute: a renamed heading breaks the golden and passes axe, a low-contrast button passes the golden and breaks axe.

[BOUNDARY vs the unit lane] — axe runs against a real rendered page (styles, contrast, frames); a DOM-only structural assertion belongs to `happy-dom`/`jsdom` in the unit lane; the audit is a promise-rail Playwright citizen — it composes inside `.pw.ts` suites only, never on the Effect rail.

## [04]-[RAIL_LAW]

- Owns: rules-engine accessibility conformance over live Playwright pages — tag-selected wcag audits, frame-piercing scope, and the typed violation receipt.
- Accept: one builder per audit call; tag policy as data (`withTags`); `include`/`exclude` scoping; asserting on `violations` rule ids.
- Reject: a second axe injection path (`page.addScriptTag` with raw axe source); asserting on `failureSummary` prose — rule `id` and `tags` are the typed tokens; `best-practice` tags in the conformance floor — advisory rules are a ruled opt-in per audit, never the default gate; importing `axe-core` directly in a spec — the builder owns injection and the receipt types re-export through it.
- Boundary: the audit executes in the page and its receipt crosses back as one `AxeResults`; a violation is a page fact, so the repair lands in the served surface (or the hermetic corpus), never in the spec.
