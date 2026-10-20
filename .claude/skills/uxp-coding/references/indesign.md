# [INDESIGN]

Typings declare DOM classes as interfaces, `instanceof` holds for the exact class alone (`Rectangle instanceof PageItem` reads false), and code discriminates instances through a member their prototypes declare or their literal `constructorName` field.

## [01]-[MEMBERS]

InDesign holds its running object model as ExtendScript's `$.dictionary`, read through `app.doScript(<source>, ScriptLanguage.JAVASCRIPT, [<class>])`:
- `$.dictionary.toXML().descendants("topicref").@href` lists every class and enumeration name, each equal to the `indesign` module's
- `getClass(<name>).toXML()` rows include inherited members, and `@type == "class"` rows hold enumerators and class constants
- Readers index each `XMLList`, where `for each` yields unnamed rows
- `varies=any` rows state accepted types in English descriptions alone
- "(Optional)" marks a parameter optional with no default, and reserved-word parameters are `in`, `with`, and `from`
- `Object.getOwnPropertyDescriptors(indesign.<Class>.prototype)` lists the members the installed build exposes, getters as fields
- Prototypes hold `constructorName`, `equals`, `toSource`, `toSpecifier`, and `toString` beside object model members
- Aggregate `.properties` invokes nested getters before a failure can be isolated, and readers read each getter with its metadata
- Find/Change drop-shadow and inner-shadow `angle`, `distance`, `xOffset`, and `yOffset` accessors crash without a native owner
- Encoders await storage getters, and an entry `url` can be a URL object that `String(entry.url)` serializes
- Modules a Node program imports for their schemas import `adobe:indesign` for types alone and read the module from a service inside effects

## [02]-[SPECIFIERS]

Collection members are specifiers that resolve at each member read, and an unmatched lookup returns a specifier with `isValid` false:

```ts
import type { BaseCollection, Document, InDesignDOMObject, PageItem } from 'adobe:indesign';

const IDENTIFIED = /(?<kind>[\w-]+)\[@id=\d+\]$/u;
class Model extends Context.Service<Model>()('Model', {
    make: (indesign: typeof import('adobe:indesign')) =>
        Effect.sync(() => {
            const model: typeof indesign = Object.create(indesign);
            const prototypes = Record.map(
                Array.groupBy(
                    Array.flatMap(Object.getOwnPropertyNames(indesign), (name): readonly (readonly [string, object])[] => {
                        const type: unknown = Reflect.get(indesign, name);
                        return Predicate.isFunction(type) ? Array.map(Object.getOwnPropertyNames(type.prototype), (member) => [member, type.prototype] as const) : [];
                    }),
                    ([member]) => member,
                ),
                (owners) => new Set(Array.map(owners, ([, prototype]) => prototype)),
            );
            const owning = <A>(member: keyof A & string): (<T>(value: T) => value is T & A) => {
                const owners = Option.getOrElse(Record.get(prototypes, member), () => new Set<unknown>());
                return <T>(value: T): value is T & A => Predicate.isObject(value) && owners.has(Object.getPrototypeOf(value));
            };
            return Object.assign(model, { owning, host: owning<InDesignDOMObject<unknown>>('toSpecifier'), collection: owning<BaseCollection<unknown>>('everyItem') });
        }),
}) {}
const pageItem = (document: Document, id: number): Option.Option<PageItem> => {
    const found = document.pageItems.itemByID(id);
    return found.isValid ? Array.head(found.getElements()) : Option.none();
};
const located = (specifier: string) =>
    Effect.map(Model, ({ app, ScriptLanguage, host }) => {
        const kind = (path: string): string | undefined => IDENTIFIED.exec(path)?.groups?.kind;
        return Option.filter(
            Option.liftThrowable((): unknown => app.doScript('resolve(arguments[0])', ScriptLanguage.JAVASCRIPT, [specifier]))(),
            (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] => Array.ensure<unknown>(found).every((each) => host(each) && (kind(specifier) === undefined || kind(each.toSpecifier()) === kind(specifier))),
        );
    });
```

