import type { Application, CellStyle, CharacterStyle, Document, Font, InDesignDOMObject, InsertionPoint, MasterSpread, ObjectStyle, Page, PageItem, ParagraphStyle, PreflightProcess, Spread, TableStyle, Text, TextDefault, TextFrame, TextPath, TextStyleRange } from 'adobe:indesign';
import { ExecuteScript, HostError, ObjectModel, type Open, type Render } from '@rasm/automation';
import { Peer } from '@rasm/automation/peer';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/uxp';
import { Array, Cache, Cause, Effect, Equivalence, Filter, Function, flow, identity, Layer, Match, Number, Option, Path, Predicate, pipe, Record, Result, Schema, SchemaGetter, Scope, Semaphore, String, Struct } from 'effect';
import { type Adjust, Call, type Change, type FindChange, type FontFile, InDesign, Inspect, type Invoke, type Kerns, Menu, Preflight, type Runs, type Typeset } from '#/contract';
import type { Slice } from '#/internal/slice';
import { Specifier } from '#/internal/specifier';
import { Value } from '#/internal/value';
import { InDesignModel } from '#/uxp/indesign-model';
import { InDesignSettings } from '#/uxp/indesign-settings';
import { AGGREGATE_FIELD, declaredMember, unavailableReason } from '#/uxp/internal/dictionary';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARK = /^\p{M}+$/v;
const SEARCHES = {
    Text: { find: 'findText', change: 'changeText', preferences: ['findTextPreferences', 'findChangeTextOptions', 'changeTextPreferences'] },
    Grep: { find: 'findGrep', change: 'changeGrep', preferences: ['findGrepPreferences', 'findChangeGrepOptions', 'changeGrepPreferences'] },
    Glyph: { find: 'findGlyph', change: 'changeGlyph', preferences: ['findGlyphPreferences', 'findChangeGlyphOptions', 'changeGlyphPreferences'] },
    Object: { find: 'findObject', change: 'changeObject', preferences: ['findObjectPreferences', 'findChangeObjectOptions', 'changeObjectPreferences'] },
    Color: { find: 'findColor', change: 'changeColor', preferences: ['findColorPreferences', 'findChangeColorOptions', 'changeColorPreferences'] },
    Transliterate: { find: 'findTransliterate', change: 'changeTransliterate', preferences: ['findTransliteratePreferences', 'findChangeTransliterateOptions', 'changeTransliteratePreferences'] },
} as const satisfies { readonly [M in FindChange['mode']]: { readonly find: `find${M}`; readonly change: `change${M}`; readonly preferences: readonly [`find${M}Preferences`, `findChange${M}Options`, `change${M}Preferences`] } };

// --- [MODELS] --------------------------------------------------------------------------

const Contents = Schema.Struct({ contents: Schema.Json });
const Issues = Preflight.Result.value.fields.aggregatedResults;
const Flat = Schema.Tuple([Schema.Int, Schema.String, Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.String, Schema.String]))]);
const Deltas = Schema.Struct({ kerning: InDesignSettings.Thousandths, tracking: InDesignSettings.Thousandths, baselineShift: Schema.Finite, marks: InDesignSettings.fields.marks });
const Kerned = Schema.Array(Schema.Struct({ index: Schema.Int, length: Schema.Int, kerning: InDesignSettings.Thousandths, baselineShift: Schema.Finite }));
const MissingFile = Schema.Struct({ code: Schema.Literal(-2) });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SEARCH]

const preserved = Effect.fnUntraced(function* (member: (typeof SEARCHES)[FindChange['mode']]['preferences'][number]) {
    const model = yield* InDesignModel;
    const { app, NothingEnum } = model;
    const target = app[member];
    const { fields } = yield* Cache.get(model.declarations, target.constructorName);
    const current = Option.liftThrowable((name: string): unknown => Reflect.get(target, name));
    const saved = Record.filterMap(fields, (row, name) => (row.settable && !model.collectionField(row) && Option.isNone(row.unavailable) && name !== AGGREGATE_FIELD ? Result.fromOption(Option.filter(current(name), Predicate.not(model.nothing)), Function.constVoid) : Result.failVoid));
    const reset = Effect.try({ try: () => Object.assign(app, { [member]: NothingEnum.NOTHING })[member], catch: HostError.refused(`reset of ${member}`) });
    return yield* Effect.acquireRelease(
        Effect.as(reset, saved),
        Function.constant(
            reset.pipe(
                Effect.orDie,
                Effect.flatMap((cleared) => Uxp.restore(cleared, saved)),
            ),
        ),
    );
});
const search = Effect.fnUntraced(function* ({ mode, find, change, options }: Pick<FindChange, 'mode' | 'find' | 'change' | 'options'>, located: unknown) {
    const members = SEARCHES[mode];
    const model = yield* InDesignModel;
    const { app, enumerator } = model;
    yield* Effect.forEach(members.preferences, preserved, { discard: true });
    const counted = Match.type<number | readonly unknown[]>().pipe(Match.when(Predicate.isNumber, identity<number>), Match.orElse(Array.length));
    const rendered = (contents: unknown): string => Array.map(Array.ensure(contents), (each) => (enumerator(each) ? `<${globalThis.String(each)}>` : globalThis.String(each))).join('');
    const searched = model.owning(members.change);
    const scopes = yield* Effect.forEach(
        Array.ensure(located),
        flow(
            Option.liftPredicate(searched),
            Effect.fromOption(() => Unmatched.make({ subject: `${mode} search target`, query: model.domObject(located) ? Option.some(located.toSpecifier()) : Option.none() })),
        ),
    );
    yield* pipe(
        Array.zip(members.preferences, [find, options, change]),
        Array.filterMap(([target, fields]) => Result.fromOption(Option.all([Option.some(app[target]), fields] as const), Function.constVoid)),
        Effect.forEach(([target, values]) => InDesignModel.write(target, values), { discard: true }),
    );
    const describe = (hit: Text | PageItem) => {
        const placed = model.text(hit) ? Option.filter(Array.head(hit.parentTextFrames), model.item) : Option.liftPredicate(hit, model.item);
        return { specifier: hit.toSpecifier(), text: model.text(hit) ? Option.some(rendered(hit.contents)) : Option.none(), page: Option.flatMap(placed, (item) => Option.fromNullishOr(item.parentPage?.name)) };
    };
    const found = yield* Effect.forEach(
        scopes,
        Effect.fnUntraced(function* (each) {
            const owner = model.owner(each);
            yield* Effect.transposeOption(Option.as(owner, Uxp.preserve(app, Record.getSomes({ activeDocument: owner }))));
            const refused = HostError.refused(`${mode} search of ${each.toSpecifier()}`);
            const [hits, matches] = yield* Effect.try({
                try: () => {
                    const held = each[members.find]();
                    return [held, Predicate.isNumber(held) ? [] : held.map(describe)] as const;
                },
                catch: refused,
            });
            const changes = yield* Effect.transposeOption(Option.as(change, Effect.try({ try: () => counted(each[members.change]()), catch: refused })));
            return { count: { matches: counted(hits), changes }, matches };
        }, Effect.scoped),
    );
    const changes = Array.getSomes(found.map(({ count }) => count.changes));
    return { count: { matches: Number.sumAll(found.map(({ count }) => count.matches)), changes: Option.map(Option.liftPredicate(changes, Array.isReadonlyArrayNonEmpty), Number.sumAll) }, matches: Array.flatMap(found, Struct.get('matches')) };
}, Effect.scoped);

