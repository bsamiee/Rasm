import swift from 'tree-sitter-swift/grammar.js';
import { after, before, children, extend, named, rewrite, string, symbol, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PREFIXED = ['try_expression', 'await_expression', 'unsafe_expression', 'consume_expression'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const operator = (rule) => (rule.type === 'SEQ' ? rule.members.find((member) => !named('expr')(member)) : children(rule).map(operator).find(Boolean));
const unwrapped = (rule) => (['SEQ', 'CHOICE'].includes(rule.type) && rule.members.length === 1 ? rule.members[0] : rule);
const bare = (_, original) => rewrite(original, unwrapped);
const conditioned = widen(symbol('_expression'), ...PREFIXED.map((name) => ($) => alias($[`_condition_${name}`], $[name])));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(swift, {
    conflicts: ($, original) => original.map((rules) => (rules.some((rule) => rule.name === 'computed_modify') ? [...rules, $.computed_init] : rules)),
    rules: {
        ...Object.fromEntries(PREFIXED.map((name) => [`_condition_${name}`, ($) => seq(operator(swift.grammar.rules[name]), field('expr', $._expression))])),
        _if_condition_sequence_item: conditioned,
        _if_let_binding: conditioned,
        switch_statement: conditioned,
        function_type: after(symbol('_arrow_operator'), ($) => optional($.parameter_modifiers)),
        _annotated_inheritance_specifier: before(symbol('inheritance_specifier'), ($) => optional(alias('nonisolated', $.member_modifier))),
        equality_constraint: widen(named('must_equal'), ($) => $.integer_literal),
        _attribute_argument: ($, original) => choice(original, alias($._bodyless_function_declaration, $.function_declaration), $.init_declaration, $.property_declaration, $.type_constraints, seq(optional(seq($.simple_identifier, ':')), 'borrow', $.simple_identifier)),
        module_selector: () => token(seq(choice(...swift.grammar.rules.simple_identifier.members.filter(({ type }) => type === 'PATTERN')), '::')),
        _selected_identifier: ($) => seq($.module_selector, $.simple_identifier),
        _expression: widen(symbol('simple_identifier'), ($) => alias($._selected_identifier, $.identifier)),
        navigation_suffix: before(named('suffix'), ($) => optional($.module_selector)),
        modifiers: ($, original) => seq(original, repeat(seq($.directive, optional(original)))),
        super_expression: bare,
        inheritance_modifier: bare,
        diagnostic: bare,
        tuple_expression: (_, original) => choice(original, prec.right(original.value, seq('(', ')'))),
        _simple_user_type: ($, original) => prec.right(original.value, seq(optional($.module_selector), alias($.simple_identifier, $.type_identifier), optional(prec.dynamic(swift.grammar.rules.call_expression.content.value + 1, $.type_arguments)))),
        associatedtype_declaration: after(named('must_inherit'), ($) => repeat(seq(',', field('must_inherit', $._type)))),
        function_declaration: ($, original) => choice(original, prec.right(seq(alias($._external_function_modifiers, $.modifiers), $._modifierless_function_declaration_no_body, optional(field('body', $.function_body))))),
        _external_function_modifiers: ($) => seq(repeat(swift.grammar.rules.modifiers.content), alias($._external_function_attribute, $.attribute), repeat(swift.grammar.rules.modifiers.content)),
        _external_function_attribute: ($) => seq('@', alias($._external_function_name, $.user_type), '(', optional(seq($._attribute_argument, repeat(seq(',', $._attribute_argument)))), ')'),
        _external_function_name: ($) => alias(choice('_silgen_name', '_extern'), $.type_identifier),
        getter_specifier: widen(string('get'), '_read', 'borrow', seq('yielding', 'borrow')),
        modify_specifier: widen(string('_modify'), 'mutate', seq('yielding', 'mutate')),
        protocol_property_requirements: widen(symbol('setter_specifier'), ($) => $.modify_specifier),
        computed_property: widen(symbol('computed_modify'), ($) => $.computed_init),
        computed_init: ($) => seq(repeat($.attribute), 'init', optional(seq('(', field('name', $.simple_identifier), ')')), $._block),
    },
});
