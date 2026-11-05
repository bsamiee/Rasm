import rust from 'tree-sitter-rust/grammar.js';
import { after, before, children, choiceOf, drop, extend, named, optionalOf, reaches, rewrite, string, symbol, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DOLLAR = /^\\\$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = (rule, matches) => (matches(rule) ? rule : children(rule).reduce((found, part) => found ?? located(part, matches), undefined));
const replace = (matches, replacement) => ($, original) => rewrite(original, (part) => (matches(part) ? replacement($, part) : part));
const repeated = (separator) => (rule) => rule.type === 'REPEAT' && reaches(rule, string(separator));
const closed = (rule) => rule.type === 'SEQ' && symbol('_expression')(rule.members[0]) && symbol('_expression')(rule.members.at(-1));
const prefixed = (rule) => rule.type === 'SEQ' && string('..')(rule.members[0]);
const ranged = ({ value, content }, lifted) => {
    const forms = [
        [closed, lifted],
        [prefixed, (level, member) => prec.right(level + 1, member)],
        [() => true, prec.left],
    ];
    return choice(...content.members.map((member) => forms.find(([matches]) => matches(member))[1](value, member)));
};
const restricted = replace(symbol('_expression'), ($) => $._restricted_expression);
const modifiedBounds = widen(named('trait'), ($) => $.modified_trait_bound);
const boundList = widen(choiceOf(symbol('_type')), ($) => $.modified_trait_bound);
const constness = optional(field('constness', 'const'));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(rust, {
    transforms: [widen(choiceOf(string('f64')), 'f16', 'f128')],
    conflicts: ($, original) => [...original, [$.function_modifiers, $.gen_block], [$.function_modifiers, $.trait_item], [$.function_modifiers, $.impl_item, $.trait_item], [$.function_modifiers, $.modified_trait_bound], [$.remaining_field_pattern, $.return_type_parameters], [$.range_expression, $._restricted_range]],
    rules: {
        _declaration_statement: ($, original) => choice(original, $.trait_alias),
        function_modifiers: widen(choiceOf(string('async')), 'safe', 'gen', 'final'),
        static_item: before(string('static'), optional(choice('safe', 'unsafe'))),
        field_declaration: ($, original) => after(named('type'), optional(seq('=', field('value', $._expression))))($, before(named('name'), optional($.mut_restriction), optional('unsafe'))($, original)),
        struct_item: widen(choiceOf(string(';')), ($) => seq($.where_clause, ';')),
        ordered_field_declaration_list: after(optionalOf(symbol('visibility_modifier')), ($) => optional($.mut_restriction)),
        base_field_initializer: widen(symbol('_expression'), blank()),
        associated_type: ($, original) => prec(-1, seq(optional($.visibility_modifier), before(string(';'), optional(seq('=', field('default', $._type))))($, original))),
        trait_bounds: boundList,
        higher_ranked_trait_bound: widen(named('type'), ($) => $.modified_trait_bound),
        modified_trait_bound: ($) => seq(choice(seq(field('constness', choice('const', seq('[', 'const', ']'), seq('~', 'const'))), optional(field('asyncness', 'async'))), field('asyncness', 'async')), field('type', $._trait_bound_type)),
        _trait_bound_type: ($) => choice(located(rust.grammar.rules.impl_item, named('trait')).content, $.function_type),
        abstract_type: modifiedBounds,
        dynamic_type: modifiedBounds,
        trait_alias: ($) => seq(optional($.visibility_modifier), optional('const'), 'trait', field('name', $._type_identifier), field('type_parameters', optional($.type_parameters)), '=', field('bounds', optional($._trait_alias_bounds)), optional($.where_clause), ';'),
        _trait_alias_bounds: ($) => drop(string(':'))($, boundList($, rust.grammar.rules.trait_bounds)),
        trait_item: ($, original) => before(string('trait'), optional('auto'))($, before(optionalOf(string('unsafe')), optional($.impl_restriction), constness)($, original)),
        impl_restriction: ($) => seq('impl', $._restriction),
        mut_restriction: ($) => seq('mut', $._restriction),
        _restriction: ($) => seq('(', choice('crate', 'self', 'super', seq('in', $._path)), ')'),
        impl_item: before(optionalOf(string('unsafe')), constness),
        range_expression: (_, original) => ranged(original, (value, member) => prec.left(value + 1, member)),
        _restricted_expression: ($) => choice($._expression_except_range, alias($._restricted_range, $.range_expression)),
        _restricted_range: () => ranged(rust.grammar.rules.range_expression, (value, member) => prec.dynamic(1, prec.left(value - 1, member))),
        _condition: restricted,
        match_expression: restricted,
        for_expression: restricted,
        closure_parameters: after(repeated(','), optional(',')),
        field_pattern: before(optionalOf(string('ref')), ($) => repeat($.attribute_item)),
        struct_pattern: widen(named('type'), ($) => alias($.generic_type_with_turbofish, $.generic_type)),
        try_block: ($, original) => replace(symbol('block'), (_, part) => field('body', part))($, after(string('try'), optional(seq('bikeshed', field('type', $._type))))($, original)),
        gen_block: before(string('gen'), optional('async')),
        function_type: widen(named('parameters'), ($) => alias($.return_type_parameters, $.parameters)),
        return_type_parameters: (_) => seq('(', '..', ')'),
        where_predicate: widen(
            named('left'),
            ($) => $.unit_type,
            ($) => $.function_type,
        ),
        _non_special_token: (_, original) => choice(original, '~'),
        metavariable: (_, original) => seq('$', { ...original, value: original.value.replace(DOLLAR, '') }),
    },
});