// --- [READS]

const nested: (rows: readonly (typeof Flat.Type)[]) => typeof Issues.Type = Array.match({
    onEmpty: Array.empty,
    onNonEmpty: ([[depth, label, page, , details], ...rest]) => {
        const [under, after] = Array.span(rest, ([below]) => below > depth);
        return [{ label, ...Record.getSomes({ page: Option.liftPredicate(page, String.isNonEmpty) }), details: Record.fromEntries(details), rows: nested(under) }, ...nested(after)];
    },
});
const Aggregated = Schema.Tuple([Schema.String, Schema.String, Schema.Array(Flat)]).pipe(Schema.decodeTo(Issues, { decode: SchemaGetter.transform(([, , rows]) => nested(rows)), encode: SchemaGetter.forbiddenEncoding }));
const aggregated = (process: PreflightProcess) => Result.try(() => process.aggregatedResults).pipe(Result.flatMap(Schema.decodeUnknownResult(Aggregated)));
const overset = ({ stories }: Document): readonly string[] => Array.filterMap(Array.zip(stories.everyItem().overflows, stories.everyItem().id), ([overflows, id]) => (overflows === true ? Result.succeed(stories.itemByID(id).toSpecifier()) : Result.failVoid));
const savedPath = (document: Document) => Effect.flatMap(Effect.try({ try: () => document.saved, catch: HostError.refused(`saved of ${document.toSpecifier()}`) }), (saved) => (saved ? InDesignModel.file(document, 'fullName') : Effect.succeedNone));
const overrides = (text: Pick<Text, 'textHasOverrides' | 'toSpecifier'>) =>
    InDesignModel.use(({ StyleType }) => Effect.try({ try: () => ({ paragraph: text.textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE), character: text.textHasOverrides(StyleType.CHARACTER_STYLE_TYPE) }), catch: HostError.refused(`overrides of ${text.toSpecifier()}`) }));
const differing = Effect.fnUntraced(function* (object: InDesignDOMObject<unknown>, properties: object, baseValue: (name: string) => unknown, except: Schema.Struct.Fields) {
    const [{ declarations, collectionField }, encoded] = yield* Effect.all([InDesignModel, InDesignModel.encoder]);
    const { fields } = yield* Cache.get(declarations, object.constructorName);
    return Record.filterMap(fields, (row, name) => {
        if (!row.settable || collectionField(row) || Object.hasOwn(except, name)) {
            return Result.failVoid;
        }
        const value: unknown = Reflect.get(properties, name);
        return encoded(value) === encoded(baseValue(name)) ? Result.failVoid : Result.succeed(value);
    });
});
const accessors = (found: InDesignDOMObject<unknown>) =>
    pipe(
        Array.flatMap(Uxp.members(Object.getPrototypeOf(found)), ({ fields }) => fields),
        Array.dedupeWith(Equivalence.mapInput(Equivalence.String, Struct.get('name'))),
    ).filter(({ name }) => name !== AGGREGATE_FIELD);
const readings = Effect.fn('readings')(function* (found: InDesignDOMObject<unknown>, wanted: readonly string[]) {
    const { declarations } = yield* InDesignModel;
    const { fields } = yield* Cache.get(declarations, found.constructorName);
    const readable = accessors(found);
    const reading = (each: string) => Array.findFirst(readable, ({ name }) => Unmatched.named(name, each));
    const [named, missing] = Array.partition(wanted, Filter.fromPredicateOption(reading));
    yield* Array.isReadonlyArrayEmpty(missing) ? Effect.void : Unmatched.make({ subject: `field of ${found.toSpecifier()}`, query: Option.some(missing.join(', ')), candidates: Array.map(readable, Struct.get('name')) });
    return yield* Effect.forEach(
        named,
        Effect.fnUntraced(function* (accessor) {
            const subject = `${found.toSpecifier()} ${accessor.name}`;
            const row = yield* declaredMember(ObjectModel.Field, fields, accessor);
            const refusal = Effect.transposeOption(Option.map(unavailableReason(found.constructorName, accessor.name, Option.some(found), false), HostError.refused(subject)));
            const held = Option.contains(row.type, 'File') ? InDesignModel.file(found, accessor.name) : Effect.map(Effect.try({ try: (): unknown => Reflect.get(found, accessor.name), catch: HostError.refused(subject) }), Option.fromNullishOr);
            const value = yield* Effect.result(Effect.andThen(refusal, Effect.map(held, Option.getOrNull)));
            return { ...row, value };
        }),
    );
});

