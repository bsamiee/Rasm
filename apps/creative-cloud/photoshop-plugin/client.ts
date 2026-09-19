// --- [IMPORTS] -------------------------------------------------------------------------

import { action, app, core } from 'adobe:photoshop';
import { host, versions } from 'adobe:uxp';
import { active, type Client, type Handler, handle, handler, type Settled, settle } from '@rasm/creative-cloud-server/client';
import type { Identity, Job, State } from '@rasm/creative-cloud-server/frames';
import { Bodies, type Kind, Results } from '@rasm/creative-cloud-server/photoshop/jobs';
import { Effect, Option, Predicate, Queue, Stream, Struct } from 'effect';
import { applyTypeStyles, batchPlay, composeLayers, execute, getDocument, getPreferences, listPresets, modal, runAction, setPreferences, snapshot } from './jobs.ts';
import { bridge } from './uxp.config.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _EVENTS = ['open', 'close', 'select', 'save', 'make'];

// --- [JOBS] ----------------------------------------------------------------------------

const _jobs: { readonly [K in Kind]: Handler } = {
    execute: handler(Bodies.fields.execute, Results.fields.execute, execute),
    batchPlay: handler(Bodies.fields.batchPlay, Results.fields.batchPlay, batchPlay),
    snapshot: handler(Bodies.fields.snapshot, Results.fields.snapshot, snapshot),
    getDocument: handler(Bodies.fields.getDocument, Results.fields.getDocument, getDocument),
    getPreferences: handler(Bodies.fields.getPreferences, Results.fields.getPreferences, getPreferences),
    setPreferences: handler(Bodies.fields.setPreferences, Results.fields.setPreferences, setPreferences),
    listPresets: handler(Bodies.fields.listPresets, Results.fields.listPresets, listPresets),
    runAction: handler(Bodies.fields.runAction, Results.fields.runAction, runAction),
    applyTypeStyles: handler(Bodies.fields.applyTypeStyles, Results.fields.applyTypeStyles, applyTypeStyles),
    composeLayers: handler(Bodies.fields.composeLayers, Results.fields.composeLayers, composeLayers),
};

// --- [HOST] ----------------------------------------------------------------------------

const _identity = (): Identity => ({
    plugin: bridge.manifest.id,
    version: versions.plugin,
    host: { name: host.name, version: host.version },
    uxp: versions.uxp,
    app: Option.none(),
    dom: Option.none(),
});

const _states: Stream.Stream<State> = Stream.callback<string>((queue) => {
    const notifier = (name: string): boolean => Queue.offerUnsafe(queue, name);
    return Effect.acquireRelease(
        Effect.promise(() => action.addNotificationListener(_EVENTS, notifier)),
        () => Effect.promise(() => action.removeNotificationListener(_EVENTS, notifier)),
    );
}).pipe(
    Stream.map(() => active(app)),
    Stream.map((open) => ({ modalState: core.isModal(), activeDocumentId: Option.map(open, Struct.get('id')), modified: Option.exists(open, Predicate.not(Struct.get('saved'))) })),
);

const _perform = (job: Job): Effect.Effect<Settled> =>
    settle(Option.match(job.commandName, { onNone: () => handle(_jobs, job), onSome: (commandName) => modal(handle(_jobs, job), commandName, job.suspendHistory) }));

// --- [CLIENT] --------------------------------------------------------------------------

const client: Client = { endpoint: bridge.endpoint, identity: _identity, states: _states, perform: _perform };

// --- [EXPORTS] -------------------------------------------------------------------------

export { client };
