import fs from 'adobe:fs';
import indesign from 'adobe:indesign';
import uxp from 'adobe:uxp';
import type { Unmatched } from '@rasm/search';
import { Array, type Cause, Effect, Layer, Option, Record, Schema, Struct } from 'effect';
import { Rpc, RpcMiddleware } from 'effect/rpc';
import { FontView } from '../fonts.ts';
import { Host } from '../host.ts';
import { Classes } from '../objects/classes.ts';
import { active, read } from '../objects/specifiers.ts';
import { Handoff, Mutation } from '../objects/transactions.ts';
import { preserve } from '../objects/values.ts';
import { Preflight, Reports } from '../preflight.ts';
import { Query } from '../search.ts';
import { MenuAction } from '../session.ts';
import { Complete, Enter, InDesign } from './contract.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class Hold extends RpcMiddleware.Service<Hold>()('@rasm/indesign/service/panel/Hold', { error: Schema.ErrorInstance() }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const group = InDesign.omit(Enter._tag).middleware(Hold).add(Enter);
const identity = Effect.map(
    Effect.tryPromise(() => indesign.app.fullName),
    (entry) => ({ application: String(entry.url) }),
);
const hold = Layer.effect(Hold)(
    Effect.map(
        Effect.acquireRelease(
            Effect.sync(() => document.body.appendChild(document.createElement('sp-label'))),
            (element) => Effect.sync(() => element.remove()),
        ),
        (shown) =>
            (effect, { payload, rpc }) => {
                const { app, MeasurementUnits, UserInteractionLevels } = indesign;
                const label = Schema.is(Mutation)(payload) ? payload.label : rpc._tag;
                return Effect.andThen(
                    Effect.all([
                        preserve(app.scriptPreferences, { userInteractionLevel: UserInteractionLevels.NEVER_INTERACT, enableRedraw: false, measurementUnit: MeasurementUnits.POINTS }),
                        Effect.acquireRelease(
                            Effect.sync(() => shown.replaceChildren(label)),
                            () => Effect.sync(() => shown.replaceChildren()),
                        ),
                    ]),
                    effect,
                );
            },
    ),
);
const handlers = InDesign.toLayer({
    enter: () => Rpc.fork(Effect.flatMap(Handoff, ({ enter }) => enter)),
    complete: ({ operation, parameter }) => {
        const candidates: Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, Effect.Effect<readonly string[], Cause.UnknownError | Unmatched, Classes | Host>>> = {
            menu: { area: Effect.map(MenuAction.listed(Option.none()), (rows) => Array.dedupe(Array.map(rows, Struct.get('area')))) },
            preflight: { profile: read(Complete.successSchema, () => indesign.app.preflightProfiles.everyItem().name) },
            queries: { name: Effect.map(Query.listed(Option.none()), Array.map(Struct.get('name'))) },
        };
        return Option.getOrElse(Option.flatMap(Record.get(candidates, operation), Record.get(parameter)), () => Effect.succeed([]));
    },
    inspect: (payload) => payload.run(),
    change: (payload) => payload.run(),
    create: (payload) => payload.run(),
    remove: (payload) => payload.run(),
    execute_script: (payload) => payload.run(),
    find_change: (payload) => payload.run(),
    invoke: (payload) => payload.run(),
    render: (payload) => payload.run(),
    font: Effect.fnUntraced(function* ({ name, text }) {
        const font = yield* FontView.named(name);
        const [view, source] = yield* Effect.all([FontView.of(font), FontView.source(font, text)]);
        return { ...view, installed: Option.zipWith(view.installed, source, (installed, bytes) => ({ ...installed, source: bytes })) };
    }),
    menu: ({ area }) => MenuAction.listed(area),
    preflight: ({ profile }) => Effect.flatMap(active, (document) => Preflight.of(document, profile)),
    queries: ({ name }) => Query.listed(name),
}).pipe(Layer.provideMerge(hold), Layer.provide([Classes.layer, Handoff.layer, Reports.layer]), Layer.provide(Layer.succeed(Host, { indesign, fs, uxp })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { group, handlers, identity };
