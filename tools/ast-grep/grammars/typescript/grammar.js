import { Array, Record } from 'effect';
import javascript from 'tree-sitter-javascript/grammar.js';
import tsx from 'tree-sitter-typescript/tsx/grammar.js';
import { after, chain, choiceOf, classes, collected, constants, drop, extend, lexical, named, optionalOf, reaches, rewrite, spans, string, symbol, widen } from '#/tools/ast-grep/grammars/derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PHASES = { defer: 'namespace_import', source: 'identifier' };
const IMPORT_TYPES = ['_type_query_call_expression_in_type_annotation', '_type_query_member_expression_in_type_annotation'];
const CONTEXTUAL = ['using', ...Object.keys(PHASES), 'out', 'accessor', 'abstract', 'as', 'satisfies', 'unknown', 'never', 'is', 'global', 'require', 'intrinsic', 'assert'];
const BINDING_OPERATORS = ['as', 'satisfies'];
const RETIRED_CONFLICTS = [
    ['jsx_opening_element', 'type_parameter'],
    ['nested_identifier', 'nested_type_identifier', 'primary_expression'],
];
const WORD = /^\p{ID_Start}\p{ID_Continue}*$/u;
const RESERVED_WORDS = new Set(javascript.grammar.reserved.global.map(({ value }) => value));
const TYPE_KEYWORDS = Array.dedupe(
    ['type', 'primary_type']
        .flatMap((name) => tsx.grammar.rules[name].members)
        .map((arm) => (arm.type === 'SYMBOL' ? tsx.grammar.rules[arm.name] : arm))
        .flatMap((rule) => collected(rule, ({ type }) => type === 'SEQ'))
        .map(({ members: [head] }) => head)
        .filter((head) => head.type === 'STRING' && WORD.test(head.value) && !RESERVED_WORDS.has(head.value))
        .map(({ value }) => value),
);
const PREDEFINED_TYPES = new Set(tsx.grammar.rules.predefined_type.members.filter(({ type }) => type === 'STRING').map(({ value }) => value));
const TYPE_NAMES = Array.dedupe([...collected(tsx.grammar.rules._reserved_identifier, ({ type }) => type === 'STRING').map(({ value }) => value), ...CONTEXTUAL]).filter((word) => !(RESERVED_WORDS.has(word) || PREDEFINED_TYPES.has(word) || TYPE_KEYWORDS.includes(word)));
const DEFAULT_VALUES = ['function_expression', 'generator_function', 'class'];
const REFERENCE_LIMITS = { HEXADECIMAL_REFERENCE_DIGITS: 6, DECIMAL_REFERENCE_DIGITS: 7, NAMED_REFERENCE_LETTERS: 30 };
const CHARACTER_REFERENCE = new RegExp(`&(?:#(?:[xX][0-9a-fA-F]{1,${REFERENCE_LIMITS.HEXADECIMAL_REFERENCE_DIGITS}}|[0-9]{1,${REFERENCE_LIMITS.DECIMAL_REFERENCE_DIGITS}})|[A-Za-z]{1,${REFERENCE_LIMITS.NAMED_REFERENCE_LETTERS}});`, 'u');
const DIRECTIVE = /#(?:include|includepath|target|targetengine|script|strict|engine)/u;
const DIRECTIVE_VALUE = /[^\s"';]+/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const clause = (phase) => `_${phase}_import_clause`;
const variance = () => choice(seq('in', optional('out')), 'out');
const arms = (rule) => rule.members.flatMap((arm) => (arm.type === 'CHOICE' ? arm.members : [arm]));
const augmented = ($, rule) => rewrite(rule, (part) => (string('global')(part) ? field('name', alias(part, $.identifier)) : part));
const typeNamed = (kind) => ($) => alias(choice(...TYPE_NAMES), $[kind]);
const reservedName = (matches) => widen(matches, typeNamed('identifier'));
const typeNested = ($, rule) => rewrite(rule, (part) => (symbol('nested_identifier')(part) ? alias($._type_nested_identifier, $.nested_identifier) : part));
const bindable = ($) => alias(choice(...BINDING_OPERATORS), $.identifier);
const tupled = widen(
    named('name'),
    ($) => alias($.predefined_type, $.identifier),
    ($) => alias(choice(...TYPE_NAMES), $.identifier),
);
const importType = ({ type }) => type === 'PREC';
const separatedMark = (_, original) => rewrite(original, (rule) => (rule.type === 'STRING' && rule.value.endsWith('?:') ? choice(rule, seq(...rule.value)) : rule));

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(tsx, {
    precedences: (_, original) => [...original.filter((row) => !row.some(({ name }) => IMPORT_TYPES.includes(name))), ['call', 'import_type'], ['member', 'import_type']],
    externals: ($, original) => [...original, $._adjacent_type_arguments_open],
    conflicts: ($, original) => [
        ...original.filter((set) => !RETIRED_CONFLICTS.some((names) => names.length === set.length && set.every(({ name }) => names.includes(name)))),
        [$.primary_expression, $.using_declaration],
        [$.primary_expression, $.type_alias_declaration],
        [$._for_header, $.expression],
        [$._module_export_name, $.import_specifier],
        [$._module_export_name, $.export_specifier],
        [$.primary_type, $.type_parameter],
        [$.jsx_opening_element, $.generic_type],
        [$.jsx_opening_element, $.primary_type, $.type_parameter],
        [$._type_query_member_expression_in_type_annotation],
        [$.primary_expression, $.arrow_function, $.method_definition],
        [$.call_expression, $._type_query_call_expression_in_type_annotation],
        [$.member_expression, $._type_query_member_expression_in_type_annotation],
        [$.primary_expression, $.accessibility_modifier, $.primary_type],
        [$.primary_expression, $.accessibility_modifier, $.generic_type],
        [$.primary_expression, $.override_modifier, $.primary_type],
        [$.primary_expression, $.override_modifier, $.generic_type],
        [$.primary_expression, $.arrow_function, $.generic_type],
        [$.primary_expression, $.infer_type],
        [$.primary_expression, $.index_type_query],
        [$.pattern, $.index_type_query],
        [$.primary_expression, $.nested_type_identifier, $._type_nested_identifier],
        [$.nested_identifier, $._type_nested_identifier],
        [$.nested_type_identifier, $._type_nested_identifier],
        ...DEFAULT_VALUES.map((name) => [$[name], $[`_default_${name}`]]),
    ],
    rules: {
        _reserved_identifier: (_, original) => choice(original, ...CONTEXTUAL, ...TYPE_KEYWORDS),
        _for_header: ($) =>
            widen((rule) => rule.type === 'ALIAS' && string('of')(rule.content), bindable)(
                $,
                rewrite(javascript.grammar.rules._for_header, (rule) => (named('left')(rule) && reaches(rule, symbol('parenthesized_expression')) ? field('left', choice(rule.content, $.as_expression, $.satisfies_expression)) : rule)),
            ),
        assignment_expression: () => javascript.grammar.rules.assignment_expression,
        declaration: ($, original) => choice(original, $.using_declaration),
        statement: ($, original) => choice(original, $.preprocessor_directive),
        preprocessor_directive: ($) => seq(field('name', alias(token(prec(1, DIRECTIVE)), $.identifier)), field('value', choice($.string, alias(DIRECTIVE_VALUE, $.identifier)))),
        expression: ($, original) => choice(original, $.type_assertion),
        using_declaration: () => javascript.grammar.rules.using_declaration,
        html_character_reference: () => CHARACTER_REFERENCE,
        _constants: () => constants({ ...REFERENCE_LIMITS, SPACE: spans(classes({ identifier: [], whitespace: tsx.grammar.extras.filter(lexical) }).whitespace) }),
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
        _exported_import_require: ($) => seq('import', optional('type'), $.import_require_clause, $._semicolon),
        export_statement: ($, original) =>
            after(symbol('_from_clause'), ($$) => optional($$.import_attribute))(
                $,
                choice(
                    seq('export', 'default', field('value', choice(...DEFAULT_VALUES.map((name) => alias($[`_default_${name}`], $[name]))))),
                    seq('export', field('declaration', alias($._exported_import_require, $.import_statement))),
                    ...rewrite(original, (rule) =>
                        rule.type === 'SEQ' && rule.members.some(string('default'))
                            ? widen(
                                  named('declaration'),
                                  ($$) => alias($$._default_abstract_class, $$.abstract_class_declaration),
                                  ($$) => alias($$._default_function_signature, $$.function_signature),
                              )($, rule)
                            : rule,
                    ).members,
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
        ...Record.fromIterableWith(DEFAULT_VALUES, (name) => [`_default_${name}`, () => prec.dynamic(1, tsx.grammar.rules[name])]),
        _default_abstract_class: ($) => drop(named('name'))($, tsx.grammar.rules.abstract_class_declaration),
        _default_function_signature: ($) => drop(named('name'))($, tsx.grammar.rules.function_signature),
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
        class_body: ($, original) =>
            rewrite(original, (rule) =>
                rule.type === 'SEQ' && rule.members.some(symbol('method_definition')) ? seq(choice($.method_definition, alias($._decorated_method_definition, $.method_definition)), ...rule.members.filter((member) => !(symbol('method_definition')(member) || reaches(member, named('decorator'))))) : rule,
            ),
        _decorated_method_definition: ($) => seq(repeat1(field('decorator', $.decorator)), tsx.grammar.rules.method_definition),
        public_field_definition: ($, original) =>
            rewrite(original, (rule) =>
                rule.type === 'CHOICE' && rule.members.some(optionalOf(string('accessor')))
                    ? choice(
                          ...Array.dedupe([
                              ...rule.members.filter((alternative) => !optionalOf(string('accessor'))(alternative)),
                              ...rule.members.filter((alternative) => alternative.type === 'SEQ').map((alternative) => seq(...drop(optionalOf(string('readonly')))($, alternative).members, optional('accessor'))),
                              seq(optional('abstract'), optional($.override_modifier), optional('readonly')),
                          ]),
                      )
                    : rule,
            ),
        type: drop(importType),
        primary_type: (_, original) => choice(original, ...tsx.grammar.rules.type.members.filter(importType).map(({ content }) => prec('import_type', content))),
        index_type_query: widen(symbol('primary_type'), ($) => $.readonly_type),
        template_literal_type: widen(symbol('template_type'), ($) => $.escape_sequence),
        omitting_type_annotation: separatedMark,
        adding_type_annotation: separatedMark,
        opting_type_annotation: separatedMark,
        _type_identifier: ($, original) => choice(original, alias(choice(...TYPE_NAMES), $.type_identifier)),
        nested_type_identifier: ($, original) => reservedName(named('module'))($, typeNested($, original)),
        _type_nested_identifier: ($) => chain(reservedName(named('object')), widen(named('property'), typeNamed('property_identifier')))($, typeNested($, tsx.grammar.rules.nested_identifier)),
        type_predicate: reservedName(named('name')),
        asserts: reservedName(symbol('identifier')),
        mapped_type_clause: widen(named('name'), ($) => alias($._reserved_identifier, $.type_identifier)),
        variable_declarator: widen(named('name'), bindable),
        tuple_parameter: tupled,
        optional_tuple_parameter: tupled,
        _type_query_member_expression_in_type_annotation: ($, original) => prec('member', seq(original, optional(field('type_arguments', $.type_arguments)))),
        _type_query_call_expression_in_type_annotation: ($, original) => prec('call', drop(({ type }) => type === 'ALIAS')($, original)),
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
