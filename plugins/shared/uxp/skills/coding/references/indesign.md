# [INDESIGN]

Typings declare DOM classes as interfaces, and `instanceof` holds for the exact class alone (`Rectangle instanceof PageItem` reads false). Code discriminates instances by a member their prototypes declare or by their `constructorName` string.

## [01]-[MEMBERS]

DOM members are native accessors on class prototypes:
- `app.menuActions.everyItem().getElements()` lists every menu action with `name`, `area`, `id`, `enabled`, and `checked`
- Prototypes hold `constructorName`, `equals`, `toSource`, `toSpecifier`, and `toString` beside object model members
- Aggregate `.properties` invokes every getter before a failure can be isolated, and a class with a crashing getter reads its other declared fields one at a time
- Find/Change drop-shadow and inner-shadow `angle`, `distance`, `xOffset`, and `yOffset` accessors crash without a native owner
- Encoders refuse a promise a storage getter returns, and write an entry's `url` through `String`
- `Panel.visible` writes apply after the running script returns, and a read in that script returns the old value
- `MenuAction` `enabled` and `checked` reads take about 4.5 ms per action, one by one or through `itemByRange`, where `everyItem()` reads `name`, `area`, or `id` of all 4,219 actions in under 100 ms
- `placeGuns.loadPlaceGun` fails with "Import failed." in a document opened without a window, and a loaded place gun makes `PLACE_CURSOR_TOOL` the current tool
- `ToolBox.AFTER_ATTRIBUTE_CHANGED` is `afterAttributeChanged` on the module's `ToolBox`, and `UITools` holds 38 members where `indesign-uxp-types` 2.0.0 declares 36

## [02]-[SPECIFIERS]

Collection members are specifiers that resolve at each member read, and an unmatched lookup returns a specifier with `isValid` false:

```ts
import type { InDesignDOMObject } from 'adobe:indesign';

class Dom extends Context.Service<Dom>()('@app/indesign/Dom', {
    make: (indesign: typeof import('adobe:indesign')) =>
        Effect.sync(() => {
            const dom: typeof indesign = Object.create(indesign);
            const owning =
                <A>(member: keyof A & string) =>
                <T>(value: T): value is T & A =>
                    Predicate.hasProperty(value, 'toSpecifier') && Predicate.hasProperty(value, member);
            return Object.assign(dom, { owning, host: owning<InDesignDOMObject<unknown>>('toSpecifier') });
        }),
}) {
    static readonly resolve = Effect.fn('Dom.resolve')(function* (specifier: Specifier) {
        const [{ app, ScriptLanguage, host }, written] = yield* Effect.all([Dom, specifier.path]);
        const identified = (path: string): Option.Option<string> =>
            Option.flatMap(Schema.decodeUnknownOption(Specifier.FromString)(path), ({ kind, selector, end }) => Option.as(Option.filter(selector, (each) => Option.isNone(end) && Selector.guards.Identified(each)), kind));
        const resolvePath = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
            const requested = identified(path);
            return Option.filter(Option.liftThrowable((): unknown => app.doScript('resolve(arguments[0])', ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] =>
                Array.ensure<unknown>(found).every((each) => host(each) && each.isValid && (Option.isNone(requested) || Equal.equals(identified(each.toSpecifier()), requested))),
            );
        };
        const missed = (): Unmatched => {
            const siblings = Array.flatMap(Option.toArray(resolvePath(`${specifier.container}${specifier.separator}${specifier.kind}`)), Array.ensure);
            return Option.exists(specifier.selector, Selector.guards.Indexed) && Array.isReadonlyArrayNonEmpty(siblings)
                ? Unmatched.make({ subject: `object among indexes 0 to ${siblings.length - 1}`, query: Option.some(written) })
                : Unmatched.make({ subject: 'object', query: Option.some(written), candidates: Array.map(siblings, (each) => each.toSpecifier()) });
        };
        return yield* Option.match(resolvePath(written), {
            onSome: (found) => Effect.succeed(specifier.root || Option.isSome(specifier.selector) ? found : Array.ensure(found)),
            onNone: () => Effect.fail(Option.isSome(specifier.end) ? Unmatched.make({ subject: 'range', query: Option.some(written) }) : missed()),
        });
    });
}
const Container = Schema.String.check(Schema.isPattern(/(?:^|[^/])$/u));
const Separator = Schema.Literals(['//', '/']);
const Kind = Schema.String.check(Schema.isPattern(/^@?[a-z][\w-]*$/u));
const { Selector } = Reference;
const Step = Schema.Struct({ container: Container, separator: Separator, kind: Schema.Union([Kind, Schema.Literal('')]), selector: Schema.OptionFromOptionalKey(Selector) });
const Written = Schema.Union([Schema.TemplateLiteralParser([Container, Separator, Kind, Schema.toEncoded(Selector)]), Schema.TemplateLiteralParser([Container, Separator, Kind])]).pipe(
    Schema.decodeTo(
        Schema.toEncoded(Step),
        SchemaTransformation.transform({
            decode: ([container, separator, kind, selector]) => ({ container, separator, kind, ...(selector === undefined ? {} : { selector }) }),
            encode: ({ container, separator, kind, selector }) => (selector === undefined ? ([container, separator, kind] as const) : ([container, separator, kind, selector] as const)),
        }),
    ),
);
const Root = Schema.Literal('/').pipe(Schema.decodeTo(Schema.toEncoded(Step), { decode: SchemaGetter.succeed({ container: '', separator: '/', kind: '' }), encode: SchemaGetter.succeed('/') }));
class Specifier extends Schema.Class<Specifier, { readonly brand: unique symbol }>('Specifier')({ ...Step.fields, end: Schema.OptionFromOptionalKey(Step) }) {
    static readonly FromString = Schema.Union([
        Schema.TemplateLiteralParser(['(', Written, ' to ', Written, ')']).pipe(
            Schema.decodeTo(Schema.toEncoded(Schema.Struct({ ...Step.fields, end: Step })), SchemaTransformation.transform({ decode: ([, start, , end]) => ({ ...start, end }), encode: ({ end, ...start }) => ['(', start, ' to ', end, ')'] as const })),
        ),
        Written,
        Root,
    ]).pipe(Schema.decodeTo(this, SchemaTransformation.passthrough()));

    get root(): boolean {
        return this.kind === '';
    }
    get path(): Effect.Effect<string> {
        return Effect.orDie(Schema.encodeEffect(Specifier.FromString)(this));
    }
}
```

