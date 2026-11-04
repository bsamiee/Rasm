import java from 'tree-sitter-java/grammar.js';

// --- [EXPORTS] -------------------------------------------------------------------------

export default grammar(java, {
    name: java.grammar.name,
    supertypes: (_, original) => original.filter(({ name }) => !java.grammar.inline.includes(name)),
    rules: {
        import_declaration: ($, original) => choice(original, seq('import', 'module', field('module', $._name), ';')),
        constructor_body: ($) => seq('{', repeat($.statement), optional(seq($.explicit_constructor_invocation, repeat($.statement))), '}'),
        annotation_type_body: ($, original) => seq(...original.members.map((member) => (member.type === 'REPEAT' ? repeat(choice(member.content, $.record_declaration)) : member))),
        switch_label: ($, original) => choice(original, seq('case', $.pattern, repeat1(seq(',', $.pattern)), optional($.guard))),
        record_pattern: ($, original) => choice(original, seq($.scoped_type_identifier, $.record_pattern_body)),
        type_pattern: ($, original) => choice(original, seq($.modifiers, $._unannotated_type, choice($.identifier, $._reserved_identifier))),
        record_pattern_component: ($, original) => choice(original, seq($.modifiers, $._unannotated_type, choice($.identifier, $._reserved_identifier))),
    },
});
