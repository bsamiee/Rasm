import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Pid = Schema.Int.annotate({ description: 'Process of a running instance' });
const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CGWindowID that status elements report and screenshot takes' });
const ElementID = Schema.String.annotate({ description: 'Base64 Accessibility remote token of an element, absent once the element closes' });
const ElementAction = Schema.Union([
    Schema.Struct({ perform: Schema.Struct({ element: ElementID, action: Schema.String.annotate({ description: 'Accessibility action the element lists, except AXRaise' }) }) }),
    Schema.Struct({ set: Schema.Struct({ element: ElementID, value: Schema.String.annotate({ description: 'Text written to AXValue' }) }) }),
]);
const Perform = Schema.Struct({ actions: Schema.NonEmptyArray(ElementAction).annotate({ description: 'Actions on status element ids, run in order until one fails' }) });
const Fields = Schema.Record(Schema.String, Schema.Json);
const Launching = Schema.Struct({ launching: Fields });
const Responding = Schema.Struct({ responding: Schema.StructWithRest(Schema.Struct({ elements: Schema.Array(Schema.StructWithRest(Schema.Struct({ requiresAnswer: Schema.Boolean }), [Fields])) }), [Fields]) });
const Unresponsive = Schema.Struct({ unresponsive: Fields });
const Running = Schema.Struct({ running: Schema.Struct({ status: Schema.Union([Launching, Responding, Unresponsive]) }) });
const Failed = Schema.Struct({ failed: Fields });
const Exited = Schema.Struct({ exited: Fields });
const State = Schema.Union([Running, Failed, Exited]).annotate({
    description: 'Instance state: running with open windows, sheets, and menus while responding, launching until launch finishes, or unresponsive when it answers no Accessibility request within the messaging timeout; why a status read failed; or how it exited',
});
const RunningInstance = Schema.Struct({
    pid: Pid,
    url: Schema.URL.annotate({ description: 'Bundle the instance runs' }),
    activationPolicy: Schema.Literals(['regular', 'accessory', 'prohibited']).annotate({ description: 'NSApplication activation policy: regular for an application in the Dock, accessory for a menu bar or helper process' }),
    state: State,
});
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL,
    identifier: Schema.String,
    name: Schema.String,
    version: Schema.OptionFromOptionalKey(Schema.String),
    scriptable: Schema.Boolean,
}) {}
class Screenshot extends Schema.Class<Screenshot, { readonly brand: unique symbol }>('Screenshot')({
    window: Schema.OptionFromOptionalKey(WindowID.annotate({ description: "Window to capture, absent for the application's on-screen windows on the display showing most of them" })),
    pixels: Schema.OptionFromOptionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Longest image side in pixels, larger captures scale down and smaller ones keep native size' })),
}) {
    static readonly Result = Schema.Struct({ image: Schema.Uint8ArrayFromBase64, width: Schema.Int.annotate({ description: 'Image width in pixels' }), height: Schema.Int.annotate({ description: 'Image height in pixels' }) });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ElementAction, Exited, Failed, Installation, Launching, Perform, Pid, Responding, Running, RunningInstance, Screenshot, State, Unresponsive, WindowID };