`getElements()` returns each member as its concrete class and throws on an invalid specifier. Reading a native getter (`constructorName`, `length`) with a class prototype as receiver crashes InDesign, and `owning` reads no getter: `Predicate.hasProperty` tests membership through the prototype chain, `toSpecifier` for a DOM object (`host`) and `everyItem` for a collection. `Predicate.hasProperty` alone holds wherever an ancestor prototype holds the name (`name` on any function or `Error`), and `owning` pairs it with `toSpecifier`. Guards for one kind take a member only that kind's prototypes declare (`documentOffset` for `Page`, `itemLayer` for every `PageItem` class). `Specifier.FromString` decodes a saved specifier into container, separator, and last step, or into a start and an end step for the `(<A> to <B>)` range that `findText` hits, `textStyleRanges` members, and `itemByRange` return, the application root `/` into an empty step `root` names, and refuses any other text. `Reference.Selector`, the tagged union every host's specifier steps share, decodes `[@id=<n>]`, `[<n>]`, and `[@name="<name>"]` into `Identified`, `Indexed`, and `Named`. `Dom.resolve` passes the path to ExtendScript's `resolve` as an argument, compares the `kind` of a lone `[@id=<n>]` step, answers a collection step without a selector as an array, and fails `Unmatched` with sibling specifiers as candidates or, for a missed `[<n>]`, the index span. Empty collections resolve as `undefined`, and `everyItem().getElements()` on a known collection reads an empty array. Supplied source calls native collection methods without computed-member rewriting.

| [INDEX] | [NEED]                     | [FORM]                                             | [REASON]                                      |
| :-----: | :------------------------- | :------------------------------------------------- | :-------------------------------------------- |
|  [01]   | Member by position or name | `item(index)`, negative from the end, or a name    | `collection[index]` reads `undefined`         |
|  [02]   | Every member as an array   | `everyItem().getElements()`                        | Collections have no iterator                  |
|  [03]   | One field of every member  | `everyItem().<field>`, one call per field          | Array at any count, `properties` reads all    |
|  [04]   | Same object across reads   | `equals`                                           | Repeated reads differ under `===`             |
|  [05]   | Address held across calls  | `id`, or `toSpecifier()` after `getElements()`     | `item(index)` specifiers shift on insert      |
|  [06]   | Address of a text range    | `toSpecifier()`, `Specifier.FromString`, `resolve` | `Character`, `Word`, `Paragraph` have no `id` |
|  [07]   | Style inside a group       | Specifier steps per group                          | Groups can repeat one style name              |
|  [08]   | Active document            | `Effect.try` over `app.activeDocument`             | Throws with only windowless documents open    |

