# [INDESIGN]

Typings declare DOM classes as interfaces, and `instanceof` holds for the exact class alone (`Rectangle instanceof PageItem` reads false). Code discriminates instances by a member their prototypes declare or by their `constructorName` string.

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
- Encoders refuse a promise a storage getter returns, and write an entry's `url` through `String`
- Modules a Node program imports for their schemas import `adobe:indesign` for types alone and read the module from a service inside effects

## [02]-[SPECIFIERS]

Collection members are specifiers that resolve at each member read, and an unmatched lookup returns a specifier with `isValid` false:

```ts
import type { InDesignDOMObject } from 'adobe:indesign';

class Model extends Context.Service<Model>()('@app/indesign/Model', {
    make: (indesign: typeof import('adobe:indesign')) =>
        Effect.sync(() => {
            const model: typeof indesign = Object.create(indesign);
            const exported = Array.map(Object.getOwnPropertyNames(indesign), (name): unknown => Reflect.get(indesign, name));
            const owning = <A>(member: keyof A & string) => {
                const owners = new Set(Array.flatMap(exported, (type) => (Predicate.isFunction(type) && Object.hasOwn(type.prototype, member) ? [type.prototype] : [])));
                return <T>(value: T): value is T & A => Predicate.isObject(value) && owners.has(Object.getPrototypeOf(value));
            };
            return Object.assign(model, { owning, host: owning<InDesignDOMObject<unknown>>('toSpecifier') });
        }),
}) {}
const Container = Schema.String.check(Schema.isPattern(/(?:^|[^/])$/u));
const Separator = Schema.Literals(['//', '/']);
const Kind = Schema.String.check(Schema.isPattern(/^@?[a-z][\w-]*$/u));
const Identified = Schema.TemplateLiteral(['[@id=', Schema.Int, ']']);
const Selector = Schema.Union([Identified, Schema.TemplateLiteral(['[', Schema.Int, ']']), Schema.TemplateLiteral(['[@name="', Schema.String, '"]'])]);
const Step = Schema.Struct({ container: Container, separator: Separator, kind: Kind, selector: Schema.OptionFromOptionalKey(Selector) });
const Written = Schema.Union([Schema.TemplateLiteralParser([Container, Separator, Kind, Selector]), Schema.TemplateLiteralParser([Container, Separator, Kind])]).pipe(
    Schema.decodeTo(
        Schema.toEncoded(Step),
        SchemaTransformation.transform({
            decode: ([container, separator, kind, selector]) => ({ container, separator, kind, ...(selector === undefined ? {} : { selector }) }),
            encode: ({ container, separator, kind, selector }) => (selector === undefined ? ([container, separator, kind] as const) : ([container, separator, kind, selector] as const)),
        }),
    ),
);
class Specifier extends Schema.Class<Specifier, { readonly brand: unique symbol }>('Specifier')(Step.mapFields(Struct.assign({ end: Schema.OptionFromOptionalKey(Step) }))) {
    static readonly Path = Schema.Union([Schema.TemplateLiteralParser(['(', Written, ' to ', Written, ')']), Written]).pipe(
        Schema.decodeTo(
            Specifier,
            SchemaTransformation.transform({
                decode: (written) => (Array.isArray<typeof written>(written) ? { ...written[1], end: written[3] } : written),
                encode: ({ end, ...start }: typeof Specifier.Encoded) => (end === undefined ? start : (['(', start, ' to ', end, ')'] as const)),
            }),
        ),
    );
    static readonly resolve = (specifier: Specifier) =>
        Model.useSync(({ app, ScriptLanguage, host }) => {
            const identified = ({ kind, selector, end }: Specifier): Option.Option<string> => Option.as(Option.filter(selector, (each) => Option.isNone(end) && Schema.is(Identified)(each)), kind);
            const requested = identified(specifier);
            return Option.filter(
                Option.liftThrowable((): unknown => app.doScript('resolve(arguments[0])', ScriptLanguage.JAVASCRIPT, [specifier.path]))(),
                (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] =>
                    Array.ensure<unknown>(found).every((each) => host(each) && each.isValid && (Option.isNone(requested) || Equal.equals(Option.flatMap(Schema.decodeUnknownOption(Specifier.Path)(each.toSpecifier()), identified), requested))),
            );
        });

    get path(): string {
        const steps = Array.map([this, ...Option.toArray(this.end)], ({ container, separator, kind, selector }) => `${container}${separator}${kind}${Option.getOrElse(selector, () => '')}`);
        return Option.match(this.end, { onNone: () => steps.join(''), onSome: () => `(${steps.join(' to ')})` });
    }
}
```

