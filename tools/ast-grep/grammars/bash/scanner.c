#ifndef SCANNER
#define SCANNER(fn) tree_sitter_bash_external_scanner_##fn
#endif
#define tree_sitter_bash_external_scanner_create SCANNER(create)
#define tree_sitter_bash_external_scanner_destroy SCANNER(destroy)
#define tree_sitter_bash_external_scanner_serialize SCANNER(serialize)
#define tree_sitter_bash_external_scanner_deserialize SCANNER(deserialize)
#define tree_sitter_bash_external_scanner_scan bash_upstream_scan
#include "tree-sitter-bash/src/scanner.c"
#undef tree_sitter_bash_external_scanner_scan

// --- [OPERATIONS] ----------------------------------------------------------------------

static inline bool joins(TSLexer *lexer, const bool *valid_symbols) {
    return valid_symbols[CONCAT] && !in_error_recovery(valid_symbols) && (lexer->lookahead == '\\' || (lexer->lookahead == ')' && valid_symbols[CLOSING_BRACE]));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

bool SCANNER(scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    if (!joins(lexer, valid_symbols)) {
        return bash_upstream_scan(payload, lexer, valid_symbols);
    }
    lexer->result_symbol = CONCAT;
    lexer->mark_end(lexer);
    if (lexer->lookahead == ')') {
        return true;
    }
    advance(lexer);
    return !lexer->eof(lexer) && !iswspace(lexer->lookahead);
}