- `app.panels` holds a member resolving to nothing, its `item(<index>)` reads `undefined`, and `everyItem()` or a range reads `undefined` in UXP and throws in ExtendScript
- Removed objects keep their `toSpecifier()`, read `isValid` false, and read `constructorName` empty
- `toSpecifier()` repeats the access path: `item(index)` positional, `itemByName` `[@name="<name>"]`, `itemByID` and `getElements()` `[@id=<n>]`
- `resolve` of an `[@id=<n>]` step returns the object with that id whatever class the step names
- Text specifiers are character spans that name another range after an edit before them, and `parentPage` reads `null` on the pasteboard
- Parents discriminate on `constructorName` (`Character` for an anchored item, `Spread` or `MasterSpread` for a top-level one)
- ExtendScript run through `doScript` keeps its own `scriptPreferences`, and reads and writes go through the object `Dom.resolve` returns
- `app.selection` throws with no document window open, windowless documents included, where `app.windows.length` reads 0

## [03]-[FIELDS]

Enumerators are objects: `===` between reads of one member is false, `JSON.stringify` yields `{}`, and `String` yields the member name. Each `Enumeration` holds every member under upper snake, camel, and lowercase keys (`CENTER_ALIGN`, `centerAlign`, `centeralign`). Scripts pass native values. JSON inputs decode `{type, member}` to an enumerator and `{specifier}` to the object it resolves, and a string written to a field whose `indesign-uxp-types` setter takes no string to a member of its enumeration or the named object in the document's collection of its class. Every other value passes to the host, which takes a name for a font, swatch, or style field.

Refusals throw one field at a time with the host's expected type ("Expected Unit or Leading enumerator"). Writes assign each decoded value once, keeping literal strings, and records write field by field into a field holding a DOM object (preferences). `JSON.stringify` replacers write enumerators as name, collections as type, specifier, and length, DOM objects as type, specifier, name, and `valid`.

- `indesign-uxp-types` states a field's range as `(Range: <minimum> to <maximum>)` in its doc comment and a collection's element class as `@collection <class>`
- Enumerators compare through `equals` (`Justification.CENTER_ALIGN.equals(value)`)
- Members decoding to one value (`FARSI_DIGITS`, `FULL_FARSI_DIGITS`) keep the held member where its decode `Equal.equals` the written member's
- New members take `add()` with required arguments, then field writes, and `properties` drops each key it refuses
- `add(record)` throws "Invalid parameter." or "Data is out of range." naming no field, and drops unknown keys and nested record refusals
- `documents.add(false, undefined, { documentPreferences: { pageWidth, pageHeight } })` sets a page from 1 to 15552 points
- Existing documents refuse `pageWidth` or `pageHeight` below about 73 points
- Swatch fields take a swatch name, and style fields a style object
- `appliedFont` takes a `Font`, `"<family>\t<style>"`, or a family keeping `fontStyle`, and on a style reads a `Font`, family name, or `null`
- Absent families throw on an `appliedFont` write, and an absent style writes a missing face

## [04]-[TRANSACTIONS]

Throws inside a step roll back every change in every document and record no undo entry. Plugin top-level `app.doScript(<function>, ScriptLanguage.UXPSCRIPT, …)` can hang InDesign on a callback the fenced plugin thread never runs. ExtendScript source-text calls and function calls nested inside a callback complete. Open menus, dialogs, and drags hold a peer call before its callback runs. Each kind of work takes one form:

| [INDEX] | [WORK]                 | [FORM]                                                                |
| :-----: | :--------------------- | :-------------------------------------------------------------------- |
|  [01]   | Read                   | DOM call, or ExtendScript source text through `doScript`              |
|  [02]   | Write with no UXP code | ExtendScript source text under `ENTIRE_SCRIPT` around a function step |
|  [03]   | Write from a plugin    | Function `doScript` nested in an ExtendScript `$.sleep` call          |
|  [04]   | Savepoint inside write | Function `doScript` nested in that transaction                        |
|  [05]   | Rolled-back read       | ExtendScript source text around a throwing function `doScript`        |
|  [06]   | Write from a script    | Top-level function `doScript` under `ENTIRE_SCRIPT`                   |

`app.doScript` returns plain objects with scripting property keys alone. Throws return as a plain `Error`: "<name>: <message>" from a function, string throws as text, "Uncaught JavaScript exception: <message>" from source. Value and failure leave through a `Deferred`:

