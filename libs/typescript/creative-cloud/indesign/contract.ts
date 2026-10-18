import { Surface } from '@rasm/mcp/link';
import { Activity, Image, Instructions, READING, Unmatched } from '@rasm/mcp/server';
import { Array, Context, Effect, Formatter, Option, Schema, SchemaGetter, type SchemaIssue, SchemaTransformation, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { Rpc, RpcGroup } from 'effect/rpc';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PATH = '/indesign';

// --- [MODELS] --------------------------------------------------------------------------

const Specifier = Schema.String.annotate({ identifier: 'Specifier', description: "InDesign's path to an object, as results return it" }).pipe(Schema.brand('Specifier'));
const Points = Schema.Finite.annotate({ identifier: 'Points', description: 'Length in points' });
const Box = Schema.Struct(Struct.Record(['top', 'left', 'bottom', 'right'], Points)).annotate({ identifier: 'Box' });
const Sides = Schema.Struct(Struct.Record(['top', 'bottom', 'inside', 'outside'], Points)).annotate({ identifier: 'Sides' });
const Index = Schema.Natural.annotate({ description: 'Zero-based position in the document' });
const Flag = Schema.OptionFromOptionalKey(Schema.Literal(true));
const Reference = Schema.Struct({ type: Schema.String, specifier: Specifier, name: Schema.OptionFromOptionalKey(Schema.String) });
const Field = Schema.Struct({
    name: Schema.String,
    type: Schema.OptionFromOptionalKey(Schema.String),
    description: Schema.OptionFromOptionalKey(Schema.String),
    array: Flag,
    ...Struct.Record(['minimum', 'maximum'], Schema.OptionFromOptionalKey(Schema.Finite)),
    settable: Schema.OptionFromOptionalKey(Schema.Boolean),
    members: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String)),
}).annotate({ identifier: 'Field' });
const Geometry = Schema.Struct({
    bleed: Sides,
    baseline: Schema.Struct({ start: Points, division: Points, relativeTo: Schema.String }),
    pages: Schema.NonEmptyArray(Schema.Struct({ specifier: Specifier, master: Schema.OptionFromOptionalKey(Specifier), bounds: Box, margins: Sides, content: Box, columns: Schema.Array(Box.mapFields(Struct.pick(['left', 'right']))) })),
    items: Schema.Array(
        Reference.pipe(
            Schema.fieldsAssign({
                layer: Schema.String,
                appliedObjectStyle: Schema.String,
                locked: Flag,
                anchored: Flag,
                hidden: Flag,
                parent: Schema.OptionFromOptionalKey(Specifier),
                story: Schema.OptionFromOptionalKey(Specifier),
                label: Schema.OptionFromOptionalKey(Schema.String),
                bounds: Box,
                overflows: Schema.OptionFromOptionalKey(Schema.Boolean),
                effectivePpi: Schema.OptionFromOptionalKey(Schema.Tuple([Schema.Finite, Schema.Finite])),
            }),
        ),
    ),
});
const Target = Schema.TaggedUnion({
    Page: {
        index: Index,
        region: Schema.OptionFromOptionalKey(
            Schema.Struct(Struct.Record(Struct.keys(Box.fields), Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 }))))
                .annotate({ description: 'Part of the page as fractions of its width and height from the top-left corner' })
                .check(Schema.makeFilter((region) => region.top < region.bottom && region.left < region.right, { expected: 'top above bottom and left before right, as fractions of the page' })),
        ),
    },
    Spread: { index: Index },
    Item: { specifier: Specifier, isolate: Flag.annotate({ description: 'Render the item alone: what overlaps it leaves the image and its covered parts show' }) },
}).annotate({ identifier: 'Target' });
const Shot = Schema.Struct({
    image: Image('image/jpeg'),
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
const Peer = Schema.Struct({ version: Schema.String.pipe(Schema.decodeTo(Schema.Int, SchemaTransformation.transform({ decode: (version) => Number.parseInt(version, 10), encode: String }))) });
const Mode = Schema.Literals(['Text', 'Grep', 'Glyph', 'Object']);
const Outcome = Schema.Struct({ corrections: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String)), preview: Schema.OptionFromOptionalKey(Shot) });
const Phase = Schema.Literals(['Idle', 'Held', 'Entered']);
const unit = <P extends Schema.Top>(preview: P) =>
    Activity.mapFields(
        Struct.assign({
            description: Activity.fields.description.annotate({ description: 'Present-progressive sentence shown while the call runs, naming its undo step' }),
            preview: Schema.OptionFromOptionalKey(Schema.Union([Schema.Literal(true), preview]).annotate({ description: 'Runs the call, then undoes every change, returning its result or refusal, with an image of a given target' })),
        }),
    );
