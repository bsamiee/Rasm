import rust from 'tree-sitter-rust/grammar.js';

// --- [OPERATIONS] ----------------------------------------------------------------------

const modifiedBounds = ($, original) =>
    seq(...original.members.map((member) => (member.type === 'FIELD' && member.name === 'trait' ? field('trait', member.content.type === 'PREC' ? prec(member.content.value, choice(member.content.content, $.modified_trait_bound)) : choice(member.content, $.modified_trait_bound)) : member)));

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(rust, {
    name: rust.grammar.name,
    supertypes: (_, original) => original.filter((rule) => rule.name !== '_declaration_statement'),
    conflicts: ($, original) => [...original, [$.function_modifiers, $.async_block], [$.function_modifiers, $.trait_item], [$.function_modifiers, $.impl_item, $.trait_item], [$.function_modifiers, $.modified_trait_bound], [$.remaining_field_pattern, $.return_type_parameters]],
    rules: {
        _declaration_statement: ($, original) => choice(original, $.trait_alias),
        function_modifiers: (_, original) => repeat1(choice(original.content, 'safe', 'gen', 'final')),
        static_item: (_, original) => seq(...original.members.flatMap((member) => (member.type === 'STRING' && member.value === 'static' ? [optional(choice('safe', 'unsafe')), member] : [member]))),
        field_declaration: ($, original) => seq(...original.members.flatMap((member) => (member.type === 'FIELD' && member.name === 'name' ? [optional($.mut_restriction), optional('unsafe'), member] : [member])), optional(seq('=', field('value', $._expression)))),
        struct_item: ($, original) => seq(...original.members.map((member) => (member.type === 'CHOICE' && member.members.some((part) => part.type === 'STRING' && part.value === ';') ? choice(member, seq($.where_clause, ';')) : member))),
        ordered_field_declaration_list: ($) => seq('(', optional(seq($._ordered_field_declaration, repeat(seq(',', $._ordered_field_declaration)), optional(','))), ')'),
        _ordered_field_declaration: ($) => seq(repeat($.attribute_item), optional($.visibility_modifier), optional($.mut_restriction), field('type', $._type)),
        base_field_initializer: ($) => seq('..', optional($._expression)),
        associated_type: ($, original) => seq(optional($.visibility_modifier), original),
        type_item: ($, original) => seq(...original.members.flatMap((member) => (member.type === 'FIELD' && member.name === 'type_parameters' ? [member, field('bounds', optional($.trait_bounds))] : [member]))),
        _type: ($, original) => choice(original, $.modified_trait_bound),
        modified_trait_bound: ($) => seq(choice(seq(choice('const', seq('[', 'const', ']'), seq('~', 'const')), optional('async')), 'async'), field('type', $._trait_bound_type)),
        _trait_bound_type: ($) => choice($._type_identifier, $.scoped_type_identifier, $.generic_type, $.function_type),
        abstract_type: modifiedBounds,
        dynamic_type: modifiedBounds,
        trait_alias: ($) => seq(optional($.visibility_modifier), optional('const'), 'trait', field('name', $._type_identifier), field('type_parameters', optional($.type_parameters)), '=', field('bounds', optional($._trait_alias_bounds)), optional($.where_clause), ';'),
        _trait_alias_bounds: (_) => seq(...rust.grammar.rules.trait_bounds.members.filter((member) => member.type !== 'STRING' || member.value !== ':')),
        trait_item: ($) =>
            seq(
                optional($.visibility_modifier),
                optional($.impl_restriction),
                optional('const'),
                optional('unsafe'),
                optional('auto'),
                'trait',
                field('name', $._type_identifier),
                field('type_parameters', optional($.type_parameters)),
                field('bounds', optional($.trait_bounds)),
                optional($.where_clause),
                field('body', $.declaration_list),
            ),
        impl_restriction: ($) => seq('impl', $._restriction),
        mut_restriction: ($) => seq('mut', $._restriction),
        _restriction: ($) => seq('(', choice('crate', 'self', 'super', seq('in', $._path)), ')'),
        impl_item: (_, original) => seq(optional('const'), original),
        try_block: ($) => seq('try', optional(seq('bikeshed', field('type', $._type))), field('body', $.block)),
        async_block: (_, original) => seq(...original.members.flatMap((member) => (member.type === 'STRING' && member.value === 'async' ? [member, optional('gen')] : [member]))),
        function_type: ($, original) =>
            seq(...original.members.map((member) => (member.type === 'PREC' ? prec(member.value, seq(...member.content.members.map((part) => (part.type === 'FIELD' && part.name === 'parameters' ? field('parameters', choice(part.content, alias($.return_type_parameters, $.parameters))) : part)))) : member))),
        return_type_parameters: (_) => seq('(', '..', ')'),
        where_predicate: ($, original) => seq(...original.members.map((member) => (member.type === 'FIELD' && member.name === 'left' ? field('left', choice(member.content, $.unit_type, $.function_type)) : member))),
        _non_special_token: (_, original) => choice(original, '~'),
    },
});
