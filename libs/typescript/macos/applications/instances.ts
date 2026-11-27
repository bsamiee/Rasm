import { Schema, Struct } from 'effect';
import { Container, Control, Fact, Frame, Instant, Ref, WindowID } from '#/interface';
import { DiagnosticReport, Exited, LogEntry, Pid, ProcessInstance, ProcessStatus, Usage, UsageInterval } from '#/processes';

// --- [MODELS] --------------------------------------------------------------------------

const open = <const Fields extends Schema.Struct.Fields>(fields: Fields) => Schema.StructWithRest(Schema.Struct(fields), [Schema.Record(Schema.String.check(Schema.makeFilter((key: string) => !Object.hasOwn(fields, key))), Schema.Json)]);
const failure = { reason: Schema.String.annotate({ description: 'ApplicationFailure.Reason case' }), subject: Schema.String, message: Schema.optionalKey(Schema.String) };
const Failed = open(failure);
const Window = Schema.Struct({
    number: WindowID,
    title: Schema.optionalKey(Schema.String.annotate({ description: 'kCGWindowName, absent when unset or without Screen Recording grant' })),
    frame: Frame,
    layer: Container.fields.layer.schema,
    band: Schema.Literals(['normal', 'floating', 'modalPanel', 'utility', 'dock', 'mainMenu', 'status', 'popUpMenu', 'overlay', 'help', 'dragging', 'screenSaver', 'assistiveTechHigh', 'cursor']).annotate({ description: 'CGWindowLevelKey whose level is the highest at or below the layer' }),
    parent: Schema.optionalKey(WindowID.annotate({ description: 'Window this child window is attached to, from the window server' })),
    spaces: Schema.Array(Schema.Int).annotate({ description: 'Spaces the window is on, empty while never ordered in' }),
    onScreen: Schema.Boolean,
    hidden: Schema.Boolean.annotate({ description: 'Ordered out while on a current Space: its application hidden, or the window ordered out or minimized' }),
    shared: Schema.Boolean.annotate({ description: 'kCGWindowSharingState is not kCGWindowSharingNone: false where the window refuses capture' }),
    remote: Schema.optionalKey(WindowID.annotate({ description: 'Window of a view service the application is responsible for, at the same frame and title, whose content this window shows, as a file panel' })),
    container: Schema.optionalKey(Ref.annotate({ description: 'Container the window shows' })),
    kind: Schema.optionalKey(Container.fields.kind),
    document: Schema.optionalKey(Container.fields.document.schema),
    asking: Schema.Boolean.annotate({ description: 'Requires an answer, or blocks its application at modal layer' }),
    presented: Schema.Boolean.annotate({ description: 'A container describes it, or it is on screen' }),
}).annotate({ description: 'Window server row, asking rows first' });
const Status = Schema.TaggedUnion({ launching: {}, responding: {}, unresponsive: { thread: ProcessStatus } }).annotate({ description: 'Launching, responding to Accessibility reads, or unresponsive with the state of its main thread' });
class Running extends Schema.Opaque<Running>()(
    Schema.Struct({
        active: Schema.Boolean,
        status: Status,
        pendingEventAge: Schema.optionalKey(Schema.Finite.annotate({ description: 'Seconds the oldest event the window server holds for the instance has waited unread, present while the window server holds it unresponsive' })),
        usage: Schema.optionalKey(UsageInterval.annotate({ description: 'Growth of the instance usage since it turned unresponsive, with its cause: computing, waiting for a processor, paging, reading, or writing' })),
        usageFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Usage read failure' })),
        windows: Schema.Array(Window),
        statusItems: Schema.optionalKey(Schema.Natural.annotate({ description: 'Status items the application shows in the menu bar' })),
        statusItemsFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Status item read failure' })),
    }),
) {}
const InstanceState = Object.assign(
    Schema.Union([Schema.TaggedStruct('running', Running.fields), open({ _tag: Schema.tag('failed'), ...failure }), Schema.TaggedStruct('exited', { termination: Exited })])
        .annotate({ description: 'Running with every window, failed read, or exited with its cause' })
        .pipe(Schema.toTaggedUnion('_tag')),
    { Exited, Status },
);
const QuitMode = Schema.Literals(['asking', 'saving', 'discarding', 'forcing']).annotate({ description: 'Save option the quit event states where the scripting definition declares one on quit, forcing ending the process with no event' });
const Written = Schema.Result(Schema.Array(Schema.URL), Schema.ErrorInstance()).annotate({ description: 'Documents the instance holds open, by AXDocument and the open command, that FSEvents saw written during the step, else the watch failure' });
class Session extends Schema.Opaque<Session>()(
    Schema.Struct({
        onConsole: Schema.Boolean.annotate({ description: 'kCGSessionOnConsoleKey, false while another user session holds the screen' }),
        locked: Schema.Boolean.annotate({ description: 'The lock screen shows' }),
        asleep: Schema.Boolean.annotate({ description: 'The main display sleeps' }),
        secureInput: Schema.Boolean.annotate({ description: 'A process holds secure event input' }),
        spaces: Schema.Array(Schema.Int).annotate({ description: 'Current Space of every display, as SLSCopySpaces reads it for the spaces of a window, each change read from activeSpaceDidChange' }),
    }).annotate({ description: 'Login session and screens; view and act refuse a session off the console, a locked screen, or a sleeping display' }),
) {}
class Pasteboard extends Schema.Opaque<Pasteboard>()(
    Schema.Struct({
        locator: Container.fields.locator,
        changeCount: Schema.Int.annotate({ description: 'NSPasteboard changeCount, rising with each write by any process' }),
        types: Schema.Array(Schema.String).annotate({ description: 'Pasteboard types the items declare, read with no alert' }),
        access: Schema.Union([Schema.Literals(['default', 'ask', 'alwaysAllow', 'alwaysDeny']), Schema.Int.annotate({ description: 'Raw value of a case this build names none for' })]).annotate({
            description: "NSPasteboard accessBehavior: the owner's Paste from Other Apps setting for the server, default until a read first raised the system's paste alert, which lists the server in that setting",
        }),
        patterns: Schema.Array(Schema.String).annotate({ description: 'NSPasteboardDetectionPattern values the system detects in the content, read with no alert' }),
        contentType: Schema.optionalKey(Schema.String.annotate({ description: 'Uniform type of the file the pasteboard references, read with no alert' })),
        content: Schema.optionalKey(
            Schema.TaggedUnion({ text: { value: Schema.String }, withheld: {}, concealed: {} }).annotate({
                description:
                    'Plain text, read only where the request asks for content and access is alwaysAllow, else withheld as a read would raise the alert or meet the denial; concealed where an item declares an nspasteboard.org ConcealedType, TransientType, or AutoGeneratedType marker, as password managers set, its text and detected kinds never read; absent where the request asks for no content or no item holds plain text',
            }),
        ),
    }).annotate({ description: 'General pasteboard, the clipboard a person copies to and pastes from' }),
) {}
const LaunchedInstance = Schema.Struct({
    pid: Pid,
    instance: ProcessInstance,
    identifier: Schema.String.annotate({ description: 'Bundle identifier' }),
    created: Schema.Boolean.annotate({ description: 'Launch started the instance, false when it reused a running one' }),
    replaced: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Reused instance runs code replaced on disk since it started' })),
});
const container = { container: Ref, kind: Container.fields.kind, title: Container.fields.title };
const control = { control: Ref, kind: Control.fields.kind };
const Change = Schema.TaggedUnion({
    started: { identifier: Schema.optionalKey(LaunchedInstance.fields.identifier), name: Schema.optionalKey(Schema.String) },
    opened: container,
    closed: container,
    asking: {
        ...container,
        text: Container.fields.text,
        answers: Schema.Array(Schema.Struct({ ref: Ref, label: Schema.optionalKey(Schema.String) }).annotate({ description: 'Action control answering the container, with its label' })),
        ...Struct.pick(Container.fields, ['default', 'cancel', 'close']),
    },
    focused: control,
    value: { ...control, fact: Fact },
    selection: { ...control, evidence: Control.fields.selected.schema },
    moved: { ...Struct.omit(container, ['title']), frame: Frame },
    frontmost: { active: Schema.Boolean },
    hidden: { hidden: Schema.Boolean },
    window: {
        change: Schema.Union([Schema.Literals(['created', 'destroyed', 'moved', 'resized', 'levelChanged', 'shown', 'hidden', 'addedToSpace', 'removedFromSpace']), Schema.String.annotate({ description: 'Notification of a case this build names none for, which readers pass over' })]).annotate({
            description: 'Window server notification: created, destroyed, moved, or resized for a window no container describes, its level changed, shown or hidden, or added to or removed from a Space',
        }),
        window: Window.annotate({ description: 'Row after the change, the last row read for a destroyed window' }),
    },
    status: { status: Status },
    announced: {
        text: Schema.String.annotate({ description: 'AXAnnouncementKey: text the application asked assistive technology to speak, as a live region does on each change' }),
        priority: Schema.optionalKey(Schema.Literals(['low', 'medium', 'high']).annotate({ description: 'AXPriorityKey the application posted with it' })),
    },
    dragging: {
        notification: Schema.Literals(['AXDraggingSourceDragBegan', 'AXDraggingSourceDragEnded', 'AXDraggingDestinationDropAllowed', 'AXDraggingDestinationDropNotAllowed', 'AXDraggingDestinationDragAccepted', 'AXDraggingDestinationDragNotAccepted']).annotate({
            description: 'AppKit dragging notification: a drag began or ended at its source, a drop over a destination became allowed or refused, or the destination accepted or refused the drop',
        }),
        control: Schema.optionalKey(Ref),
        kind: Schema.optionalKey(Control.fields.kind),
    },
    logged: { entry: LogEntry.annotate({ description: 'Fault the instance logged, streamed while a handle drives it' }) },
    reported: { report: DiagnosticReport.annotate({ description: 'Hang, spin, or diagnostic report written for the instance while a handle drives it, a crash report arriving with exited' }) },
    sampled: { report: Schema.URLFromString.annotate({ description: 'sample report of the main thread and every other thread, taken as the instance turned unresponsive while a handle drives it' }) },
    exited: { termination: Exited },
}).annotate({
    description:
        'Change of a selected instance: started, container opened, closed, or asking, focus, value, or selection of a control, a moved container, frontmost, hidden, a window server window change, status, an announcement, a drag at a control, a logged fault, a written diagnostic report, a sample taken on a hang, exited with its crash report',
});
class Event extends Schema.Opaque<Event>()(Schema.Struct({ instance: ProcessInstance, sequence: Schema.Int.annotate({ description: 'Per instance, rising with each change' }), at: Schema.String.annotate({ description: 'ISO 8601 time the change was read' }), change: Change })) {}
class InstanceSnapshot extends Schema.Class<InstanceSnapshot, { readonly brand: unique symbol }>('InstanceSnapshot')({
    ...Struct.pick(LaunchedInstance.fields, ['pid', 'instance', 'identifier']),
    name: Schema.optionalKey(Schema.String.annotate({ description: 'Display name the running application states, absent where it states none' })),
    url: Schema.URL.annotate({ description: 'Application bundle' }),
    activationPolicy: Schema.Union([Schema.Literals(['regular', 'accessory', 'prohibited']), Schema.TemplateLiteral([Schema.Int])]).annotate({
        description: 'Regular in Dock, accessory for menu bar or helper process, prohibited for background only, the raw value for a policy this build names no case for',
    }),
    language: Schema.optionalKey(Schema.String.annotate({ description: 'Interface language of the instance, on the reply of watch' })),
    launchDate: Schema.optionalKey(Instant.annotate({ description: 'When Launch Services launched the instance, absent for a process started outside Launch Services' })),
    usage: Schema.optionalKey(Usage.annotate({ description: 'Counters of the instance since it started, with its power assertions, on the reply of watch' })),
    usageFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Usage read failure, on the reply of watch' })),
    state: InstanceState,
    listening: Schema.Array(Schema.Int).annotate({ description: 'TCP ports the process listens on, read at each state, empty unless running or on a failed read' }),
    listeningFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Listening ports read failure' })),
}) {
    static readonly Watched = InstanceSnapshot.pipe(Schema.refine((snapshot): snapshot is InstanceSnapshot & { readonly language: string } => snapshot.language !== undefined, { expected: 'an instance snapshot stating its interface language' }));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Event, Failed, InstanceSnapshot, InstanceState, LaunchedInstance, Pasteboard, QuitMode, Running, Session, Written };
