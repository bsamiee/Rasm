import { HostError, Selector, Specifier } from '@rasm/object-model';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/plugin';
import type { Application, CellStyle, CharacterStyle, Document, Event, Font, InDesignDOMObject, MasterSpread, ObjectStyle, Page, PageItem, ParagraphStyle, PreflightProcess, Spread, TableStyle, Text, TextFrame, TextPath, TextStyleRange } from 'adobe:indesign';
import { Array, Cause, Effect, Formatter, Function, flow, identity, Match, Number, Option, Predicate, Record, Result, Schema, SchemaGetter, String, Struct } from 'effect';
import type { FontSource } from '#/typesetting';
import { Model } from '#/uxp/model';
import { InDesignSettings } from '#/uxp/settings';
import { type Call, Called, type Change, type FindChange, type Inspect, Inspection, type Issue, type Preflight, Search, type Slice } from '#/views';

// --- [MODELS] --------------------------------------------------------------------------

const Contents = Schema.Struct({ contents: Schema.Json });
class Flat extends Schema.Opaque<Flat, { readonly brand: unique symbol }>()(Schema.Tuple([Schema.Int, Schema.String, Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.String, Schema.String]))])) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SEARCH]

const search = Effect.fnUntraced(function* ({ mode, find, change, options }: Pick<FindChange, 'mode' | 'find' | 'change' | 'options'>, located: unknown) {
    const members = Search.members(mode);
    const model = yield* Model;
    const { app, enumerator } = model;
    yield* Effect.forEach(
        members.preferences,
        Effect.fnUntraced(function* (member) {
            const saved = yield* Model.saved(app[member]);
            const reset = Effect.try({ try: () => Object.assign(app, { [member]: model.NothingEnum.NOTHING })[member], catch: HostError.refused(`reset of ${member}`) });
            return yield* Effect.acquireRelease(Effect.as(reset, saved), Function.constant(Effect.flatMap(Effect.orDie(reset), (cleared) => Uxp.restore(cleared, saved))));
        }),
        { discard: true },
    );
    const counted = Match.type<number | readonly unknown[]>().pipe(Match.when(Predicate.isNumber, identity<number>), Match.orElse(Array.length));
    const spelled = Match.type<unknown>().pipe(
        Match.when(enumerator, (special) => `<${special.toString()}>`),
        Match.when(Predicate.isString, identity<string>),
        Match.orElse(Formatter.format),
    );
    const scopes = yield* Effect.forEach(Array.ensure(located), (each) => (model.owning(members.change)(each) ? Effect.succeed(each) : Unmatched.of({ subject: `${mode} search target`, query: model.domObject(located) ? Option.some(located.toSpecifier()) : Option.none() })));
    yield* Effect.forEach(Array.zip(members.preferences, [find, options, change]), ([target, fields]) => Effect.transposeOption(Option.map(fields, (values) => Effect.flatten(Model.write(app[target], values)))), { discard: true });
    const describe = (hit: Text | PageItem) => {
        const placed = model.text(hit) ? Option.filter(Array.head(hit.parentTextFrames), model.item) : Option.liftPredicate(hit, model.item);
        return {
            specifier: hit.toSpecifier(),
            text: model.text(hit) ? Option.some(Array.ensure<unknown>(hit.contents).map(spelled).join('')) : Option.none(),
            page: Option.map(Option.flatMapNullishOr(placed, Struct.get('parentPage')), Struct.get('name')),
        };
    };
    const found = yield* Effect.forEach(
        scopes,
        Effect.fnUntraced(function* (each) {
            const owner = model.owner(each);
            yield* Effect.transposeOption(Option.as(owner, Uxp.preserve(app, Record.getSomes({ activeDocument: owner }))));
            const refused = HostError.refused(`${mode} search of ${each.toSpecifier()}`);
            const hits = yield* Effect.try({ try: () => each[members.find](), catch: refused });
            const matches = yield* Effect.try({ try: () => (Predicate.isNumber(hits) ? [] : hits.map(describe)), catch: refused });
            const changes = yield* Effect.transposeOption(Option.as(change, Effect.try({ try: () => counted(each[members.change]()), catch: refused })));
            return { count: { matches: counted(hits), changes }, matches };
        }, Effect.scoped),
    );
    const changes = Array.getSomes(found.map(({ count }) => count.changes));
    return { count: { matches: Number.sumAll(found.map(({ count }) => count.matches)), changes: Option.map(Option.liftPredicate(changes, Array.isReadonlyArrayNonEmpty), Number.sumAll) }, matches: Array.flatMap(found, Struct.get('matches')) };
}, Effect.scoped);

// --- [READS]

