import { Schema, SchemaGetter, Struct } from 'effect';
import { Created, Document, Placed, type Survey } from '#/photoshop/contract';
import { Application, many, placings } from '#/precondition';
import { Enumerated } from '#/results';

// --- [MODELS] --------------------------------------------------------------------------

const Open = Schema.NonEmptyString.annotate({ description: 'Document path under the product folder, its copy opened with a window' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const member = (enumeration: string) =>
    Schema.NonEmptyString.pipe(Schema.decodeTo(Enumerated, { decode: SchemaGetter.transform((name) => ({ $: `enum:${enumeration}.${name}` as const })), encode: SchemaGetter.forbiddenEncoding })).annotate({
        description: `${enumeration} member name the engine reads through $.global, one the host lacks failing the case`,
    });
const Made = Schema.Struct({ ...Created.fields, mode: member('NewDocumentMode'), initialFill: member('DocumentFill'), bitsPerChannel: member('BitsPerChannelType') });
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