`getElements()` returns each member as its concrete class and throws on an invalid specifier. Reading a native getter (`constructorName`, `length`) with a class prototype as receiver crashes InDesign. `owning` reads no getter and holds for an object whose prototype is an exported class prototype declaring the member: `toSpecifier` for a DOM object, `everyItem` for a collection. A guard for one kind takes a member only that kind's prototypes declare (`documentOffset` for `Page`, `itemLayer` for every `PageItem` class). `located` passes a saved specifier to ExtendScript's `resolve` as an argument and compares an id step's class. Empty collections resolve as `undefined`, and code reads a known collection through `everyItem().getElements()` for an empty array. Supplied source calls native collection methods without computed-member rewriting.

| [INDEX] | [NEED]                     | [FORM]                                          | [DECIDING_FACT]                                |
| :-----: | :------------------------- | :---------------------------------------------- | :--------------------------------------------- |
|  [01]   | Member by position or name | `item(index)`, negative from the end, or a name | `collection[index]` reads `undefined`          |
|  [02]   | Every member as an array   | `everyItem().getElements()`                     | Collections have no iterator                   |
|  [03]   | One field of every member  | `everyItem().<field>`, one call per field       | Array at any count, `properties` reads all     |
|  [04]   | Same object across reads   | `equals`                                        | Repeated reads differ under `===`              |
|  [05]   | Address held across calls  | `id`, or `toSpecifier()` after `getElements()`  | `item(index)` specifiers shift on insert       |
|  [06]   | Address of a text range    | `toSpecifier()`, then `located`                 | `Character`, `Word`, `Paragraph` have no `id`  |
|  [07]   | Style inside a group       | Specifier steps per group                       | Groups can repeat one style name               |
|  [08]   | Active document            | `Effect.try` over `app.activeDocument`          | Throws while every open document is windowless |

- `app.panels` holds a member resolving to nothing, and its `item(<index>)` reads `undefined`
- `everyItem()` or a range over `app.panels` reads `undefined` in UXP and throws in ExtendScript
- `toSpecifier()` repeats the access path: `item(index)` positional, `itemByName` `[@name="<name>"]`, `itemByID` and `getElements()` `[@id=<n>]`
- `resolve` of an `[@id=<n>]` step returns the object with that id whatever class the step names
- Text specifiers are character spans that name another range after an edit before them, and `parentPage` reads `null` on the pasteboard
- Parents discriminate on `constructorName` (`Character` for an anchored item, `Spread` or `MasterSpread` for a top-level one)
- ExtendScript run through `doScript` keeps its own `scriptPreferences`, and reads and writes go through the object `located` returns

## [03]-[FIELDS]

Enumerators are objects: `===` between reads of one member is false, `JSON.stringify` yields `{}`, and `String` yields the member name. Each `Enumeration` holds every member under upper snake, camel, and lowercase keys (`CENTER_ALIGN`, `centerAlign`, `centeralign`). Scripts pass native values. JSON inputs decode by the type a field's dictionary row declares: an enumeration takes a member name or `{type, member}`, and a named class a name resolved in the owning document's collection of that class or `{type, specifier}`.

Refusals throw one field at a time with the host's expected type ("Expected Unit or Leading enumerator"). Writes assign each decoded value once, preserving literal strings, and a record value writes field by field into the object a read-only member holds (a preference object). Values leave through a `JSON.stringify` replacer that writes an enumerator as its member name, a collection as type, specifier, and length, and an object `host` accepts as type, specifier, name, and `isValid`.

- Enumeration members are the prototype keys holding an `Enumerator` whose `String` equals the key
- Enumerators compare through `equals` (`Justification.CENTER_ALIGN.equals(value)`)
- New members take `add()` with required arguments, then field writes, and `properties` drops each key it refuses
- `add(record)` throws "Invalid parameter." or "Data is out of range." naming no field, and drops unknown keys and nested record refusals
- `documents.add(false, undefined, { documentPreferences: { pageWidth, pageHeight } })` sets a page from 1 to 15552 points
- Existing documents refuse `pageWidth` or `pageHeight` below about 73 points
- Swatch fields take a swatch name, and style fields a style object
- `appliedFont` takes a `Font`, `"<family>\t<style>"`, or a family keeping `fontStyle`, and on a style reads a `Font`, family name, or `null`
- Absent families throw on an `appliedFont` write, and an absent style writes a missing face

