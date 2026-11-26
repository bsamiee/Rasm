import { assert, it } from '@effect/vitest';
import { events, execute, MediaEncoder } from '@rasm/mediaencoder/uxp';
import { HostError } from '@rasm/object-model/model';
import { Deferred, Effect, Fiber, Option, Stream } from 'effect';
import { vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVENTS = { status: 'RenderQueueStatusEvent', next: 'RenderNextJobEvent', created: 'RenderMediaInfoCreatedEvent', complete: 'RenderQueueEncodeCompleteEvent', watched: 'WatchFolderStatusEvent' } as const;
const GROUP = { id: 'g1', getOutputCount: () => 1 } as const;
const JOB = '/renderJobGroup[@id="g1"]/renderJob[@batchItemID="j1"]';

// --- [MODELS] --------------------------------------------------------------------------

class RenderJob {
    readonly batchItemID: string;
    readonly groupID: string;
    readonly #group: typeof GROUP;

    constructor(batchItemID: string, group: typeof GROUP) {
        this.batchItemID = batchItemID;
        this.groupID = group.id;
        this.#group = group;
    }

    getGroup() {
        return this.#group;
    }
}
class Emitter extends EventTarget {
    readonly #accepts: boolean;
    readonly #added: (type: string) => void;

    constructor(accepts: boolean, added: (type: string) => void) {
        super();
        this.#accepts = accepts;
        this.#added = added;
    }

    subscribeToEvent(_name: string) {
        return this.#accepts;
    }

    override addEventListener(...args: Parameters<EventTarget['addEventListener']>) {
        super.addEventListener(...args);
        this.#added(args[0]);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const hosted = (accepts: boolean) => {
    const job = new RenderJob('j1', GROUP);
    const completing = Deferred.makeUnsafe<void>();
    const added = vi.fn<(type: string) => void>((type) => {
        if (type === EVENTS.complete) {
            Deferred.doneUnsafe(completing, Effect.void);
        }
    });
    const [queue, folder] = [new Emitter(accepts, added), new Emitter(accepts, added)];
    const RenderQueue = Object.assign(() => undefined, {
        RENDER_QUEUE_STATUS_CHANGED: EVENTS.status,
        NEXT_JOB_ENCODING_EVENT: EVENTS.next,
        MEDIA_ITEM_CREATED_EVENT: EVENTS.created,
        ENCODE_COMPLETE_EVENT: EVENTS.complete,
        getInstance: () => queue,
        getJob: (id: string) => (id === job.batchItemID ? job : null),
        getJobGroup: (id: string) => (id === GROUP.id ? GROUP : null),
    });
    const WatchFolder = Object.assign(() => undefined, { WATCH_FOLDER_ENCODER_STATUS_CHANGED: EVENTS.watched, getInstance: () => folder });
    const ProgressCategoryContainer = Object.assign(() => undefined, { getContainer: () => ({ getAllProgressCategories: () => [] }) });
    const PresetManager = Object.assign(() => undefined, { getFormatNameList: () => [], getPresetsForFormat: () => [] });
    const module = { RenderQueue, WatchFolder, ProgressCategoryContainer, PresetManager };
    const required = (id: string): unknown => (id === 'mediaencoder' ? module : undefined);
    return {
        queue,
        added,
        completing,
        run: <A, E, R>(effect: Effect.Effect<A, E, R>) =>
            Effect.acquireRelease(
                Effect.sync(() => vi.stubGlobal('require', required)),
                () => Effect.sync(() => vi.unstubAllGlobals()),
            ).pipe(Effect.andThen(Effect.provide(effect, MediaEncoder.layer)), Effect.scoped),
    };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

vi.mock(import('adobe:uxp'), () => ({}));
vi.mock(import('adobe:fs'), () => ({}));

it.effect('answers a job a body returns by its group id and batch item id, the keys getJobGroup and getJob take', () =>
    hosted(true).run(
        Effect.map(execute(`(function body() {\n    return resolve('${JOB}');\n})`), (result) => {
            assert.deepStrictEqual(result, Option.some({ type: 'RenderJob', specifier: JOB, valid: true }));
        }),
    ),
);

it.effect('names a completed job by the specifier of its group and batch item, its other fields the detail', () => {
    const host = hosted(true);
    return host.run(
        Effect.gen(function* () {
            const subscribed = vi.spyOn(host.queue, 'subscribeToEvent');
            const first = yield* Effect.forkScoped(Stream.runHead(events.renderQueue.changes));
            yield* Deferred.await(host.completing);
            host.queue.dispatchEvent(Object.assign(new Event(EVENTS.complete), { jobId: 'j1', isOkToMoveFile: true }));
            assert.deepStrictEqual(yield* Fiber.join(first), Option.some({ name: EVENTS.complete, target: Option.some(JOB), detail: Option.some({ isOkToMoveFile: true }) }));
            assert.deepStrictEqual(subscribed.mock.calls, [[EVENTS.status], [EVENTS.next], [EVENTS.created], [EVENTS.complete]]);
        }),
    );
});

it.effect('fails with a refusal and adds no listener where subscribeToEvent answers false, as Media Encoder returns a refused subscription as a value', () => {
    const host = hosted(false);
    return host.run(
        Effect.gen(function* () {
            const failed = yield* Effect.flip(Stream.runHead(events.renderQueue.changes));
            assert.instanceOf(failed, HostError);
            assert.strictEqual(failed.reason._tag, 'Refused');
            assert.deepStrictEqual(host.added.mock.calls, []);
        }),
    );
});
