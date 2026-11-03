import type { CellStyle, CharacterStyle, InDesignDOMObject, InsertionPoint, MasterSpread, ObjectStyle, PageItem, ParagraphStyle, TableStyle, TextDefault, TextFrame, TextStyleRange } from 'adobe:indesign';
import { ExecuteScript, HostError, ObjectModel, type Open, type Render } from '@rasm/automation/requests';
import type { Adjust, Kerns, Runs, Typeset } from '@rasm/automation/typesetting';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cache, Cause, Effect, flow, identity, Match, Number, Option, Path, Predicate, pipe, Record, Result, Schema, SchemaGetter, Struct } from 'effect';
import { Application, Call, type Change, Document, FindChange, Font, type FontFile, InDesign, Inspect, type Invoke, Layer, Link, Menu, Page, Preflight, PreflightProcess, Spread, Story, Style, Swatch, Text } from '#/contract';
import { Dictionary } from '#/dom/dictionary';
import { Model } from '#/dom/model';
import { HostSettings } from '#/dom/settings';
import { Slice } from '#/dom/slice';
import { Specifier } from '#/dom/specifier';
import { commit, written } from '#/dom/transactions';
import { Value } from '#/dom/value';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARK = /^\p{M}+$/v;
const MODES = {
    Text: { find: 'findText', change: 'changeText', preferences: ['findTextPreferences', 'findChangeTextOptions', 'changeTextPreferences'] },
    Grep: { find: 'findGrep', change: 'changeGrep', preferences: ['findGrepPreferences', 'findChangeGrepOptions', 'changeGrepPreferences'] },
    Glyph: { find: 'findGlyph', change: 'changeGlyph', preferences: ['findGlyphPreferences', 'findChangeGlyphOptions', 'changeGlyphPreferences'] },
    Object: { find: 'findObject', change: 'changeObject', preferences: ['findObjectPreferences', 'findChangeObjectOptions', 'changeObjectPreferences'] },
    Color: { find: 'findColor', change: 'changeColor', preferences: ['findColorPreferences', 'findChangeColorOptions', 'changeColorPreferences'] },
    Transliterate: { find: 'findTransliterate', change: 'changeTransliterate', preferences: ['findTransliteratePreferences', 'findChangeTransliterateOptions', 'changeTransliteratePreferences'] },
} as const satisfies Record<FindChange['mode'], { readonly find: string; readonly change: string; readonly preferences: readonly [string, string, string] }>;

// --- [MODELS] --------------------------------------------------------------------------