## [04]-[TRANSACTIONS]

Throws inside a step roll back every change in every document and record no undo entry. Top-level `app.doScript(<function>, ScriptLanguage.UXPSCRIPT, …)` in a plug-in can lose its hand-off: main thread waits for a callback that never runs, plug-in thread blocks on a fence, and InDesign hangs until killed, where ExtendScript source-text calls and function calls nested inside a callback complete. Peer calls the plug-in serves during an ExtendScript `$.sleep` enter a nested function `doScript`, and open menus, dialogs, and drags hold a peer call before its callback runs. Each kind of work takes one form:

| [INDEX] | [WORK]                 | [FORM]                                                                |
| :-----: | :--------------------- | :-------------------------------------------------------------------- |
|  [01]   | Read                   | DOM call, or ExtendScript source text through `doScript`              |
|  [02]   | Write with no UXP code | ExtendScript source text under `ENTIRE_SCRIPT` around a function step |
|  [03]   | Write running UXP code | Function `doScript` nested in an ExtendScript `$.sleep` call          |
|  [04]   | Savepoint inside write | Function `doScript` nested in that transaction                        |
|  [05]   | Rolled-back read       | ExtendScript source text around a throwing function `doScript`        |

`app.doScript` returns a plain object with its scripting property keys alone, and rethrows as a new `Error` with no other field a function's throw as "<name>: <message>", a thrown string as its text, and an ExtendScript source throw as "Uncaught JavaScript exception: <message>". Value and failure leave through a `Deferred`:

```ts
import type { Document } from 'adobe:indesign';

type Handoff = Data.TaggedEnum<{ Idle: Record.ReadonlyRecord<never, never>; Held: { readonly step: Effect.Effect<void> }; Entered: Record.ReadonlyRecord<never, never> }>;
const Handoff = Data.taggedEnum<Handoff>();

const run = Effect.fn('run')(function* <A, E, R>(state: Ref.Ref<Handoff>, label: string, body: Effect.Effect<A, E, R>) {
    const { app, SaveOptions, ScriptLanguage, UndoModes } = yield* Model;
    yield* Effect.filterOrFail(
        Effect.sync(() => app.modalState),
        (modalState) => !modalState,
        (modalState) => new Cause.UnknownError({ modalState }, label),
    );
    const steps = () => {
        const every = app.documents.everyItem();
        return Array.zipWith(every.id, every.undoHistory, (id, history) => [id, history.length] as const);
    };
    const [before, done, sync] = [new Map(steps()), yield* Deferred.make<A, E | Cause.UnknownError>(), Effect.runSyncWith(yield* Effect.context<R>())];
    const refused = (cause: unknown) => new Cause.UnknownError(cause, label);
    const step = Effect.try({ try: (): unknown => app.doScript(() => sync(Effect.andThen(Deferred.into(body, done), Deferred.await(done))), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, label), catch: refused }).pipe(
        Effect.catch((error) => Deferred.fail(done, error)),
        Effect.asVoid,
    );
    const value = yield* Ref.set(state, Handoff.Held({ step })).pipe(
        Effect.andThen(Effect.try({ try: (): unknown => app.doScript('$.sleep(5)', ScriptLanguage.JAVASCRIPT), catch: refused }).pipe(Effect.repeat({ until: () => Deferred.isDone(done) }))),
        Effect.andThen(Deferred.await(done)),
        Effect.ensuring(Ref.set(state, Handoff.Idle())),
        Effect.onError(() =>
            Effect.sync(() =>
                Array.difference(app.documents.everyItem().id, [...before.keys()]).forEach((id) => {
                    app.documents.itemByID(id).close(SaveOptions.NO);
                }),
            ),
        ),
    );
    const touched: readonly Document[] = steps().flatMap(([id, length]) => (before.get(id) === length ? [] : [app.documents.itemByID(id)]));
    return { value, touched };
});
```

`run` holds its step in `state` and repeats `$.sleep(5)` until the step ran. The peer's `enter` call swaps `Held` for `Entered` and runs the step inside a call InDesign completes, and answers `Idle` when no step waits. `run` returns the body's value with each document whose undo history changed, or its typed failure with every change rolled back. A host refusal fails `Cause.UnknownError` with `label` as message, and an open modal state fails with `{ modalState }` as cause. `Deferred.fail` completes `done` only when `doScript` refused before the body ran, and documents a rolled-back step opened close by id difference. An async body fails `Cause.UnknownError`.

