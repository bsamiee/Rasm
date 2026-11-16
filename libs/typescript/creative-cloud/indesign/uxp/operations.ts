import { Declaration, HostError, Specifier } from '@rasm/object-model';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/plugin';
import type { Application, CellStyle, CharacterStyle, Document, Font, InDesignDOMObject, MasterSpread, ObjectStyle, Page, PageItem, ParagraphStyle, PreflightProcess, Spread, TableStyle, Text, TextFrame, TextPath, TextStyleRange } from 'adobe:indesign';
import { Array, Cache, Cause, Effect, Equivalence, Filter, Formatter, Function, flow, identity, Match, Number, Option, Path, Predicate, pipe, Record, Result, Schema, SchemaGetter, String, Struct } from 'effect';
import { AGGREGATE_FIELD, unavailableReason } from '#/uxp/dictionary';
import { Model } from '#/uxp/model';
import { InDesignSettings } from '#/uxp/settings';
import type { Value } from '#/value';
import { Called, Inspection, MenuActions, Search, type Slice } from '#/views';

// --- [TYPES] ---------------------------------------------------------------------------

type Values = Record.ReadonlyRecord<string, typeof Value.Type>;

// --- [MODELS] --------------------------------------------------------------------------

const Contents = Schema.Struct({ contents: Schema.Json });
const Flat = Schema.Tuple([Schema.Int, Schema.String, Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.String, Schema.String]))]);
const MissingFile = Schema.Literal(-2).annotate({ description: 'Value UXP fs throws for a path it finds no entry at' });
const Executed = Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SEARCH]

