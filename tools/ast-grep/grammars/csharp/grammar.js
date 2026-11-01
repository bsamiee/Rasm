import csharp from 'tree-sitter-c-sharp/grammar.js';

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(csharp, {
    name: csharp.grammar.name,
    conflicts: ($, original) => [...original, [$.lvalue_expression, $._expression_statement_expression]],
    rules: {
        lvalue_expression: ($, original) => choice(original, $.invocation_expression),
    },
});
