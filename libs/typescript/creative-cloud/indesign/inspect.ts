import indesign, { type Document, type MasterSpread, type Page, type Spread } from 'adobe:indesign';
import { host as running } from 'adobe:uxp';
import { Unmatched } from '@rasm/mcp/server';
import { Array, Cache, Effect, Option, Predicate, Record, Schema, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import { Inspect, Menu, Preflight } from './contract.ts';
import { active, collection, encoded, type Host, host, kinds, Model, members, owning, plural, read, record, resolve, transacted } from './dom.ts';
import { extent, holder, ruled } from './render.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [STATE]
const problems = Effect.fn('problems')(function* (document: Document, named: Option.Option<string>) {
    const { app } = indesign;
    const seconds = 30;
    const [profiles, working] = [app.preflightProfiles.everyItem().getElements(), document.preflightOptions.preflightWorkingProfile];
    const name = Option.getOrElse(named, () => (Predicate.isString(working) ? working : working.name));
    const profile = yield* Effect.fromOption(
        Array.findFirst(profiles, (each) => each.name === name),
        () => Unmatched.make({ subject: 'preflight profile', query: name, candidates: Array.map(profiles, Struct.get('name')) }),
    );
    const process = yield* Effect.acquireRelease(
        Effect.sync(() => app.preflightProcesses.add(document, profile)),
        (added) => Effect.sync(() => added.remove()),
    );
    yield* Effect.sync(() => process.waitForProcess(seconds));
    const [, , rows] = process.aggregatedResults;
    const [, found] = Array.mapAccum(rows, '', (rule, [, label, page, , [first, second]]) => (first === undefined ? [label, []] : [rule, [{ rule, problem: first[1], page, fix: second?.[1] }]]));
    return yield* read(Preflight.successSchema, () => ({ profile: name, rules: profile.preflightProfileRules.everyItem().name, problems: Array.flatten(found) }));
}, Effect.scoped);
const state = Effect.fn('state')(function* () {
    const { app, StyleType } = indesign;
    const modal = app.modalState;
    const documents = modal ? [] : app.documents.everyItem().getElements();
    const paths = yield* Effect.forEach(documents, (document) => (document.saved ? Effect.asSome(Effect.promise(() => document.fullName)) : Effect.succeedNone));
    const opened = Option.filter(Option.liftThrowable(() => app.activeDocument)(), () => !modal);
    const found = yield* Effect.transposeOption(Option.map(opened, (document) => problems(document, Option.none())));
    const text = (item: object, name: string): string | undefined => Option.getOrUndefined(Schema.decodeUnknownOption(Schema.String)(Reflect.get(item, name)));
    const panels = Array.flatMap(
        Array.makeBy(app.panels.length, (index) => app.panels.item(index)),
        (panel) => Option.getOrElse(Option.liftThrowable(() => (panel.visible ? [panel.name] : []))(), () => []),
    );
    const selection = () =>
        Array.map(app.selection, (item) => ({
            type: item.constructorName,
            specifier: item,
            name: text(item, 'name'),
            contents: text(item, 'contents'),
            story: 'parentStory' in item ? item.parentStory : undefined,
            overrides: 'textHasOverrides' in item ? Option.getOrUndefined(Option.liftThrowable(() => ({ paragraph: item.textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE), character: item.textHasOverrides(StyleType.CHARACTER_STYLE_TYPE) }))()) : undefined,
        }));
    const overset = (document: Document): readonly unknown[] => Array.flatMap(Array.zip(document.stories.everyItem().overflows, document.stories.everyItem().id), ([overflows, id]) => (overflows === true ? [document.stories.itemByID(id)] : []));
    const live = Option.map(Option.all([opened, found]), ([document, { profile, problems: issues }]) => ({
        document,
        window: app.activeWindow,
        selection: selection(),
        overset: overset(document),
        preflight: { on: !document.preflightOptions.preflightOff, profile, problems: issues.length },
        undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory },
    }));
    const listed = Array.zipWith(documents, paths, (document, path) => ({ specifier: document, name: document.name, modified: document.modified, saved: document.saved, path: Option.getOrUndefined(Option.map(path, Struct.get('nativePath'))), windows: document.windows.everyItem().getElements() }));
    return yield* read(Inspect.successSchema, () => ({ _tag: 'State', version: running.version, status: modal ? { _tag: 'Modal' } : { _tag: 'Ready', tool: app.toolBoxTools.currentTool, panels, documents: listed, active: Option.getOrUndefined(live) } }));
});
const menu = Effect.fn('menu')(function* ({ area }: Rpc.Payload<typeof Menu>) {
    const actions = yield* read(Menu.successSchema, () => {
        const every = indesign.app.menuActions.everyItem();
        return Array.zipWith(Array.zip(every.id, every.name), every.area, ([id, name], within) => ({ id, name, area: within }));
    });
    const chosen = Option.match(area, { onNone: () => actions, onSome: (wanted) => actions.filter((action) => action.area === wanted) });
    yield* Option.isSome(area) && Array.isReadonlyArrayEmpty(chosen) ? Effect.fail(Unmatched.make({ subject: 'menu area', query: area.value, candidates: Array.dedupe(Array.map(actions, Struct.get('area'))) })) : Effect.void;
    return chosen;
});
const preflight = ({ profile }: Rpc.Payload<typeof Preflight>) => Effect.flatMap(active, (document) => problems(document, profile));

