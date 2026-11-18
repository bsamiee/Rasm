import { HostError, Selector, Specifier } from '@rasm/object-model';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/plugin';
import type { Application, CellStyle, CharacterStyle, Document, Font, InDesignDOMObject, MasterSpread, ObjectStyle, Page, PageItem, ParagraphStyle, PNGExportPreference, PreflightProcess, Spread, TableStyle, Text, TextFrame, TextPath, TextStyleRange } from 'adobe:indesign';
import { Array, Cause, Effect, Formatter, Function, flow, identity, Match, Number, Option, Predicate, pipe, Record, Result, Schema, SchemaGetter, String, Struct } from 'effect';
import { Model } from '#/uxp/model';
import { InDesignSettings } from '#/uxp/settings';
import type { Value } from '#/value';
import { Called, Inspection, MenuActions, Search, type Slice } from '#/views';

// --- [TYPES] ---------------------------------------------------------------------------

type Values = Record.ReadonlyRecord<string, typeof Value.Type>;

// --- [MODELS] --------------------------------------------------------------------------

const Contents = Schema.Struct({ contents: Schema.Json });
const Flat = Schema.Tuple([Schema.Int, Schema.String, Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.String, Schema.String]))]);
// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SEARCH]

const preserved = Effect.fnUntraced(function* (member: ReturnType<typeof Search.members<typeof Search.Type>>['preferences'][number]) {
    const { app, NothingEnum } = yield* Model;
    const saved = yield* Model.saved(app[member]);
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
                Match.when(Predicate.isString, identity<string>),
                Match.orElse(Formatter.format),
            ),
        ).join('');
    const searched = model.owning(members.change);
    const scopes = yield* Effect.forEach(
        Array.ensure(located),
        flow(
            Option.liftPredicate(searched),
            Effect.fromOption(() => Unmatched.of({ subject: `${mode} search target`, query: model.domObject(located) ? Option.some(located.toSpecifier()) : Option.none() })),
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
const differing = (object: InDesignDOMObject<unknown>, properties: Record.ReadonlyRecord<string, unknown>, baseValue: (name: string) => unknown, except: Schema.Struct.Fields) =>
    Effect.map(Effect.all([Model, Model.encoder]), ([{ declaration }, encoded]) => {
        const { fields } = declaration(object.constructorName);
        return Record.filter(properties, (value, name) => Option.exists(Record.get(fields, name), Struct.get('settable')) && !Object.hasOwn(except, name) && encoded(value) !== encoded(baseValue(name)));
    });
const readings = Effect.fn('readings')(function* (found: InDesignDOMObject<unknown>, wanted: Option.Option<readonly string[]>) {
    const { declaration } = yield* Model;
    const missing = Option.getOrElse(wanted, Array.empty<string>).filter((name) => !Predicate.hasProperty(found, name));
    yield* Array.isReadonlyArrayEmpty(missing) ? Effect.void : Unmatched.of({ subject: `field of ${found.toSpecifier()}`, query: Option.some(missing.join(', ')), candidates: Record.keys(declaration(found.constructorName).fields) });
    const entries = yield* Option.match(wanted, {
        onNone: () =>
            Effect.map(
                Model.properties(found),
                Record.collect((name, held): readonly [string, Effect.Effect<unknown, HostError>] => [name, Effect.succeed(held)]),
            ),
        onSome: (names) =>
            Effect.succeed(
                Array.map(names, (name): readonly [string, Effect.Effect<unknown, HostError>] => {
                    const refused = HostError.refused(`${found.toSpecifier()} ${name}`);
                    return [name, Effect.andThen(Effect.transposeOption(Option.map(Model.unavailable(found.constructorName, name, Option.some(found), false), refused)), Effect.try({ try: (): unknown => Reflect.get(found, name), catch: refused }))];
                }),
            ),
    });
    return yield* Effect.forEach(
        entries,
        Effect.fnUntraced(function* ([name, read]) {
            const value = yield* Effect.result(
                Effect.gen(function* () {
                    const held = yield* read;
                    return Predicate.isPromise(held) ? Option.getOrNull(yield* Model.file(found, name)) : (held ?? null);
                }),
            );
            return { name, value };
        }),
    );
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
                    const properties = yield* Model.properties(style);
                    const { basedOn } = properties;
                    const container = Option.getOrElse(Array.findFirst(ancestors, model.document), (): Application => model.app);
                    const [own, above] = yield* Effect.all([Schema.decodeEffect(Specifier.Path)(style.toSpecifier()), Schema.decodeEffect(Specifier.Path)(container.toSpecifier())]);
                    const base: unknown = Predicate.isString(basedOn) ? yield* Model.resolve(Specifier.make({ steps: [...above.steps, ...Array.map(Array.takeRight(own.steps, 1), ({ kind }) => ({ separator: '/' as const, kind, selector: Selector.cases.Key.make({ key: 'name', value: basedOn }) }))] })) : basedOn;
                    const baseProperties = model.domObject(base) ? yield* Model.properties(base) : Record.empty<string, unknown>();
                    const differs = yield* differing(style, properties, (name): unknown => baseProperties[name], Inspection.Style.cases.ParagraphStyle.fields);
                    const settings = yield* Effect.transposeOption(Option.map(Option.liftPredicate(style, model.paragraphStyle), InDesignSettings.read));
                    return yield* Model.picked(
                        Inspection.Style,
                        style,
                        flow(() => ({ group: ancestors, basedOn: model.ancestors(basedOn, 'basedOn'), differing: differs, settings }), Struct.evolve({ group: Array.filterMap((ancestor) => (model.named(ancestor) && !model.document(ancestor) ? Result.succeed(ancestor.name) : Result.failVoid)) })),
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
                            const [properties, [paragraphStyle, characterStyle]] = yield* Effect.all([Model.properties(range), Effect.try({ try: () => [{ ...styles[range.appliedParagraphStyle.toSpecifier()] }, { ...styles[range.appliedCharacterStyle.toSpecifier()] }] as const, catch: refused })]);
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
    const shown = yield* view(model, found, slice);
    const entries = yield* readings(
        found,
        Option.orElse(fields, () => Option.as(shown, Record.keys(Record.filter(model.declaration(found.constructorName).fields, ({ collection }, name) => Option.isSome(collection) && Predicate.hasProperty(found, name))))),
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
    const { method } = payload;
    const { receiver, kind } = yield* Option.match(payload.collection, {
        onNone: () => Effect.succeed({ receiver: found, kind: before.type }),
        onSome: (field) => {
            const member: unknown = Reflect.get(found, field);
            return model.collection(member)
                ? Effect.succeed({ receiver: member, kind: member.constructor.name })
                : Unmatched.of({ subject: `collection of ${before.specifier}`, query: Option.some(field), candidates: Record.keys(Record.filter(model.declaration(before.type).fields, ({ collection }) => Option.isSome(collection))) });
        },
    });
    const { methods } = model.declaration(kind);
    const refused = HostError.refused(`${kind} ${method}`);
    const invoked = yield* Effect.try({ try: (): unknown => Reflect.get(receiver, method), catch: refused }).pipe(Effect.filterOrFail(Predicate.isFunction, () => Unmatched.of({ subject: `method of ${kind}`, query: Option.some(method), candidates: Record.keys(methods) })));
    const parameters = Option.getOrElse(Record.get(methods, method), Array.empty<readonly string[]>);
    const positioned = yield* Effect.forEach(payload.arguments, (value, index) => Model.decode(found, Option.getOrElse(Array.get(parameters, index), Array.empty<string>))(value));
    const properties = Option.map(payload.properties, (supplied) => flow(Array.ensure<unknown>, Array.findFirst(model.domObject), Effect.fromOption(Function.constant(new Cause.IllegalArgumentError(`${kind} ${method} returned no object for properties`))), Effect.flatMap(Model.write(supplied))));
    return yield* Model.commit(
        payload.label,
        Effect.try({ try: (): unknown => Reflect.apply(invoked, receiver, positioned), catch: refused }).pipe(
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
              Effect.catchIf(
                  (error) => Schema.is(Uxp.Missing)(error.cause),
                  () => subset,
              ),
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
                Effect.filterOrElse(Model.resolve(specifier), model.document, () => Unmatched.of({ subject: 'open document', query: Option.some(specifier.path), candidates: Array.map(open, (each) => each.toSpecifier()) })),
                Array.of,
            ),
    });
    return yield* Effect.forEach(targets, (target) => checked(target, profile));
});
const open = Effect.fn('open')(function* ({ label, file }: { readonly label: string; readonly file: string }) {
    const { app } = yield* Model;
    return yield* Model.reference(yield* Effect.try({ try: () => app.open(file, false), catch: HostError.refused(label) }));
});
const rasterize = Effect.fn('rasterize')(function* (target: string, longestSide: number, options: Option.Option<Values>, path: string) {
    const [model, specifier] = yield* Effect.all([Model, Schema.decodeEffect(Specifier.Path)(target)]);
    const { app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum } = model;
    const found = yield* Effect.filterOrFail(
        Model.resolve(specifier),
        (value): value is Page | Spread | PageItem => (model.page(value) && model.spread(value.parent)) || model.spread(value) || model.item(value),
        () => Unmatched.of({ subject: 'document page, spread, or page item', query: Option.some(target) }),
    );
    const { source, placed, range } = Match.value(found).pipe(
        Match.when(model.page, (each) => ({ source: each.parent.parent, placed: [each], range: { pageString: `+${each.documentOffset + 1}`, exportingSpread: false } })),
        Match.when(model.spread, (each) => ({ source: each.parent, placed: each.pages.everyItem().getElements(), range: { pageString: `+${each.pages.firstItem().documentOffset + 1}`, exportingSpread: true } })),
        Match.orElse((each) => ({ source: each, placed: [each], range: {} })),
    );
    const { top, left, bottom, right } = yield* Model.extent(...placed);
    const [pointsPerInch, across, preferences] = [72, Math.max(right - left, bottom - top), app.pngExportPreferences];
    const resolution = (longestSide * pointsPerInch) / across;
    const declared = Record.get(model.declaration(preferences.constructorName).fields, 'exportResolution' satisfies keyof PNGExportPreference);
    const { minimum, maximum } = yield* Effect.orDie(Effect.fromOption(Option.flatMap(declared, ({ minimum: low, maximum: high }) => Option.all({ minimum: low, maximum: high }))));
    yield* resolution < minimum
        ? new Cause.IllegalArgumentError(`longestSide ${longestSide} px of ${target}, ${across} pt across, needs ${resolution} ppi, under the declared exportResolution range ${minimum} to ${maximum} ppi, so longestSide takes ${Math.ceil((minimum * across) / pointsPerInch)} px or more`)
        : Effect.void;
    const defaults = {
        pngColorSpace: PNGColorSpaceEnum.RGB,
        pngQuality: PNGQualityEnum.MAXIMUM,
        transparentBackground: false,
        antiAlias: true,
        useDocumentBleeds: false,
        pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
        pngSuffix: '',
        exportingHiddenSpread: true,
        exportResolution: Math.min(resolution, maximum),
        ...range,
    };
    const given = Option.getOrElse(options, Record.empty);
    yield* Uxp.preserve(preferences, defaults, [...Struct.keys(defaults), ...Record.keys(given)]);
    yield* Model.write(preferences, given);
    yield* Effect.try({ try: () => source.exportFile(ExportFormat.PNG_FORMAT, path), catch: HostError.refused(`PNG export of ${target} to ${path}`) });
}, Effect.scoped);
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

export { call, change, findChange, font, inspect, invoke, listened, menu, open, preflight, rasterize, session };
