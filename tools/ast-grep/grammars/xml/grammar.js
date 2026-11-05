import xml from '@tree-sitter-grammars/tree-sitter-xml/xml/grammar.js';
import { children, extend } from '../derive.js';

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(xml, {
    rules: {
        AttValue: ($, original) => choice(...children(original).map(({ members: [open, { content }, close] }) => seq(open, optional(field('value', alias(repeat1(content.content), $.AttText))), close))),
    },
});
