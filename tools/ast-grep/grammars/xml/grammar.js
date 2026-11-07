import xml from '@tree-sitter-grammars/tree-sitter-xml/xml/grammar.js';
import { automaton, chain, choiceOf, classes, constants, drop, extend, located, named, optionalOf, pattern, reaches, replace, rewrite, spans, string, symbol, without } from '#/tools/ast-grep/grammars/derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RULES = xml.grammar.rules;
const NAME_START = ':A-Z_a-z\\u{C0}-\\u{D6}\\u{D8}-\\u{F6}\\u{F8}-\\u{2FF}\\u{370}-\\u{37D}\\u{37F}-\\u{1FFF}\\u{200C}-\\u{200D}\\u{2070}-\\u{218F}\\u{2C00}-\\u{2FEF}\\u{3001}-\\u{D7FF}\\u{F900}-\\u{FDCF}\\u{FDF0}-\\u{FFFD}\\u{10000}-\\u{EFFFF}';
const NAME_CHARACTER = `${NAME_START}\\-.0-9\\u{B7}\\u{300}-\\u{36F}\\u{203F}-\\u{2040}`;
const NAME = { type: 'PATTERN', value: `[${NAME_START}][${NAME_CHARACTER}]*`, flags: 'u' };
const WORD = classes({ identifier: [NAME], whitespace: [] });
const NAMING = choiceOf(symbol('Name'));
const NOTATION_NAME = located(RULES.NotationType, NAMING);

// --- [OPERATIONS] ----------------------------------------------------------------------

const quoted = (text) => replace(named('content'), ($, { content: { content } }) => optional(field('value', alias(repeat1(content), $[text]))));
const separated = ({ type, content }) => type === 'REPEAT' && reaches(content, string('|'));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(xml, {
    rules: {
        Name: () => NAME,
        Nmtoken: () => ({ type: 'PATTERN', value: `[${NAME_CHARACTER}]+`, flags: 'u' }),
        _constants: () => constants({ WORD_START: spans(WORD['identifier start']), WORD_CHARACTER: spans(WORD.identifier) }),
        AttValue: quoted('AttText'),
        Attribute: replace(symbol('AttValue'), ($) => quoted('AttText')($, RULES.AttValue)),
        PseudoAtt: replace(symbol('PseudoAttValue'), ($) => quoted('AttText')($, RULES.PseudoAttValue)),
        EntityValue: chain(
            replace(
                ({ type }) => type === 'PATTERN',
                (_, part) => pattern(automaton(choice(part, '<'))),
            ),
            quoted('EntityText'),
        ),
        SystemLiteral: replace(
            ({ type }) => type === 'ALIAS',
            (_, alias) => optional({ ...alias, content: pattern(without(automaton(alias.content), automaton({ type: 'BLANK' }))) }),
        ),
        NotationType: chain(
            drop(NAMING),
            replace(separated, (_, { content }) => seq(NOTATION_NAME, repeat(seq(...content.members, NOTATION_NAME)))),
        ),
        AttlistDecl: ($, original) => rewrite(original, (part) => (symbol('AttDef')(part) ? seq($._S, part) : part)),
        AttDef: (_, original) => rewrite(original, (part) => (part.type === 'SEQ' && part.members.some(NAMING) ? { ...part, members: part.members.slice(part.members.findIndex(NAMING)) } : part)),
        doctypedecl: ($, original) => rewrite(original, (part) => (choiceOf(symbol('_intSubset'))(part) && choiceOf(symbol('_S'))(part) ? $._intSubset : part)),
        _intSubset: chain(
            drop(optionalOf(symbol('_S'))),
            replace(
                ({ type }) => type === 'SEQ',
                (_, { members }) => choice(...members),
            ),
        ),
        CDSect: replace(optionalOf(symbol('CData')), ($) => repeat($.CData)),
        PI: replace(symbol('_pi_content'), (_, part) => optional(part)),
    },
});
