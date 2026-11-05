import { Array, Record } from 'effect';
import javascript from 'tree-sitter-javascript/grammar.js';
import tsx from 'tree-sitter-typescript/tsx/grammar.js';
import { after, choiceOf, drop, extend, named, optionalOf, reaches, rewrite, string, symbol, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PHASES = { defer: 'namespace_import', source: 'identifier' };
const CHARACTER_REFERENCE = /&(?:#(?:[xX][0-9a-fA-F]{1,6}|[0-9]{1,7})|[A-Za-z]{1,30});/u;
const DIRECTIVE = /#(?:include|includepath|target|targetengine|script|strict|engine)/u;
const DIRECTIVE_VALUE = /[^\s"';]+/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const clause = (phase) => `_${phase}_import_clause`;
const variance = () => choice(seq('in', optional('out')), 'out');
const arms = (rule) => rule.members.flatMap((arm) => (arm.type === 'CHOICE' ? arm.members : [arm]));
const augmented = ($, rule) => rewrite(rule, (part) => (string('global')(part) ? field('name', alias(part, $.identifier)) : part));
const tupled = widen(named('name'), ($) => alias($.predefined_type, $.identifier));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(tsx, {
    externals: ($, original) => [...original, $._adjacent_type_arguments_open],
    conflicts: ($, original) => [
        ...original.filter((set) => !(set.length === 2 && set.every(({ name }) => name === 'jsx_opening_element' || name === 'type_parameter'))),
        [$.primary_expression, $.using_declaration],
        [$.primary_expression, $.type_alias_declaration],
        [$._for_header, $.expression],
        [$._module_export_name, $.import_specifier],
        [$._module_export_name, $.export_specifier],
        [$.primary_type, $.type_parameter],
        [$.jsx_opening_element, $.generic_type],
        [$.jsx_opening_element, $.primary_type, $.type_parameter],
        [$._type_query_member_expression_in_type_annotation],
    ],
    rules: {
        _reserved_identifier: (_, original) => choice(original, 'using', ...Object.keys(PHASES), 'out', 'accessor', 'abstract'),
        _for_header: ($) => rewrite(javascript.grammar.rules._for_header, (rule) => (named('left')(rule) && reaches(rule, symbol('parenthesized_expression')) ? field('left', choice(rule.content, $.as_expression, $.satisfies_expression)) : rule)),
        assignment_expression: () => javascript.grammar.rules.assignment_expression,
        declaration: ($, original) => choice(original, $.using_declaration),
        statement: ($, original) => choice(original, $.preprocessor_directive),
        preprocessor_directive: ($) => seq(field('name', alias(token(prec(1, DIRECTIVE)), $.identifier)), field('value', choice($.string, alias(DIRECTIVE_VALUE, $.identifier)))),
        expression: ($, original) => choice(original, $.type_assertion),
        using_declaration: () => javascript.grammar.rules.using_declaration,
        html_character_reference: () => CHARACTER_REFERENCE,
        _import_identifier: ($, original) => choice(original, ...[...Object.keys(PHASES), 'as'].map((word) => alias(word, $.identifier))),
        _module_export_name: ($, original) => choice(original, alias('as', $.identifier), alias('type', $.identifier)),
        import_statement: ($, original) => {
            const clauses = rewrite(original, (rule) => (choiceOf(symbol('import_require_clause'))(rule) ? rule.members.find((member) => reaches(member, symbol('_from_clause'))) : rule));
            return choice(
                original,
                ...Object.keys(PHASES).map((phase) =>
                    rewrite(clauses, (rule) => {
                        if (optionalOf(choiceOf(string('typeof')))(rule)) {
                            return field('phase', phase);
                        }
                        return symbol('import_clause')(rule) ? alias($[clause(phase)], $.import_clause) : rule;
                    }),
                ),
            );
        },
        ...Record.mapEntries(PHASES, (bound, phase) => [clause(phase), ($) => $[bound]]),
        import_alias: after(string('import'), optional('type')),
        export_statement: ($, original) =>
            after(symbol('_from_clause'), ($$) => optional($$.import_attribute))(
                $,
                choice(
                    ...rewrite(original, (rule) => (rule.type === 'SEQ' && rule.members.some(string('default')) ? widen(named('declaration'), ($$) => alias($$._default_abstract_class, $$.abstract_class_declaration))($, rule) : rule)).members,
                    seq(
                        ...arms(original)
                            .find((arm) => reaches(arm, symbol('namespace_export')))
                            .members.flatMap((part) => {
                                if (string('export')(part)) {
                                    return [part, 'type'];
                                }
                                return choiceOf(symbol('export_clause'))(part) ? [choice(...part.members.filter((member) => !reaches(member, symbol('export_clause'))))] : [part];
                            }),
                    ),
                ),
            ),
        _default_abstract_class: ($) => drop(named('name'))($, tsx.grammar.rules.abstract_class_declaration),
        type_parameter: ($, original) =>
            widen(named('name'), ($$) => alias('out', $$.type_identifier))(
                $,
                rewrite(original, (rule) => {
                    if (!optionalOf(string('const'))(rule)) {
                        return rule;
                    }
                    const constant = rule.members.find(string('const'));
                    return optional(choice(seq(constant, optional(variance())), seq(variance(), optional(constant))));
                }),
            ),
        object_type: ($, original) => rewrite(original, (rule) => (rule.type === 'CHOICE' && rule.members.some(symbol('export_statement')) ? seq(repeat($.export_statement), rule) : rule)),
        type_arguments: widen(string('<'), ($) => alias($._adjacent_type_arguments_open, '<')),
        type_alias_declaration: widen(
            named('name'),
            ($) => alias('as', $.type_identifier),
            ($) => alias('satisfies', $.type_identifier),
        ),
        index_signature: (_, original) => seq(optional('static'), original),
        module: ($, original) => rewrite(original, (rule) => (symbol('_module')(rule) ? $._ambient_module : rule)),
        _ambient_module: ($) => rewrite(tsx.grammar.rules._module, (rule) => (symbol('statement_block')(rule) ? alias($._ambient_module_block, $.statement_block) : rule)),
        _ambient_module_block: ($) => rewrite(tsx.grammar.rules.statement_block, (rule) => (symbol('statement')(rule) ? choice(rule, alias($._global_augmentation, $.ambient_declaration)) : rule)),
        ambient_declaration: augmented,
        _global_augmentation: ($) =>
            augmented(
                $,
                arms(tsx.grammar.rules.ambient_declaration).find((arm) => reaches(arm, string('global'))),
            ),
        class_body: ($, original) => rewrite(original, (rule) => (rule.type === 'SEQ' && rule.members.some(symbol('method_definition')) ? seq(choice($.method_definition, alias($._decorated_method_definition, $.method_definition)), ...rule.members.slice(2)) : rule)),
        _decorated_method_definition: ($) => seq(repeat1(field('decorator', $.decorator)), tsx.grammar.rules.method_definition),
        public_field_definition: ($, original) =>
            rewrite(original, (rule) =>
                rule.type === 'CHOICE' && rule.members.some(optionalOf(string('accessor')))
                    ? choice(...Array.dedupe([...rule.members.filter((alternative) => !optionalOf(string('accessor'))(alternative)), ...rule.members.filter((alternative) => alternative.type === 'SEQ').map((alternative) => seq(...drop(optionalOf(string('readonly')))($, alternative).members, optional('accessor')))]))
                    : rule,
            ),
        tuple_parameter: tupled,
        optional_tuple_parameter: tupled,
        _type_query_member_expression_in_type_annotation: ($, original) => seq(original, optional(field('type_arguments', $.type_arguments))),
        _import_phase: () => seq('import', '.', field('phase', choice(...Object.keys(PHASES)))),
        call_expression: ($, original) =>
            rewrite(original, (rule) => {
                if (rule.type !== 'PREC') {
                    return rule;
                }
                const direct = rule.value === 'call' ? widen(named('function'), ($$) => alias($$._import_phase, $$.import))($, rule) : rule;
                return rule.value === 'template_call' || rule.value === 'member' ? widen(named('function'), ($$) => $$.instantiation_expression)($, direct) : direct;
            }),
    },
});