`getElements()` returns each member as its concrete class and throws on an invalid specifier. Reading a native getter (`constructorName`, `length`) with a class prototype as receiver crashes InDesign, and `owning` reads no getter and holds for an object whose own prototype is a module class prototype declaring the member, `toSpecifier` for a DOM object (`host`) and `everyItem` for a collection. `Predicate.hasProperty` holds wherever an ancestor prototype holds the name (`name` on any function or `Error`). Guards for one kind take a member only that kind's prototypes declare (`documentOffset` for `Page`, `itemLayer` for every `PageItem` class). `Specifier.Path` decodes a saved specifier into container, separator, and last step, or into a start and an end step for the `(<A> to <B>)` range that `findText` hits, `textStyleRanges` members, and `itemByRange` return, and refuses any other text. `Specifier.resolve` passes the path to ExtendScript's `resolve` as an argument and compares the `kind` of a lone `[@id=<n>]` step. Empty collections resolve as `undefined`, and `everyItem().getElements()` on a known collection reads an empty array. Supplied source calls native collection methods without computed-member rewriting.

| [INDEX] | [NEED]                     | [FORM]                                                 | [REASON]                                      |
| :-----: | :------------------------- | :----------------------------------------------------- | :-------------------------------------------- |
|  [01]   | Member by position or name | `item(index)`, negative from the end, or a name        | `collection[index]` reads `undefined`         |
|  [02]   | Every member as an array   | `everyItem().getElements()`                            | Collections have no iterator                  |
|  [03]   | One field of every member  | `everyItem().<field>`, one call per field              | Array at any count, `properties` reads all    |
|  [04]   | Same object across reads   | `equals`                                               | Repeated reads differ under `===`             |
|  [05]   | Address held across calls  | `id`, or `toSpecifier()` after `getElements()`         | `item(index)` specifiers shift on insert      |
|  [06]   | Address of a text range    | `toSpecifier()` through `Specifier.Path` and `resolve` | `Character`, `Word`, `Paragraph` have no `id` |
|  [07]   | Style inside a group       | Specifier steps per group                              | Groups can repeat one style name              |
|  [08]   | Active document            | `Effect.try` over `app.activeDocument`                 | Throws with only windowless documents open    |

- `app.panels` holds a member resolving to nothing, its `item(<index>)` reads `undefined`, and `everyItem()` or a range reads `undefined` in UXP and throws in ExtendScript
- Removed objects keep their `toSpecifier()`, read `isValid` false, and read `constructorName` empty
- `toSpecifier()` repeats the access path: `item(index)` positional, `itemByName` `[@name="<name>"]`, `itemByID` and `getElements()` `[@id=<n>]`
- `resolve` of an `[@id=<n>]` step returns the object with that id whatever class the step names
- Text specifiers are character spans that name another range after an edit before them, and `parentPage` reads `null` on the pasteboard
- Parents discriminate on `constructorName` (`Character` for an anchored item, `Spread` or `MasterSpread` for a top-level one)
- ExtendScript run through `doScript` keeps its own `scriptPreferences`, and reads and writes go through the object `Specifier.resolve` returns

## [03]-[FIELDS]

Enumerators are objects: `===` between reads of one member is false, `JSON.stringify` yields `{}`, and `String` yields the member name. Each `Enumeration` holds every member under upper snake, camel, and lowercase keys (`CENTER_ALIGN`, `centerAlign`, `centeralign`). Scripts pass native values. JSON inputs decode by the type a field's dictionary row declares. Enumeration fields take a member name or `{type, member}`, class fields `{type, specifier}` or a name in the document's collection of that class.

Refusals throw one field at a time with the host's expected type ("Expected Unit or Leading enumerator"). Writes assign each decoded value once, keeping literal strings, and records write field by field into a read-only member's object (preferences). `JSON.stringify` replacers write enumerators as name, collections as type, specifier, and length, DOM objects as type, specifier, name, and `valid`.

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
    const { app, SaveOptions, ScriptLanguage, UndoModes } = yield* Model;
    const [opened, done, sync] = [app.documents.everyItem().id, yield* Deferred.make<A, E | HostError>(), Effect.runSyncWith(yield* Effect.context<R>())];
    yield* Effect.try({ try: (): unknown => app.doScript(() => sync(Effect.andThen(Deferred.into(body, done), Deferred.await(done))), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, label), catch: HostError.refused(label) }).pipe(Effect.catch((error) => Deferred.fail(done, error)));
    return yield* Deferred.await(done).pipe(Effect.onError(() => Effect.forEach(Array.difference(app.documents.everyItem().id, opened), (id) => Effect.sync(() => app.documents.itemByID(id).close(SaveOptions.NO)), { discard: true })));
});
```

`commit` runs in a script context and returns the body's value or rolled-back typed failure. `Deferred.fail` completes `done` only when `doScript` refused before the body ran. Documents a rolled-back step opened close, found by id difference. Plugins hold one step open with repeated ExtendScript `$.sleep` calls until a peer call enters it and runs inside the sleep.

- Nested `doScript` calls are savepoints inside their outer step, where `document.undo()` throws
- `document.undoHistory` lists step names newest first
- Synchronous `doScript` calls after awaited work group the edits after them
- Rollback restores documents and app-wide find, change, and export preferences, and leaves `app.scriptPreferences` and files on disk
- `app.modalState` reads true under the Preferences dialog, where a plugin `doScript` drops its field writes with no error
- `app.modalState` reads true under an ExtendScript dialog open through `do script`, where writes take effect, and reads return under either

ExtendScript writes and rolled-back reads send source text with arguments as data, and a write names its function step with the undo name. Values leave the nested function through outer variables. Catches around a nested throw receive no thrown value, and `done`, set before the throw, tells a rollback from a failure:

```ts
import type { InDesignDOMObject } from 'adobe:indesign';