const declaration = Effect.fnUntraced(function* (name: string) {
    const model = yield* InDesignModel;
    const { enumeration, declarations, parents, members } = model;
    const { Class, Enumeration } = ObjectModel.Result.cases;
    const declared = yield* Effect.catchTag(Effect.asSome(Cache.get(declarations, name)), 'Unmatched', () => Effect.succeedNone);
    const { fields, methods, statics } = Option.getOrElse(declared, () => ({ fields: {}, methods: {}, statics: {} }));
    const type: unknown = Reflect.get(model, name);
    const [live] = Predicate.isFunction(type) ? Uxp.members(type.prototype) : [{ fields: [], methods: [] }];
    return yield* Option.match(enumeration(name), {
        onSome: (found) => Effect.succeed(Enumeration.make({ name, members: Array.map(members(found), (each) => ({ name: globalThis.String(each), description: Record.get(statics, globalThis.String(each)) })) })),
        onNone: () =>
            Effect.map(
                Effect.all({
                    parents: Option.isSome(declared) ? parents(name) : Effect.succeed([]),
                    fields: Effect.forEach(live.fields, (accessor) => declaredMember(ObjectModel.Field, fields, accessor)),
                    methods: Effect.forEach(live.methods, (method) => declaredMember(ObjectModel.Method, methods, { name: method })),
                }),
                (described) => Class.make({ name, ...described }),
            ),
    });
});
const fontView = Effect.fnUntraced(function* (face: Font) {
    const model = yield* InDesignModel;
    const specifier = yield* Effect.orDie(Schema.decodeUnknownEffect(Specifier.Path)(face.toSpecifier()));
    const open = Option.toArray(yield* Effect.option(InDesignModel.current));
    const searched = yield* Effect.forEach(open, (document) =>
        search({ mode: 'Text', find: Option.some({ appliedFont: { specifier } }), options: Option.some({ includeFootnotes: true, includeHiddenLayers: true, includeLockedLayersForFind: true, includeLockedStoriesForFind: true, includeMasterPages: true }), change: Option.none() }, document),
    );
    const styled = yield* Effect.try({
        try: () =>
            Array.flatMap(open, (document) => [...document.allParagraphStyles, ...document.allCharacterStyles])
                .filter(({ appliedFont, fontStyle }) => Unmatched.named(`${Predicate.isString(appliedFont) ? appliedFont : appliedFont?.fontFamily}\t${fontStyle}`, face.name))
                .map((each) => each.toSpecifier()),
        catch: HostError.refused(`uses of ${face.name}`),
    });
    const installed = yield* Effect.when(
        Effect.try({
            try: () => {
                const every = model.app.fonts.everyItem();
                return {
                    ...model.picked(Inspect.Font.fields.installed, face, ['styles', 'axes', 'features']),
                    styles: Array.dedupe(Array.filterMap(Array.zip(every.fontFamily, every.fontStyleName), ([family, named]) => (family === face.fontFamily ? Result.succeed(named) : Result.failVoid))),
                    axes: Option.map(
                        Option.liftThrowable(() => Array.zip(face.designAxesName, face.designAxesValues))(),
                        Array.map(([name, value]) => ({ name, value })),
                    ),
                    features: model
                        .members(model.OpenTypeFeature)
                        .filter((feature) => face.checkOpenTypeFeature(feature))
                        .map(globalThis.String),
                };
            },
            catch: HostError.refused(`styles and features of ${face.name}`),
        }),
        Effect.succeed(face.status.equals(model.FontStatus.INSTALLED)),
    );
    const uses = [...Array.flatMap(searched, ({ matches }) => Array.map(matches, Struct.get('specifier'))), ...styled];
    return yield* InDesignModel.picked(Inspect.Font, face, () => ({ uses, installed }));
});
const view = (model: InDesignModel['Service'], found: unknown, slice: Slice) => {
    const { picked } = model;
    return Effect.transposeOption(
        Match.value(found).pipe(
            Match.when(
                model.application,
                Effect.fnUntraced(function* (app: Application) {
                    const opened = yield* Effect.option(InDesignModel.current);
                    const documents = yield* Effect.forEach(app.documents.everyItem().getElements(), (document) => savedPath(document).pipe(Effect.bindTo('fullName'), Effect.map(Struct.assign({ document }))));
                    const window = Option.liftThrowable(() => app.activeWindow.toSpecifier())();
                    const selection = yield* Effect.forEach(
                        Option.match(opened, { onNone: Array.empty, onSome: () => app.selection }),
                        Effect.fnUntraced(function* (item: (typeof app.selection)[number]) {
                            const overridden = yield* Effect.transposeOption(Option.map(Option.liftPredicate(item, model.overridable), overrides));
                            return { ...InDesignModel.referenceUnsafe(item), contents: Option.map(Schema.decodeUnknownOption(Contents)(item), Struct.get('contents')), parentStory: Option.map(Option.liftPredicate(item, model.storied), Struct.get('parentStory')), overrides: overridden };
                        }),
                    );
                    const panels = Array.getSomes(globalThis.Array.from({ length: app.panels.length }, (_, index) => Option.map(Option.filter(Option.fromUndefinedOr(app.panels.item(index)), Struct.get('visible')), Struct.get('name'))));
                    return yield* InDesignModel.picked(
                        Inspect.Application,
                        app,
                        flow(
                            () => ({ panels, documents, current: opened }),
                            Struct.evolve({
                                documents: Array.map(({ document, fullName }) => ({ ...picked(Inspect.Application.fields.documents.value, document, ['fullName']), fullName })),
                                current: Option.map((document) => ({ document: document.toSpecifier(), window, selection, overset: overset(document), undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory } })),
                            }),
                        ),
                    );
                }),
            ),
            Match.when(
                model.document,
                Effect.fnUntraced(function* (document: Document) {
                    const [fullName, settings] = yield* Effect.all([savedPath(document), InDesignSettings.read(document.textDefaults)]);
                    return yield* InDesignModel.picked(Inspect.Document, document, () => ({
                        fullName,
                        overset: overset(document),
                        links: Record.map(Array.groupBy(document.links.everyItem().status, globalThis.String), Array.length),
                        textDefaults: { ...picked(Inspect.Document.fields.textDefaults, document.textDefaults, ['settings']), settings },
                    }));
                }),
            ),
            Match.when(model.page, (page) => Effect.flatMap(InDesignModel.extent(page), (extent) => InDesignModel.picked(Inspect.Page, page, Function.constant({ extent })))),
            Match.whenOr(
                model.spread,
                model.masterSpread,
                Effect.fnUntraced(function* (spread: Spread | MasterSpread) {
                    const pages = yield* Effect.forEach(spread.pages.everyItem().getElements(), (page) => InDesignModel.extent(page).pipe(Effect.bindTo('extent'), Effect.map(Struct.assign({ page }))));
                    return yield* InDesignModel.picked(Inspect.Spread, spread, flow(Function.constant({ pages }), Struct.evolve({ pages: Array.map(({ page, extent }) => ({ ...picked(Inspect.Spread.cases.Spread.fields.pages.value, page, ['extent']), extent })) })));
                }),
            ),
            Match.when(model.layer, (layer) => InDesignModel.picked(Inspect.Layer, layer)),
            Match.when(
                model.style,
                Effect.fnUntraced(function* (style: ParagraphStyle | CharacterStyle | ObjectStyle | TableStyle | CellStyle) {
                    const ancestors = model.ancestors(style.parent);
                    const refused = HostError.refused(style.toSpecifier());
                    const properties = yield* Effect.try({ try: () => style.properties, catch: refused });
                    const { basedOn } = properties;
                    const container = Option.orElse(Array.findFirst(ancestors, model.document), () => Option.some(model.app));
                    const base = Predicate.isString(basedOn) ? yield* InDesignModel.object(style.constructorName, basedOn, container) : basedOn;
                    const baseProperties = yield* Effect.try({ try: (): object => (model.domObject(base) ? base.properties : {}), catch: refused });
                    const own = yield* differing(style, properties, (name) => Reflect.get(baseProperties, name), Inspect.Style.cases.ParagraphStyle.fields);
                    const settings = yield* Effect.transposeOption(Option.map(Option.liftPredicate(style, model.paragraphStyle), InDesignSettings.read));
                    return yield* InDesignModel.picked(
                        Inspect.Style,
                        style,
                        flow(() => ({ group: ancestors, basedOn: model.ancestors(basedOn, 'basedOn'), differing: own, settings }), Struct.evolve({ group: Array.filterMap((ancestor) => (model.named(ancestor) && !model.document(ancestor) ? Result.succeed(ancestor.name) : Result.failVoid)) })),
                    );
                }),
            ),
            Match.when(model.swatch, (swatch) => InDesignModel.picked(Inspect.Swatch, swatch)),
            Match.when(model.link, (link) => InDesignModel.picked(Inspect.Link, link)),
            Match.when(model.story, (story) =>
                InDesignModel.picked(
                    Inspect.Story,
                    story,
                    flow(
                        () => ({ overset: story.textContainers, styles: story.paragraphs.everyItem().appliedParagraphStyle }),
                        Struct.evolve({
                            overset: (containers) => story.length - Number.sumAll(Array.map(containers, flow(Struct.get('characters'), Struct.get('length')))),
                            styles: flow(Array.map(Struct.get('name')), Array.dedupe),
                        }),
                    ),
                ),
            ),
            Match.whenOr(
                model.text,
                model.frame,
                Effect.fnUntraced(function* (source: Text | TextFrame | TextPath) {
                    const text = model.frame(source) ? source.texts.item(0) : source;
                    const [overridden, settings] = yield* Effect.all([overrides(text), InDesignSettings.read(text)]);
                    const ranges = slice.of(text.textStyleRanges.everyItem().getElements());
                    const run = Inspect.Text.fields.runs.value;
                    const styles = yield* Effect.try({
                        try: () =>
                            Record.map(
                                Record.fromIterableWith(
                                    Array.flatMap(ranges, (range) => [range.appliedParagraphStyle, range.appliedCharacterStyle]),
                                    (style) => [style.toSpecifier(), style],
                                ),
                                Struct.get('properties'),
                            ),
                        catch: HostError.refused(`styles of ${text.toSpecifier()}`),
                    });
                    const runs = yield* Effect.forEach(
                        ranges,
                        Effect.fnUntraced(function* (range: TextStyleRange) {
                            const refused = HostError.refused(range.toSpecifier());
                            const [properties, paragraphStyle, characterStyle] = yield* Effect.try({ try: () => [range.properties, { ...styles[range.appliedParagraphStyle.toSpecifier()] }, { ...styles[range.appliedCharacterStyle.toSpecifier()] }] as const, catch: refused });
                            const inherited = (name: string): unknown => {
                                const value: unknown = Reflect.get(characterStyle, name);
                                return value === null || value === '' || model.nothing(value) ? Reflect.get(paragraphStyle, name) : value;
                            };
                            const [own, held] = yield* Effect.all([differing(range, properties, inherited, { ...run.fields, ...Inspect.Text.fields }), InDesignSettings.read(range)]);
                            return yield* Effect.try({ try: () => ({ ...picked(run, range, ['designAxes', 'settings', 'differing']), designAxes: Result.try(flow(Function.constant(range), Struct.get('designAxes'))), settings: held, differing: own }), catch: refused });
                        }),
                    );
                    return yield* InDesignModel.picked(Inspect.Text, text, () => ({ overrides: overridden, runs, settings }));
                }),
            ),
            Match.when(model.font, fontView),
            Match.when(model.preflightProcess, (process) => InDesignModel.picked(Inspect.PreflightProcess, process, () => ({ aggregatedResults: aggregated(process) }))),
            Match.option,
        ),
    );
};

