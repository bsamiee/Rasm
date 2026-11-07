#include <stdbool.h>
#include <stdint.h>
#include <string.h>
#include <wctype.h>

#include "grammar.h"

static inline bool is_space(int32_t character);

#define iswspace is_space

#include "tree-sitter-typescript/common/scanner.h"

// --- [TYPES] ---------------------------------------------------------------------------

enum { ADJACENT_TYPE_ARGUMENTS_OPEN = ERROR_RECOVERY + 1 };

typedef struct {
    int32_t min;
    int32_t max;
} Span;

// --- [CONSTANTS] -----------------------------------------------------------------------

static const Span SPACES[] = {SPACE};

static const char TYPE_CONTINUATIONS[] = "`,.;*%>=?^|&/:";

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CHARACTERS]

static inline bool is_space(int32_t character) {
    for (size_t index = 0; index < sizeof(SPACES) / sizeof(*SPACES); index++) {
        if (character >= SPACES[index].min && character <= SPACES[index].max) {
            return true;
        }
    }
    return false;
}

static bool is_ascii_letter(int32_t character) { return (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z'); }

static bool is_decimal_digit(int32_t character) { return character >= '0' && character <= '9'; }

static bool is_hexadecimal_digit(int32_t character) { return is_decimal_digit(character) || (character >= 'a' && character <= 'f') || (character >= 'A' && character <= 'F'); }

static unsigned advance_while(TSLexer *lexer, unsigned limit, bool (*matches)(int32_t)) {
    unsigned count = 0;
    for (; count < limit && matches(lexer->lookahead); count++) {
        advance(lexer);
    }
    return count;
}

// --- [LEXEMES]

static bool scan_character_reference(TSLexer *lexer) {
    advance(lexer);
    if (lexer->lookahead != '#') {
        return advance_while(lexer, NAMED_REFERENCE_LETTERS, is_ascii_letter) > 0 && lexer->lookahead == ';';
    }
    advance(lexer);
    bool hexadecimal = lexer->lookahead == 'x' || lexer->lookahead == 'X';
    if (hexadecimal) {
        advance(lexer);
    }
    unsigned digits = hexadecimal ? advance_while(lexer, HEXADECIMAL_REFERENCE_DIGITS, is_hexadecimal_digit) : advance_while(lexer, DECIMAL_REFERENCE_DIGITS, is_decimal_digit);
    return digits > 0 && lexer->lookahead == ';';
}

static void skip_quoted(TSLexer *lexer, int32_t quote) {
    advance(lexer);
    while (lexer->lookahead != 0 && lexer->lookahead != quote && (quote == '`' || lexer->lookahead != '\n')) {
        if (lexer->lookahead == '\\') {
            advance(lexer);
        }
        advance(lexer);
    }
    advance(lexer);
}

static bool skip_comment(TSLexer *lexer) {
    advance(lexer);
    if (lexer->lookahead == '/') {
        while (lexer->lookahead != 0 && lexer->lookahead != '\n') {
            advance(lexer);
        }
        return true;
    }
    if (lexer->lookahead != '*') {
        return false;
    }
    advance(lexer);
    for (int32_t previous = 0; lexer->lookahead != 0 && !(previous == '*' && lexer->lookahead == '/');) {
        previous = lexer->lookahead;
        advance(lexer);
    }
    advance(lexer);
    return true;
}

static bool skip_trivia(TSLexer *lexer) {
    while (iswspace(lexer->lookahead)) {
        advance(lexer);
    }
    return lexer->lookahead != '/' || (skip_comment(lexer) && skip_trivia(lexer));
}

static bool skip_balanced(TSLexer *lexer, int32_t open, int32_t close) {
    for (unsigned depth = 0;;) {
        switch (lexer->lookahead) {
            case 0:
                return false;
            case '\'':
            case '"':
            case '`':
                skip_quoted(lexer, lexer->lookahead);
                break;
            case '/':
                skip_comment(lexer);
                break;
            case '=':
                advance(lexer);
                if (lexer->lookahead == '>') {
                    advance(lexer);
                }
                break;
            default:
                depth = depth + (lexer->lookahead == open) - (lexer->lookahead == close);
                advance(lexer);
                if (depth == 0) {
                    return true;
                }
        }
    }
}

// --- [TOKENS]

static bool scan_adjacent_type_arguments_open(TSLexer *lexer) {
    while (iswspace(lexer->lookahead)) {
        skip(lexer);
    }
    if (lexer->lookahead != '<') {
        return false;
    }
    advance(lexer);
    lexer->mark_end(lexer);
    lexer->result_symbol = ADJACENT_TYPE_ARGUMENTS_OPEN;
    if (lexer->lookahead != '<' || !skip_balanced(lexer, '<', '>') || !skip_trivia(lexer) || lexer->lookahead != '(' || !skip_balanced(lexer, '(', ')') || !skip_trivia(lexer) || lexer->lookahead != '=') {
        return false;
    }
    advance(lexer);
    return lexer->lookahead == '>';
}

static bool closes_annotated_pattern(TSLexer *lexer) {
    do {
        skip(lexer);
    } while (iswspace(lexer->lookahead));
    return lexer->lookahead == ':' || lexer->lookahead == '?';
}

static bool scan_type_position_semicolon(TSLexer *lexer, const bool *valid_symbols) {
    bool scanned_comment = false;
    lexer->result_symbol = AUTOMATIC_SEMICOLON;
    lexer->mark_end(lexer);
    while (iswspace(lexer->lookahead) && lexer->lookahead != '\n') {
        skip(lexer);
    }
    if (lexer->lookahead == '\n') {
        skip(lexer);
        if (!scan_whitespace_and_comments(lexer, &scanned_comment)) {
            return false;
        }
    } else if (lexer->lookahead != '}' && lexer->lookahead != 0) {
        return false;
    }
    if (lexer->lookahead == '}') {
        return !closes_annotated_pattern(lexer);
    }
    if (lexer->lookahead == '{') {
        return !valid_symbols[FUNCTION_SIGNATURE_AUTOMATIC_SEMICOLON];
    }
    return lexer->lookahead == 0 || lexer->lookahead >= 0x80 || strchr(TYPE_CONTINUATIONS, lexer->lookahead) == NULL;
}

static bool scan_template_text(TSLexer *lexer) {
    lexer->result_symbol = TEMPLATE_CHARS;
    for (bool has_content = false;; has_content = true) {
        lexer->mark_end(lexer);
        if (lexer->eof(lexer)) {
            return false;
        }
        if (lexer->lookahead == '`' || lexer->lookahead == '\\') {
            return has_content;
        }
        bool dollar = lexer->lookahead == '$';
        advance(lexer);
        if (dollar && lexer->lookahead == '{') {
            return has_content;
        }
    }
}

static bool scan_decimal_after_question(TSLexer *lexer) {
    lexer->mark_end(lexer);
    lexer->result_symbol = TERNARY_QMARK;
    advance(lexer);
    return iswdigit(lexer->lookahead);
}

static bool scan_jsx_text_from_ampersand(TSLexer *lexer, bool saw_text) {
    for (; lexer->lookahead == '&'; saw_text = true) {
        lexer->mark_end(lexer);
        if (scan_character_reference(lexer)) {
            return saw_text;
        }
        scan_jsx_text(lexer);
    }
    lexer->mark_end(lexer);
    return saw_text;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

void *tree_sitter_tsx_external_scanner_create(void) { return NULL; }

void tree_sitter_tsx_external_scanner_destroy(void *payload) {}

unsigned tree_sitter_tsx_external_scanner_serialize(void *payload, char *buffer) { return 0; }

void tree_sitter_tsx_external_scanner_deserialize(void *payload, const char *buffer, unsigned length) {}

bool tree_sitter_tsx_external_scanner_scan(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    if (valid_symbols[JSX_TEXT] && !valid_symbols[ERROR_RECOVERY]) {
        bool saw_text = scan_jsx_text(lexer);
        if (lexer->lookahead == '&') {
            return scan_jsx_text_from_ampersand(lexer, saw_text);
        }
        if (saw_text) {
            return true;
        }
    }
    if (valid_symbols[TEMPLATE_CHARS] && !valid_symbols[ERROR_RECOVERY]) {
        return scan_template_text(lexer);
    }
    if ((valid_symbols[AUTOMATIC_SEMICOLON] || valid_symbols[FUNCTION_SIGNATURE_AUTOMATIC_SEMICOLON]) && !valid_symbols[LOGICAL_OR] && !valid_symbols[TEMPLATE_CHARS] && !valid_symbols[JSX_TEXT] && !valid_symbols[ERROR_RECOVERY]) {
        return scan_type_position_semicolon(lexer, valid_symbols);
    }
    if (external_scanner_scan(payload, lexer, valid_symbols)) {
        return true;
    }
    if (valid_symbols[TERNARY_QMARK] && !valid_symbols[ERROR_RECOVERY] && lexer->lookahead == '.') {
        return scan_decimal_after_question(lexer);
    }
    return (valid_symbols[ADJACENT_TYPE_ARGUMENTS_OPEN] && valid_symbols[LOGICAL_OR] && !valid_symbols[ERROR_RECOVERY] && scan_adjacent_type_arguments_open(lexer));
}
