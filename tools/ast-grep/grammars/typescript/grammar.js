import javascript from 'tree-sitter-javascript/grammar.js';
import tsx from 'tree-sitter-typescript/tsx/grammar.js';

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(tsx, {
    name: tsx.grammar.name,
    conflicts: ($, original) => [...original, [$.primary_expression, $.using_declaration]],
    rules: {
        _reserved_identifier: (_, original) => choice(original, 'using', 'defer', 'source', 'out'),
        _for_header: () => javascript.grammar.rules._for_header,
        assignment_expression: () => javascript.grammar.rules.assignment_expression,
        declaration: ($, original) => choice(original, $.using_declaration),
        import_statement: ($, original) => choice(original, seq('import', field('phase', choice('defer', 'source')), $.import_clause, $._from_clause, optional($.import_attribute), $._semicolon)),
        type_parameter: (_, original) => seq(optional(choice(seq('in', optional('out')), 'out')), original),
        using_declaration: () => javascript.grammar.rules.using_declaration,
    },
});
