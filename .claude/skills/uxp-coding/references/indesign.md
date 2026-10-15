# [INDESIGN]

Each InDesign member read and call is one host round trip that returns before the next statement, and a collection returns one field of every member in one call. Typings declare DOM classes as interfaces, and code discriminates instances on their literal `constructorName` field.

## [01]-[MEMBERS]

InDesign holds its running object model as `$.dictionary`, and one ExtendScript reader sent through `do script` prints each class:

```bash
osascript -e 'on run argv' \
    -e 'tell application id "com.adobe.InDesign" to do script (item 1 of argv) language javascript with arguments {item 2 of argv}' \
    -e 'end run' "$(cat <<'JSX'
var model = $.dictionary.getClass(arguments[0]).toXML(), rows = [];
function typed(row) {
    var type = row.datatype;
    return (String(type.type) || "void") + (type.array.length() ? "[]" : "") + (type.value.length() ? " = " + type.value : "") +
        (type.min.length() ? " (" + type.min + " to " + type.max + ")" : "");
}
function listed(list, each) { var out = []; for (var index = 0; index < list.length(); index++) out.push(each(list[index])); return out; }
listed(model.elements, function (element) {
    return listed(element.*, function (row) {
        var call = row.localName() == "method";
        var parameters = listed(row.parameters.parameter, function (parameter) {
            return parameter.@name + (parameter.@optional == "true" ? "?" : "") + ": " + typed(parameter);
        });
        var signature = (element.@type == "class" ? "static " : "") + row.@name + (call ? "(" + parameters.join(", ") + ")" : "");
        rows.push([signature + ": " + typed(row), call ? "method" : String(row.@rwaccess) || "readwrite", row.shortdesc].join("\t"));
    });
});
rows.join("\n");
JSX
)" <Class>
```

```text
static CENTER_ALIGN: number = 1667591796	readonly	Center aligns the text.
tintValue: number (0 to 100)	readwrite	The percent of the base color. (Range: 0 to 100)
```

- `$.dictionary.toXML().descendants("topicref").@href` lists every class and enumeration name, each equal to the `indesign` module's
- Rows include inherited members, and `static` rows are enumerators and class constants
- `varies=any` rows list their types in a closing "Can accept:" or "Can return:" clause, and "(Optional)" marks a parameter optional with no default
- Reserved-word parameters are `in`, `with`, and `from`
- `Object.getOwnPropertyDescriptors(indesign.<Class>.prototype)` lists what the installed build exposes, getters as fields, and the rows type them
- `textHasOverrides` and `createTemporaryCopy` sit on prototypes alone, outside the object model
- `for each` over an `XMLList` yields unnamed rows, and readers index the list
- Plug-ins send a reader as source text through `app.doScript(<source>, ScriptLanguage.JAVASCRIPT, [name])`

## [02]-[SPECIFIERS]

Collection members are specifiers that resolve at each member read, and an unmatched lookup returns a specifier with `isValid` false:

```ts
import indesign, { app, type BaseCollection, type Document, type PageItem, ScriptLanguage } from 'adobe:indesign';

interface Hosted {
    readonly constructorName: string;
    readonly toSpecifier: () => string;
}

const owning = (member: string): ReadonlySet<unknown> =>
    new Set(
        Array.flatMap(Object.getOwnPropertyNames(indesign), (name): readonly unknown[] => {
            const type: unknown = Reflect.get(indesign, name);
            return Predicate.isFunction(type) && Object.hasOwn(type.prototype, member) ? [type.prototype] : [];
        }),
    );
const [objects, collections] = [owning('toSpecifier'), owning('everyItem')];
const hosted = (value: unknown): value is Hosted => Predicate.isObject(value) && objects.has(Object.getPrototypeOf(value));
const collected = (value: unknown): value is BaseCollection<unknown> => Predicate.isObject(value) && collections.has(Object.getPrototypeOf(value));
const located = (document: Document, id: number): Option.Option<PageItem> => {
    const found = document.pageItems.itemByID(id);
    return found.isValid ? Array.head(found.getElements()) : Option.none();
};
const resolved = (specifier: string): Option.Option<Hosted | readonly Hosted[]> =>
    Option.filter(
        Option.liftThrowable((): unknown => app.doScript('resolve(arguments[0])', ScriptLanguage.JAVASCRIPT, [specifier]))(),
        (found): found is Hosted | readonly Hosted[] => hosted(found) || (Array.isArray(found) && Array.every(found, hosted)),
    );
const item = (target: unknown, key: PropertyKey): unknown =>
    collected(target) && !Predicate.isSymbol(key) && !(key in target) ? target.item(key) : Reflect.get(new Object(target), key);
```

