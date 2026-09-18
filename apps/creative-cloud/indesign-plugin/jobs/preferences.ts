// --- [IMPORTS] -------------------------------------------------------------------------

import { app } from 'adobe:indesign';
import { opened, type Render, read, written } from '@rasm/creative-cloud-server/client';
import type { HostRejection } from '@rasm/creative-cloud-server/errors';
import { members, preferences } from '@rasm/creative-cloud-server/indesign';
import { Applied, type Body, type Reply } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, flow, Match, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { constant, type Live, live, undoable } from '../host.ts';

// --- [VALUES] --------------------------------------------------------------------------

const _json: (input: unknown) => Option.Option<Schema.Json> = Schema.decodeUnknownOption(Schema.Json);

const _rendered = (value: unknown): Schema.Json =>
    Match.value(value).pipe(
        Match.when(Array.isArray, Array.map(_rendered)),
        Match.when((candidate: unknown): candidate is object => Predicate.isObject(candidate) && candidate.constructor.name === 'Enumerator', String),
        Match.when(
            (candidate: unknown): candidate is { readonly toSpecifier: () => unknown } => Predicate.hasProperty(candidate, 'toSpecifier') && Predicate.isFunction(candidate.toSpecifier),
            (specifier) => Option.getOrElse(Option.filter(_json(Reflect.get(specifier, 'name')), Predicate.isString), () => String(specifier.toSpecifier())),
        ),
        Match.orElse(flow(_json, Option.getOrNull)),
    );

const _render: Render = flow(_rendered, Option.some);

// --- [WRITES] --------------------------------------------------------------------------

const _write = (
    table: Live,
    { target, label }: { readonly target: object; readonly label: string },
    { key, value }: { readonly key: string; readonly value: Schema.Json },
): Result.Result<(typeof Applied)['Type']['applied'][number], (typeof Applied)['Type']['rejected'][number]> => {
    const path = `${label}.${key}`;
    const { assigned, intended } = Option.match(constant(table, key, value), {
        onNone: () => ({ assigned: value, intended: value }),
        onSome: ({ constant: name, enumerator }) => ({ assigned: enumerator, intended: name }),
    });
    return Result.mapBoth(written(_render)(target, key, assigned, intended), { onSuccess: (changed) => ({ path, ...changed }), onFailure: (reason) => ({ path, reason }) });
};

const _applied = (outcomes: Iterable<ReturnType<typeof _write>>): (typeof Applied)['Type'] => {
    const [rejected, applied] = Array.separate(outcomes);
    return { kind: 'applied', applied, rejected };
};

// --- [HANDLERS] ------------------------------------------------------------------------

const getPreferences = ({ sections }: Body<'getPreferences'>): Effect.Effect<Reply<'getPreferences'>> => {
    const [unreadable, rows] = Array.partition(
        Array.flatMap(sections, (section) => Array.map(Struct.keys(members[preferences[section]]), (key) => ({ section, key }))),
        ({ section, key }) => Result.mapBoth(read(_render)(app[section], key), { onSuccess: (value) => ({ section, key, value }), onFailure: (cause) => ({ path: `${section}.${key}`, cause }) }),
    );
    return Effect.succeed({
        kind: 'preferences',
        values: Record.fromIterableWith(sections, (section) => [
            section,
            Record.fromIterableWith(
                Array.filter(rows, (row) => row.section === section),
                ({ key, value }) => [key, value],
            ),
        ]),
        unreadable,
    });
};

const setPreferences = ({ values }: Body<'setPreferences'>): Effect.Effect<Reply<'setPreferences'>, HostRejection> => {
    const open = app.documents.length;
    return open > 0
        ? Effect.succeed({ kind: 'rejected', reason: { _tag: 'documentOpen', count: open } })
        : Effect.flatMap(live, (table) =>
              undoable('set_preferences', Applied, () => _applied(Array.map(values, ({ section, key, value }) => _write(table, { target: app[section], label: section }, { key, value })))),
          );
};

const setTextDefaults = ({ scope, values }: Body<'setTextDefaults'>): Effect.Effect<Reply<'setTextDefaults'>, HostRejection> =>
    Effect.flatMap(
        Effect.all([
            live,
            Match.value(scope).pipe(
                Match.when('application', () => {
                    const style = app.textDefaults.appliedParagraphStyle;
                    return Effect.succeed([
                        { target: app.textDefaults, label: 'textDefaults' },
                        { target: style, label: style.name },
                    ]);
                }),
                Match.when('document', () => Effect.map(opened(app), (open) => [{ target: open.textDefaults, label: 'textDefaults' }])),
                Match.exhaustive,
            ),
        ]),
        ([table, slots]) => undoable('set_text_defaults', Applied, () => _applied(Array.cartesianWith(slots, values, (slot, row) => _write(table, slot, row)))),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { getPreferences, setPreferences, setTextDefaults };
