// --- [IMPORTS] -------------------------------------------------------------------------

import { app, type Event } from 'adobe:indesign';
import { host, versions } from 'adobe:uxp';
import { active, type Client, type Handler, handle, handler, type Settled, settle } from '@rasm/creative-cloud-server/client';
import type { Identity, Job, State } from '@rasm/creative-cloud-server/frames';
import type { members } from '@rasm/creative-cloud-server/indesign';
import { Bodies, type Kind, Results } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, type Effect, Option, Stream, Struct } from 'effect';
import { scripting } from './host.ts';
import { execute } from './jobs/execute.ts';
import { findKeyStrings } from './jobs/find-key-strings.ts';
import { getLayout } from './jobs/get-layout.ts';
import { listEnums } from './jobs/list-enums.ts';
import { getPreferences, setPreferences, setTextDefaults } from './jobs/preferences.ts';
import { snapshot } from './jobs/snapshot.ts';
import { bridge } from './uxp.config.ts';

// --- [JOBS] ----------------------------------------------------------------------------

const _jobs: { readonly [K in Exclude<Kind, 'execute'>]: Handler } = {
    listEnums: handler(Bodies.fields.listEnums, Results.fields.listEnums, listEnums),
    snapshot: handler(Bodies.fields.snapshot, Results.fields.snapshot, snapshot),
    getLayout: handler(Bodies.fields.getLayout, Results.fields.getLayout, getLayout),
    findKeyStrings: handler(Bodies.fields.findKeyStrings, Results.fields.findKeyStrings, findKeyStrings),
    getPreferences: handler(Bodies.fields.getPreferences, Results.fields.getPreferences, getPreferences),
    setPreferences: handler(Bodies.fields.setPreferences, Results.fields.setPreferences, setPreferences),
    setTextDefaults: handler(Bodies.fields.setTextDefaults, Results.fields.setTextDefaults, setTextDefaults),
};

// --- [HOST] ----------------------------------------------------------------------------

const _identity = (): Identity => ({
    plugin: bridge.manifest.id,
    version: versions.plugin,
    host: { name: host.name, version: host.version },
    uxp: versions.uxp,
    app: Option.some(app.version),
    dom: Option.some(app.scriptPreferences.version),
});

const _states: Stream.Stream<State> = Stream.mergeAll(
    Array.map(
        ['afterContextChanged', 'afterSelectionChanged', 'afterSelectionAttributeChanged', 'afterClose', 'afterActivate', 'afterOpen', 'afterNew', 'afterSave'] satisfies readonly (
            | keyof (typeof members)['Application']
            | keyof (typeof members)['Document']
        )[],
        (type) => Stream.fromEventListener<Event>(app, type),
    ),
    { concurrency: 'unbounded' },
).pipe(
    Stream.map(() => active(app)),
    Stream.map((open) => ({ modalState: app.modalState, activeDocumentId: Option.map(open, Struct.get('id')), modified: Option.exists(open, Struct.get('modified')) })),
);

const _perform = (job: Job): Effect.Effect<Settled> => (job.kind === 'execute' ? execute(job.body) : settle(scripting(handle(_jobs, job))));

// --- [CLIENT] --------------------------------------------------------------------------

const client: Client = { endpoint: bridge.endpoint, version: bridge.manifest.version, identity: _identity, states: _states, perform: _perform };

// --- [EXPORTS] -------------------------------------------------------------------------

export { client };
