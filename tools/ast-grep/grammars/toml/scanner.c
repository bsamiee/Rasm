#define tree_sitter_toml_external_scanner_scan(...) toml_upstream_scan(__VA_ARGS__)

#include "@tree-sitter-grammars/tree-sitter-toml/src/scanner.c"

// --- [CONSTANTS] -----------------------------------------------------------------------

static const uint32_t DELIMITER_RUN = 3;

// --- [OPERATIONS] ----------------------------------------------------------------------

static bool held_character(TSLexer *lexer, int32_t delimiter) {
    int32_t code = lexer->lookahead;
    if (lexer->eof(lexer) || (code == '\\' && delimiter == '"')) {
        return false;
    }
    if (code == '\r') {
        lexer->advance(lexer, false);
        return lexer->lookahead == '\n';
    }
    return code == '\t' || code == '\n' || (code > 0x1F && code != 0x7F);
}

static bool scan_text(TSLexer *lexer, int32_t delimiter, TokenType content, TokenType end) {
    lexer->result_symbol = content;
    for (bool held = false;; held = true) {
        if (lexer->lookahead == delimiter) {
            uint32_t run = 0;
            for (; lexer->lookahead == delimiter; run++) {
                lexer->advance(lexer, false);
                if (!held && run == 0) {
                    lexer->mark_end(lexer);
                }
            }
            if (!held && run == DELIMITER_RUN) {
                lexer->mark_end(lexer);
                lexer->result_symbol = end;
            }
            if (run >= DELIMITER_RUN) {
                return held || run < 2 * DELIMITER_RUN;
            }
        } else if (held_character(lexer, delimiter)) {
            lexer->advance(lexer, false);
        } else {
            return held;
        }
        lexer->mark_end(lexer);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

bool(tree_sitter_toml_external_scanner_scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    bool basic = valid_symbols[MULTILINE_BASIC_STRING_CONTENT];
    bool literal = valid_symbols[MULTILINE_LITERAL_STRING_CONTENT];
    if (basic != literal) {
        return basic ? scan_text(lexer, '"', MULTILINE_BASIC_STRING_CONTENT, MULTILINE_BASIC_STRING_END) : scan_text(lexer, '\'', MULTILINE_LITERAL_STRING_CONTENT, MULTILINE_LITERAL_STRING_END);
    }
    return toml_upstream_scan(payload, lexer, valid_symbols);
}