`getElements()` returns each member as its concrete class and throws on an invalid specifier. Reading a native getter (`constructorName`, `length`) with a class prototype as receiver crashes InDesign. `hosted` and `collected` read no getter: a DOM object's prototype is an exported class prototype owning `toSpecifier`, and a collection's owns `everyItem`. `resolved` turns any `toSpecifier()` string (ids, `[@name]` selectors, group paths, text ranges, collections) back into its object through ExtendScript's `resolve`, with the specifier passed as an argument. Supplied source compiles with `item` as its computed member read: a collection lacking the key returns `item(key)`, and arrays, records, and `length` keep their own.

| [INDEX] | [NEED]                     | [FORM]                                           | [DECIDING_FACT]                                |
| :-----: | :------------------------- | :----------------------------------------------- | :--------------------------------------------- |
|  [01]   | Member by position or name | `item(index)`, negative from the end, or a name  | `collection[index]` reads `undefined`          |
|  [02]   | Every member as an array   | `everyItem().getElements()`                      | Collections have no iterator                   |
|  [03]   | One field of every member  | `everyItem().<field>`, one call per field        | `everyItem().properties` reads every field     |
|  [04]   | Same object across reads   | `equals`                                         | Repeated reads differ under `===`              |
|  [05]   | Address held across calls  | `id`                                             | Positions shift on insert, `item(0)` is newest |
|  [06]   | Address of a text range    | `toSpecifier()`, then `resolved`                 | `Character`, `Word`, `Paragraph` have no `id`  |
|  [07]   | Style inside a group       | Specifier steps per group                        | Groups can repeat one style name               |
|  [08]   | Active document            | `Option.liftThrowable` over `app.activeDocument` | Throws while every open document is windowless |

- Text specifiers are index-based and name another range after an edit before them, and `parentPage` reads `null` on the pasteboard
- Parents discriminate on `constructorName` (`Character` for an anchored item, `Spread` or `MasterSpread` for a top-level one)
- ExtendScript run through `doScript` keeps its own `scriptPreferences`, and reads and writes go through the object `resolved` returns

## [03]-[FIELDS]

Enumerators are objects: `===` between reads of one member is false, `JSON.stringify` yields `{}`, and `String` yields the member name. Each `Enumeration` holds every member under upper snake, camel, and lowercase keys (`CENTER_ALIGN`, `centerAlign`, `centeralign`), and one field write form checks each GREP field and retries a refused string as the enumerator of that name:

```ts
import indesign, { type Enumerator } from 'adobe:indesign';

const { Enumeration } = indesign;

const enumerations: Record.ReadonlyRecord<string, typeof Enumeration> = Record.filter(
    Record.fromIterableWith(Object.getOwnPropertyNames(indesign), (name) => [name, Reflect.get(indesign, name)]),
    (value): value is typeof Enumeration => value instanceof Enumeration,
);
const members = (enumeration: typeof Enumeration): string[] =>
    Array.filter(Object.getOwnPropertyNames(Object.getPrototypeOf(enumeration)), (key) => String(Reflect.get(enumeration, key)) === key);
const member = (enumeration: typeof Enumeration, name: string): Option.Option<Enumerator> =>
    Option.filter(Option.fromNullishOr<unknown>(Reflect.get(enumeration, name)), (value) => value instanceof indesign.Enumerator);

const UNSUPPORTED = /\$<|\$\d{2}/u;
const REFERENCE = /\$(?<sign>[$&])/gu;

class FieldError extends Schema.TaggedError<FieldError>()('FieldError', { field: Schema.String, cause: Schema.Defect() }) {}

const pattern = (value: unknown): Effect.Effect<unknown, PatternError> =>
    Predicate.isRegExp(value) ? Effect.fromResult(grep(value)) : Effect.succeed(value);
const replacement = (value: unknown): Effect.Effect<unknown, PatternError> =>
    Predicate.isString(value) && UNSUPPORTED.test(value)
        ? Effect.fail(new PatternError({ pattern: value, constructs: [value] }))
        : Effect.succeed(Predicate.isString(value) ? value.replace(REFERENCE, (_, sign: string) => (sign === '$' ? '\\$' : '$0')) : value);
const checks: Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, (value: unknown) => Effect.Effect<unknown, PatternError>>> = {
    FindGrepPreference: { findWhat: pattern },
    ChangeGrepPreference: { changeTo: replacement },
    NestedGrepStyle: { grepExpression: pattern },
};
const written = (target: { readonly constructorName: string }, values: object): Effect.Effect<void, Array.NonEmptyArray<FieldError | PatternError>> =>
    Effect.validate(
        Object.entries(values),
        ([field, given]: readonly [string, unknown]) => {
            const put = (value: unknown): Effect.Effect<unknown, FieldError> =>
                Effect.try({ try: () => Object.assign(target, { [field]: value }), catch: (cause) => new FieldError({ field, cause }) });
            const named = (value: unknown): readonly Effect.Effect<unknown, FieldError>[] =>
                Array.map(
                    Predicate.isString(value) ? Array.flatMap(Record.values(enumerations), (each) => Option.toArray(member(each, value))) : [],
                    put,
                );
            const checked = Option.match(Option.flatMap(Record.get(checks, target.constructorName), Record.get(field)), {
                onNone: () => Effect.succeed(given),
                onSome: (check) => check(given),
            });
            return Effect.flatMap(checked, (value) =>
                Effect.catch(put(value), (refused) => Effect.firstSuccessOf([...named(value), Effect.fail(refused)])),
            );
        },
        { discard: true },
    );
```

