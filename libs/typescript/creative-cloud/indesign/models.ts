import { Array, Context, Effect, Formatter, Option, Schema, SchemaGetter, type SchemaIssue, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Specifier = Schema.String.annotate({ identifier: 'Specifier', description: "InDesign's path to an object, as results return it" });
const Reference = Schema.Struct({ type: Schema.String, specifier: Specifier, name: Schema.OptionFromOptionalKey(Schema.String) });
const Field = Schema.Struct({
    name: Schema.String,
    type: Schema.OptionFromOptionalKey(Schema.String),
    description: Schema.OptionFromOptionalKey(Schema.String),
    array: Schema.OptionFromOptionalKey(Schema.Literal(true)),
    ...Struct.Record(['minimum', 'maximum'], Schema.OptionFromOptionalKey(Schema.Finite)),
    settable: Schema.OptionFromOptionalKey(Schema.Boolean),
    members: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String)),
    unavailable: Schema.OptionFromOptionalKey(Schema.Defect()),
}).annotate({ identifier: 'Field' });
const Mode = Schema.Literals(['Text', 'Grep', 'Glyph', 'Object', 'Color']);
const Phase = Schema.Literals(['Idle', 'Held', 'Entered']);
const Description = Schema.String.annotate({ description: 'Present-progressive sentence shown while the call runs, naming its undo step' });
const TypeScript = Schema.String.annotate({ description: 'JavaScript or TypeScript function body' }).pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transformEffect((code: string) => Effect.flatMap(Transpile, (strip) => strip(code))), encode: SchemaGetter.passthrough() }));
const Reason = Schema.TaggedUnion({
    Refused: { at: Schema.OptionFromOptionalKey(Schema.String), cause: Schema.Defect() },
    Modal: {},
});

