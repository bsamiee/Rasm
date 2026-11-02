import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Expression = Schema.NonEmptyString.annotate({ description: 'Host expression reading doc, app, and made' });
const Unit = Schema.NonEmptyString.annotate({ description: 'MeasurementUnits member name as the engine spells it' });
const shared = {
    units: Schema.optionalKey(Schema.Union([Unit, Schema.Struct({ script: Schema.optionalKey(Unit), horizontal: Schema.optionalKey(Unit), vertical: Schema.optionalKey(Unit) })]).annotate({ description: 'Script and view units, one name setting all three' })),
    labels: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'Document insertLabel rows' })),
    set: Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json])).annotate({ description: 'Member path and value written in order, [*] spreading over a collection' })),
    add: Schema.optionalKey(
        Schema.Array(Schema.Struct({ as: Schema.NonEmptyString, to: Expression, kind: Schema.NonEmptyString.annotate({ description: 'Collection member of the target, as rectangles' }), properties: Schema.optionalKey(Schema.Record(Schema.String, Schema.Json)) })).annotate({
            description: 'Items added and named for later expressions as made.<as>',
        }),
    ),
    steps: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Statements run in order after set and add' })),
    select: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Items app.selection returns, an insertion point included' })),
    menus: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Capture script menu actions and invoke each onInvoke after the run' })),
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const Precondition = Schema.Union([
    Schema.Struct({ open: Schema.NonEmptyString.annotate({ description: 'Document path under the product folder, opened without a window' }), ...shared }),
    Schema.Struct({ document: Schema.optionalKey(Schema.Record(Schema.String, Schema.Json).annotate({ description: 'documents.add properties, {"$": "enum:<Path>"} naming an enumerator' })), ...shared }),
]);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Precondition };