const TypeScript = Schema.String.annotate({ description: 'JavaScript or TypeScript function body' }).pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transformEffect((code: string) => Effect.flatMap(Transpile, (strip) => strip(code))), encode: SchemaGetter.passthrough() }));

// --- [ERRORS] --------------------------------------------------------------------------

const Reason = Schema.TaggedUnion({
    Refused: { at: Schema.OptionFromOptionalKey(Schema.String), cause: Schema.Defect() },
    Untranslatable: { at: Schema.String, constructs: Schema.NonEmptyArray(Schema.Struct({ construct: Schema.String, alternative: Schema.OptionFromOptionalKey(Schema.String) })) },
    Unrendered: { at: Schema.String },
    Modal: {},
    Unsettled: {},
});
class InDesignError extends Schema.TaggedError<InDesignError, { readonly brand: unique symbol }>()('InDesignError', { reasons: Schema.NonEmptyArray(Reason) }) {
    static readonly refused = (at: Option.Option<string>) => (cause: unknown) => InDesignError.make({ reasons: [{ _tag: 'Refused', at, cause }] });
    override get message(): string {
        return Array.map(this.reasons, (reason) =>
            Reason.match(reason, {
                Refused: ({ at, cause }) => [...Option.toArray(at), Formatter.format(cause)].join(': '),
                Untranslatable: ({ at, constructs }) => `${at}: InDesign GREP cannot read ${Array.map(constructs, ({ construct, alternative }) => Option.match(alternative, { onNone: () => construct, onSome: (form) => `${construct} (write ${form})` })).join(', ')}`,
                Unrendered: ({ at }) => `${at}: InDesign exported no image`,
                Modal: () => 'A dialog open in InDesign blocks the call: close it, then retry',
                Unsettled: () => 'A preview body returned a promise: work after its first await runs outside the rollback and stays',
            }),
        ).join('\n');
    }
}
const Failure = Schema.Union([InDesignError, Unmatched]);

// --- [SERVICES] ------------------------------------------------------------------------

const Transpile = Context.Reference<(code: string) => Effect.Effect<string, SchemaIssue.Issue>>('@rasm/indesign/contract/Transpile', { defaultValue: () => Effect.succeed });

// --- [RPCS] ----------------------------------------------------------------------------

const Inspect = Rpc.make('inspect', {
    payload: Schema.Struct({
        target: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'A specifier, or the name of a class or enumeration; application state when absent' })),
        fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Fields to read, every field when absent; one that holds a preference object reads that object' })),
    }).annotate({
        description:
            "Reads InDesign. No target: application state with documents, windows, selection, overset stories, preflight problem count, and undo history. A specifier: the object's fields with value, type, valid values, settable flag, the collection holding valid objects, and the value without the object's overrides where they change it; then its collections, and the geometry of a page or spread. A collection specifier: its members. A class name: its fields and methods from InDesign's object model. An enumeration name: its members",
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
        Class: {
            name: Schema.String,
            parents: Schema.Array(Schema.String),
            fields: Schema.Array(Field),
            methods: Schema.Array(
                Field.mapFields(Struct.pick(['name', 'description'])).pipe(
                    Schema.fieldsAssign({
                        parameters: Schema.Array(Field.mapFields(Struct.omit(['settable'])).pipe(Schema.fieldsAssign({ optional: Schema.Boolean }))),
                        returns: Schema.OptionFromOptionalKey(Field.mapFields(Struct.pick(['type', 'array']))),
                    }),
                ),
            ),
        },
        Enumeration: { name: Schema.String, members: Schema.Array(Field.mapFields(Struct.pick(['name', 'description']))) },
    }),
    error: Failure,
})
    .annotateMerge(READING)
    .annotate(Instructions, [
        '- inspect: no target reads application state, first and after each change. A specifier from any result reads that object, and fields limits the read to those fields. Document field viewPreferences holds its units, a collection field gives its collection specifier, and a collection specifier with fields reads those fields of every member.',
    ]);