const Operations = {
    inspect: {
        payload: Schema.Struct({
            target: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'A specifier, or the name of a class or enumeration; application state when absent' })),
            fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Limits object fields or class and enumeration members; collections list references unless fields are requested. Object values return specifiers to inspect further' })),
        }).annotate({
            description:
                "Reads InDesign's native object model without changing it. No target: application state, documents, windows, selection, overset stories, and undo history. A specifier: fields, values, types, valid values, writable fields, and related collections. A collection: its members. A class name: fields and methods from the host dictionary. An enumeration name: its members. Start with Application to discover panels, tools, preferences, menus, and document collections; follow their specifiers and class names, using fields to limit output",
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
                                undo: Schema.Struct({ mode: Schema.String, name: Schema.String, history: Schema.Array(Schema.String) }),
                            }),
                        ),
                    },
                }),
            },
            Object: {
                type: Schema.String,
                specifier: Specifier,
                fields: Schema.Array(Schema.TaggedUnion({ Success: { ...Struct.omit(Field.fields, ['unavailable']), value: Schema.Json }, Failure: { ...Field.fields, unavailable: Schema.Defect() } })),
                collections: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, count: Schema.Int, specifier: Specifier })),
            },
            Items: { specifier: Specifier, total: Schema.Int, items: Schema.Array(Reference.pipe(Schema.fieldsAssign({ values: Schema.JsonObject, unavailable: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Defect())) }))) },
            Class: {
                name: Schema.String,
                parents: Schema.Array(Schema.String),
                fields: Schema.Array(Field),
                methods: Schema.Array(
                    Field.mapFields(Struct.pick(['name', 'description'])).pipe(
                        Schema.fieldsAssign({
                            parameters: Schema.OptionFromOptionalKey(Schema.Array(Field.mapFields(Struct.omit(['settable'])).pipe(Schema.fieldsAssign({ optional: Schema.Boolean })))),
                            returns: Schema.OptionFromOptionalKey(Field.mapFields(Struct.pick(['type', 'array']))),
                        }),
                    ),
                ),
            },
            Enumeration: { name: Schema.String, members: Schema.Array(Field.mapFields(Struct.pick(['name', 'description']))).annotate({ description: 'For Find/Change JSON, use {type: enumeration name, member: member name}; scripts use the native enumeration member' }) },
        }),
    },
    execute: {
        payload: Schema.Struct({
            description: Description,
            run: Schema.TaggedStruct('Script', { code: TypeScript }).annotate({
                description: [
                    'Native UXP JavaScript or TypeScript with app, referenced native enumerations and DOM classes, and resolve(specifier) in scope. Source runs unchanged after TypeScript stripping.',
                    'A synchronous body shares one undo step, rolled back on a throw. Top-level await makes the body asynchronous; a rejection keeps earlier writes. Group later changes in app.doScript(fn, ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name).',
                    'Return values encode enumerators by name, objects as {type, specifier}, and collections as {type, length}. Inspect class names for native fields and method signatures.',
                ].join('\n'),
            }),
        }).annotate({ description: 'Runs native InDesign code to create, change, or read objects through their own APIs' }),
        success: Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) }),
    },
    render: {
        payload: Schema.Struct({ target: Specifier.annotate({ identifier: undefined, description: 'Specifier of a document page, spread, or page item' }) }).annotate({
            description:
                "Exports a complete document page, spread, or page item through InDesign's native PNG export at maximum quality and the host's export resolution. An item shows itself; render its page or spread for surrounding context. Set app.pngExportPreferences.exportResolution for the detail needed. To see application panels, dialogs, pasteboard, or master spreads, use screenshot",
        }),
        success: Schema.Struct({ image: Schema.Uint8Array, width: Schema.Int, height: Schema.Int }),
    },
    find_change: {
        payload: Schema.Struct({
            description: Description,
            mode: Mode,
            find: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find preferences: the pattern and the format criteria" })),
            change: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's change preferences, applied to every match. An empty object applies a saved query's changes; absent, the call only finds" })),
            options: Schema.OptionFromOptionalKey(Schema.JsonObject.annotate({ description: "Fields of the mode's find/change options" })),
            scope: Schema.OptionFromOptionalKey(Specifier.annotate({ identifier: undefined, description: 'Specifier of the document, story, text, or page item to search; the active document when absent' })),
            query: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Saved query loaded before find, change, and options apply' })),
            save: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Name to save the query under once the call succeeds' })),
        }).annotate({
            description: 'Finds or changes native text, GREP, glyph, object, or color matches. Search preferences restore after each call. Values remain literal; use {type, specifier} for native objects and {type, member} for native enumerators. Inspect the class or enumeration for its fields and members',
        }),
        success: Schema.Struct({
            count: Schema.Struct({ matches: Schema.Int, changes: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Native numeric change result, returned by Color operations. Absent for array returns and search-only calls; array length does not count edits' })) }),
            matches: Schema.Array(Schema.Struct({ specifier: Specifier, text: Schema.OptionFromOptionalKey(Schema.String), page: Schema.OptionFromOptionalKey(Schema.String) })).annotate({
                description: 'Native matches with their text before changes; inspect their specifiers for surrounding content. Color returns counts alone',
            }),
        }),
    },
    font: {
        payload: Schema.Struct({ name: Schema.String.annotate({ description: 'Native font name, PostScript name, full name, or font specifier; a family selects its Regular face' }), text: Schema.String }),
        success: Schema.Struct({
            family: Schema.String,
            style: Schema.OptionFromOptionalKey(Schema.String),
            status: Schema.String,
            uses: Schema.Array(Specifier),
            installed: Schema.OptionFromOptionalKey(
                Schema.Struct({
                    postscriptName: Schema.String,
                    fullName: Schema.String,
                    version: Schema.String,
                    type: Schema.String,
                    location: Schema.String,
                    styles: Schema.Array(Schema.String),
                    coordinates: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.Finite)),
                    allows: Schema.Struct(Struct.Record(['outlines', 'pdfEmbedding', 'editableEmbedding', 'restrictedPrinting'], Schema.Boolean)),
                    data: Schema.Uint8Array,
                    subset: Schema.Boolean.annotate({ description: 'The original file was unavailable: metrics describe a native subset at the default instance, without variation axes and with only the requested characters and their features' }),
                }),
            ),
        }),
    },
    menu: {
        payload: Schema.Struct({ area: Schema.OptionFromOptionalKey(Schema.String) }).annotate({ description: 'Menu actions by id, name, and area, every area or one' }),
        success: Schema.Array(Schema.Struct({ id: Schema.Int, name: Schema.String, area: Schema.String })),
    },
    preflight: {
        payload: Schema.Struct({ profile: Schema.OptionFromOptionalKey(Schema.String) }).annotate({ description: "Preflight results of the active document under a profile, the document's working profile when absent" }),
        success: Schema.Struct({
            documentName: Schema.String,
            profileName: Schema.String,
            appliedProfile: Specifier,
            results: Schema.Array(Schema.Struct({ parentNodeID: Schema.Int, errorName: Schema.String, pageNumber: Schema.String, errorInfo: Schema.String, errorDetail: Schema.Array(Schema.Tuple([Schema.String, Schema.String]).annotate({ description: 'Native label and description pair' })) })),
        }),
    },
    queries: {
        payload: Schema.Struct({ name: Schema.OptionFromOptionalKey(Schema.String) }).annotate({ description: 'Lists saved Find/Change queries of every mode; a name includes the native XML containing complete patterns, formatting, effects, and options' }),
        success: Schema.Array(Schema.Struct({ name: Schema.String, mode: Mode, origin: Schema.Literals(['user', 'factory']), content: Schema.OptionFromOptionalKey(Schema.String) })),
    },
};

// --- [ERRORS] --------------------------------------------------------------------------

class InDesignError extends Schema.TaggedError<InDesignError, { readonly brand: unique symbol }>()('InDesignError', { reasons: Schema.NonEmptyArray(Reason) }) {
    static readonly refused = (at: Option.Option<string>) => (cause: unknown) => InDesignError.make({ reasons: [{ _tag: 'Refused', at, cause }] });
    override get message(): string {
        return Array.map(this.reasons, (reason) =>
            Reason.match(reason, {
                Refused: ({ at, cause }) => [...Option.toArray(at), Formatter.format(cause)].join(': '),
                Modal: () => 'A dialog open in InDesign blocks the call: close it, then retry',
            }),
        ).join('\n');
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

const Transpile = Context.Reference<(code: string) => Effect.Effect<string, SchemaIssue.Issue>>('@rasm/indesign/models/Transpile', { defaultValue: () => Effect.succeed });

// --- [EXPORTS] -------------------------------------------------------------------------

export { InDesignError, Operations, Phase, Specifier, Transpile };