`members` keeps each member's canonical spelling, the one key equal to its `String`. Refusals throw one field at a time with the host's expected type ("Expected Unit or Leading enumerator"), and the host's type check picks the enumeration: `justification: 'centerAlign'` writes `Justification.CENTER_ALIGN`. `replacement` maps JavaScript's `$&` and `$$` to Change To's `$0` and `\$` and refuses named and two-digit references. Values leave through a `JSON.stringify` replacer that tests enumerators before strings, since a text read can return a `SpecialCharacters` enumerator, and writes each object `hosted` accepts as class, specifier, and `isValid`.

- New members take `add()` with required arguments, then `written` fields, since `add(record)` and `properties` drop keys they refuse
- Swatch fields take a swatch name, style fields a style object alone, and font fields `"<family>\t<style>"`, silently keeping the old face on failure
- Enumerators compare through `Justification.CENTER_ALIGN.equals(value)`

## [04]-[TRANSACTIONS]

Throws inside a step roll back every change in every document and record no undo entry. A plug-in's top-level `app.doScript(<function>, ScriptLanguage.UXPSCRIPT, …)` can lose its hand-off: main thread waits for a callback that never runs, plug-in thread blocks on a fence, and InDesign hangs until killed. ExtendScript source-text `doScript` calls and calls nested inside a callback never hung. Peer calls a plug-in serves during an ExtendScript `$.sleep` enter a nested function `doScript`, and open menus, dialogs, and drags hold it before its callback runs. Each kind of work takes one form:

| [INDEX] | [WORK]                 | [FORM]                                                                |
| :-----: | :--------------------- | :-------------------------------------------------------------------- |
|  [01]   | Read                   | DOM call, or ExtendScript source text through `doScript`              |
|  [02]   | Write with no UXP code | ExtendScript source text under `ENTIRE_SCRIPT` around a function step |
|  [03]   | Write running UXP code | Function `doScript` nested in an ExtendScript `$.sleep` call          |
|  [04]   | Savepoint inside write | Function `doScript` nested in that transaction                        |
|  [05]   | Rolled-back read       | ExtendScript source text around a throwing function `doScript`        |

`app.doScript` filters returned objects to scripting keys and rethrows a new `Error` without the original. Value and failure leave through a `Deferred`:

```ts
import { app, SaveOptions, ScriptLanguage, UndoModes } from 'adobe:indesign';

const StepReason = Schema.TaggedUnion({ Modal: {}, Refused: { cause: Schema.Defect() } });
class StepError extends Schema.TaggedError<StepError>()('StepError', { step: Schema.String, reason: StepReason }) {}

const transacted = Effect.fn('transacted')(function* <A, E, R>(step: string, preview: boolean, body: Effect.Effect<A, E, R>) {
    yield* Effect.filterOrFail(
        Effect.sync(() => app.modalState),
        (modal) => !modal,
        () => new StepError({ step, reason: { _tag: 'Modal' } }),
    );
    const [services, outcome, opened] = [yield* Effect.context<R>(), yield* Deferred.make<A, E | StepError>(), app.documents.everyItem().id];
    const recorded = Effect.flatMap(
        Effect.tap(Effect.exit(body), (exit) => Deferred.done(outcome, exit)),
        (exit) => (preview ? Effect.interrupt : exit),
    );
    const run = (): A => Effect.runSyncWith(services)(recorded);
    const close = Effect.sync(() =>
        Array.forEach(Array.difference(app.documents.everyItem().id, opened), (id) => app.documents.itemByID(id).close(SaveOptions.NO)),
    );
    yield* Effect.catch(
        entered(() => app.doScript(run, ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, step)),
        (error) => Effect.andThen(Deferred.fail(outcome, error), close),
    );
    return yield* Deferred.await(outcome);
});
```

