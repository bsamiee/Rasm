// --- [IMPORTS] -------------------------------------------------------------------------

import { action, app, core, type ExecutionContext } from 'adobe:photoshop';
import { host, versions } from 'adobe:uxp';
import { type Client, type Handler, type Settled, thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { HistoryState, type Identity, type Job, type State } from '@rasm/creative-cloud-server/frames';
import { Effect, Exit, Option, Queue, Schema, Stream, Struct } from 'effect';
import { batchPlay } from './jobs/batch-play.ts';
import { execute } from './jobs/execute.ts';
import { getDocument } from './jobs/get-document.ts';
import { listPresets } from './jobs/list-presets.ts';
import { getPreferences, setPreferences } from './jobs/preferences.ts';
import { runAction } from './jobs/run-action.ts';
import { snapshot } from './jobs/snapshot.ts';
import { endpoint, manifest } from './uxp.config.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _JOBS: Partial<Readonly<Record<Job['kind'], Handler>>> = { execute, batchPlay, snapshot, getDocument, getPreferences, setPreferences, listPresets, runAction };

const _EVENTS = ['open', 'close', 'select', 'save', 'make'];

const _MODAL_HELD = 9;

// --- [MODELS] --------------------------------------------------------------------------

const _held: (cause: unknown) => boolean = Schema.is(Schema.Struct({ number: Schema.Literal(_MODAL_HELD) }));

const _history = Schema.encodeSync(HistoryState.pipe(Schema.encodeKeys({ documentId: 'documentID' })));

// --- [HOST] ----------------------------------------------------------------------------

const _identity = (): Identity => ({
    plugin: manifest.id,
    version: versions.plugin,
    host: { name: host.name, version: host.version },
    uxp: versions.uxp,
    app: Option.none(),
    dom: Option.none(),
});

const _state = (): State => {
    const active = app.documents.length === 0 ? Option.none() : Option.some(app.activeDocument);
    return { modalState: core.isModal(), activeDocumentId: Option.map(active, Struct.get('id')), modified: Option.exists(active, (document) => !document.saved) };
};

const _states: Stream.Stream<State> = Stream.map(
    Stream.callback<string>((queue) => {
        const notifier = (name: string): void => {
            Queue.offerUnsafe(queue, name);
        };
        return Effect.acquireRelease(
            Effect.promise(() => action.addNotificationListener(_EVENTS, notifier)),
            () => Effect.promise(() => action.removeNotificationListener(_EVENTS, notifier)),
        );
    }),
    _state,
);

// --- [SCOPE] ---------------------------------------------------------------------------

const _handle = (job: Job): Effect.Effect<Schema.Json, HostRejection> =>
    Option.match(Option.fromNullishOr(_JOBS[job.kind]), { onNone: () => Effect.fail(HostRejection.cases.unknownMethod.make({ method: job.kind })), onSome: (handler) => handler(job.body) });

const _suspended = (context: ExecutionContext, job: Job): Effect.Effect<Schema.Json, HostRejection> =>
    Effect.acquireUseRelease(
        Effect.transposeOption(Option.map(job.suspendHistory, (held) => Effect.tryPromise({ try: () => context.hostControl.suspendHistory(_history(held)), catch: thrown }))),
        () => _handle(job),
        (suspension, exit) =>
            Effect.asVoid(Effect.transposeOption(Option.map(suspension, (held) => Effect.tryPromise({ try: () => context.hostControl.resumeHistory(held, Exit.isSuccess(exit)), catch: thrown })))),
    ).pipe(
        Effect.filterOrFail(
            () => !context.isCancelled,
            () => HostRejection.cases.userCancelled.make({}),
        ),
    );

const _modal = (job: Job, commandName: string): Effect.Effect<Schema.Json, HostRejection> =>
    Effect.flatMap(
        Effect.tryPromise({
            try: () => core.executeAsModal((context) => Effect.runPromise(Effect.result(_suspended(context, job))), { commandName }),
            catch: (cause) => (_held(cause) ? HostRejection.cases.modalDenied.make({ holder: Option.none() }) : thrown(cause)),
        }),
        Effect.fromResult,
    );

const _perform = (job: Job): Effect.Effect<Settled> =>
    Effect.map(Effect.result(Option.match(job.commandName, { onNone: () => _handle(job), onSome: (commandName) => _modal(job, commandName) })), (result) => ({
        autocorrections: Option.none(),
        result,
    }));

// --- [CLIENT] --------------------------------------------------------------------------

const client: Client = { endpoint, version: manifest.version, identity: _identity, states: _states, perform: _perform };

// --- [EXPORTS] -------------------------------------------------------------------------

export { client };