// --- [TEXT]

const textTarget = Effect.fnUntraced(function* (path: string) {
    const [{ app, textContainer, text, textDefault }, specifier] = yield* Effect.all([InDesignModel, Schema.decodeUnknownEffect(Specifier.Path)(path)]);
    const found = specifier.root ? app.textDefaults : yield* InDesignModel.located(specifier);
    const target = text(found) || textDefault(found) || !textContainer(found) ? found : found.texts.item(0);
    return yield* text(target) || textDefault(target) ? Effect.succeed(target) : Unmatched.make({ subject: 'text', query: Option.some(path) });
});
const textRange = (target: Text | TextDefault) => InDesignModel.use(({ text }) => Effect.filterOrFail(Effect.succeed(target), text, () => Unmatched.make({ subject: 'text range', query: Option.some(target.toSpecifier()) })));
const kerningMethod = (method: 'Manual' | 'Metrics') => InDesignModel.use(({ app }) => Effect.try({ try: () => app.translateKeyString(`$ID/${method}`), catch: HostError.refused(`$ID/${method}`) }));
const pointAfterBase = (text: Text) =>
    Effect.try({
        try: () => {
            const last = text.insertionPoints.lastItem();
            const container = last.parent;
            const marks = Array.unfold(last.index - 1, (index) => (index >= 0 && MARK.test(globalThis.String(container.characters.item(index).contents)) ? Option.some([index, index - 1] as const) : Option.none()));
            return last.index > marks.length ? Option.some(container.insertionPoints.item(last.index - marks.length)) : Option.none();
        },
        catch: HostError.refused(`base letter before ${text.toSpecifier()}`),
    }).pipe(Effect.flatMap(Effect.fromOption(() => Unmatched.make({ subject: 'base letter', query: Option.some(text.toSpecifier()) }))));