const Contents = Schema.Struct({ contents: Schema.Json });
const Issues = Preflight.Result.value.fields.aggregatedResults;
const Flat = Schema.Tuple([Schema.Int, Schema.String, Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.String, Schema.String]))]);
const Deltas = Schema.Struct({ kerning: HostSettings.Thousandths, tracking: HostSettings.Thousandths, baselineShift: Schema.Finite, marks: HostSettings.fields.marks });
const Kerned = Schema.Array(Schema.Struct({ index: Schema.Int, length: Schema.Int, kerning: HostSettings.Thousandths, baselineShift: Schema.Finite }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SEARCH]

const preserved = Effect.fnUntraced(function* (member: (typeof MODES)[FindChange['mode']]['preferences'][number]) {
    const model = yield* Model;
    const { app, NothingEnum } = model;
    const target = app[member];
    const { fields } = yield* Cache.get(model.declarations, target.constructorName);
    const current = (name: string) => Option.filter(Option.liftThrowable((): unknown => Reflect.get(target, name))(), Predicate.not(model.nothing));
    const writable = Record.filter(fields, (row, name) => row.settable && !model.collectionField(row) && Option.isNone(row.unavailable) && name !== Dictionary.aggregate);
    const reset = Effect.try({ try: () => Object.assign(app, { [member]: NothingEnum.NOTHING })[member], catch: HostError.refused(`reset of ${member}`) });
    return yield* Effect.acquireRelease(Effect.sync(() => Record.getSomes(Record.map(writable, (_, name) => current(name)))).pipe(Effect.tap(() => reset)), (saved) =>
        reset.pipe(
            Effect.orDie,
            Effect.flatMap((cleared) => Uxp.restore(cleared, saved)),
        ),
    );
});
const search = Effect.fnUntraced(function* ({ mode, find: sought, change: replacing, options }: Pick<FindChange, 'mode' | 'find' | 'change' | 'options'>, located: unknown) {
    const { find, change, preferences } = MODES[mode];
    const model = yield* Model;
    const { app, enumerator } = model;
    yield* Effect.forEach(preferences, preserved, { discard: true });
    const counted = Match.type<number | readonly unknown[]>().pipe(Match.when(Predicate.isNumber, identity<number>), Match.orElse(Array.length));
    const rendered = (contents: unknown): string => Array.map(Array.ensure(contents), (each) => (enumerator(each) ? `<${String(each)}>` : String(each))).join('');
    const searched = model.owning<InDesignDOMObject<unknown> & Pick<import('adobe:indesign').Document, typeof find | typeof change>>(change);
    const scopes = yield* Effect.forEach(
        Array.ensure(located),
        flow(
            Option.liftPredicate(searched),
            Effect.fromOption(() => Unmatched.make({ subject: `${mode} search target`, query: Option.map(Option.liftPredicate(located, model.domObject), (target) => target.toSpecifier()) })),
        ),
    );
    yield* Effect.forEach(Array.zip(preferences, [sought, options, replacing]), ([target, fields]) => Effect.transposeOption(Option.map(fields, (values) => Model.write(app[target], values))), { discard: true });
    const describe = (hit: import('adobe:indesign').Text | PageItem) => {
        const frame = model.text(hit) ? hit.parentTextFrames[0] : undefined;
        return model.text(hit) ? { specifier: hit.toSpecifier(), text: rendered(hit.contents), page: frame !== undefined && frame.constructorName !== 'TextPath' ? frame.parentPage?.name : undefined } : { specifier: hit.toSpecifier(), page: model.item(hit) ? hit.parentPage?.name : undefined };
    };
    const found = yield* Effect.forEach(
        scopes,
        Effect.fnUntraced(function* (each) {
            yield* Effect.transposeOption(Option.map(model.owner(each), (context) => Uxp.preserve(app, { activeDocument: context })));
            return yield* Effect.try({
                try: () => {
                    const hits = each[find]();
                    const matches = Predicate.isNumber(hits) ? [] : hits.map(describe);
                    return { count: { matches: counted(hits), changes: Option.map(replacing, () => counted(each[change]())) }, matches };
                },
                catch: HostError.refused(`${mode} search of ${each.toSpecifier()}`),
            });
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
        return [{ label, ...Record.getSomes({ page: Option.liftPredicate(page, (name) => name.length > 0) }), details: Record.fromEntries(details), rows: nested(under) }, ...nested(after)];
    },
});
const Aggregated = Schema.Tuple([Schema.String, Schema.String, Schema.Array(Flat)]).pipe(Schema.decodeTo(Issues, { decode: SchemaGetter.transform(([, , rows]) => nested(rows)), encode: SchemaGetter.forbiddenEncoding }));
const aggregated = (process: import('adobe:indesign').PreflightProcess) => Result.try(() => process.aggregatedResults).pipe(Result.flatMap(Schema.decodeUnknownResult(Aggregated)));
const overset = ({ stories }: import('adobe:indesign').Document): readonly string[] => Array.filterMap(Array.zip(stories.everyItem().overflows, stories.everyItem().id), ([overflows, id]) => (overflows === true ? Result.succeed(stories.itemByID(id).toSpecifier()) : Result.failVoid));
const fullName = (document: import('adobe:indesign').Document) => Effect.flatMap(Effect.try({ try: () => document.saved, catch: HostError.refused(`saved of ${document.toSpecifier()}`) }), (saved) => (saved ? Model.file(document, 'fullName') : Effect.succeedNone));
const overrides = (text: Pick<import('adobe:indesign').Text, 'textHasOverrides' | 'toSpecifier'>) =>
    Model.use(({ StyleType }) => Effect.try({ try: () => ({ paragraph: text.textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE), character: text.textHasOverrides(StyleType.CHARACTER_STYLE_TYPE) }), catch: HostError.refused(`overrides of ${text.toSpecifier()}`) }));
const differing = Effect.fnUntraced(function* (object: InDesignDOMObject<unknown>, properties: object, baseValue: (name: string) => unknown, except: Schema.Struct.Fields) {
    const [{ declarations, collectionField }, encoded] = yield* Effect.all([Model, Model.encoder]);
    const { fields } = yield* Cache.get(declarations, object.constructorName);
    return Record.filterMap(fields, (row, name) => (row.settable && !collectionField(row) && !Object.hasOwn(except, name) && encoded(Reflect.get(properties, name)) !== encoded(baseValue(name)) ? Result.succeed<unknown>(Reflect.get(properties, name)) : Result.failVoid));
});
const accessors = (found: InDesignDOMObject<unknown>) =>
    pipe(
        Array.flatMap(Uxp.members(Object.getPrototypeOf(found)), ({ fields }) => fields),
        Array.dedupeWith((left, right) => left.name === right.name),
    ).filter(({ name }) => name !== Dictionary.aggregate);
const readings = Effect.fn('readings')(function* (found: InDesignDOMObject<unknown>, wanted: readonly string[]) {
    const { declarations } = yield* Model;
    const { fields } = yield* Cache.get(declarations, found.constructorName);
    const readable = accessors(found);
    const [named, missing] = Array.partition(wanted, (each) =>
        Result.fromOption(
            Array.findFirst(readable, ({ name }) => Unmatched.named(name, each)),
            () => each,
        ),
    );
    yield* Array.isReadonlyArrayEmpty(missing) ? Effect.void : Unmatched.make({ subject: `field of ${found.toSpecifier()}`, query: Option.some(missing.join(', ')), candidates: Array.map(readable, Struct.get('name')) });
    return yield* Effect.forEach(
        named,
        Effect.fnUntraced(function* (accessor) {
            const subject = `${found.toSpecifier()} ${accessor.name}`;
            const row = yield* Option.match(Record.get(fields, accessor.name), { onNone: () => Dictionary.undeclared(ObjectModel.Field, accessor), onSome: Effect.succeed });
            const refusal = Effect.transposeOption(Option.map(Dictionary.unavailable(found.constructorName, accessor.name, Option.some(found), false), (because) => HostError.refused(`${subject}: ${because}`)()));
            const held = Option.contains(row.type, 'File') ? Model.file(found, accessor.name) : Effect.map(Effect.try({ try: (): unknown => Reflect.get(found, accessor.name), catch: HostError.refused(subject) }), Option.fromNullishOr);
            const value = yield* Effect.result(Effect.andThen(refusal, Effect.map(held, Option.getOrNull)));
            return { ...row, value };
        }),
    );
});

const view = (model: Model['Service'], found: unknown, slice: Slice) => {
    const { picked } = model;
    return Effect.transposeOption(
        Match.value(found).pipe(
            Match.when(
                model.application,
                Effect.fnUntraced(function* (app: import('adobe:indesign').Application) {
                    const opened = yield* Effect.option(Model.current);
                    const documents = yield* Effect.forEach(app.documents.everyItem().getElements(), (document) => Effect.map(fullName(document), (name) => ({ document, name })));
                    const window = Option.liftThrowable(() => app.activeWindow.toSpecifier())();
                    const selection = yield* Effect.forEach(
                        Option.match(opened, { onNone: Array.empty, onSome: () => app.selection }),
                        Effect.fnUntraced(function* (item: (typeof app.selection)[number]) {
                            const overridden = yield* Effect.transposeOption(Option.map(Option.liftPredicate(item, model.overridable), overrides));
                            return { ...Model.referenceUnsafe(item), contents: Option.map(Schema.decodeUnknownOption(Contents)(item), Struct.get('contents')), parentStory: Option.map(Option.liftPredicate(item, model.storied), Struct.get('parentStory')), overrides: overridden };
                        }),
                    );
                    return yield* Model.picked(Application, app, () => ({
                        panels: Array.getSomes(Array.map(Array.range(0, app.panels.length - 1), (index) => Option.map(Option.filter(Option.fromUndefinedOr(app.panels.item(index)), Struct.get('visible')), Struct.get('name')))),
                        documents: Array.map(documents, ({ document, name }) => ({ ...picked(Application.fields.documents.value, document, ['fullName']), fullName: name })),
                        current: Option.map(opened, (document) => ({ document: document.toSpecifier(), window, selection, overset: overset(document), undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory } })),
                    }));
                }),
            ),
            Match.when(
                model.document,
                Effect.fnUntraced(function* (document: import('adobe:indesign').Document) {
                    const [name, settings] = yield* Effect.all([fullName(document), HostSettings.read(document.textDefaults)]);
                    return yield* Model.picked(Document, document, () => ({
                        fullName: name,
                        overset: overset(document),
                        links: Record.map(Array.groupBy(document.links.everyItem().status, String), Array.length),
                        textDefaults: { ...picked(Document.fields.textDefaults, document.textDefaults, ['settings']), settings },
                    }));
                }),
            ),
            Match.when(model.page, (page) => Effect.flatMap(Model.extent(page), (extent) => Model.picked(Page, page, () => ({ extent })))),
            Match.whenOr(
                model.spread,
                model.masterSpread,
                Effect.fnUntraced(function* (spread: import('adobe:indesign').Spread | MasterSpread) {
                    const pages = yield* Effect.forEach(spread.pages.everyItem().getElements(), (page) => Effect.map(Model.extent(page), (extent) => ({ page, extent })));
                    return yield* Model.picked(Spread, spread, () => ({ pages: Array.map(pages, ({ page, extent }) => ({ ...picked(Spread.cases.Spread.fields.pages.value, page, ['extent']), extent })) }));
                }),
            ),
            Match.when(model.layer, (layer) => Model.picked(Layer, layer)),
            Match.when(
                model.style,
                Effect.fnUntraced(function* (style: ParagraphStyle | CharacterStyle | ObjectStyle | TableStyle | CellStyle) {
                    const ancestors = model.ancestors(style.parent);
                    const refused = HostError.refused(style.toSpecifier());
                    const properties = yield* Effect.try({ try: () => style.properties, catch: refused });
                    const { basedOn } = properties;
                    const container = Option.orElse(Array.findFirst(ancestors, model.document), () => Option.some(model.app));
                    const base = Predicate.isString(basedOn) ? yield* Model.object(style.constructorName, basedOn, container) : basedOn;
                    const baseProperties = yield* Effect.try({ try: (): object => (model.domObject(base) ? base.properties : {}), catch: refused });
                    const own = yield* differing(style, properties, (name) => Reflect.get(baseProperties, name), Style.cases.ParagraphStyle.fields);
                    const settings = yield* Effect.transposeOption(Option.map(Option.liftPredicate(style, model.paragraphStyle), HostSettings.read));
                    return yield* Model.picked(Style, style, () => ({
                        group: Array.filterMap(ancestors, (ancestor) => (model.named(ancestor) && !model.document(ancestor) ? Result.succeed(ancestor.name) : Result.failVoid)),
                        basedOn: Array.unfold(basedOn, (above) => (model.style(above) ? Option.some([above, above.basedOn] as const) : Option.none())),
                        differing: own,
                        settings,
                    }));
                }),
            ),
            Match.when(model.swatch, (swatch) => Model.picked(Swatch, swatch)),
            Match.when(model.link, (link) => Model.picked(Link, link)),
            Match.when(model.story, (story) =>
                Model.picked(Story, story, () => ({
                    overset: story.length - Number.sumAll(Array.map(story.textContainers, ({ characters }) => characters.length)),
                    styles: Array.dedupe(Array.map(story.paragraphs.everyItem().appliedParagraphStyle, Struct.get('name'))),
                })),
            ),
            Match.whenOr(
                model.text,
                model.frame,
                Effect.fnUntraced(function* (source: import('adobe:indesign').Text | TextFrame) {
                    const text = model.frame(source) ? source.texts.item(0) : source;
                    const [overridden, settings] = yield* Effect.all([overrides(text), HostSettings.read(text)]);
                    const ranges = slice.of(text.textStyleRanges.everyItem().getElements());
                    const run = Text.fields.runs.value;
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
                            const [own, held] = yield* Effect.all([differing(range, properties, inherited, { ...run.fields, ...Text.fields }), HostSettings.read(range)]);
                            return yield* Effect.try({ try: () => ({ ...picked(run, range, ['designAxes', 'settings', 'differing']), designAxes: Result.try(() => range.designAxes), settings: held, differing: own }), catch: refused });
                        }),
                    );
                    return yield* Model.picked(Text, text, () => ({ overrides: overridden, runs, settings }));
                }),
            ),
            Match.when(
                model.font,
                Effect.fnUntraced(function* (face: import('adobe:indesign').Font) {
                    const installed = face.status.equals(model.FontStatus.INSTALLED);
                    const specifier = yield* Effect.orDie(Schema.decodeUnknownEffect(Specifier.Path)(face.toSpecifier()));
                    const open = Option.toArray(yield* Effect.option(Model.current));
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
                    const every = model.app.fonts.everyItem();
                    const [styles, features] = yield* Effect.try({
                        try: () =>
                            installed
                                ? [
                                      Array.dedupe(Array.filterMap(Array.zip(every.fontFamily, every.fontStyleName), ([family, named]) => (family === face.fontFamily ? Result.succeed(named) : Result.failVoid))),
                                      model
                                          .members(model.OpenTypeFeature)
                                          .filter((feature) => face.checkOpenTypeFeature(feature))
                                          .map(String),
                                  ]
                                : [[], []],
                        catch: HostError.refused(`styles and features of ${face.name}`),
                    });
                    const axes = Option.liftThrowable(() => Array.zipWith(face.designAxesName, face.designAxesValues, (name, value) => ({ name, value })))();
                    return yield* Model.picked(Font, face, () => ({
                        uses: [...Array.flatMap(searched, ({ matches }) => Array.map(matches, Struct.get('specifier'))), ...styled],
                        installed: installed ? { ...picked(Font.fields.installed, face, ['styles', 'axes', 'features']), styles, axes, features } : undefined,
                    }));
                }),
            ),
            Match.when(model.preflightProcess, (process) => Model.picked(PreflightProcess, process, () => ({ aggregatedResults: aggregated(process) }))),
            Match.option,
        ),
    );
};

