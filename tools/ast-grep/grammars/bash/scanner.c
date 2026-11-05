#ifndef SCANNER
#define SCANNER(fn) tree_sitter_bash_external_scanner_##fn
#endif
#define tree_sitter_bash_external_scanner_create SCANNER(create)
#define tree_sitter_bash_external_scanner_destroy SCANNER(destroy)
#define tree_sitter_bash_external_scanner_serialize SCANNER(serialize)
#define tree_sitter_bash_external_scanner_deserialize SCANNER(deserialize)
#pragma push_macro("tree_sitter_bash_external_scanner_scan")
#define tree_sitter_bash_external_scanner_scan bash_upstream_scan
#include "tree-sitter-bash/src/scanner.c"
#pragma pop_macro("tree_sitter_bash_external_scanner_scan")

// --- [TYPES] ---------------------------------------------------------------------------

enum { TIME_POSIX = ERROR_RECOVERY + 1 };

// --- [OPERATIONS] ----------------------------------------------------------------------

static inline bool joins(TSLexer *lexer, const bool *valid_symbols) {
    return valid_symbols[CONCAT] && (lexer->lookahead == '\\' || (lexer->lookahead == ')' && valid_symbols[CLOSING_BRACE]));
}

static inline bool ends(TSLexer *lexer) {
    return lexer->eof(lexer) || iswspace(lexer->lookahead);
}

static bool posix(TSLexer *lexer) {
    lexer->result_symbol = TIME_POSIX;
    advance(lexer);
    if (lexer->lookahead != 'p') {
        return false;
    }
    advance(lexer);
    lexer->mark_end(lexer);
    return ends(lexer) || lexer->lookahead == ';' || lexer->lookahead == '&' || lexer->lookahead == '|';
}

static bool concat(TSLexer *lexer) {
    lexer->result_symbol = CONCAT;
    lexer->mark_end(lexer);
    if (lexer->lookahead == ')') {
        return true;
    }
    advance(lexer);
    return !ends(lexer);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

bool SCANNER(scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    if (in_error_recovery(valid_symbols)) {
        return bash_upstream_scan(payload, lexer, valid_symbols);
    }
    if (valid_symbols[TIME_POSIX]) {
        while (lexer->lookahead == ' ' || lexer->lookahead == '\t') {
            skip(lexer);
        }
        if (lexer->lookahead == '-') {
            return posix(lexer);
        }
    }
    return joins(lexer, valid_symbols) ? concat(lexer) : bash_upstream_scan(payload, lexer, valid_symbols);
}
