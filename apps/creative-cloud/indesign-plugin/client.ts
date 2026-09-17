// --- [IMPORTS] -------------------------------------------------------------------------

import { app, type Event, MeasurementUnits, UserInteractionLevels } from 'adobe:indesign';
import { host, versions } from 'adobe:uxp';
import { active, type Client, type Handler, handle, type Settled, settle } from '@rasm/creative-cloud-server/client';
import type { Identity, Job, State } from '@rasm/creative-cloud-server/frames';
import type { Kind } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, Option, Stream, Struct } from 'effect';
import { type Live, live } from './enums.ts';
import { execute } from './jobs/execute.ts';
import { findKeyStrings } from './jobs/find-key-strings.ts';
import { getLayout } from './jobs/get-layout.ts';
import { listEnums } from './jobs/list-enums.ts';
import { getPreferences, setPreferences, setTextDefaults } from './jobs/preferences.ts';
import { snapshot } from './jobs/snapshot.ts';
import { bridge } from './uxp.config.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _EVENTS = ['afterContextChanged', 'afterSelectionChanged', 'afterOpen', 'afterClose', 'afterNew', 'afterSave'];

// --- [HOST] ----------------------------------------------------------------------------

const _handlers = (table: Live): { readonly [K in Exclude<Kind, 'execute'>]: Handler } => ({
    listEnums: listEnums(table),
    snapshot,
    getLayout,
    findKeyStrings,
    getPreferences,
    setPreferences: setPreferences(table),
    setTextDefaults: setTextDefaults(table),
});

const _identity = (): Identity => ({
    plugin: bridge.manifest.id,
    version: versions.plugin,
    host: { name: host.name, version: host.version },
    uxp: versions.uxp,
    app: Option.some(app.version),
    dom: Option.some(app.scriptPreferences.version),
});

const _state = (): State => {
    const open = active(app);
    return { modalState: app.modalState, activeDocumentId: Option.map(open, Struct.get('id')), modified: Option.exists(open, Struct.get('modified')) };
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

const _perform = (table: Live, handlers: Readonly<Record<string, Handler>>, job: Job): Effect.Effect<Settled> =>
    Effect.scoped(Effect.andThen(_executor, job.kind === 'execute' ? execute(table, job) : settle(handle(handlers, job))));

// --- [LINK] ----------------------------------------------------------------------------

const client = (dom: object): Client => {
    const table = live(dom);
    const handlers = _handlers(table);
    return { endpoint: bridge.endpoint, version: bridge.manifest.version, identity: _identity, states: _states, perform: (job) => _perform(table, handlers, job) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { client };
