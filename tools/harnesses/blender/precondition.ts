import { Schema } from 'effect';
import { Placed, type Survey } from '#/blender/contract';
import { Name } from '#/cases';

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = (survey: typeof Survey.Type) => {
    const [none, opened, saved] = Placed.fields.document.schema.members;
    const Open = Name.annotate({ description: '.blend path under the product folder, its copy opened from the case folder' });
    return Schema.Struct({
        ...Placed.fields,
        document: Schema.optionalKey(Schema.Union([none, opened.mapFields((fields) => ({ ...fields, open: Open })), saved])),
        area: Schema.optionalKey(Schema.Struct({ type: Schema.Literals(survey.areas), region: Schema.optionalKey(Schema.Literals(survey.regions)) })),
        add: Schema.optionalKey(Schema.Record(Name, Schema.Struct({ kind: Schema.Literals(survey.collections), properties: Placed.fields.add.schema.value.fields.properties }))),
        mode: Schema.optionalKey(Schema.Literals(survey.modes)),
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
