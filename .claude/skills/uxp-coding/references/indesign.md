# [INDESIGN]

Each InDesign member read and call is one host round trip that returns before the next statement, and a collection returns one field of every member in one call. Typings declare DOM classes as interfaces, `instanceof` holds for the exact class alone (`Rectangle instanceof PageItem` reads false), and code discriminates instances on their literal `constructorName` field.

## [01]-[MEMBERS]

InDesign holds its running object model as `$.dictionary`, read per class by `Model` of `@rasm/indesign/dom` through one ExtendScript source sent with `app.doScript(<source>, ScriptLanguage.JAVASCRIPT, [<class>])` and returned by `inspect` of a class or enumeration name:
- `$.dictionary.toXML().descendants("topicref").@href` lists every class and enumeration name, each equal to the `indesign` module's
- `getClass(<name>).toXML()` rows include inherited members, its `@type == "class"` rows hold enumerators and class constants, and readers index each `XMLList`, where `for each` yields unnamed rows
- `varies=any` rows list their types in a closing "Can accept:" or "Can return:" clause
- "(Optional)" marks a parameter optional with no default, and reserved-word parameters are `in`, `with`, and `from`
- `Object.getOwnPropertyDescriptors(indesign.<Class>.prototype)` lists what the installed build exposes, getters as fields: the object model's members with `constructorName`, `equals`, `toSource`, `toSpecifier`, and `toString`

## [02]-[SPECIFIERS]

Collection members are specifiers that resolve at each member read, and an unmatched lookup returns a specifier with `isValid` false:

```ts
import indesign, { app, type BaseCollection, type Document, type PageItem, ScriptLanguage } from 'adobe:indesign';

interface Hosted {
    readonly constructorName: string;
    readonly toSpecifier: () => string;
}

const owning = <A>(key: string): ((value: unknown) => value is A) => {
    const owners = new Set(
        Array.flatMap(Object.getOwnPropertyNames(indesign), (name): readonly unknown[] => {
            const type: unknown = Reflect.get(indesign, name);
            return Predicate.isFunction(type) && Object.hasOwn(type.prototype, key) ? [type.prototype] : [];
        }),
    );
    return (value): value is A => Predicate.isObject(value) && owners.has(Object.getPrototypeOf(value));
};
const [hosted, collected] = [owning<Hosted>('toSpecifier'), owning<BaseCollection<unknown>>('everyItem')];
const located = (document: Document, id: number): Option.Option<PageItem> => {
    const found = document.pageItems.itemByID(id);
    return found.isValid ? Array.head(found.getElements()) : Option.none();
};
const ID_STEP = /(?<kind>[\w-]+)\[@id=\d+\]$/u;
const resolved = (specifier: string): Option.Option<Hosted | readonly Hosted[]> => {
    const kind = (path: string): string | undefined => ID_STEP.exec(path)?.groups?.kind;
    return Option.filter(
        Option.liftThrowable((): unknown => app.doScript('resolve(arguments[0])', ScriptLanguage.JAVASCRIPT, [specifier]))(),
        (found): found is Hosted | readonly Hosted[] => (hosted(found) || (Array.isArray(found) && found.every(hosted))) && Array.ensure(found).every((each: Hosted) => kind(specifier) === undefined || kind(each.toSpecifier()) === kind(specifier)),
    );
};
const item = (target: unknown, key: PropertyKey): unknown => (collected(target) && !Predicate.isSymbol(key) && !(key in target) ? target.item(key) : Reflect.get(new Object(target), key));
```

`getElements()` returns each member as its concrete class and throws on an invalid specifier. Reading a native getter (`constructorName`, `length`) with a class prototype as receiver crashes InDesign. `hosted` and `collected` read no getter: a DOM object's prototype is an exported class prototype owning `toSpecifier`, and a collection's owns `everyItem`. `resolved` turns any `toSpecifier()` string (ids, `[@name]` selectors, group paths, text ranges, collections) back into its object through ExtendScript's `resolve`, with the specifier passed as an argument and an id step's class compared. Supplied source compiles with `item` as its computed member read: a collection lacking the key returns `item(key)`, and arrays, records, and `length` keep their own.