const Execute = Rpc.make('execute', {
    payload: Schema.Struct({
        ...unit(Target).fields,
        run: Schema.Union([
            Schema.TaggedStruct('Script', { code: TypeScript }).annotate({
                description: [
                    'UXP JavaScript or TypeScript run in InDesign with app, every enumeration and DOM class, and resolve(specifier) in scope; DOM methods take arguments by position.',
                    'A body without top-level await is one undo step undone whole on a throw; with await, work before the first await is that step and later work lands as separate steps a throw keeps; a preview body has no await. A nested app.doScript(fn, ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name) is a savepoint whose throw undoes its own changes.',
                    'Results encode enumerators by name, DOM objects as {type, specifier} with invalid: true once removed, and collections as {type, length}. collection[key] reads as collection.item(key), and a constant name written as a string to an enumeration property becomes its enumerator, both listed in corrections. Collections are not iterable: everyItem().getElements() gives an array. DOM objects and enumerators compare with equals(), and reading a property the object state disallows throws naming it.',
                    '"\\r" ends a paragraph and "\\n" breaks a line inside one; marginPreferences.left is the inside margin on facing pages. Name and label created objects so later calls find them.',
                ].join('\n'),
            }),
            Schema.TaggedStruct('Set', {
                target: Specifier.annotate({ identifier: undefined, description: 'Specifier of the object or text to write, or of a collection to write every member' }),
                properties: Schema.JsonObject.annotate({ description: 'Value per field: a specifier names an object, a nested record writes into a preference object' }),
                add: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Collection field of the target that gains a member written from properties' })),
            }).annotate({ description: "Writes fields of an object, of every member of a collection, or of a member add creates, as one undo step. Each refused field fails with InDesign's message and undoes the call. Returns each field before and after" }),
        ]),
    }).annotate({ description: 'Changes InDesign as one undo step through Set or a Script' }),
    success: Outcome.pipe(Schema.fieldsAssign({ result: Schema.OptionFromOptionalKey(Schema.Json) })),
    error: Failure,
})
    .annotate(Tool.OpenWorld, false)
    .annotate(Instructions, ['- execute: Set writes fields, dialog fields included, and Script runs UXP JavaScript for the rest.']);
const Render = Rpc.make('render', {
    payload: Schema.Struct({ target: Target }).annotate({
        description:
            "Renders a page, page region, spread, or page item of the active document through InDesign's JPEG export at its export resolution, an item capped at its image resolution. Pixel (x, y) lies at bounds.left + x * (bounds.right - bounds.left) / width and bounds.top + y * (bounds.bottom - bounds.top) / height in points. An item renders with what overlaps it unless isolate",
    }),
    success: Shot,
    error: Failure,
})
    .annotateMerge(READING)
    .annotate(Instructions, ['- render: a page, page region, spread, or page item as an image.']);
const FindChange = Rpc.make('find_change', {
    payload: Schema.Struct({
        ...unit(Target).fields,
        mode: Mode,
        find: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find preferences: the pattern and the format criteria" })),
        action: Schema.OptionFromOptionalKey(
            Schema.TaggedUnion({
                Change: { preferences: Schema.JsonObject.annotate({ description: "Fields of the mode's change preferences, written to every match" }) },
                Style: { paragraphStyle: Specifier, characterStyle: Specifier },
            }).annotate({ description: 'Change every match, or add the pattern to the paragraph style as a GREP style applying the character style; absent, the call finds alone' }),
        ),
        options: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find/change options" })),
        scope: Schema.OptionFromOptionalKey(Specifier.annotate({ identifier: undefined, description: 'Specifier of the document, story, text, or page item to search; the active document when absent' })),
        query: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Saved query loaded before find, action, and options apply' })),
        save: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Name to save the query under once the call succeeds' })),
    }).annotate({
        description:
            "Finds, and with action changes, text, GREP, glyph, or objects through InDesign's Find/Change. A JavaScript regular expression is translated to InDesign's GREP with each rewrite listed in corrections, and a construct InDesign rejects fails naming its alternative. action Style adds the pattern to a paragraph style as a GREP style; save stores the query for the Find/Change dialog",
    }),
    success: Outcome.pipe(
        Schema.fieldsAssign({
            find: Schema.JsonObject,
            change: Schema.OptionFromOptionalKey(Schema.JsonObject),
            count: Schema.Struct({ matches: Schema.Int, changes: Schema.Int }),
            matches: Schema.Array(
                Schema.Struct({
                    specifier: Specifier,
                    text: Schema.OptionFromOptionalKey(Schema.String),
                    context: Schema.OptionFromOptionalKey(Schema.String),
                    page: Schema.OptionFromOptionalKey(Schema.String),
                    paragraphStyle: Schema.OptionFromOptionalKey(Specifier),
                }),
            ),
            changes: Schema.Array(Schema.Struct({ specifier: Specifier, before: Schema.String, after: Schema.String })),
        }),
    ),
    error: Failure,
})
    .annotate(Tool.OpenWorld, false)
    .annotate(Instructions, ['- find_change: read indesign://grep/reference before writing GREP, or start from the grep prompt.']);