`entered` runs its step inside a peer call the plug-in serves during an ExtendScript `$.sleep`, a host refusal failing `Refused`. `transacted` returns the body's value, or its typed failure with every change rolled back. Under `preview` the body's value returns and the interruption rolls the document back with no undo step: a dry run, a rendered preview, or a host refusal reported as data. `Deferred.fail` completes `outcome` only when `doScript` refused before the body ran, and documents a rolled-back step opened close by id difference. Bodies run on the caller's services, which a bare `Effect.runSync` drops, and an async body fails `StepError`. `transacted` refuses with `Modal` before its `doScript` while `app.modalState` reads true.

- Nested `doScript` calls are savepoints inside their outer step
- Rollback restores documents and app-wide find, change, and export preferences, and leaves `app.scriptPreferences` and files on disk
- `document.undo()` inside a step throws, and `document.undoHistory` lists step names newest first
- Thrown lines are read inside the `catch` that first sees the throw, before `doScript` rethrows
- With the Preferences dialog open, `app.modalState` reads true and a plug-in `doScript` runs with its field writes dropped and no error
- With an ExtendScript dialog shown through `do script` open, `app.modalState` reads true and plug-in field writes take effect
- Reads return under either dialog

ExtendScript writes and rolled-back reads send source text with arguments as data, and a write names its function step with the undo name. Values leave the nested function through outer variables. A catch around a nested throw receives no thrown value, and `done`, set before the throw, tells a rollback from a failure:

```ts
const based = (target: Hosted, field: string): Effect.Effect<unknown, StepError> => {
    const source = [
        'var target = resolve(arguments[0]), field = arguments[1], value = null, done = false;',
        'try {',
        '    app.doScript(',
        '        function () { target.clearOverrides(); value = target[field]; done = true; throw 0; },',
        '        ScriptLanguage.JAVASCRIPT, [], UndoModes.ENTIRE_SCRIPT);',
        '} catch (error) { if (!done) throw error; }',
        'value',
    ].join('\n');
    return Effect.try({
        try: (): unknown => app.doScript(source, ScriptLanguage.JAVASCRIPT, [target.toSpecifier(), field]),
        catch: (cause) => new StepError({ step: 'Clear Overrides', reason: { _tag: 'Refused', cause } }),
    });
};
```

`based` clears overrides on `target` in a step its throw rolls back and returns `field` as styles set it.

- Arguments pass primitives, enumerators, single DOM objects, and nested arrays of primitives, and sources resolve specifiers
- Empty arrays, collections, and `everyItem()` arrive undefined, plain objects arrive empty, and DOM object arrays lack `length` and index
- DOM objects a rolled-back step returns lose their specifier, and sources return `toSpecifier()` read inside the step
- ExtendScript assignment of a string to `activePage` crashes InDesign, and a `Page` object assigns

## [05]-[PREFERENCES]

`app.scriptPreferences` and app-wide export and find/change preferences are shared with the user's dialogs and persist across scripts and launches, and one scope restores the keys it wrote:

```ts
const held = <T extends { readonly constructorName: string }, K extends keyof T & string>(
    owner: T,
    values: Pick<T, K>,
): Effect.Effect<void, Array.NonEmptyArray<FieldError | PatternError>, Scope.Scope> =>
    Effect.andThen(
        Effect.acquireRelease(
            Effect.sync(() => Record.fromIterableWith(Struct.keys(values), (key) => [key, owner[key]])),
            (saved) => Effect.sync(() => Object.assign(owner, saved)),
        ),
        written(owner, values),
    );
```

Preference members are prototype accessors that own-key copies (`Struct.pick`, a spread) skip. `held` reads each key by name before `written` sets any, and the scope restores every key whether its write succeeded or failed.

- `MeasurementUnits.POINTS` makes every measurement field read and write points, text size and leading included
- `AutoEnum.AUTO_VALUE` varies units per field by ruler axis and stroke setting, and unit strings (`'9mm'`) parse under every unit
- App-wide preference writes outside a step set `app.undoName` alone and leave `document.undoHistory` unchanged
- Reads that change export or find preferences hold them through `held` with no transaction
- Scripts start at `INTERACT_WITH_ALL`
- `NEVER_INTERACT` turns a PDF font embedding refusal into a silent omission, and an uncaught script throw alerts under both levels
- `NEVER_INTERACT` makes `createSubsetFont` of a synced face and `menuAction.invoke()` complete in place of opening a modal

