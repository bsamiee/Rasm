import { Record } from 'effect';
import javascript from 'tree-sitter-javascript/grammar.js';
import tsx from 'tree-sitter-typescript/tsx/grammar.js';
import { after, choiceOf, extend, named, optionalOf, reaches, rewrite, string, symbol, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PHASES = { defer: 'namespace_import', source: 'identifier' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const clause = (phase) => `_${phase}_import_clause`;
const variance = () => choice(seq('in', optional('out')), 'out');
const modifiers = (constant) => optional(choice(seq(constant, optional(variance())), seq(variance(), optional(constant))));
const arms = (rule) => rule.members.flatMap((arm) => (arm.type === 'CHOICE' ? arm.members : [arm]));
const unique = (rules) => [...new Map(rules.map((rule) => [JSON.stringify(rule), rule])).values()];
const phased = ($, original, phase) =>
    rewrite(original, (rule) => {
        if (optionalOf(choiceOf(string('typeof')))(rule)) {
            return field('phase', phase);
        }
        return choiceOf(symbol('import_require_clause'))(rule)
            ? rewrite(
                  rule.members.find((member) => reaches(member, symbol('_from_clause'))),
                  (part) => (symbol('import_clause')(part) ? alias($[clause(phase)], $.import_clause) : part),
              )
            : rule;
    });
const typed = (original) =>
    seq(
        ...arms(original)
            .find((arm) => reaches(arm, symbol('namespace_export')))
            .members.flatMap((part) => {
                if (string('export')(part)) {
                    return [part, 'type'];
                }
                return choiceOf(symbol('export_clause'))(part) ? [choice(...part.members.filter((member) => !reaches(member, symbol('export_clause'))))] : [part];
            }),
    );
const accessible = (modifier) =>
    choice(...unique([...modifier.members.filter((alternative) => !optionalOf(string('accessor'))(alternative)), ...modifier.members.filter((alternative) => alternative.type === 'SEQ').map((alternative) => seq(...alternative.members.filter((part) => !optionalOf(string('readonly'))(part)), optional('accessor')))]));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(tsx, {
    externals: ($, original) => [...original, $._adjacent_type_arguments_open],
    conflicts: ($, original) => [...original, [$.primary_expression, $.using_declaration], [$._type_query_member_expression_in_type_annotation]],
    rules: {
        _reserved_identifier: (_, original) => choice(original, 'using', 'defer', 'source', 'out', 'accessor'),
        _for_header: () => javascript.grammar.rules._for_header,
        assignment_expression: () => javascript.grammar.rules.assignment_expression,
        declaration: ($, original) => choice(original, $.using_declaration),
        using_declaration: () => javascript.grammar.rules.using_declaration,
        _import_identifier: ($, original) => choice(original, ...Object.keys(PHASES).map((phase) => alias(phase, $.identifier))),
        import_statement: ($, original) => choice(original, ...Object.keys(PHASES).map((phase) => phased($, original, phase))),
        ...Record.mapEntries(PHASES, (bound, phase) => [clause(phase), ($) => $[bound]]),
        import_alias: after(string('import'), optional('type')),
        export_statement: ($, original) => after(symbol('_from_clause'), ($$) => optional($$.import_attribute))($, choice(...original.members, typed(original))),
        type_parameter: ($, original) =>
            widen(named('name'), ($$) => alias('out', $$.type_identifier))(
                $,
                rewrite(original, (rule) => (optionalOf(string('const'))(rule) ? modifiers(rule.members.find(string('const'))) : rule)),
            ),
        type_arguments: widen(string('<'), ($) => alias($._adjacent_type_arguments_open, '<')),
        index_signature: (_, original) => seq(optional('static'), original),
        module: ($, original) => rewrite(original, (rule) => (symbol('_module')(rule) ? $._ambient_module : rule)),
        _ambient_module: ($) => rewrite(tsx.grammar.rules._module, (rule) => (symbol('statement_block')(rule) ? alias($._ambient_module_block, $.statement_block) : rule)),
        _ambient_module_block: ($) => rewrite(tsx.grammar.rules.statement_block, (rule) => (symbol('statement')(rule) ? choice(rule, alias($._global_augmentation, $.ambient_declaration)) : rule)),
        _global_augmentation: () => arms(tsx.grammar.rules.ambient_declaration).find((arm) => reaches(arm, string('global'))),
        public_field_definition: (_, original) => rewrite(original, (rule) => (rule.type === 'CHOICE' && rule.members.some(optionalOf(string('accessor'))) ? accessible(rule) : rule)),
        tuple_parameter: widen(named('name'), ($) => alias($.predefined_type, $.identifier)),
        optional_tuple_parameter: widen(named('name'), ($) => alias($.predefined_type, $.identifier)),
        _type_query_member_expression_in_type_annotation: ($, original) => seq(original, optional(field('type_arguments', $.type_arguments))),
        call_expression: ($, original) => rewrite(original, (rule) => (rule.type === 'PREC' && rule.value === 'template_call' ? widen(named('function'), ($$) => $$.instantiation_expression)($, rule) : rule)),
    },
});
