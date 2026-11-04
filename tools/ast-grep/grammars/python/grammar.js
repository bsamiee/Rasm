import python from 'tree-sitter-python/grammar.js';
import { extend, named, rewrite, symbol, widen } from '../derive.js';

// --- [OPERATIONS] ----------------------------------------------------------------------

const lazyImport = (_, original) => seq(optional(field('lazy', 'lazy')), original);
const listBody = widen(named('body'), ($) => $.list_splat);
const replace = (matches, replacement) => ($, original) => rewrite(original, (part) => (matches(part) ? replacement($) : part));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(python, {
    conflicts: (_, original) => original.filter((set) => !set.some(symbol('type_alias_statement'))),
    rules: {
        import_statement: lazyImport,
        import_from_statement: lazyImport,
        keyword_identifier: ($, original) => choice(original, alias('lazy', $.identifier)),
        type_alias_statement: replace(named('left'), ($) => field('left', alias($._type_alias_name, $.type))),
        _type_alias_name: ($) => choice($.identifier, alias($._type_alias_generic, $.generic_type)),
        _type_alias_generic: ($) => seq($.identifier, $.type_parameter),
        type_parameter: replace(symbol('type'), ($) => $._type_parameter),
        _type_parameter: ($) => seq($.type, optional(seq('=', field('default', choice($.expression, $.list_splat))))),
        generic_type: replace(symbol('type_parameter'), ($) => alias($._type_arguments, $.type_parameter)),
        _type_arguments: () => python.grammar.rules.type_parameter,
        list_comprehension: listBody,
        set_comprehension: listBody,
        generator_expression: listBody,
        dictionary_comprehension: widen(named('body'), ($) => $.dictionary_splat),
    },
});