```ts
const commit = Effect.fn('commit')(function* <A, E, R>(label: string, body: Effect.Effect<A, E, R>) {
    const { app, SaveOptions, ScriptLanguage, UndoModes } = yield* Dom;
    const [opened, done, sync] = [app.documents.everyItem().id, yield* Deferred.make<A, E | HostError>(), Effect.runSyncWith(yield* Effect.context<R>())];
    yield* Effect.try({ try: (): unknown => app.doScript(() => sync(Effect.andThen(Deferred.into(body, done), Deferred.await(done))), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, label), catch: HostError.refused(label) }).pipe(Effect.catch((error) => Deferred.fail(done, error)));
    return yield* Deferred.await(done).pipe(Effect.onError(() => Effect.forEach(Array.difference(app.documents.everyItem().id, opened), (id) => Effect.sync(() => app.documents.itemByID(id).close(SaveOptions.NO)), { discard: true })));
});
```

`commit` runs in a script context and returns the body's value or rolled-back typed failure. `Deferred.fail` completes `done` only when `doScript` refused before the body ran. Documents a rolled-back step opened close, found by id difference. Plugins hold one step open with repeated ExtendScript `$.sleep` calls until a peer call enters it and runs inside the sleep.

- Nested `doScript` calls are savepoints inside their outer step, where `document.undo()` throws
- `document.undoHistory` lists step names newest first
- Synchronous `doScript` calls after awaited work group the edits after them
- Writes decode every field before their step opens, and a failed decode opens no step
- Throws rolling back a plugin step log `Uncaught JS Exception` at error in the UXP log, and a script step logs none
- Rollback restores documents and app-wide find, change, and export preferences, and leaves `app.scriptPreferences` and files on disk
- `app.modalState` reads true under the Preferences dialog, where a plugin `doScript` drops its field writes with no error
- `app.modalState` reads true under an ExtendScript dialog open through `do script`, where writes take effect, and reads return under either

ExtendScript writes and rolled-back reads send source text with arguments as data, and a write names its function step with the undo name. Values leave the nested function through outer variables. Catches around a nested throw receive no thrown value, and `done`, set before the throw, tells a rollback from a failure:

```ts
import type { InDesignDOMObject } from 'adobe:indesign';

const based = (target: InDesignDOMObject<unknown>, field: string) =>
    Dom.use(({ app, ScriptLanguage }) =>
        Effect.try({
            try: (): unknown =>
                app.doScript(
                    `var target = resolve(arguments[0]), field = arguments[1], value = null, done = false;
try {
    app.doScript(function () { target.clearOverrides(); value = target[field]; done = true; throw 0; }, ScriptLanguage.JAVASCRIPT, [], UndoModes.ENTIRE_SCRIPT);
} catch (error) { if (!done) throw error; }
value`,
                    ScriptLanguage.JAVASCRIPT,
                    [target.toSpecifier(), field],
                ),
            catch: HostError.refused(`${target.toSpecifier()} ${field}`),
        }),
    );
```

`based` clears overrides on `target` in a step its throw rolls back and returns `field` as styles set it.

- Arguments pass primitives, dates, enumerators, DOM objects, and arrays of them at any depth, and sources resolve specifiers
- Argument lists holding a DOM object and a string pass the object in every position (`[app, "fullName"]` as `[app, app]`)
- Sources take specifier strings beside other strings
- Collections and `everyItem()` arrive as a member array or one member, an empty one undefined, and plain objects with scripting property keys alone
- DOM objects a rolled-back step returns lose their specifier, and sources return `toSpecifier()` read inside the step
- ExtendScript assignment of a string to `activePage` crashes InDesign, and a `Page` object assigns

## [05]-[PREFERENCES]

`app.scriptPreferences` and app-wide export and find/change preferences are shared with the user's dialogs and persist across scripts and launches. Export and scripting scopes hold every key they write, from defaults and from caller options an export writes after them.

Find/Change scopes save every settable field reading other than `NothingEnum.NOTHING`, reset preferences to `NOTHING` on acquire and release, and restore saved fields onto the object that release reset returns.

