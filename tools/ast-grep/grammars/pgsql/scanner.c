#include <stdbool.h>
#include <stdint.h>

#pragma push_macro("tree_sitter_postgres_external_scanner_scan")
#undef tree_sitter_postgres_external_scanner_scan
#define tree_sitter_postgres_external_scanner_scan postgres_upstream_scan
#include "tree-sitter-postgres/postgres/src/scanner.c"
#undef tree_sitter_postgres_external_scanner_scan
#pragma pop_macro("tree_sitter_postgres_external_scanner_scan")

// --- [TYPES] ---------------------------------------------------------------------------

enum { OPERATOR = DOLLAR_QUOTED_STRING + 1, COMMENT };

// --- [MODELS] --------------------------------------------------------------------------

typedef struct {
    int32_t head[2];
    uint32_t length;
    uint32_t marked;
    bool qualified;
} Run;

// --- [OPERATIONS] ----------------------------------------------------------------------

static bool spacing(int32_t character) { return character == ' ' || (character >= '\t' && character <= '\r'); }

static bool sign(int32_t character) { return character == '+' || character == '-'; }

static bool qualifying(int32_t character) {
    switch (character) {
        case '~':
        case '!':
        case '@':
        case '#':
        case '^':
        case '&':
        case '|':
        case '?':
        case '%':
            return true;
        default:
            return false;
    }
}

static bool operating(int32_t character) {
    switch (character) {
        case '+':
        case '-':
        case '*':
        case '/':
        case '<':
        case '>':
        case '=':
            return true;
        default:
            return qualifying(character);
    }
}

static bool opens_comment(int32_t character, int32_t next) { return (character == '-' && next == '-') || (character == '/' && next == '*'); }

static bool self_lexed(int32_t first, int32_t second, uint32_t length) {
    if (length == 1) {
        return first == '%' || first == '^' || (operating(first) && !qualifying(first));
    }
    return length == 2 && ((second == '=' && (first == '<' || first == '>' || first == '!')) || (first == '<' && second == '>') || (first == '=' && second == '>'));
}

static bool extended(Run *run, TSLexer *lexer, int32_t character) {
    if (opens_comment(character, lexer->lookahead)) {
        return false;
    }
    if (run->length < 2) {
        run->head[run->length] = character;
    }
    run->length++;
    run->qualified = run->qualified || qualifying(character);
    if (run->qualified || !sign(character) || run->length == 1) {
        lexer->mark_end(lexer);
        run->marked = run->length;
    }
    return true;
}

static bool scan_operator(TSLexer *lexer, int32_t taken) {
    Run run = {.head = {0, 0}, .length = 0, .marked = 0, .qualified = false};
    bool open = taken == 0 || extended(&run, lexer, taken);
    while (open && operating(lexer->lookahead) && !lexer->eof(lexer)) {
        int32_t character = lexer->lookahead;
        lexer->advance(lexer, false);
        open = extended(&run, lexer, character);
    }
    lexer->result_symbol = OPERATOR;
    return run.marked > 0 && !self_lexed(run.head[0], run.head[1], run.marked);
}

static bool scan_block(TSLexer *lexer) {
    lexer->advance(lexer, false);
    for (unsigned depth = 1; depth > 0;) {
        if (lexer->eof(lexer)) {
            return false;
        }
        int32_t character = lexer->lookahead;
        lexer->advance(lexer, false);
        if (character == '/' && lexer->lookahead == '*') {
            lexer->advance(lexer, false);
            depth++;
        } else if (character == '*' && lexer->lookahead == '/') {
            lexer->advance(lexer, false);
            depth--;
        }
    }
    lexer->mark_end(lexer);
    lexer->result_symbol = COMMENT;
    return true;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

bool tree_sitter_postgres_external_scanner_scan(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    while (spacing(lexer->lookahead)) {
        lexer->advance(lexer, true);
    }
    if (lexer->lookahead == '$') {
        return postgres_upstream_scan(payload, lexer, valid_symbols);
    }
    int32_t taken = 0;
    if (lexer->lookahead == '/' && valid_symbols[COMMENT]) {
        lexer->advance(lexer, false);
        if (lexer->lookahead == '*') {
            return scan_block(lexer);
        }
        taken = '/';
    }
    return valid_symbols[OPERATOR] && scan_operator(lexer, taken);
}
