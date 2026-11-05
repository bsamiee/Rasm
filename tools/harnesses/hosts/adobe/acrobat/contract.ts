import { Effect, Schema, Struct } from 'effect';
import { hostCase, Outcome, Records, recorded } from '#/cases/results';
import { answers } from '#/cases/schema';
import { JsonObject } from '#/cases/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BOXES = ['Art', 'Bleed', 'Crop', 'Media', 'Trim'] as const;
const QUARTER_TURN = 90;
const FULL_TURN = 360;

// --- [MODELS] --------------------------------------------------------------------------

const Engine = Schema.Literal('javascript').annotate({ description: 'Acrobat JavaScript, the one language do script runs' });
const Rect = Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]).annotate({ description: 'Left, top, right, bottom in default user space points' });
const Page = Schema.Struct({
    width: Schema.Finite.annotate({ description: 'Page width in points, app.newDoc nWidth' }),
    height: Schema.Finite.annotate({ description: 'Page height in points, app.newDoc nHeight' }),
    pages: Schema.Int.check(Schema.isGreaterThan(0))
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed(1)))
        .annotate({ description: 'Page count, newPage adding each page after the first' }),
    rotation: Schema.optionalKey(Schema.Int.check(Schema.isMultipleOf(QUARTER_TURN), Schema.isBetween({ minimum: 0, maximum: FULL_TURN, exclusiveMaximum: true })).annotate({ description: 'setPageRotations over every page, a quarter-turn multiple below a full turn' })),
    boxes: Schema.optionalKey(Schema.Struct(Struct.Record(BOXES, Schema.optionalKey(Rect))).annotate({ description: 'setPageBoxes over every page' })),
}).annotate({ description: 'Document app.newDoc makes, saved as <sandbox>/<index>.pdf before the case' });
const Opened = Schema.Union([Schema.Struct({ open: Schema.NonEmptyString.annotate({ description: 'Absolute path of the case copy app.openDoc opens with a window' }) }), Page]);
const Event = Schema.Union([
    Schema.Struct({ field: Schema.NonEmptyString, trigger: Schema.Literal('Calculate').annotate({ description: 'Fired by calculateNow' }) }),
    Schema.Struct({ field: Schema.NonEmptyString, trigger: Schema.Literals(['Keystroke', 'Validate', 'Format']), value: Schema.Json.annotate({ description: 'Field value write firing the commit Keystroke, Validate, and Format' }) }),
]).annotate({ description: 'Field action of the first document the run source attaches to and fires, in place of a plain call' });
const Staged = Schema.Struct({
    documents: Schema.Array(Opened).annotate({ description: 'Case documents opened with a window in order, the first one the case this, none for an application case' }),
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
});
const Placed = Staged.mapFields(
    Struct.assign({
        sandbox: Schema.String.annotate({ description: 'Case folder under the sandbox exception holding the document copies and every file the case writes until they move to the case folder' }),
    }),
);
const Rooted = Staged.mapFields(
    Struct.assign({
        exception: Schema.NonEmptyString.annotate({ description: 'Absolute-path read-write exception of the app sandbox entitlement the survey states' }),
    }),
);
const HostCase = hostCase({ precondition: Placed, folder: Schema.String.annotate({ description: 'Case folder the sandbox files move to after the case' }), ...Struct.omit(answers, ['presses']) });
const Parameter = Schema.Struct({ name: Schema.String, type: Schema.String, optional: Schema.Boolean });
const Model = Schema.Struct({
    members: Schema.Record(Schema.String, Schema.Array(Schema.String)).annotate({ description: 'Member names each object kind enumerates, keyed by its Object.prototype.toString kind' }),
    signatures: Schema.Record(Schema.String, Schema.Array(Parameter)).annotate({ description: 'acrohelp parameters of each member the engine answers or guards, keyed <Kind>.<member>, a member refusing acrohelp absent and every one of its arguments vetted as a path' }),
    modal: Schema.Array(Schema.String).annotate({ description: 'Menu item names whose dictionary title ends in an ellipsis' }),
}).annotate({ description: 'Object model of the installation' });
const Viewer = Schema.Struct({
    type: Schema.String.annotate({ description: 'app.viewerType' }),
    variation: Schema.String.annotate({ description: 'app.viewerVariation' }),
    language: Schema.String.annotate({ description: 'app.language, the UI language code' }),
    plugIns: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.Finite, loaded: Schema.Boolean, certified: Schema.Boolean })).annotate({ description: 'app.plugIns, deciding which objects and members exist' }),
});
const Install = Schema.Struct({
    sandbox: Schema.NonEmptyString.annotate({ description: 'Absolute-path read-write exception of the app sandbox entitlement, the folder case files pass through' }),
    lockdown: Schema.optionalKey(Schema.String.annotate({ description: 'sha256 of the FeatureLockDown file deciding execMenuItem and launch permissions' })),
}).annotate({ description: 'Install state read after launch that decides answers beside the version' });
const Survey = Schema.Struct({ model: Model, viewer: Viewer, host: Install }).annotate({ description: 'Installation facts the index holds per bundle identifier and version' });
const batch = {
    home: Schema.String.annotate({ description: 'POSIX home folder, found in a host path to read the volume prefix of device-independent paths' }),
    boxes: Schema.Array(Schema.Literals(BOXES)),
    modal: Model.fields.modal,
};
const EngineRequest = Schema.TaggedUnion({ Survey: batch, Case: { ...batch, signatures: Model.fields.signatures, case: HostCase } });
const HostOutcome = Outcome.mapFields(
    Struct.assign({
        ...Outcome.common,
        alerts: recorded(Schema.Struct({ text: Schema.String, title: Schema.optionalKey(Schema.String), checkbox: Schema.optionalKey(Schema.String) })),
        confirms: recorded(Schema.String.annotate({ description: 'app.alert question or closeDoc save prompt the confirms answered' })),
        prompts: recorded(Schema.String.annotate({ description: 'app.response question or popUpMenu items the prompts answered' })),
        files: recorded(Schema.String.annotate({ description: 'Picker member each files answer went to' })),
        windows: recorded(Schema.Json.annotate({ description: 'app.execDialog description, the item ids pressed, and the result' })),
        events: recorded(Schema.Json.annotate({ description: 'Field event the attached source ran in: type, name, rc, value, change, willCommit' })),
        opened: recorded(Schema.Json.annotate({ description: 'Case document starting state: path, page boxes and rotation, info, XMP, annotations, fields, OCGs, bookmarks, data objects' })),
    }),
);
const Recorded = Records.line(HostOutcome);
const Reason = Schema.Struct({ reason: Schema.String.annotate({ description: 'Failure around the case, or the syntax error the eval wrapper caught' }) });
const replied = <A extends Schema.Top>(answer: A) => Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Schema.toCodecJson(Schema.Result(answer, Reason))))).annotate({ description: 'URI-encoded JSON result the engine returns, ASCII through the Mac Roman reply' });
const Surveyed = replied(Schema.Struct({ viewer: Viewer, model: Model.mapFields(Struct.omit(['modal'])) }));
const Answered = replied(Schema.Struct({ outcome: HostOutcome, kept: Schema.Array(Schema.String.annotate({ description: 'Made document file the case never saved over' })) }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Answered, BOXES, Engine, EngineRequest, HostCase, HostOutcome, Install, Model, Page, Placed, Recorded, Rooted, Staged, Survey, Surveyed };
