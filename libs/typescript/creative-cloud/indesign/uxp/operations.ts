import { HostError, Selector, Specifier } from '@rasm/object-model/model';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/plugin';
import type { Application, CellStyle, CharacterStyle, Document, Font, InDesignDOMObject, MasterSpread, ObjectStyle, Page, PageItem, ParagraphStyle, PreflightProcess, Spread, TableStyle, Text, TextFrame, TextPath, TextStyleRange } from 'adobe:indesign';
import { Array, Cause, Effect, Formatter, Function, flow, identity, Match, Number, Option, Predicate, Record, Result, Schema, SchemaGetter, String, Struct } from 'effect';
import type { FontSource } from '#/typesetting';
import { Dom } from '#/uxp/dom';
import { InDesignSettings } from '#/uxp/settings';
import { Call, type Change, type FindChange, type Inspect, Inspection, type Issue, type Preflight, FindChangeMode, type Slice } from '#/views';

// --- [MODELS] --------------------------------------------------------------------------

const Contents = Schema.Struct({ contents: Schema.Json });
const Row = Schema.Tuple([Schema.Int, Schema.String, Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.String, Schema.String]))]);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SEARCH]

const search = Effect.fnUntraced(function* (payload: Pick<FindChange, 'mode' | 'find' | 'change' | 'options'>, located: unknown) {
    const members = FindChangeMode.members(payload.mode);
    const dom = yield* Dom;
    const { app, enumerator } = dom;
    yield* Effect.forEach(
        Record.values(members.preferences),
        Effect.fnUntraced(function* (member) {
            const saved = yield* Dom.saved(app[member]);
            const reset = Effect.try({ try: () => Object.assign(app, { [member]: dom.NothingEnum.NOTHING })[member], catch: HostError.refused(`reset of ${member}`) });
            return yield* Effect.acquireRelease(Effect.as(reset, saved), Function.constant(Effect.flatMap(Effect.orDie(reset), (cleared) => Uxp.restore(cleared, saved))));
        }),
        { discard: true },
    );
    const countItems = Match.type<number | readonly unknown[]>().pipe(Match.when(Predicate.isNumber, identity<number>), Match.orElse(Array.length));
    const spellText = Match.type<unknown>().pipe(
        Match.when(enumerator, (special) => `<${special.toString()}>`),
        Match.when(Predicate.isString, identity<string>),
        Match.orElse(Formatter.format),
    );
    const scopes = yield* Effect.forEach(Array.ensure(located), (each) => (dom.owning(members.change)(each) ? Effect.succeed(each) : Unmatched.of({ subject: `${payload.mode} search target`, query: dom.domObject(located) ? Option.some(located.toSpecifier()) : Option.none() })));
    yield* Effect.forEach(Record.toEntries(members.preferences), ([field, target]) => Effect.transposeOption(Option.map(payload[field], (values) => Effect.flatten(Dom.write(app[target], values)))), { discard: true });
    const describe = (hit: Text | PageItem) => {
        const text = Option.liftPredicate(hit, dom.text);
        const placed = Option.match(text, { onNone: () => Option.liftPredicate(hit, dom.item), onSome: ({ parentTextFrames }) => Option.filter(Array.head(parentTextFrames), dom.item) });
        return {
            specifier: hit.toSpecifier(),
            text: Option.map(text, ({ contents }) => Array.ensure<unknown>(contents).map(spellText).join('')),
            page: Option.map(Option.flatMapNullishOr(placed, Struct.get('parentPage')), Struct.get('name')),
        };
    };
    const found = yield* Effect.forEach(
        scopes,
        Effect.fnUntraced(function* (each) {
            const owner = dom.owner(each);
            yield* Effect.transposeOption(Option.as(owner, Uxp.preserve(app, Record.getSomes({ activeDocument: owner }))));
            const refused = HostError.refused(`${payload.mode} search of ${each.toSpecifier()}`);
            const hits = yield* Effect.try({ try: () => each[members.find](), catch: refused });
            const matches = yield* Effect.try({ try: () => (Predicate.isNumber(hits) ? [] : hits.map(describe)), catch: refused });
            const changes = yield* Effect.transposeOption(Option.as(payload.change, Effect.try({ try: () => countItems(each[members.change]()), catch: refused })));
            return { count: { matches: countItems(hits), changes }, matches };
        }, Effect.scoped),
    );
    return Struct.makeReducer({ count: Struct.makeReducer({ matches: Number.ReducerSum, changes: Option.makeReducer(Number.ReducerSum) }), matches: Array.makeReducerConcat<(typeof FindChange.Result.Type)['matches'][number]>() }).combineAll(found);
}, Effect.scoped);

