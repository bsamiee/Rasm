import { Unmatched } from '@rasm/search/unmatched';
import { Effect, Schema, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { Capture, LongestSide, Recording } from '#/capture';
import { AppleEventValue, Installation, LaunchEvent, ScriptingDictionary } from '#/installations';
import { Event, Failed, InstanceSnapshot, InstanceState, LaunchedInstance, Pasteboard, QuitMode, Session, Written } from '#/instances';
import { Build, Frame, Locator, Model, Observation, Presentation, Ref, Scope, WindowID } from '#/interface';
import { Diagnostics, Pressure, ProcessInstance } from '#/processes';
import { Act } from '#/steps';

// --- [MODELS] --------------------------------------------------------------------------

const Listing = Schema.Struct({
    installations: Schema.Array(Installation),
    unreadable: Schema.Array(Failed).annotate({ description: 'Bundles that failed to read and application folders whose file events failed to start, each with its cause, the path as subject' }),
});
const Identifiers = { identifiers: Schema.optionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: "Bundle identifiers of the instances monitored, each matching the server's applications, every application the server matches when absent" })) };
const Table = Schema.Array(Locator).annotate({ description: 'Locator table of a Watch of the instance in order, the reference of each locator its position counted from 1' });

// --- [ERRORS] --------------------------------------------------------------------------