// --- [COMMANDS]

const checked = Effect.fn('checked')(function* (document: Document, named: Option.Option<string>) {
    const { app } = yield* InDesignModel;
    const { preflightOff, preflightWorkingProfile } = document.preflightOptions;
    const profiles = [...app.preflightProfiles.everyItem().getElements(), ...document.preflightProfiles.everyItem().getElements()];
    const live = preflightOff ? Option.none() : Option.some(document.activeProcess);
    const applied = Option.flatMap(live, (process) => Option.fromNullishOr(process.appliedProfile));
    const requested = Option.orElse(named, () => applied).pipe(Option.getOrElse(() => preflightWorkingProfile));
    const chosen = yield* Predicate.isString(requested) ? Unmatched.single(profiles, 'preflight profile', Option.some(requested), (each) => [each.name]) : Effect.succeed(requested);
    const refused = HostError.refused(`preflight of ${document.toSpecifier()} under ${chosen.name}`);
    const finished = Option.isNone(named) ? Option.flatMap(live, flow(aggregated, Result.getSuccess)) : Option.none();
    const waited = Effect.acquireUseRelease(
        Effect.try({ try: () => app.preflightProcesses.add(document, chosen), catch: refused }),
        (process) =>
            Effect.try({
                try: () => {
                    process.waitForProcess();
                    return aggregated(process);
                },
                catch: refused,
            }).pipe(Effect.flatMap(flow(Result.mapError(refused), Effect.fromResult))),
        (process) => Effect.try({ try: () => process.remove(), catch: refused }).pipe(Effect.orDie),
    );
    return yield* Effect.all({
        targetObject: InDesignModel.reference(document),
        appliedProfile: InDesignModel.reference(chosen),
        aggregatedResults: Option.match(finished, { onSome: Effect.succeed, onNone: () => waited }),
    });
});
const actions = InDesignModel.use(({ app }) =>
    InDesignModel.read(
        Menu.Result,
        'menu actions',
        flow(
            () => {
                const every = app.menuActions.everyItem();
                return Array.zip(Array.zip(every.id, every.name), every.area);
            },
            Array.map(([[id, name], area]) => ({ id, name, area })),
        ),
    ),
);
const scriptPreferences = Effect.flatMap(InDesignModel, ({ app, MeasurementUnits, UserInteractionLevels }) => Uxp.preserve(app.scriptPreferences, { enableRedraw: false, measurementUnit: MeasurementUnits.POINTS, userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const inDesignHandlers = InDesign.Contract.of({
    inspect: Effect.fn('InDesign.Contract.inspect')(function* ({ target, fields, slice }: Inspect) {
        const [found, model, path] = yield* Effect.all([InDesignModel.resolve(target), InDesignModel, target.path]);
        if (!model.domObject(found)) {
            const items = yield* Effect.forEach(slice.of(found), (item) => Effect.zipWith(InDesignModel.reference(item), readings(item, Option.getOrElse(fields, Array.empty)).pipe(Effect.bindTo('fields')), Struct.assign));
            return yield* InDesignModel.read(Inspect.Items, path, () => ({ _tag: 'Items', specifier: path, total: found.length, items }));
        }
        const [declared, shown] = yield* Effect.all([Cache.get(model.declarations, found.constructorName), view(model, found, slice)]);
        const entries = yield* readings(
            found,
            Option.getOrElse(fields, () => Option.match(shown, { onNone: () => Array.map(accessors(found), Struct.get('name')), onSome: () => Record.keys(Record.filter(declared.fields, model.collectionField)) })),
        );
        const [collections, scalars] = Array.partition(
            entries,
            Match.type<(typeof entries)[number]>().pipe(
                Match.when({ value: { _tag: 'Success', success: model.collection } }, ({ name, value }) => ({ name, type: value.success.constructor.name, length: value.success.length, specifier: value.success.everyItem().toSpecifier() })),
                Match.result,
            ),
        );
        return Inspect.Object.make({ ...(yield* InDesignModel.reference(found)), view: shown, fields: yield* InDesignModel.read(Inspect.Readings, found.toSpecifier(), () => scalars), collections });
    }),
    change: (payload: Change) => Effect.flatMap(InDesignModel.located(payload.target), (found) => InDesignModel.written(payload.label, found, InDesignModel.write(found, payload.values))),
    call: Effect.fn('InDesign.Contract.call')(function* (payload: Call) {
        const [model, found] = yield* Effect.all([InDesignModel, InDesignModel.located(payload.target)]);
        const before = yield* InDesignModel.reference(found);
        const { fields } = yield* Cache.get(model.declarations, before.type);
        const { receiver, kind } = yield* Option.match(payload.collection, {
            onNone: () => Effect.succeed({ receiver: found, kind: before.type }),
            onSome: (field) => {
                const member: unknown = Reflect.get(found, field);
                return model.collection(member) ? Effect.succeed({ receiver: member, kind: member.constructor.name }) : Unmatched.make({ subject: `collection of ${before.specifier}`, query: Option.some(field), candidates: Record.keys(Record.filter(fields, model.collectionField)) });
            },
        });
        const { methods } = yield* Cache.get(model.declarations, kind);
        const [method, { parameters, returns }] = yield* Unmatched.single(Record.toEntries(methods), `method of ${kind}`, Option.some(payload.method), ([name]) => [name]);
        const required = parameters.filter(({ optional }) => !optional).length;
        const positional = payload.arguments;
        yield* positional.length < required || positional.length > parameters.length
            ? new Cause.IllegalArgumentError(`${kind} ${method} takes ${required === parameters.length ? required : `${required} to ${parameters.length}`} arguments (${Array.map(parameters, ({ name, optional }) => (optional ? `${name}?` : name)).join(', ')}), given ${positional.length}`)
            : Effect.void;
        const decode = InDesignModel.decoder(found, yield* Effect.cached(InDesignModel.fonts));
        const positioned = yield* Effect.forEach(Array.zip(positional, parameters), ([value, parameter]) => decode(Option.some(parameter), Option.map(returns, Struct.get('type')))(value));
        const properties = Option.map(payload.properties, (supplied) => flow(Array.ensure<unknown>, Array.findFirst(model.domObject), Effect.fromOption(Function.constant(new Cause.IllegalArgumentError(`${kind} ${method} returned no object for properties`))), Effect.flatMap(InDesignModel.write(supplied))));
        return yield* InDesignModel.commit(
            payload.label,
            Effect.try({ try: (): unknown => Reflect.apply(Reflect.get(receiver, method), receiver, positioned), catch: HostError.refused(`${kind} ${method}`) }).pipe(
                Effect.tap((returned) => Effect.transposeOption(Option.map(properties, Function.apply(returned)))),
                Effect.flatMap((returned) => InDesignModel.read(Call.Result, payload.label, flow(Function.constant({ ...before, result: Option.fromUndefinedOr(returned) }), Struct.evolve({ valid: flow(Function.constant(found), Struct.get('isValid')) })))),
            ),
        );
    }),
    find_change: (payload: FindChange) =>
        Option.match(payload.scope, { onSome: InDesignModel.resolve, onNone: () => InDesignModel.current }).pipe(
            Effect.flatMap((found: unknown) => (Option.isSome(payload.change) ? InDesignModel.commit(payload.label, search(payload, found)) : search(payload, found))),
            Effect.map(({ count, matches }) => ({ count, matches: payload.slice.of(matches) })),
        ),
    font: Effect.fn('InDesign.Contract.font')(function* ({ name, text, file }: FontFile) {
        const [face, model, { fs }, path] = yield* Effect.all([Effect.flatMap(InDesignModel.fonts, (lookup) => lookup(name)), InDesignModel, Uxp, Path.Path]);
        const subset = Effect.as(Effect.try({ try: () => face.createSubsetFont(text, file), catch: HostError.refused(`subset of ${face.name}`) }), Option.some({ file, postscriptName: Option.none<string>() }));
        const source = face.status.equals(model.FontStatus.INSTALLED)
            ? yield* Effect.flatMap(path.toFileUrl(face.location), (url) => Effect.try({ try: () => fs.lstatSync(url.href), catch: HostError.refused(face.location) })).pipe(
                  Effect.as(Option.some({ file: face.location, postscriptName: Option.some(face.postscriptName) })),
                  Effect.catchTags({ BadArgument: () => subset, HostError: (error) => (Schema.is(MissingFile)(error.cause) ? subset : error) }),
              )
            : Option.none();
        return { font: yield* fontView(face), source };
    }),
    menu: ({ area }: Menu) =>
        Effect.flatMap(
            actions,
            Unmatched.matching('menu area', area, (row) => [row.area]),
        ),
    invoke: Effect.fn('InDesign.Contract.invoke')(function* (payload: Invoke) {
        const [{ app }, listed] = yield* Effect.all([InDesignModel, actions]);
        const chosen = yield* Unmatched.single(listed, 'menu action', Option.some(globalThis.String(payload.action)), ({ id, name, area }) => [`${id} ${name} (${area})`, globalThis.String(id), name]);
        yield* InDesignModel.commit(payload.label, Effect.try({ try: () => app.menuActions.itemByID(chosen.id).invoke(), catch: HostError.refused(chosen.name) }));
        return chosen;
    }),
    preflight: Effect.fn('InDesign.Contract.preflight')(function* ({ document, profile }: Preflight) {
        const model = yield* InDesignModel;
        const open = model.app.documents.everyItem().getElements();
        const specifiers = Array.map(open, (each) => each.toSpecifier());
        const targets = yield* Option.match(document, {
            onNone: () => Effect.succeed(open.filter((each) => !each.preflightOptions.preflightOff)),
            onSome: (specifier) => Effect.map(Effect.filterOrElse(InDesignModel.resolve(specifier), model.document, Function.constant(Effect.flatMap(specifier.path, (path) => Unmatched.make({ subject: 'open document', query: Option.some(path), candidates: specifiers })))), Array.of),
        });
        return yield* Effect.forEach(targets, (target) => checked(target, profile));
    }),
    adjust: Effect.fn('InDesign.Contract.adjust')(function* (payload: Adjust) {
        const { kerning = 0, tracking = 0, baselineShift = 0, marks = { x: 0, y: 0 } } = payload.deltas;
        const [{ text }, target, manual, encoded] = yield* Effect.all([InDesignModel, textTarget(payload.target), kerningMethod('Manual'), Effect.orDie(Schema.encodeEffect(Deltas)({ kerning, tracking, baselineShift, marks }))]);
        const points: readonly InsertionPoint[] = kerning === 0 ? [] : [yield* Effect.flatMap(textRange(target), pointAfterBase)];
        const summed = (run: TextStyleRange | TextDefault) => Record.filterMap({ tracking: encoded.tracking, baselineShift: encoded.baselineShift * run.pointSize, ...encoded.marks }, (amount, member) => (amount === 0 ? Result.failVoid : Result.succeed(run[member] + amount)));
        const sums = yield* Effect.try({ try: () => Array.map(text(target) ? target.textStyleRanges.everyItem().getElements() : [target], (run) => [run, summed(run)] as const), catch: HostError.refused(`style runs of ${target.toSpecifier()}`) });
        const kerned = yield* Effect.try({ try: () => Array.map(points, (point) => [point, { kerningValue: (point.kerningMethod === manual ? point.kerningValue : 0) + encoded.kerning }] as const), catch: HostError.refused(`kerning of ${target.toSpecifier()}`) });
        return yield* InDesignModel.written(payload.label, target, Effect.forEach([...sums, ...kerned], ([run, values]) => InDesignModel.write(run, values)).pipe(Effect.map(flow(Array.flatten, Array.dedupe))));
    }),
    kerns: Effect.fn('InDesign.Contract.kerns')(function* (payload: Kerns) {
        const [text, manual, metrics, rows] = yield* Effect.all([Effect.flatMap(textTarget(payload.target), textRange), kerningMethod('Manual'), kerningMethod('Metrics'), Effect.orDie(Schema.encodeEffect(Kerned)(payload.kerns))]);
        const refused = HostError.refused(`insertion points of ${text.toSpecifier()}`);
        const count = yield* Effect.try({ try: () => text.insertionPoints.length, catch: refused });
        const outOfRange = [...Option.toArray(payload.restore), ...rows].filter(({ index, length }) => index + length >= count);
        yield* Array.isReadonlyArrayEmpty(outOfRange) ? Effect.void : Unmatched.make({ subject: `run in characters 0 to ${count - 2}`, query: Option.some(outOfRange.map(({ index, length }) => `${index} to ${index + length - 1}`).join(', ')) });
        const characters = ({ index, length }: { readonly index: number; readonly length: number }) => text.characters.itemByRange(index, index + length - 1);
        const restored = Effect.transposeOption(Option.map(payload.restore, (run) => Effect.flatMap(Effect.try({ try: () => characters(run).getElements(), catch: refused }), Effect.forEach(InDesignModel.write({ kerningMethod: metrics }), { discard: true }))));
        const shifted = (row: (typeof rows)[number]) =>
            characters(row)
                .getElements()
                .map((character) => [character, { baselineShift: row.baselineShift * character.pointSize }] as const);
        const kerned = Effect.try({
            try: () => {
                const methods = text.insertionPoints.everyItem().kerningMethod;
                const kept = rows.filter(({ index }) => methods[index] !== manual || text.insertionPoints.item(index).kerningValue === 0);
                const points = kept.filter(({ kerning }) => kerning !== 0).map(({ index, kerning }) => [text.insertionPoints.item(index), { kerningValue: kerning }] as const);
                return { kept: kept.length, members: [...points, ...kept.filter(({ baselineShift }) => baselineShift !== 0).flatMap(shifted)] };
            },
            catch: refused,
        }).pipe(
            Effect.tap(
                flow(
                    Struct.get('members'),
                    Effect.forEach(([target, values]) => InDesignModel.write(target, values), { discard: true }),
                ),
            ),
            Effect.map(Struct.get('kept')),
        );
        return yield* InDesignModel.written(payload.label, text, Effect.andThen(restored, kerned));
    }),
    runs: Effect.fn('InDesign.Contract.runs')(function* (payload: Runs) {
        const [{ text, textContainer }, target] = yield* Effect.all([InDesignModel, textTarget(payload.target)]);
        if (!text(target)) {
            return yield* Effect.try({ try: () => ({ ...InDesignModel.referenceUnsafe(target), start: 0, length: 0, runs: [{ text: '', font: target.appliedFont.name, axes: target.designAxes }] }), catch: HostError.refused(`applied font of ${target.toSpecifier()}`) });
        }
        const container = target.parent;
        const whole = yield* textContainer(container) ? Effect.succeed(container.texts.item(0)) : Unmatched.make({ subject: 'text container', query: Option.some(payload.target) });
        const ranges = yield* Effect.try({
            try: () => Array.map(whole.textStyleRanges.everyItem().getElements(), (run) => ({ text: Predicate.isString(run.contents) ? run.contents : '\n'.repeat(run.length), font: run.appliedFont.name, axes: run.designAxes })),
            catch: HostError.refused(`style ranges of ${whole.toSpecifier()}`),
        });
        return { ...(yield* InDesignModel.reference(container)), start: target.index, length: target.length, runs: ranges };
    }),
    typeset: Effect.fn('InDesign.Contract.typeset')(function* (payload: Typeset) {
        const target = yield* textTarget(payload.target);
        yield* Effect.transposeOption(Option.map(payload.settings, (settings) => InDesignModel.commit(payload.label, InDesignSettings.write(target, settings))));
        return yield* InDesignSettings.read(target);
    }),
    execute_script: Effect.fn('InDesign.Contract.execute_script')(function* (payload: ExecuteScript) {
        const { exported } = yield* InDesignModel;
        const given = { ...Object.fromEntries(exported), resolve: flow(Schema.decodeUnknownEffect(Specifier.Path), Effect.flatMap(InDesignModel.resolve), Effect.runSyncWith(yield* Effect.context<InDesignModel>())) };
        const result = yield* InDesignModel.commit(payload.label, yield* Uxp.evaluate(given, payload.code));
        return yield* InDesignModel.read(ExecuteScript.Result, payload.label, () => ({ result }));
    }),
    open: Effect.fn('InDesign.Contract.open')(function* (payload: Open) {
        const [{ app }, path] = yield* Effect.all([InDesignModel, Path.Path]);
        const native = yield* path.fromFileUrl(payload.file);
        return yield* InDesignModel.reference(yield* Effect.try({ try: () => app.open(native, false), catch: HostError.refused(payload.label) }));
    }),
    render: Effect.fn('InDesign.Contract.render')(function* ({ target, longestSide, options, file, label }: Render) {
        const [model, specifier, path] = yield* Effect.all([InDesignModel, Schema.decodeUnknownEffect(Specifier.Path)(target), Path.Path]);
        const { app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum } = model;
        const found = yield* Effect.filterOrFail(
            InDesignModel.resolve(specifier),
            (value): value is Page | Spread | PageItem => (model.page(value) && model.spread(value.parent)) || model.spread(value) || model.item(value),
            () => Unmatched.make({ subject: 'document page, spread, or page item', query: Option.some(target) }),
        );
        const { source, placed, range } = Match.value(found).pipe(
            Match.when(model.page, (each) => ({ source: each.parent.parent, placed: [each], range: { pageString: `+${each.documentOffset + 1}`, exportingSpread: false } })),
            Match.when(model.spread, (each) => ({ source: each.parent, placed: each.pages.everyItem().getElements(), range: { pageString: `+${each.pages.firstItem().documentOffset + 1}`, exportingSpread: true } })),
            Match.orElse((each) => ({ source: each, placed: [each], range: {} })),
        );
        const { top, left, bottom, right } = yield* InDesignModel.extent(...placed);
        const pointsPerInch = 72;
        const preferences = app.pngExportPreferences;
        const { fields } = yield* Cache.get(model.declarations, preferences.constructorName);
        const { minimum, maximum } = yield* Effect.orDie(Effect.fromOption(Record.get(fields, 'exportResolution')));
        const bounds = yield* Effect.orDie(Effect.fromOption(Option.all({ minimum, maximum })));
        const defaults = {
            pngColorSpace: PNGColorSpaceEnum.RGB,
            pngQuality: PNGQualityEnum.MAXIMUM,
            transparentBackground: false,
            antiAlias: true,
            useDocumentBleeds: false,
            pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
            pngSuffix: '',
            exportingHiddenSpread: true,
            exportResolution: Number.clamp((longestSide * pointsPerInch) / Math.max(right - left, bottom - top), bounds),
            ...range,
        };
        const given = yield* Schema.decodeEffect(Value.Fields)(Option.getOrElse(options, Record.empty));
        yield* Uxp.preserve(preferences, defaults, [...Struct.keys(defaults), ...Record.keys(given)]);
        yield* InDesignModel.write(preferences, given);
        const native = yield* path.fromFileUrl(file);
        yield* Effect.try({ try: () => source.exportFile(ExportFormat.PNG_FORMAT, native), catch: HostError.refused(label) });
    }, Effect.scoped),
    object_model: Effect.fn('InDesign.Contract.object_model')(function* (payload: ObjectModel) {
        const model = yield* InDesignModel;
        return yield* payload.describe(model.classes, model.enumerations, declaration);
    }),
});
const linkInDesign = Effect.gen(function* () {
    const [model, permit] = yield* Effect.all([InDesignModel.make, Semaphore.make(1)]);
    const scripted = <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.scopedWith((scope) => Effect.andThen(Scope.provide(scriptPreferences, scope), effect)).pipe(permit.withPermit, Effect.provideService(InDesignModel, model));
    const server = Layer.merge(InDesign.Contract.toLayer(inDesignHandlers), Layer.succeed(InDesign.ScriptPreferences)(scripted)).pipe(Layer.provide(Layer.succeed(InDesignModel)(model)));
    const application = Effect.tryPromise({ try: () => model.app.fullName, catch: HostError.refused('application fullName') }).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Uxp.Entry)),
        Effect.map(({ url }) => new URL(url)),
    );
    return { linked: Peer.dial(InDesign.Contract, server, application), scripted };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { inDesignHandlers, linkInDesign };
