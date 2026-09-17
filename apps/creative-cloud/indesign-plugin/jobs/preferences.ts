// --- [IMPORTS] -------------------------------------------------------------------------

import { app, ScriptLanguage, UndoModes } from 'adobe:indesign';
import { type Handler, handler, thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { members, preferences, properties } from '@rasm/creative-cloud-server/indesign';
import { Applied, GetPreferences, Preferences, type Rejection, SetPreferences, SetTextDefaults, Settings } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, Equal, flow, identity, Match, Option, Predicate, pipe, Record, Result, Schema, Struct } from 'effect';
import { document } from '../document.ts';
import { type Constant, coded, type Live, named } from '../enums.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Row = (typeof Applied)['Type']['applied'][number];

type Refusal = (typeof Applied)['Type']['rejected'][number];

type Section = (typeof GetPreferences)['Type']['sections'][number];

interface Slot {
    readonly target: object;
    readonly klass: string;
    readonly label: string;
}

interface Write {
    readonly slot: Slot;
    readonly key: string;
    readonly value: Schema.Json;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _MEMBERS: Readonly<Record<string, Readonly<Record<string, boolean>>>> = members;

const _BASIC_PARAGRAPH = '[Basic Paragraph]';

// --- [VALUES] --------------------------------------------------------------------------

const _json: (input: unknown) => Option.Option<Schema.Json> = Schema.decodeUnknownOption(Schema.Json);

const _enumerator = (value: unknown): value is object => Predicate.isObject(value) && value.constructor.name === 'Enumerator';

const _specifier = (value: unknown): value is { readonly toSpecifier: () => string } =>
    Predicate.isObject(value) && Predicate.hasProperty(value, 'toSpecifier') && Predicate.isFunction(value.toSpecifier);

const _rendered = (value: unknown): Schema.Json =>
    Match.value(value).pipe(
        Match.when(Array.isArray, Array.map(_rendered)),
        Match.when(_enumerator, String),
        Match.when(_specifier, (object) => Option.getOrElse(Option.filter(_json(Reflect.get(object, 'name')), Predicate.isString), () => object.toSpecifier())),
        Match.orElse(flow(_json, Option.getOrNull)),
    );

const _constant = (live: Live, candidates: readonly string[], value: Schema.Json): Option.Option<Constant> =>
    Option.orElse(
        Option.flatMap(Option.liftPredicate(value, Predicate.isString), (text) => named(live, candidates, text)),
        () => Option.flatMap(Option.liftPredicate(value, Predicate.isNumber), (code) => coded(live, candidates, code)),
    );

const _resolved = (live: Live, key: string, value: Schema.Json): { readonly assigned: unknown; readonly intended: Schema.Json } =>
    Option.match(_constant(live, Option.match(Record.get(properties, key), { onNone: () => [], onSome: Struct.get('enumerations') }), value), {
        onNone: () => ({ assigned: value, intended: value }),
        onSome: ({ enumeration, constant }) => ({ assigned: Option.getOrNull(Option.flatMap(Record.get(live, enumeration), Record.get(constant))), intended: constant }),
    });

const _assigned = (live: Live, { slot, key, value }: Write): Result.Result<Pick<Row, 'from' | 'to'>, Rejection> => {
    const from = _rendered(Reflect.get(slot.target, key));
    const { assigned, intended } = _resolved(live, key, value);
    return Result.flatMap(Result.try({ try: () => Reflect.set(slot.target, key, assigned), catch: (cause): Rejection => ({ _tag: 'threw', cause }) }), () => {
        const to = _rendered(Reflect.get(slot.target, key));
        return Equal.equals(to, intended) || !Equal.equals(to, from) ? Result.succeed({ from, to }) : Result.fail({ _tag: 'unchanged' });
    });
};

const _write = (live: Live, write: Write): Result.Result<Row, Refusal> => {
    const path = `${write.slot.label}.${write.key}`;
    return pipe(
        Record.get(_MEMBERS, write.slot.klass),
        Option.flatMap(Record.get(write.key)),
        Result.fromOption((): Rejection => ({ _tag: 'unknownKey' })),
        Result.filterOrFail(identity, (): Rejection => ({ _tag: 'readOnly' })),
        Result.flatMap(() => _assigned(live, write)),
        Result.mapBoth({ onSuccess: (changed) => ({ path, ...changed }), onFailure: (reason) => ({ path, reason }) }),
    );
};

const _applied = (live: Live, writes: readonly Write[]): (typeof Applied)['Type'] => {
    const [rejected, applied] = Array.separate(Array.map(writes, (write) => _write(live, write)));
    return { kind: 'applied', applied, rejected };
};

const _undoable = <A>(name: string, result: Schema.Codec<A, unknown, never, never>, body: () => A): Effect.Effect<A, HostRejection> => {
    const codec = Schema.toCodecJson(result);
    return Effect.flatMap(
        Effect.try({
            try: (): unknown => app.doScript(() => JSON.stringify(Schema.encodeSync(codec)(body())), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name),
            catch: thrown,
        }),
        flow(
            Schema.decodeUnknownEffect(Schema.fromJsonString(codec)),
            Effect.mapError((cause) => HostRejection.cases.resultNotJson.make({ cause })),
        ),
    );
};

const _section = (section: Section): { readonly section: Section; readonly values: Readonly<Record<string, Schema.Json>>; readonly unreadable: (typeof Preferences)['Type']['unreadable'] } => {
    const target = app[section];
    const [unreadable, values] = Array.separate(
        Array.map(Record.keys(Option.getOrElse(Record.get(_MEMBERS, preferences[section]), () => ({}))), (key) =>
            Result.mapBoth(
                Result.try(() => _rendered(Reflect.get(target, key))),
                {
                    onSuccess: (value) => [key, value] as const,
                    onFailure: (cause) => ({ path: `${section}.${key}`, cause }),
                },
            ),
        ),
    );
    return { section, values: Record.fromEntries(values), unreadable };
};

// --- [HANDLERS] ------------------------------------------------------------------------

const getPreferences: Handler = handler(GetPreferences, Preferences, ({ sections }) =>
    Effect.try({
        try: () => {
            const reads = Array.map(sections, _section);
            return { kind: 'preferences' as const, values: Record.fromIterableWith(reads, ({ section, values }) => [section, values]), unreadable: Array.flatMap(reads, Struct.get('unreadable')) };
        },
        catch: thrown,
    }),
);

const setPreferences = (live: Live): Handler =>
    handler(SetPreferences, Settings, ({ values }) =>
        app.documents.length > 0
            ? Effect.succeed({ kind: 'rejected' as const, reason: { _tag: 'documentOpen' as const, count: app.documents.length } })
            : _undoable('set_preferences', Applied, () =>
                  _applied(
                      live,
                      Array.map(values, ({ section, key, value }) => ({ slot: { target: app[section], klass: preferences[section], label: section }, key, value })),
                  ),
              ),
    );

const setTextDefaults = (live: Live): Handler =>
    handler(SetTextDefaults, Applied, ({ scope, values }) =>
        Match.value(scope).pipe(
            Match.when('application', () =>
                Effect.succeed<readonly Slot[]>([
                    { target: app.textDefaults, klass: 'TextDefault', label: 'textDefaults' },
                    { target: app.paragraphStyles.itemByName(_BASIC_PARAGRAPH), klass: 'ParagraphStyle', label: _BASIC_PARAGRAPH },
                ]),
            ),
            Match.when('document', () => Effect.map(document, (doc): readonly Slot[] => [{ target: doc.textDefaults, klass: 'TextDefault', label: 'textDefaults' }])),
            Match.exhaustive,
            Effect.map((slots) => Array.cartesianWith(slots, values, (slot, { key, value }): Write => ({ slot, key, value }))),
            Effect.flatMap((writes) => _undoable('set_text_defaults', Applied, () => _applied(live, writes))),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { getPreferences, setPreferences, setTextDefaults };