const Font = Rpc.make('font', {
    payload: { name: Schema.String },
    success: Schema.Struct({
        family: Schema.String,
        style: Schema.String,
        status: Schema.String,
        uses: Schema.Array(Specifier),
        autoLeading: Schema.Finite,
        installed: Schema.OptionFromOptionalKey(
            Schema.Struct({
                postscriptName: Schema.String,
                fullName: Schema.String,
                version: Schema.String,
                type: Schema.String,
                location: Schema.String,
                styles: Schema.Array(Schema.String),
                axes: Schema.Array(Schema.String),
                coordinates: Schema.Array(Schema.Finite),
                allows: Schema.Struct(Struct.Record(['outlines', 'pdfEmbedding', 'editableEmbedding', 'restrictedPrinting'], Schema.Boolean)),
            }),
        ),
    }),
    error: Failure,
})
    .annotate(Surface, { _tag: 'None' })
    .annotate(Instructions, [
        "- font: accepts InDesign's face name, family and style joined by a tab, and adds the face's status (missing or substituted included), its document uses, and faces only InDesign activates, read through a subset at their default instance. InDesign lays out Auto underline (underlineOffset and underlineWeight -9999) at the face underline, else 0.15 em below the baseline at 0.05 em, and Auto strikethrough 1/3 em above the baseline at the underline weight. Text takes features through opentypeFeatures as [tag, 1] pairs and axis values through designAxes in axis order, and opsz follows the point size.",
    ]);
const Subset = Rpc.make('subset', { payload: { postscriptName: Schema.String, text: Schema.String }, success: Schema.String, error: Failure }).annotate(Surface, { _tag: 'None' });
const Menu = Rpc.make('menu', {
    payload: Schema.Struct({ area: Schema.OptionFromOptionalKey(Schema.String) }).annotate({ description: 'Menu actions by id, name, and area, every area or one' }),
    success: Schema.Array(Schema.Struct({ id: Schema.Int, name: Schema.String, area: Schema.String })),
    error: Failure,
}).annotate(Surface, { _tag: 'Resource', uri: 'indesign://menu' });
const Preflight = Rpc.make('preflight', {
    payload: Schema.Struct({ profile: Schema.OptionFromOptionalKey(Schema.String) }).annotate({ description: "Preflight results of the active document under a profile, the document's working profile when absent" }),
    success: Schema.Struct({ profile: Schema.String, rules: Schema.Array(Schema.String), problems: Schema.Array(Schema.Struct({ rule: Schema.String, problem: Schema.String, page: Schema.OptionFromOptionalKey(Schema.String), fix: Schema.OptionFromOptionalKey(Schema.String) })) }),
    error: Failure,
}).annotate(Surface, { _tag: 'Resource', uri: 'indesign://preflight' });
const Queries = Rpc.make('queries', {
    payload: Schema.Struct({ name: Schema.OptionFromOptionalKey(Schema.String) }).annotate({ description: 'Saved Find/Change queries of every mode with their find, change, and options records, every query or one' }),
    success: Schema.Array(Schema.Struct({ name: Schema.String, mode: Mode, origin: Schema.Literals(['user', 'factory']), find: Schema.JsonObject, change: Schema.JsonObject, options: Schema.JsonObject })),
    error: Failure,
}).annotate(Surface, { _tag: 'Resource', uri: 'indesign://grep/query' });
const Dialect = Rpc.make('grep_reference', {
    payload: Schema.Struct({}).annotate({ description: 'Constructs InDesign GREP supports, the forms it reads differently with their working equivalents, Change To grammar, and special-character names' }),
    success: Schema.Json,
    error: Failure,
}).annotate(Surface, { _tag: 'Resource', uri: 'indesign://grep/reference' });
const Prompt = Rpc.make('grep', {
    payload: Schema.Struct({ task: Schema.String, scope: Specifier }).annotate({ description: 'Compose and run an InDesign GREP find/change with the dialect reference and the current find/change preferences' }),
    success: Schema.String,
    error: Failure,
})
    .annotate(Surface, { _tag: 'Prompt' })
    .annotate(Instructions, ['- indesign://menu{/area}, indesign://preflight{/profile}, indesign://grep/query{/name}, indesign://grep/reference: menu actions, preflight results, saved queries, the GREP dialect.']);
const Enter = Rpc.make('enter', { payload: {}, success: Phase }).annotate(Surface, { _tag: 'None' });
const InDesign = RpcGroup.make(Inspect, Execute, Render, FindChange, Font, Subset, Menu, Preflight, Queries, Dialect, Prompt, Enter).annotate(Instructions, ['InDesign is served through its panel, which links when InDesign starts. Measurements are numbers in points.']);

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Box, Enter, Execute, FindChange, Font, InDesign, InDesignError, Inspect, Menu, PATH, Peer, type Phase, Preflight, type Prompt, Queries, Specifier, Subset, Target, Transpile, unit };
