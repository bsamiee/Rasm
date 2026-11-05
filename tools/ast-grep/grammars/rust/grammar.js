import rust from 'tree-sitter-rust/grammar.js';
import { after, before, chain, choiceOf, drop, extend, located, named, optionalOf, reaches, replace, string, symbol, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DOLLAR = /^\\\$/u;
const CONTEXTUAL = ['safe', 'auto', 'pin', 'builtin'];
const CONTEXTUAL_TYPES = [...CONTEXTUAL, 'reuse', 'raw', 'union', 'default'];
const WHITESPACE = /\p{Pattern_White_Space}/u;
const MACRO_RULES = token(/macro_rules\p{Pattern_White_Space}*!/u);

// --- [OPERATIONS] ----------------------------------------------------------------------

const from = (name, edit) => ($) => edit($, rust.grammar.rules[name]);
const repeated = (separator) => (rule) => rule.type === 'REPEAT' && reaches(rule, string(separator));
const opened = (delimiter) => (rule) => rule.type === 'SEQ' && string(delimiter)(rule.members[0]);
const ranged = ({ value, content }, closed, prefix, open) => {
    const forms = [
        [(rule) => rule.type === 'SEQ' && symbol('_expression')(rule.members[0]) && symbol('_expression')(rule.members.at(-1)), closed],
        [opened('..'), prefix],
        [() => true, open],
    ];
    return choice(...content.members.map((member) => forms.find(([matches]) => matches(member))[1](value, opened('..')(member) ? seq(choice('..', '..='), ...member.members.slice(1)) : member)));
};
const restricted = replace(symbol('_expression'), ($) => $._restricted_expression);
const emptied = (separator) =>
    replace(
        (rule) => rule.type === 'SEQ' && rule.members[1]?.type === 'REPEAT' && reaches(rule.members[1], string(separator)),
        (_, part) => optional(part),
    );
const bounds = widen(
    named('trait'),
    ($) => $.modified_trait_bound,
    ($) => $.negative_trait_bound,
    ($) => $.parenthesized_bound,
    ($) => $.lifetime,
);
const boundList = widen(
    choiceOf(symbol('_type')),
    ($) => $.modified_trait_bound,
    ($) => $.negative_trait_bound,
    ($) => $.parenthesized_bound,
    ($) => $.use_bounds,
);
const pinned = widen(optionalOf(symbol('mutable_specifier')), ($) => seq('pin', choice('const', $.mutable_specifier)));
const guarded = widen(symbol('_pattern'), ($) => $.guard_pattern);
const captured = widen(optionalOf(string('move')), 'use');
const constness = optional(field('constness', 'const'));
const attributed = ($) => repeat($.attribute_item);
const unattributed = drop((rule) => rule.type === 'REPEAT' && symbol('attribute_item')(rule.content));
const underscored = replace(
    ({ type, value }) => type === 'PATTERN' && value.startsWith('u\\{'),
    (_, part) => ({ ...part, value: part.value.replace('a-fA-F', 'a-fA-F_') }),
);
const specialized = optional(choice('default', 'final'));
const aliased = ({ type }) => type === 'ALIAS';
const bodiless = ($, trailing) => seq(repeat(choice($.attribute_item, $.inner_attribute_item)), field('pattern', $.match_pattern), trailing);
const contracted = before(optionalOf(symbol('where_clause')), ($) => optional($.function_contract));
const pathed = replace(symbol('_path'), (_, part) => field('path', part));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(rust, {
    transforms: [widen(choiceOf(string('f64')), 'f16', 'f128')],
    extras: ($, original) => [...original.map((rule) => (rule.type === 'PATTERN' ? WHITESPACE : rule)), $._keyword_prefix],
    externals: ($, original) => [...original, $.frontmatter, $._shebang, $._lone_dollar],
    conflicts: ($, original) => [
        ...original.filter((set) => !(set.some(symbol('array_expression')) || (set.some(symbol('parameters')) && set.some(symbol('_pattern'))))),
        [$.function_modifiers, $.trait_item],
        [$.function_modifiers, $.impl_item],
        [$.function_modifiers, $.impl_item, $.trait_item],
        [$.function_modifiers, $.modified_trait_bound],
        [$.function_modifiers, $.closure_expression],
        [$.function_modifiers, $.gen_block, $.closure_expression],
        [$.remaining_field_pattern, $.return_type_parameters],
        [$.generic_type],
        [$.break_expression],
        [$.function_modifiers, $.parameter],
        [$.generic_type_with_turbofish, $.generic_pattern],
        [$.never_type, $.never_pattern],
        [$.scoped_identifier],
        [$.function_modifiers, $.const_item],
        [$.function_modifiers, $.const_item, $.impl_item],
    ],
    rules: {
        source_file: chain(
            after(optionalOf(symbol('shebang')), ($) => optional($.frontmatter)),
            replace(symbol('shebang'), ($) => alias($._shebang, $.shebang)),
            replace(
                (rule) => rule.type === 'REPEAT' && symbol('_statement')(rule.content),
                ($, part) => choice(part, prec(-1, $._expression)),
            ),
        ),
        _keyword_prefix: () => 'k#',
        attribute: ($, original) => choice(pathed($, original), seq('unsafe', '(', pathed($, original), ')')),
        _declaration_statement: ($, original) => choice(replace(symbol('attribute_item'), (_, part) => prec(1, part))($, original), $.trait_alias, $.delegation_item, $.const_block_item),
        declaration_list: widen(symbol('_declaration_statement'), ($) => $.const_block),
        const_block_item: ($) => seq($.visibility_modifier, $.const_block),
        mod_item: before(string('mod'), optional('unsafe')),
        extern_modifier: widen(symbol('string_literal'), ($) => $.raw_string_literal),
        parameter: before(optionalOf(symbol('mutable_specifier')), optional('const')),
        macro_invocation: ($, original) => choice(widen(named('macro'), located(rust.grammar.rules._pattern, aliased), alias(choice('try', 'reuse'), $.identifier))($, original), seq(field('macro', alias(MACRO_RULES, $.identifier)), alias($.delim_token_tree, $.token_tree))),
        fragment_specifier: widen(choiceOf(string('expr')), 'guard'),
        _reserved_identifier: widen(choiceOf(string('raw')), ...CONTEXTUAL),
        _type_identifier: ($, original) => choice(original, alias(choice(...CONTEXTUAL_TYPES), $.type_identifier)),
        char_literal: underscored,
        escape_sequence: underscored,
        _expression_except_range: widen(
            choiceOf(symbol('return_expression')),
            ($) => $.become_expression,
            ($) => $.yeet_expression,
            ($) => $.attributed_expression,
            ($) => $.builtin_expression,
            ($) => $.move_expression,
            ($) => prec(-1, alias(choice('try', 'reuse'), $.identifier)),
            ($) => prec(rust.grammar.rules.closure_expression.value - 1, alias('async', $.identifier)),
        ),
        become_expression: ($) => prec.left(seq('become', $._expression)),
        yeet_expression: from(
            'return_expression',
            replace(string('return'), () => seq('do', 'yeet')),
        ),
        attributed_expression: ($) => prec(rust.grammar.rules.unary_expression.value, seq(repeat1($.attribute_item), $._expression)),
        builtin_expression: ($) => seq('builtin', '#', field('name', $.identifier), alias($.delim_token_tree, $.token_tree)),
        move_expression: from('parenthesized_expression', before(string('('), 'move')),
        function_modifiers: widen(choiceOf(string('async')), 'safe', 'gen', 'final'),
        function_item: contracted,
        function_signature_item: contracted,
        function_contract: ($) => choice(seq('contract_requires', field('requires', $.block), optional(seq('contract_ensures', field('ensures', $._expression)))), seq('contract_ensures', field('ensures', $._expression))),
        static_item: before(string('static'), optional(choice('safe', 'unsafe'))),
        const_item: chain(
            before(string('const'), specialized),
            widen(named('name'), ($) => $._reserved_identifier),
            after(named('name'), ($) => field('type_parameters', optional($.type_parameters))),
            before(string(';'), ($) => optional($.where_clause)),
        ),
        type_item: before(string('type'), specialized),
        let_declaration: before(string('let'), optional('super')),
        field_declaration: chain(
            before(named('name'), ($) => optional($.mut_restriction), optional('unsafe')),
            after(named('type'), ($) => optional(seq('=', field('value', $._expression)))),
        ),
        struct_item: widen(choiceOf(string(';')), ($) => seq($.where_clause, ';')),
        ordered_field_declaration_list: after(optionalOf(symbol('visibility_modifier')), ($) => optional($.mut_restriction)),
        base_field_initializer: widen(symbol('_expression'), blank()),
        associated_type: ($, original) => prec(-1, seq(optional($.visibility_modifier), before(string(';'), optional(seq('=', field('default', $._type))))($, before(string('type'), specialized)($, original)))),
        delegation_item: ($) =>
            prec.dynamic(
                -1,
                seq(
                    optional($.visibility_modifier),
                    optional('default'),
                    'reuse',
                    choice(field('name', $._delegated_path), seq(field('path', $._delegated_path), 'as', field('name', $.identifier)), seq(field('name', choice($._path, $.bracketed_type, alias($.generic_type_with_turbofish, $.generic_type))), '::', choice('*', field('list', $.use_list))), $._delegated_impl),
                    choice(';', field('body', $.block)),
                ),
            ),
        _delegated_path: ($) => choice($._path, alias($._delegated_function, $.generic_function)),
        _delegated_function: from('generic_function', drop(symbol('field_expression'))),
        _delegated_impl: from(
            'impl_item',
            chain(
                before(optionalOf(string('unsafe')), constness),
                drop(choiceOf(named('body'))),
                replace(named('trait'), (_, { content }) => field('name', content)),
            ),
        ),
        macro_definition: ($, original) =>
            choice(replace(string('macro_rules!'), () => alias(MACRO_RULES, 'macro_rules!'))($, original), seq(optional($.visibility_modifier), 'macro', located(original, named('name')), choice(alias($._macro_signature, $.macro_rule), replace(string(';'), () => choice(',', ';'))($, located(original, opened('{')))))),
        macro_rule: before(named('left'), ($) => optional(seq(optional('unsafe'), choice('attr', 'derive'), field('arguments', $.token_tree_pattern)))),
        _macro_signature: from(
            'macro_rule',
            chain(
                drop(string('=>')),
                replace(symbol('token_tree_pattern'), ($) => alias($._macro_parameters, $.token_tree_pattern)),
            ),
        ),
        _macro_parameters: from('token_tree_pattern', (_, rule) => located(rule, opened('('))),
        type_arguments: chain(
            widen(
                choiceOf(symbol('_literal')),
                ($) => $.negative_literal,
                ($) => $.const_block,
            ),
            emptied(','),
        ),
        const_parameter: widen(
            choiceOf(symbol('_literal')),
            ($) => $.negative_literal,
            ($) => $.const_block,
        ),
        type_binding: chain(
            widen(
                named('type'),
                ($) => $._literal,
                ($) => $.block,
                ($) => $.const_block,
                ($) => $.negative_literal,
            ),
            after(named('name'), optional('::')),
        ),
        type_parameters: emptied(','),
        trait_bounds: ($, original) => prec.right(chain(boundList, after(repeated('+'), optional('+')), emptied('+'))($, original)),
        negative_trait_bound: from('removed_trait_bound', ($, rule) => prec(1, replace(string('?'), (_, part) => ({ ...part, value: '!' }))($, rule))),
        higher_ranked_trait_bound: widen(named('type'), ($) => $.modified_trait_bound),
        modified_trait_bound: ($) => seq(choice(seq(field('constness', choice('const', seq('[', 'const', ']'), seq('~', 'const'))), optional(field('asyncness', 'async'))), field('asyncness', 'async')), field('type', $._trait_bound_type)),
        parenthesized_bound: ($) => seq('(', choice($.higher_ranked_trait_bound, $.modified_trait_bound, $.parenthesized_bound), ')'),
        _trait_bound_type: ($) => choice(located(rust.grammar.rules.impl_item, named('trait')).content, $.function_type),
        bounded_type: (
            $,
            {
                value,
                content: {
                    members: [left, plus, right],
                },
            },
        ) => choice(prec.left(value, seq(left, plus, choice(right, $.higher_ranked_trait_bound, $.modified_trait_bound, $.negative_trait_bound, $.parenthesized_bound))), prec.left(value - 1, seq(left, plus))),
        abstract_type: bounds,
        dynamic_type: bounds,
        _type: widen(
            choiceOf(symbol('never_type')),
            ($) => $.unsafe_binder_type,
            ($) => alias($.generic_type_with_turbofish, $.generic_type),
            ($) => alias(choice('_', 'try'), $.type_identifier),
            ($) => prec(-1, alias('dyn', $.type_identifier)),
            ($) => alias($._empty_primitive_generic, $.generic_type),
            ($) => prec(rust.grammar.rules.bounded_type.value - 1, $.higher_ranked_trait_bound),
        ),
        _empty_primitive_generic: ($) => prec(-1, seq(field('type', located(rust.grammar.rules._type, aliased)), field('type_arguments', alias($._empty_type_arguments, $.type_arguments)))),
        _empty_type_arguments: () => seq('<', '>'),
        for_lifetimes: ($) => prec(-1, seq('for', $.type_parameters)),
        unsafe_binder_type: ($) => seq('unsafe', $.type_parameters, field('type', $._type)),
        parameters: chain(drop(string('_')), replace(optionalOf(symbol('attribute_item')), attributed)),
        reference_type: pinned,
        reference_expression: pinned,
        reference_pattern: pinned,
        _double_reference_pattern: from(
            'reference_pattern',
            replace(string('&'), (_, part) => ({ ...part, value: '&&' })),
        ),
        match_pattern: widen(symbol('_pattern'), ($) => alias($._double_reference_pattern, $.reference_pattern)),
        captured_pattern: replace(symbol('identifier'), ($, part) => choice(part, located(rust.grammar.rules._pattern, aliased), $._reserved_identifier)),
        negative_literal: replace(choiceOf(symbol('integer_literal')), ($) => $._literal),
        ref_pattern: after(string('ref'), ($) => optional(seq('pin', choice('const', $.mutable_specifier)))),
        self_parameter: pinned,
        trait_alias: ($) => seq(optional($.visibility_modifier), optional('const'), 'trait', field('name', $._type_identifier), field('type_parameters', optional($.type_parameters)), '=', field('bounds', optional($._trait_alias_bounds)), optional($.where_clause), ';'),
        _trait_alias_bounds: ($) => drop(string(':'))($, boundList($, rust.grammar.rules.trait_bounds)),
        trait_item: chain(
            before(optionalOf(string('unsafe')), ($) => optional($.impl_restriction), constness),
            before(string('trait'), optional('auto')),
        ),
        impl_restriction: ($) => seq('impl', $._restriction),
        mut_restriction: ($) => seq('mut', $._restriction),
        _restriction: ($) => seq('(', choice('crate', 'self', 'super', seq('in', $._path)), ')'),
        impl_item: before(optionalOf(string('unsafe')), optional('default'), constness),
        range_expression: (_, original) =>
            ranged(
                original,
                (value, member) => prec.left(value + 1, member),
                (value, member) => prec.right(value + 1, member),
                prec.left,
            ),
        _restricted_expression: ($) => choice($._expression_except_range, alias($._restricted_range, $.range_expression)),
        _restricted_range: () =>
            ranged(
                rust.grammar.rules.range_expression,
                (value, member) => prec.left(value + 1, member),
                (value, member) => prec.right(value + 1, member),
                (value, member) => prec.left(value + 2, member),
            ),
        _condition: restricted,
        break_expression: ($) => choice(prec.dynamic(1, prec.left(seq('break', optional($.label), $._expression))), seq('break', optional($.label))),
        continue_expression: (_, original) => prec.right(original.content),
        match_expression: ($, original) =>
            choice(
                restricted($, original),
                from(
                    'field_expression',
                    replace(named('field'), () => seq('match', field('body', $.match_block))),
                )($),
            ),
        match_arm: ($, original) => choice(original, bodiless($, ',')),
        last_match_arm: ($, original) => choice(original, bodiless($, optional(','))),
        for_expression: chain(restricted, after(string('for'), optional('await'))),
        closure_expression: chain(
            before(optionalOf(string('static')), ($) => optional($.for_lifetimes), constness),
            widen(optionalOf(string('async')), 'gen', prec(0, seq('async', 'gen'))),
            captured,
        ),
        closure_parameters: chain(before(choiceOf(symbol('parameter')), attributed), after(repeated(','), optional(','))),
        arguments: unattributed,
        array_expression: unattributed,
        tuple_expression: unattributed,
        field_pattern: chain(
            before(optionalOf(string('ref')), attributed, ($) => optional($.mutable_specifier)),
            widen(
                (rule) => named('name')(rule) && reaches(rule, symbol('_field_identifier')),
                ($) => $.integer_literal,
            ),
            widen(named('pattern'), ($) => $.guard_pattern),
        ),
        struct_pattern: widen(named('type'), ($) => alias($.generic_type_with_turbofish, $.generic_type)),
        scoped_identifier: widen(
            named('name'),
            ($) => $._reserved_identifier,
            ($) => alias('reuse', $.identifier),
        ),
        _path: widen(choiceOf(symbol('_reserved_identifier')), ($) => alias('reuse', $.identifier)),
        scoped_type_identifier_in_expression_position: widen(named('path'), ($) => $.bracketed_type),
        _pattern: widen(
            choiceOf(symbol('ref_pattern')),
            ($) => $.box_pattern,
            ($) => $.never_pattern,
            ($) => $.builtin_expression,
            ($) => prec(-1, alias(choice('try', 'await', 'reuse'), $.identifier)),
        ),
        never_pattern: () => '!',
        box_pattern: from(
            'ref_pattern',
            replace(string('ref'), (_, part) => ({ ...part, value: 'box' })),
        ),
        guard_pattern: from(
            'match_pattern',
            replace(
                optionalOf((part) => reaches(part, string('if'))),
                (_, part) => part.members.find(({ type }) => type !== 'BLANK'),
            ),
        ),
        tuple_pattern: guarded,
        tuple_struct_pattern: guarded,
        slice_pattern: guarded,
        try_block: chain(
            after(string('try'), ($) => optional(seq('bikeshed', field('type', $._type)))),
            replace(symbol('block'), (_, part) => field('body', part)),
        ),
        async_block: captured,
        gen_block: chain(before(string('gen'), optional('async')), captured),
        function_type: chain(
            widen(named('parameters'), ($) => alias($.return_type_parameters, $.parameters)),
            after(named('trait'), optional('::')),
        ),
        return_type_parameters: (_) => seq('(', '..', ')'),
        where_predicate: chain(
            replace(named('left'), ($) => field('left', choice($.lifetime, $._type, $.higher_ranked_trait_bound))),
            before(named('left'), attributed),
        ),
        _token_pattern: ($, original) => choice(original, alias($._lone_dollar, '$')),
        generic_function: widen(symbol('identifier'), located(rust.grammar.rules._expression_except_range, aliased), ($) => $._reserved_identifier),
        unary_expression: replace(choiceOf(string('-')), (_, part) => field('operator', part)),
        binary_expression: replace(string('<='), () => token(prec(2, '<='))),
        _non_special_token: (_, original) => choice(original, '~'),
        _tokens: ($, original) => choice(original, prec(-1, '$'), $.metavariable_expression),
        metavariable_expression: () => seq('$', located(rust.grammar.rules.token_tree, opened('{'))),
        metavariable: (_, original) => seq('$', { ...original, value: original.value.replace(DOLLAR, '') }),
    },
});