- Nested `doScript` calls are savepoints inside their outer step, where `document.undo()` throws
- `document.undoHistory` lists step names newest first
- Synchronous `doScript` calls after awaited work group the edits after them
- Rollback restores documents and app-wide find, change, and export preferences, and leaves `app.scriptPreferences` and files on disk
- `app.modalState` reads true under the Preferences dialog, where a plug-in `doScript` drops its field writes with no error
- `app.modalState` reads true under an ExtendScript dialog open through `do script`, where writes take effect, and reads return under either

ExtendScript writes and rolled-back reads send source text with arguments as data, and a write names its function step with the undo name. Values leave the nested function through outer variables. A catch around a nested throw receives no thrown value, and `done`, set before the throw, tells a rollback from a failure:

```ts
import type { InDesignDOMObject } from 'adobe:indesign';

const based = (target: InDesignDOMObject<unknown>, field: string): Effect.Effect<unknown, Cause.UnknownError, Model> => {
    const source = `var target = resolve(arguments[0]), field = arguments[1], value = null, done = false;
try {
    app.doScript(function () { target.clearOverrides(); value = target[field]; done = true; throw 0; }, ScriptLanguage.JAVASCRIPT, [], UndoModes.ENTIRE_SCRIPT);
} catch (error) { if (!done) throw error; }
value`;
    return Effect.flatMap(Model, ({ app, ScriptLanguage }) =>
        Effect.try({
            try: (): unknown => app.doScript(source, ScriptLanguage.JAVASCRIPT, [target.toSpecifier(), field]),
            catch: (cause) => new Cause.UnknownError(cause, `${target.toSpecifier()} ${field}`),
        }),
    );
};
```

`based` clears overrides on `target` in a step its throw rolls back and returns `field` as styles set it.

- Arguments pass primitives, dates, enumerators, DOM objects, and arrays of them at any depth, and sources resolve specifiers
- Collections and `everyItem()` arrive as a member array or one member, an empty one undefined, and plain objects with scripting property keys alone
- DOM objects a rolled-back step returns lose their specifier, and sources return `toSpecifier()` read inside the step
- ExtendScript assignment of a string to `activePage` crashes InDesign, and a `Page` object assigns

## [05]-[PREFERENCES]

`app.scriptPreferences` and app-wide export and find/change preferences are shared with the user's dialogs and persist across scripts and launches. Export and scripting scopes restore written keys:

```ts
const preserve = <T extends object>(target: T, replacements: Partial<T>) =>
    Effect.acquireRelease(
        Effect.try(() => Record.fromIterableWith(Struct.keys(replacements), (key) => [key, target[key]])),
        (saved) => Effect.orDie(Effect.try(() => Object.assign(target, saved))),
    ).pipe(Effect.tap(() => Effect.try(() => Object.assign(target, replacements))));
```

Preference members are prototype accessors that own-key copies (`Struct.pick`, a spread) skip. `preserve` reads each key by name before writing any, and the scope restores every key whether its write succeeded or failed.

Find/Change scopes save, load, and delete a native query. Native serialization preserves unset `NothingEnum.NOTHING` values and nested preferences that property copies lose.

- `MeasurementUnits.POINTS` makes every measurement field read and write points, text size and leading included
- `AutoEnum.AUTO_VALUE` varies units per field by ruler axis and stroke setting, and unit strings (`'9mm'`) parse under every unit
- Panels hold `userInteractionLevel` at `NEVER_INTERACT`, `enableRedraw` false, and `measurementUnit` at `POINTS` for each served call
- Reads that change export preferences hold them through `preserve` without a transaction or document undo entry
- Scripts start at `INTERACT_WITH_ALL`, and an uncaught script throw alerts under it and `NEVER_INTERACT`
- `NEVER_INTERACT` turns a PDF font embedding refusal into a silent omission
- `NEVER_INTERACT` completes `createSubsetFont` of a synced face and `menuAction.invoke()` in place of a modal