| [INDEX] | [NEED]                     | [FORM]                                           | [DECIDING_FACT]                                |
| :-----: | :------------------------- | :----------------------------------------------- | :--------------------------------------------- |
|  [01]   | Member by position or name | `item(index)`, negative from the end, or a name  | `collection[index]` reads `undefined`          |
|  [02]   | Every member as an array   | `everyItem().getElements()`                      | Collections have no iterator                   |
|  [03]   | One field of every member  | `everyItem().<field>`, one call per field        | Array at any count, `properties` reads all     |
|  [04]   | Same object across reads   | `equals`                                         | Repeated reads differ under `===`              |
|  [05]   | Address held across calls  | `id`, or `toSpecifier()` after `getElements()`   | `item(index)` specifiers shift on insert       |
|  [06]   | Address of a text range    | `toSpecifier()`, then `resolved`                 | `Character`, `Word`, `Paragraph` have no `id`  |
|  [07]   | Style inside a group       | Specifier steps per group                        | Groups can repeat one style name               |
|  [08]   | Active document            | `Option.liftThrowable` over `app.activeDocument` | Throws while every open document is windowless |

- `app.panels` holds a member resolving to nothing, its `item(<index>)` reads `undefined`, and `everyItem()` or a range over it reads `undefined` in UXP and throws in ExtendScript
- `toSpecifier()` repeats the access path: `item(index)` positional, `itemByName` `[@name="<name>"]`, `itemByID` and `getElements()` `[@id=<n>]`, and `resolve` of an `[@id=<n>]` step returns the object with that id whatever class the step names
- Text specifiers are character spans that name another range after an edit before them, and `parentPage` reads `null` on the pasteboard
- Parents discriminate on `constructorName` (`Character` for an anchored item, `Spread` or `MasterSpread` for a top-level one)
- ExtendScript run through `doScript` keeps its own `scriptPreferences`, and reads and writes go through the object `resolved` returns

## [03]-[FIELDS]

Enumerators are objects: `===` between reads of one member is false, `JSON.stringify` yields `{}`, and `String` yields the member name. Each `Enumeration` holds every member under upper snake, camel, and lowercase keys (`CENTER_ALIGN`, `centerAlign`, `centeralign`), and one field write form retries a refused string as each enumerator of that name:

```ts
import indesign from 'adobe:indesign';

const Reason = Schema.TaggedUnion({
    Rejected: { field: Schema.String, cause: Schema.Defect() },
    Modal: { step: Schema.String },
    Refused: { step: Schema.String, cause: Schema.Defect() },
    Unrendered: { page: Schema.Int },
    Untranslatable: { pattern: Schema.String, constructs: Schema.NonEmptyArray(Schema.String) },
    Unavailable: { name: Schema.String },
    Unwritten: { file: Schema.String, cause: Schema.Defect() },
});
class InDesignError extends Schema.TaggedError<InDesignError, { readonly brand: unique symbol }>()('InDesignError', { reason: Reason }) {}

const written = (target: object, values: object): Effect.Effect<void, Array.NonEmptyArray<InDesignError>> => {
    const enumerations = Object.getOwnPropertyNames(indesign).flatMap((name) => {
        const value: unknown = Reflect.get(indesign, name);
        return value instanceof indesign.Enumeration ? [value] : [];
    });
    return Effect.validate(
        Object.entries(values),
        ([field, value]: readonly [string, unknown]) => {
            const put = (candidate: unknown): Effect.Effect<object, InDesignError> => Effect.try({ try: () => Object.assign(target, { [field]: candidate }), catch: (cause) => InDesignError.make({ reason: { _tag: 'Rejected', field, cause } }) });
            const named = enumerations.flatMap((enumeration) => {
                const member: unknown = Predicate.isString(value) ? Reflect.get(enumeration, value) : undefined;
                return member instanceof indesign.Enumerator ? [put(member)] : [];
            });
            return Effect.catch(put(value), (refused) => Effect.firstSuccessOf([...named, Effect.fail(refused)]));
        },
        { discard: true },
    );
};
```

Refusals throw one field at a time with the host's expected type ("Expected Unit or Leading enumerator"), and the host's type check picks the enumeration: `justification: 'centerAlign'` writes `Justification.CENTER_ALIGN`. Values leave through a `JSON.stringify` replacer that tests enumerators before strings, since a text read can return a `SpecialCharacters` enumerator, and writes each object `hosted` accepts as class, specifier, and `isValid`.