// --- [READS]

const nestIssues = (rows: readonly (typeof Row.Type)[]): readonly Issue[] => {
    const heads = Array.filterMap(rows, (row, index) => (row[0] === rows[0]?.[0] ? Result.succeed({ row, index }) : Result.failVoid));
    return Array.map(heads, ({ row: [, label, page, , details], index }, position) => ({
        label,
        ...Record.getSomes({ page: Option.liftPredicate(page, String.isNonEmpty) }),
        details: Record.fromEntries(details),
        rows: nestIssues(rows.slice(index + 1, Option.match(Array.get(heads, position + 1), { onNone: () => rows.length, onSome: Struct.get('index') }))),
    }));
};
const AggregatedResults = Schema.Tuple([Schema.String, Schema.String, Schema.Array(Row)]).pipe(Schema.decodeTo(Inspection.Issues, { decode: SchemaGetter.transform((aggregate) => nestIssues(aggregate[2])), encode: SchemaGetter.forbiddenEncoding }));
const readAggregated = (process: PreflightProcess) => Result.try(() => process.aggregatedResults).pipe(Result.flatMap(Schema.decodeUnknownResult(AggregatedResults)));
const overset = ({ stories }: Document): readonly string[] => Array.filterMap(Array.zip(stories.everyItem().overflows, stories.everyItem().id), ([overflows, id]) => (overflows ? Result.succeed(stories.itemByID(id).toSpecifier()) : Result.failVoid));
const readFullName = (document: Document) => Effect.flatMap(Effect.try({ try: () => document.saved, catch: HostError.refused(`saved of ${document.toSpecifier()}`) }), (saved) => (saved ? Dom.file(document, 'fullName') : Effect.succeedNone));
const overrides = (text: Pick<Text, 'textHasOverrides' | 'toSpecifier'>) =>
    Dom.use(({ StyleType }) => Effect.try({ try: () => ({ paragraph: text.textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE), character: text.textHasOverrides(StyleType.CHARACTER_STYLE_TYPE) }), catch: HostError.refused(`overrides of ${text.toSpecifier()}`) }));
