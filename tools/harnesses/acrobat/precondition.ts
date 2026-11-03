import { Function, Schema, Struct } from 'effect';
import { Page, Staged } from '#/acrobat/contract';
import { Application, many, Open, placings } from '#/precondition';

// --- [MODELS] --------------------------------------------------------------------------

const rows = Struct.omit(Staged.fields, ['documents']);

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = Function.constant(
    placings(
        Staged,
        { document: Schema.Struct({ document: many(Page), ...rows }), open: Schema.Struct({ open: many(Open), ...rows }), application: Schema.Struct({ application: Application, ...rows }) },
        {
            document: ({ document, ...rest }) => ({ ...rest, documents: document }),
            open: ({ open, ...rest }) => ({ ...rest, documents: open.map((file) => ({ open: file })) }),
            application: ({ application: _, ...rest }) => ({ ...rest, documents: [] }),
        },
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