// --- [OBJECTS]
const lineage: (name: string) => Effect.Effect<readonly string[], never, Model> = Effect.fnUntraced(function* (name: string) {
    const parent = Option.filter(Option.map(yield* Effect.option(Cache.get(yield* Model, name)), Struct.get('parent')), Predicate.isTruthy);
    return Option.isSome(parent) ? [parent.value, ...(yield* lineage(parent.value))] : [];
});
const measured = (value: unknown): unknown => {
    const [paged, spreadLike] = [owning<Page>('masterPageItems'), owning<Spread | MasterSpread>('showMasterItems')];
    const surface = (each: unknown): each is Page | Spread | MasterSpread => paged(each) || spreadLike(each);
    const laid = Option.liftPredicate(value, surface);
    const pages: readonly Page[] = [...(paged(value) ? [value] : []), ...(spreadLike(value) ? value.pages.everyItem().getElements() : [])];
    const page = (each: Page) => {
        const [margins, bounds] = [each.marginPreferences, ruled(each, extent(each))];
        const [near, far] = each.side.equals(indesign.PageSideOptions.LEFT_HAND) ? [margins.right, margins.left] : [margins.left, margins.right];
        const content = { top: bounds.top + margins.top, left: bounds.left + near, bottom: bounds.bottom - margins.bottom, right: bounds.right - far };
        const columns = Array.map(Array.chunksOf(margins.columnsPositions, 2), (pair) => ({ left: content.left + Array.headNonEmpty(pair), right: content.left + Array.lastNonEmpty(pair) }));
        return { specifier: each, master: each.appliedMaster ?? undefined, bounds, margins: { top: margins.top, bottom: margins.bottom, inside: margins.left, outside: margins.right }, content, columns };
    };
    const geometry = ([holding, start]: readonly [Page | Spread | MasterSpread, Page]) => {
        const { documentPreferences: setup, gridPreferences: grid } = start.parent.parent;
        return {
            bleed: { top: setup.documentBleedTopOffset, bottom: setup.documentBleedBottomOffset, inside: setup.documentBleedInsideOrLeftOffset, outside: setup.documentBleedOutsideOrRightOffset },
            baseline: { start: grid.baselineStart, division: grid.baselineDivision, relativeTo: grid.baselineGridRelativeOption },
            pages: Array.map(pages, page),
            items: Array.map(holding.allPageItems, (item) => ({
                type: item.constructorName,
                specifier: item,
                name: item.name,
                layer: item.itemLayer.name,
                appliedObjectStyle: item.appliedObjectStyle.name,
                locked: item.locked || undefined,
                anchored: item.parent.constructorName === 'Character' || undefined,
                parent: Option.getOrUndefined(holder(item)),
                story: 'parentStory' in item ? item.parentStory : undefined,
                label: item.label || undefined,
                hidden: !item.visible || undefined,
                bounds: ruled(item.parentPage ?? start, extent(item)),
                overflows: Option.getOrUndefined(Schema.decodeUnknownOption(Schema.Boolean)(Reflect.get(item, 'overflows'))),
                effectivePpi: Option.getOrUndefined(Schema.decodeUnknownOption(Schema.Tuple([Schema.Finite, Schema.Finite]))(Reflect.get(item, 'effectivePpi'))),
            })),
        };
    };
    return Option.getOrUndefined(Option.map(Option.all([laid, Option.fromNullishOr(pages[0])]), geometry));
};
const taken = (found: Host, wanted: readonly string[]) =>
    Effect.forEach(
        wanted,
        Effect.fnUntraced(function* (name) {
            const value = yield* name in found ? Effect.succeed(found[name]) : Effect.fail(Unmatched.make({ subject: `field of ${found.toSpecifier()}`, query: name, candidates: Object.getOwnPropertyNames(Object.getPrototypeOf(found)) }));
            const shape = host(value) ? yield* Effect.option(Cache.get(yield* Model, value.constructorName)) : Option.none();
            return [name, host(value) && Option.exists(shape, ({ parent }) => parent === 'Preference') ? value.properties : value] as const;
        }),
    );
