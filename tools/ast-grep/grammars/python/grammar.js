import python from 'tree-sitter-python/grammar.js';

// --- [OPERATIONS] ----------------------------------------------------------------------

const lazyImport = (_, original) => seq(optional('lazy'), original);
const comprehension = ($, original) => seq(...original.members.map((member) => (member.type === 'FIELD' ? field(member.name, choice(member.content, member.content.name === 'pair' ? $.dictionary_splat : $.list_splat)) : member)));

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(python, {
    name: python.grammar.name,
    supertypes: (_, original) => original.filter(({ name }) => !python.grammar.inline.includes(name)),
    rules: {
        import_statement: lazyImport,
        import_from_statement: lazyImport,
        keyword_identifier: ($, original) => choice(original, alias('lazy', $.identifier)),
        type_parameter: ($) => seq('[', $._type_parameter, repeat(seq(',', $._type_parameter)), optional(','), ']'),
        _type_parameter: ($) => seq($.type, optional(seq('=', field('default', choice($.expression, $.list_splat))))),
        list_comprehension: comprehension,
        set_comprehension: comprehension,
        generator_expression: comprehension,
        dictionary_comprehension: comprehension,
    },
});
