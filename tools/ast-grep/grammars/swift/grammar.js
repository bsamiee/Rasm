import swift from 'tree-sitter-swift/grammar.js';
import { after, any, before, chain, children, drop, extend, located, named, optionalOf, replace, rewrite, string, symbol, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PREFIXED = ['try_expression', 'await_expression', 'unsafe_expression', 'consume_expression'];
const MEMBER_KEYWORDS = ['try', 'await', 'as', 'is', 'self', 'unsafe'];
const RESERVED_MODIFIERS = new Set(['class', 'fileprivate', 'inout', 'internal', 'private', 'public', 'static']);
const LIFETIME_DEPENDENCE = ['borrow', 'copy', '&'];
const MULTI_LINE_TEXT = /(?:[^\\"]|"[^\\"]|""[^\\"])+/u;
const WORD = /^\p{XID_Start}\p{XID_Continue}*$/u;
const IDENTIFIER_CONTINUE = '[_\\p{XID_Continue}';
const EFFECTS_ARGUMENT = /[^()]+/u;
const REGEX_NESTING = 3;
const REGEX_TEXT = String.raw`[^()/\n\\]`;
const REGEX_EDGE = String.raw`[^ \t()/\n\\]`;
const MULTI_LINE_DIAGNOSTIC = /(?:error|warning)\s*\(\s*#*"""(?:[^"]|"[^"]|""[^"])*"""#*\s*\)/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const operator = (rule) => (rule.type === 'SEQ' ? rule.members.find((member) => !named('expr')(member)) : children(rule).map(operator).find(Boolean));
const bare = (_, original) => rewrite(original, (rule) => (['SEQ', 'CHOICE'].includes(rule.type) && rule.members.length === 1 ? rule.members[0] : rule));
const conditioned = widen(symbol('_expression'), ...PREFIXED.map((name) => ($) => alias($[`_condition_${name}`], $[name])));
const dollared = (rule) => rewrite(rule, (part) => (part.type === 'PATTERN' ? { ...part, value: part.value.replace(IDENTIFIER_CONTINUE, `[$${IDENTIFIER_CONTINUE.slice(1)}`) } : part));
const heads = (rule) => ({ STRING: () => [rule.value], SYMBOL: () => heads(swift.grammar.rules[rule.name]), SEQ: () => heads(rule.members[0]) })[rule.type]?.() ?? children(rule).flatMap(heads);
const selected = after(symbol('_hash_symbol'), ($) => optional($.module_selector));
const nonisolated = ({ type, members }) => type === 'SEQ' && string('nonisolated')(members[0]);
const returning = after(symbol('_arrow_operator'), ($) => optional($.parameter_modifiers));
const initialized = widen(named('value'), ($) => alias($._function_type_expression, $.function_type));
const ownership = widen(symbol('mutation_modifier'), ($) => alias($._parameter_ownership_modifier, $.mutation_modifier));
const regexAtom = (text, depth) => [String.raw`\\.`, String.raw`\[[^\]\n]*\]`, text, ...(depth > 0 ? [String.raw`\((?:${regexAtom(REGEX_TEXT, depth - 1)})*\)`] : [])].join('|');
const bodiless = replace(any(named('body'), symbol('computed_property')), ($, part) => optional(choice(part, $._function_signature_end)));
const keyword = (word) => replace(string(word), ($) => alias(word, $[`${word}_keyword`]));
const operatorName = replace(symbol('_referenceable_operator'), ($, part) => alias(part, $.referenceable_operator));
const rangeOperator = widen(
    symbol('_referenceable_operator'),
    ($) => alias($._three_dot_operator, $.referenceable_operator),
    ($) => alias($._open_ended_range_operator, $.referenceable_operator),
);
const declaring = (kind) => replace(symbol('simple_identifier'), ($, part) => alias(part, $[kind]));
const typeNamed = replace(
    ({ type, value }) => type === 'ALIAS' && value === 'type_identifier',
    ($, { content }) => alias(content, $.type_name),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(swift, {
    conflicts: ($, original) => [
        ...original.map((rules) => (rules.some((rule) => rule.name === 'computed_modify') ? [...rules, $.computed_init] : rules)),
        [$.type_modifiers],
        [$.function_type, $._function_type_expression],
        [$._unary_expression, $.switch_statement],
        [$._primary_expression, $._attribute_argument],
        [$._consume_operator, $._attribute_argument],
        [$._contextual_simple_identifier, $._consume_operator, $._attribute_argument],
        [$.tuple_type, $.tuple_expression, $._tuple_pattern],
        [$._locally_permitted_modifier, $._parameter_ownership_modifier],
        [$._multiplicative_operator, $._prefix_unary_operator],
        [$._contextual_simple_identifier, $._nonsending_modifier, $._isolation_modifier],
        [$.type_modifiers, $.parameter_modifier],
        [$._nonsending_modifier, $._isolation_modifier],
        ...['_isolation_modifier', '_nonsending_modifier', 'member_modifier', 'function_modifier', 'mutation_modifier', 'property_modifier', 'inheritance_modifier', 'ownership_modifier'].map((name) => [$._contextual_simple_identifier, $[name]]),
    ],
    externals: ($, original) => [...original, $._postfix_operator_custom, $._raw_string_open, $._raw_string_close, $._function_signature_end, $._trailing_constraint_comma],
    rules: {
        raw_string_literal: ($) => seq(alias($._raw_string_open, '#"'), repeat(seq(optional(field('text', $.raw_str_part)), field('interpolation', $.raw_str_interpolation))), optional(field('text', $.raw_str_end_part)), alias($._raw_string_close, '"#')),
        multi_line_str_text: () => MULTI_LINE_TEXT,
        _oneline_regex_literal: replace(
            ({ type }) => type === 'PATTERN',
            (_, part) => ({ ...part, value: `(?:${regexAtom(REGEX_EDGE, REGEX_NESTING)})(?:(?:${regexAtom(REGEX_TEXT, REGEX_NESTING)})*(?:${regexAtom(REGEX_EDGE, REGEX_NESTING)}))?` }),
        ),
        ...Object.fromEntries(PREFIXED.map((name) => [`_condition_${name}`, ($) => seq(operator(swift.grammar.rules[name]), field('expr', $._expression))])),
        _if_condition_sequence_item: conditioned,
        _if_let_binding: conditioned,
        where_clause: conditioned,
        _for_statement_collection: chain(
            drop(({ type }) => type === 'ALIAS'),
            conditioned,
        ),
        for_statement: widen(named('item'), ($) => alias($._case_prefixed_pattern, $.pattern)),
        _case_prefixed_pattern: ($) => seq('case', choice($.wildcard_pattern, $._tuple_pattern, seq('is', $._type)), optional($._quest)),
        switch_entry: after(string('fallthrough'), ($) => optional($._semi)),
        switch_statement: chain(
            conditioned,
            widen(symbol('switch_entry'), ($) => seq($.directive, optional(seq($._semi, optional($.statements))))),
        ),
        catch_block: replace(named('error'), ($) => {
            const item = field('error', alias($._binding_pattern_no_expr, $.pattern));
            return optional(seq(item, repeat(seq(optional($.where_clause), ',', item))));
        }),
        prefix_expression: widen(string('async'), ...MEMBER_KEYWORDS),
        _case_pattern: widen(symbol('simple_identifier'), ($) => alias(choice(...new Set(MEMBER_KEYWORDS).difference(new Set(heads(swift.grammar.rules._contextual_simple_identifier)))), $.simple_identifier)),
        _tuple_pattern: (_, original) => choice(original, seq('(', ')')),
        _prefix_unary_operator: widen(symbol('custom_operator'), ($) => alias(located(swift.grammar.rules._multiplicative_operator, ({ type }) => type === 'ALIAS').content, $.custom_operator)),
        custom_operator: (_, original) => choice(original, '..'),
        _postfix_unary_operator: widen(symbol('bang'), ($) => alias($._postfix_operator_custom, $.custom_operator)),
        function_type: returning,
        _function_type_expression: ($) => replace(named('params'), ($$) => field('params', $$.tuple_type))($, returning($, swift.grammar.rules.function_type)),
        tuple_type_item: chain(
            widen(named('type'), ($) => $.wildcard_pattern),
            after(named('type'), ($) => optional($._three_dot_operator)),
        ),
        type_pack_expansion: after(string('repeat'), ($) => optional(choice($.parameter_modifiers, $.type_modifiers))),
        bracket_qualified_type: widen(symbol('array_type'), ($) => seq('(', $.type_parameter_pack, ')')),
        type_modifiers: widen(symbol('attribute'), ($) => alias($._nonsending_modifier, $.member_modifier)),
        _nonsending_modifier: () => seq('nonisolated', '(', 'nonsending', ')'),
        _contextual_simple_identifier: (_, original) => choice(original, ...new Set(heads(swift.grammar.rules._modifier).filter((word) => WORD.test(word) && !RESERVED_MODIFIERS.has(word))).difference(new Set(heads(original)))),
        _locally_permitted_modifier: ($, original) => choice(original, alias($._isolation_modifier, $.member_modifier), alias('_const', $.parameter_modifier), alias('_local', $.member_modifier)),
        member_modifier: drop(nonisolated),
        _isolation_modifier: () => located(swift.grammar.rules.member_modifier, nonisolated),
        _parameter_ownership_modifier: (_, original) => choice(original, '__owned', '__shared', '__consuming', '_const'),
        _annotated_inheritance_specifier: before(symbol('inheritance_specifier'), ($) => optional(alias('nonisolated', $.member_modifier))),
        _value_generic_argument: ($) => choice($.integer_literal, alias(token(seq('-', swift.grammar.rules.integer_literal.content)), $.integer_literal)),
        type_arguments: replace(symbol('integer_literal'), ($) => $._value_generic_argument),
        equality_constraint: chain(
            widen(named('must_equal'), ($) => $._value_generic_argument),
            widen(named('constrained_type'), ($) => $._value_generic_argument),
        ),
        _attribute_argument: ($, original) => {
            const name = choice($.simple_identifier, $.self_expression);
            return choice(original, alias($._bodyless_function_declaration, $.function_declaration), $.init_declaration, $.subscript_declaration, $.property_declaration, $.type_constraints, prec.dynamic(1, choice(seq(name, ':', optional(choice(...LIFETIME_DEPENDENCE)), name), seq(choice(...LIFETIME_DEPENDENCE), name))));
        },
        simple_identifier: (_, original) => dollared(original),
        module_selector: () => token(seq(choice(...dollared(swift.grammar.rules.simple_identifier).members.filter(({ type }) => type === 'PATTERN')), '::')),
        _selected_identifier: ($) => seq($.module_selector, $.simple_identifier),
        _expression: widen(symbol('simple_identifier'), ($) => alias($._selected_identifier, $.identifier)),
        _expression_with_willset_didset: initialized,
        _expression_without_willset_didset: initialized,
        identifier: after(symbol('_dot'), ($) => optional($.module_selector)),
        navigation_suffix: before(named('suffix'), ($) => optional($.module_selector)),
        suppressed_constraint: ($, original) => prec.right(swift.grammar.rules._expression.value, widen(named('suppressed'), ($$) => alias($$._qualified_user_type, $$.user_type))($, before(named('suppressed'), ($$) => optional($$.module_selector))($, original.content))),
        _qualified_user_type: ($) => prec.right(seq(alias($.simple_identifier, $.type_identifier), repeat1(seq($._dot, $._simple_user_type)))),
        macro_invocation: selected,
        modifiers: ($, original) => seq(original, repeat(seq($.directive, optional(original)))),
        _type_level_declaration: ($, original) => choice(original, $._freestanding_declaration),
        _protocol_member_declaration: ($, original) => choice(original, $._freestanding_declaration, $.class_declaration, $.protocol_declaration),
        _freestanding_declaration: ($) => choice($.diagnostic, $.macro_invocation, alias($._modified_macro_invocation, $.macro_invocation)),
        enum_class_body: (_, original) => rewrite(original, (rule) => (rule.type === 'REPEAT' ? repeat(seq(rule.content, optional(';'))) : rule)),
        enum_type_parameters: widen(symbol('simple_identifier'), ($) => alias(choice('some', 'any'), $.simple_identifier)),
        enum_entry: chain(drop(string(';')), declaring('case_name')),
        _class_member_separator: (_, original) => repeat1(original),
        _modifierless_typealias_declaration: chain(
            after(named('value'), ($) => optional($.type_constraints)),
            drop(symbol('_equal_sign')),
            replace(named('value'), ($, part) => optional(seq($._equal_sign, part))),
        ),
        _global_declaration: ($, original) => choice(original, alias($._modified_macro_invocation, $.macro_invocation)),
        _modified_macro_invocation: ($) => before(symbol('_hash_symbol'), $.modifiers)($, selected($, swift.grammar.rules.macro_invocation)),
        super_expression: bare,
        inheritance_modifier: bare,
        diagnostic: chain(
            bare,
            widen(({ type, value }) => type === 'PATTERN' && value.startsWith('warning'), MULTI_LINE_DIAGNOSTIC),
        ),
        _compilation_condition: chain(
            drop(({ type, members }) => type === 'SEQ' && string('(')(members[1]) && !members.some(symbol('_comparison_operator'))),
            widen(symbol('boolean_literal'), ($) =>
                seq($.simple_identifier, token.immediate('('), choice(seq($.simple_identifier, repeat(seq('.', $.simple_identifier))), $.line_string_literal), optional(seq(',', $.simple_identifier, ':', choice($.line_string_literal, seq($.integer_literal, repeat(seq('.', $.integer_literal)))))), ')'),
            ),
        ),
        tuple_expression: ($, original) => choice(original, prec.right(original.value, seq('(', ')')), prec.right(original.value, seq('(', optional($.type_modifiers), field('value', alias($._function_type_expression, $.function_type)), ')'))),
        _simple_user_type: ($, original) => prec.right(original.value, seq(optional($.module_selector), alias($.simple_identifier, $.type_identifier), optional(prec.dynamic(swift.grammar.rules.call_expression.content.value + 1, $.type_arguments)))),
        associatedtype_declaration: chain(
            drop(optionalOf(symbol('type_constraints'))),
            after(
                optionalOf((rule) => located(rule, named('default_value')) !== undefined),
                ($) => optional($.type_constraints),
            ),
            after(named('must_inherit'), ($) => repeat(seq(',', field('must_inherit', $._type)))),
        ),
        type_constraints: replace(optionalOf(string(',')), ($) => optional(alias($._trailing_constraint_comma, ','))),
        init_declaration: chain(keyword('init'), bodiless),
        subscript_declaration: chain(keyword('subscript'), bodiless),
        _non_constructor_function_decl: chain(rangeOperator, operatorName, declaring('function_name')),
        macro_declaration: declaring('function_name'),
        _modifierless_class_declaration: typeNamed,
        protocol_declaration: typeNamed,
        operator_declaration: chain(rangeOperator, operatorName),
        function_declaration: bodiless,
        deinit_declaration: chain(keyword('deinit'), bodiless),
        getter_specifier: chain(ownership, widen(string('get'), '_read', 'borrow', seq('yielding', 'borrow'), 'unsafeAddress')),
        modify_specifier: chain(ownership, widen(string('_modify'), 'mutate', seq('yielding', 'mutate'), 'unsafeMutableAddress')),
        protocol_property_requirements: chain(
            widen(symbol('setter_specifier'), ($) => $.modify_specifier),
            before(
                ({ type, members }) => type === 'CHOICE' && members.some(symbol('getter_specifier')),
                ($) => repeat($.attribute),
            ),
        ),
        computed_property: widen(symbol('computed_modify'), ($) => $.computed_init),
        computed_init: ($) => seq(repeat($.attribute), 'init', optional(seq('(', field('name', $.simple_identifier), ')')), optional($._block)),
        _function_value_parameter: replace(optionalOf(symbol('attribute')), ($) => repeat($.attribute)),
        attribute: ($, original) => choice(replace(string(','), () => choice(',', ';'))($, original), seq('@', alias($._effects_attribute_name, $.user_type), '(', EFFECTS_ARGUMENT, ')')),
        _effects_attribute_name: ($) => alias('_effects', $.type_identifier),
        parameter_modifier: widen(symbol('_parameter_ownership_modifier'), ($) => $._nonsending_modifier),
    },
});