// --- [TEXT]

const textTarget = Effect.fnUntraced(function* (path: string) {
    const [{ app, textContainer, text, textDefault }, specifier] = yield* Effect.all([Model, Schema.decodeUnknownEffect(Specifier.Path)(path)]);
    const found = specifier.root ? app.textDefaults : yield* Model.located(specifier);
    const target = text(found) || textDefault(found) || !textContainer(found) ? found : found.texts.item(0);
    return yield* text(target) || textDefault(target) ? Effect.succeed(target) : Unmatched.make({ subject: 'text', query: Option.some(path) });
});
const textRange = (target: import('adobe:indesign').Text | TextDefault) => Model.use(({ text }) => Effect.filterOrFail(Effect.succeed(target), text, () => Unmatched.make({ subject: 'text range', query: Option.some(target.toSpecifier()) })));
const kerningMethod = (method: 'Manual' | 'Metrics') => Model.use(({ app }) => Effect.try({ try: () => app.translateKeyString(`$ID/${method}`), catch: HostError.refused(`$ID/${method}`) }));
const pointAfterBase = (text: import('adobe:indesign').Text) =>
    Effect.try({
        try: () => {
            const last = text.insertionPoints.lastItem();
            const container = last.parent;
            const marks = Array.unfold(last.index - 1, (index) => (index >= 0 && MARK.test(String(container.characters.item(index).contents)) ? Option.some([index, index - 1] as const) : Option.none()));
            return last.index > marks.length ? Option.some(container.insertionPoints.item(last.index - marks.length)) : Option.none();
        },
        catch: HostError.refused(`base letter before ${text.toSpecifier()}`),
    }).pipe(Effect.flatMap(Effect.fromOption(() => Unmatched.make({ subject: 'base letter', query: Option.some(text.toSpecifier()) }))));

