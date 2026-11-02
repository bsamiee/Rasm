import { Schema, Struct } from 'effect';
import { Page, Staged } from '#/acrobat/contract';
import { Application, many, placings } from '#/precondition';

// --- [MODELS] --------------------------------------------------------------------------

const Open = Schema.NonEmptyString.annotate({ description: 'Document path under the product folder, its copy opened with a window' });
const rows = Struct.omit(Staged.fields, ['documents']);

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = () =>
    placings(
        Staged,
        { document: Schema.Struct({ document: many(Page), ...rows }), open: Schema.Struct({ open: many(Open), ...rows }), application: Schema.Struct({ application: Application, ...rows }) },
        {
            document: ({ document, ...rest }) => ({ ...rest, documents: document }),
            open: ({ open, ...rest }) => ({ ...rest, documents: open.map((file) => ({ open: file })) }),
            application: ({ application: _, ...rest }) => ({ ...rest, documents: [] }),
        },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
