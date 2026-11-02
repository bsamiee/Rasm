import { Effect, Schema, SchemaGetter, Struct } from 'effect';
import { answers, Expression, Name, Reads } from '#/cases';
import { outcome, recorded } from '#/results';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BOXES = ['Art', 'Bleed', 'Crop', 'Media', 'Trim'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Engine = Schema.Literal('javascript').annotate({ description: 'Acrobat JavaScript, the one language do script runs' });
const Rect = Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]).annotate({ description: 'Left, top, right, bottom in default user space points' });
const Page = Schema.Struct({
    width: Schema.Finite.annotate({ description: 'Page width in points, app.newDoc nWidth' }),
    height: Schema.Finite.annotate({ description: 'Page height in points, app.newDoc nHeight' }),
    pages: Schema.Int.check(Schema.isGreaterThan(0))
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed(1)))
        .annotate({ description: 'Page count, newPage adding each page after the first' }),
    rotation: Schema.optionalKey(Schema.Literals([0, 90, 180, 270]).annotate({ description: 'setPageRotations over every page' })),
    boxes: Schema.optionalKey(Schema.Struct(Struct.Record(BOXES, Schema.optionalKey(Rect))).annotate({ description: 'setPageBoxes over every page' })),
}).annotate({ description: 'Document app.newDoc makes, saved as <folder>/<index>.pdf before the case' });
const Opened = Schema.Union([Schema.Struct({ open: Schema.NonEmptyString.annotate({ description: 'Absolute path of the case copy app.openDoc opens with a window' }) }), Page]);
const Event = Schema.Union([
    Schema.Struct({ field: Schema.NonEmptyString, trigger: Schema.Literal('Calculate').annotate({ description: 'Fired by calculateNow' }) }),
    Schema.Struct({ field: Schema.NonEmptyString, trigger: Schema.Literals(['Keystroke', 'Validate', 'Format']), value: Schema.Json.annotate({ description: 'Field value write firing the commit Keystroke, Validate, and Format' }) }),
]).annotate({ description: 'Field action of the first document the run source attaches to and fires, in place of a plain call' });
const rows = {
    fields: Schema.optionalKey(
        Schema.Array(
            Schema.Struct({
                name: Schema.NonEmptyString,
                type: Schema.Literals(['text', 'button', 'checkbox', 'combobox', 'listbox', 'radiobutton', 'signature']),
                page: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)),
                rect: Rect,
                properties: Schema.optionalKey(JsonObject.annotate({ description: 'Field members written in order, value firing the field actions' })),
            }),
        ).annotate({ description: 'addField rows of the first document' }),
    ),
    annots: Schema.optionalKey(Schema.Array(JsonObject).annotate({ description: 'addAnnot properties of the first document, type, page, and rect among them' })),
    info: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'Document info entries of the first document' })),
    metadata: Schema.optionalKey(Schema.String.annotate({ description: 'XMP packet of the first document' })),
    event: Schema.optionalKey(Event),
    steps: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Statements run in order after the rows' })),
};
const Placed = Schema.Struct({ documents: Schema.Array(Opened).annotate({ description: 'Case documents opened with a window in order, the first one the case this, none for an application case' }), ...rows });
const many = <S extends Schema.Top>(item: S) => Schema.Union([Schema.NonEmptyArray(item), item.pipe(Schema.decodeTo(Schema.NonEmptyArray(Schema.toType(item)), { decode: SchemaGetter.transform((one) => [one] as const), encode: SchemaGetter.forbiddenEncoding }))]);
const placing = <S extends Schema.Top & Schema.Decoder<unknown>>(row: S, decode: (held: S['Type']) => typeof Placed.Type) => row.pipe(Schema.decodeTo(Schema.toType(Placed), { decode: SchemaGetter.transform(decode), encode: SchemaGetter.forbiddenEncoding }));
const Condition = Schema.Union([
    placing(Schema.Struct({ open: many(Schema.NonEmptyString.annotate({ description: 'Document path under the product folder, its copy opened with a window' })), ...rows }), ({ open, ...rest }) => ({ ...rest, documents: open.map((file) => ({ open: file })) })),
    placing(Schema.Struct({ document: many(Page), ...rows }), ({ document, ...rest }) => ({ ...rest, documents: document })),
    placing(Schema.Struct({ application: Schema.Literal(true).annotate({ description: 'Application-level case opening no document' }), ...rows }), ({ application: _, ...rest }) => ({ ...rest, documents: [] })),
]);
const Source = Schema.Struct({
    script: Schema.String.annotate({ description: 'Original script path' }),
    code: Schema.String.annotate({ description: 'Kept script lines the engine compiles' }),
    lines: Schema.Array(Schema.Int).annotate({ description: 'Script line of each kept line, body line n at index n - 1' }),
});
const shared = { name: Name, precondition: Placed, read: Reads, folder: Schema.String.annotate({ description: 'Case folder holding document copies and every file the case writes' }), ...Struct.omit(answers, ['presses', 'dialogs']) };
const HostCase = Schema.TaggedUnion({
    Run: { ...shared, source: Source },
    Call: { ...shared, source: Schema.optionalKey(Source), call: Expression, args: Schema.Array(Schema.Json) },
    Probe: { ...shared, evaluate: Expression },
});
const Parameter = Schema.Struct({ name: Schema.String, type: Schema.String, optional: Schema.Boolean });
const Model = Schema.Struct({
    members: Schema.Record(Schema.String, Schema.Array(Schema.String)).annotate({ description: 'Member names each object kind enumerates, keyed by its Object.prototype.toString kind' }),
    signatures: Schema.Record(Schema.String, Schema.Array(Parameter)).annotate({ description: 'acrohelp parameters of each member the engine answers or guards, keyed <Kind>.<member>' }),
    modal: Schema.Array(Schema.String).annotate({ description: 'Menu item names whose dictionary title ends in an ellipsis or that sit in the application menu' }),
}).annotate({ description: 'Object model the index holds per bundle identifier and version' });
const Viewer = Schema.Struct({
    type: Schema.String.annotate({ description: 'app.viewerType' }),
    variation: Schema.String.annotate({ description: 'app.viewerVariation' }),
    language: Schema.String.annotate({ description: 'app.language, the UI language code' }),
    plugIns: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.Finite, loaded: Schema.Boolean, certified: Schema.Boolean })).annotate({ description: 'app.plugIns, deciding which objects and members exist' }),
});
const Host = Schema.Struct({ ...Viewer.fields, lockdown: Schema.optionalKey(Schema.String.annotate({ description: 'sha256 of the FeatureLockDown file deciding execMenuItem and launch permissions' })) }).annotate({ description: 'Instance state read every session that decides answers beside the version' });
const batch = {
    home: Schema.String.annotate({ description: 'POSIX home folder, found in a host path to read the volume prefix of device-independent paths' }),
    boxes: Schema.Array(Schema.Literals(BOXES)),
    modal: Model.fields.modal,
    cases: Schema.Array(HostCase),
};
const Request = Schema.TaggedUnion({ Survey: batch, Cases: { ...batch, signatures: Model.fields.signatures } });
const fields = {
    alerts: recorded(Schema.Struct({ text: Schema.String, title: Schema.optionalKey(Schema.String), checkbox: Schema.optionalKey(Schema.String) })),
    confirms: recorded(Schema.String.annotate({ description: 'app.alert question or closeDoc save prompt the confirms answered' })),
    prompts: recorded(Schema.String.annotate({ description: 'app.response question or popUpMenu items the prompts answered' })),
    files: recorded(Schema.String.annotate({ description: 'Picker member each files answer went to' })),
    windows: recorded(Schema.Json.annotate({ description: 'app.execDialog description, the item ids pressed, and the result' })),
    console: recorded(Schema.String.annotate({ description: 'console.println line the case printed' })),
    activations: recorded(Schema.String.annotate({ description: 'Member reaching another app, the screen, the menus, or persistent state, recorded and never sent' })),
    writes: recorded(Schema.Struct({ member: Schema.String, before: Schema.optionalKey(Schema.Json), after: Schema.Json }).annotate({ description: 'Application member the case changed, restored after it' })),
    events: recorded(Schema.Json.annotate({ description: 'Field event the attached source ran in: type, name, rc, value, change, willCommit' })),
    opened: recorded(Schema.Json.annotate({ description: 'Case document starting state: path, page boxes and rotation, info, XMP, annotations, fields, OCGs, bookmarks, data objects' })),
    written: recorded(Schema.Struct({ file: Schema.String, size: Schema.Int, sha256: Schema.String }).annotate({ description: 'File the case wrote into its folder, PDFs digested without per-save parts' })),
};
const Outcome = outcome(fields);
const Span = Schema.Struct({ start: Schema.Finite, end: Schema.Finite, kept: Schema.Array(Schema.String).annotate({ description: 'Made document files the case never saved over' }) }).annotate({ description: 'Case start and end in epoch milliseconds on the shared machine clock' });
const ran = { viewer: Viewer, outcomes: Schema.Array(Outcome), spans: Schema.Array(Span) };
const Reply = Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Schema.TaggedUnion({ Survey: { ...ran, model: Model.mapFields(Struct.omit(['modal'])) }, Cases: ran, Failed: { reason: Schema.String.annotate({ description: 'Batch failure around the cases' }) } })))).annotate({
    description: 'URI-encoded JSON the engine returns, ASCII through the Mac Roman reply',
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { BOXES, Condition, Engine, fields, Host, HostCase, Model, Outcome, Reply, Request };
