// --- [IMPORTS] -------------------------------------------------------------------------

import { app, type Book, Document, DocumentEvent, IdleTask, Library, MeasurementUnits, SaveOptions, ScriptLanguage, UndoModes, UserInteractionLevels } from 'adobe:indesign';
import { type File, storage } from 'adobe:uxp';
import { thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { ancestors, enumerations, members, phrases, properties } from '@rasm/creative-cloud-server/indesign';
import type { AbsolutePath } from '@rasm/creative-cloud-server/values';
import { Array, Deferred, Effect, flow, HashMap, Number, Option, Predicate, Record, Schema, type Scope, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Constant {
    readonly enumeration: string;
    readonly constant: string;
    readonly enumerator: unknown;
}

interface Live {
    readonly host: typeof import('adobe:indesign');
    readonly enumerations: Readonly<Record<string, object>>;
    readonly constants: Readonly<Record<string, Readonly<Record<string, unknown>>>>;
    readonly owners: readonly {
        readonly constructor: { readonly [Symbol.hasInstance]: (value: unknown) => boolean };
        readonly properties: (typeof properties)[string];
        readonly members: Readonly<Record<string, boolean>>;
    }[];
    readonly lookup: {
        readonly exact: HashMap.HashMap<readonly [string, string], Constant>;
        readonly folded: HashMap.HashMap<readonly [string, string | number], readonly Constant[]>;
    };
}

// --- [ENUMERATIONS] --------------------------------------------------------------------

const live: Effect.Effect<Live> = Effect.runSync(
    Effect.cached(
        Effect.map(
            Effect.promise((): Promise<typeof import('adobe:indesign')> => import('adobe:indesign')),
            (host): Live => {
                const registered = Record.filter(
                    Record.fromIterableWith(Object.getOwnPropertyNames(host), (name) => [name, Reflect.get(host, name)]),
                    (value): value is object => Predicate.isObject(value) && value.constructor.name === 'Enumeration',
                );
                const constants = Record.map(registered, (enumeration) =>
                    Record.fromIterableWith(
                        Array.map(
                            Array.filter(Object.getOwnPropertyNames(Object.getPrototypeOf(enumeration)), (spelling) => spelling === spelling.toUpperCase()),
                            (spelling): unknown => Reflect.get(enumeration, spelling),
                        ),
                        (value) => [String(value), value],
                    ),
                );
                const rows = Array.flatMap(Record.toEntries(constants), ([enumeration, values]) =>
                    Array.map(Record.toEntries(values), ([name, enumerator]) => ({ enumeration, constant: name, enumerator })),
                );
                const spellings = Array.flatMap(rows, (row) =>
                    Array.map(
                        [row.constant, ...Option.toArray(Option.flatMap(Record.get(phrases, row.enumeration), Record.get(row.constant)))],
                        (name) => [[row.enumeration, name.replaceAll('_', '').replaceAll(' ', '').toLowerCase()] as const, row] as const,
                    ),
                );
                const documented = Schema.is(Schema.Literals(Struct.keys(enumerations)));
                const codes = Array.flatMap(rows, (row) =>
                    Array.map(
                        Option.toArray(documented(row.enumeration) ? Record.get<string, number>(enumerations[row.enumeration], row.constant) : Option.none()),
                        (value) => [[row.enumeration, value] as const, row] as const,
                    ),
                );
                const lookup = {
                    exact: HashMap.fromIterable(Array.map(rows, (row) => [[row.enumeration, row.constant] as const, row] as const)),
                    folded: Array.reduce([...spellings, ...codes], HashMap.empty<readonly [string, string | number], readonly Constant[]>(), (index, [key, row]) =>
                        HashMap.modifyAt(index, key, (found) => Option.some(Array.dedupe([...Array.flatten(Option.toArray(found)), row]))),
                    ),
                };
                const owners = Array.getSomes(
                    Array.map(
                        Array.sortWith(Record.toEntries(ancestors), ([, parents]) => -parents.length, Number.Order),
                        ([name]) =>
                            Option.all({
                                constructor: Option.liftPredicate(Reflect.get(host, name), Predicate.isFunction),
                                properties: Record.get(properties, name),
                                members: Record.get<string, Readonly<Record<string, boolean>>>(members, name),
                            }),
                    ),
                );
                return { host, enumerations: registered, constants, owners, lookup };
            },
        ),
    ),
);

const constant = (table: Live, target: unknown, key: string, value: unknown): Option.Option<Constant> => {
    const field = Option.flatMap(
        Array.findFirst(table.owners, ({ constructor }) => target instanceof constructor),
        (owner) => Record.get(owner.properties, key),
    );
    const candidates = Array.flatten(
        Option.toArray(
            Option.map(
                Option.filter(field, (property) => !Array.contains(property.types, typeof value)),
                Struct.get('enumerations'),
            ),
        ),
    );
    const text = Option.liftPredicate(value, Predicate.isString);
    const token = Option.orElse(
        Option.map(text, (name) => name.replaceAll('_', '').replaceAll(' ', '').toLowerCase()),
        () => Option.liftPredicate(value, Schema.is(Schema.Int)),
    );
    const exact = Array.flatMap(candidates, (enumeration) => Option.toArray(Option.flatMap(text, (name) => HashMap.get(table.lookup.exact, [enumeration, name]))));
    const found = Array.isReadonlyArrayNonEmpty(exact)
        ? exact
        : Array.flatMap(candidates, (enumeration) => Array.flatten(Option.toArray(Option.flatMap(token, (input) => HashMap.get(table.lookup.folded, [enumeration, input])))));
    return Option.flatMap(
        Option.liftPredicate(Array.dedupe(found), (rows) => rows.length === 1),
        Array.head,
    );
};

// --- [LOCALIZATION] --------------------------------------------------------------------

const translate = (key: string): Effect.Effect<string, HostRejection> =>
    Effect.flatMap(
        Effect.try({ try: (): unknown => app.doScript('app.translateKeyString(arguments[0]);', ScriptLanguage.JAVASCRIPT, [key]), catch: thrown }),
        flow(
            Schema.decodeUnknownEffect(Schema.String),
            Effect.mapError((cause) => HostRejection.cases.resultNotJson.make({ cause })),
        ),
    );

// --- [SCOPES] --------------------------------------------------------------------------

const fileFor = (path: AbsolutePath, create: boolean): Effect.Effect<File, HostRejection> =>
    Effect.tryPromise({
        try: () => (create ? storage.localFileSystem.createEntryWithUrl(`file:${path}`, { type: storage.types.file, overwrite: true }) : storage.localFileSystem.getEntryWithUrl(`file:${path}`)),
        catch: thrown,
    }).pipe(
        Effect.filterOrFail(
            (entry): entry is File => entry.isFile === true,
            () => HostRejection.cases.malformedParams.make({ cause: new TypeError(`Expected a file at ${path}`) }),
        ),
    );

const close: (resource: Document | Book | Library) => Effect.Effect<void, HostRejection> = Effect.fnUntraced(function* (resource: Document | Book | Library) {
    const closed = yield* Deferred.make<void>();
    const events = yield* resource instanceof Document
        ? Effect.succeed(app)
        : Effect.acquireRelease(Effect.try({ try: () => app.idleTasks.add({ sleep: 1 }), catch: thrown }), (task) => Effect.sync(() => task.remove()));
    yield* Effect.acquireUseRelease(
        Effect.try({
            try: () =>
                events.addEventListener(resource instanceof Document ? DocumentEvent.AFTER_CLOSE : IdleTask.ON_IDLE, () => {
                    if (!(resource instanceof Document && resource.isValid)) {
                        Deferred.doneUnsafe(closed, Effect.void);
                    }
                }),
            catch: thrown,
        }),
        () => Effect.andThen(Effect.try({ try: () => (resource instanceof Library ? resource.close() : resource.close(SaveOptions.NO)), catch: thrown }), Deferred.await(closed)),
        (listener) => Effect.try({ try: () => listener.remove(), catch: thrown }),
    );
    if (resource.isValid) {
        return yield* Effect.fail(thrown(new Error('Native resource remains open after its close completion event')));
    }
}, Effect.scoped);

const undoable = <A>(name: string, result: Schema.Codec<A, unknown, never, never>, body: () => A): Effect.Effect<A, HostRejection> => {
    const codec = Schema.fromJsonString(Schema.toCodecJson(result));
    return Effect.flatMap(
        Effect.sync((): unknown => app.doScript(() => Schema.encodeSync(codec)(body()), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name)),
        flow(
            Schema.decodeUnknownEffect(codec),
            Effect.mapError((cause) => HostRejection.cases.resultNotJson.make({ cause })),
        ),
    );
};

const swapped = <T extends object>(target: T, values: Partial<T>): Effect.Effect<void, never, Scope.Scope> =>
    Effect.acquireRelease(
        Effect.sync(() => Record.fromIterableWith(Struct.keys(values), (key) => [key, target[key]])),
        (saved) =>
            Effect.sync(() => {
                Object.assign(target, saved);
            }),
    ).pipe(
        Effect.andThen(
            Effect.sync(() => {
                Object.assign(target, values);
            }),
        ),
    );

const scripting = <A, E>(work: Effect.Effect<A, E>): Effect.Effect<A, E> =>
    Effect.scoped(Effect.andThen(swapped(app.scriptPreferences, { userInteractionLevel: UserInteractionLevels.NEVER_INTERACT, measurementUnit: MeasurementUnits.POINTS }), work));

const active = (): Option.Option<Document> =>
    Option.map(
        Option.liftPredicate(app, (host) => host.layoutWindows.length + host.storyWindows.length > 0),
        Struct.get('activeDocument'),
    );

const documentFor: (documentId: Option.Option<number>) => Effect.Effect<Document, HostRejection> = Option.match({
    onNone: () =>
        Effect.flatMap(
            Effect.sync(active),
            Effect.fromOption(() => HostRejection.cases.noActiveDocument.make({})),
        ),
    onSome: (id) =>
        Effect.filterOrFail(
            Effect.sync(() => app.documents.itemByID(id)),
            Struct.get('isValid'),
            () => HostRejection.cases.documentNotFound.make({ documentId: id }),
        ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Live };
export { active, close, constant, documentFor, fileFor, live, scripting, swapped, translate, undoable };
