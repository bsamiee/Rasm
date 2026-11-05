import { Predicate, Record } from 'effect';
import csharp from 'tree-sitter-c-sharp/grammar.js';
import { after, any, children, choiceOf, extend, named, optionalOf, reaches, replace, rewrite, string, symbol, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DIRECTIVE_SEPARATORS = { sdk: '@', package: '@', property: '=', project: '', ref: '', include: '', exclude: '' };
const METADATA_DIRECTIVES = new Set(['package', 'project', 'ref', 'include']);
const DIRECTIVE_SPACE = /[\t\v\f\p{Zs}\u{FEFF}]+/u;
const QUOTED = /(?:[^"\\\r\n]|\\[^\r\n])+/u;
const METADATA_NAME = /[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}.-]*/u;
const BARE = { '@': /[^\s"@]+/u, '=': /[^\s"=]+/u, '': /[^\s"]+/u };
const LINE_REST = /[^\s](?:[^\r\n]*[^\s])?/u;
const CONDITIONAL_LISTS = {
    argument_list: '_in_argument_list',
    enum_member_declaration_list: '_in_enum_member_declaration',
    initializer_expression: '_in_initializer_expression',
    parameter_list: '_in_parameter_list',
    bracketed_parameter_list: '_in_parameter_list',
    _switch_expression_body: '_in_switch_expression',
    collection_expression: '_in_collection_expression',
    attribute_argument_list: '_in_attribute_arguments',
    anonymous_object_creation_expression: '_in_anonymous_object',
    base_list: '_in_base_list',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [EXPRESSIONS]

const precedence = (rule) => csharp.grammar.rules[rule].value;
const modifiers = optionalOf(choiceOf(string('out')));
const operand = choiceOf(symbol('declaration_expression'));
const declaring = (modifier) => (part) => (symbol('declaration_expression')(part) ? seq(children(modifier).flatMap(children).find(string('out')), part) : seq(modifier, part));
const modified = (members) => members.filter((part) => !modifiers(part)).map((part) => (operand(part) ? choice(...children(part).map(declaring(members.find(modifiers)))) : part));
const leading = (rule) => (rule.type === 'STRING' ? [rule.value] : rule.type === 'SEQ' ? leading(rule.members[0]) : children(rule).flatMap(leading));
const prefixes = new Set(['prefix_unary_expression', '_address_of_expression', '_pointer_indirection_expression'].flatMap((rule) => leading(csharp.grammar.rules[rule])));
const castEdge = csharp.grammar.rules.cast_expression.content.value + 1;
const overCast = (edge) => (arm) => (reaches(arm, (part) => named('operator')(part) && prefixes.has(part.content.value)) ? prec.dynamic(edge, arm) : arm);
const valued = (_, original) => rewrite(original, (part) => (part.type === 'SEQ' && part.members.length === 2 && string('=')(part.members[0]) ? seq(part.members[0], field('value', part.members[1])) : part));

// --- [PARAMETERS]

const implicit = (part) => part.type === 'ALIAS' && part.value === 'implicit_parameter';
const element = (rule) => (choiceOf(implicit)(rule) ? rule : children(rule).map(element).find(Boolean));
const parameters = replace(symbol('_parameter_array'), ($) => alias($._parameter_array, $.parameter));

// --- [LITERALS]

const immediate = (part) => (part.type === 'CHOICE' ? choice(...children(part).map((member) => (member.type === 'BLANK' ? blank() : token.immediate(member)))) : token.immediate(part));
const verbatim = ($, part) => (part.type === 'REPEAT' ? optional(field('content', alias(token.immediate(repeat1(part.content)), $.string_content))) : immediate(part));
const opened = ($, [delimiter, ...rest]) => seq(delimiter, ...rest.map((part) => verbatim($, part)));

// --- [DIRECTIVES]

const piece = (role, kind, pattern) => field(role, alias(token.immediate(pattern), kind));
const quotable = (role, kind, bare) => choice(piece(role, kind, bare), seq(token.immediate('"'), optional(piece(role, kind, QUOTED)), token.immediate('"')));
const metadata = ($, kind) => (METADATA_DIRECTIVES.has(kind) ? [repeat(seq($._directive_space, field('metadata', $.directive_metadata)))] : []);
const bodies = {
    '@': ($) => [quotable('name', $.directive_name, BARE['@']), optional(seq(token.immediate(prec(1, '@')), optional(quotable('value', $.directive_value, BARE['@']))))],
    '=': ($) => [quotable('name', $.directive_name, BARE['=']), optional($._directive_space), token.immediate('='), optional(seq(optional($._directive_space), piece('value', $.directive_value, LINE_REST)))],
    '': ($) => [quotable('name', $.directive_name, BARE[''])],
};
const directive = ($, kind, separator) => seq('#:', optional($._directive_space), field('kind', alias(token.immediate(kind), $.directive_kind)), $._directive_space, ...bodies[separator]($), ...metadata($, kind), optional($._directive_space));

// --- [CONDITIONAL_LISTS]

const separated = (part) => part.type === 'SEQ' && part.members.length === 2 && part.members[1].type === 'REPEAT' && part.members[1].content.type === 'SEQ' && string(',')(part.members[1].content.members[0]);
const separatedIn = (rule) => (separated(rule) ? rule : children(rule).map(separatedIn).find(Boolean));
const directiveAlias = (part) => part.type === 'ALIAS' && part.value === 'preproc_if';
const listItem = ({ members: [item] }) => (choiceOf(directiveAlias)(item) ? item.members.find(Predicate.not(directiveAlias)) : item);
const listing = (list, conditional, trailing) => {
    const item = listItem(list);
    const run = seq(conditional, repeat(seq(optional(','), conditional)), optional(','));
    const gap = choice(seq(',', optional(run)), run);
    return choice(seq(optional(run), item, repeat(seq(gap, item)), trailing ? optional(gap) : optional(seq(optional(','), run))), run);
};
const conditioned = (conditional) => (part) => {
    const at = part.type === 'SEQ' ? part.members.findIndex(any(separated, optionalOf(separated))) : -1;
    const held = part.members?.[at];
    const trailing = at >= 0 && optionalOf(string(','))(part.members[at + 1] ?? {});
    return at < 0 ? part : seq(...part.members.slice(0, at), separated(held) ? listing(held, conditional, trailing) : optional(listing(children(held)[0], conditional, trailing)), ...part.members.slice(at + (trailing ? 2 : 1)));
};
const conditionalList = (rule, suffix, order) => {
    const conditional = ($) => alias($[`preproc_if${suffix}`], $.preproc_if);
    return [
        [rule, ($, original) => rewrite(parameters($, original), conditioned(conditional($)))],
        ...['if', 'else', 'elif'].map((branch) => [
            `preproc_${branch}${suffix}`,
            ($) =>
                prec(
                    csharp.grammar.rules[`preproc_${branch}${suffix}`]?.value ?? precedence('preproc_if_in_expression') - order,
                    rewrite(csharp.grammar.rules[`preproc_${branch}_in_enum_member_declaration`].content, (part) =>
                        optionalOf(symbol('enum_member_declaration'))(part)
                            ? seq(optional(','), optional(listing(separatedIn(parameters($, csharp.grammar.rules[rule])), conditional($), true)))
                            : part.type === 'SYMBOL' && part.name.endsWith('_in_enum_member_declaration')
                              ? $[part.name.replace('_in_enum_member_declaration', suffix)]
                              : part,
                    ),
                ),
        ]),
    ];
};

// --- [COMPOSITION] -------------------------------------------------------------------

const EXTENSION = {
    externals: ($, original) => [...original, $._conditional_access_question],
    conflicts: ($, original) => [
        ...original.filter((rules) => !rules.every(any(symbol('tuple_element'), symbol('declaration_expression')))),
        [$.lvalue_expression, $._expression_statement_expression],
        [$.parameter, $.declaration_expression],
        [$._reserved_identifier, $.union_declaration],
        [$._constant_binary_expression, $.binary_expression],
        ...[[], [$.is_expression]].map((extra) => [$.type_pattern, $.declaration_pattern, $._lambda_expression_init, ...extra]),
    ],
};
const RULES = {
    _top_level_item: ($, original) => choice(original, $.file_based_directive),
    file_based_directive: ($) => choice(...Record.collect(DIRECTIVE_SEPARATORS, (kind, separator) => directive($, kind, separator))),
    directive_metadata: ($) => seq(piece('name', $.directive_name, METADATA_NAME), token.immediate('='), optional(quotable('value', $.directive_value, BARE['']))),
    _directive_space: () => token.immediate(prec(1, DIRECTIVE_SPACE)),
    _reserved_identifier: (_, original) => choice(original, 'allows', 'with', 'union', 'closed', 'safe'),
    modifier: widen(choiceOf(string('public')), 'closed', 'safe'),
    type_declaration: ($, original) => choice(original, $.union_declaration),
    declaration: ($, original) => choice(original, $.union_declaration),
    union_declaration: ($) => prec.dynamic(1, seq(repeat($._attribute_list), repeat($.modifier), 'union', field('name', $.identifier), optional($.type_parameter_list), $.union_case_list, optional($.base_list), repeat($.type_parameter_constraints_clause), $._declaration_list_body)),
    union_case_list: ($) => seq('(', $.type, repeat(seq(',', $.type)), ')'),
    _struct_declaration_initializer: after(string('ref'), optional('partial')),
    _extension_member_declaration: ($, original) => choice(original, $.indexer_declaration),
    break_statement: after(string('break'), ($) => optional($.identifier)),
    continue_statement: after(string('continue'), ($) => optional($.identifier)),
    declaration_pattern: ($, original) => choice(original, prec(precedence('with_expression') + 1, seq(field('type', $.type), field('name', alias('with', $.identifier))))),
    type_parameter_constraint: (_, original) => choice(original, seq('allows', 'ref', 'struct')),
    _parameter_type_with_modifiers: ($, original) => choice(original, seq(prec.left(precedence('argument') + 1, alias(choice('in', 'out'), $.modifier)), field('type', $.type))),
    parameter_list: parameters,
    bracketed_parameter_list: parameters,
    variable_declarator: valued,
    using_variable_declarator: valued,
    parameter: valued,
    _parameter_array: replace(string('params'), ($) => alias('params', $.modifier)),
    _lambda_parameters: replace(symbol('_implicit_parameter_list_with_modifiers'), ($) => alias($._implicit_parameter_list_with_modifiers, $.parameter_list)),
    _implicit_parameter_list_with_modifiers: ($, original) => rewrite(original, (part) => (choiceOf(implicit)(part) ? alias($._implicit_parameter, $.parameter) : part)),
    _implicit_parameter: () =>
        rewrite(
            children(element(csharp.grammar.rules._implicit_parameter_list_with_modifiers)).find(({ type }) => type === 'SEQ'),
            (part) => (implicit(part) ? field('name', part.content) : part.type === 'REPEAT1' ? repeat(part.content) : part),
        ),
    argument: (_, original) => rewrite(original, (part) => (part.type === 'SEQ' && part.members.some(modifiers) ? seq(...modified(part.members)) : part)),
    tuple_expression: replace(symbol('argument'), ($) => alias($._tuple_argument, $.argument)),
    _tuple_argument: ($) => replace(symbol('declaration_expression'), () => prec.dynamic(-1, $.declaration_expression))($, csharp.grammar.rules.argument),
    generic_name: (_, original) => prec.dynamic(precedence('qualified_name') + 1, original),
    binary_expression: (_, original) => choice(...children(original).map(overCast(castEdge))),
    element_access_expression: (_, original) => prec.dynamic(castEdge, original),
    negated_pattern: (_, original) => prec(precedence('and_pattern') + 1, original),
    constant_pattern: replace(symbol('binary_expression'), ($) => alias($._constant_binary_expression, $.binary_expression)),
    _constant_binary_expression: () =>
        choice(
            ...children(csharp.grammar.rules.binary_expression)
                .filter((arm) => arm.value > precedence('is_pattern_expression'))
                .map(overCast(castEdge + 1)),
        ),
    conditional_access_expression: ($, original) => choice(prec.right(precedence('member_access_expression'), seq(field('condition', $.expression), alias($._conditional_access_question, '?'), $.member_binding_expression)), original),
    lvalue_expression: ($, original) => choice(original, $.invocation_expression),
    non_lvalue_expression: ($, original) => choice(original, $.unsafe_expression),
    unsafe_expression: ($) => seq('unsafe', '(', $.expression, ')'),
    slice_pattern: (_, original) =>
        choice(
            prec(precedence('range_expression') + 1, '..'),
            rewrite(original, (part) => (optionalOf(symbol('pattern'))(part) ? children(part).find(symbol('pattern')) : part)),
        ),
    from_clause: widen(named('name'), ($) => alias($.discard, $.identifier)),
    collection_element: ($, original) => choice(original, $.with_element),
    with_element: ($) => prec(1, seq('with', $.argument_list)),
    verbatim_string_literal: ($, original) => opened($, children(children(original)[0])),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export const hosted = extend(csharp, { ...EXTENSION, rules: RULES });
export default extend(csharp, { ...EXTENSION, rules: { ...RULES, ...Record.fromEntries(Object.entries(CONDITIONAL_LISTS).flatMap(([rule, suffix], order) => conditionalList(rule, suffix, order + 1))) } });