const readDiffering = Effect.fnUntraced(function* (object: InDesignDOMObject<unknown>, properties: Record.ReadonlyRecord<string, unknown>, base: Record.ReadonlyRecord<string, unknown>, except: Schema.Struct.Fields) {
    const [{ declaration }, encoded] = yield* Effect.all([Dom, Dom.encoder]);
    const { fields } = declaration(object.constructorName);
    return Record.filter(properties, (value, name) => Option.exists(Record.get(fields, name), Struct.get('settable')) && !Object.hasOwn(except, name) && encoded(value) !== encoded(base[name]));
});
const readings = Effect.fn('readings')(function* (found: InDesignDOMObject<unknown>, wanted: Option.Option<readonly string[]>) {
    const { declaration } = yield* Dom;
    const missing = Option.getOrElse(wanted, Array.empty<string>).filter((name) => !Predicate.hasProperty(found, name));
    yield* Array.isReadonlyArrayEmpty(missing) ? Effect.void : Unmatched.of({ subject: `field of ${found.toSpecifier()}`, query: Option.some(missing.join(', ')), candidates: Record.keys(declaration(found.constructorName).fields) });
    const entries = yield* Option.match(wanted, {
        onNone: () => Dom.properties(found).pipe(Effect.map(Record.collect((name, held): readonly [string, Effect.Effect<unknown, HostError>] => [name, Effect.succeed(held)]))),
        onSome: (names) =>
            Effect.succeed(
                Array.map(names, (name): readonly [string, Effect.Effect<unknown, HostError>] => {
                    const refused = HostError.refused(`${found.toSpecifier()} ${name}`);
                    return [name, Option.match(Dom.unavailable(found, name, false), { onSome: refused, onNone: () => Effect.try({ try: (): unknown => Reflect.get(found, name), catch: refused }) })];
                }),
            ),
    });
    return yield* Effect.forEach(entries, ([name, read]) =>
        read.pipe(
            Effect.flatMap((held) => (Predicate.isPromise(held) ? Effect.map(Dom.file(found, name), Option.getOrNull) : Effect.succeed(held ?? null))),
            Effect.result,
            Effect.map((value) => ({ name, value })),
        ),
    );
});
const fontView = Effect.fnUntraced(function* (face: Font) {
    const dom = yield* Dom;
    const specifier = yield* Effect.orDie(Schema.decodeEffect(Specifier.FromString)(face.toSpecifier()));
    const open = Option.toArray(yield* Effect.option(Dom.current));
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
            const every = dom.app.fonts.everyItem();
            return {
                ...dom.picked(Inspection.View.cases.Font.fields.installed, face, ['styles', 'axes', 'features']),
                styles: Array.dedupe(Array.filterMap(Array.zip(every.fontFamily, every.fontStyleName), ([family, named]) => (family === face.fontFamily ? Result.succeed(named) : Result.failVoid))),
                axes: Option.liftThrowable(() => Array.zip(face.designAxesName, face.designAxesValues).map(([name, value]) => ({ name, value })))(),
                features: Array.filterMap(dom.members(dom.OpenTypeFeature), (feature) => (face.checkOpenTypeFeature(feature) ? Result.succeed(feature.toString()) : Result.failVoid)),
            };
        },
        catch: HostError.refused(`styles and features of ${face.name}`),
    }).pipe(Effect.when(Effect.sync(() => face.status.equals(dom.FontStatus.INSTALLED))));
    return yield* Dom.pick(Inspection.View.cases.Font, face, () => ({ uses: [...Array.flatMap(searched, ({ matches }) => Array.map(matches, Struct.get('specifier'))), ...styled], installed }));
});
const view = (dom: Dom['Service'], found: unknown, slice: Slice) => {
    const { picked } = dom;
    return Match.value(found).pipe(
        Match.when(
            dom.application,
            Effect.fnUntraced(function* (app: Application) {
                const opened = yield* Effect.option(Dom.current);
                const documents = yield* Effect.forEach(app.documents.everyItem().getElements(), (document) => readFullName(document).pipe(Effect.bindTo('fullName'), Effect.map(Struct.assign({ document }))));
                const window = Option.liftThrowable(() => app.activeWindow.toSpecifier())();
                const selected = yield* Effect.try({ try: () => (app.windows.length === 0 ? [] : app.selection), catch: HostError.refused('selection') });
                const selection = yield* Effect.forEach(selected, (item) =>
                    Effect.map(Effect.transposeOption(Option.map(Option.liftPredicate(item, dom.overridable), overrides)), (overridden) => ({
                        ...Dom.referenceUnsafe(item),
                        contents: Option.map(Schema.decodeUnknownOption(Contents)(item), Struct.get('contents')),
                        parentStory: Option.map(Option.liftPredicate(item, dom.storied), Struct.get('parentStory')),
                        overrides: overridden,
                    })),
                );
                const panels = Array.getSomes(globalThis.Array.from({ length: app.panels.length }, (_, index) => Option.map(Option.filter(Option.fromUndefinedOr(app.panels.item(index)), Struct.get('visible')), Struct.get('name'))));
                return yield* Dom.pick(Inspection.View.cases.Application, app, () => ({
                    toolBoxTools: { ...picked(Inspection.View.cases.Application.fields.toolBoxTools, app.toolBoxTools), tools: Array.map(dom.members(dom.UITools), (tool) => tool.toString()) },
                    panels,
                    documents: Array.map(documents, ({ document, fullName }) => ({ ...picked(Inspection.View.cases.Application.fields.documents.value, document, ['fullName']), fullName })),
                    current: Option.map(opened, (document) => ({ document: document.toSpecifier(), window, selection, overset: overset(document), undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory } })),
                }));
            }),
        ),
        Match.when(
            dom.document,
            Effect.fnUntraced(function* (document: Document) {
                const [fullName, settings] = yield* Effect.all([readFullName(document), InDesignSettings.read(document.textDefaults)]);
                return yield* Dom.pick(Inspection.View.cases.Document, document, () => ({
                    fullName,
                    overset: overset(document),
                    links: Record.map(Array.groupBy(document.links.everyItem().status, globalThis.String), Array.length),
                    textDefaults: { ...picked(Inspection.View.cases.Document.fields.textDefaults, document.textDefaults, ['settings']), settings },
                }));
            }),
        ),
        Match.when(dom.page, (page) => Effect.flatMap(Dom.extent(page), (extent) => Dom.pick(Inspection.View.cases.Page, page, Function.constant({ extent })))),
        Match.whenOr(
            dom.spread,
            dom.masterSpread,
            Effect.fnUntraced(function* (spread: Spread | MasterSpread) {
                const pages = yield* Effect.forEach(spread.pages.everyItem().getElements(), (page) => Dom.extent(page).pipe(Effect.bindTo('extent'), Effect.map(Struct.assign({ page }))));
                return yield* Dom.pick(Inspection.View, spread, () => ({ pages: Array.map(pages, ({ page, extent }) => ({ ...picked(Inspection.View.cases.Spread.fields.pages.value, page, ['extent']), extent })) }));
            }),
        ),
        Match.when(dom.layer, (layer) => Dom.pick(Inspection.View.cases.Layer, layer)),
        Match.when(
            dom.style,
            Effect.fnUntraced(function* (style: ParagraphStyle | CharacterStyle | ObjectStyle | TableStyle | CellStyle) {
                const ancestors = dom.ancestors(style.parent);
                const properties = yield* Dom.properties(style);
                const { basedOn } = properties;
                const container = Option.getOrElse(Array.findFirst(ancestors, dom.document), (): Application => dom.app);
                const [own, above] = yield* Effect.all([Schema.decodeEffect(Specifier.FromString)(style.toSpecifier()), Schema.decodeEffect(Specifier.FromString)(container.toSpecifier())]);
                const base: unknown = Predicate.isString(basedOn) ? yield* Dom.resolve(Specifier.make({ steps: [...above.steps, ...Array.map(Array.takeRight(own.steps, 1), ({ kind }) => ({ separator: '/' as const, kind, selector: Selector.cases.Key.make({ key: 'name', value: basedOn }) }))] })) : basedOn;
                const baseProperties = dom.domObject(base) ? yield* Dom.properties(base) : Record.empty<string, unknown>();
                const differs = yield* readDiffering(style, properties, baseProperties, Inspection.View.cases.ParagraphStyle.fields);
                const settings = yield* Effect.transposeOption(Option.map(Option.liftPredicate(style, dom.paragraphStyle), InDesignSettings.read));
                return yield* Dom.pick(Inspection.View, style, () => ({
                    group: Array.filterMap(ancestors, (ancestor) => (dom.named(ancestor) && !dom.document(ancestor) ? Result.succeed(ancestor.name) : Result.failVoid)),
                    basedOn: dom.ancestors(basedOn, 'basedOn'),
                    differing: differs,
                    settings,
                }));
            }),
        ),
        Match.when(dom.swatch, (swatch) => Dom.pick(Inspection.View, swatch)),
        Match.when(dom.link, (linked) => Dom.pick(Inspection.View.cases.Link, linked)),
        Match.when(dom.story, (story) =>
            Dom.pick(Inspection.View.cases.Story, story, () => ({
                overset: story.length - Number.sumAll(Array.map(story.textContainers, ({ characters }) => characters.length)),
                styles: Array.dedupe(Array.map(story.paragraphs.everyItem().appliedParagraphStyle, ({ name }) => name)),
            })),
        ),
        Match.whenOr(
            dom.text,
            dom.frame,
            Effect.fnUntraced(function* (source: Text | TextFrame | TextPath) {
                const text = dom.frame(source) ? source.texts.item(0) : source;
                const [overridden, settings] = yield* Effect.all([overrides(text), InDesignSettings.read(text)]);
                const ranges = slice.of(text.textStyleRanges.everyItem().getElements());
                const run = Inspection.View.cases.Text.fields.runs.value;
                const runs = yield* Effect.forEach(
                    ranges,
                    Effect.fnUntraced(function* (range: TextStyleRange) {
                        const refused = HostError.refused(range.toSpecifier());
                        const [properties, base] = yield* Effect.all([Dom.properties(range), Effect.try({ try: () => ({ ...range.appliedParagraphStyle.properties, ...Record.filter(range.appliedCharacterStyle.properties, (value) => !(value === null || value === '' || dom.nothing(value))) }), catch: refused })]);
                        const [own, held] = yield* Effect.all([readDiffering(range, properties, base, { ...run.fields, ...Inspection.View.cases.Text.fields }), InDesignSettings.read(range)]);
                        return yield* Effect.try({ try: () => ({ ...picked(run, range, ['designAxes', 'settings', 'differing']), designAxes: Result.try(() => range.designAxes), settings: held, differing: own }), catch: refused });
                    }),
                );
                return yield* Dom.pick(Inspection.View.cases.Text, text, () => ({ overrides: overridden, runs, settings }));
            }),
        ),
        Match.when(dom.font, fontView),
        Match.when(dom.preflightProcess, (process) => Dom.pick(Inspection.View.cases.PreflightProcess, process, () => ({ aggregatedResults: readAggregated(process) }))),
        Match.option,
        Effect.transposeOption,
    );
};

