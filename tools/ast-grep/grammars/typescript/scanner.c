#include "tree-sitter-typescript/common/scanner.h"

#ifndef SCANNER
#define SCANNER(fn) tree_sitter_tsx_external_scanner_##fn
#endif

// --- [TYPES] ---------------------------------------------------------------------------

enum { ADJACENT_TYPE_ARGUMENTS_OPEN = ERROR_RECOVERY + 1 };

// --- [CONSTANTS] -----------------------------------------------------------------------

enum { DECIMAL_REFERENCE_DIGITS = 7, HEXADECIMAL_REFERENCE_DIGITS = 6, NAMED_REFERENCE_LETTERS = 30 };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CHARACTERS]

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

void *SCANNER(create)(void) { return NULL; }

void SCANNER(destroy)(void *payload) {}

unsigned SCANNER(serialize)(void *payload, char *buffer) { return 0; }

void SCANNER(deserialize)(void *payload, const char *buffer, unsigned length) {}

bool SCANNER(scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    if (valid_symbols[JSX_TEXT] && !valid_symbols[ERROR_RECOVERY]) {
        bool saw_text = scan_jsx_text(lexer);
        if (lexer->lookahead == '&') {
            return scan_jsx_text_from_ampersand(lexer, saw_text);
        }
        if (saw_text) {
            return true;
        }
    }
    return external_scanner_scan(payload, lexer, valid_symbols) ||
           (valid_symbols[ADJACENT_TYPE_ARGUMENTS_OPEN] && valid_symbols[LOGICAL_OR] && !valid_symbols[ERROR_RECOVERY] && scan_adjacent_type_arguments_open(lexer));
}
