import { defaulted, Phase, unit } from '@rasm/ai/bridge';
import { Tokens } from '@rasm/ai/vision';
import { Array, Formatter, Option, Schema, SchemaTransformation, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PATH = '/indesign';

// --- [MODELS] --------------------------------------------------------------------------

const Major = Schema.String.pipe(Schema.decodeTo(Schema.Int, SchemaTransformation.transform({ decode: (version) => Number.parseInt(version, 10), encode: String })));
const Peer = Schema.Struct({ version: Major });
const Index = Schema.Natural.annotate({ description: 'Zero-based position in the document' });
const Points = Schema.Finite.annotate({ identifier: 'Points', description: 'Length in points' });
const Specifier = Schema.String.annotate({ identifier: 'Specifier', description: "InDesign's path to an object, as results return it" }).pipe(Schema.brand('Specifier'));
const Box = Schema.Struct(Struct.Record(['top', 'left', 'bottom', 'right'], Points)).annotate({ identifier: 'Box' });
const Sides = Schema.Struct(Struct.Record(['top', 'bottom', 'inside', 'outside'], Points)).annotate({ identifier: 'Sides' });
const Region = Schema.Struct(Struct.Record(Struct.keys(Box.fields), Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 }))))
    .annotate({ description: 'Part of the page as fractions of its width and height from the top-left corner' })
    .check(Schema.makeFilter((region) => region.top < region.bottom && region.left < region.right, { expected: 'top above bottom and left before right, as fractions of the page' }));
const Reference = Schema.Struct({ type: Schema.String, specifier: Specifier, name: Schema.OptionFromOptionalKey(Schema.String) });
const Field = Schema.Struct({
    name: Schema.String,
    ...Struct.Record(['type', 'description'], Schema.OptionFromOptionalKey(Schema.String)),
    array: Schema.OptionFromOptionalKey(Schema.Literal(true)),
    ...Struct.Record(['minimum', 'maximum'], Schema.OptionFromOptionalKey(Schema.Finite)),
    settable: Schema.OptionFromOptionalKey(Schema.Boolean),
    members: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String)),
}).annotate({ identifier: 'Field' });
const Method = Field.mapFields(Struct.pick(['name', 'description'])).pipe(
    Schema.fieldsAssign({
        parameters: Schema.OptionFromOptionalKey(Schema.Array(Field.mapFields(Struct.omit(['settable'])).pipe(Schema.fieldsAssign({ optional: Schema.Boolean })))),
        returns: Schema.OptionFromOptionalKey(Field.mapFields(Struct.pick(['type', 'array']))),
    }),
);
const Item = Reference.pipe(
    Schema.fieldsAssign({
        ...Struct.Record(['layer', 'appliedObjectStyle'], Schema.String),
        ...Struct.Record(['locked', 'anchored'], Schema.OptionFromOptionalKey(Schema.Literal(true))),
        ...Struct.Record(['parent', 'story'], Schema.OptionFromOptionalKey(Specifier)),
        label: Schema.OptionFromOptionalKey(Schema.String),
        visible: Schema.OptionFromOptionalKey(Schema.Literal(false)),
        bounds: Box,
        overflows: Schema.OptionFromOptionalKey(Schema.Boolean),
        effectivePpi: Schema.OptionFromOptionalKey(Schema.Tuple([Schema.Finite, Schema.Finite])),
    }),
);
const Geometry = Schema.Struct({
    bleed: Sides,
    baseline: Schema.Struct({ start: Points, division: Points, relativeTo: Schema.String }),
    pages: Schema.NonEmptyArray(Schema.Struct({ specifier: Specifier, master: Schema.OptionFromOptionalKey(Specifier), bounds: Box, margins: Sides, content: Box, columns: Schema.Array(Box.mapFields(Struct.pick(['left', 'right']))) })),
    items: Schema.Array(Item),
});
const Target = Schema.TaggedUnion({
    Page: { index: Index, region: Schema.OptionFromOptionalKey(Region) },
    Spread: { index: Index },
    Item: { specifier: Specifier, isolate: defaulted(Schema.Boolean.annotate({ description: 'Render the item alone: what overlaps it leaves the image and its covered parts show' }), false) },
}).annotate({ identifier: 'Target' });
const Shot = Schema.Struct({
    image: Schema.Uint8Array,
    width: Schema.Int,
    height: Schema.Int,
    bounds: Box,
    omitted: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Reference.pipe(Schema.fieldsAssign({ cause: Schema.Defect() }))).annotate({ description: 'Items the image leaves out, each with the cause InDesign gave' })),
    item: Schema.OptionFromOptionalKey(
        Schema.Struct({
            type: Schema.String,
            effectivePpi: Schema.OptionFromOptionalKey(Schema.Finite),
            capped: Schema.Boolean.annotate({ description: "Whether the item's image resolution limited the render" }),
            overlaps: Schema.Array(Reference.pipe(Schema.fieldsAssign({ front: Schema.Boolean }))),
        }),
    ),
});
const Unit = unit(Target);
const Mode = Schema.Literals(['Text', 'Grep', 'Glyph', 'Object']);
const Outcome = Schema.Struct({ corrections: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String)), preview: Schema.OptionFromOptionalKey(Shot) });
const Body = Schema.TaggedUnion({
    Script: {
        code: Schema.String.annotate({
            description: 'Function body whose return value comes back as JSON',
        }),
    },
    Set: {
        target: Specifier.annotate({ identifier: undefined, description: 'Specifier of the object or text to write, or of a collection to write every member' }),
        properties: Schema.JsonObject.annotate({ description: 'Value per field: a specifier names an object, a nested record writes into a preference object' }),
        add: defaulted(Schema.Boolean.annotate({ description: 'Add a member to the target collection from `properties`' }), false),
    },
});

