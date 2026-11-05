#define tree_sitter_plpgsql_external_scanner_create(...) plpgsql_upstream_create(__VA_ARGS__)
#define tree_sitter_plpgsql_external_scanner_destroy(...) plpgsql_upstream_destroy(__VA_ARGS__)
#define tree_sitter_plpgsql_external_scanner_serialize(...) plpgsql_upstream_serialize(__VA_ARGS__)
#define tree_sitter_plpgsql_external_scanner_deserialize(...) plpgsql_upstream_deserialize(__VA_ARGS__)
#define tree_sitter_plpgsql_external_scanner_scan(...) plpgsql_upstream_scan(__VA_ARGS__)
#ifndef SCANNER
#define SCANNER(fn) tree_sitter_plpgsql_external_scanner_##fn
#endif

#include "tree-sitter-postgres/plpgsql/src/scanner.c"

#include "tree_sitter/alloc.h"

// --- [TYPES] ---------------------------------------------------------------------------

enum { TEMPLATE_OPEN = SQL_UNTIL_SEMICOLON_GUARDED + 1, DOLLAR_QUOTED, COMMENT, USING_ARGUMENT, FETCH_COUNT, FETCH_EXPRESSION, SUBSTITUTION, FORMAT_STRING };

typedef enum { CODE, QUOTED, TAG, BODY, LINE, BLOCK } Region;

// --- [CONSTANTS] -----------------------------------------------------------------------

enum { OPAQUE = '_', WIDE = 0x80, WORD_SIZE = 32, TAG_SIZE = 64 };

static const char *const RESERVED[] = {"begin", "case", "declare", "else", "end", "for", "foreach", "if", "loop", "null", "when", "while"};

// --- [MODELS] --------------------------------------------------------------------------

typedef struct {
    TSLexer lexer;
    TSLexer *inner;
    bool templated;
    Region region;
    bool escaped;
    bool paired;
    bool spaced;
    uint32_t holes;
    uint64_t resumes;
    int32_t script;
    int32_t closer;
    bool escapes;
    bool held;
    uint32_t comments;
    char tag[TAG_SIZE];
    int32_t tag_length;
    int32_t matched;
    bool word;
    bool dotted;
    bool prefix;
    bool kept;
    char text[WORD_SIZE];
    uint32_t text_length;
    uint32_t depth;
    uint32_t words;
    bool query;
    bool targeting;
    bool bare;
    bool cut;
} Lexer;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [WORDS]

static bool is_space(int32_t c) { return c == ' ' || (c >= '\t' && c <= '\r'); }

static bool is_reserved_statement(const char *word) {
    for (size_t at = 0; at < sizeof(RESERVED) / sizeof(*RESERVED); at++) {
        if (strcmp(word, RESERVED[at]) == 0) {
            return true;
        }
    }
    return false;
}

static int32_t folded(int32_t c) { return c < WIDE ? c : WIDE | (c & (WIDE - 1)); }

// --- [REGIONS]

static void code_passed(Lexer *self, int32_t c, int32_t next) {
    bool lead = !self->word && is_tag_start_char(c);
    bool inside = lead || (self->word && (is_tag_char(c) || c == '$'));
    bool named = lead && self->depth == 0 && !self->dotted;
    if (self->word && !inside && self->words == 1) {
        self->query = is_sql_statement_start(self->text);
    }
    self->words += named;
    self->text_length = named ? 0 : self->text_length;
    self->kept = named || (self->kept && inside);
    if (self->kept && self->text_length + 1 < WORD_SIZE) {
        self->text[self->text_length] = c < WIDE ? ascii_tolower(c) : OPAQUE;
        self->text[++self->text_length] = '\0';
    }
    if (c == '\'' || c == '"') {
        self->region = QUOTED;
        self->closer = c;
        self->escapes = c == '\'' && self->prefix;
    } else if (c == '$' && !self->word) {
        self->region = TAG;
        self->tag_length = 0;
    } else if (c == '-' && next == '-') {
        self->region = LINE;
    } else if (c == '/' && next == '*') {
        self->region = BLOCK;
        self->comments = 1;
        self->paired = true;
    }
    self->depth = self->depth + (c == '(' || c == '[') - (self->depth > 0 && (c == ')' || c == ']'));
    self->prefix = lead && (c == 'e' || c == 'E');
    self->word = inside;
    self->dotted = c == '.';
    bool bare = self->bare && (is_tag_char(c) || is_space(c));
    self->cut = self->targeting && self->bare && self->words == 1 && next == '=' && (bare || c == ':');
    if (self->cut && bare && !self->spaced) {
        self->inner->mark_end(self->inner);
    }
    self->bare = bare;
}