- `MeasurementUnits.POINTS` makes every measurement field read and write points, text size and leading included
- `AutoEnum.AUTO_VALUE` varies units per field by ruler axis and stroke setting, and unit strings (`'9mm'`) parse under every unit
- Served calls and panel actions hold `enableRedraw` false, `measurementUnit` `POINTS`, and `userInteractionLevel` `NEVER_INTERACT`
- Scopes holding app-wide `app.scriptPreferences` run under one permit, where overlapping scopes restore each other's writes
- Reads that change export preferences hold them through `preserve` without a transaction or document undo entry
- Scripts start at `INTERACT_WITH_ALL`, and an uncaught script throw alerts under it and `NEVER_INTERACT`
- `NEVER_INTERACT` turns a PDF font embedding refusal into a silent omission
- `NEVER_INTERACT` completes `createSubsetFont` of a synced face and `menuAction.invoke()` in place of a modal

## [06]-[GEOMETRY]

`resolve` returns one location in a named coordinate space, independent of ruler units and origin. Top-left and bottom-right anchors give an axis-aligned box for rotated, grouped, anchored, and pasteboard items:

```ts
import type { Page, PageItem } from 'adobe:indesign';

class Extent extends Schema.Class<Extent, { readonly brand: unique symbol }>('Extent')(Struct.Record(['top', 'left', 'bottom', 'right'], Schema.Finite)) {
    static readonly of = Effect.fn('Extent.of')((...placed: readonly (Page | PageItem)[]) =>
        Dom.use(({ AnchorPoint, BoundingBoxLimits, CoordinateSpaces }) =>
            Effect.try({
                try: () =>
                    Extent.make(
                        Struct.makeReducer({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax }).combineAll(
                            Array.map(
                                Array.flatMap(Array.cartesian(placed, [AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR]), ([each, anchor]): readonly (readonly [number, number])[] => each.resolve([anchor, BoundingBoxLimits.OUTER_STROKE_BOUNDS, CoordinateSpaces.SPREAD_COORDINATES], CoordinateSpaces.SPREAD_COORDINATES)),
                                ([x, y]) => ({ top: y, left: x, bottom: y, right: x }),
                            ),
                        ),
                    ),
                catch: HostError.refused(`extent of ${Array.map(placed, (each) => each.toSpecifier()).join(', ')}`),
            }),
        ),
    );
}
```

`Extent.of` returns spread points with x 0 at a facing-pages spread's spine and at the center of any other spread. Bounds-space anchor forms read anchored items the two-element form misreads. Folding corners keeps a rotated group's box tight where four corners per item overstate it.

- `page.parent.resolve([[0, 0], page.index], CoordinateSpaces.SPREAD_COORDINATES, true)` locates a page's ruler origin in spread space
- `geometricBounds` orders `[top, left, bottom, right]` in ruler units
- `page.bounds` reads spread-origin points shifted by `zeroPoint` under every ruler origin
- Anchored items move at composition, and `parentStory.recompose()` or `document.recompose()` precedes their read
- `spread.pageItems.everyItem().getElements()` lists front to back, and `page.pageItems` lists its base layer first
- `effectivePpi` reads per item of `allGraphics`, a bulk `everyItem().effectivePpi` returns `undefined`
- `marginPreferences.left` is the inside margin on a facing page, and `page.side` names the side
- Anchor-point `resolve` on a `Spread` reads the spread with its pasteboard, and a spread's pages come from `Extent.of` over those pages

## [07]-[EXPORT]

Export reads app-wide preferences another script left, fails with no throw under `NEVER_INTERACT`, and writes nothing for an input it rejects. `render` holds every key a page, spread, or item export reads and decides success from the written bytes:

```ts
import type { Page, PageItem, Spread } from 'adobe:indesign';

const render = Effect.fn('render')(function* (found: Page | Spread | PageItem, longestSide: number, file: URL, given: Record.ReadonlyRecord<string, unknown>) {
    const [dom, { fs }, path] = yield* Effect.all([Dom, Uxp, Path.Path]);
    const { app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum } = dom;
    const { source, placed, range } = Match.value(found).pipe(
        Match.when(dom.page, (each) => ({ source: each.parent.parent, placed: [each], range: { pageString: `+${each.documentOffset + 1}`, exportingSpread: false } })),
        Match.when(dom.spread, (each) => ({ source: each.parent, placed: each.pages.everyItem().getElements(), range: { pageString: `+${each.pages.firstItem().documentOffset + 1}`, exportingSpread: true } })),
        Match.orElse((each) => ({ source: each, placed: [each], range: {} })),
    );
    const { top, left, bottom, right } = yield* Extent.of(...placed);
    const defaults = {
        pngColorSpace: PNGColorSpaceEnum.RGB,
        pngQuality: PNGQualityEnum.MAXIMUM,
        transparentBackground: false,
        antiAlias: true,
        useDocumentBleeds: false,
        pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
        pngSuffix: '',
        exportingHiddenSpread: true,
        exportResolution: (longestSide * 72) / Math.max(right - left, bottom - top),
        ...range,
    };
    const refused = HostError.refused(`export of ${found.toSpecifier()}`);
    yield* Uxp.preserve(app.pngExportPreferences, defaults, [...Struct.keys(defaults), ...Record.keys(given)]);
    yield* written(app.pngExportPreferences, given);
    const native = yield* path.fromFileUrl(file);
    yield* Effect.try({ try: () => source.exportFile(ExportFormat.PNG_FORMAT, native), catch: refused });
    yield* Effect.filterOrFail(Effect.try({ try: () => fs.readFileSync(file.href).byteLength, catch: refused }), (size) => size > 0, refused);
}, Effect.scoped);
```

