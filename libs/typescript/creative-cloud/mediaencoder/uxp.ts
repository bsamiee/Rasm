import { Containment, reference } from '@rasm/object-model/host';
import { HostError } from '@rasm/object-model/model';
import { Callable, Uxp } from '@rasm/uxp/plugin';
import { Array, Context, Effect, Function, Layer, Option, Predicate, Record, Schema, Stream, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Statics = <const Fields extends Schema.Struct.Fields>(fields: Fields) => {
    const members = Schema.Struct(fields);
    return Schema.declare((value: unknown): value is typeof members.Type => Predicate.isFunction(value) && Schema.is(members)(Record.fromIterableWith(Struct.keys(fields), (key) => [key, Reflect.get(value, key)])));
};
const Module = Schema.Struct({
    RenderQueue: Statics({ getInstance: Callable, getJob: Callable, getJobGroup: Callable, RENDER_QUEUE_STATUS_CHANGED: Schema.String, NEXT_JOB_ENCODING_EVENT: Schema.String, MEDIA_ITEM_CREATED_EVENT: Schema.String, ENCODE_COMPLETE_EVENT: Schema.String }),
    WatchFolder: Statics({ getInstance: Callable, WATCH_FOLDER_ENCODER_STATUS_CHANGED: Schema.String }),
    ProgressCategoryContainer: Statics({ getContainer: Callable }),
    PresetManager: Statics({ getFormatNameList: Callable, getPresetsForFormat: Callable }),
}).annotate({ description: 'Classes require("mediaencoder") holds, by the static members ScriptingAPIManifest.json declares and the plugin reads' });
const Emitter = Schema.Struct({ subscribeToEvent: Callable, addEventListener: Callable, removeEventListener: Callable }).pipe(Schema.is, Schema.declare).annotate({
    description: 'Event source a class getInstance returns: subscribeToEvent from ScriptingAPIManifest.json, the listener pair from AdobeDocs uxp-media-encoder',
});
const Container = Schema.Struct({ getAllProgressCategories: Callable }).pipe(Schema.is, Schema.declare).annotate({ description: 'ProgressCategoryContainer instance getContainer returns' });
const Formats = Schema.Array(Schema.String).annotate({ description: 'Format names getFormatNameList returns' });
const Listed = Schema.Array(Schema.Unknown).annotate({ description: 'Array a host list call returns' });
const Identified = Schema.Struct({ id: Schema.String }).annotate({ description: 'Job group or progress category keyed by its id' });
const Batched = Schema.Struct({ batchItemID: Schema.String }).annotate({ description: 'Job keyed by its batch item id' });
const JobGroup = Identified.mapFields(Struct.assign({ getOutputCount: Callable })).annotate({ description: 'RenderJobGroup, one queued source with its outputs' });
const Job = Batched.mapFields(Struct.assign({ groupID: Schema.String, getGroup: Callable })).annotate({ description: 'RenderJob, one output of a job group' });
const Category = Identified.mapFields(Struct.assign({ getProgressItems: Callable })).annotate({ description: 'ProgressCategory, the render queue or watch folder progress' });
const Item = Schema.Struct({ referenceId: Schema.Unknown, getProgressState: Callable }).annotate({ description: 'ProgressItem of a progress category' });
const Preset = Schema.Struct({ getID: Callable, getFormatName: Callable, getSummaryJSON: Callable }).annotate({ description: 'Preset of a format PresetManager lists' });
const Payload = Schema.Struct({ jobId: Schema.optionalKey(Schema.String), status: Schema.optionalKey(Schema.Int), isOkToMoveFile: Schema.optionalKey(Schema.Boolean) }).annotate({
    description: 'Fields of RenderQueueEvent, RenderNextJobEvent, RenderMediaInfoCreatedEvent, RenderQueueEncodeCompleteEvent, and WatchFolderStatusEvent',
});

// --- [SERVICES] ------------------------------------------------------------------------

class MediaEncoder extends Context.Service<MediaEncoder>()('@rasm/mediaencoder/uxp/MediaEncoder', {
    make: Effect.map(Effect.flatMap(Uxp.require('mediaencoder'), Schema.decodeUnknownEffect(Module)), (module) => {
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
                yield* Effect.validate(
                    names,
                    (name) => {
                        const refused = HostError.refused(`subscribeToEvent ${name}`);
                        return Effect.try({ try: (): unknown => emitter.subscribeToEvent(name), catch: refused }).pipe(
                            Effect.filterOrFail(
                                (subscribed) => subscribed === true,
                                () => refused(),
                            ),
                        );
                    },
                    { discard: true },
                );
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
