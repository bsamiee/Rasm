// --- [IMPORTS] -------------------------------------------------------------------------

import { app } from 'adobe:indesign';
import { type Render, thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection, WriteRejection } from '@rasm/creative-cloud-server/errors';
import { members, preferences } from '@rasm/creative-cloud-server/indesign';
import type { Applied, Body, Reply } from '@rasm/creative-cloud-server/indesign/jobs';
import { AbsolutePath } from '@rasm/creative-cloud-server/values';
import { Array, Effect, Equal, flow, identity, Match, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { constant, documentFor, fileFor, type Live, live, undoable } from '../host.ts';

// --- [VALUES] --------------------------------------------------------------------------

const _json: (input: unknown) => Option.Option<Schema.Json> = Schema.decodeUnknownOption(Schema.Json);
const _Entry: Schema.Struct<{ readonly nativePath: Schema.String }> = Schema.Struct({ nativePath: Schema.String });
const _Writes: Schema.$Array<Schema.Result<typeof Schema.Json, typeof WriteRejection>> = Schema.Array(Schema.Result(Schema.Json, WriteRejection));

const render: Render = (value) =>
    Match.value(value).pipe(
        Match.when(Array.isArray, (values) => Option.all(Array.map(values, render))),
        Match.when({ constructor: (constructor: unknown): boolean => Predicate.hasProperty(constructor, 'name') && constructor.name === 'Enumerator' }, flow(String, Option.some)),
        Match.when(
            (candidate: unknown): candidate is { readonly toSpecifier: () => unknown } => Predicate.hasProperty(candidate, 'toSpecifier') && Predicate.isFunction(candidate.toSpecifier),
            (specifier) => Option.orElse(Option.liftPredicate(Reflect.get(specifier, 'name'), Predicate.isString), () => Option.liftPredicate(specifier.toSpecifier(), Predicate.isString)),
        ),
        Match.orElse(_json),
    );

// --- [PROPERTIES] ----------------------------------------------------------------------

const _readProperty: (table: Live, target: object, key: string) => Effect.Effect<Schema.Json, HostRejection> = Effect.fnUntraced(function* (table, target, key) {
    const raw = yield* Effect.tryPromise({ try: async (): Promise<unknown> => await Reflect.get(target, key), catch: thrown });
    const file = Option.filter(
        Option.flatMap(
            Array.findFirst(table.owners, ({ constructor }) => target instanceof constructor),
            (owner) => Record.get(owner.properties, key),
        ),
        ({ types }) => Array.contains(types, 'File') && !Array.contains(types, typeof raw),
    );
    const value = yield* Option.match(file, {
        onNone: () => Effect.succeed(raw),
        onSome: ({ list }) =>
            Effect.tryPromise({
                try: async () =>
                    list
                        ? Array.map(Schema.decodeUnknownSync(Schema.Array(_Entry))(await Promise.all(Schema.decodeUnknownSync(Schema.Array(Schema.Unknown))(raw))), Struct.get('nativePath'))
                        : Schema.decodeUnknownSync(_Entry)(raw).nativePath,
                catch: (cause) => HostRejection.cases.resultNotJson.make({ cause }),
            }),
    });
    const rendered = yield* Effect.try({ try: () => render(value), catch: thrown });
    return yield* Effect.fromOption(rendered, () => HostRejection.cases.resultNotJson.make({ cause: value }));
});

const readProperties = (table: Live, target: object, fields: Readonly<Record<string, boolean>>): Effect.Effect<Record<string, Result.Result<Schema.Json, HostRejection>>> =>
    Effect.map(
        Effect.forEach(Record.keys(Record.filter(fields, (_, key) => !Record.has<string, boolean>(members.Preference, key))), (key) =>
            Effect.map(Effect.result(_readProperty(table, target, key)), (result) => [key, result] as const),
        ),
        Record.fromEntries,
    );

const writeProperty = (target: object, key: string, assigned: unknown, intended: Schema.Json, types: readonly string[]): Result.Result<Schema.Json, WriteRejection> =>
    Result.try({
        try: () => {
            if (!Reflect.set(target, key, assigned)) {
                return Result.fail(WriteRejection.cases.readOnly.make({}));
            }
            const current: unknown = Array.contains(types, 'File') && !Array.contains(types, typeof intended) ? intended : Reflect.get(target, key);
            return Result.succeed(
                (Predicate.isString(current) || Predicate.isNumber(current) || Predicate.isBoolean(current)) &&
                    typeof current !== typeof intended &&
                    Array.contains(types, typeof intended) &&
                    Array.contains(types, typeof current)
                    ? current
                    : intended,
            );
        },
        catch: (cause) => WriteRejection.cases.threw.make({ cause }),
    }).pipe(Result.flatMap(identity));

const writeProperties: (
    table: Live,
    values: readonly { readonly target: object; readonly key: string; readonly value: Schema.Json }[],
    transaction: Option.Option<string>,
) => Effect.Effect<readonly Result.Result<{ readonly from: Schema.Json; readonly to: Schema.Json }, WriteRejection>[], HostRejection> = Effect.fnUntraced(function* (table, values, transaction) {
    const prepared = yield* Effect.all(
        Array.map(
            values,
            Effect.fnUntraced(function* ({ target, key, value }) {
                const { writable, property } = yield* Effect.fromOption(
                    Option.flatMap(
                        Array.findFirst(table.owners, ({ constructor }) => target instanceof constructor),
                        (owner) => Option.all({ writable: Record.get(owner.members, key), property: Record.get(owner.properties, key) }),
                    ),
                    () => WriteRejection.cases.unknownKey.make({}),
                );
                if (!writable || Record.has<string, boolean>(members.Preference, key)) {
                    return yield* Effect.fail(WriteRejection.cases.readOnly.make({}));
                }
                const from = yield* Effect.mapError(_readProperty(table, target, key), (cause) => WriteRejection.cases.threw.make({ cause }));
                const fileProperty = Option.liftPredicate(property, ({ types }) => Array.contains(types, 'File') && !Array.contains(types, typeof value));
                const conversion: Effect.Effect<{ readonly assigned: unknown; readonly intended: Schema.Json }, Schema.SchemaError | HostRejection> = Option.match(fileProperty, {
                    onNone: () =>
                        Effect.succeed(
                            Option.match(constant(table, target, key, value), {
                                onNone: () => ({ assigned: value, intended: value }),
                                onSome: ({ constant: name, enumerator }) => ({ assigned: enumerator, intended: name }),
                            }),
                        ),
                    onSome: ({ list }) =>
                        list
                            ? Schema.decodeUnknownEffect(Schema.Array(AbsolutePath))(value).pipe(
                                  Effect.flatMap(
                                      flow(
                                          Array.map((path) => fileFor(path, true)),
                                          Effect.all,
                                      ),
                                  ),
                                  Effect.map((files) => ({ assigned: files, intended: Array.map(files, Struct.get('nativePath')) })),
                              )
                            : Schema.decodeUnknownEffect(AbsolutePath)(value).pipe(
                                  Effect.flatMap((path) => fileFor(path, true)),
                                  Effect.map((file) => ({ assigned: file, intended: file.nativePath })),
                              ),
                });
                const converted = yield* Effect.mapError(conversion, (cause) => WriteRejection.cases.threw.make({ cause }));
                return {
                    target,
                    key,
                    from,
                    write: (): Result.Result<Schema.Json, WriteRejection> => writeProperty(target, key, converted.assigned, converted.intended, property.types),
                };
            }),
        ),
        { mode: 'result' },
    );
    const commit = (): (typeof _Writes)['Type'] =>
        Array.map(
            prepared,
            Result.flatMap(({ write }) => write()),
        );
    const committed = yield* Option.match(transaction, { onNone: () => Effect.sync(commit), onSome: (name) => undoable(name, _Writes, commit) });
    return yield* Effect.all(
        Array.map(
            Array.zip(prepared, committed),
            Effect.fnUntraced(function* ([staged, written]) {
                const { target, key, from } = yield* Effect.fromResult(staged);
                const intended = yield* Effect.fromResult(written);
                const to = yield* _readProperty(table, target, key).pipe(
                    Effect.mapError((cause) => WriteRejection.cases.threw.make({ cause })),
                    Effect.filterOrFail(Equal.equals(intended), () => WriteRejection.cases.unchanged.make({})),
                );
                return { from, to };
            }),
        ),
        { mode: 'result' },
    );
});

const _applied = (paths: readonly string[], outcomes: readonly Result.Result<{ readonly from: Schema.Json; readonly to: Schema.Json }, WriteRejection>[]): (typeof Applied)['Type'] => {
    const [rejected, applied] = Array.partition(Array.zip(paths, outcomes), ([path, outcome]) =>
        Result.mapBoth(outcome, { onSuccess: (changed) => ({ path, ...changed }), onFailure: (reason) => ({ path, reason }) }),
    );
    return { kind: 'applied', applied, rejected };
};

// --- [HANDLERS] ------------------------------------------------------------------------

const getPreferences: (body: Body<'getPreferences'>) => Effect.Effect<Reply<'getPreferences'>> = Effect.fnUntraced(function* ({ sections }) {
    const table = yield* live;
    const rows = yield* Effect.forEach(sections, (section) => Effect.map(readProperties(table, app[section], members[preferences[section]]), (values) => ({ section, values })));
    return {
        kind: 'preferences',
        values: Record.fromIterableWith(rows, ({ section, values }) => [section, Record.getSuccesses(values)]),
        unreadable: Array.flatMap(rows, ({ section, values }) => Array.map(Record.toEntries(Record.getFailures(values)), ([key, cause]) => ({ path: `${section}.${key}`, cause }))),
    };
});

const setPreferences: (body: Body<'setPreferences'>) => Effect.Effect<Reply<'setPreferences'>, HostRejection> = Effect.fnUntraced(function* ({ values }: Body<'setPreferences'>) {
    const open = app.documents.length;
    if (open > 0) {
        return { kind: 'rejected', reason: { _tag: 'documentOpen', count: open } } as const;
    }
    const table = yield* live;
    return _applied(
        Array.map(values, ({ section, key }) => `${section}.${key}`),
        yield* writeProperties(
            table,
            Array.map(values, ({ section, key, value }) => ({ target: app[section], key, value })),
            Option.some('set_preferences'),
        ),
    );
});

const setTextDefaults: (body: Body<'setTextDefaults'>) => Effect.Effect<Reply<'setTextDefaults'>, HostRejection> = Effect.fnUntraced(function* ({ scope, values }: Body<'setTextDefaults'>) {
    const [table, slots] = yield* Effect.all([
        live,
        Match.value(scope).pipe(
            Match.when('application', () => {
                const style = app.textDefaults.appliedParagraphStyle;
                return Effect.succeed([
                    { target: app.textDefaults, label: 'textDefaults' },
                    { target: style, label: style.name },
                ]);
            }),
            Match.when('document', () => Effect.map(documentFor(Option.none()), (open) => [{ target: open.textDefaults, label: 'textDefaults' }])),
            Match.exhaustive,
        ),
    ]);
    const assignments = Array.cartesianWith(slots, values, (slot, row) => ({ ...slot, ...row }));
    return _applied(
        Array.map(assignments, ({ label, key }) => `${label}.${key}`),
        yield* writeProperties(table, assignments, Option.some('set_text_defaults')),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { getPreferences, readProperties, render, setPreferences, setTextDefaults, writeProperties, writeProperty };
