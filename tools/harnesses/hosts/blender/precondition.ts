import { Schema, Struct } from 'effect';
import { Open } from '#/cases/precondition';
import { Added, type Contract, Document, Placed } from '#/hosts/blender/contract';

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = (survey: typeof Contract.Survey.Type) => {
    const [none, opened, saved] = Document.members;
    return Placed.mapFields(
        Struct.assign({
            document: Schema.optionalKey(Schema.Union([none, opened.mapFields(Struct.assign({ open: Open })), saved])),
            area: Schema.optionalKey(Schema.Struct({ type: Schema.Literals(survey.areas), region: Schema.optionalKey(Schema.Literals(survey.regions)) })),
            add: Schema.optionalKey(Schema.Record(Schema.NonEmptyString, Schema.Struct({ kind: Schema.Literals(survey.collections), properties: Added.fields.properties }))),
            mode: Schema.optionalKey(Schema.Literals(survey.modes)),
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
