import { Struct } from 'effect';
import { Open, placings } from '#/cases/precondition';
import { type Contract, Page, Rooted, Staged } from '#/hosts/adobe/acrobat/contract';

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = ({ host: { sandbox } }: typeof Contract.Survey.Type) =>
    placings(Struct.omit(Staged.fields, ['documents']), { document: Page, open: Open })(Rooted, ({ documents, ...rest }) => ({
        ...rest,
        exception: sandbox,
        documents: documents.map((row) => (typeof row === 'string' ? { open: row } : row)),
    }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
