// --- [IMPORTS] -------------------------------------------------------------------------

import { app, type Event, MeasurementUnits, UserInteractionLevels } from 'adobe:indesign';
import { host, versions } from 'adobe:uxp';
import type { Client, Settled } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import type { Identity, Job, State } from '@rasm/creative-cloud-server/frames';
import { Kind } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, Option, Result, Schema, Stream, Struct } from 'effect';
import { type Live, live } from './enums.ts';
import { execute } from './jobs/execute.ts';
import { findKeyStrings } from './jobs/find-key-strings.ts';
import { getLayout } from './jobs/get-layout.ts';
import type { Handler } from './jobs/handler.ts';
import { listEnums } from './jobs/list-enums.ts';
import { getPreferences, setPreferences, setTextDefaults } from './jobs/preferences.ts';
import { snapshot } from './jobs/snapshot.ts';
import { endpoint, manifest } from './uxp.config.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _EVENTS = ['afterContextChanged', 'afterSelectionChanged', 'afterOpen', 'afterClose', 'afterNew', 'afterSave'];

// --- [HOST] ----------------------------------------------------------------------------

const _handlers = (table: Live): { readonly [K in Kind]: Handler } => ({
    execute: execute(table),
    listEnums: listEnums(table),
    snapshot,
    getLayout,
    findKeyStrings,
    getPreferences,
    setPreferences: setPreferences(table),
    setTextDefaults: setTextDefaults(table),
});

const _identity = (): Identity => ({
    plugin: manifest.id,
    version: versions.plugin,
    host: { name: host.name, version: host.version },
    uxp: versions.uxp,
    app: Option.some(app.version),
    dom: Option.some(app.scriptPreferences.version),
});

const _state = (): State => {
    const active = app.documents.length === 0 ? Option.none() : Option.some(app.activeDocument);
    return { modalState: app.modalState, activeDocumentId: Option.map(active, Struct.get('id')), modified: Option.exists(active, Struct.get('modified')) };
};

const _states: Stream.Stream<State> = Stream.map(
    Stream.mergeAll(
        Array.map(_EVENTS, (type) => Stream.fromEventListener<Event>(app, type)),
        { concurrency: 'unbounded' },
    ),
    _state,
);

const _executor = Effect.acquireRelease(
    Effect.sync(() => {
        const preferences = app.scriptPreferences;
        const saved = { userInteractionLevel: preferences.userInteractionLevel, measurementUnit: preferences.measurementUnit };
        preferences.userInteractionLevel = UserInteractionLevels.NEVER_INTERACT;
        preferences.measurementUnit = MeasurementUnits.POINTS;
        return saved;
    }),
    (saved) =>
        Effect.sync(() => {
            app.scriptPreferences.userInteractionLevel = saved.userInteractionLevel;
            app.scriptPreferences.measurementUnit = saved.measurementUnit;
        }),
);

const _known: (kind: unknown) => Option.Option<Kind> = Schema.decodeUnknownOption(Kind);

const _perform = Effect.fnUntraced(function* (table: { readonly [K in Kind]: Handler }, job: Job) {
    const kind = _known(job.kind);
    if (Option.isNone(kind)) {
        return { autocorrections: Option.none(), result: Result.fail(HostRejection.cases.unknownMethod.make({ method: job.kind })) } satisfies Settled;
    }
    yield* _executor;
    return yield* table[kind.value](job);
}, Effect.scoped);

// --- [LINK] ----------------------------------------------------------------------------

const client = (dom: object): Client => {
    const table = _handlers(live(dom));
    return { endpoint, version: manifest.version, identity: _identity, states: _states, perform: (job) => _perform(table, job) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { client };