## [06]-[GEOMETRY]

`resolve` returns one location in a named coordinate space independent of ruler units and origin, and its top-left and bottom-right anchors give an axis-aligned box for rotated, grouped, anchored, and pasteboard items:

```ts
import type { Page, PageItem } from 'adobe:indesign';

const Box = Schema.Struct(Struct.Record(['top', 'left', 'bottom', 'right'], Schema.Finite));
const extent = Effect.fn('extent')(function* (...placed: readonly (Page | PageItem)[]) {
    const { AnchorPoint, BoundingBoxLimits, CoordinateSpaces } = yield* Model;
    return yield* Effect.try({
        try: () => {
            const corners: ReadonlyArray<readonly [number, number]> = Array.flatMap(Array.cartesian(placed, [AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR]), ([each, anchor]) =>
                each.resolve([anchor, BoundingBoxLimits.OUTER_STROKE_BOUNDS, CoordinateSpaces.SPREAD_COORDINATES], CoordinateSpaces.SPREAD_COORDINATES),
            );
            return Struct.makeReducer<typeof Box.Type>({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax }).combineAll(Array.map(corners, ([x, y]) => ({ top: y, left: x, bottom: y, right: x })));
        },
        catch: (cause) => new Cause.UnknownError(cause, `extent of ${Array.map(placed, (each) => each.toSpecifier()).join(', ')}`),
    });
});
```

`extent` returns spread points with x 0 at a facing-pages spread's spine and at the center of any other spread. The bounds-space anchor form reads anchored items the two-element form misreads, and folding corners keeps a rotated group's box tight where four corners per item overstate it.

- `page.parent.resolve([[0, 0], page.index], CoordinateSpaces.SPREAD_COORDINATES, true)` locates a page's ruler origin in spread space
- `geometricBounds` orders `[top, left, bottom, right]` in ruler units
- `page.bounds` reads spread-origin points shifted by `zeroPoint` under every ruler origin
- Anchored items move at composition, and `parentStory.recompose()` or `document.recompose()` precedes their read
- `spread.pageItems.everyItem().getElements()` lists front to back, and `page.pageItems` lists its base layer first
- `effectivePpi` reads per item of `allGraphics`, a bulk `everyItem().effectivePpi` returns `undefined`
- `marginPreferences.left` is the inside margin on a facing page, and `page.side` names the side
- Anchor-point `resolve` on a `Spread` reads the spread with its pasteboard, and a spread's pages come from `extent` over those pages

## [07]-[EXPORT]

Export reads app-wide preferences another script left, fails with no throw under `NEVER_INTERACT`, and writes nothing for an input it rejects. `rendered` holds every key export reads and decides success from the written bytes:

```ts
import type { Page } from 'adobe:indesign';

const rendered = Effect.fn('rendered')(function* (page: Page, pixels: number) {
    const [{ app, ExportFormat, PNGExportRangeEnum }, { fs, temporary }] = yield* Effect.all([Model, Uxp]);
    const { top, left, bottom, right } = yield* extent(page);
    const refused = (cause: unknown) => new Cause.UnknownError(cause, `export of ${page.toSpecifier()}`);
    const pointsPerInch = 72;
    const file = yield* temporary('render.png');
    yield* preserve(app.pngExportPreferences, {
        pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
        pageString: `+${page.documentOffset + 1}`,
        exportingSpread: false,
        exportResolution: (pixels * pointsPerInch) / Math.max(right - left, bottom - top),
        useDocumentBleeds: false,
    }).pipe(Effect.mapError(refused));
    yield* Effect.try({ try: () => page.parent.parent.exportFile(ExportFormat.PNG_FORMAT, file.nativePath), catch: refused });
    return yield* Effect.filterOrFail(
        Effect.try({ try: () => new Uint8Array(fs.readFileSync(file.url)), catch: refused }),
        (image) => image.length > 0,
        () => new Cause.UnknownError(undefined, `export of ${page.toSpecifier()} wrote no image`),
    );
}, Effect.scoped);
```

`Uxp` holds the `fs` module and `temporary`, which creates an entry in the plug-in temporary folder that scope release deletes after its byte read. `exportFile` takes the entry's `nativePath`, and `fs` takes its `url`. Page pixel size is points times `exportResolution` (1 to 2400) over 72.

