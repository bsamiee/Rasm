import csharp from 'tree-sitter-c-sharp/grammar.js';
import { after, any, children, choiceOf, extend, token as literal, named, optionalOf, rewrite, symbol, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DIRECTIVE_SEPARATORS = { sdk: '@', package: '@', property: '=', project: '', ref: '', include: '', exclude: '' };
const METADATA_DIRECTIVES = new Set(['package', 'project', 'ref', 'include']);
const DIRECTIVE_SPACE = /[\t\v\f\p{Zs}\u{FEFF}]+/u;
const QUOTED = /"(?:[^"\\\r\n]|\\[^\r\n])*"/u;
const METADATA_NAME = /[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}.-]*/u;
const BARE = { '@': /[^\s"@]+/u, '=': /[^\s"=]+/u, '': /[^\s"]+/u };
const LINE_REST = /[^\s](?:[^\r\n]*[^\s])?/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [EXPRESSIONS]

const precedence = (rule) => csharp.grammar.rules[rule].value;
const replaced = (matches, replacement) => ($, original) => rewrite(original, (part) => (matches(part) ? replacement($) : part));
const modifiers = optionalOf(choiceOf(literal('out')));
const operand = choiceOf(symbol('declaration_expression'));
const declaring = (modifier) => (part) => (symbol('declaration_expression')(part) ? seq(children(modifier).flatMap(children).find(literal('out')), part) : seq(modifier, part));
const modified = (members) => members.filter((part) => !modifiers(part)).map((part) => (operand(part) ? choice(...children(part).map(declaring(members.find(modifiers)))) : part));

// --- [LITERALS]

const immediate = (part) => (part.type === 'CHOICE' ? choice(...children(part).map((member) => (member.type === 'BLANK' ? blank() : token.immediate(member)))) : token.immediate(part));
const verbatim = ($, part) => (part.type === 'REPEAT' ? optional(field('content', alias(token.immediate(repeat1(part.content)), $.string_content))) : immediate(part));
const opened = ($, [delimiter, ...rest]) => seq(delimiter, ...rest.map((part) => verbatim($, part)));

// --- [DIRECTIVES]

const name = ($, separator) => field('name', alias(token.immediate(choice(BARE[separator], QUOTED)), $.directive_name));
const value = ($, pattern) => field('value', alias(token.immediate(pattern), $.directive_value));
const metadata = ($, kind) => (METADATA_DIRECTIVES.has(kind) ? [repeat(seq($._directive_space, field('metadata', $.directive_metadata)))] : []);
const bodies = {
    '@': ($) => [name($, '@'), optional(seq(token.immediate(prec(1, '@')), optional(value($, choice(BARE['@'], QUOTED)))))],
    '=': ($) => [name($, '='), optional($._directive_space), token.immediate('='), optional(seq(optional($._directive_space), value($, LINE_REST)))],
    '': ($) => [name($, '')],
};
const directive = ($, [kind, separator]) => seq('#:', optional($._directive_space), field('kind', alias(token.immediate(kind), $.directive_kind)), $._directive_space, ...bodies[separator]($), ...metadata($, kind), optional($._directive_space));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(csharp, {
    externals: ($, original) => [...original, $._conditional_access_question],
    conflicts: ($, original) => [
        ...original.filter((rules) => !rules.every(any(symbol('tuple_element'), symbol('declaration_expression')))),
        [$.lvalue_expression, $._expression_statement_expression],
        [$.parameter, $.declaration_expression],
        [$._reserved_identifier, $.union_declaration],
        [$._constant_binary_expression, $.binary_expression],
        ...[[], [$.is_expression]].map((extra) => [$.type_pattern, $.declaration_pattern, $._lambda_expression_init, ...extra]),
    ],
    rules: {
        _top_level_item: ($, original) => choice(original, $.file_based_directive),
        file_based_directive: ($) => choice(...Object.entries(DIRECTIVE_SEPARATORS).map((entry) => directive($, entry))),
        directive_metadata: ($) => seq(field('name', alias(token.immediate(METADATA_NAME), $.directive_name)), token.immediate('='), optional(value($, choice(BARE[''], QUOTED)))),
        _directive_space: () => token.immediate(prec(1, DIRECTIVE_SPACE)),
        _reserved_identifier: (_, original) => choice(original, 'allows', 'with', 'union', 'closed', 'safe'),
        modifier: widen(choiceOf(literal('public')), 'closed', 'safe'),
        type_declaration: ($, original) => choice(original, $.union_declaration),
        declaration: ($, original) => choice(original, $.union_declaration),
        union_declaration: ($) => prec.dynamic(1, seq(repeat($._attribute_list), repeat($.modifier), 'union', field('name', $.identifier), optional($.type_parameter_list), $.union_case_list, optional($.base_list), repeat($.type_parameter_constraints_clause), $._declaration_list_body)),
        union_case_list: ($) => seq('(', $.type, repeat(seq(',', $.type)), ')'),
        _struct_declaration_initializer: after(literal('ref'), optional('partial')),
        _extension_member_declaration: ($, original) => choice(original, $.indexer_declaration),
        break_statement: after(literal('break'), ($) => optional($.identifier)),
        continue_statement: after(literal('continue'), ($) => optional($.identifier)),
        declaration_pattern: ($, original) => choice(original, prec(precedence('with_expression') + 1, seq(field('type', $.type), field('name', alias('with', $.identifier))))),
        type_parameter_constraint: (_, original) => choice(original, seq('allows', 'ref', 'struct')),
        _parameter_type_with_modifiers: ($, original) => choice(original, seq(prec.left(precedence('argument') + 1, alias(choice('in', 'out'), $.modifier)), field('type', $.type))),
        argument: (_, original) => rewrite(original, (part) => (part.type === 'SEQ' && part.members.some(modifiers) ? seq(...modified(part.members)) : part)),
        tuple_expression: replaced(symbol('argument'), ($) => alias($._tuple_argument, $.argument)),
        _tuple_argument: ($) => replaced(symbol('declaration_expression'), () => prec.dynamic(-1, $.declaration_expression))($, csharp.grammar.rules.argument),
        generic_name: (_, original) => prec.dynamic(precedence('qualified_name') + 1, original),
        constant_pattern: replaced(symbol('binary_expression'), ($) => alias($._constant_binary_expression, $.binary_expression)),
        _constant_binary_expression: () => choice(...children(csharp.grammar.rules.binary_expression).filter((arm) => arm.value > precedence('is_pattern_expression'))),
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
    },
});
