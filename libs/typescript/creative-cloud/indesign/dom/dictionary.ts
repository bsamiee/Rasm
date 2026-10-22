import type { InDesignDOMObject, MenuAction } from 'adobe:indesign';
import { Array, Effect, Match, Number, Option, Predicate, Schema, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DECLARED = /Can (?:return|accept): (?<types>.+?)\.(?: \(Optional\))?$/u;
const ALTERNATIVE = /\. Can also accept: |, | or /u;
const ENUMERATOR = / enumerator$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Member = Schema.Struct({
    name: Schema.String,
    type: Schema.OptionFromOptionalKey(Schema.String),
    description: Schema.OptionFromOptionalKey(Schema.String),
    array: Schema.OptionFromOptionalKey(Schema.Literal(true)),
    ...Struct.Record(['minimum', 'maximum'], Schema.OptionFromOptionalKey(Schema.Finite)),
    members: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String)),
    unavailable: Schema.OptionFromOptionalKey(Schema.String),
}).annotate({ identifier: 'Member' });
const Field = Member.pipe(Schema.fieldsAssign({ settable: Schema.Boolean })).annotate({ identifier: 'Field' });
const Method = Member.mapFields(Struct.pick(['name', 'description']))
    .pipe(
        Schema.fieldsAssign({
            parameters: Schema.Array(Member.mapFields(Struct.omit(['unavailable'])).pipe(Schema.fieldsAssign({ optional: Schema.Boolean }))),
            returns: Schema.OptionFromOptionalKey(Member.mapFields(Struct.pick(['type', 'array']))),
        }),
    )
    .annotate({ identifier: 'Method' });
const Class = Schema.Struct({ parent: Schema.OptionFromOptionalKey(Schema.String), fields: Schema.Record(Schema.String, Field), methods: Schema.Record(Schema.String, Method), statics: Schema.Record(Schema.String, Schema.String) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const declared = ({ type, description }: Pick<typeof Member.Type, 'type' | 'description'>): readonly string[] =>
    Option.match(
        Option.flatMapNullishOr(description, (written) => DECLARED.exec(written)?.groups?.types),
        {
            onNone: () => Option.toArray(type),
            onSome: (types) => Array.map(types.split(ALTERNATIVE), (each) => each.replace(ENUMERATOR, '')),
        },
    );
const unavailable = (kind: string, field: string, observed: Option.Option<InDesignDOMObject<unknown>>, writing: boolean): Option.Option<string> => {
    const first = 0x69_74;
    const last = 0x69_87;
    const id = Option.map(
        Option.filter(observed, (each): each is MenuAction => each.constructorName === 'MenuAction'),
        Struct.get('id'),
    );
    return Match.value({ kind, field, writing, stylistic: !Option.exists(id, Predicate.not(Number.between({ minimum: first, maximum: last }))) }).pipe(
        Match.when({ kind: Match.is('FindChangeDropShadowSetting', 'FindChangeInnerShadowSetting'), field: Match.is('angle', 'distance', 'xOffset', 'yOffset', 'properties') }, () => 'The native Transparency provider crashes for this Find/Change shadow accessor'),
        Match.when({ kind: 'EventListener', field: Match.is('handler', 'properties'), writing: false }, () => 'Reading an event handler whose UXP context has ended crashes InDesign; the DOM exposes no context lifetime'),
        Match.when({ kind: 'MenuAction', field: Match.is('enabled', 'checked', 'properties'), writing: false, stylistic: true }, () => 'The native CharacterPanel provider crashes when reading Stylistic Set action state'),
        Match.option,
    );
};
const bare = (name: string) => Effect.orDie(Schema.decodeEffect(Field)({ name, settable: false }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { bare, Class, declared, Field, Member, Method, unavailable };