## [06]-[GEOMETRY]

`resolve` returns one location in a named coordinate space independent of ruler units and origin, and its top-left and bottom-right anchors give an axis-aligned box for rotated, grouped, anchored, and pasteboard items:

```ts
import { AnchorPoint, BoundingBoxLimits, CoordinateSpaces, type Page, type PageItem } from 'adobe:indesign';

interface Box {
    readonly top: number;
    readonly left: number;
    readonly bottom: number;
    readonly right: number;
}

const extent = (...located: readonly (Page | PageItem)[]): Box =>
    Struct.makeReducer<Box>({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax }).combineAll(
        Array.flatMap(located, (each) =>
            Array.flatMap([AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR], (anchor) =>
                Array.map(
                    each.resolve(
                        [anchor, BoundingBoxLimits.OUTER_STROKE_BOUNDS, CoordinateSpaces.SPREAD_COORDINATES],
                        CoordinateSpaces.SPREAD_COORDINATES,
                    ),
                    ([x, y]) => ({ top: y, left: x, bottom: y, right: x }),
                ),
            ),
        ),
    );
```

`extent` returns spread points about the spread's center. The bounds-space anchor form reads anchored items the two-element form misreads, and folding corners keeps a rotated group's box tight where four corners per item overstate it.

- `page.parent.resolve([[0, 0], page.index], CoordinateSpaces.SPREAD_COORDINATES, true)` locates a page's ruler origin in spread space
- `geometricBounds` orders `[top, left, bottom, right]` in ruler units, and `page.bounds` ignores ruler origin and spread center alike
- Anchored items move at composition, and `parentStory.recompose()` or `document.recompose()` precedes their read
- `spread.pageItems.everyItem().getElements()` lists front to back, and `page.pageItems` lists its base layer first
- `effectivePpi` reads per item of `allGraphics`, a bulk `everyItem().effectivePpi` returns `undefined`
- `marginPreferences.left` is the inside margin on a facing page, and `page.side` names the side
- Anchor-point `resolve` on a `Spread` reads the spread with its pasteboard, and a spread's pages come from `extent` over those pages

## [07]-[EXPORT]

Export reads app-wide preferences another script left, fails with no throw under `NEVER_INTERACT`, and writes nothing for an input it rejects. `rendered` holds every key export reads and decides success from the written bytes:

```ts
import fs from 'adobe:fs';
import { app, type Document, ExportFormat, ExportRangeOrAllPages, MeasurementUnits, UserInteractionLevels } from 'adobe:indesign';

class Unrendered extends Schema.TaggedError<Unrendered>()('Unrendered', { page: Schema.Int }) {}

const rendered = Effect.fn('rendered')(function* (
    document: Document,
    page: number,
    longPixels: number,
    file: { readonly native: string; readonly read: string },
) {
    yield* held(app.scriptPreferences, {
        measurementUnit: MeasurementUnits.POINTS,
        userInteractionLevel: UserInteractionLevels.NEVER_INTERACT,
        enableRedraw: false,
    });
    const target = yield* Effect.filterOrFail(
        Effect.sync(() => document.pages.item(page)),
        ({ isValid }) => isValid,
        () => new Unrendered({ page }),
    );
    const { top, left, bottom, right } = extent(target);
    const pointsPerInch = 72;
    yield* held(app.jpegExportPreferences, {
        jpegExportRange: ExportRangeOrAllPages.EXPORT_RANGE,
        pageString: `+${page + 1}`,
        exportingSpread: false,
        exportResolution: (longPixels * pointsPerInch) / Math.max(bottom - top, right - left),
        useDocumentBleeds: false,
    });
    fs.writeFileSync(file.read, '');
    document.exportFile(ExportFormat.JPG, file.native);
    return yield* Effect.filterOrFail(
        Effect.sync(() => new Uint8Array(fs.readFileSync(file.read))),
        (image) => image.length > 0,
        () => new Unrendered({ page }),
    );
}, Effect.scoped);
```

`rendered` runs on its own or inside a `transacted` preview, where rollback discards the rendered state. `file` names one file for `exportFile` and for `fs`, in a plug-in `<getTemporaryFolder().nativePath>page.jpg` and `plugin-temp:/page.jpg` with the folder resolved before `rendered` runs. Emptying the file first makes zero bytes the one sign of a failed render, and pixel size is points times `exportResolution` (1 to 2400) over 72.