- Enumeration members are the prototype keys holding an `Enumerator` whose `String` equals the key, compared through `Justification.CENTER_ALIGN.equals(value)`
- New members take `add()` with required arguments, then `written` fields, and `properties` drops each key it refuses
- `add(record)` throws "Invalid parameter." or "Data is out of range." naming no field, and drops unknown keys and nested record refusals
- `documents.add(false, undefined, { documentPreferences: { pageWidth, pageHeight } })` sets a page from 1 to 15552 points, where an existing document refuses either field below about 73 points
- Swatch fields take a swatch name, and style fields a style object
- `appliedFont` takes a `Font`, `"<family>\t<style>"`, or a family keeping `fontStyle`, and a style's `appliedFont` reads a `Font`, a family name string, or `null`
- Absent families throw on an `appliedFont` write, and an absent style writes a missing face

## [04]-[TRANSACTIONS]

Throws inside a step roll back every change in every document and record no undo entry. A plug-in's top-level `app.doScript(<function>, ScriptLanguage.UXPSCRIPT, …)` can lose its hand-off: main thread waits for a callback that never runs, plug-in thread blocks on a fence, and InDesign hangs until killed, where ExtendScript source-text calls and function calls nested inside a callback complete. A peer call the plug-in serves during an ExtendScript `$.sleep` enters a nested function `doScript`, and open menus, dialogs, and drags hold that call before its callback runs. Each kind of work takes one form:

| [INDEX] | [WORK]                 | [FORM]                                                                |
| :-----: | :--------------------- | :-------------------------------------------------------------------- |
|  [01]   | Read                   | DOM call, or ExtendScript source text through `doScript`              |
|  [02]   | Write with no UXP code | ExtendScript source text under `ENTIRE_SCRIPT` around a function step |
|  [03]   | Write running UXP code | Function `doScript` nested in an ExtendScript `$.sleep` call          |
|  [04]   | Savepoint inside write | Function `doScript` nested in that transaction                        |
|  [05]   | Rolled-back read       | ExtendScript source text around a throwing function `doScript`        |

`app.doScript` returns a plain object with its scripting property keys alone, and rethrows as a new `Error` with no other field a function's throw as "<name>: <message>", a thrown string as its text, and an ExtendScript source throw as "Uncaught JavaScript exception: <message>". Value and failure leave through a `Deferred`:

```ts
import { app, SaveOptions, ScriptLanguage, UndoModes } from 'adobe:indesign';

const transacted = Effect.fn('transacted')(function* <A, E, R>(step: string, preview: boolean, body: Effect.Effect<A, E, R>) {
    yield* Effect.filterOrFail(
        Effect.sync(() => app.modalState),
        (modal) => !modal,
        () => InDesignError.make({ reason: { _tag: 'Modal', step } }),
    );
    const [services, outcome, opened] = [yield* Effect.context<R>(), yield* Deferred.make<A, E | InDesignError>(), app.documents.everyItem().id];
    const recorded = Effect.flatMap(
        Effect.tap(Effect.exit(body), (exit) => Deferred.done(outcome, exit)),
        (exit) => (preview ? Effect.interrupt : exit),
    );
    const run = (): A => Effect.runSyncWith(services)(recorded);
    const close = Effect.sync(() =>
        Array.difference(app.documents.everyItem().id, opened).forEach((id) => {
            app.documents.itemByID(id).close(SaveOptions.NO);
        }),
    );
    yield* Effect.catch(
        entered(() => app.doScript(run, ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, step)),
        (error) => Effect.andThen(Deferred.fail(outcome, error), close),
    );
    return yield* Deferred.await(outcome);
});
```

`entered` holds its step where the peer's call runs it and repeats `app.doScript('$.sleep(<ms>)', ScriptLanguage.JAVASCRIPT)` until the step ran, a host refusal failing `Refused`. `transacted` returns the body's value, or its typed failure with every change rolled back. Under `preview` the body's value returns and the interruption rolls the document back with no undo step: a dry run, a rendered preview, or a host refusal reported as data. `Deferred.fail` completes `outcome` only when `doScript` refused before the body ran, and documents a rolled-back step opened close by id difference. An async body fails `Refused`.

