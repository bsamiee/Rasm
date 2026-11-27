import { Schema, Struct } from 'effect';
import { Element } from '#/server/element';
import { Instance } from '#/server/instance';

// --- [MODELS] --------------------------------------------------------------------------

const container = { container: Element.Ref, kind: Element.Container.fields.kind, title: Element.Container.fields.title };
const control = { control: Element.Ref, kind: Element.Control.fields.kind };
const answer = Schema.Struct({ ref: Element.Ref, label: Schema.optionalKey(Schema.String) });
const Change = Schema.TaggedUnion({
    started: { identifier: Schema.optionalKey(Schema.String), name: Schema.optionalKey(Schema.String) },
    opened: container,
    closed: container,
    asking: { ...container, text: Element.Container.fields.text, answers: Schema.Array(answer), secure: Schema.optionalKey(answer), presentation: Schema.optionalKey(Instance.Presentation), ...Struct.pick(Element.Container.fields, ['default', 'cancel', 'close']) },
    focused: control,
    value: { ...control, fact: Element.Fact },
    selection: { ...control, evidence: Element.Control.fields.selected.schema },
    moved: {},
    frontmost: {},
    hidden: { hidden: Schema.Boolean },
    window: {},
    status: { status: Instance.State.Status },
    announced: { text: Schema.String, priority: Schema.optionalKey(Schema.Literals(['low', 'medium', 'high'])) },
    dragging: {},
    proven: { followed: Schema.Int, drawn: Schema.Boolean },
    logged: {
        entry: Schema.Struct({
            timestamp: Schema.String,
            messageType: Schema.Literals(['Debug', 'Info', 'Default', 'Error', 'Fault']),
            processID: Schema.Int,
            processImagePath: Schema.String,
            senderImagePath: Schema.String,
            subsystem: Schema.String,
            category: Schema.String,
            eventMessage: Schema.String,
        }),
    },
    reported: { report: Instance.Report },
    sampled: { report: Schema.URLFromString },
    exited: { termination: Instance.State.Termination },
});
class Event extends Schema.Opaque<Event>()(Schema.Struct({ instance: Instance.fields.instance, sequence: Schema.Int, at: Schema.String, change: Change })) {
    static readonly Session = Schema.Struct({ onConsole: Schema.Boolean, locked: Schema.Boolean, asleep: Schema.Boolean, secureInput: Schema.Boolean });
    static readonly Pressure = Schema.Struct({
        memory: Schema.Union([Schema.Literals(['normal', 'warning', 'critical']), Schema.TemplateLiteral([Schema.Int])]),
        thermal: Schema.Union([Schema.Literals(['nominal', 'fair', 'serious', 'critical']), Schema.TemplateLiteral([Schema.Int])]),
        advisory: Schema.optionalKey(Schema.Record(Schema.String, Schema.Union([Schema.Literals(['bad', 'ok', 'great']), Schema.TemplateLiteral([Schema.Int])]))),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Event };
