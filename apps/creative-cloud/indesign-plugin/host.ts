// --- [IMPORTS] -------------------------------------------------------------------------

import { app, MeasurementUnits, ScriptLanguage, UndoModes, UserInteractionLevels } from 'adobe:indesign';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { enumerations, phrases, properties } from '@rasm/creative-cloud-server/indesign';
import { Array, Effect, flow, Option, Predicate, Record, Schema, type Scope, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Hosted = typeof import('adobe:indesign');

type Live = Readonly<Record<string, Readonly<Record<string, unknown>>>>;

// --- [ENUMERATIONS] --------------------------------------------------------------------

const hosted: Effect.Effect<Hosted> = Effect.runSync(Effect.cached(Effect.promise((): Promise<Hosted> => import('adobe:indesign'))));

const registered: Effect.Effect<Readonly<Record<string, object>>> = Effect.map(hosted, (host) => Struct.pick<Readonly<Record<string, object>>, readonly string[]>(host, Record.keys(enumerations)));

const live: Effect.Effect<Live> = Effect.map(
    registered,
    Record.map((enumeration) =>
        Record.fromIterableWith(
            Array.filter(Object.getOwnPropertyNames(Object.getPrototypeOf(enumeration)), (spelling) => spelling === spelling.toUpperCase()),
            (spelling) => [spelling, Reflect.get(enumeration, spelling)],
        ),
    ),
);

const constant = (table: Live, key: string, value: unknown): Option.Option<{ readonly enumeration: string; readonly constant: string; readonly enumerator: unknown }> => {
    const folded = (spelling: string): string => spelling.replaceAll('_', '').replaceAll(' ', '').toLowerCase();
    const rows = Array.flatMap(Option.match(Record.get(properties, key), { onNone: () => [], onSome: Struct.get('enumerations') }), (enumeration) =>
        Array.map(Option.match(Record.get(table, enumeration), { onNone: () => [], onSome: Record.toEntries }), ([name, enumerator]) => ({ enumeration, constant: name, enumerator })),
    );
    const one = (accepts: Predicate.Predicate<(typeof rows)[number]>): Option.Option<(typeof rows)[number]> =>
        Option.flatMap(
            Option.liftPredicate(Array.filter(rows, accepts), (found) => found.length === 1),
            Array.head,
        );
    const text = Option.liftPredicate(value, Predicate.isString);
    const code = Option.liftPredicate(value, Schema.is(Schema.Int));
    return Option.firstSomeOf([
        Option.flatMap(text, (wanted) => one((row) => row.constant === wanted)),
        Option.flatMap(text, (wanted) =>
            one((row) => Array.contains(Array.map([row.constant, ...Option.toArray(Option.flatMap(Record.get(phrases, row.enumeration), Record.get(row.constant)))], folded), folded(wanted))),
        ),
        Option.flatMap(code, (wanted) => one((row) => Option.contains(Option.flatMap(Record.get(enumerations, row.enumeration), Record.get(row.constant)), wanted))),
    ]);
};

// --- [SCOPES] --------------------------------------------------------------------------

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
    Effect.asVoid(
        Effect.acquireRelease(
            Effect.sync(() => {
                const saved = Struct.pick(target, Struct.keys(values));
                Object.assign(target, values);
                return saved;
            }),
            (saved) =>
                Effect.sync(() => {
                    Object.assign(target, saved);
                }),
        ),
    );

const scripting = <A, E>(work: Effect.Effect<A, E>): Effect.Effect<A, E> =>
    Effect.scoped(Effect.andThen(swapped(app.scriptPreferences, { userInteractionLevel: UserInteractionLevels.NEVER_INTERACT, measurementUnit: MeasurementUnits.POINTS }), work));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Live };
export { constant, hosted, live, registered, scripting, swapped, undoable };
