import { Schema, Struct } from 'effect';
import { Added, Document, Placed, type Survey } from '#/blender/contract';
import { Name } from '#/cases';

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = (survey: typeof Survey.Type) => {
    const [none, opened, saved] = Document.members;
    const Open = Name.annotate({ description: '.blend path under the product folder, its copy opened from the case folder' });
    return Placed.mapFields(
        Struct.assign({
            document: Schema.optionalKey(Schema.Union([none, opened.mapFields(Struct.assign({ open: Open })), saved])),
            area: Schema.optionalKey(Schema.Struct({ type: Schema.Literals(survey.areas), region: Schema.optionalKey(Schema.Literals(survey.regions)) })),
            add: Schema.optionalKey(Schema.Record(Name, Schema.Struct({ kind: Schema.Literals(survey.collections), properties: Added.fields.properties }))),
            mode: Schema.optionalKey(Schema.Literals(survey.modes)),
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
