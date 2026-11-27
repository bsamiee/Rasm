import { Schema, Struct } from 'effect';
import { Element } from '#/server/element';
import { ApplicationFailure } from '#/server/failure';
import { Installation } from '#/server/installation';
import { Instance } from '#/server/instance';

// --- [MODELS] --------------------------------------------------------------------------

const container = { container: Element.Ref, ...Struct.pick(Element.cases.container.fields, ['kind', 'title']) };
const control = { control: Element.Ref, kind: Element.cases.control.fields.kind };
const answer = Schema.Struct({ ref: Element.Ref, label: Schema.optionalKey(Schema.String) });
const Change = Schema.TaggedUnion({
    started: { identifier: Schema.optionalKey(Schema.String), name: Schema.optionalKey(Schema.String) },
    opened: container,
    closed: { ...container, front: Schema.optionalKey(Schema.String) },
    asking: { ...container, answers: Schema.Array(answer), secure: Schema.optionalKey(answer), presentation: Schema.optionalKey(Instance.Presentation), ...Struct.pick(Element.cases.container.fields, ['text', 'default', 'cancel', 'close']) },
    consenting: { granted: Schema.optionalKey(Schema.Boolean) },
    focused: control,
    value: { ...control, fact: Element.Fact },
    selection: { ...control, evidence: Element.cases.control.fields.selected.schema },
    hidden: { hidden: Schema.Boolean },
    status: { status: Instance.State.Status },
    announced: { text: Schema.String, priority: Schema.optionalKey(Schema.Literals(['low', 'medium', 'high'])) },
    proven: { followed: Schema.Int, drawn: Schema.Boolean },
    reported: { report: Instance.Report },
    sampled: { report: Schema.URLFromString },
    exited: { termination: Instance.State.Termination },
});
const Changed = Schema.Struct({ instance: Instance.fields.instance, sequence: Schema.Int, at: Schema.String, change: Change });
const Event = Object.assign(
    Schema.TaggedUnion({
        installations: Installation.Listing.fields,
        instances: { read: Instance.pipe(Schema.Array, ApplicationFailure.read) },
        changed: { read: ApplicationFailure.read(Changed) },
        observed: { instance: Instance.fields.instance, identifier: Schema.optionalKey(Schema.String), language: Schema.String, observation: Element.Observation },
        session: { read: ApplicationFailure.read(Schema.Struct({ onConsole: Schema.Boolean, locked: Schema.Boolean, asleep: Schema.Boolean, secureInput: Schema.Boolean })) },
        pressure: {
            read: ApplicationFailure.read(
                Schema.Struct({
                    memory: Schema.Union([Schema.Literals(['normal', 'warning', 'critical']), Schema.TemplateLiteral([Schema.Int])]),
                    thermal: Schema.Union([Schema.Literals(['nominal', 'fair', 'serious', 'critical']), Schema.TemplateLiteral([Schema.Int])]),
                    advisory: Schema.optionalKey(Schema.Record(Schema.String, Schema.Union([Schema.Literals(['bad', 'ok', 'great']), Schema.TemplateLiteral([Schema.Int])]))),
                }),
            ),
        },
    }),
    { Changed },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Event };