// --- [ERRORS] --------------------------------------------------------------------------

const At = { target: Schema.OptionFromOptionalKey(Specifier), field: Schema.String };
const Refusal = Schema.TaggedUnion({
    Refused: { ...At, cause: Schema.Defect() },
    Untranslatable: { ...At, pattern: Schema.String, constructs: Schema.NonEmptyArray(Schema.Struct({ construct: Schema.String, alternative: Schema.OptionFromOptionalKey(Schema.String) })) },
});
const GalleyReason = Schema.TaggedUnion({
    Missing: { subject: Schema.String, query: Schema.String, candidates: Schema.Array(Schema.String), range: Schema.OptionFromOptionalKey(Schema.Struct(Struct.Record(['first', 'last'], Index))) },
    Thrown: { cause: Schema.Defect(), line: Schema.OptionFromOptionalKey(Schema.Int), source: Schema.OptionFromOptionalKey(Schema.String), corrections: Outcome.fields.corrections, kept: Schema.Boolean },
    Rejected: { fields: Schema.NonEmptyArray(Refusal) },
    Blocked: {},
    Unsettled: {},
    Unentered: {},
});
class GalleyError extends Schema.TaggedError<GalleyError>()('GalleyError', { reason: GalleyReason }) {
    override get message(): string {
        return GalleyReason.match(this.reason, {
            Missing: ({ subject, candidates, range }) =>
                Array.match([...Option.toArray(Option.map(range, ({ first, last }) => `${first} to ${last}`)), ...candidates], {
                    onEmpty: () => `No ${subject}`,
                    onNonEmpty: (listed) => `No ${subject}; candidates: ${Array.join(listed, ', ')}`,
                }),
            Thrown: ({ cause, line, source, corrections, kept }) =>
                Array.join([Formatter.format(cause), ...Option.toArray(Option.map(line, (at) => Array.join([`line ${at}`, ...Option.toArray(source)], ': '))), ...Option.toArray(Option.map(corrections, (applied) => `corrections: ${Array.join(applied, '; ')}`)), kept ? 'changes kept' : 'changes undone'], '\n'),
            Rejected: ({ fields }) =>
                Array.join(
                    [
                        ...Array.map(fields, (refusal) =>
                            Array.join(
                                [
                                    ...Option.toArray(refusal.target),
                                    `${refusal.field}:`,
                                    refusal._tag === 'Refused'
                                        ? Formatter.format(refusal.cause)
                                        : `InDesign GREP cannot read ${Array.join(
                                              Array.map(refusal.constructs, ({ construct, alternative }) => Option.match(alternative, { onNone: () => construct, onSome: (form) => `${construct} (write ${form})` })),
                                              ', ',
                                          )} in ${refusal.pattern}`,
                                ],
                                ' ',
                            ),
                        ),
                        'changes undone',
                    ],
                    '\n',
                ),
            Blocked: () => 'A dialog open in InDesign blocks the call: close it, then retry',
            Unsettled: () => 'A preview body returned a promise: work before its first await is undone, and work after it runs outside the rollback and stays',
            Unentered: () => 'InDesign held the call open and no entry signal from Atelier reached it: nothing ran, retry',
        });
    }
}