- Leftover `pageString` from another script makes a one-page export write nothing unless range and page are set together
- `exportFile` on an item ignores the page range and writes nothing for a zero-area item, and `exportingSpread` true exports the whole spread
- Export composes on its own and leaves out hidden, non-printing, and invisible items
- Native writes take any absolute path except a file directly in `/tmp` or `/private/tmp`, which fails with "Cannot find the folder"

Whole items export through `PageItem.exportFile`, pages and spreads through `Document.exportFile`. Native item raster extents differ from geometric bounds, and returned image dimensions describe pixels without a derived coordinate transform. Application screenshots retain dialogs, panels, and pasteboard context.

## [08]-[TEXT]

Text members return a range's first-run value, and `textStyleRanges.everyItem().<field>` returns one value per run, a `SpecialCharacters` enumerator for a run of one special character:

| [INDEX] | [NEED]                       | [FORM]                                             | [DECIDING_FACT]                               |
| :-----: | :--------------------------- | :------------------------------------------------- | :-------------------------------------------- |
|  [01]   | Override presence            | `textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE)` | Text, paragraph, word hold it, story lacks it |
|  [02]   | Style applied, overrides cut | `appliedParagraphStyle` assignment                 | Matches `applyParagraphStyle(style)`          |
|  [03]   | Character overrides kept     | `applyParagraphStyle(style, false)`                | Paragraph-level overrides still go            |
|  [04]   | Overset characters           | `story.length` less frame `characters.length`      | `overflows` returns a boolean alone           |
|  [05]   | Anchored object position     | `U+FFFC` in `story.contents`                       | Marker is one character                       |

- GREP styles can leave `appliedCharacterStyle` as `[None]` and keep one `textStyleRange` across different formatting
- Overlapping GREP rules can render differently from character getters, and readers read `appliedNestedStyles` and rule order, then render

## [09]-[GREP]

`findWhat`, `grepExpression`, and `changeTo` take native strings unchanged. Invalid expressions can return no matches without throwing, and tests run on an intended passage through the native search scope:

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

`fontFamily` and `fontStyleName` supply face metadata that splitting `Font.name` can misstate. `location` is a file path or a label for a face with no readable file, and `createSubsetFont` supplies bytes when a source file is absent:

```ts
import type { Font } from 'adobe:indesign';

const source = Effect.fn('source')(function* (face: Font, characters: string) {
    const [{ FontStatus }, { fs, temporary }] = yield* Effect.all([Model, Uxp]);
    const bytes = (path: string) => Effect.try(() => new Uint8Array(fs.readFileSync(path)));
    const subset = Effect.flatMap(temporary('font.otf'), (entry) => Effect.andThen(Effect.try(() => face.createSubsetFont(characters, entry.nativePath)), bytes(entry.url)));
    return face.status.equals(FontStatus.INSTALLED)
        ? Option.some(yield* bytes(`file:${face.location}`).pipe(Effect.catchIf(({ cause }) => Schema.is(Schema.Struct({ code: Schema.Literal(-2) }))(cause), () => subset)))
        : Option.none<Uint8Array>();
}, Effect.scoped);
```

`source` reads original bytes and recovers a missing file (`code` -2) through a subset of `characters` under `NEVER_INTERACT`. Permission failures remain failures. `status` decides first: `app.fonts.itemByName` returns a valid specifier for a face InDesign lacks, and its `location` read throws. `postscriptName` selects one face inside a collection file.

- `Font.checkOpenTypeFeature(<OpenTypeFeature member>)` reports GSUB and GPOS features alone, and text applies `morx` features too
- Variable faces read axes through `designAxesName` and `designAxesValues`, and a subset drops them
- Every `designAxes*` and `numDesignAxes` read of a static face throws, a bulk read holding one included
- Application font rows can repeat equal objects, and document and application fonts for one face can compare unequal
- Point-size changes can reset `opsz`, and size goes before explicit optical-size coordinates

## [11]-[PREFLIGHT]

