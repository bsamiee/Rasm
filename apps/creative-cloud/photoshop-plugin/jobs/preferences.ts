// --- [IMPORTS] -------------------------------------------------------------------------

import { app } from 'adobe:photoshop';
import { type Handler, handler } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { Applied, GetPreferences, Preferences, type Rejection, SetPreferences } from '@rasm/creative-cloud-server/photoshop/jobs';
import { Array, Effect, Equal, identity, Option, Predicate, Record, Result, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Section = (typeof GetPreferences)['Type']['sections'][number];

type Write = (typeof SetPreferences)['Type']['values'][number];

type Row = (typeof Applied)['Type']['applied'][number];

type Refusal = (typeof Applied)['Type']['rejected'][number];

// --- [VALUES] --------------------------------------------------------------------------

const _json: (input: unknown) => Option.Option<Schema.Json> = Schema.decodeUnknownOption(Schema.Json);

const _value = (target: object, key: string): Schema.Json => Option.getOrNull(_json(Reflect.get(target, key)));

const _keys = (target: object): readonly string[] => {
    const prototype: object = Object.getPrototypeOf(target);
    return Array.filter(
        Object.getOwnPropertyNames(prototype),
        (name) => name !== 'typename' && Option.exists(Option.fromNullishOr(Object.getOwnPropertyDescriptor(prototype, name)), (descriptor) => Predicate.isFunction(descriptor.get)),
    );
};

const _section = (section: Section): { readonly section: Section; readonly values: Readonly<Record<string, Schema.Json>>; readonly unreadable: (typeof Preferences)['Type']['unreadable'] } => {
    const target = app.preferences[section];
    const [unreadable, values] = Array.separate(
        Array.map(_keys(target), (key) =>
            Result.mapBoth(
                Result.try(() => _value(target, key)),
                {
                    onSuccess: (value) => [key, value] as const,
                    onFailure: (cause) => ({ section, key, cause }),
                },
            ),
        ),
    );
    return { section, values: Record.fromEntries(values), unreadable };
};

const _write = ({ section, key, value }: Write): Effect.Effect<Result.Result<Row, Refusal>, HostRejection> => {
    const target = app.preferences[section];
    const refused = (reason: Rejection): Result.Result<Row, Refusal> => Result.fail({ section, key, reason });
    if (!Reflect.has(target, key)) {
        return Effect.succeed(refused({ _tag: 'unknownKey' }));
    }
    const from = _value(target, key);
    return Effect.try({ try: () => Reflect.set(target, key, value), catch: identity }).pipe(
        Effect.matchEffect({
            onFailure: (cause) =>
                section === 'notifications' && app.preferences.notifications.quietMode
                    ? Effect.fail(HostRejection.cases.preferenceLocked.make({ section, key }))
                    : Effect.succeed(refused({ _tag: 'threw', cause })),
            onSuccess: () => {
                const to = _value(target, key);
                return Effect.succeed(Equal.equals(to, value) || !Equal.equals(to, from) ? Result.succeed({ section, key, from, to }) : refused({ _tag: 'unchanged' }));
            },
        }),
    );
};

// --- [HANDLERS] ------------------------------------------------------------------------

const getPreferences: Handler = handler(GetPreferences, Preferences, ({ sections }) =>
    Effect.sync(() => {
        const reads = Array.map(sections, _section);
        return { kind: 'preferences' as const, values: Record.fromIterableWith(reads, ({ section, values }) => [section, values]), unreadable: Array.flatMap(reads, Struct.get('unreadable')) };
    }),
);

const setPreferences: Handler = handler(SetPreferences, Applied, ({ values }) =>
    Effect.map(Effect.forEach(values, _write), (outcomes) => {
        const [rejected, applied] = Array.separate(outcomes);
        return { kind: 'applied' as const, applied, rejected };
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { getPreferences, setPreferences };