// --- [RPCS] ----------------------------------------------------------------------------

const Execute = Rpc.make('Execute', {
    payload: Unit.pipe(Schema.fieldsAssign({ body: Body })),
    success: Outcome.pipe(Schema.fieldsAssign({ result: Schema.OptionFromOptionalKey(Schema.Json) })),
    error: GalleyError,
});
const Enter = Rpc.make('Enter', { success: Phase });
const Render = Rpc.make('Render', {
    payload: Schema.Struct({ target: Target, tokens: Tokens }),
    success: Shot,
    error: GalleyError,
});
const Inspect = Rpc.make('Inspect', {
    payload: Schema.Struct({
        target: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'A specifier, or the name of a class or enumeration; application state when absent' })),
        fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Fields to read, every field when absent; one that holds a preference object reads that object' })),
    }),
    success: Schema.TaggedUnion({
        State: {
            version: Schema.String,
            status: Schema.TaggedUnion({
                Modal: {},
                Ready: {
                    tool: Schema.String,
                    panels: Schema.Array(Schema.String),
                    documents: Schema.Array(Schema.Struct({ specifier: Specifier, name: Schema.String, modified: Schema.Boolean, saved: Schema.Boolean, path: Schema.OptionFromOptionalKey(Schema.String), windows: Schema.Array(Specifier) })),
                    active: Schema.OptionFromOptionalKey(
                        Schema.Struct({
                            document: Specifier,
                            window: Specifier,
                            selection: Schema.Array(
                                Reference.pipe(
                                    Schema.fieldsAssign({
                                        contents: Schema.OptionFromOptionalKey(Schema.String),
                                        story: Schema.OptionFromOptionalKey(Specifier),
                                        overrides: Schema.OptionFromOptionalKey(Schema.Struct(Struct.Record(['paragraph', 'character'], Schema.Boolean))),
                                    }),
                                ),
                            ),
                            overset: Schema.Array(Specifier),
                            preflight: Schema.Struct({ on: Schema.Boolean, profile: Schema.String, problems: Schema.Int }),
                            undo: Schema.Struct({ mode: Schema.String, name: Schema.String, history: Schema.Array(Schema.String) }),
                        }),
                    ),
                },
            }),
        },
        Object: {
            type: Schema.String,
            specifier: Specifier,
            fields: Schema.Array(
                Field.pipe(
                    Schema.fieldsAssign({
                        value: Schema.Json,
                        mixed: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.Json)),
                        style: Schema.OptionFromOptionalKey(Schema.Json.annotate({ description: "Value without the object's overrides, present where they change it" })),
                        source: Schema.OptionFromOptionalKey(Specifier.annotate({ identifier: undefined, description: "Document collection holding the field's valid objects" })),
                    }),
                ),
            ),
            collections: Schema.Array(Schema.Struct({ name: Schema.String, count: Schema.Int, specifier: Specifier })),
            geometry: Schema.OptionFromOptionalKey(Geometry),
        },
        Items: { specifier: Specifier, total: Schema.Int, items: Schema.Array(Reference.pipe(Schema.fieldsAssign({ values: Schema.JsonObject, geometry: Schema.OptionFromOptionalKey(Geometry) }))) },
        Class: { name: Schema.String, parents: Schema.Array(Schema.String), fields: Schema.Array(Field), methods: Schema.Array(Method) },
        Enumeration: { name: Schema.String, members: Schema.Array(Field.mapFields(Struct.pick(['name', 'description']))) },
    }),
    error: GalleyError,
});
const Face = Rpc.make('Face', {
    payload: Schema.Struct({ name: Schema.String }),
    success: Schema.Struct({
        ...Struct.Record(['family', 'style', 'status'], Schema.String),
        uses: Schema.Array(Specifier),
        autoLeading: Schema.Finite,
        installed: Schema.OptionFromOptionalKey(
            Schema.Struct({
                ...Struct.Record(['postscriptName', 'fullName', 'version', 'type', 'location'], Schema.String),
                ...Struct.Record(['styles', 'axes'], Schema.Array(Schema.String)),
                coordinates: Schema.Array(Schema.Finite),
                allows: Schema.Struct(Struct.Record(['outlines', 'pdfEmbedding', 'editableEmbedding', 'restrictedPrinting'], Schema.Boolean)),
            }),
        ),
    }),
    error: GalleyError,
});
const Subset = Rpc.make('Subset', {
    payload: Schema.Struct({ postscriptName: Schema.String, text: Schema.String }),
    success: Schema.String,
    error: GalleyError,
});
const Find = Rpc.make('Find', {
    payload: Unit.pipe(
        Schema.fieldsAssign({
            mode: Mode,
            find: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find preferences: the pattern and the format criteria" })),
            action: Schema.OptionFromOptionalKey(
                Schema.TaggedUnion({
                    Change: { preferences: Schema.JsonObject.annotate({ description: "Fields of the mode's change preferences, written to every match" }) },
                    Style: Struct.Record(['paragraphStyle', 'characterStyle'], Specifier),
                }).annotate({ description: 'Change every match, or add the pattern to the paragraph style as a GREP style applying the character style; absent, the call finds alone' }),
            ),
            options: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find/change options" })),
            scope: Schema.OptionFromOptionalKey(Specifier.annotate({ identifier: undefined, description: 'Specifier of the document, story, text, or page item to search; the active document when absent' })),
            query: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Saved query loaded before `find`, `action`, and `options` apply' })),
            save: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Name to save the query under once the call succeeds' })),
        }),
    ),
    success: Outcome.pipe(
        Schema.fieldsAssign({
            find: Schema.JsonObject,
            change: Schema.OptionFromOptionalKey(Schema.JsonObject),
            count: Schema.Struct(Struct.Record(['matches', 'changes'], Schema.Int)),
            matches: Schema.Array(Schema.Struct({ specifier: Specifier, ...Struct.Record(['text', 'context', 'page'], Schema.OptionFromOptionalKey(Schema.String)), paragraphStyle: Schema.OptionFromOptionalKey(Specifier) })),
            changes: Schema.Array(Schema.Struct({ specifier: Specifier, ...Struct.Record(['before', 'after'], Schema.String) })),
        }),
    ),
    error: GalleyError,
});
const Menu = Rpc.make('Menu', {
    payload: Schema.Struct({ area: Schema.OptionFromOptionalKey(Schema.String) }),
    success: Schema.Array(Schema.Struct({ id: Schema.Int, name: Schema.String, area: Schema.String })),
    error: GalleyError,
});
const Preflight = Rpc.make('Preflight', {
    payload: Schema.Struct({ profile: Schema.OptionFromOptionalKey(Schema.String) }),
    success: Schema.Struct({ profile: Schema.String, rules: Schema.Array(Schema.String), problems: Schema.Array(Schema.Struct({ rule: Schema.String, problem: Schema.String, page: Schema.OptionFromOptionalKey(Schema.String), fix: Schema.OptionFromOptionalKey(Schema.String) })) }),
    error: GalleyError,
});
const Queries = Rpc.make('Queries', {
    success: Schema.Array(Schema.Struct({ name: Schema.String, mode: Mode, origin: Schema.Literals(['user', 'factory']), ...Struct.Record(['find', 'change', 'options'], Schema.JsonObject) })),
    error: GalleyError,
});
const Panel = RpcGroup.make(Execute, Enter, Render, Inspect, Face, Subset, Find, Menu, Preflight, Queries);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Body, Box, Execute, Face, Find, GalleyError, Inspect, Menu, PATH, Panel, Peer, Preflight, Queries, Refusal, Render, Shot, Specifier, Subset, Target };