class ApplicationFailure extends Schema.TaggedError<ApplicationFailure, { readonly brand: unique symbol }>()('ApplicationFailure', {
    reason: Failed.schema.fields.reason,
    subject: Schema.String,
    message: Schema.String,
    grant: Schema.optionalKey(Schema.Literals(['accessibility', 'screenCapture', 'postEvent']).annotate({ description: 'Privacy grant of noGrant, which the server lacks, or of notResponsible, which its responsible process lacks' })),
    settings: Schema.optionalKey(Schema.String.annotate({ description: 'x-apple.systempreferences URL of the Privacy & Security switch that gives the grant' })),
}) {}
class AppleEventFailure extends Schema.TaggedError<AppleEventFailure, { readonly brand: unique symbol }>()('AppleEventFailure', {
    reason: Schema.String.annotate({ description: 'AppleEventFailure.Reason case, unconsented for a denied or undecided Automation switch' }),
    subject: Schema.String,
    message: Schema.String,
    status: Schema.optionalKey(Schema.Int.annotate({ description: 'OSStatus of the send or reply' })),
}) {}
const Failure = Schema.Union([Unmatched, ApplicationFailure, AppleEventFailure, Schema.ErrorInstance()]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const ApplicationRpcs = RpcGroup.make(
    Rpc.make('installations', {
        payload: {
            identifiers: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Bundle identifiers' })),
            prefix: Schema.optionalKey(Schema.String.annotate({ description: 'Bundle identifier prefix, joined with identifiers by or, every application the server matches when both are absent' })),
            name: Schema.optionalKey(Schema.String.annotate({ description: 'Text matched against the display name and file name of each application bundle the Spotlight index holds, wherever it sits, where identifiers and prefix search the application folders alone' })),
        },
        success: Listing.annotate({ description: 'Applications folders and running regular applications, or the bundles name matches, on subscribe, each Spotlight update, and each regular launch or exit' }),
        stream: true,
    }),
    Rpc.make('installation', { payload: { url: Installation.fields.url.annotate({ description: 'Application bundle, listed or not' }) }, success: Installation, error: Failure }),
    Rpc.make('instances', {
        payload: Identifiers,
        success: Schema.Result(Schema.Array(InstanceSnapshot), Schema.ErrorInstance()).annotate({
            description: 'On subscribe and each change, every instance not background only, an instance watch or a request started a Watch for in its Accessibility state, exited instances kept until the next change, else the listing failure',
        }),
        stream: true,
    }),
    Rpc.make('events', {
        payload: Identifiers,
        success: Schema.Result(Event, Schema.ErrorInstance()).annotate({
            description: 'Events of each instance a watch or a request started a Watch for, and of each process outside the Dock raising a modal panel, else the failure of the modal panel scan',
        }),
        stream: true,
    }),
    Rpc.make('observations', {
        payload: Identifiers,
        success: Schema.Struct({ instance: ProcessInstance, identifier: Schema.optionalKey(LaunchedInstance.fields.identifier), language: Schema.String.annotate({ description: 'Interface language of the instance' }), observation: Observation }).annotate({
            description: 'Each container read whose static facts differ from the last sent, or that opened again',
        }),
        stream: true,
    }),
    Rpc.make('session', { success: Schema.Result(Session, Schema.ErrorInstance()).annotate({ description: 'On subscribe and each change of console, lock, display sleep, secure input, or current Space, or the window server refusing its secure input notices' }), stream: true }),
    Rpc.make('pressure', { success: Schema.Result(Pressure, Schema.ErrorInstance()).annotate({ description: 'On subscribe and each kernel change, or read error' }), stream: true }),
    Rpc.make('pasteboard', {
        payload: {
            text: Schema.optionalKey(Schema.String.annotate({ description: 'Plain text written as the only item before the read' })),
            content: Schema.Boolean.annotate({ description: 'Read the plain text too; false reads the change count, types, and detected kinds alone' }),
        },
        success: Pasteboard.annotate({ description: 'General pasteboard once the write, if any, finished' }),
        error: Failure,
    }),
    Rpc.make('watch', {
        payload: { instance: ProcessInstance, locators: Schema.optionalKey(Table.annotate({ description: 'Table an earlier Watch of the instance held, which a new Watch starts from so every reference keeps its number' })) },
        success: InstanceSnapshot.Watched.annotate({ description: 'State and interface language of the instance, its Accessibility events, states, and observations streamed from this call on' }),
        error: Failure,
    }),
    Rpc.make('locators', { payload: { instance: ProcessInstance }, success: Table, error: Failure }),
    Rpc.make('release', { payload: { instance: ProcessInstance.annotate({ description: 'Instance no handle drives any longer, its Watch ended until a request drives it again' }) } }),
    Rpc.make('model', {
        payload: {
            instance: ProcessInstance,
            scope: Scope,
            known: Schema.optionalKey(
                Schema.Struct({
                    build: Schema.OptionFromOptionalKey(Build.annotate({ description: 'Absent where the bundle states no CFBundleVersion' })),
                    containers: Schema.Array(Locator).annotate({ description: 'Containers the interface map holds as read in this build: a window the watch holds open among them is checked in place of read whole' }),
                    labels: Schema.Array(Schema.String).annotate({ description: 'Labels the interface map holds for menus, the readings text recognition picks among the candidates Vision gives for each flyout line' }),
                }),
            ),
        },
        success: Model,
        error: Failure,
    }),
    Rpc.make('intern', {
        payload: { instance: ProcessInstance, locators: Schema.Array(Locator) },
        success: Schema.Array(Ref).annotate({ description: 'Reference of each locator in order, from the table the Watch of the instance keys its own locators by' }),
        error: Failure,
    }),
    Rpc.make('handlers', {
        payload: { document: Schema.URL.annotate({ description: 'File or URL' }) },
        success: Listing.annotate({ description: "Applications the server matches that open the document, the system's default first, then by the system's suitability" }),
        error: Failure,
    }),
    Rpc.make('launch', {
        payload: {
            url: Installation.fields.url,
            documents: Schema.Array(Schema.URL)
                .pipe(Schema.withConstructorDefault(Effect.succeed([])))
                .annotate({ description: 'Files the application opens' }),
            arguments: Schema.Array(Schema.String)
                .pipe(Schema.withConstructorDefault(Effect.succeed([])))
                .annotate({ description: 'Arguments of a new process, refused where a running instance would receive them' }),
            environment: Schema.Record(Schema.String, Schema.String)
                .pipe(Schema.withConstructorDefault(Effect.succeed({})))
                .annotate({ description: 'Environment variables of a new process, refused as arguments are' }),
            new: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))).annotate({ description: 'Start an instance beside the running ones' }),
            opening: Schema.Record(Schema.String, AppleEventValue)
                .pipe(Schema.withConstructorDefault(Effect.succeed({})))
                .annotate({ description: 'Parameters of the open command by name or code, sent with the documents to the instance once it turns idle behind the frontmost application' }),
            wait: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))).annotate({ description: 'Events continue until the instance exits' }),
        },
        success: LaunchEvent,
        error: Failure,
        stream: true,
    }),
    Rpc.make('act', {
        payload: {
            instance: ProcessInstance,
            steps: Act.steps(
                Ref,
                { from: Schema.optionalKey(Act.Rung.pick(['accessibility', 'process', 'window', 'focused', 'system']).annotate({ description: 'Route the step starts at, the one that last proved it in this build' })) },
                { windows: Schema.optionalKey(Schema.Array(WindowID).annotate({ description: 'Windows the image the points were read from composites, the window under the first point among them taking the events' })) },
            ),
            record: Schema.optionalKey(Recording.annotate({ description: 'Source recorded while the steps run, its key frames returned with the outcomes' })),
        },
        success: Act.Result,
        error: Failure,
    }),
    Rpc.make('activate', { payload: { instance: ProcessInstance }, success: Presentation.annotate({ description: 'forward: the instance brought in front and left there; behind: the system refused to bring it forward' }), error: Failure }),
    Rpc.make('quit', { payload: { instance: ProcessInstance, mode: QuitMode.pipe(Schema.withConstructorDefault(Effect.succeed('asking'))) }, success: InstanceState.annotate({ description: 'Last state, a dialog or unresponsive instance staying running' }), error: Failure }),
    Rpc.make('dictionary', { payload: { url: Installation.fields.url }, success: ScriptingDictionary, error: Failure }),
    Rpc.make('execute', {
        payload: {
            instance: ProcessInstance,
            arguments: Schema.Record(Schema.String, AppleEventValue)
                .annotate({ description: 'Parameters of the script command by name or code, direct for the direct parameter, reply values accepted' })
                .pipe(Schema.withConstructorDefault(Effect.succeed({}))),
        },
        success: Schema.Struct({ reply: AppleEventValue.annotate({ description: 'Direct parameter of the reply, decoded by the scripting dictionary' }), written: Written }),
        error: Failure,
    }),
    Rpc.make('capture', {
        payload: {
            instance: ProcessInstance,
            ...Capture.fields,
            marks: Schema.Array(Schema.Struct({ frame: Frame, label: Schema.String }))
                .pipe(Schema.withConstructorDefault(Effect.succeed([])))
                .annotate({ description: 'Frames drawn into each image they meet, each label set beside its frame (above, below, right, then left) where it stays inside the image and covers no other frame, else inside its top left corner' }),
        },
        success: Capture.Result,
        error: Failure,
    }),
    Rpc.make('record', {
        payload: {
            instance: ProcessInstance,
            ...Recording.fields,
            seconds: Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Seconds recorded, the windows the application opens meanwhile on that display included' }),
            movie: Schema.optionalKey(Schema.URL.annotate({ description: 'File URL SCRecordingOutput writes the movie to, its extension naming a type it writes (mp4, mov); no movie when absent' })),
        },
        success: Recording.Result,
        error: Failure,
    }),
    Rpc.make('scale', {
        payload: { image: Schema.Uint8Array.annotate({ description: 'Image bytes in any format ImageIO reads' }), longestSide: LongestSide },
        success: Schema.Struct(Struct.pick(Capture.Screenshot.fields, ['png', 'width', 'height'])),
        error: Failure,
    }),
    Rpc.make('diagnostics', { payload: Diagnostics.fields, success: Diagnostics.Result, error: Failure }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleEventFailure, ApplicationFailure, ApplicationRpcs, Failure };
