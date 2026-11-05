import java from 'tree-sitter-java/grammar.js';
import { after, before, choiceOf, drop, extend, located, optionalOf, outermost, reaches, rewrite, string, symbol, widen } from '../derive.js';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BACKSLASH = /\\u+005[Cc]/u;
const ESCAPE = /[btnfrs"'\\]|[0-3][0-7]{2}|[0-7]{1,2}|u+[0-9a-fA-F]{4}/u;
const LINE_TERMINATOR = /\r\n?|\n/u;
const CONTEXTUAL = new Set(['exports', 'module', 'non-sealed', 'open', 'opens', 'permits', 'provides', 'record', 'requires', 'sealed', 'to', 'transitive', 'uses', 'var', 'when', 'with', 'yield']);
const UNUSED = ['const', 'goto'];
const RESERVED = [...new Set(Object.values(java.grammar.rules).flatMap(outermost).filter(({ type, value }) => type === 'STRING' && /^\p{L}+$/u.test(value) && !CONTEXTUAL.has(value)).map(({ value }) => value))];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SYNTAX]

const invocation = symbol('explicit_constructor_invocation');
const unnamed = widen(choiceOf(symbol('_reserved_identifier')), ($) => $.underscore_pattern);
const modified = before(symbol('_unannotated_type'), ($) => optional($.modifiers));
const binding = ($, original) => unnamed($, modified($, original));
const moved = (moving, target, place) => ($, original) => {
    const member = original.members.find(moving);
    return seq(...original.members.filter((part) => part !== member).flatMap((part) => (target(part) ? place($, member, part) : [part])));
};

// --- [LEXEMES]

const escaped = (rest) => seq(choice('\\', BACKSLASH), rest);
const digits = located(java.grammar.rules.decimal_floating_point_literal, (rule) => rule.type === 'TOKEN' && rule.content.type === 'SEQ');
const separated = (_, original) => rewrite(original, (part) => (string('_')(part) ? repeat1(part) : part));
const fractional = ($, original) =>
    separated(
        $,
        rewrite(original, (part) => (part.type === 'TOKEN' && choiceOf(string('0'))(part.content) ? digits : part)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(java, {
    reserved: { global: ($) => [...RESERVED, ...UNUSED.map((word) => $[`_${word}`])] },
    rules: {
        ...Object.fromEntries(UNUSED.map((word) => [`_${word}`, () => token(prec(1, new RegExp(word, 'u')))])),
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
        method_reference: widen(choiceOf(symbol('super')), ($) => seq($.primary_expression, '.', $.super)),
        instanceof_expression: unnamed,
        lambda_expression: unnamed,
        inferred_parameters: unnamed,
        receiver_parameter: (_, original) => prec.dynamic(1, original),
        spread_parameter: moved(
            (member) => reaches(member, symbol('_annotation')),
            string('...'),
            (_, annotations, ellipsis) => [annotations, ellipsis],
        ),
        catch_type: ($, original) => rewrite(original, (part) => (part.type === 'REPEAT' ? rewrite(part, (alternative) => (symbol('_unannotated_type')(alternative) ? $._type : alternative)) : part)),
        decimal_integer_literal: separated,
        hex_integer_literal: separated,
        octal_integer_literal: ($, original) => separated($, after(string('0'), optional('_'))($, drop(string('0o'), string('0O'))($, original))),
        binary_integer_literal: separated,
        decimal_floating_point_literal: fractional,
        hex_floating_point_literal: fractional,
        character_literal: ($, original) =>
            widen(
                choiceOf(({ type }) => type === 'PATTERN'),
                escaped(ESCAPE),
            )($, drop(({ type, value }) => type === 'PATTERN' && value.startsWith('\\\\'))($, original)),
        _escape_sequence: ($) => choice(token.immediate(escaped(LINE_TERMINATOR)), $.escape_sequence),
        escape_sequence: () => token.immediate(escaped(ESCAPE)),
    },
});
