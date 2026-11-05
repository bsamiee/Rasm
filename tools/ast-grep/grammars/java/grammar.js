import java from 'tree-sitter-java/grammar.js';
import { after, before, choiceOf, extend, optionalOf, reaches, string, symbol, widen } from '../derive.js';

// --- [OPERATIONS] ----------------------------------------------------------------------

const invocation = symbol('explicit_constructor_invocation');
const unnamed = widen(choiceOf(symbol('_reserved_identifier')), ($) => $.underscore_pattern);
const modified = before(symbol('_unannotated_type'), ($) => optional($.modifiers));
const binding = ($, original) => unnamed($, modified($, original));
const moved = (moving, target, place) => ($, original) => {
    const member = original.members.find(moving);
    return seq(...original.members.filter((part) => part !== member).flatMap((part) => (target(part) ? place($, member, part) : [part])));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(java, {
    rules: {
        _toplevel_statement: widen(choiceOf(symbol('method_declaration')), ($) => $.field_declaration),
        field_declaration: (_, original) => prec(1, original),
        import_declaration: ($, original) => choice(original, seq('import', 'module', field('module', $._name), ';')),
        constructor_body: moved(
            (member) => member.type === 'REPEAT',
            optionalOf(invocation),
            ($, statements, prologue) => [statements, after(invocation, () => statements)($, prologue)],
        ),
        annotation_type_body: widen(choiceOf(string(';')), ($) => $.record_declaration),
        switch_label: ($, original) => choice(original, seq('case', $.pattern, repeat1(seq(',', $.pattern)), optional($.guard)), prec(1, seq('case', $.null_literal, ',', 'default'))),
        record_pattern: ($, original) => choice(original, seq($.scoped_type_identifier, $.record_pattern_body)),
        type_pattern: binding,
        record_pattern_component: binding,
        instanceof_expression: unnamed,
        lambda_expression: unnamed,
        inferred_parameters: unnamed,
        receiver_parameter: (_, original) => prec.dynamic(1, original),
        spread_parameter: moved(
            (member) => reaches(member, symbol('_annotation')),
            string('...'),
            (_, annotations, ellipsis) => [annotations, ellipsis],
        ),
    },
});
