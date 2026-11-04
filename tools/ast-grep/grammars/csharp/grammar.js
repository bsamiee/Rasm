import csharp from 'tree-sitter-c-sharp/grammar.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const directiveText = /[^\r\n]*/u;

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(csharp, {
    name: csharp.grammar.name,
    conflicts: ($, original) => [...original, [$.lvalue_expression, $._expression_statement_expression], [$.parameter, $.declaration_expression], ...[[], [$.is_expression]].map((extra) => [$.type_pattern, $.declaration_pattern, $._lambda_expression_init, ...extra])],
    rules: {
        _top_level_item: ($, original) => choice(original, $.file_based_directive),
        file_based_directive: (_) => token(seq('#:', directiveText)),
        _reserved_identifier: (_, original) => choice(original, 'allows', 'with'),
        declaration_pattern: ($, original) => choice(original, prec(csharp.grammar.rules.with_expression.value + 1, seq(field('type', $.type), field('name', alias('with', $.identifier))))),
        type_parameter_constraint: (_, original) => choice(original, seq('allows', 'ref', 'struct')),
        _parameter_type_with_modifiers: ($, original) => choice(original, seq(prec.left(csharp.grammar.rules.argument.value + 1, alias(choice('in', 'out'), $.modifier)), field('type', $.type))),
        lvalue_expression: ($, original) => choice(original, $.invocation_expression),
    },
});
