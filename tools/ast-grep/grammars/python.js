import { createRequire } from 'node:module';
import { join } from 'node:path';

// --- [CONSTANTS] -----------------------------------------------------------------------

const python = createRequire(import.meta.url)(join(process.cwd(), 'grammar.js'));

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(python, {
    name: 'python',
    rules: {
        import_statement: (_, original) => seq(optional('lazy'), original),
        import_from_statement: (_, original) => seq(optional('lazy'), original),
        keyword_identifier: ($, original) => choice(...original.members, alias('lazy', $.identifier)),
    },
});
