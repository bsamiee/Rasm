import { createRequire } from 'node:module';
import { join } from 'node:path';

// --- [CONSTANTS] -----------------------------------------------------------------------

const rust = createRequire(import.meta.url)(join(process.cwd(), 'grammar.js'));

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(rust, {
    name: 'rust',
    rules: {
        _declaration_statement: ($, original) => choice(original, $.trait_alias),
        associated_type: ($, original) => prec(-1, seq(...original.members.slice(0, -1), optional(seq('=', field('default', $._type))), ';')),
        field_declaration: ($, original) => seq(original, optional(seq('=', field('value', $._expression)))),
        base_field_initializer: ($) => seq('..', optional($._expression)),
        try_block: ($) => seq('try', optional(seq('bikeshed', field('type', $._type))), field('body', $.block)),
        function_modifiers: (_, original) => repeat1(choice(...original.content.members, 'safe', 'gen')),
        trait_alias: ($) => seq(optional($.visibility_modifier), 'trait', field('name', $._type_identifier), field('type_parameters', optional($.type_parameters)), '=', field('bounds', alias($._trait_alias_bounds, $.trait_bounds)), optional($.where_clause), ';'),
        _trait_alias_bounds: () => seq(...rust.grammar.rules.trait_bounds.members.slice(1)),
    },
});