`dom.page` and `dom.spread` are `owning` guards (`documentOffset`, `spreadHidden`). `preserve` holds derived keys and every caller option key, and `written` then writes each option. `exportFile` takes the native path `fromFileUrl` returns, and `fs` the `file:` URL. Pixel size is points times `exportResolution` over 72.

- `exportResolution` holds its declared range, a longest side above it clamps to the maximum, and one under it is refused with the side the minimum allows
- Leftover `pageString` from another script makes a one-page export write nothing unless range and page are set together
- `exportResolution` persists across launches, and a render with no caller size holds it at 72, one pixel per point
- `exportFile` on an item ignores the page range and writes nothing for a zero-area item, and `exportingSpread` true exports the whole spread
- Export composes on its own and leaves out hidden, non-printing, and invisible items
- Native writes take any absolute path except a file directly in `/tmp` or `/private/tmp`, which fails with "Cannot find the folder"

Whole items export through `PageItem.exportFile`, pages and spreads through `Document.exportFile`. Native item raster extents differ from geometric bounds, and returned image dimensions describe pixels without a derived coordinate transform. Application screenshots retain dialogs, panels, and pasteboard context.

## [08]-[TEXT]

Text members return a range's first-run value. `textStyleRanges.everyItem().<field>` returns one value per run, a `SpecialCharacters` enumerator for a lone special character:

| [INDEX] | [NEED]                       | [FORM]                                             | [REASON]                                      |
| :-----: | :--------------------------- | :------------------------------------------------- | :-------------------------------------------- |
|  [01]   | Override presence            | `textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE)` | Text, paragraph, word hold it, story lacks it |
|  [02]   | Style applied, overrides cut | `appliedParagraphStyle` assignment                 | Matches `applyParagraphStyle(style)`          |
|  [03]   | Character overrides kept     | `applyParagraphStyle(style, false)`                | Paragraph-level overrides still go            |
|  [04]   | Overset characters           | `story.length` less frame `characters.length`      | `overflows` returns a boolean alone           |
|  [05]   | Anchored object position     | `U+FFFC` in `story.contents`                       | Marker is one character                       |

- GREP styles can leave `appliedCharacterStyle` as `[None]` and keep one `textStyleRange` across different formatting
- Overlapping GREP rules can render differently from character getters, and readers read `appliedNestedStyles` and rule order, then render

## [09]-[GREP]

`findWhat`, `grepExpression`, and `changeTo` take native strings unchanged. Invalid expressions can return no matches without throwing.

| [INDEX] | [PATTERN]                    | [GREP_READS]                           | [GREP_FORM]                              |
| :-----: | :--------------------------- | :------------------------------------- | :--------------------------------------- |
|  [01]   | `\uXXXX`, surrogate pairs    | `\u` as uppercase, pairs match nothing | `\x{XXXX}`, `\x{1F600}`                  |
|  [02]   | `\p{L}`, `\p{N}`, `\p{P}`    | Lowercase alone, nothing, nothing      | `\p{Letter}`, `\p{Number}`               |
|  [03]   | `[a-z\p{Lu}]`                | Property as literal characters         | `(?:[a-z]\|\p{Lu})`                      |
|  [04]   | `^`, `$` inside alternation  | Branch-order-dependent missed matches  | Lookaround per branch                    |
|  [05]   | `^`, `$` without `m`         | Paragraph start and end                | `(?-m)` opening the pattern              |
|  [06]   | `\d`, `\w`, `\b`             | Unicode digits, letters, boundaries    | Native form, ASCII classes when required |
|  [07]   | `i`, `s` flags               | No flags, `.` stops at `\r` alone      | `(?i)`, `(?s)` opening the pattern       |
|  [08]   | `(?<=\$\|USD)`, `(?<!a\|bc)` | Invalid, alternatives differ in length | `(?:(?<=\$)\|(?<=USD))`, `(?<!a)(?<!bc)` |
|  [09]   | `(?<=a+)`                    | Invalid, unbounded lookbehind          | `a+\K` in GREP written directly          |
|  [10]   | `\n`                         | Forced line break U+000A alone         | `\r` ends a paragraph                    |