// --- [REQUESTS]

const inspect = Effect.fn('inspect')(function* ({ target, fields, slice }: Inspect) {
    const [found, dom] = yield* Effect.all([Dom.resolve(target), Dom]);
    if (!dom.domObject(found)) {
        const items = yield* Effect.forEach(slice.of(found), (item) => Effect.zipWith(Dom.reference(item), readings(item, Option.orElseSome(fields, Array.empty)).pipe(Effect.bindTo('fields')), Struct.assign));
        return yield* Dom.read(Inspection.Items, target.path, () => ({ _tag: 'Items', specifier: target.path, total: found.length, items }));
    }
    const shown = yield* view(dom, found, slice);
    const entries = yield* readings(
        found,
        Option.orElse(fields, () => Option.as(shown, Record.keys(Record.filter(dom.declaration(found.constructorName).fields, ({ collection }, name) => Option.isSome(collection) && Predicate.hasProperty(found, name))))),
    );
    const [collections, scalars] = Array.partition(
        entries,
        Match.type<(typeof entries)[number]>().pipe(
            Match.when({ value: { _tag: 'Success', success: dom.collection } }, ({ name, value }) => ({ name, ...Dom.collect(value.success) })),
            Match.result,
        ),
    );
    return Inspection.Object.make({ ...(yield* Dom.reference(found)), view: shown, fields: yield* Dom.read(Inspection.Readings, found.toSpecifier(), () => scalars), collections });
});
const change = Effect.fn('change')(function* ({ label, target, values }: Change) {
    const located = yield* Dom.locate(target);
    return yield* Dom.commitWrite(label, located, yield* Dom.write(located, values));
});
const call = Effect.fn('call')(function* (payload: Call) {
    const [dom, found] = yield* Effect.all([Dom, Dom.locate(payload.target)]);
    const before = yield* Dom.reference(found);
    const { method } = payload;
    const { receiver, kind } = yield* Option.match(payload.collection, {
        onNone: () => Effect.succeed({ receiver: found, kind: before.type }),
        onSome: (field) =>
            Effect.try({ try: (): unknown => Reflect.get(found, field), catch: HostError.refused(`${before.specifier} ${field}`) }).pipe(
                Effect.filterOrFail(dom.collection, () => Unmatched.of({ subject: `collection of ${before.specifier}`, query: Option.some(field), candidates: Record.keys(Record.filter(dom.declaration(before.type).fields, ({ collection }) => Option.isSome(collection))) })),
                Effect.map((member) => ({ receiver: member, kind: member.constructor.name })),
            ),
    });
    const { methods } = dom.declaration(kind);
    const refused = HostError.refused(`${kind} ${method}`);
    const invoked = yield* Effect.try({ try: (): unknown => Reflect.get(receiver, method), catch: refused }).pipe(Effect.filterOrFail(Predicate.isFunction, () => Unmatched.of({ subject: `method of ${kind}`, query: Option.some(method), candidates: Record.keys(methods) })));
    const parameters = Option.getOrElse(Record.get(methods, method), Array.empty<readonly string[]>);
    const positioned = yield* Effect.forEach(payload.arguments, (value, index) => Dom.decode(found, Option.getOrElse(Array.get(parameters, index), Array.empty<string>))(value));
    const properties = Option.map(payload.properties, (supplied) => flow(Array.ensure<unknown>, Array.findFirst(dom.domObject), Effect.fromOption(Function.constant(new Cause.IllegalArgumentError(`${kind} ${method} returned no object for properties`))), Effect.flatMap(flow(Dom.write(supplied), Effect.flatten))));
    return yield* Dom.commit(
        payload.label,
        Effect.try({ try: (): unknown => Reflect.apply(invoked, receiver, positioned), catch: refused }).pipe(
            Effect.tap((returned) => Effect.transposeOption(Option.map(properties, Function.apply(returned)))),
            Effect.flatMap((returned) => Dom.read(Call.Result, payload.label, () => ({ ...before, result: Option.fromUndefinedOr(returned), valid: found.isValid }))),
        ),
    );
});
const findChange = (payload: FindChange) =>
    Option.match(payload.scope, { onSome: Dom.resolve, onNone: () => Dom.current }).pipe(
        Effect.flatMap((found: unknown) => (Option.isSome(payload.change) ? Dom.commit(payload.label, search(payload, found)) : search(payload, found))),
        Effect.map(({ count, matches }) => ({ count, matches: payload.slice.of(matches) })),
    );
