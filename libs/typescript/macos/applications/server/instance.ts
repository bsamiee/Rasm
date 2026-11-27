import { Function, Record, Schema, Struct } from 'effect';
import { Element } from '#/server/element';
import { ApplicationFailure } from '#/server/failure';

// --- [MODELS] --------------------------------------------------------------------------

const spent = { cpu: Schema.Finite, runnable: Schema.Finite, pageWait: Schema.Finite, diskBytesRead: Schema.Finite, diskBytesWritten: Schema.Finite };
const Usage = Schema.Struct({ ...spent, footprint: Schema.Finite, assertions: Schema.String.pipe(Schema.Array, Schema.optionalKey), assertionsFailure: Schema.optionalKey(Schema.String) });
const Report = Schema.Struct({
    url: Schema.URLFromString,
    kind: Schema.Literals(['crash', 'hang', 'diag', 'spin']),
    date: Schema.String,
    pid: Schema.Int,
    launched: Schema.optionalKey(Schema.String),
    name: Schema.optionalKey(Schema.String),
    executable: Schema.optionalKey(Schema.String),
    coalition: Schema.optionalKey(Schema.String),
    bundleIdentifier: Schema.optionalKey(Schema.String),
    event: Schema.optionalKey(Schema.String),
    exception: Schema.optionalKey(Schema.Struct({ type: Schema.String, signal: Schema.optionalKey(Schema.String), subtype: Schema.optionalKey(Schema.String), message: Schema.optionalKey(Schema.String), rawCodes: Schema.Finite.pipe(Schema.Array, Schema.optionalKey) })),
    termination: Schema.optionalKey(
        Schema.Struct({
            namespace: Schema.String,
            code: Schema.optionalKey(Schema.Finite),
            indicator: Schema.optionalKey(Schema.String),
            byProc: Schema.optionalKey(Schema.String),
            reasons: Schema.String.pipe(Schema.Array, Schema.optionalKey),
            details: Schema.String.pipe(Schema.Array, Schema.optionalKey),
        }),
    ),
    applicationSpecificInformation: Schema.optionalKey(Schema.Record(Schema.String, Schema.Array(Schema.String))),
    virtualMemoryRegionInfo: Schema.optionalKey(Schema.String),
    faultingThread: Schema.Struct({ image: Schema.optionalKey(Schema.String), imageOffset: Schema.Finite, symbol: Schema.optionalKey(Schema.String), symbolLocation: Schema.optionalKey(Schema.Finite) }).pipe(Schema.Array, Schema.optionalKey),
});
const ended = {
    detail: Schema.Literals(['memory', 'codeSigning', 'decryption']).pipe(Schema.Array, Schema.optionalKey),
    reporting: Schema.optionalKey(Schema.Boolean),
    report: Schema.optionalKey(Report),
    reportFailure: Schema.optionalKey(Schema.String),
};
const Termination = Schema.TaggedUnion({ status: { status: Schema.Int, ...ended }, signal: { signal: Schema.Int, ...ended }, statusFailure: { statusFailure: Schema.String, ...ended } });
const Status = Schema.TaggedUnion({ launching: {}, responding: {}, unresponsive: { thread: Schema.Literals(['running', 'runnable', 'throttled', 'uninterruptible', 'sleeping', 'stopped', 'traced']) } });
const State = Object.assign(
    Schema.Union([
        Schema.TaggedStruct('running', {
            active: Schema.Boolean,
            status: Status,
            pendingEventAge: Schema.optionalKey(Schema.Finite),
            usage: Schema.optionalKey(Schema.Struct({ cause: Schema.Literals(['running', 'runnable', 'paging', 'reading', 'writing', 'idle']), duration: Schema.Finite, ...spent, end: Usage })),
            usageFailure: Schema.optionalKey(Schema.String),
            windows: Schema.Array(
                Schema.Struct({
                    number: Element.WindowNumber,
                    title: Schema.optionalKey(Schema.String),
                    frame: Element.Frame,
                    onScreen: Schema.Boolean,
                    hidden: Schema.Boolean,
                    container: Schema.optionalKey(Element.Ref),
                    kind: Schema.optionalKey(Element.Container.fields.kind),
                    document: Schema.optionalKey(Schema.String),
                    asking: Schema.Boolean,
                    presented: Schema.Boolean,
                }),
            ),
            statusItems: Schema.optionalKey(Schema.Natural),
            statusItemsFailure: Schema.optionalKey(Schema.String),
        }),
        Schema.TaggedStruct('failed', ApplicationFailure.stated),
        Schema.TaggedStruct('exited', { termination: Termination }),
    ]).pipe(Schema.toTaggedUnion('_tag')),
    { Status, Termination },
);
class Instance extends Schema.Class<Instance, { readonly brand: unique symbol }>('Instance')({
    instance: Schema.String.check(Schema.isPattern(/^\d+@\d+$/u)).pipe(Schema.brand('ProcessInstance')),
    identifier: Schema.String,
    name: Schema.optionalKey(Schema.String),
    url: Schema.URL,
    language: Schema.optionalKey(Schema.String),
    launchDate: Schema.optionalKey(Schema.DateTimeUtcFromString),
    usage: Schema.optionalKey(Usage),
    usageFailure: Schema.optionalKey(Schema.String),
    state: State,
    listening: Schema.Array(Schema.Int),
    listeningFailure: Schema.optionalKey(Schema.String),
}) {
    static readonly State = State;
    static readonly Report = Report;
    static readonly Presentation = Schema.TaggedUnion({ behind: {}, forward: {}, restored: { back: Schema.Boolean } });
    static readonly QuitMode = Schema.Literals(['asking', 'saving', 'discarding', 'forcing']);
    static readonly Watched = Instance.pipe(Schema.refine((instance): instance is Instance & { readonly language: string } => instance.language !== undefined, { expected: 'an instance stating its interface language' }));
    static readonly byInstance = Function.memoize((rows: readonly Instance[]) => Record.fromIterableBy(rows, Struct.get('instance')));
    static readonly running = Function.memoize((rows: readonly Instance[]) => Record.filter(Instance.byInstance(rows), ({ state }) => !State.isAnyOf(['exited'])(state)));
    static readonly runs =
        ({ url }: Pick<Instance, 'url'>) =>
        (instance: Instance) =>
            instance.url.href === url.href && !State.isAnyOf(['exited'])(instance.state);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Instance };
