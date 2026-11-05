import { Schema, Struct } from 'effect';
import { member, Open, placings } from '#/cases/precondition';
import { type Contract, Created, Document, Placed } from '#/hosts/adobe/photoshop/contract';

// --- [MODELS] --------------------------------------------------------------------------

const Made = Created.mapFields(Struct.assign({ mode: member('NewDocumentMode'), initialFill: member('DocumentFill'), bitsPerChannel: member('BitsPerChannelType') }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = (survey: typeof Contract.Survey.Type) =>
    placings({ ...Struct.omit(Placed.fields, ['documents', 'units']), units: Schema.Struct({ ruler: member(survey.units.ruler), type: member(survey.units.type) }) }, { document: Made, open: Open })(Placed, ({ documents, ...rest }) => ({
        ...rest,
        documents: documents.map((row) => (typeof row === 'string' ? Document.cases.Opened.make({ open: row }) : Document.cases.Made.make({ arguments: row }))),
    }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
