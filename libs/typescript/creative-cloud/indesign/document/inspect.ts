import indesign, { type Document, type InDesignDOMObject, type ReadonlyNamedDOMObject } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Effect, flow, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { InDesignError, Operations } from '../models.ts';
import { active, collection, host, kinds, Model, members, owning, read, resolve, unavailable } from './objects.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [STATE]
const problems = Effect.fn('problems')(function* (document: Document, named: Option.Option<string>) {
    const { app } = indesign;
    const selected = Option.getOrElse(named, () => document.preflightOptions.preflightWorkingProfile);
    const profile = yield* Predicate.isString(selected)
        ? Effect.gen(function* () {
              const profiles = app.preflightProfiles.everyItem().getElements();
              return yield* Effect.fromOption(
                  Array.findFirst(profiles, (each) => each.name === selected),
                  () => Unmatched.make({ subject: 'preflight profile', query: selected, candidates: Array.map(profiles, Struct.get('name')) }),
              );
          })
        : Effect.succeed(selected);
    const process = yield* Effect.acquireRelease(
        Effect.sync(() => app.preflightProcesses.add(document, profile)),
        (added) => Effect.sync(() => added.remove()),
    );
    yield* Effect.sync(() => process.waitForProcess());
    const [documentName, profileName, rows] = process.aggregatedResults;
    return yield* read(Operations.preflight.success, () => ({ documentName, profileName, appliedProfile: process.appliedProfile, results: Array.map(rows, ([parentNodeID, errorName, pageNumber, errorInfo, errorDetail]) => ({ parentNodeID, errorName, pageNumber, errorInfo, errorDetail })) }));
}, Effect.scoped);
const state = Effect.fn('state')(function* () {
    const { app, StyleType } = indesign;
    const modal = app.modalState;
    const documents = modal ? [] : app.documents.everyItem().getElements();
    const paths = yield* Effect.forEach(documents, (document) => (document.saved ? Effect.asSome(Effect.promise(() => document.fullName)) : Effect.succeedNone));
    const opened = Option.filter(Option.liftThrowable(() => app.activeDocument)(), () => !modal);
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
    const live = Option.map(opened, (document) => ({
        document,
        window: app.activeWindow,
        selection: selection(),
        overset: overset(document),
        undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory },
    }));
    const listed = Array.zipWith(documents, paths, (document, path) => ({ specifier: document, name: document.name, modified: document.modified, saved: document.saved, path: Option.getOrUndefined(Option.map(path, Struct.get('nativePath'))), windows: document.windows.everyItem().getElements() }));
    return yield* read(Operations.inspect.success, () => ({ _tag: 'State', version: app.version, status: modal ? { _tag: 'Modal' } : { _tag: 'Ready', tool: app.toolBoxTools.currentTool, panels, documents: listed, active: Option.getOrUndefined(live) } }));
});
const menu = Effect.fn('menu')(function* ({ area }: typeof Operations.menu.payload.Type) {
    const actions = yield* read(Operations.menu.success, () => {
        const every = indesign.app.menuActions.everyItem();
        return Array.zipWith(Array.zip(every.id, every.name), every.area, ([id, name], within) => ({ id, name, area: within }));
    });
    const chosen = Option.match(area, { onNone: () => actions, onSome: (wanted) => actions.filter((action) => action.area === wanted) });
    yield* Option.isSome(area) && Array.isReadonlyArrayEmpty(chosen) ? Effect.fail(Unmatched.make({ subject: 'menu area', query: area.value, candidates: Array.dedupe(Array.map(actions, Struct.get('area'))) })) : Effect.void;
    return chosen;
});
const preflight = ({ profile }: typeof Operations.preflight.payload.Type) => Effect.flatMap(active, (document) => problems(document, profile));