- Nested `doScript` calls are savepoints inside their outer step, where `document.undo()` throws, and `document.undoHistory` lists step names newest first
- Rollback restores documents and app-wide find, change, and export preferences, and leaves `app.scriptPreferences` and files on disk
- `app.modalState` reads true under the Preferences dialog, where a plug-in `doScript` drops its field writes with no error
- `app.modalState` reads true under an ExtendScript dialog open through `do script`, where writes take effect, and reads return under either

ExtendScript writes and rolled-back reads send source text with arguments as data, and a write names its function step with the undo name. Values leave the nested function through outer variables. A catch around a nested throw receives no thrown value, and `done`, set before the throw, tells a rollback from a failure:

```ts
const based = (target: Hosted, field: string): Effect.Effect<unknown, InDesignError> => {
    const source = `var target = resolve(arguments[0]), field = arguments[1], value = null, done = false;
try {
    app.doScript(function () { target.clearOverrides(); value = target[field]; done = true; throw 0; }, ScriptLanguage.JAVASCRIPT, [], UndoModes.ENTIRE_SCRIPT);
} catch (error) { if (!done) throw error; }
value`;
    return Effect.try({
        try: (): unknown => app.doScript(source, ScriptLanguage.JAVASCRIPT, [target.toSpecifier(), field]),
        catch: (cause) => InDesignError.make({ reason: { _tag: 'Refused', step: 'Clear Overrides', cause } }),
    });
};
```

`based` clears overrides on `target` in a step its throw rolls back and returns `field` as styles set it.

- Arguments pass primitives, dates, enumerators, DOM objects, and arrays of them at any depth, and sources resolve specifiers
- Collections and `everyItem()` arrive as a member array or one member, an empty one undefined, and plain objects with scripting property keys alone
- DOM objects a rolled-back step returns lose their specifier, and sources return `toSpecifier()` read inside the step
- ExtendScript assignment of a string to `activePage` crashes InDesign, and a `Page` object assigns

## [05]-[PREFERENCES]

`app.scriptPreferences` and app-wide export and find/change preferences are shared with the user's dialogs and persist across scripts and launches, and one scope restores the keys it wrote:

```ts
const held = <T extends { readonly constructorName: string }, K extends keyof T & string>(owner: T, values: Pick<T, K>): Effect.Effect<void, Array.NonEmptyArray<InDesignError>, Scope.Scope> =>
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
- Reads that change export or find preferences hold them through `held` with no transaction, each write setting `app.undoName` alone and leaving `document.undoHistory` unchanged
- Scripts start at `INTERACT_WITH_ALL`, and an uncaught script throw alerts under it and `NEVER_INTERACT`
- `NEVER_INTERACT` turns a PDF font embedding refusal into a silent omission and completes `createSubsetFont` of a synced face and `menuAction.invoke()` in place of a modal

## [06]-[GEOMETRY]

`resolve` returns one location in a named coordinate space independent of ruler units and origin, and its top-left and bottom-right anchors give an axis-aligned box for rotated, grouped, anchored, and pasteboard items:

```ts
import { AnchorPoint, BoundingBoxLimits, CoordinateSpaces, type Page, type PageItem } from 'adobe:indesign';

type Box = { readonly [Side in 'top' | 'left' | 'bottom' | 'right']: number };

const extent = (...placed: readonly (Page | PageItem)[]): Box =>
    Struct.makeReducer<Box>({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax }).combineAll(
        Array.flatMap(placed, (each) =>
            Array.flatMap([AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR], (anchor) => Array.map(each.resolve([anchor, BoundingBoxLimits.OUTER_STROKE_BOUNDS, CoordinateSpaces.SPREAD_COORDINATES], CoordinateSpaces.SPREAD_COORDINATES), ([x, y]) => ({ top: y, left: x, bottom: y, right: x }))),
        ),
    );