- Leftover `pageString` from another script makes a one-page export write nothing unless range and page are set together
- `exportFile` on an item ignores the page range and writes nothing for a zero-area item, and `exportingSpread` true exports the whole spread
- Export composes on its own and leaves out hidden, non-printing, and invisible items
- Native writes take any absolute path except a file directly in `/tmp`

Regions export from a scratch document that matches what InDesign draws:
1. `app.documents.add(false)` opens a windowless document, sized through `documentPreferences.properties`, with a layer per source layer
2. Master items come first: pages in descending index, each page's `masterPageItems` reversed and deduplicated by `id`
3. Copies keep their spread-center offset, and `duplicate(layer, by)` of a master item takes `by` as its page's `extent` corner less its master page's
4. Shown top-level spread items follow back to front, each `duplicate` going to `layers.item(item.itemLayer.index)`
5. `rendered` exports the scratch page, and `close(SaveOptions.NO)` releases the document

## [08]-[TEXT]

Text members return a range's first-run value, and `textStyleRanges.everyItem().<field>` returns one value per run, a `SpecialCharacters` enumerator for a run of one special character:

| [INDEX] | [NEED]                       | [FORM]                                             | [DECIDING_FACT]                               |
| :-----: | :--------------------------- | :------------------------------------------------- | :-------------------------------------------- |
|  [01]   | Override presence            | `textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE)` | Text, paragraph, word hold it, story lacks it |
|  [02]   | Style applied, overrides cut | `appliedParagraphStyle` assignment                 | Matches `applyParagraphStyle(style)`          |
|  [03]   | Character overrides kept     | `applyParagraphStyle(style, false)`                | Paragraph-level overrides still go            |
|  [04]   | Overset characters           | `story.length` less frame `characters.length`      | `overflows` returns a boolean alone           |
|  [05]   | Anchored object position     | `U+FFFC` in `story.contents`                       | Marker is one character                       |

- Characters a GREP style matches read the resolved attribute while `appliedCharacterStyle` stays `[None]` and `textStyleRanges` keeps one run
- `checkOpenTypeFeature` reports GSUB and GPOS features alone

## [09]-[GREP]

InDesign's GREP dialect reads some JavaScript pattern syntax with another meaning, and finds nothing with no error for a pattern it cannot parse. A `RegExp` translates through a parser: each node the dialect misreads becomes one splice of the source at the node's range, and every construct with no dialect form fails the translation:

```ts
import MagicString from 'magic-string';
import regjsparser, { type AstNode } from 'regjsparser';

class PatternError extends Schema.TaggedError<PatternError>()('PatternError', {
    pattern: Schema.String,
    constructs: Schema.NonEmptyArray(Schema.String),
}) {}

interface Splice {
    readonly start: number;
    readonly end: number;
    readonly text: string;
}

const features = { lookbehind: true, namedGroups: true, unicodePropertyEscape: true, unicodeSet: true } as const;
const hex = (point: number): string => {
    const radix = 16;
    return `\\x{${point.toString(radix).toUpperCase()}}`;
};
const splices = (node: AstNode<typeof features>): readonly Result.Result<Splice, string>[] => {
    const [start, end] = node.range;
    switch (node.type) {
        case 'value':
            return node.kind === 'unicodeEscape' || String.fromCodePoint(node.codePoint).length > 1
                ? [Result.succeed({ start, end, text: hex(node.codePoint) })]
                : [];
        case 'group':
            return node.behavior.endsWith('ookbehind') && Array.some(node.body, (child) => child.type === 'quantifier' && child.min !== child.max)
                ? [Result.fail(node.raw)]
                : Array.flatMap(node.body, splices);
        case 'alternative':
        case 'disjunction':
        case 'quantifier':
            return Array.flatMap(node.body, splices);
        default:
            return [];
    }
};
const grep = ({ source, flags, ignoreCase, multiline }: RegExp): Result.Result<string, PatternError> => {
    const rows = Result.mapError(
        Result.try(() => splices(regjsparser.parse(source, flags.includes('v') ? 'v' : 'u', features))),
        () => new PatternError({ pattern: source, constructs: [source] }),
    );
    const spliced = (kept: readonly Result.Result<Splice, string>[]): string =>
        Array.reduce(Array.getSuccesses(kept), new MagicString(source), (edited, { start, end, text }) => edited.overwrite(start, end, text))
            .toString();
    return Result.flatMap(rows, (kept) =>
        Array.match(Array.getFailures(kept), {
            onNonEmpty: (constructs) => Result.fail(new PatternError({ pattern: source, constructs })),
            onEmpty: () => Result.succeed(`${ignoreCase ? '(?i)' : ''}${multiline ? '' : '(?-m)'}${spliced(kept)}`),
        }),
    );
};
```

