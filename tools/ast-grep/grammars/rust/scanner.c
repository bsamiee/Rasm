#define tree_sitter_rust_external_scanner_scan(...) rust_upstream_scan(__VA_ARGS__)

#include "tree-sitter-rust/src/scanner.c"

// --- [CONSTANTS] -----------------------------------------------------------------------

enum { FIELD_LETTER = 'a', FRONTMATTER = ERROR_SENTINEL + 1, SHEBANG, LONE_DOLLAR, FENCE_MINIMUM = 3 };

// --- [MODELS] --------------------------------------------------------------------------

typedef struct {
    TSLexer lexer;
    TSLexer *inner;
} FieldLexer;

// --- [OPERATIONS] ----------------------------------------------------------------------

static uint32_t fence_length(TSLexer *lexer) {
    uint32_t length = 0;
    for (; lexer->lookahead == '-'; length++) {
        advance(lexer);
    }
    return length;
}

static void skip_line(TSLexer *lexer) {
    while (lexer->lookahead != '\n' && !lexer->eof(lexer)) {
        advance(lexer);
    }
}

static void skip_block_comment(TSLexer *lexer, bool *line_marked) {
    for (uint32_t depth = 1; depth > 0 && !lexer->eof(lexer);) {
        int32_t current = lexer->lookahead;
        advance(lexer);
        if (current == '\n' && !*line_marked) {
            lexer->mark_end(lexer);
            *line_marked = true;
        } else if (current == '/' && lexer->lookahead == '*') {
            advance(lexer);
            depth++;
        } else if (current == '*' && lexer->lookahead == '/') {
            advance(lexer);
            depth--;
        }
    }
}

static bool scan_shebang(TSLexer *lexer) {
    advance(lexer);
    if (lexer->lookahead != '!') {
        return false;
    }
    advance(lexer);
    bool line_marked = false;
    for (;;) {
        if (lexer->lookahead == '[') {
            return false;
        }
        if (lexer->eof(lexer)) {
            break;
        }
        if (lexer->lookahead == '\n' && !line_marked) {
            advance(lexer);
            lexer->mark_end(lexer);
            line_marked = true;
        } else if (iswspace(lexer->lookahead)) {
            advance(lexer);
        } else if (lexer->lookahead == '/') {
            advance(lexer);
            if (lexer->lookahead == '/') {
                skip_line(lexer);
            } else if (lexer->lookahead == '*') {
                advance(lexer);
                skip_block_comment(lexer, &line_marked);
            } else {
                break;
            }
        } else {
            break;
        }
    }
    if (!line_marked) {
        skip_line(lexer);
        advance(lexer);
        lexer->mark_end(lexer);
    }
    lexer->result_symbol = SHEBANG;
    return true;
}

static bool scan_frontmatter(TSLexer *lexer) {
    uint32_t opening = fence_length(lexer);
    if (opening < FENCE_MINIMUM) {
        return false;
    }
    for (;;) {
        skip_line(lexer);
        if (lexer->eof(lexer)) {
            return false;
        }
        advance(lexer);
        if (fence_length(lexer) >= opening) {
            skip_line(lexer);
            lexer->result_symbol = FRONTMATTER;
            lexer->mark_end(lexer);
            return true;
        }
    }
}

static bool scan_lone_dollar(TSLexer *lexer) {
    advance(lexer);
    lexer->mark_end(lexer);
    while (iswspace(lexer->lookahead)) {
        advance(lexer);
    }
    lexer->result_symbol = LONE_DOLLAR;
    return lexer->lookahead != '(' && lexer->lookahead != '_' && !iswalpha(lexer->lookahead) && lexer->lookahead < 0x80;
}

static void field_advance(TSLexer *lexer, bool skipped) {
    FieldLexer *self = (FieldLexer *)lexer;
    bool dot = self->lexer.lookahead == '.';
    self->inner->advance(self->inner, skipped);
    self->lexer.lookahead = dot && self->inner->lookahead == '_' ? FIELD_LETTER : self->inner->lookahead;
}

static void field_mark_end(TSLexer *lexer) {
    FieldLexer *self = (FieldLexer *)lexer;
    self->inner->mark_end(self->inner);
}

static uint32_t field_get_column(TSLexer *lexer) {
    FieldLexer *self = (FieldLexer *)lexer;
    return self->inner->get_column(self->inner);
}

static bool field_is_at_included_range_start(const TSLexer *lexer) {
    const FieldLexer *self = (const FieldLexer *)lexer;
    return self->inner->is_at_included_range_start(self->inner);
}

static bool field_eof(const TSLexer *lexer) {
    const FieldLexer *self = (const FieldLexer *)lexer;
    return self->inner->eof(self->inner);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

bool(tree_sitter_rust_external_scanner_scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    if (valid_symbols[SHEBANG] && !valid_symbols[ERROR_SENTINEL] && lexer->lookahead == '#') {
        return scan_shebang(lexer);
    }
    if (valid_symbols[FRONTMATTER] && !valid_symbols[ERROR_SENTINEL]) {
        while (iswspace(lexer->lookahead)) {
            skip(lexer);
        }
        if (lexer->lookahead == '-') {
            return scan_frontmatter(lexer);
        }
    }
    if (valid_symbols[LONE_DOLLAR] && !valid_symbols[ERROR_SENTINEL]) {
        while (iswspace(lexer->lookahead)) {
            skip(lexer);
        }
        if (lexer->lookahead == '$') {
            return scan_lone_dollar(lexer);
        }
    }
    if (!valid_symbols[FLOAT_LITERAL]) {
        return rust_upstream_scan(payload, lexer, valid_symbols);
    }
    FieldLexer field = {
        .lexer = {
            .lookahead = lexer->lookahead,
            .result_symbol = lexer->result_symbol,
            .advance = field_advance,
            .mark_end = field_mark_end,
            .get_column = field_get_column,
            .is_at_included_range_start = field_is_at_included_range_start,
            .eof = field_eof,
        },
        .inner = lexer,
    };
    bool found = rust_upstream_scan(payload, &field.lexer, valid_symbols);
    lexer->result_symbol = field.lexer.result_symbol;
    return found;
}