// --- [COMMANDS]

const checked = Effect.fn('checked')(function* (document: import('adobe:indesign').Document, named: Option.Option<string>) {
    const { app } = yield* Model;
    const { preflightOff, preflightWorkingProfile: working } = document.preflightOptions;
    const profiles = [...app.preflightProfiles.everyItem().getElements(), ...document.preflightProfiles.everyItem().getElements()];
    const live = preflightOff ? Option.none() : Option.some(document.activeProcess);
    const applied = Option.flatMap(live, (process) => Option.fromNullishOr(process.appliedProfile));
    const requested = Option.orElse(named, () => applied).pipe(Option.getOrElse(() => working));
    const chosen = yield* Predicate.isString(requested) ? Unmatched.single(profiles, 'preflight profile', Option.some(requested), (each) => [each.name]) : Effect.succeed(requested);
    const refused = HostError.refused(`preflight of ${document.toSpecifier()} under ${chosen.name}`);
    const finished = Option.isNone(named) ? Option.flatMap(live, flow(aggregated, Result.getSuccess)) : Option.none();
    const waited = Effect.acquireUseRelease(
        Effect.try({ try: () => app.preflightProcesses.add(document, chosen), catch: refused }),
        (process) =>
            Effect.andThen(
                Effect.try({ try: () => process.waitForProcess(), catch: refused }),
                Effect.suspend(() => Effect.fromResult(Result.mapError(aggregated(process), refused))),
            ),
        (process) => Effect.sync(() => process.remove()),
    );
    return yield* Effect.all({
        targetObject: Model.reference(document),
        appliedProfile: Model.reference(chosen),
        aggregatedResults: Option.match(finished, { onSome: Effect.succeed, onNone: () => waited }),
    });
});
const actions = Model.use(({ app }) =>
    Model.read(Menu.Result, 'menu actions', () => {
        const every = app.menuActions.everyItem();
        return Array.zipWith(Array.zip(every.id, every.name), every.area, ([id, name], area) => ({ id, name, area }));
    }),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const requests = InDesign.Contract.of({
    inspect: Effect.fn('InDesign.Contract.inspect')(function* ({ target, fields, slice }: Inspect) {
        const [found, model, path] = yield* Effect.all([Model.resolve(target), Model, target.path]);
        if (!model.domObject(found)) {
            const items = yield* Effect.forEach(slice.of(found), (item) => Effect.zipWith(Model.reference(item), readings(item, Option.getOrElse(fields, Array.empty)), (reference, read) => ({ ...reference, fields: read })));
            return yield* Model.read(Inspect.Items, path, () => ({ _tag: 'Items', specifier: path, total: found.length, items }));
        }
        const [{ fields: own }, shown] = yield* Effect.all([Cache.get(model.declarations, found.constructorName), view(model, found, slice)]);
        const entries = yield* readings(
            found,
            Option.getOrElse(fields, () => (Option.isSome(shown) ? Record.keys(Record.filter(own, model.collectionField)) : Array.map(accessors(found), Struct.get('name')))),
        );
        const [collections, scalars] = Array.partition(entries, (entry) =>
            pipe(
                Result.getSuccess(entry.value),
                Option.filter(model.collection),
                Option.map((items) => ({ name: entry.name, type: items.constructor.name, length: items.length, specifier: items.everyItem().toSpecifier() })),
                Result.fromOption(() => entry),
            ),
        );
        return Inspect.Object.make({ ...(yield* Model.reference(found)), view: shown, fields: yield* Model.read(Inspect.Readings, found.toSpecifier(), () => scalars), collections });
    }),
    change: (payload: Change) => Effect.flatMap(Model.located(payload.target), (found) => written(payload.label, found, Model.write(found, payload.values))),
    call: Effect.fn('InDesign.Contract.call')(function* (payload: Call) {
        const [model, found] = yield* Effect.all([Model, Model.located(payload.target)]);
        const { fields } = yield* Cache.get(model.declarations, found.constructorName);
        const [receiver, kind] = yield* Option.match(payload.collection, {
            onNone: () => Effect.succeed([found, found.constructorName] as const),
            onSome: (field) =>
                Effect.map(
                    Effect.fromOption(Option.liftPredicate(Reflect.get(found, field), model.collection), () => Unmatched.make({ subject: `collection of ${found.toSpecifier()}`, query: Option.some(field), candidates: Record.keys(Record.filter(fields, model.collectionField)) })),
                    (items) => [items, items.constructor.name] as const,
                ),
        });
        const { methods } = yield* Cache.get(model.declarations, kind);
        const [method, { parameters, returns }] = yield* Unmatched.single(Record.toEntries(methods), `method of ${kind}`, Option.some(payload.method), ([name]) => [name]);
        const required = parameters.filter(({ optional }) => !optional).length;
        const positional = payload.arguments;
        yield* positional.length < required || positional.length > parameters.length
            ? new Cause.IllegalArgumentError(`${kind} ${method} takes ${required === parameters.length ? required : `${required} to ${parameters.length}`} arguments (${Array.map(parameters, ({ name, optional }) => (optional ? `${name}?` : name)).join(', ')}), given ${positional.length}`)
            : Effect.void;
        const decode = Model.decoder(found, yield* Effect.cached(Model.fonts));
        const positioned = yield* Effect.forEach(Array.zip(positional, parameters), ([value, parameter]) => decode(Option.some(parameter), Option.map(returns, Struct.get('type')))(value));
        const before = yield* Model.reference(found);
        return yield* commit(
            payload.label,
            Effect.gen(function* () {
                const returned = yield* Effect.try({ try: (): unknown => Reflect.apply(Reflect.get(receiver, method), receiver, positioned), catch: HostError.refused(`${kind} ${method}`) });
                const made = Effect.fromOption(Array.head(Array.ensure<unknown>(returned).filter(model.domObject)), () => new Cause.IllegalArgumentError(`${kind} ${method} returned no object for properties`));
                yield* Effect.forEach(Option.toArray(payload.properties), (supplied) => Effect.flatten(Effect.zipWith(made, Effect.succeed(supplied), Model.write)), { discard: true });
                return yield* Model.read(Call.Result, payload.label, () => ({ ...before, valid: found.isValid, result: Option.fromUndefinedOr(returned) }));
            }),
        );
    }),
    find_change: (payload: FindChange) =>
        Option.match(payload.scope, { onSome: Model.resolve, onNone: () => Model.current }).pipe(
            Effect.flatMap((found: unknown) => (Option.isSome(payload.change) ? commit(payload.label, search(payload, found)) : search(payload, found))),
            Effect.flatMap(({ count, matches }) => Model.read(FindChange.Result, payload.label, () => ({ count, matches: payload.slice.of(matches) }))),
        ),
    font: Effect.fn('InDesign.Contract.font')(function* ({ name, text, file }: FontFile) {
        const [face, model, { fs }, path] = yield* Effect.all([Effect.flatMap(Model.fonts, (lookup) => lookup(name)), Model, Uxp, Path.Path]);
        const subset = Effect.as(Effect.try({ try: () => face.createSubsetFont(text, file), catch: HostError.refused(`subset of ${face.name}`) }), Option.some({ file, postscriptName: Option.none<string>() }));
        const source = face.status.equals(model.FontStatus.INSTALLED)
            ? yield* Effect.flatMap(path.toFileUrl(face.location), (url) => Effect.try({ try: () => fs.lstatSync(url.href), catch: HostError.refused(face.location) })).pipe(
                  Effect.as(Option.some({ file: face.location, postscriptName: Option.some(face.postscriptName) })),
                  Effect.catchIf(
                      (error) => !Schema.is(HostError)(error) || Schema.is(Schema.Struct({ code: Schema.Literal(-2) }))(error.cause),
                      () => subset,
                  ),
              )
            : Option.none();
        const font = yield* Effect.flatMap(view(model, face, Slice.make({ offset: 0, limit: Option.none() })), flow(Option.filter(Schema.is(Font)), Effect.fromOption, Effect.orDie));
        return { font, source };
    }),
    menu: ({ area }: Menu) =>
        Effect.flatMap(
            actions,
            Unmatched.matching('menu area', area, (row) => [row.area]),
        ),
    invoke: Effect.fn('InDesign.Contract.invoke')(function* (payload: Invoke) {
        const [{ app }, listed] = yield* Effect.all([Model, actions]);
        const chosen = yield* Unmatched.single(listed, 'menu action', Option.some(String(payload.action)), ({ id, name, area }) => [`${id} ${name} (${area})`, String(id), name]);
        yield* commit(payload.label, Effect.try({ try: () => app.menuActions.itemByID(chosen.id).invoke(), catch: HostError.refused(chosen.name) }));
        return chosen;
    }),
    preflight: Effect.fn('InDesign.Contract.preflight')(function* ({ document, profile }: Preflight) {
        const model = yield* Model;
        const open = model.app.documents.everyItem().getElements();
        const specifiers = Array.map(open, (each) => each.toSpecifier());
        const targets = yield* Option.match(document, {
            onNone: () => Effect.succeed(open.filter((each) => !each.preflightOptions.preflightOff)),
            onSome: (specifier) =>
                Effect.map(
                    Effect.filterOrElse(Model.resolve(specifier), model.document, () => Effect.flatMap(specifier.path, (path) => Unmatched.make({ subject: 'open document', query: Option.some(path), candidates: specifiers }))),
                    Array.of,
                ),
        });
        return yield* Effect.forEach(targets, (target) => checked(target, profile));
    }),
    adjust: Effect.fn('InDesign.Contract.adjust')(function* (payload: Adjust) {
        const { kerning = 0, tracking = 0, baselineShift = 0, marks = { x: 0, y: 0 } } = payload.deltas;
        const [{ text }, target, manual, encoded] = yield* Effect.all([Model, textTarget(payload.target), kerningMethod('Manual'), Effect.orDie(Schema.encodeEffect(Deltas)({ kerning, tracking, baselineShift, marks }))]);
        const points: readonly InsertionPoint[] = kerning === 0 ? [] : [yield* Effect.flatMap(textRange(target), pointAfterBase)];
        const sums = yield* Effect.try({
            try: () =>
                Array.map(
                    text(target) ? target.textStyleRanges.everyItem().getElements() : [target],
                    (run) => [run, Record.filterMap({ tracking: encoded.tracking, baselineShift: encoded.baselineShift * run.pointSize, ...encoded.marks }, (amount, member) => (amount === 0 ? Result.failVoid : Result.succeed(run[member] + amount)))] as const,
                ),
            catch: HostError.refused(`style runs of ${target.toSpecifier()}`),
        });
        const kerned = yield* Effect.try({ try: () => Array.map(points, (point) => [point, { kerningValue: (point.kerningMethod === manual ? point.kerningValue : 0) + encoded.kerning }] as const), catch: HostError.refused(`kerning of ${target.toSpecifier()}`) });
        return yield* written(payload.label, target, Effect.forEach([...sums, ...kerned], ([run, values]) => Model.write(run, values)).pipe(Effect.map(flow(Array.flatten, Array.dedupe))));
    }),
    kerns: Effect.fn('InDesign.Contract.kerns')(function* (payload: Kerns) {
        const [text, manual, metrics, rows] = yield* Effect.all([Effect.flatMap(textTarget(payload.target), textRange), kerningMethod('Manual'), kerningMethod('Metrics'), Effect.orDie(Schema.encodeEffect(Kerned)(payload.kerns))]);
        const refused = HostError.refused(`insertion points of ${text.toSpecifier()}`);
        const count = yield* Effect.try({ try: () => text.insertionPoints.length, catch: refused });
        const outOfRange = [...Option.toArray(payload.restore), ...rows].filter(({ index, length }) => index + length >= count);
        yield* Array.isReadonlyArrayEmpty(outOfRange) ? Effect.void : Unmatched.make({ subject: `run in characters 0 to ${count - 2}`, query: Option.some(outOfRange.map(({ index, length }) => `${index} to ${index + length - 1}`).join(', ')) });
        const characters = ({ index, length }: { readonly index: number; readonly length: number }) => text.characters.itemByRange(index, index + length - 1);
        const restored = Effect.forEach(
            Option.toArray(payload.restore),
            (run) =>
                Effect.flatMap(
                    Effect.try({ try: () => characters(run).getElements(), catch: refused }),
                    Effect.forEach((each) => Model.write(each, { kerningMethod: metrics }), { discard: true }),
                ),
            { discard: true },
        );
        const kerned = Effect.try({
            try: () => {
                const methods = text.insertionPoints.everyItem().kerningMethod;
                const kept = rows.filter(({ index }) => methods[index] !== manual || text.insertionPoints.item(index).kerningValue === 0);
                const points = kept.filter(({ kerning }) => kerning !== 0).map(({ index, kerning }) => [text.insertionPoints.item(index), { kerningValue: kerning }] as const);
                const shifted = kept
                    .filter(({ baselineShift }) => baselineShift !== 0)
                    .flatMap((row) =>
                        characters(row)
                            .getElements()
                            .map((character) => [character, { baselineShift: row.baselineShift * character.pointSize }] as const),
                    );
                return { kept, members: [...points, ...shifted] };
            },
            catch: refused,
        }).pipe(
            Effect.flatMap(({ kept, members }) =>
                Effect.as(
                    Effect.forEach(members, ([target, values]) => Model.write(target, values), { discard: true }),
                    kept.length,
                ),
            ),
        );
        return yield* written(payload.label, text, Effect.andThen(restored, kerned));
    }),
    runs: Effect.fn('InDesign.Contract.runs')(function* (payload: Runs) {
        const [{ text, textContainer }, target] = yield* Effect.all([Model, textTarget(payload.target)]);
        if (!text(target)) {
            return yield* Effect.try({ try: () => ({ ...Model.referenceUnsafe(target), start: 0, length: 0, runs: [{ text: '', font: target.appliedFont.name, axes: target.designAxes }] }), catch: HostError.refused(`applied font of ${target.toSpecifier()}`) });
        }
        const container = target.parent;
        const whole = yield* textContainer(container) ? Effect.succeed(container.texts.item(0)) : Unmatched.make({ subject: 'text container', query: Option.some(payload.target) });
        const ranges = yield* Effect.try({
            try: () => Array.map(whole.textStyleRanges.everyItem().getElements(), (run) => ({ text: Predicate.isString(run.contents) ? run.contents : '\n'.repeat(run.length), font: run.appliedFont.name, axes: run.designAxes })),
            catch: HostError.refused(`style ranges of ${whole.toSpecifier()}`),
        });
        return { ...(yield* Model.reference(container)), start: target.index, length: target.length, runs: ranges };
    }),
    typeset: Effect.fn('InDesign.Contract.typeset')(function* (payload: Typeset) {
        const target = yield* textTarget(payload.target);
        yield* Effect.transposeOption(Option.map(payload.settings, (settings) => commit(payload.label, HostSettings.write(target, settings))));
        return yield* HostSettings.read(target);
    }),
    execute_script: Effect.fn('InDesign.Contract.execute_script')(function* (payload: ExecuteScript) {
        const { exported } = yield* Model;
        const given = { ...Object.fromEntries(exported), resolve: flow(Schema.decodeUnknownEffect(Specifier.Path), Effect.flatMap(Model.resolve), Effect.runSyncWith(yield* Effect.context<Model>())) };
        const result = yield* commit(payload.label, yield* Uxp.evaluate(given, payload.code));
        return yield* Model.read(ExecuteScript.Result, payload.label, () => ({ result }));
    }),
    open: Effect.fn('InDesign.Contract.open')(function* (payload: Open) {
        const [{ app }, path] = yield* Effect.all([Model, Path.Path]);
        const native = yield* path.fromFileUrl(payload.file);
        return yield* Model.reference(yield* Effect.try({ try: () => app.open(native, false), catch: HostError.refused(payload.label) }));
    }),
    render: Effect.fn('InDesign.Contract.render')(function* ({ target, longestSide, options, file, label }: Render) {
        const [model, specifier, path] = yield* Effect.all([Model, Schema.decodeUnknownEffect(Specifier.Path)(target), Path.Path]);
        const { app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum } = model;
        const found = yield* Effect.filterOrFail(
            Model.resolve(specifier),
            (value): value is import('adobe:indesign').Page | import('adobe:indesign').Spread | PageItem => (model.page(value) && model.spread(value.parent)) || model.spread(value) || model.item(value),
            () => Unmatched.make({ subject: 'document page, spread, or page item', query: Option.some(target) }),
        );
        const { source, placed, range } = Match.value(found).pipe(
            Match.when(model.page, (each) => ({ source: each.parent.parent, placed: [each], range: { pageString: `+${each.documentOffset + 1}`, exportingSpread: false } })),
            Match.when(model.spread, (each) => ({ source: each.parent, placed: each.pages.everyItem().getElements(), range: { pageString: `+${each.pages.firstItem().documentOffset + 1}`, exportingSpread: true } })),
            Match.orElse((each) => ({ source: each, placed: [each], range: {} })),
        );
        const { top, left, bottom, right } = yield* Model.extent(...placed);
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
        yield* Model.write(preferences, given);
        const native = yield* path.fromFileUrl(file);
        yield* Effect.try({ try: () => source.exportFile(ExportFormat.PNG_FORMAT, native), catch: HostError.refused(label) });
    }, Effect.scoped),
    object_model: Effect.fn('InDesign.Contract.object_model')(function* (payload: ObjectModel) {
        const model = yield* Model;
        const { classes, enumerations, enumeration, declarations, parents, members } = model;
        const { Class, Enumeration } = ObjectModel.Result.cases;
        const declaration = Effect.fnUntraced(function* (name: string) {
            const declared = yield* Effect.catchTag(Effect.asSome(Cache.get(declarations, name)), 'Unmatched', () => Effect.succeedNone);
            const { fields, methods, statics } = Option.getOrElse(declared, () => ({ fields: {}, methods: {}, statics: {} }));
            const type: unknown = Reflect.get(model, name);
            const [live] = Predicate.isFunction(type) ? Uxp.members(type.prototype) : [{ fields: [], methods: [] }];
            return yield* Option.match(enumeration(name), {
                onSome: (found) => Effect.succeed(Enumeration.make({ name, members: Array.map(members(found), (each) => ({ name: String(each), description: Record.get(statics, String(each)) })) })),
                onNone: () =>
                    Effect.map(
                        Effect.all({
                            parents: Option.isSome(declared) ? parents(name) : Effect.succeed([]),
                            fields: Effect.forEach(live.fields, (accessor) => Option.match(Record.get(fields, accessor.name), { onNone: () => Dictionary.undeclared(ObjectModel.Field, accessor), onSome: Effect.succeed })),
                            methods: Effect.forEach(live.methods, (method) => Option.match(Record.get(methods, method), { onNone: () => Dictionary.undeclared(ObjectModel.Method, { name: method }), onSome: Effect.succeed })),
                        }),
                        (described) => Class.make({ name, ...described }),
                    ),
            });
        });
        return yield* payload.describe(classes, enumerations, declaration);
    }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { requests };