`splices` holds one rewrite (an escaped or astral code point as `\x{…}`) and one rejection (a lookbehind of unbounded length), and each table row is one more `case` of its switch. Rows the dialect cannot spell as written (a script, a property inside a class, `\d`) take a class of code points built from the runtime's Unicode data under the pattern's flags, one range boundary per membership change. A lookbehind's fixed width is the sum of its children's widths, a quantifier counting when `min` equals `max`.

| [INDEX] | [JAVASCRIPT]                  | [GREP_READS]                              | [GREP_FORM]                              |
| :-----: | :---------------------------- | :---------------------------------------- | :--------------------------------------- |
|  [01]   | `\uXXXX`, surrogate pairs     | `\u` as uppercase, pairs match nothing    | `\x{XXXX}`, `\x{1F600}`                  |
|  [02]   | `\p{L}`, `\p{N}`, `\p{P}`     | Lowercase alone, nothing, nothing         | `\p{Letter}`, `\p{Number}`               |
|  [03]   | `\p{Script=X}`, `[a-z\p{Lu}]` | Nothing, property as literal characters   | Class of code points                     |
|  [04]   | `^`, `$` inside alternation   | Anchor binds every alternative            | Lookaround per branch                    |
|  [05]   | `^`, `$` without `m`          | Paragraph start and end                   | `(?-m)` opening the pattern              |
|  [06]   | `\d`, `\w`, `\b`              | Unicode digits, letters, boundaries       | ASCII classes and lookarounds            |
|  [07]   | `i`, `s` flags                | No flags, `.` stops at `\r` alone         | `(?i)`, `(?s)` opening the pattern       |
|  [08]   | `(?<=\$\|USD)`, `(?<!a\|bc)`  | Invalid, alternatives differ in length    | `(?:(?<=\$)\|(?<=USD))`, `(?<!a)(?<!bc)` |
|  [09]   | `(?<=a+)`                     | Invalid, unbounded lookbehind             | `a+\K` in GREP written directly          |
|  [10]   | `\n`                          | Forced line break U+000A alone            | `\r` ends a paragraph                    |

Searches and changes run in one sequence:
1. Find, change, and options preferences take `NothingEnum.NOTHING`, then `written` sets every key the search reads
2. `findGrep()` of `(?:<pattern>)|<sentinel>` over a scratch story holding `<sentinel>`, as a rolled-back read, hits exactly when `<pattern>` parses
3. `findGrep()` returns `Text` subclasses with `contents`, `index`, and `parentStory`, zero-length `InsertionPoint` matches included
4. `changeGrep()` returns ranges that later changes in their story leave stale, and changed text reads from the story's paragraphs

- Change To takes `$0` through `$9`, `\r`, `\t`, `\n`, `\x{XXXX}`, and `\U`, `\u`, `\L`, and inserts `\1`, `$&`, and `${1}` as literal text
- `saveFindChangeQuery(name, SearchModes.GREP_SEARCH)` stores queries `loadFindChangeQuery` restores, and `findChangeGrepOptions` holds no case option
- GREP styles are `nestedGrepStyles.add()` on a paragraph style, then `written` with `grepExpression` and `appliedCharacterStyle`

## [10]-[FONTS]

`Font.name` is family and style joined by a tab, `location` is a file path or a label for a face with no readable file, and `createSubsetFont` writes a single-face file for any installed face:

```ts
import { readFileSync } from 'adobe:fs';
import { app, FontStatus } from 'adobe:indesign';

const FaceReason = Schema.TaggedUnion({ Unavailable: { name: Schema.String }, Unwritten: { file: Schema.String, cause: Schema.Defect() } });
class FaceError extends Schema.TaggedError<FaceError>()('FaceError', { reason: FaceReason }) {}

const face = Effect.fn('face')(function* (family: string, style: string, characters: string, folder: string) {
    const name = `${family}\t${style}`;
    const font = yield* Effect.filterOrFail(
        Effect.sync(() => app.fonts.itemByName(name)),
        ({ status }) => !status.equals(FontStatus.NOT_AVAILABLE),
        () => new FaceError({ reason: { _tag: 'Unavailable', name } }),
    );
    const [{ location, postscriptName }, subset] = [font, `${folder}/${font.postscriptName}`];
    const unwritten = (file: string) => (cause: unknown): FaceError => new FaceError({ reason: { _tag: 'Unwritten', file, cause } });
    const file = yield* location.startsWith('/')
        ? Effect.succeed(location)
        : Effect.as(Effect.try({ try: () => font.createSubsetFont(characters, subset), catch: unwritten(subset) }), subset);
    const bytes = yield* Effect.try({ try: () => new Uint8Array(readFileSync(file)), catch: unwritten(file) });
    return { bytes, postscriptName, subset: file === subset };
});
```