Searches and changes run in one sequence:
1. Find and change preferences take `NothingEnum.NOTHING`, then native fields set patterns, formatting, and inclusion options
2. `findGrep()` returns `Text` subclasses and zero-length `InsertionPoint` matches
3. Read match text and `paragraphs.everyItem().contents` before changing, with cell and footnote indices counting in their local text
4. `changeGrep()` can return stale ranges or an empty array after changing text, and its length counts no edits

- Change To takes `$0` through `$9`, `\r`, `\t`, `\n`, `\x{XXXX}`, and `\U`, `\u`, `\L`, and inserts `\1`, `$&`, `${1}`, and `$$` as literal text
- `saveFindChangeQuery(name, SearchModes.GREP_SEARCH)` stores application preferences, excluding search scope
- `SearchModes` holds text, GREP, glyph, object, and color modes, and transliterate preferences have no query mode
- Factory queries sit under `<app.filePath>/Presets/Find-Change Queries/<mode>/<$.locale>/` in GREP, object, and text modes
- User queries sit under `Find-Change Queries/<mode>/` beside the parent of `app.scriptPreferences.scriptsFolder`
- `findChangeGrepOptions` holds no case option, and pattern modifiers set case matching
- Locked-layer and locked-story options permit finding alone, and hidden conditional text stays excluded
- `nestedGrepStyles.add({ grepExpression, appliedCharacterStyle })` creates rules on paragraph styles and text objects
- GREP styles inherit through paragraph styles and apply character formatting within paragraphs without changing text
- Find/Change can match across paragraph returns, and find and change formatting stay apart from pattern text

## [10]-[FONTS]

`fontFamily` and `fontStyleName` supply face metadata that splitting `Font.name` can misstate. `location` is a file path or a label for a face with no readable file:

```ts
import type { Font } from 'adobe:indesign';

const source = Effect.fn('source')(function* (face: Font, characters: string) {
    const { FontStatus } = yield* Dom;
    const subset = temporary(`${face.postscriptName}.otf`).pipe(
        Effect.tap((path) => Effect.try({ try: () => face.createSubsetFont(characters, path), catch: HostError.refused(path) })),
        Effect.flatMap(bytes),
    );
    return face.status.equals(FontStatus.INSTALLED) ? yield* Effect.filterOrElse(bytes(face.location), Option.isSome, () => subset) : Option.none();
}, Effect.scoped);
```

`bytes` reads a file at a path as an `Option`, `none` for a missing one, and `source` recovers a missing original through a subset of `characters` under `NEVER_INTERACT`. Permission failures remain failures. `status` decides first: `app.fonts.itemByName` returns a valid specifier for a face InDesign lacks, and its `location` read throws. `postscriptName` selects one face inside a collection file.

- `Font.checkOpenTypeFeature(<OpenTypeFeature member>)` reports GSUB and GPOS features alone, and text applies `morx` features it leaves out
- Variable faces read axes through `designAxesName` and `designAxesValues`, and a subset drops them
- Every `designAxes*` and `numDesignAxes` read of a static face throws, a bulk read holding one included
- Application font rows can repeat equal objects, and document and application fonts for one face can compare unequal
- Point-size changes can reset `opsz`, and size goes before explicit optical-size coordinates

## [11]-[PREFLIGHT]

`document.activeProcess` is one live process per document under its preflight options, and the Preflight panel shows it:
- `activeProcess` throws "Preflighting is turned off for this document." while `preflightOff` is true
- InDesign's idle task completes the live process after a change while InDesign is the active application alone, App Nap disabled or not
- Scripts receive no event marking completion
- `aggregatedResults` throws "Aggregated Result for this process is not available." while running, else reads `[name, profile, rows]`
- Rows read `[depth, label, page, info, [key, value][]]`, depth 1 a category, 2 a rule, and 3 an issue with the object kind as label
- Rows are empty exactly when the process found no issue
- Rule labels hold their issue count ("Overset text (1)")
- `processResults` text and `aggregatedResults` rows name no object specifier
- `preflightWorkingProfile` writes apply to the live process at once
- Profiles resolve by name over `app.preflightProfiles.everyItem().getElements()`, and `itemByName('[Basic]')` reads invalid
- `app.preflightProcesses.add(document, profile)` ignores document preflight options and takes no argument for them
- `waitForProcess()` turns preflight off and on for the application and every open document, and returns false once finished
- `app.preflightProcesses.add` processes finish through `waitForProcess()` with InDesign inactive
- `activeProcess.waitForProcess()` returns false at once and leaves the live process running
- `preflightEmbedWorkingProfile` reads on the application alone, a document's `preflightOptions` throws "Invalid object for this request."