static void text_passed(Lexer *self, int32_t c, int32_t next) {
    switch (self->region) {
        case CODE:
            code_passed(self, c, next);
            break;
        case QUOTED:
            if (self->held) {
                self->held = false;
            } else if (self->escapes && c == '\\') {
                self->held = true;
            } else if (c == self->closer && next == c) {
                self->paired = true;
            } else if (c == self->closer) {
                self->region = CODE;
            }
            break;
        case TAG:
            if (c == '$') {
                self->region = BODY;
                self->matched = -1;
            } else if (self->tag_length + 1 < TAG_SIZE && (self->tag_length > 0 ? is_tag_char(c) : is_tag_start_char(c))) {
                self->tag[self->tag_length++] = (char)folded(c);
            } else {
                self->region = CODE;
                code_passed(self, c, next);
            }
            break;
        case BODY:
            if (self->matched == self->tag_length && c == '$') {
                self->region = CODE;
            } else if (self->matched >= 0 && self->matched < self->tag_length && folded(c) == (unsigned char)self->tag[self->matched]) {
                self->matched++;
            } else {
                self->matched = c == '$' ? 0 : -1;
            }
            break;
        case LINE:
            self->region = next == '\n' || next == '\r' ? CODE : LINE;
            break;
        case BLOCK:
            self->comments = self->comments + (c == '/' && next == '*') - (c == '*' && next == '/');
            self->paired = (c == '/' && next == '*') || (c == '*' && next == '/');
            break;
    }
}

// --- [HOLES]

static void hole_passed(Lexer *self, int32_t c, int32_t next) {
    switch (self->script) {
        case 0:
            if (c == '\'' || c == '"' || c == '`') {
                self->script = c;
            } else if (c == '/' && (next == '/' || next == '*')) {
                self->script = next;
                self->paired = next == '*';
            } else if (c == '{') {
                self->holes++;
                self->resumes <<= 1;
            } else if (c == '}') {
                self->holes--;
                self->script = (self->resumes & 1) != 0 ? '`' : 0;
                self->resumes >>= 1;
                self->word = self->holes == 0 || self->word;
            }
            break;
        case '/':
            self->script = c == '\n' ? 0 : '/';
            break;
        case '*':
            self->script = c == '*' && next == '/' ? 0 : '*';
            self->paired = self->script == 0;
            break;
        case '`':
            if (c == '$' && next == '{') {
                self->script = 0;
                self->holes++;
                self->resumes = (self->resumes << 1) | 1;
                self->paired = true;
            } else if (c == '`') {
                self->script = 0;
            }
            break;
        default:
            self->script = c == self->script ? 0 : self->script;
            break;
    }
}

// --- [VIEW]

static bool closes(const Lexer *self) { return self->templated && self->holes == 0 && !self->escaped && self->inner->lookahead == '`'; }

static int32_t shown(const Lexer *self) {
    int32_t c = self->inner->lookahead;
    if (closes(self) || self->cut) {
        return 0;
    }
    if (self->holes > 0 && !self->paired) {
        return OPAQUE;
    }
    switch (self->region) {
        case CODE:
            return c >= WIDE || (c == '$' && self->word) || (self->dotted && is_tag_start_char(c)) ? OPAQUE : c;
        case QUOTED:
            return self->escapes && (self->held || c == '\\') ? OPAQUE : c;
        case TAG:
        case BODY:
            return folded(c);
        default:
            return c;
    }
}

static void passed(Lexer *self, int32_t c) {
    int32_t next = self->inner->lookahead;
    bool escaped = self->escaped;
    self->escaped = self->templated && c == '\\' && !escaped;
    if (self->paired) {
        self->paired = false;
        self->region = self->region == BLOCK && self->comments == 0 ? CODE : self->region;
    } else if (self->holes > 0) {
        if (!escaped) {
            hole_passed(self, c, next);
        }
    } else if (self->templated && c == '$' && next == '{' && !escaped) {
        self->holes = 1;
        self->paired = true;
    } else if (!self->escaped) {
        text_passed(self, c, next);
    }
}