```

`extent` returns spread points with x 0 at a facing-pages spread's spine and at the center of any other spread. The bounds-space anchor form reads anchored items the two-element form misreads, and folding corners keeps a rotated group's box tight where four corners per item overstate it.

- `page.parent.resolve([[0, 0], page.index], CoordinateSpaces.SPREAD_COORDINATES, true)` locates a page's ruler origin in spread space
- `geometricBounds` orders `[top, left, bottom, right]` in ruler units, and `page.bounds` reads spread-origin points shifted by `zeroPoint` under every ruler origin
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

const rendered = Effect.fn('rendered')(function* (document: Document, page: number, longPixels: number, file: { readonly native: string; readonly read: string }) {
    yield* held(app.scriptPreferences, {
        measurementUnit: MeasurementUnits.POINTS,
        userInteractionLevel: UserInteractionLevels.NEVER_INTERACT,
        enableRedraw: false,
    });
    const target = yield* Effect.filterOrFail(
        Effect.sync(() => document.pages.item(page)),
        ({ isValid }) => isValid,
        () => InDesignError.make({ reason: { _tag: 'Unrendered', page } }),
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
        () => InDesignError.make({ reason: { _tag: 'Unrendered', page } }),
    );
}, Effect.scoped);
```

`rendered` runs on its own or inside a `transacted` preview, where rollback discards the rendered state. `file` is one entry from `getTemporaryFolder().createFile(name, { overwrite: true })` in a plug-in: `exportFile` takes its `nativePath`, `fs` its `url`. Emptying the file first makes zero bytes the one sign of a failed render, and pixel size is points times `exportResolution` (1 to 2400) over 72.

- Leftover `pageString` from another script makes a one-page export write nothing unless range and page are set together
- `exportFile` on an item ignores the page range and writes nothing for a zero-area item, and `exportingSpread` true exports the whole spread
- Export composes on its own and leaves out hidden, non-printing, and invisible items
- Native writes take any absolute path except a file directly in `/tmp` or `/private/tmp`, which fails with "Cannot find the folder"

Regions export from a scratch document that matches what InDesign draws:
1. `app.documents.add(false, undefined, { documentPreferences })` opens a windowless page of the region's size with `facingPages` false, then a layer per source layer
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

## [09]-[GREP]

InDesign's GREP dialect reads some JavaScript pattern syntax with another meaning, and finds nothing with no error for a pattern it cannot parse. A `RegExp` translates through a parser: each node the dialect misreads becomes one splice of the source at the node's range, and every construct with no dialect form fails the translation:

```ts
import MagicString from 'magic-string';
import regjsparser, { type AstNode } from 'regjsparser';

const features = { lookbehind: true, namedGroups: true, unicodePropertyEscape: true, unicodeSet: true } as const;
const splices = (node: AstNode<typeof features>): readonly Result.Result<readonly [start: number, end: number, text: string], string>[] => {
    const radix = 16;
    return Match.value(node).pipe(
        Match.discriminators('type')({
            value: ({ kind, codePoint, range: [start, end] }) => (kind === 'unicodeEscape' || String.fromCodePoint(codePoint).length > 1 ? [Result.succeed([start, end, `\\x{${codePoint.toString(radix).toUpperCase()}}`] as const)] : []),
            group: ({ behavior, body, raw }) => (behavior.endsWith('ookbehind') && body.some((child) => child.type === 'quantifier' && child.min !== child.max) ? [Result.fail(raw)] : body.flatMap(splices)),
        }),
        Match.discriminator('type')('alternative', 'disjunction', 'quantifier', ({ body }) => body.flatMap(splices)),
        Match.orElse(() => []),
    );
};
const grep = ({ source, flags, ignoreCase, dotAll, multiline }: RegExp): Result.Result<string, InDesignError> => {
    const untranslatable = (constructs: Array.NonEmptyArray<string>): InDesignError => InDesignError.make({ reason: { _tag: 'Untranslatable', pattern: source, constructs } });
    return Result.try(() => splices(regjsparser.parse(source, flags.includes('v') ? 'v' : 'u', features))).pipe(
        Result.mapError(() => untranslatable([source])),
        Result.flatMap((kept) =>
            Array.match(Array.getFailures(kept), {
                onNonEmpty: flow(untranslatable, Result.fail),
                onEmpty: () =>
                    Result.succeed(
                        `${ignoreCase ? '(?i)' : ''}${dotAll ? '(?s)' : ''}${multiline ? '' : '(?-m)'}${Array.getSuccesses(kept).reduce((edited, [start, end, text]) => edited.overwrite(start, end, text), new MagicString(source)).toString()}`,
                    ),
            }),
        ),
    );
};
```