// --- [OBJECTS]
const lineage: (name: string) => Effect.Effect<readonly string[], InDesignError, Model> = Effect.fnUntraced(function* (name: string) {
    const parent = yield* Cache.get(yield* Model, name).pipe(
        Effect.map(flow(Struct.get('parent'), Option.liftPredicate(Predicate.isTruthy))),
        Effect.catchTag('Unmatched', () => Effect.succeedNone),
    );
    return Option.isSome(parent) ? [parent.value, ...(yield* lineage(parent.value))] : [];
});
const taken = (found: Pick<InDesignDOMObject<unknown>, 'constructorName' | 'toSpecifier'>, wanted: Option.Option<readonly string[]>) => {
    const prototype = Object.getPrototypeOf(found);
    const fields = () => Array.flatMap(Object.entries(Object.getOwnPropertyDescriptors(prototype)), ([name, { get }]) => (get === undefined ? [] : [name]));
    return Effect.forEach(
        Option.getOrElse(wanted, () => fields().filter((name) => name !== 'properties')),
        (name) =>
            Effect.gen(function* () {
                yield* Object.getOwnPropertyDescriptor(prototype, name)?.get === undefined ? Effect.fail(Unmatched.make({ subject: `field of ${found.toSpecifier()}`, query: name, candidates: fields() })) : Effect.void;
                const refused = InDesignError.refused(Option.some(name));
                const blocked = unavailable(found.constructorName, name, Option.some(found), false);
                yield* Option.isNone(blocked) ? Effect.void : Effect.fail(refused(blocked.value));
                const value = yield* Effect.try({ try: (): unknown => Reflect.get(found, name), catch: refused });
                return yield* Predicate.isPromiseLike(value) ? Effect.tryPromise({ try: () => Promise.resolve(value), catch: refused }) : Effect.succeed(value);
            }).pipe(
                Effect.result,
                Effect.map((value) => [name, value] as const),
            ),
    );
};
const inspectObject = Effect.fn('inspectObject')(function* (found: InDesignDOMObject<unknown>, fields: (typeof Operations.inspect.payload.Type)['fields']) {
    const { fields: own } = yield* Cache.get(yield* Model, found.constructorName);
    const entries = yield* taken(found, fields);
    const field = Effect.fnUntraced(function* ([name, result]: (typeof entries)[number]) {
        const row = { ...own[name], name, unavailable: undefined };
        return yield* Effect.fromResult(result).pipe(
            Effect.matchEffect({
                onFailure: (error) => Effect.map(Schema.encodeEffect(Schema.Defect())(error), (failure) => [{ ...row, _tag: 'Failure' as const, unavailable: failure }]),
                onSuccess: (value) => Effect.succeed(collection(value) ? [] : [{ ...row, _tag: 'Success' as const, value: value === undefined ? null : value }]),
            }),
            Effect.orDie,
        );
    });
    return {
        _tag: 'Object',
        type: found.constructorName,
        specifier: found,
        fields: Array.flatten(yield* Effect.forEach(entries, field)),
        collections: Array.flatMap(entries, ([name, value]) => Result.match(value, { onFailure: () => [], onSuccess: (each) => (collection(each) ? [{ name, type: each.constructor.name, count: each.length, specifier: each }] : []) })),
    };
});
const typed = Effect.fn('typed')(function* (name: string, selected: (typeof Operations.inspect.payload.Type)['fields']) {
    const value: unknown = Reflect.get(indesign, name);
    const { fields, methods, statics } = yield* Cache.get(yield* Model, name).pipe(Effect.catchTag('Unmatched', () => Effect.succeed({ fields: {}, methods: {}, statics: {} })));
    const parents = yield* lineage(name);
    const includes = (key: string) => Option.isNone(selected) || selected.value.includes(key);
    const live = Predicate.isFunction(value) ? Object.entries(Object.getOwnPropertyDescriptors(value.prototype)).filter(([key]) => includes(key)) : [];
    return value instanceof indesign.Enumeration
        ? {
              _tag: 'Enumeration',
              name,
              members: Array.map(
                  members(value).filter((each) => includes(String(each))),
                  (each) => ({ name: String(each), description: Reflect.get(statics, String(each)) || undefined }),
              ),
          }
        : {
              _tag: 'Class',
              name,
              parents,
              fields: Array.flatMap(live, ([key, { get }]) => (get === undefined ? [] : [Option.getOrElse(Record.get(fields, key), () => ({ name: key }))])),
              methods: Array.flatMap(live, ([key, { value: method }]) => (Predicate.isFunction(method) && method !== value ? [Option.getOrElse(Record.get(methods, key), () => ({ name: key }))] : [])),
          };
});
const items = Effect.fn('items')(function* (target: string, found: readonly InDesignDOMObject<unknown>[], fields: (typeof Operations.inspect.payload.Type)['fields']) {
    const named = owning<ReadonlyNamedDOMObject<unknown>>('name');
    const results = yield* Effect.forEach(found, (item) =>
        taken(
            item,
            Option.orElseSome(fields, (): readonly string[] => []),
        ),
    );
    const values = yield* Effect.forEach(
        results,
        Effect.fnUntraced(function* (entries) {
            const attributes = Record.fromEntries(entries);
            const failures = Option.liftPredicate(Record.getFailures(attributes), Predicate.not(Record.isEmptyReadonlyRecord));
            return {
                values: Record.getSuccesses(attributes),
                unavailable: yield* Effect.transposeOption(Option.map(failures, Schema.encodeEffect(Operations.inspect.success.cases.Items.fields.items.value.fields.unavailable.from.schema))).pipe(Effect.map(Option.getOrUndefined), Effect.orDie),
            };
        }),
    );
    return { _tag: 'Items', specifier: target, total: found.length, items: Array.zipWith(found, values, (item, attributes) => ({ type: item.constructorName, specifier: item, name: named(item) ? item.name : undefined, ...attributes })) };
});
const described = Effect.fn('described')(function* (target: string, fields: (typeof Operations.inspect.payload.Type)['fields']) {
    const resolved = Effect.fnUntraced(function* () {
        const found = yield* Effect.catchTag(resolve(target), 'Unmatched', (missed) => Effect.fail(Array.isReadonlyArrayEmpty(missed.candidates) ? Unmatched.make({ ...missed, candidates: kinds }) : missed));
        return host(found) ? yield* inspectObject(found, fields) : yield* items(target, found, fields);
    });
    const raw = yield* Option.match(
        Array.findFirst(kinds, (name) => name.toLowerCase() === target.toLowerCase()),
        { onSome: (name) => typed(name, fields), onNone: resolved },
    );
    return yield* read(Operations.inspect.success, () => raw);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { described, menu, preflight, state };