static void view_advance(TSLexer *lexer, bool skipped) {
    Lexer *self = (Lexer *)lexer;
    int32_t c = self->inner->lookahead;
    bool space = !skipped && is_space(c);
    if (space && !self->spaced) {
        self->inner->mark_end(self->inner);
    }
    self->spaced = space;
    self->inner->advance(self->inner, skipped);
    passed(self, c);
    self->lexer.lookahead = shown(self);
}

static void view_mark_end(TSLexer *lexer) {
    Lexer *self = (Lexer *)lexer;
    if (!self->spaced && !self->cut) {
        self->inner->mark_end(self->inner);
    }
}

// --- [TOKENS]

static bool recovering(const bool *valid_symbols) { return valid_symbols[SQL_STATEMENT] && valid_symbols[SQL_UNTIL_THEN]; }

static bool finished(Lexer *self, int32_t symbol) {
    while (self->region != CODE && self->lexer.lookahead != 0) {
        self->lexer.advance(&self->lexer, false);
    }
    self->lexer.mark_end(&self->lexer);
    self->lexer.result_symbol = symbol;
    return self->region == CODE || self->region == LINE;
}

static bool format_string(Lexer *self, const bool *valid_symbols) {
    self->lexer.advance(&self->lexer, false);
    return valid_symbols[FORMAT_STRING] && finished(self, FORMAT_STRING);
}

static bool literal(Lexer *self, const bool *valid_symbols) {
    TSLexer *lexer = &self->lexer;
    if (lexer->lookahead == '\'') {
        return format_string(self, valid_symbols);
    }
    if (lexer->lookahead == '$') {
        lexer->advance(lexer, false);
        if (self->holes == 0) {
            while (self->region == TAG && lexer->lookahead != 0) {
                lexer->advance(lexer, false);
            }
            return valid_symbols[DOLLAR_QUOTED] && self->region == BODY && finished(self, DOLLAR_QUOTED);
        }
    }
    bool held = false;
    while (self->holes > 0 || is_tag_char(lexer->lookahead) || lexer->lookahead == '$') {
        held = held || self->holes > 0;
        lexer->advance(lexer, false);
    }
    if (!held && self->prefix && lexer->lookahead == '\'') {
        return format_string(self, valid_symbols);
    }
    lexer->mark_end(lexer);
    lexer->result_symbol = SUBSTITUTION;
    return held && valid_symbols[SUBSTITUTION];
}

static bool assignment_or_statement(Lexer *self, const bool *valid_symbols) {
    TSLexer *lexer = &self->lexer;
    self->targeting = true;
    self->bare = true;
    if (lexer->lookahead == '#') {
        return false;
    }
    if (scan_assignment_or_statement(lexer, valid_symbols)) {
        lexer->result_symbol = self->cut ? SQL_UNTIL_ASSIGNMENT : lexer->result_symbol;
        return true;
    }
    if (!self->word || is_reserved_statement(self->text)) {
        return false;
    }
    self->targeting = false;
    lexer->mark_end(lexer);
    while (lexer->lookahead != 0 && (is_space(lexer->lookahead) || self->region != CODE || lexer->lookahead == '-' || lexer->lookahead == '/')) {
        lexer->advance(lexer, false);
    }
    if (lexer->lookahead == '[') {
        return scan_assignment_or_statement(lexer, valid_symbols);
    }
    if (lexer->lookahead == ':') {
        lexer->advance(lexer, false);
    }
    lexer->result_symbol = SQL_UNTIL_ASSIGNMENT;
    return lexer->lookahead == '=';
}

static bool range_or_query(Lexer *self, const bool *valid_symbols) {
    TSLexer *lexer = &self->lexer;
    bool named = lexer->lookahead == '"' || is_tag_start_char(lexer->lookahead);
    bool found = plpgsql_upstream_scan(NULL, lexer, valid_symbols);
    return found && !(named && lexer->result_symbol == SQL_UNTIL_LOOP && self->words <= 2 && !self->query);
}

static bool using_argument(Lexer *self, const bool *valid_symbols) {
    ScanMode mode = mode_for(SQL_UNTIL_COMMA_OR_SEMICOLON);
    mode.symbol = (enum TokenType)USING_ARGUMENT;
    mode.stop_into = true;
    return scan_sql(&self->lexer, mode, valid_symbols);
}