const preserved = Effect.fnUntraced(function* (member: ReturnType<typeof Search.members<typeof Search.Type>>['preferences'][number]) {
    const model = yield* Model;
    const { app, NothingEnum } = model;
    const target = app[member];
    const { fields } = yield* Cache.get(model.declarations, target.constructorName);
    const current = Option.liftThrowable((name: string): unknown => Reflect.get(target, name));
    const saved = Record.filterMap(fields, (row, name) => (row.settable && !model.collectionField(row) && Option.isNone(row.unavailable) && name !== AGGREGATE_FIELD ? Result.fromOption(Option.filter(current(name), Predicate.not(model.nothing)), Function.constVoid) : Result.failVoid));
    const reset = Effect.try({ try: () => Object.assign(app, { [member]: NothingEnum.NOTHING })[member], catch: HostError.refused(`reset of ${member}`) });
    return yield* Effect.acquireRelease(Effect.as(reset, saved), () => Effect.flatMap(Effect.orDie(reset), (cleared) => Uxp.restore(cleared, saved)));
});
const search = Effect.fnUntraced(function* ({ mode, find, change, options }: { readonly mode: typeof Search.Type; readonly find: Option.Option<Values>; readonly change: Option.Option<Values>; readonly options: Option.Option<Values> }, located: unknown) {
    const members = Search.members(mode);
    const model = yield* Model;
    const { app, enumerator } = model;
    yield* Effect.forEach(members.preferences, preserved, { discard: true });
    const counted = Match.type<number | readonly unknown[]>().pipe(Match.when(Predicate.isNumber, identity<number>), Match.orElse(Array.length));
    const spelled = (contents: unknown) =>
        Array.map(Array.ensure(contents), (each) =>
            Match.value(each).pipe(
                Match.when(enumerator, (special) => `<${special.toString()}>`),
                Match.when(Predicate.isString, identity),
                Match.orElse(Formatter.format),
            ),
        ).join('');
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
        Effect.forEach(([target, values]) => Model.write(target, values), { discard: true }),
    );
    const describe = (hit: Text | PageItem) => {
        const placed = model.text(hit) ? Option.filter(Array.head(hit.parentTextFrames), model.item) : Option.liftPredicate(hit, model.item);
        return {
            specifier: hit.toSpecifier(),
            text: model.text(hit) ? Option.some(spelled(hit.contents)) : Option.none(),
            page: Option.map(Option.flatMapNullishOr(placed, Struct.get('parentPage')), Struct.get('name')),
        };
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

const nested: (rows: readonly (typeof Flat.Type)[]) => typeof Inspection.Issues.Type = Array.match({
    onEmpty: Array.empty,
    onNonEmpty: ([[depth, label, page, , details], ...rest]) => {
        const [under, after] = Array.span(rest, ([below]) => below > depth);
        return [{ label, ...Record.getSomes({ page: Option.liftPredicate(page, String.isNonEmpty) }), details: Record.fromEntries(details), rows: nested(under) }, ...nested(after)];
    },
});
const Aggregated = Schema.Tuple([Schema.String, Schema.String, Schema.Array(Flat)]).pipe(Schema.decodeTo(Inspection.Issues, { decode: SchemaGetter.transform((aggregate) => nested(aggregate[2])), encode: SchemaGetter.forbiddenEncoding }));
const aggregated = (process: PreflightProcess) => Result.try(() => process.aggregatedResults).pipe(Result.flatMap(Schema.decodeUnknownResult(Aggregated)));
const overset = ({ stories }: Document): readonly string[] => Array.filterMap(Array.zip(stories.everyItem().overflows, stories.everyItem().id), ([overflows, id]) => (overflows ? Result.succeed(stories.itemByID(id).toSpecifier()) : Result.failVoid));
const savedPath = (document: Document) => Effect.flatMap(Effect.try({ try: () => document.saved, catch: HostError.refused(`saved of ${document.toSpecifier()}`) }), (saved) => (saved ? Model.file(document, 'fullName') : Effect.succeedNone));
const overrides = (text: Pick<Text, 'textHasOverrides' | 'toSpecifier'>) =>
    Model.use(({ StyleType }) => Effect.try({ try: () => ({ paragraph: text.textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE), character: text.textHasOverrides(StyleType.CHARACTER_STYLE_TYPE) }), catch: HostError.refused(`overrides of ${text.toSpecifier()}`) }));
const differing = Effect.fnUntraced(function* (object: InDesignDOMObject<unknown>, properties: object, baseValue: (name: string) => unknown, except: Schema.Struct.Fields) {
    const [{ declarations, collectionField }, encoded] = yield* Effect.all([Model, Model.encoder]);
    const { fields } = yield* Cache.get(declarations, object.constructorName);
    return Record.filterMap(fields, (row, name) => {
        if (!row.settable || collectionField(row) || Object.hasOwn(except, name)) {
            return Result.failVoid;
        }
        const value: unknown = Reflect.get(properties, name);
        return encoded(value) === encoded(baseValue(name)) ? Result.failVoid : Result.succeed(value);
    });
});
const readings = Effect.fn('readings')(function* (found: InDesignDOMObject<unknown>, wanted: Option.Option<readonly string[]>) {
    const { declarations } = yield* Model;
    const { fields } = yield* Cache.get(declarations, found.constructorName);
    const readable = pipe(Array.flatMap(Uxp.members(Object.getPrototypeOf(found)), Struct.get('fields')), Array.dedupeWith(Equivalence.mapInput(Equivalence.String, Struct.get('name')))).filter(({ name }) => name !== AGGREGATE_FIELD);
    const [named, missing] = Option.match(wanted, { onNone: () => [readable, []] as const, onSome: Array.partition(Filter.fromPredicateOption((each: string) => Array.findFirst(readable, ({ name }) => Unmatched.named(name, each)))) });
    yield* Array.isReadonlyArrayEmpty(missing) ? Effect.void : Unmatched.make({ subject: `field of ${found.toSpecifier()}`, query: Option.some(missing.join(', ')), candidates: Array.map(readable, Struct.get('name')) });
    return yield* Effect.forEach(
        named,
        Effect.fnUntraced(function* (accessor) {
            const subject = `${found.toSpecifier()} ${accessor.name}`;
            const row = yield* Declaration.enrich(Declaration.Field, fields, accessor);
            const refusal = Effect.transposeOption(Option.map(unavailableReason(found.constructorName, accessor.name, Option.some(found), false), HostError.refused(subject)));
            const held = Option.contains(row.type, 'File') ? Model.file(found, accessor.name) : Effect.map(Effect.try({ try: (): unknown => Reflect.get(found, accessor.name), catch: HostError.refused(subject) }), Option.fromNullishOr);
            const value = yield* Effect.result(Effect.andThen(refusal, Effect.map(held, Option.getOrNull)));
            return { ...row, value };
        }),
    );
});
const declaration = Effect.fnUntraced(function* (name: string) {
    const model = yield* Model;
    const { enumeration, declarations, parents, members } = model;
    const declared = yield* Effect.catchTag(Effect.asSome(Cache.get(declarations, name)), 'Unmatched', () => Effect.succeedNone);
    const { fields, methods, statics } = Option.getOrElse(declared, () => ({ fields: {}, methods: {}, statics: {} }));
    const type: unknown = Reflect.get(model, name);
    const [live = { fields: [], methods: [] }] = Uxp.members(Predicate.isFunction(type) ? type.prototype : undefined);
    return yield* Option.match(enumeration(name), {
        onSome: (enumerated) => Effect.succeed(Declaration.cases.Enumeration.make({ name, members: Array.map(members(enumerated), (member) => ({ name: member.toString(), description: Record.get(statics, member.toString()) })) })),
        onNone: () =>
            Effect.map(
                Effect.all({ parents: Option.isSome(declared) ? parents(name) : Effect.succeed([]), fields: Effect.forEach(live.fields, (accessor) => Declaration.enrich(Declaration.Field, fields, accessor)), methods: Effect.forEach(live.methods, (method) => Declaration.enrich(Declaration.Method, methods, method)) }),
                (described) => Declaration.cases.Class.make({ name, ...described }),
            ),
    });
});
const fontView = Effect.fnUntraced(function* (face: Font) {
    const model = yield* Model;
    const specifier = yield* Effect.orDie(Schema.decodeEffect(Specifier.Path)(face.toSpecifier()));
    const open = Option.toArray(yield* Effect.option(Model.current));
    const searched = yield* Effect.forEach(open, (document) =>
        search({ mode: 'Text', find: Option.some({ appliedFont: { specifier } }), options: Option.some({ includeFootnotes: true, includeHiddenLayers: true, includeLockedLayersForFind: true, includeLockedStoriesForFind: true, includeMasterPages: true }), change: Option.none() }, document),
    );
    const styled = yield* Effect.try({
        try: () =>
            Array.flatMap(open, (document) => [...document.allParagraphStyles, ...document.allCharacterStyles])
                .filter(({ appliedFont, fontStyle }) => Predicate.isString(fontStyle) && Option.exists(Option.fromNullishOr(Predicate.isString(appliedFont) ? appliedFont : appliedFont?.fontFamily), (family) => Unmatched.named(`${family}\t${fontStyle}`, face.name)))
                .map((each) => each.toSpecifier()),
        catch: HostError.refused(`uses of ${face.name}`),
    });
    const installed = yield* Effect.when(
        Effect.try({
            try: () => {
                const every = model.app.fonts.everyItem();
                return {
                    ...model.picked(Inspection.View.cases.Font.fields.installed, face, ['styles', 'axes', 'features']),
                    styles: Array.dedupe(Array.filterMap(Array.zip(every.fontFamily, every.fontStyleName), ([family, named]) => (family === face.fontFamily ? Result.succeed(named) : Result.failVoid))),
                    axes: Option.map(
                        Option.liftThrowable(() => Array.zip(face.designAxesName, face.designAxesValues))(),
                        Array.map(([name, value]) => ({ name, value })),
                    ),
                    features: model
                        .members(model.OpenTypeFeature)
                        .filter((feature) => face.checkOpenTypeFeature(feature))
                        .map((feature) => feature.toString()),
                };
            },
            catch: HostError.refused(`styles and features of ${face.name}`),
        }),
        Effect.succeed(face.status.equals(model.FontStatus.INSTALLED)),
    );
    const uses = [...Array.flatMap(searched, ({ matches }) => Array.map(matches, Struct.get('specifier'))), ...styled];
    return yield* Model.picked(Inspection.View.cases.Font, face, () => ({ uses, installed }));
});
const view = (model: Model['Service'], found: unknown, slice: Slice) => {
    const { picked } = model;
    return Effect.transposeOption(
        Match.value(found).pipe(
            Match.when(
                model.application,
                Effect.fnUntraced(function* (app: Application) {
                    const opened = yield* Effect.option(Model.current);
                    const documents = yield* Effect.forEach(app.documents.everyItem().getElements(), (document) => savedPath(document).pipe(Effect.bindTo('fullName'), Effect.map(Struct.assign({ document }))));
                    const window = Option.liftThrowable(() => app.activeWindow.toSpecifier())();
                    const selection = yield* Effect.forEach(
                        Option.match(opened, { onNone: Array.empty, onSome: () => app.selection }),
                        Effect.fnUntraced(function* (item: (typeof app.selection)[number]) {
                            const overridden = yield* Effect.transposeOption(Option.map(Option.liftPredicate(item, model.overridable), overrides));
                            return { ...Model.referenceUnsafe(item), contents: Option.map(Schema.decodeUnknownOption(Contents)(item), Struct.get('contents')), parentStory: Option.map(Option.liftPredicate(item, model.storied), Struct.get('parentStory')), overrides: overridden };
                        }),
                    );
                    const panels = Array.getSomes(globalThis.Array.from({ length: app.panels.length }, (_, index) => Option.map(Option.filter(Option.fromUndefinedOr(app.panels.item(index)), Struct.get('visible')), Struct.get('name'))));
                    return yield* Model.picked(
                        Inspection.View.cases.Application,
                        app,
                        flow(
                            () => ({ panels, documents, current: opened }),
                            Struct.evolve({
                                documents: Array.map(({ document, fullName }) => ({ ...picked(Inspection.View.cases.Application.fields.documents.value, document, ['fullName']), fullName })),
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
                    return yield* Model.picked(Inspection.View.cases.Document, document, () => ({
                        fullName,
                        overset: overset(document),
                        links: Record.map(Array.groupBy(document.links.everyItem().status, globalThis.String), Array.length),
                        textDefaults: { ...picked(Inspection.View.cases.Document.fields.textDefaults, document.textDefaults, ['settings']), settings },
                    }));
                }),
            ),
            Match.when(model.page, (page) => Effect.flatMap(Model.extent(page), (extent) => Model.picked(Inspection.View.cases.Page, page, Function.constant({ extent })))),
            Match.whenOr(
                model.spread,
                model.masterSpread,
                Effect.fnUntraced(function* (spread: Spread | MasterSpread) {
                    const pages = yield* Effect.forEach(spread.pages.everyItem().getElements(), (page) => Model.extent(page).pipe(Effect.bindTo('extent'), Effect.map(Struct.assign({ page }))));
                    return yield* Model.picked(Inspection.Spread, spread, flow(Function.constant({ pages }), Struct.evolve({ pages: Array.map(({ page, extent }) => ({ ...picked(Inspection.Spread.cases.Spread.fields.pages.value, page, ['extent']), extent })) })));
                }),
            ),
            Match.when(model.layer, (layer) => Model.picked(Inspection.View.cases.Layer, layer)),
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
                    const own = yield* differing(style, properties, (name): unknown => Reflect.get(baseProperties, name), Inspection.Style.cases.ParagraphStyle.fields);
                    const settings = yield* Effect.transposeOption(Option.map(Option.liftPredicate(style, model.paragraphStyle), InDesignSettings.read));
                    return yield* Model.picked(
                        Inspection.Style,
                        style,
                        flow(() => ({ group: ancestors, basedOn: model.ancestors(basedOn, 'basedOn'), differing: own, settings }), Struct.evolve({ group: Array.filterMap((ancestor) => (model.named(ancestor) && !model.document(ancestor) ? Result.succeed(ancestor.name) : Result.failVoid)) })),
                    );
                }),
            ),
            Match.when(model.swatch, (swatch) => Model.picked(Inspection.Swatch, swatch)),
            Match.when(model.link, (linked) => Model.picked(Inspection.View.cases.Link, linked)),
            Match.when(model.story, (story) =>
                Model.picked(
                    Inspection.View.cases.Story,
                    story,
                    flow(
                        () => ({ overset: story.textContainers, styles: story.paragraphs.everyItem().appliedParagraphStyle }),
                        Struct.evolve({ overset: (containers) => story.length - Number.sumAll(Array.map(containers, flow(Struct.get('characters'), Struct.get('length')))), styles: flow(Array.map(Struct.get('name')), Array.dedupe) }),
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
                    const run = Inspection.View.cases.Text.fields.runs.value;
                    const styles = yield* Effect.try({
                        try: () =>
                            Record.fromIterableWith(
                                Array.flatMap(ranges, (range) => [range.appliedParagraphStyle, range.appliedCharacterStyle]),
                                (style) => [style.toSpecifier(), style.properties] as const,
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
                            const [own, held] = yield* Effect.all([differing(range, properties, inherited, { ...run.fields, ...Inspection.View.cases.Text.fields }), InDesignSettings.read(range)]);
                            return yield* Effect.try({ try: () => ({ ...picked(run, range, ['designAxes', 'settings', 'differing']), designAxes: Result.try(flow(Function.constant(range), Struct.get('designAxes'))), settings: held, differing: own }), catch: refused });
                        }),
                    );
                    return yield* Model.picked(Inspection.View.cases.Text, text, () => ({ overrides: overridden, runs, settings }));
                }),
            ),
            Match.when(model.font, fontView),
            Match.when(model.preflightProcess, (process) => Model.picked(Inspection.View.cases.PreflightProcess, process, () => ({ aggregatedResults: aggregated(process) }))),
            Match.option,
        ),
    );
};

// --- [COMMANDS]

const checked = Effect.fn('checked')(function* (document: Document, named: Option.Option<string>) {
    const { app } = yield* Model;
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
    return yield* Effect.all({ targetObject: Model.reference(document), appliedProfile: Model.reference(chosen), aggregatedResults: Option.match(finished, { onSome: Effect.succeed, onNone: () => waited }) });
});
const actions = Model.use(({ app }) =>
    Model.read(
        MenuActions,
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

// --- [REQUESTS]

const inspect = Effect.fn('inspect')(function* ({ target, fields, slice }: { readonly target: Specifier; readonly fields: Option.Option<readonly string[]>; readonly slice: Slice }) {
    const [found, model] = yield* Effect.all([Model.resolve(target), Model]);
    if (!model.domObject(found)) {
        const items = yield* Effect.forEach(slice.of(found), (item) => Effect.zipWith(Model.reference(item), readings(item, Option.orElseSome(fields, Array.empty)).pipe(Effect.bindTo('fields')), Struct.assign));
        return yield* Model.read(Inspection.Items, target.path, () => ({ _tag: 'Items', specifier: target.path, total: found.length, items }));
    }
    const [declared, shown] = yield* Effect.all([Cache.get(model.declarations, found.constructorName), view(model, found, slice)]);
    const entries = yield* readings(
        found,
        Option.orElse(fields, () => Option.as(shown, Record.keys(Record.filter(declared.fields, model.collectionField)))),
    );
    const [collections, scalars] = Array.partition(
        entries,
        Match.type<(typeof entries)[number]>().pipe(
            Match.when({ value: { _tag: 'Success', success: model.collection } }, ({ name, value }) => ({ name, ...Model.collected(value.success) })),
            Match.result,
        ),
    );
    return Inspection.Object.make({ ...(yield* Model.reference(found)), view: shown, fields: yield* Model.read(Inspection.Readings, found.toSpecifier(), () => scalars), collections });
});
const change = ({ label, target, values }: { readonly label: string; readonly target: Specifier; readonly values: Values }) => Effect.flatMap(Model.located(target), (located) => Model.written(label, located, Model.write(located, values)));
const call = Effect.fn('call')(function* (payload: { readonly label: string; readonly target: Specifier; readonly collection: Option.Option<string>; readonly method: string; readonly arguments: readonly (typeof Value.Type)[]; readonly properties: Option.Option<Values> }) {
    const [model, found] = yield* Effect.all([Model, Model.located(payload.target)]);
    const before = yield* Model.reference(found);
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
    const arity = Array.dedupe([required, parameters.length]).join(' to ');
    const signature = Array.map(parameters, ({ name, optional }) => (optional ? `${name}?` : name)).join(', ');
    yield* positional.length < required || positional.length > parameters.length ? new Cause.IllegalArgumentError(`${kind} ${method} takes ${arity} arguments (${signature}), given ${positional.length}`) : Effect.void;
    const decode = Model.decoder(found, yield* Effect.cached(Model.fonts));
    const positioned = yield* Effect.forEach(Array.zip(positional, parameters), ([value, parameter]) => decode(Option.some(parameter), Option.map(returns, Struct.get('type')))(value));
    const properties = Option.map(payload.properties, (supplied) => flow(Array.ensure<unknown>, Array.findFirst(model.domObject), Effect.fromOption(Function.constant(new Cause.IllegalArgumentError(`${kind} ${method} returned no object for properties`))), Effect.flatMap(Model.write(supplied))));
    const refused = HostError.refused(`${kind} ${method}`);
    return yield* Model.commit(
        payload.label,
        Effect.try({ try: (): unknown => Reflect.get(receiver, method), catch: refused }).pipe(
            Effect.filterOrFail(Predicate.isFunction, refused),
            Effect.flatMap((invoked) => Effect.try({ try: (): unknown => Reflect.apply(invoked, receiver, positioned), catch: refused })),
            Effect.tap((returned) => Effect.transposeOption(Option.map(properties, Function.apply(returned)))),
            Effect.flatMap((returned) => Model.read(Called, payload.label, flow(Function.constant({ ...before, result: Option.fromUndefinedOr(returned) }), Struct.evolve({ valid: flow(Function.constant(found), Struct.get('isValid')) })))),
        ),
    );
});
const findChange = (payload: Parameters<typeof search>[0] & { readonly label: string; readonly scope: Option.Option<Specifier>; readonly slice: Slice }) =>
    Option.match(payload.scope, { onSome: Model.resolve, onNone: () => Model.current }).pipe(
        Effect.flatMap((found: unknown) => (Option.isSome(payload.change) ? Model.commit(payload.label, search(payload, found)) : search(payload, found))),
        Effect.map(({ count, matches }) => ({ count, matches: payload.slice.of(matches) })),
    );
const font = Effect.fn('font')(function* ({ name, text, file }: { readonly name: string; readonly text: string; readonly file: string }) {
    const [face, model, { fs }] = yield* Effect.all([Effect.flatMap(Model.fonts, (lookup) => lookup(name)), Model, Uxp]);
    const subset = Effect.as(Effect.try({ try: () => face.createSubsetFont(text, file), catch: HostError.refused(`subset of ${face.name}`) }), Option.some({ file, postscriptName: Option.none<string>() }));
    const source = face.status.equals(model.FontStatus.INSTALLED)
        ? yield* Effect.try({ try: () => fs.lstatSync(face.location), catch: HostError.refused(face.location) }).pipe(
              Effect.as(Option.some({ file: face.location, postscriptName: Option.some(face.postscriptName) })),
              Effect.catchIf((error) => Schema.is(MissingFile)(error.cause), () => subset),
          )
        : Option.none();
    return { font: yield* fontView(face), source };
});
const menu = ({ area }: { readonly area: Option.Option<string> }) =>
    Effect.flatMap(
        actions,
        Unmatched.matching('menu area', area, (row) => [row.area]),
    );
const invoke = Effect.fn('invoke')(function* ({ label, action }: { readonly label: string; readonly action: number | string }) {
    const [{ app }, listed] = yield* Effect.all([Model, actions]);
    const chosen = yield* Unmatched.single(listed, 'menu action', Option.some(action.toString()), ({ id, name, area }) => [`${id} ${name} (${area})`, id.toString(), name]);
    yield* Model.commit(label, Effect.try({ try: () => app.menuActions.itemByID(chosen.id).invoke(), catch: HostError.refused(chosen.name) }));
    return chosen;
});
const preflight = Effect.fn('preflight')(function* ({ document, profile }: { readonly document: Option.Option<Specifier>; readonly profile: Option.Option<string> }) {
    const model = yield* Model;
    const open = model.app.documents.everyItem().getElements();
    const targets = yield* Option.match(document, {
        onNone: () => Effect.succeed(open.filter((each) => !each.preflightOptions.preflightOff)),
        onSome: (specifier) =>
            Effect.map(
                Effect.filterOrElse(Model.resolve(specifier), model.document, () => Unmatched.make({ subject: 'open document', query: Option.some(specifier.path), candidates: Array.map(open, (each) => each.toSpecifier()) })),
                Array.of,
            ),
    });
    return yield* Effect.forEach(targets, (target) => checked(target, profile));
});
const execute = Effect.fn('execute')(function* ({ label, code }: { readonly label: string; readonly code: string }) {
    const { exported } = yield* Model;
    const given = { ...Object.fromEntries(exported), resolve: flow(Schema.decodeUnknownEffect(Specifier.Path), Effect.flatMap(Model.resolve), Effect.runSyncWith(yield* Effect.context<Model>())) };
    const result = yield* Model.commit(label, yield* Uxp.evaluate(given, code));
    return yield* Model.read(Executed, label, () => ({ result }));
});
const open = Effect.fn('open')(function* ({ label, file }: { readonly label: string; readonly file: URL }) {
    const [{ app }, path] = yield* Effect.all([Model, Path.Path]);
    const native = yield* path.fromFileUrl(file);
    return yield* Model.reference(yield* Effect.try({ try: () => app.open(native, false), catch: HostError.refused(label) }));
});
const rasterize = Effect.fn('rasterize')(function* ({ target, longestSide, options, file, label }: { readonly label: string; readonly target: string; readonly longestSide: number; readonly options: Option.Option<Values>; readonly file: URL }) {
    const [model, specifier, path] = yield* Effect.all([Model, Schema.decodeEffect(Specifier.Path)(target), Path.Path]);
    const { app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum } = model;
    const found = yield* Effect.filterOrFail(
        Model.resolve(specifier),
        (value): value is Page | Spread | PageItem => (model.page(value) && model.spread(value.parent)) || model.spread(value) || model.item(value),
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
    const given = Option.getOrElse(options, Record.empty);
    yield* Uxp.preserve(preferences, defaults, [...Struct.keys(defaults), ...Record.keys(given)]);
    yield* Model.write(preferences, given);
    const native = yield* path.fromFileUrl(file);
    yield* Effect.try({ try: () => source.exportFile(ExportFormat.PNG_FORMAT, native), catch: HostError.refused(label) });
}, Effect.scoped);
const objectModel = ({ name }: { readonly name: Option.Option<string> }) => Effect.flatMap(Model, (model) => Declaration.describe(name, model.classes, model.enumerations, declaration));
const session = Effect.flatMap(Model, ({ app, MeasurementUnits, UserInteractionLevels }) => Uxp.preserve(app.scriptPreferences, { enableRedraw: false, measurementUnit: MeasurementUnits.POINTS, userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }));
const listened = Effect.fnUntraced(function* (events: readonly string[], name: string, handler: () => void) {
    const { app } = yield* Model;
    const added = (event: string) => {
        const named = `${name}:${event}`;
        const stale = app.eventListeners.itemByName(named);
        if (stale.isValid) {
            stale.remove();
        }
        return Object.assign(app.addEventListener(event, handler), { name: named });
    };
    const removed = (listener: ReturnType<typeof added>) => Effect.sync(() => (listener.isValid ? listener.remove() : undefined));
    yield* Effect.forEach(
        events,
        (event) =>
            Effect.acquireRelease(
                Effect.sync(() => added(event)),
                removed,
            ),
        { discard: true },
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { call, change, execute, findChange, font, inspect, invoke, listened, menu, objectModel, open, preflight, rasterize, session };