const object = Effect.fn('object')(function* (found: Host, fields: Rpc.Payload<typeof Inspect>['fields']) {
    const model = yield* Model;
    const documented = owning<Document>('masterSpreads');
    const wanted = Option.getOrElse(fields, (): readonly string[] => []);
    const entries = Option.isSome(fields) ? yield* taken(found, wanted) : Object.entries(record(found.properties) ? found.properties : {});
    const own: Record.ReadonlyRecord<string, { readonly type?: string | undefined }> = Option.getOrElse(Option.map(yield* Effect.option(Cache.get(model, found.constructorName)), Struct.get('fields')), () => ({}));
    const clears = Array.map(['clearOverrides', 'clearObjectStyleOverrides', 'clearCellStyleOverrides', 'clearTableStyleOverrides'], (method) => found[method]).filter(Predicate.isFunction);
    const styled: Record.ReadonlyRecord<string, string | undefined> = Array.isReadonlyArrayNonEmpty(clears)
        ? yield* transacted(
              'Reading overrides',
              false,
              Effect.sync(() => {
                  clears.forEach((clear) => {
                      clear.call(found);
                  });
                  return Record.fromIterableWith(entries, ([name]) => [name, encoded(found[name], false)]);
              }),
          )
        : {};
    const owner = (value: unknown): Option.Option<Document> => (documented(value) ? Option.some(value) : Option.flatMap(Option.liftPredicate(value, host), (child) => owner(child.parent)));
    const document = owner(found);
    const accessors = Option.isSome(document) ? Record.toEntries(Option.getOrElse(Option.map(yield* Effect.option(Cache.get(model, 'Document')), Struct.get('fields')), () => ({}))) : [];
    const elements = yield* Effect.forEach(
        Array.flatMap(accessors, ([accessor, { type }]) => (type !== undefined && plural(type) ? [[accessor, type] as const] : [])),
        ([accessor, type]) =>
            Effect.map(Effect.option(Cache.get(model, type)), (shape) => {
                const element = Option.getOrUndefined(shape)?.methods.item?.returns?.type;
                return element === undefined ? [] : [[element, accessor] as const];
            }),
    );
    const sources = Record.fromEntries(Array.flatten(elements));
    const ranges = found.textStyleRanges;
    const runs = collection(ranges) ? ranges.everyItem() : undefined;
    const listed = Array.ensure<unknown>(runs?.properties).filter(record);
    const columns = Option.isSome(fields) ? Array.map(wanted, (name) => [name, Array.ensure<unknown>(runs?.[name])] as const) : Array.map(Object.keys(listed[0] ?? {}), (name) => [name, Array.map(listed, (each) => each[name])] as const);
    const mixed = Record.filter(
        Record.fromIterableWith(columns, ([name, column]) => [name, [...new Map(Array.map(column, (each) => [encoded(each, false), each])).values()]]),
        (distinct) => distinct.length > 1,
    );
    const shown = (name: string, value: unknown): readonly unknown[] => (collection(value) && value.length > 0 ? [{ name, count: value.length, specifier: value }] : []);
    const rest = Option.isSome(fields)
        ? []
        : Array.difference(
              Object.getOwnPropertyNames(Object.getPrototypeOf(found)),
              Array.map(entries, ([name]) => name),
          );
    const field = ([name, value]: readonly [string, unknown]): readonly unknown[] => {
        const row = own[name];
        const base = Option.filter(Option.fromNullishOr(styled[name]), (style) => style !== encoded(value, false));
        const accessor = Option.orElse(
            Option.flatMap(Option.fromNullishOr(row?.type), (type) => Record.get(sources, type)),
            () => (host(value) ? Record.get(sources, value.constructorName) : Option.none()),
        );
        const source = Option.flatMap(Option.all([document, accessor]), ([open, from]) => Option.fromNullishOr(Reflect.get(open, from)));
        return collection(value) ? [] : [{ ...(row ?? { name }), value: value ?? null, mixed: Option.getOrUndefined(Record.get(mixed, name)), style: Option.getOrUndefined(Option.flatMap(base, Schema.decodeUnknownOption(Schema.fromJsonString(Schema.Json)))), source: Option.getOrUndefined(source) }];
    };
    return {
        _tag: 'Object',
        type: found.constructorName,
        specifier: found,
        fields: Array.flatMap(entries, field),
        collections: [...Array.flatMap(entries, ([name, value]) => shown(name, value)), ...Array.flatMap(rest, (name) => Option.getOrElse(Option.liftThrowable(() => shown(name, found[name]))(), () => []))],
        geometry: measured(found),
    };
});
const typed = Effect.fn('typed')(function* (name: string) {
    const [value, shape, parents] = [Reflect.get(indesign, name), yield* Effect.option(Cache.get(yield* Model, name)), yield* lineage(name)];
    const { fields, methods, statics } = Option.getOrElse(shape, () => ({ fields: {}, methods: {}, statics: {} }));
    const live = Predicate.isFunction(value) ? Object.entries(Object.getOwnPropertyDescriptors(value.prototype)) : [];
    return value instanceof indesign.Enumeration
        ? { _tag: 'Enumeration', name, members: Array.map(members(value), (each) => ({ name: String(each), description: Reflect.get(statics, String(each)) || undefined })) }
        : {
              _tag: 'Class',
              name,
              parents,
              fields: Array.flatMap(live, ([key, { get }]) => (get === undefined ? [] : [Reflect.get(fields, key) ?? { name: key }])),
              methods: Array.flatMap(live, ([key, { value: method }]) => (Predicate.isFunction(method) && method !== value ? [Reflect.get(methods, key) ?? { name: key }] : [])),
          };
});
const items = Effect.fn('items')(function* (target: string, found: readonly Host[], fields: Rpc.Payload<typeof Inspect>['fields']) {
    const values = yield* Effect.forEach(found, (item) =>
        taken(
            item,
            Option.getOrElse(fields, (): readonly string[] => []),
        ),
    );
    return { _tag: 'Items', specifier: target, total: found.length, items: Array.zipWith(found, values, (item, entries) => ({ type: item.constructorName, specifier: item, name: item.name, values: Object.fromEntries(entries), geometry: measured(item) })) };
});
const described = Effect.fn('described')(function* (target: string, fields: Rpc.Payload<typeof Inspect>['fields']) {
    const resolved = Effect.fnUntraced(function* () {
        const found = yield* Effect.catchTag(resolve(target), 'Unmatched', (missed) => Effect.fail(Array.isReadonlyArrayEmpty(missed.candidates) ? Unmatched.make({ ...missed, candidates: kinds }) : missed));
        return host(found) ? yield* object(found, fields) : yield* items(target, found, fields);
    });
    const raw = yield* Option.match(
        Array.findFirst(kinds, (name) => name.toLowerCase() === target.toLowerCase()),
        { onSome: typed, onNone: resolved },
    );
    return yield* read(Inspect.successSchema, () => raw);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { described, menu, preflight, state };