const nested: (rows: readonly Flat[]) => readonly Issue[] = Array.match({
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
const differing = Effect.fnUntraced(function* (object: InDesignDOMObject<unknown>, properties: Record.ReadonlyRecord<string, unknown>, base: Record.ReadonlyRecord<string, unknown>, except: Schema.Struct.Fields) {
    const [{ declaration }, encoded] = yield* Effect.all([Model, Model.encoder]);
    const { fields } = declaration(object.constructorName);
    return Record.filter(properties, (value, name) => Option.exists(Record.get(fields, name), Struct.get('settable')) && !Object.hasOwn(except, name) && encoded(value) !== encoded(base[name]));
});
const readings = Effect.fn('readings')(function* (found: InDesignDOMObject<unknown>, wanted: Option.Option<readonly string[]>) {
    const { declaration } = yield* Model;
    const missing = Option.getOrElse(wanted, Array.empty<string>).filter((name) => !Predicate.hasProperty(found, name));
    yield* Array.isReadonlyArrayEmpty(missing) ? Effect.void : Unmatched.of({ subject: `field of ${found.toSpecifier()}`, query: Option.some(missing.join(', ')), candidates: Record.keys(declaration(found.constructorName).fields) });
    const entries = yield* Option.match(wanted, {
        onNone: () => Model.properties(found).pipe(Effect.map(Record.collect((name, held): readonly [string, Effect.Effect<unknown, HostError>] => [name, Effect.succeed(held)]))),
        onSome: (names) => {
            const listed = Array.map(names, (name): readonly [string, Effect.Effect<unknown, HostError>] => {
                const refused = HostError.refused(`${found.toSpecifier()} ${name}`);
                return [name, Effect.andThen(Effect.transposeOption(Option.map(Model.unavailable(found, name, false), refused)), Effect.try({ try: (): unknown => Reflect.get(found, name), catch: refused }))];
            });
            return Effect.succeed(listed);
        },
    });
    return yield* Effect.forEach(entries, ([name, read]) =>
        read.pipe(
            Effect.flatMap((held) => (Predicate.isPromise(held) ? Effect.map(Model.file(found, name), Option.getOrNull) : Effect.succeed(held ?? null))),
            Effect.result,
            Effect.map((value) => ({ name, value })),
        ),
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
    const installed = yield* Effect.try({
        try: () => {
            const every = model.app.fonts.everyItem();
            return {
                ...model.picked(Inspection.View.cases.Font.fields.installed, face, ['styles', 'axes', 'features']),
                styles: Array.dedupe(Array.filterMap(Array.zip(every.fontFamily, every.fontStyleName), ([family, named]) => (family === face.fontFamily ? Result.succeed(named) : Result.failVoid))),
                axes: Option.liftThrowable(() => Array.zip(face.designAxesName, face.designAxesValues).map(([name, value]) => ({ name, value })))(),
                features: Array.filterMap(model.members(model.OpenTypeFeature), (feature) => (face.checkOpenTypeFeature(feature) ? Result.succeed(feature.toString()) : Result.failVoid)),
            };
        },
        catch: HostError.refused(`styles and features of ${face.name}`),
    }).pipe(Effect.when(Effect.succeed(face.status.equals(model.FontStatus.INSTALLED))));
    const uses = [...Array.flatMap(searched, ({ matches }) => Array.map(matches, Struct.get('specifier'))), ...styled];
    return yield* Model.picked(Inspection.View.cases.Font, face, () => ({ uses, installed }));
});
const view = (model: Model['Service'], found: unknown, slice: Slice) => {
    const { picked } = model;
    return Match.value(found).pipe(
        Match.when(
            model.application,
            Effect.fnUntraced(function* (app: Application) {
                const opened = yield* Effect.option(Model.current);
                const documents = yield* Effect.forEach(app.documents.everyItem().getElements(), (document) => savedPath(document).pipe(Effect.bindTo('fullName'), Effect.map(Struct.assign({ document }))));
                const window = Option.liftThrowable(() => app.activeWindow.toSpecifier())();
                const selection = yield* Effect.forEach(Option.match(opened, { onNone: Array.empty, onSome: () => app.selection }), (item) =>
                    Effect.map(Effect.transposeOption(Option.map(Option.liftPredicate(item, model.overridable), overrides)), (overridden) => ({
                        ...Model.referenceUnsafe(item),
                        contents: Option.map(Schema.decodeUnknownOption(Contents)(item), Struct.get('contents')),
                        parentStory: Option.map(Option.liftPredicate(item, model.storied), Struct.get('parentStory')),
                        overrides: overridden,
                    })),
                );
                const panels = Array.getSomes(globalThis.Array.from({ length: app.panels.length }, (_, index) => Option.map(Option.filter(Option.fromUndefinedOr(app.panels.item(index)), Struct.get('visible')), Struct.get('name'))));
                return yield* Model.picked(Inspection.View.cases.Application, app, () => ({
                    panels,
                    documents: Array.map(documents, ({ document, fullName }) => ({ ...picked(Inspection.View.cases.Application.fields.documents.value, document, ['fullName']), fullName })),
                    current: Option.map(opened, (document) => ({ document: document.toSpecifier(), window, selection, overset: overset(document), undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory } })),
                }));
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
                return yield* Model.picked(Inspection.Spread, spread, () => ({ pages: Array.map(pages, ({ page, extent }) => ({ ...picked(Inspection.Spread.cases.Spread.fields.pages.value, page, ['extent']), extent })) }));
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
                const differs = yield* differing(style, properties, baseProperties, Inspection.Style.cases.ParagraphStyle.fields);
                const settings = yield* Effect.transposeOption(Option.map(Option.liftPredicate(style, model.paragraphStyle), InDesignSettings.read));
                return yield* Model.picked(Inspection.Style, style, () => ({
                    group: Array.filterMap(ancestors, (ancestor) => (model.named(ancestor) && !model.document(ancestor) ? Result.succeed(ancestor.name) : Result.failVoid)),
                    basedOn: model.ancestors(basedOn, 'basedOn'),
                    differing: differs,
                    settings,
                }));
            }),
        ),
        Match.when(model.swatch, (swatch) => Model.picked(Inspection.Swatch, swatch)),
        Match.when(model.link, (linked) => Model.picked(Inspection.View.cases.Link, linked)),
        Match.when(model.story, (story) =>
            Model.picked(Inspection.View.cases.Story, story, () => ({
                overset: story.length - Number.sumAll(Array.map(story.textContainers, ({ characters }) => characters.length)),
                styles: Array.dedupe(Array.map(story.paragraphs.everyItem().appliedParagraphStyle, ({ name }) => name)),
            })),
        ),
        Match.whenOr(
            model.text,
            model.frame,
            Effect.fnUntraced(function* (source: Text | TextFrame | TextPath) {
                const text = model.frame(source) ? source.texts.item(0) : source;
                const [overridden, settings] = yield* Effect.all([overrides(text), InDesignSettings.read(text)]);
                const ranges = slice.of(text.textStyleRanges.everyItem().getElements());
                const run = Inspection.View.cases.Text.fields.runs.value;
                const runs = yield* Effect.forEach(
                    ranges,
                    Effect.fnUntraced(function* (range: TextStyleRange) {
                        const refused = HostError.refused(range.toSpecifier());
                        const [properties, base] = yield* Effect.all([Model.properties(range), Effect.try({ try: () => ({ ...range.appliedParagraphStyle.properties, ...Record.filter(range.appliedCharacterStyle.properties, (value) => !(value === null || value === '' || model.nothing(value))) }), catch: refused })]);
                        const [own, held] = yield* Effect.all([differing(range, properties, base, { ...run.fields, ...Inspection.View.cases.Text.fields }), InDesignSettings.read(range)]);
                        return yield* Effect.try({ try: () => ({ ...picked(run, range, ['designAxes', 'settings', 'differing']), designAxes: Result.try(() => range.designAxes), settings: held, differing: own }), catch: refused });
                    }),
                );
                return yield* Model.picked(Inspection.View.cases.Text, text, () => ({ overrides: overridden, runs, settings }));
            }),
        ),
        Match.when(model.font, fontView),
        Match.when(model.preflightProcess, (process) => Model.picked(Inspection.View.cases.PreflightProcess, process, () => ({ aggregatedResults: aggregated(process) }))),
        Match.option,
        Effect.transposeOption,
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
    const waited = Effect.acquireRelease(Effect.try({ try: () => app.preflightProcesses.add(document, chosen), catch: refused }), (process) => Effect.orDie(Effect.try({ try: () => process.remove(), catch: refused }))).pipe(
        Effect.tap((process) => Effect.try({ try: () => process.waitForProcess(), catch: refused })),
        Effect.flatMap((process) => Effect.fromResult(Result.mapError(aggregated(process), refused))),
        Effect.scoped,
    );
    return yield* Effect.all({ targetObject: Model.reference(document), appliedProfile: Model.reference(chosen), aggregatedResults: Option.match(finished, { onSome: Effect.succeed, onNone: () => waited }) });
});

// --- [REQUESTS]

const inspect = Effect.fn('inspect')(function* ({ target, fields, slice }: Inspect) {
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
const change = Effect.fn('change')(function* ({ label, target, values }: Change) {
    const located = yield* Model.located(target);
    return yield* Model.written(label, located, yield* Model.write(located, values));
});
const call = Effect.fn('call')(function* (payload: Call) {
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
    const properties = Option.map(payload.properties, (supplied) => flow(Array.ensure<unknown>, Array.findFirst(model.domObject), Effect.fromOption(Function.constant(new Cause.IllegalArgumentError(`${kind} ${method} returned no object for properties`))), Effect.flatMap(flow(Model.write(supplied), Effect.flatten))));
    return yield* Model.commit(
        payload.label,
        Effect.try({ try: (): unknown => Reflect.apply(invoked, receiver, positioned), catch: refused }).pipe(
            Effect.tap((returned) => Effect.transposeOption(Option.map(properties, Function.apply(returned)))),
            Effect.flatMap((returned) => Model.read(Called, payload.label, () => ({ ...before, result: Option.fromUndefinedOr(returned), valid: found.isValid }))),
        ),
    );
});
const findChange = (payload: FindChange) =>
    Option.match(payload.scope, { onSome: Model.resolve, onNone: () => Model.current }).pipe(
        Effect.flatMap((found: unknown) => (Option.isSome(payload.change) ? Model.commit(payload.label, search(payload, found)) : search(payload, found))),
        Effect.map(({ count, matches }) => ({ count, matches: payload.slice.of(matches) })),
    );
const font = Effect.fn('font')(function* ({ name, text, file }: FontSource) {
    const [face, model, { fs }] = yield* Effect.all([Effect.flatMap(Model.fonts, (lookup) => lookup(name)), Model, Uxp]);
    const subset = Effect.as(Effect.try({ try: () => face.createSubsetFont(text, file), catch: HostError.refused(`subset of ${face.name}`) }), Option.some({ file, postscriptName: Option.none<string>() }));
    const source = face.status.equals(model.FontStatus.INSTALLED)
        ? yield* Effect.try({ try: () => fs.lstatSync(face.location), catch: HostError.refused(face.location) }).pipe(
              Effect.as(Option.some({ file: face.location, postscriptName: Option.some(face.postscriptName) })),
              Effect.catchIf((error) => Schema.is(Uxp.Missing)(error.cause), Function.constant(subset)),
          )
        : Option.none();
    return { font: yield* fontView(face), source };
});
const preflight = Effect.fn('preflight')(function* ({ document, profile }: Preflight) {
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
const render = Effect.fn('render')(function* (target: Specifier, path: string) {
    const model = yield* Model;
    const { app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum } = model;
    const found = yield* Effect.filterOrFail(
        Model.resolve(target),
        (value): value is Document | Page | Spread | PageItem => model.document(value) || (model.page(value) && model.spread(value.parent)) || model.spread(value) || model.item(value),
        () => Unmatched.of({ subject: 'document, document page, spread, or page item', query: Option.some(target.path) }),
    );
    const { source, range } = Match.value(found).pipe(
        Match.when(model.document, (each) => ({ source: each, range: { pageString: '+1', exportingSpread: true } })),
        Match.when(model.page, (each) => ({ source: each.parent.parent, range: { pageString: `+${each.documentOffset + 1}`, exportingSpread: false } })),
        Match.when(model.spread, (each) => ({ source: each.parent, range: { pageString: `+${each.pages.firstItem().documentOffset + 1}`, exportingSpread: true } })),
        Match.orElse((each) => ({ source: each, range: {} })),
    );
    const pointsPerInch = 72;
    const defaults = {
        pngColorSpace: PNGColorSpaceEnum.RGB,
        pngQuality: PNGQualityEnum.MAXIMUM,
        transparentBackground: false,
        antiAlias: true,
        useDocumentBleeds: false,
        pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
        pngSuffix: '',
        exportingHiddenSpread: true,
        exportResolution: pointsPerInch,
        ...range,
    };
    yield* Uxp.preserve(app.pngExportPreferences, defaults);
    yield* Effect.try({ try: () => source.exportFile(ExportFormat.PNG_FORMAT, path), catch: HostError.refused(`PNG export of ${target.path} to ${path}`) });
}, Effect.scoped);
const session = Model.use(({ app, MeasurementUnits, UserInteractionLevels }) => Uxp.preserve(app.scriptPreferences, { enableRedraw: false, measurementUnit: MeasurementUnits.POINTS, userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }));
const listened = (events: readonly string[], name: string, handler: (event: Event) => void) =>
    Model.useSync(({ app }) => {
        events.forEach((event) => {
            const named = `${name}:${event}`;
            const stale = app.eventListeners.itemByName(named);
            if (stale.isValid) {
                stale.remove();
            }
            Object.assign(app.addEventListener(event, handler), { name: named });
        });
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { call, change, findChange, font, inspect, listened, preflight, render, session };