static bool fetch_expression(Lexer *self, int32_t symbol, const bool *valid_symbols) {
    TSLexer *lexer = &self->lexer;
    bool holed = lexer->lookahead == '$';
    bool content = false;
    lexer->result_symbol = symbol;
    while (lexer->lookahead != 0 && (self->region != CODE || self->depth > 0 || lexer->lookahead != ';')) {
        int32_t c = lexer->lookahead;
        bool spanned = self->region == LINE || self->region == BLOCK;
        if (self->region == CODE && self->depth == 0 && self->holes == 0 && is_tag_start_char(c)) {
            lexer->mark_end(lexer);
            while (is_tag_char(lexer->lookahead)) {
                lexer->advance(lexer, false);
            }
            if (strcmp(self->text, "from") == 0 || strcmp(self->text, "in") == 0) {
                return content;
            }
            if (strcmp(self->text, "into") == 0) {
                break;
            }
            if (!content && strcmp(self->text, "all") == 0) {
                return false;
            }
            content = true;
        } else {
            lexer->advance(lexer, false);
            content = content || !(is_space(c) || spanned || self->region == LINE || self->region == BLOCK);
            if (holed && self->words == 0) {
                lexer->mark_end(lexer);
            }
        }
    }
    lexer->result_symbol = SUBSTITUTION;
    return holed && self->words <= 1 && valid_symbols[SUBSTITUTION];
}

static bool token(Lexer *self, const bool *valid_symbols) {
    TSLexer *lexer = &self->lexer;
    skip_whitespace(lexer);
    int32_t lead = lexer->lookahead;
    if (valid_symbols[COMMENT] && (lead == '-' || lead == '/')) {
        lexer->advance(lexer, false);
        if (self->region == LINE || self->region == BLOCK) {
            return finished(self, COMMENT);
        }
    }
    if (recovering(valid_symbols)) {
        return plpgsql_upstream_scan(NULL, lexer, valid_symbols);
    }
    if (valid_symbols[SQL_UNTIL_ASSIGNMENT] && valid_symbols[SQL_STATEMENT]) {
        return assignment_or_statement(self, valid_symbols);
    }
    if (valid_symbols[SQL_UNTIL_RANGE] && valid_symbols[SQL_UNTIL_LOOP]) {
        return range_or_query(self, valid_symbols);
    }
    if (valid_symbols[USING_ARGUMENT]) {
        return using_argument(self, valid_symbols);
    }
    if (valid_symbols[FETCH_EXPRESSION]) {
        return fetch_expression(self, FETCH_EXPRESSION, valid_symbols);
    }
    if (valid_symbols[FETCH_COUNT] && (is_ascii_digit(lead) || lead == '-' || lead == '+' || lead == '(')) {
        return fetch_expression(self, FETCH_COUNT, valid_symbols);
    }
    if ((valid_symbols[DOLLAR_QUOTED] && lead == '$') || (self->templated && (valid_symbols[SUBSTITUTION] || valid_symbols[FORMAT_STRING]))) {
        return literal(self, valid_symbols);
    }
    return plpgsql_upstream_scan(NULL, lexer, valid_symbols);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

void *(SCANNER(create))(void) { return ts_calloc(1, sizeof(bool)); }

void(SCANNER(destroy))(void *payload) { ts_free(payload); }

unsigned(SCANNER(serialize))(void *payload, char *buffer) {
    buffer[0] = (char)*(bool *)payload;
    return 1;
}

void(SCANNER(deserialize))(void *payload, const char *buffer, unsigned length) { *(bool *)payload = length > 0 && buffer[0] != 0; }

bool(SCANNER(scan))(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    bool *templated = (bool *)payload;
    if (valid_symbols[TEMPLATE_OPEN] && !recovering(valid_symbols) && lexer->lookahead == '`') {
        lexer->advance(lexer, false);
        lexer->result_symbol = TEMPLATE_OPEN;
        *templated = true;
        return true;
    }
    Lexer view = {
        .lexer = {
            .result_symbol = lexer->result_symbol,
            .advance = view_advance,
            .mark_end = view_mark_end,
        },
        .inner = lexer,
        .templated = *templated,
    };
    view.lexer.lookahead = shown(&view);
    bool found = token(&view, valid_symbols);
    lexer->result_symbol = view.lexer.result_symbol;
    return found;
}