## [12]-[SCRIPTS]

Scripts answer through one `script.setResult` call and fire no timers:

- Script bodies run as one `ENTIRE_SCRIPT` function step named by the label its caller passes, where a throw rolls back and an outer variable keeps its thrown record
- Clients find each install through Launch Services as an application that opens an `.idjs` URL, and send each request as a `do script` event in `uxpscript`
- Runners switch to async mode for source holding `async ` before a function or `await ` before an expression, and reply empty before the body runs
- Runner tests match `async ` and `await ` inside strings and comments, and modules the entry requires escape the test
- `.idjs` entries sent as a file URL run a `require('./main.js')` bundle beside them synchronously
- `app.doScript(<source>, ScriptLanguage.UXPSCRIPT)` returns `undefined` at once for source the runner reads as asynchronous
- Large text source runs nothing, and text source loads a bundle through `new Function('require', readFileSync(path, { encoding: 'utf-8' }))(require)`
- `fullName`, `filePath`, and `File`-typed fields return promises, where ExtendScript `resolve(<specifier>)[<field>].fsName` reads each synchronously
- `getTemporaryFolder()` returns a promise, where ExtendScript `Folder.temp.fsName` names the user temporary folder
- Listeners a script registers fire after its context ended, and reading `handler` or `properties` of one crashes InDesign
- DOM calls from a continuation after a script returned crash InDesign, and quit events served while a script runs abort it

## [13]-[PLUGINS]

InDesign runs `main` of every enabled plugin at launch, panel shown or hidden, and timers fire while it is hidden:
- `host.app` other than `ID` rejects the folder, and panels become Window menu actions where commands and shortcuts bind none
- Floating panel tabs show `label` alone
- `featureFlags.uncaughtException` delivers uncaught exceptions as events beside `unhandledRejection`
- `~/Library/Logs/Adobe/Adobe InDesign/UXPLogs_*.log` records each load and each rejection neither flag delivers
- `addEventListener(<type>, <handler>)` checks `<type>` against the target's `events`, and typings take `InDesignEventMap` keys or any string
- `adobe:indesign` exports no `InDesignEventMap`, and listeners pass its camelCase keys (`afterSelectionChanged`) as literals
- Selection views listen to `afterSelectionChanged`, `afterSelectionAttributeChanged` (same objects), and `afterContextChanged` (another front document)
- `addEventListener` returns the listener an equal type and handler pair holds, and fails 30625 with `captures` true
- Removing another context's `app.addEventListener` listener reads its `id` and `eventType` alone
- Unload calls panel handlers with the main thread held in `PluginRuntime::destroy`, where a DOM call, a scope release included, deadlocks InDesign
- Debugger sessions close before those calls, so a breakpoint in a panel handler never pauses an unload
- Listeners register as `<plugin id>:<event>`, each registration removing the listener `app.eventListeners.itemByName` finds valid, and hold no release
- Listeners of an unloaded context stay in `app.eventListeners`, and events firing on them leave later loads and quit unaffected
- Panel menu toggles call neither `show` nor `hide`, and listeners of a hidden panel keep running their work
- Menu actions read `id`, `name`, and `area` in bulk
- `enabled` or `checked` of Character panel actions 26996 to 27015 crashes InDesign, a bulk read included
- Once a user `ID.json` registry exists InDesign loads its rows alone, and an absolute row `path` loads nothing
- `UnifiedPluginInstallerAgent --remove` against a running InDesign can hang it in `PluginRuntime::destroy`, and removals run with InDesign quit

Folders join a running InDesign for one session from a script:
1. `pluginManager.createPlugin(folder, pluginManager.PLUGIN_TYPE_THIRD_PARTY_V2)` returns the plugin object
2. `pluginManager.addPlugin(plugin, true)` returns `2` for success
3. `plugin.showPanel(<panel id>)` from a `.then` callback starts its runtime once the script returned
4. InDesign restart loads new code for the id, quit unloads the plugin, and `plugin.runtime.load()` or `disablePlugin` during DOM work stops InDesign