`splices` holds one rewrite (an escaped or astral code point as `\x{…}`) and one rejection (a lookbehind of unbounded length), and each table row is one more arm of its `Match`. Rows the dialect cannot spell as written (a script, a property inside a class, `\d`) take a class of code points built from the runtime's Unicode data under the pattern's flags, one range boundary per membership change. A lookbehind's fixed width is the sum of its children's widths, a quantifier counting when `min` equals `max`.

| [INDEX] | [JAVASCRIPT]                  | [GREP_READS]                            | [GREP_FORM]                              |
| :-----: | :---------------------------- | :-------------------------------------- | :--------------------------------------- |
|  [01]   | `\uXXXX`, surrogate pairs     | `\u` as uppercase, pairs match nothing  | `\x{XXXX}`, `\x{1F600}`                  |
|  [02]   | `\p{L}`, `\p{N}`, `\p{P}`     | Lowercase alone, nothing, nothing       | `\p{Letter}`, `\p{Number}`               |
|  [03]   | `\p{Script=X}`, `[a-z\p{Lu}]` | Nothing, property as literal characters | Class of code points                     |
|  [04]   | `^`, `$` inside alternation   | Anchor binds every alternative          | Lookaround per branch                    |
|  [05]   | `^`, `$` without `m`          | Paragraph start and end                 | `(?-m)` opening the pattern              |
|  [06]   | `\d`, `\w`, `\b`              | Unicode digits, letters, boundaries     | ASCII classes and lookarounds            |
|  [07]   | `i`, `s` flags                | No flags, `.` stops at `\r` alone       | `(?i)`, `(?s)` opening the pattern       |
|  [08]   | `(?<=\$\|USD)`, `(?<!a\|bc)`  | Invalid, alternatives differ in length  | `(?:(?<=\$)\|(?<=USD))`, `(?<!a)(?<!bc)` |
|  [09]   | `(?<=a+)`                     | Invalid, unbounded lookbehind           | `a+\K` in GREP written directly          |
|  [10]   | `\n`                          | Forced line break U+000A alone          | `\r` ends a paragraph                    |

Searches and changes run in one sequence:
1. Find, change, and options preferences take `NothingEnum.NOTHING`, then `written` sets every key the search reads
2. `findGrep()` of `(?:<pattern>)|<sentinel>` over a scratch story holding `<sentinel>`, as a rolled-back read, hits exactly when `<pattern>` parses, and a pattern holding `\K` joins `|<sentinel>` ungrouped
3. `findGrep()` returns `Text` subclasses with `contents`, `index`, and `parentStory`, zero-length `InsertionPoint` matches included
4. `changeGrep()` returns ranges that later changes in their story leave stale, and changed text reads through `story.paragraphs.item(<ordinal>)` after the change

- Field writes pass `findWhat` and `grepExpression` values through `grep` and `changeTo` values through Change To translation before `written`
- Change To takes `$0` through `$9`, `\r`, `\t`, `\n`, `\x{XXXX}`, and `\U`, `\u`, `\L`, and inserts `\1`, `$&`, `${1}`, and `$$` as literal text
- JavaScript replacements map `\1` to `$1`, `$&` to `$0`, `$$` to `\$`, and a named reference to `$` with its group's ordinal, and a reference past `$9` has no form
- `saveFindChangeQuery(name, SearchModes.GREP_SEARCH)` stores queries `loadFindChangeQuery` restores, and `findChangeGrepOptions` holds no case option
- GREP styles are `nestedGrepStyles.add()` on a paragraph style, then `written` with `grepExpression` and `appliedCharacterStyle`

## [10]-[FONTS]

`Font.name` is family and style joined by a tab, giving both from one bulk read, `location` is a file path or a label for a face with no readable file, and `createSubsetFont` writes a single-face file for any installed face:

```ts
import { readFileSync } from 'adobe:fs';
import { app, FontStatus } from 'adobe:indesign';

const face = Effect.fn('face')(function* (name: string, characters: string, folder: string) {
    const font = yield* Effect.filterOrFail(
        Effect.sync(() => app.fonts.itemByName(name)),
        ({ status }) => !status.equals(FontStatus.NOT_AVAILABLE),
        () => InDesignError.make({ reason: { _tag: 'Unavailable', name } }),
    );
    const { location, postscriptName } = font;
    const subset = `${folder}/${postscriptName}`;
    const unwritten = (file: string) => (cause: unknown): InDesignError => InDesignError.make({ reason: { _tag: 'Unwritten', file, cause } });
    const path = yield* location.startsWith('/')
        ? Effect.succeed(location)
        : Effect.as(Effect.try({ try: () => font.createSubsetFont(characters, subset), catch: unwritten(subset) }), subset);
    const bytes = yield* Effect.try({ try: () => new Uint8Array(readFileSync(path)), catch: unwritten(path) });
    return { bytes, postscriptName, subset: path === subset };
});
```

`face` returns a file face's bytes, and an Adobe Fonts or system face ("Added from Adobe Fonts", "System Font") through a subset of `characters` under `NEVER_INTERACT`. `status` decides first, since `itemByName` returns a valid specifier for a face InDesign lacks, and its `location` read throws. `postscriptName` selects one face inside a collection file.

- `Font.checkOpenTypeFeature(<tag>)` reports GSUB and GPOS features alone, and text applies `morx` features too
- Variable faces read axes through `designAxesName` and `designAxesValues`, a subset drops them, and every `designAxes*` and `numDesignAxes` read throws for a static face, a bulk read holding one included

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
    Effect.gen(function* () {
        const { args, setResult } = yield* Schema.decodeUnknownEffect(Script)(script);
        const exit = yield* Effect.exit(work(args));
        const result = yield* Schema.encodeUnknownEffect(Schema.fromJsonString(Schema.Unknown))(Exit.match(exit, { onSuccess: (value) => ({ value }), onFailure: (cause) => ({ failure: Cause.pretty(cause) }) }));
        Reflect.apply(setResult, script, [result]);
    }).pipe(Effect.provide(clock), Effect.runSync);
};
```

`clock` is the host `Clock` layer. Typings omit `script` from `uxp`, and `Reflect.apply` keeps the host's `script` object as receiver. A `script` that fails the decode throws from `runSync` and the host alerts it. Each `do script` call loads a fresh context, `language javascript` running ExtendScript and `language uxpscript` UXP, and state for a later call goes through `insertLabel`.

- Source text holding `async ` before a function or `await ` before an expression switches the runner to async mode, replying empty before the body runs
- Runner tests match `async ` and `await ` inside strings and comments
- Bundles load through `new Function('require', readFileSync(path, { encoding: 'utf-8' }))(require)`, since large source sent as text runs nothing
- Listeners a script registers fire after its context ended, and reading `handler` or `properties` of one crashes InDesign
- DOM calls from a continuation after a script returned crash InDesign, and quit events served while a script runs abort it

## [13]-[PLUG_INS]

InDesign runs `main` of every enabled plug-in at launch, panel shown or hidden, and timers fire while it is hidden:
- `host.app` other than `ID` rejects the folder, and panels become Window menu actions where commands and shortcuts bind none
- `featureFlags.uncaughtException` delivers uncaught exceptions as events beside `unhandledRejection`, and `~/Library/Logs/Adobe/Adobe InDesign/UXPLogs_*.log` records each load and each rejection neither flag delivers
- `addEventListener(<type>, <handler>)` checks `<type>` against the target's `events`, named by class statics (`TextFrame.BEFORE_PLACE`), returns the listener an equal pair holds, and fails 30625 with `captures` true
- Removing another context's `app.addEventListener` listener reads its `id` and `eventType` alone
- Menu actions read `id`, `name`, and `area` in bulk, and `enabled` or `checked` of Character panel actions 26996 to 27015 crashes InDesign, a bulk read included
- Once a user `ID.json` registry exists InDesign loads its rows alone, and an absolute row `path` loads nothing

Folders join a running InDesign for one session from a script:
1. `pluginManager.createPlugin(folder, pluginManager.PLUGIN_TYPE_THIRD_PARTY_V2)` returns the plug-in object
2. `pluginManager.addPlugin(plugin, true)` returns `2` for success
3. `plugin.showPanel(<panel id>)` from a `.then` callback starts its runtime once the script returned
4. Relaunch loads new code for the id, quit unloads the plug-in, and `plugin.runtime.load()` or `disablePlugin` during DOM work stops InDesign
