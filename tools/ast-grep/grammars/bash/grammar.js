import bash from 'tree-sitter-bash/grammar.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const expansionText = /(?:[^$"'`\\}]|\\.)+/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const operatorField = ({ type, name }) => type === 'FIELD' && name === 'operator';

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(bash, {
    name: bash.grammar.name,
    supertypes: (_, original) => original.filter(({ name }) => !bash.grammar.inline.includes(name)),
    rules: {
        command_substitution: ($, original) => choice(original, seq(field('operator', choice(...[' ', '\t', '\n', '|'].map((separator) => alias(token(seq('${', repeat('\\\n'), separator)), separator === '|' ? '${|' : '${')))), optional($._terminated_statement), '}')),
        _expansion_expression: ($, original) => ({
            ...original,
            content: seq(original.content.members.find(operatorField), optional($._expansion_value)),
        }),
        _expansion_regex_replacement: ($, original) => ({
            ...original,
            members: original.members.map((member) =>
                member.type === 'CHOICE'
                    ? {
                          ...member,
                          members: member.members.map((alternative) => {
                              const operator = alternative.type === 'SEQ' ? alternative.members.find(operatorField) : undefined;
                              return operator ? seq(operator, optional($._expansion_value)) : alternative;
                          }),
                      }
                    : member,
            ),
        }),
        _expansion_value: ($) => choice($._expansion_piece, alias(seq($._expansion_piece, repeat1($._expansion_piece)), $.concatenation)),
        _expansion_piece: ($) => choice($._primary_expression, alias(token(prec(-1, expansionText)), $.word), prec(-1, alias('$', $.word))),
    },
});
