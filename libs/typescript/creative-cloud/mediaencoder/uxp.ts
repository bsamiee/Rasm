import { Containment, reference } from '@rasm/object-model/host';
import { HostError } from '@rasm/object-model/model';
import { Callable, HostObject, Uxp } from '@rasm/uxp/plugin';
import { Array, Context, Effect, Exit, Function, Layer, Option, Predicate, Schema, Stream, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Module = Schema.Struct({
    RenderQueue: HostObject({ getInstance: Callable, getJob: Callable, getJobGroup: Callable, RENDER_QUEUE_STATUS_CHANGED: Schema.String, NEXT_JOB_ENCODING_EVENT: Schema.String, MEDIA_ITEM_CREATED_EVENT: Schema.String, ENCODE_COMPLETE_EVENT: Schema.String }, Predicate.isFunction),
    WatchFolder: HostObject({ getInstance: Callable, WATCH_FOLDER_ENCODER_STATUS_CHANGED: Schema.String }, Predicate.isFunction),
    ProgressCategoryContainer: HostObject({ getContainer: Callable }, Predicate.isFunction),
    PresetManager: HostObject({ getFormatNameList: Callable, getPresetsForFormat: Callable }, Predicate.isFunction),
});
const Emitter = HostObject({ subscribeToEvent: Callable, addEventListener: Callable, removeEventListener: Callable });
const Container = HostObject({ getAllProgressCategories: Callable });
const Formats = Schema.Array(Schema.String);
const Listed = Schema.Array(Schema.Unknown);
const Identified = Schema.Struct({ id: Schema.String });
const Batched = Schema.Struct({ batchItemID: Schema.String });
const JobGroup = Identified.mapFields(Struct.assign({ getOutputCount: Callable }));
const Job = Batched.mapFields(Struct.assign({ groupID: Schema.String, getGroup: Callable }));
const Category = Identified.mapFields(Struct.assign({ getProgressItems: Callable }));
const Item = Schema.Struct({ referenceId: Schema.Unknown, getProgressState: Callable });
const Preset = Schema.Struct({ getID: Callable, getFormatName: Callable, getSummaryJSON: Callable });
const Payload = Schema.Struct({ jobId: Schema.optionalKey(Schema.String), status: Schema.optionalKey(Schema.Int), isOkToMoveFile: Schema.optionalKey(Schema.Boolean) });

// --- [SERVICES] ------------------------------------------------------------------------

class MediaEncoder extends Context.Service<MediaEncoder>()('@rasm/mediaencoder/uxp/MediaEncoder', {
    make: Effect.map(Uxp.require('mediaencoder', Module), (module) => {
        const { RenderQueue, ProgressCategoryContainer, PresetManager } = module;
        return {
            ...module,
            containment: {
                root: module,
                kinds: {
                    renderJobGroup: Containment.kind({ items: Function.constUndefined, key: Identified, by: { id: (_, id) => RenderQueue.getJobGroup(`${id}`) }, is: Schema.is(JobGroup)<object> }),
                    renderJob: Containment.kind({ items: Function.constUndefined, key: Batched, by: { batchItemID: (_, id) => RenderQueue.getJob(`${id}`) }, is: Schema.is(Job)<object>, owner: (job) => job.getGroup() }),
                    progressCategory: Containment.kind({
                        items: () =>
                            Option.liftPredicate(ProgressCategoryContainer.getContainer(), Schema.is(Container)).pipe(
                                Option.map((container) => container.getAllProgressCategories()),
                                Option.filter(Schema.is(Uxp.Collection)),
                                Option.getOrUndefined,
                            ),
                        key: Identified,
                        is: Schema.is(Category)<object>,
                    }),
                    progressItem: Containment.kind({ items: (category: typeof Category.Type) => Option.getOrUndefined(Option.liftPredicate(category.getProgressItems(), Schema.is(Uxp.Collection))), is: Schema.is(Item)<object> }),
                    preset: Containment.kind({
                        items: () => Array.flatMap(Option.getOrElse(Option.liftPredicate(PresetManager.getFormatNameList(), Schema.is(Formats)), Array.empty), (format) => Option.getOrElse(Option.liftPredicate(PresetManager.getPresetsForFormat(format), Schema.is(Listed)), Array.empty)),
                        is: Schema.is(Preset)<object>,
                    }),
                },
            } satisfies Containment,
        };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const toEvent = ({ containment, RenderQueue }: MediaEncoder['Service'], name: string, { jobId, ...detail }: typeof Payload.Type) => ({
    name,
    target: Option.fromUndefinedOr(jobId).pipe(
        Option.flatMap(Option.liftThrowable((id: string): unknown => RenderQueue.getJob(id))),
        Option.filter(Predicate.isObject),
        Option.flatMapNullishOr((job) => reference(containment, job)?.specifier),
    ),
    detail: Option.liftPredicate(detail, (held) => Array.isReadonlyArrayNonEmpty(Struct.keys(held))),
});
const listen = (select: (service: MediaEncoder['Service']) => readonly [() => unknown, Array.NonEmptyReadonlyArray<string>]) => ({
    changes: Stream.unwrap(
        MediaEncoder.use(
            Effect.fnUntraced(function* (service) {
                const [instance, names] = select(service);
                const emitter = yield* Effect.try({ try: instance, catch: HostError.refused(`event source of ${names.join(', ')}`) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Emitter)));
                yield* Effect.forEach(names, (name) => {
                    const refused = HostError.refused(`subscribeToEvent ${name}`);
                    return Effect.try({ try: (): unknown => emitter.subscribeToEvent(name), catch: refused }).pipe(
                        Effect.filterOrFail(
                            (subscribed) => subscribed === true,
                            () => refused(),
                        ),
                        Effect.exit,
                    );
                }).pipe(Effect.flatMap(Exit.asVoidAll));
                return Stream.mergeAll(
                    Array.map(names, (name) => Stream.map(Stream.fromEventListener(emitter, name), (event) => [name, event] as const)),
                    { concurrency: 'unbounded' },
                ).pipe(Stream.mapEffect(([name, event]) => Effect.map(Schema.decodeUnknownEffect(Payload)(event), (payload) => toEvent(service, name, payload))));
            }),
        ),
    ),
});
const execute = (code: string) => MediaEncoder.use(({ containment }) => Uxp.execute(containment, code));
const events = {
    renderQueue: listen(({ RenderQueue }) => [() => RenderQueue.getInstance(), [RenderQueue.RENDER_QUEUE_STATUS_CHANGED, RenderQueue.NEXT_JOB_ENCODING_EVENT, RenderQueue.MEDIA_ITEM_CREATED_EVENT, RenderQueue.ENCODE_COMPLETE_EVENT]]),
    watchFolder: listen(({ WatchFolder }) => [() => WatchFolder.getInstance(), [WatchFolder.WATCH_FOLDER_ENCODER_STATUS_CHANGED]]),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { events, execute, MediaEncoder };
