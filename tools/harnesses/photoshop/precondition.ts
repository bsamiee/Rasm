import { Schema, Struct } from 'effect';
import { Created, Document, Placed, type Survey } from '#/photoshop/contract';
import { Application, many, member, Open, placings } from '#/precondition';

// --- [MODELS] --------------------------------------------------------------------------

const Made = Schema.Struct({ ...Created.fields, mode: member('NewDocumentMode'), initialFill: member('DocumentFill'), bitsPerChannel: member('BitsPerChannelType') });

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = (survey: typeof Survey.Type) => {
    const rows = { ...Struct.omit(Placed.fields, ['documents', 'units']), units: Schema.Struct({ ruler: member(survey.units.ruler), type: member(survey.units.type) }) };
    return placings(
        Placed,
        { document: Schema.Struct({ document: many(Made), ...rows }), open: Schema.Struct({ open: many(Open), ...rows }), application: Schema.Struct({ application: Application, ...rows }) },
        {
            document: ({ document, ...rest }) => ({ ...rest, documents: document.map((held) => Document.cases.Made.make({ arguments: held })) }),
            open: ({ open, ...rest }) => ({ ...rest, documents: open.map((file) => Document.cases.Opened.make({ open: file })) }),
            application: ({ application: _, ...rest }) => ({ ...rest, documents: [] }),
        },
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
