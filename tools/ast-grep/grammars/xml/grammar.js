import xml from '@tree-sitter-grammars/tree-sitter-xml/xml/grammar.js';
import { children, choiceOf, extend, rewrite, symbol } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RULES = xml.grammar.rules;
const NAME_START = ':A-Z_a-z\\u{C0}-\\u{D6}\\u{D8}-\\u{F6}\\u{F8}-\\u{2FF}\\u{370}-\\u{37D}\\u{37F}-\\u{1FFF}\\u{200C}-\\u{200D}\\u{2070}-\\u{218F}\\u{2C00}-\\u{2FEF}\\u{3001}-\\u{D7FF}\\u{F900}-\\u{FDCF}\\u{FDF0}-\\u{FFFD}\\u{10000}-\\u{EFFFF}';
const NAME_CHARACTER = `${NAME_START}\\-.0-9\\u{B7}\\u{300}-\\u{36F}\\u{203F}-\\u{2040}`;

// --- [OPERATIONS] ----------------------------------------------------------------------

const quoted = (text, rule) => choice(...children(rule).map(({ members: [open, { content }, close] }) => seq(open, optional(field('value', alias(repeat1(content.content), text))), close)));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(xml, {
    rules: {
        Name: () => new RegExp(`[${NAME_START}][${NAME_CHARACTER}]*`, 'u'),
        Nmtoken: () => new RegExp(`[${NAME_CHARACTER}]+`, 'u'),
        AttValue: ($, original) => quoted($.AttText, original),
        Attribute: ($, { members: [name, equals] }) => seq(name, equals, quoted($.AttText, RULES.AttValue)),
        PseudoAtt: ($, { members: [name, equals] }) => seq(name, equals, quoted($.AttText, RULES.PseudoAttValue)),
        EntityValue: ($, original) =>
            quoted(
                $.EntityText,
                rewrite(original, (part) => (part.type === 'PATTERN' ? { ...part, value: part.value.replace('<', '') } : part)),
            ),
        SystemLiteral: (_, original) => rewrite(original, (part) => (part.type === 'ALIAS' ? optional({ ...part, content: { ...part.content, value: part.content.value.replace('*', '+') } }) : part)),
        NotationType: (_, { members: [keyword, space, open, lead, name, { content }, , trail, close] }) => seq(keyword, space, open, lead, name, repeat(seq(...content.members, name)), trail, close),
        AttlistDecl: ($, original) => rewrite(original, (part) => (symbol('AttDef')(part) ? seq($._S, part) : part)),
        AttDef: (_, { content }) => prec.right(seq(...content.members.slice(1))),
        doctypedecl: ($, original) => rewrite(original, (part) => (choiceOf(symbol('_intSubset'))(part) && choiceOf(symbol('_S'))(part) ? $._intSubset : part)),
        _intSubset: (_, { content }) => repeat1(choice(...content.members.slice(1))),
        CDSect: ($, { content }) => prec.left(seq(content.members[0], repeat($.CData), content.members[2])),
        PI: ($, { members: [open, target, body, close] }) => seq(open, target, optional(seq(body.members[0].members[0], optional($._pi_content))), close),
    },
});
