import { Schema, Struct } from 'effect';
import { Page, Rooted, Staged, type Survey } from '#/acrobat/contract';
import { Application, many, Open, placings } from '#/precondition';

// --- [MODELS] --------------------------------------------------------------------------

const rows = Struct.omit(Staged.fields, ['documents']);
const Sources = { document: Schema.Struct({ document: many(Page), ...rows }), open: Schema.Struct({ open: many(Open), ...rows }), application: Schema.Struct({ application: Application, ...rows }) };

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = ({ host: { sandbox } }: typeof Survey.Type) =>
    placings(Rooted, Sources, {
        document: ({ document, ...rest }) => ({ ...rest, exception: sandbox, documents: document }),
        open: ({ open, ...rest }) => ({ ...rest, exception: sandbox, documents: open.map((file) => ({ open: file })) }),
        application: ({ application: _, ...rest }) => ({ ...rest, exception: sandbox, documents: [] }),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
