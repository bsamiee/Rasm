import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Pid = Schema.Int.annotate({ description: 'Running instance process ID' });
const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CGWindowID from status elements' });
const ElementID = Schema.String.annotate({ description: 'Base64 Accessibility remote token, absent once element closes' });
const ElementAction = Schema.Union([
    Schema.Struct({ perform: Schema.Struct({ element: ElementID, action: Schema.String.annotate({ description: 'Element action except AXRaise' }) }) }),
    Schema.Struct({ set: Schema.Struct({ element: ElementID, value: Schema.String.annotate({ description: 'Text written to AXValue' }) }) }),
]);
const Perform = Schema.Struct({ actions: Schema.NonEmptyArray(ElementAction).annotate({ description: 'Actions run in order until one fails' }) });
const Fields = Schema.Record(Schema.String, Schema.Json);
const Launching = Schema.Struct({ launching: Fields });
const Responding = Schema.Struct({ responding: Schema.StructWithRest(Schema.Struct({ elements: Schema.Array(Schema.StructWithRest(Schema.Struct({ requiresAnswer: Schema.Boolean }), [Fields])) }), [Fields]) });
const Unresponsive = Schema.Struct({ unresponsive: Fields });
const Running = Schema.Struct({ running: Schema.Struct({ status: Schema.Union([Launching, Responding, Unresponsive]) }) });
const Failed = Schema.Struct({ failed: Fields });
const Exited = Schema.Struct({ exited: Fields });
const State = Schema.Union([Running, Failed, Exited]).annotate({ description: 'Windows, sheets, and menus while responding, unresponsive past Accessibility timeout, failed on status read' });
const RunningInstance = Schema.Struct({
    pid: Pid,
    url: Schema.URL.annotate({ description: 'Running bundle' }),
    activationPolicy: Schema.Literals(['regular', 'accessory', 'prohibited']).annotate({ description: 'Regular in Dock, accessory for menu bar or helper process' }),
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
    window: Schema.OptionFromOptionalKey(WindowID.annotate({ description: 'Absent for on-screen windows on display holding most' })),
    pixels: Schema.OptionFromOptionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Longest side limit, smaller captures keep native size' })),
}) {
    static readonly Result = Schema.Struct({ image: Schema.Uint8ArrayFromBase64, width: Schema.Int.annotate({ description: 'Pixels' }), height: Schema.Int.annotate({ description: 'Pixels' }) });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ElementAction, Exited, Failed, Installation, Launching, Perform, Pid, Responding, Running, RunningInstance, Screenshot, State, Unresponsive, WindowID };