`face` returns a file face's bytes, and an Adobe Fonts or system face ("Added from Adobe Fonts", "System Font") through a subset of `characters` under `NEVER_INTERACT`. `status` decides first, since `itemByName` returns a valid specifier for a face InDesign lacks, and its `location` read throws. `postscriptName` selects one face inside a collection file.

- Splitting `name` at its tab gives family and style from one bulk read, while `everyItem().location` across every face times out an Apple event
- Variable faces read axes through `designAxesName` and `designAxesValues`, and a subset drops them

## [11]-[PREFLIGHT]

Preflight runs as a process object over one document and one profile:
- Profiles resolve by name over `app.preflightProfiles.everyItem().getElements()`, since `itemByName('[Basic]')` reads invalid
- `app.preflightProcesses.add(document, profile)` starts a process that a scope releases through `remove()` on every exit
- `waitForProcess(seconds)` can return false with results complete, and a decode of `aggregatedResults` decides
- `aggregatedResults` is `[name, profile, rows]`, each row `[node, label, page, text, [key, value][]]` with category rows first and an empty page

## [12]-[SCRIPTS]

Scripts return through one `script.setResult` call and fire no timers, and each is one synchronous expression with one translation of its `Exit`:

```ts
import uxp from 'adobe:uxp';

const Script = Schema.Struct({ args: Schema.Array(Schema.String), setResult: Schema.declare(Predicate.isFunction) });
const scripted = <A, E>(work: (args: readonly string[]) => Effect.Effect<A, E>): void => {
    const script: unknown = Reflect.get(uxp, 'script');
    const { args, setResult } = Schema.decodeUnknownSync(Script)(script);
    const exit = Effect.runSyncExit(Effect.provide(work(args), HostClock));
    const result = Exit.match(exit, { onSuccess: (value) => ({ value }), onFailure: (cause) => ({ failure: Cause.pretty(cause) }) });
    Reflect.apply(setResult, script, [JSON.stringify(result)]);
};
```

Typings omit `script` from `uxp`, and `Reflect.apply` keeps the host's `script` object as receiver. Each `do script` call loads a fresh context, `language javascript` running ExtendScript and `language uxpscript` UXP.

- Source text holding `async` or `await` switches the runner to async mode, returning empty before the body runs
- State for a later call goes through `insertLabel`, and each label write records an undo step
- Bundles load through `new Function('require', readFileSync(path, { encoding: 'utf-8' }))(require)`, since large source sent as text runs nothing
- Listeners a script registers fire after its context ended, and reading `handler` or `properties` of one crashes InDesign
- DOM calls from a continuation after a script returned crash InDesign, and quit events served while a script runs abort it

## [13]-[PLUG_INS]

InDesign runs `main` of every enabled plug-in at launch, panel shown or hidden, and timers fire while it is hidden:
- `host.app` other than `ID` rejects the folder
- Panels become Window menu actions, and commands and shortcuts bind none
- `featureFlags.uncaughtException` delivers uncaught exceptions as events beside `unhandledRejection`
- Removing another context's `app.addEventListener` listener reads its `id` and `eventType` alone
- Menu actions read `id`, `name`, and `area` in one bulk read each, and `enabled` or `checked` read across actions crashes InDesign
- `fs` on the temporary folder's native path throws where its `plugin-temp:` name reads
- `~/Library/Application Support/Adobe/UXP/PluginsInfo/v1/ID.json` rows name `$localPlugins/External/<id>` with `status` `enabled`, `type` `uxp`
- Once a user `ID.json` exists InDesign loads its rows alone, and an absolute row path loads nothing
- `~/Library/Logs/Adobe/Adobe InDesign/UXPLogs_*.log` records each load and each rejection no `unhandledRejection` flag delivers

Folders join a running InDesign for one session from a script:
1. `pluginManager.createPlugin(folder, pluginManager.PLUGIN_TYPE_THIRD_PARTY_V2)` returns the plug-in object
2. `pluginManager.addPlugin(plugin, true)` returns `2` for success
3. `plugin.showPanel(<panel id>)` from a `.then` callback starts its runtime once the script returned
4. Relaunch loads new code for the id, quit unloads the plug-in, and `plugin.runtime.load()` or `disablePlugin` during DOM work stops InDesign