const based = (target: InDesignDOMObject<unknown>, field: string) =>
    Model.use(({ app, ScriptLanguage }) =>
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

`app.scriptPreferences` and app-wide export and find/change preferences are shared with the user's dialogs and persist across scripts and launches. Export and scripting scopes restore written keys:

```ts
const preserve = <T extends object>(target: T, replacements: Partial<T>) => {
    const refused = HostError.refused(Struct.keys(replacements).join(', '));
    return Effect.acquireRelease(Effect.try({ try: () => Record.fromIterableWith(Struct.keys(replacements), (key) => [key, target[key]]), catch: refused }), (saved) => Effect.sync(() => Object.assign(target, saved))).pipe(
        Effect.tap(() => Effect.try({ try: () => Object.assign(target, replacements), catch: refused })),
    );
};
```

Preference members are prototype accessors, `preserve` reads each key by name before writing any, and the scope restores every key whether its write succeeded or failed.

Find/Change scopes save, load, and delete a native query. Native serialization preserves unset `NothingEnum.NOTHING` values and nested preferences that property copies lose.

- `MeasurementUnits.POINTS` makes every measurement field read and write points, text size and leading included
- `AutoEnum.AUTO_VALUE` varies units per field by ruler axis and stroke setting, and unit strings (`'9mm'`) parse under every unit
- Served calls hold `enableRedraw` false and `measurementUnit` `POINTS`, reads `userInteractionLevel` `NEVER_INTERACT`, writes InDesign's own level
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
        Model.use(({ AnchorPoint, BoundingBoxLimits, CoordinateSpaces }) =>
            Effect.try({
                try: () =>
                    Extent.make(
                        Struct.makeReducer<typeof Extent.Encoded>({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax }).combineAll(
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

Export reads app-wide preferences another script left, fails with no throw under `NEVER_INTERACT`, and writes nothing for an input it rejects. `rendered` holds every key a page export reads and decides success from the written bytes:

```ts
import type { Page, PNGExportPreference } from 'adobe:indesign';

const rendered = Effect.fn('rendered')(function* (page: Page, pixels: number, range: { readonly minimum: number; readonly maximum: number }, request: Pick<PNGExportPreference, 'useDocumentBleeds' | 'pngColorSpace' | 'pngQuality' | 'transparentBackground' | 'antiAlias' | 'simulateOverprint'>) {
    const [{ app, ExportFormat, PNGExportRangeEnum }, { fs }, { top, left, bottom, right }, path] = yield* Effect.all([Model, Uxp, Extent.of(page), temporary(`${page.parent.parent.id}-${page.id}.png`)]);
    const refused = HostError.refused(`export of ${page.toSpecifier()}`);
    yield* preserve(app.pngExportPreferences, {
        ...request,
        pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
        pageString: `+${page.documentOffset + 1}`,
        exportingSpread: false,
        exportingHiddenSpread: true,
        exportResolution: Number.clamp((pixels * 72) / Math.max(right - left, bottom - top), range),
        pngSuffix: '',
    });
    yield* Effect.try({ try: () => page.parent.parent.exportFile(ExportFormat.PNG_FORMAT, path), catch: refused });
    return yield* Effect.filterOrFail(Effect.try({ try: () => new Uint8Array(fs.readFileSync(`file:${path}`)), catch: refused }), (image) => image.length > 0, refused);
}, Effect.scoped);
```

`temporary` reads a path under ExtendScript `Folder.temp`, and scope release removes the file through `File(<path>).remove()`, synchronous in plugins and scripts. `range` holds the `exportResolution` bounds the scripting dictionary states for `PNGExportPreference`, and `request` the image fields the caller decides. `exportFile` takes the path, and `fs` takes it as a `file:` URL. Page pixel size is points times `exportResolution` over 72.

- Leftover `pageString` from another script makes a one-page export write nothing unless range and page are set together
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

`fontFamily` and `fontStyleName` supply face metadata that splitting `Font.name` can misstate. `location` is a file path or a label for a face with no readable file:

```ts
import type { Font } from 'adobe:indesign';

const source = Effect.fn('source')(function* (face: Font, characters: string) {
    const { FontStatus } = yield* Model;
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

Scripts answer through one `script.setResult` call and fire no timers. Scripts serving an `RpcGroup` read one encoded request from their arguments as stdin and answer through `setResult` as stdout:

```ts
import uxp from 'adobe:uxp';
import { RpcMessage, RpcSerialization, RpcServer } from 'effect/rpc';

const Host = Schema.Struct({ script: Schema.declare(Schema.is(Schema.Struct({ args: Schema.Array(Schema.String), setResult: Schema.declare(Predicate.isFunction) }))) });
const script = <E>(server: Layer.Layer<never, E, RpcServer.Protocol | Uxp>): void => {
    Effect.gen(function* () {
        const [{ script: host }, serialization] = yield* Effect.all([Schema.decodeUnknownEffect(Host)(uxp), RpcSerialization.RpcSerialization]);
        const answer = (chunk: string | Uint8Array) => Effect.sync(() => host.setResult(Predicate.isString(chunk) ? chunk : new TextDecoder().decode(chunk)));
        yield* Layer.launch(server).pipe(
            Effect.provide(RpcServer.layerProtocolStdio.pipe(Layer.provide(Layer.succeed(Stdio.Stdio)(Stdio.make({ args: Effect.succeed(host.args), stdin: Stream.encodeText(Stream.fromIterable(host.args)), stdout: () => Sink.forEach(answer), stderr: () => Sink.forEach(Console.error) }))))),
            Effect.catchCauseIf(
                Predicate.not(Cause.hasInterruptsOnly),
                flow(
                    Cause.squash,
                    Schema.encodeEffect(Schema.Defect()),
                    Effect.flatMap((defect) => UndefinedOr.match(serialization.makeUnsafe().encode(RpcMessage.ResponseDefectEncoded(defect)), { onUndefined: () => Effect.void, onDefined: answer })),
                ),
            ),
        );
    }).pipe(Effect.tapCauseIf(Predicate.not(Cause.hasInterruptsOnly), logged), Effect.provide([hosted, RpcSerialization.layerJson]), Effect.runSyncExit);
};
```

`layerProtocolStdio` reads the request until `stdin` ends, writes one response to `stdout`, and interrupts the launching fiber. `runSyncExit` returns inside the script with every forked server fiber finished. Servers that fail to build answer a `Defect` message, and no failure reaches the host as a throw. Missing or misshapen `uxp.script` leaves no `setResult` to answer through, and its decode failure logs through the plugin entry's `logged` sink, where the interrupt ending each completed run logs nothing. Typings omit `script` from `uxp`. Each `do script` call loads a fresh context, ExtendScript under `language javascript` and UXP under `language uxpscript`. State for a later call goes through `insertLabel`.

- Script roots pass the `adobe:indesign` module to a one-call server layer (`script(server(indesign))`), and their entry imports no Node module into the bundle
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
- `featureFlags.uncaughtException` delivers uncaught exceptions as events beside `unhandledRejection`
- `~/Library/Logs/Adobe/Adobe InDesign/UXPLogs_*.log` records each load and each rejection neither flag delivers
- `addEventListener(<type>, <handler>)` checks `<type>` against the target's `events`, named by class statics (`TextFrame.BEFORE_PLACE`)
- `addEventListener` returns the listener an equal type and handler pair holds, and fails 30625 with `captures` true
- Removing another context's `app.addEventListener` listener reads its `id` and `eventType` alone
- Menu actions read `id`, `name`, and `area` in bulk
- `enabled` or `checked` of Character panel actions 26996 to 27015 crashes InDesign, a bulk read included
- Once a user `ID.json` registry exists InDesign loads its rows alone, and an absolute row `path` loads nothing

Folders join a running InDesign for one session from a script:
1. `pluginManager.createPlugin(folder, pluginManager.PLUGIN_TYPE_THIRD_PARTY_V2)` returns the plugin object
2. `pluginManager.addPlugin(plugin, true)` returns `2` for success
3. `plugin.showPanel(<panel id>)` from a `.then` callback starts its runtime once the script returned
4. InDesign restart loads new code for the id, quit unloads the plugin, and `plugin.runtime.load()` or `disablePlugin` during DOM work stops InDesign
