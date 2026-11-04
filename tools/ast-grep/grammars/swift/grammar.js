import swift from 'tree-sitter-swift/grammar.js';

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(swift, {
    name: swift.grammar.name,
    conflicts: ($, original) => original.map((rules) => (rules.some((rule) => rule.name === 'computed_modify') ? [...rules, $.computed_init] : rules)),
    rules: {
        super_expression: (_, original) => original.members[0],
        inheritance_modifier: (_, original) => original.members[0],
        diagnostic: ($, original) => prec(original.value, seq($._hash_symbol, choice(...original.content.members[1].members.map((rule) => rule.members[0])))),
        tuple_expression: (_, original) => choice(original, prec.right(original.value, seq('(', ')'))),
        _simple_user_type: ($, original) => prec.right(original.value, seq(alias($.simple_identifier, $.type_identifier), optional(prec.dynamic(swift.grammar.rules.call_expression.content.value + 1, $.type_arguments)))),
        associatedtype_declaration: ($) =>
            seq(optional($.modifiers), 'associatedtype', field('name', alias($.simple_identifier, $.type_identifier)), optional(seq(':', field('must_inherit', $._type), repeat(seq(',', field('must_inherit', $._type))))), optional($.type_constraints), optional(seq($._equal_sign, field('default_value', $._type)))),
        function_declaration: ($, original) => choice(original, prec.right(seq(alias($._external_function_modifiers, $.modifiers), $._modifierless_function_declaration_no_body, optional(field('body', $.function_body))))),
        _external_function_modifiers: ($) => seq(repeat(swift.grammar.rules.modifiers.content), alias($._external_function_attribute, $.attribute), repeat(swift.grammar.rules.modifiers.content)),
        _external_function_attribute: ($) => seq('@', alias($._external_function_name, $.user_type), '(', optional(seq($._attribute_argument, repeat(seq(',', $._attribute_argument)))), ')'),
        _external_function_name: ($) => alias(choice('_silgen_name', '_extern'), $.type_identifier),
        getter_specifier: ($) => seq(optional($.mutation_modifier), choice('get', '_read', 'borrow', seq('yielding', 'borrow')), optional($._getter_effects)),
        modify_specifier: ($) => seq(optional($.mutation_modifier), choice('_modify', 'mutate', seq('yielding', 'mutate'))),
        protocol_property_requirements: ($) => seq('{', repeat(choice($.getter_specifier, $.setter_specifier, $.modify_specifier)), '}'),
        computed_property: ($) => seq('{', choice(optional($.statements), repeat(choice($.computed_getter, $.computed_setter, $.computed_modify, $.computed_init))), '}'),
        computed_init: ($) => seq(repeat($.attribute), 'init', optional(seq('(', field('name', $.simple_identifier), ')')), $._block),
    },
});