Preflight runs as a process object over one document and one profile:
- Profiles resolve by name over `app.preflightProfiles.everyItem().getElements()`, since `itemByName('[Basic]')` reads invalid
- `app.preflightProcesses.add(document, profile)` starts a process that a scope releases through `remove()` on every exit
- `waitForProcess(seconds)` can return false with results complete, and a decode of `aggregatedResults` decides
- `aggregatedResults` is `[name, profile, rows]`, each row `[depth, label, page, info, [key, value][]]`
- Depth 1 rows are categories and 2 rules, both with no details, and 3 issues whose label is the object kind
- Rule labels carry their issue count ("Overset text (1)"), and an issue matches across reports by its fields without its rule label
- `processResults` text and `aggregatedResults` rows name no object specifier
- InDesign raises no preflight event, and code diffs a process run before a change against one after it

## [12]-[SCRIPTS]

Scripts return through one `script.setResult` call and fire no timers, and each is one synchronous expression with one translation of its `Exit`:

```ts
import uxp from 'adobe:uxp';

const script = <A, I>(success: Schema.Codec<A, I>, work: (args: readonly string[]) => Effect.Effect<A, Error, Uxp>): void => {
    const target: unknown = Reflect.get(uxp, 'script');
    Effect.gen(function* () {
        const { args, setResult } = yield* Schema.decodeUnknownEffect(Schema.Struct({ args: Schema.Array(Schema.String), setResult: Schema.declare(Predicate.isFunction) }))(target);
        Reflect.apply(setResult, target, [yield* Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Exit(success, Schema.ErrorInstance(), Schema.Defect()))))(yield* Effect.exit(work(args)))]);
    }).pipe(Effect.provide(hosted), Effect.runSync);
};
```

Typings omit `script` from `uxp`, and `Reflect.apply` keeps the host's `script` object as receiver. The result is the `Exit` in `Schema.Exit`'s JSON form, its value encoded by the caller's codec. `runSync` drains Effect's scheduler before the script returns, where `runPromise` yields through timers the script never fires. A `script` that fails the decode or reaches an asynchronous step throws from `runSync` and the host alerts it. Each `do script` call loads a fresh context, `language javascript` running ExtendScript and `language uxpscript` UXP, and state for a later call goes through `insertLabel`.

- Runners switch to async mode for source holding `async ` before a function or `await ` before an expression, and reply empty before the body runs
- Runner tests match `async ` and `await ` inside strings and comments
- Bundles load through `new Function('require', readFileSync(path, { encoding: 'utf-8' }))(require)`, since large source sent as text runs nothing
- Listeners a script registers fire after its context ended, and reading `handler` or `properties` of one crashes InDesign
- DOM calls from a continuation after a script returned crash InDesign, and quit events served while a script runs abort it

## [13]-[PLUG_INS]

InDesign runs `main` of every enabled plug-in at launch, panel shown or hidden, and timers fire while it is hidden:
- `host.app` other than `ID` rejects the folder, and panels become Window menu actions where commands and shortcuts bind none
- `featureFlags.uncaughtException` delivers uncaught exceptions as events beside `unhandledRejection`
- `~/Library/Logs/Adobe/Adobe InDesign/UXPLogs_*.log` records each load and each rejection neither flag delivers
- `addEventListener(<type>, <handler>)` checks `<type>` against the target's `events`, named by class statics (`TextFrame.BEFORE_PLACE`)
- `addEventListener` returns the listener an equal type and handler pair holds, and fails 30625 with `captures` true
- Removing another context's `app.addEventListener` listener reads its `id` and `eventType` alone
- Menu actions read `id`, `name`, and `area` in bulk
- `enabled` or `checked` of Character panel actions 26996 to 27015 crashes InDesign, a bulk read included
- Once a user `ID.json` registry exists InDesign loads its rows alone, and an absolute row `path` loads nothing

Folders join a running InDesign for one session from a script:
1. `pluginManager.createPlugin(folder, pluginManager.PLUGIN_TYPE_THIRD_PARTY_V2)` returns the plug-in object
2. `pluginManager.addPlugin(plugin, true)` returns `2` for success
3. `plugin.showPanel(<panel id>)` from a `.then` callback starts its runtime once the script returned
4. Relaunch loads new code for the id, quit unloads the plug-in, and `plugin.runtime.load()` or `disablePlugin` during DOM work stops InDesign
