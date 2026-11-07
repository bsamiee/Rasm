import python from 'tree-sitter-python/grammar.js';
import { drop, extend, named, replace, symbol, widen } from '#/grammars/derive.js';

// --- [OPERATIONS] ----------------------------------------------------------------------

const lazyImport = (_, original) => seq(optional(field('lazy', 'lazy')), original);
const starred = (matches) => widen(matches, ($) => $.list_splat);
const comprehension = starred(named('body'));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(python, {
    conflicts: ($, original) => [...original.filter((set) => !set.some(symbol('type_alias_statement'))), [$.type, $._type_slice]],
    rules: {
        import_statement: lazyImport,
        import_from_statement: lazyImport,
        keyword_identifier: ($, original) => choice(original, alias('lazy', $.identifier)),
        class_definition: replace(named('name'), ($) => field('name', alias($.identifier, $.type_identifier))),
        type_alias_statement: replace(named('left'), ($) => field('left', alias($._type_alias_name, $.type))),
        _type_alias_name: ($) => choice($.identifier, alias($._type_alias_generic, $.generic_type)),
        _type_alias_generic: ($) => seq($.identifier, $.type_parameter),
        type_parameter: replace(symbol('type'), ($) => $._type_parameter),
        _type_parameter: ($) => seq($.type, optional(seq('=', field('default', choice($.expression, $.list_splat))))),
        generic_type: replace(symbol('type_parameter'), ($) => alias($._type_arguments, $.type_parameter)),
        _type_arguments: ($) => widen(symbol('type'), () => alias($._type_slice, $.slice))($, python.grammar.rules.type_parameter),
        _type_slice: () => prec.dynamic(1, prec(1, python.grammar.rules.slice)),
        splat_type: widen(symbol('identifier'), ($) => $.generic_type),
        primary_expression: drop((rule) => rule.type === 'ALIAS' && symbol('list_splat_pattern')(rule.content)),
        expression_statement: ($, original) => choice(...original.members.map((arm) => (arm.type === 'SEQ' ? starred(symbol('expression'))($, arm) : arm))),
        expression_list: starred(symbol('expression')),
        match_statement: starred(named('subject')),
        subscript: starred(named('subscript')),
        list_comprehension: comprehension,
        set_comprehension: comprehension,
        generator_expression: comprehension,
        dictionary_comprehension: widen(named('body'), ($) => $.dictionary_splat),
    },
});