const font = Effect.fn('font')(function* ({ name, text, file }: FontSource) {
    const [face, dom, { fs }] = yield* Effect.all([Effect.flatMap(Dom.fonts, (lookup) => lookup(name)), Dom, Uxp]);
    const subset = Effect.as(Effect.try({ try: () => face.createSubsetFont(text, file), catch: HostError.refused(`subset of ${face.name}`) }), Option.some({ file, postscriptName: Option.none<string>() }));
    const source = face.status.equals(dom.FontStatus.INSTALLED)
        ? yield* Effect.try({ try: () => fs.lstatSync(face.location), catch: HostError.refused(face.location) }).pipe(
              Effect.as(Option.some({ file: face.location, postscriptName: Option.some(face.postscriptName) })),
              Effect.catchIf((error) => Schema.is(Uxp.Missing)(error.cause), Function.constant(subset)),
          )
        : Option.none();
    return { font: yield* fontView(face), source };
});
const preflight = Effect.fn('preflight')(function* ({ document, profile }: Preflight) {
    const dom = yield* Dom;
    const open = dom.app.documents.everyItem().getElements();
    const targets = yield* Option.match(document, {
        onNone: () => Effect.succeed(open.filter((each) => !each.preflightOptions.preflightOff)),
        onSome: (specifier) =>
            Effect.map(
                Effect.filterOrElse(Dom.resolve(specifier), dom.document, () => Unmatched.of({ subject: 'open document', query: Option.some(specifier.path), candidates: Array.map(open, (each) => each.toSpecifier()) })),
                Array.of,
            ),
    });
    return yield* Effect.forEach(
        targets,
        Effect.fnUntraced(function* (target) {
            const { preflightOff, preflightWorkingProfile } = target.preflightOptions;
            const profiles = [...dom.app.preflightProfiles.everyItem().getElements(), ...target.preflightProfiles.everyItem().getElements()];
            const live = preflightOff ? Option.none() : Option.some(target.activeProcess);
            const requested = Option.orElse(profile, () => Option.flatMapNullishOr(live, Struct.get('appliedProfile'))).pipe(Option.getOrElse(() => preflightWorkingProfile));
            const chosen = yield* Predicate.isString(requested) ? Unmatched.single(profiles, 'preflight profile', Option.some(requested), (each) => [each.name]) : Effect.succeed(requested);
            const refused = HostError.refused(`preflight of ${target.toSpecifier()} under ${chosen.name}`);
            const finished = Option.isNone(profile) ? Option.flatMap(live, flow(readAggregated, Result.getSuccess)) : Option.none();
            const waited = Effect.acquireRelease(Effect.try({ try: () => dom.app.preflightProcesses.add(target, chosen), catch: refused }), (process) => Effect.orDie(Effect.try({ try: () => process.remove(), catch: refused }))).pipe(
                Effect.tap((process) => Effect.try({ try: () => process.waitForProcess(), catch: refused })),
                Effect.flatMap((process) => Effect.fromResult(Result.mapError(readAggregated(process), refused))),
                Effect.scoped,
            );
            return yield* Effect.all({ targetObject: Dom.reference(target), appliedProfile: Dom.reference(chosen), aggregatedResults: Option.match(finished, { onSome: Effect.succeed, onNone: () => waited }) });
        }),
    );
});
const render = Effect.fn('render')(function* (target: Specifier, path: string) {
    const dom = yield* Dom;
    const { app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum } = dom;
    const found = yield* Effect.filterOrFail(
        Dom.resolve(target),
        (value): value is Document | Page | Spread | PageItem => dom.document(value) || (dom.page(value) && dom.spread(value.parent)) || dom.spread(value) || dom.item(value),
        () => Unmatched.of({ subject: 'document, document page, spread, or page item', query: Option.some(target.path) }),
    );
    const { source, range } = Match.value(found).pipe(
        Match.when(dom.document, (each) => ({ source: each, range: { pageString: '+1', exportingSpread: true } })),
        Match.when(dom.page, (each) => ({ source: each.parent.parent, range: { pageString: `+${each.documentOffset + 1}`, exportingSpread: false } })),
        Match.when(dom.spread, (each) => ({ source: each.parent, range: { pageString: `+${each.pages.firstItem().documentOffset + 1}`, exportingSpread: true } })),
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
const suppressDialogs = Dom.use(({ app, MeasurementUnits, UserInteractionLevels }) => Uxp.preserve(app.scriptPreferences, { enableRedraw: false, measurementUnit: MeasurementUnits.POINTS, userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { call, change, findChange, font, inspect, preflight, render, suppressDialogs };
