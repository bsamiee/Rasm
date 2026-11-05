#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>
#include <string.h>

#include "tree_sitter/parser.h"

#include "holes.h"

// --- [TYPES] ---------------------------------------------------------------------------

enum { HOLE = HOLE_SYMBOL - 1, HOLE_END = HOLE_SYMBOL };

// --- [MODELS] --------------------------------------------------------------------------

typedef struct {
    const char *opening;
    int32_t closing;
} Form;

typedef struct {
    int32_t minimum;
    int32_t maximum;
} Span;

typedef struct {
    TSLexer lexer;
    TSLexer *source;
    const char *replayed;
    size_t remaining;
} Replay;

// --- [SERVICES] ------------------------------------------------------------------------

bool dialect_scan(void *, TSLexer *, const bool *);

static const Form FORMS[] = {HOLE_FORMS};

static const Span TAIL[] = {HOLE_TAIL};

static const char *const WORDS[] = {HOLE_WORDS};

static int32_t replay_lookahead(const Replay *self) { return self->remaining > 0 ? (unsigned char)*self->replayed : self->source->lookahead; }

static void replay_advance(TSLexer *lexer, bool skip) {
    Replay *self = (Replay *)lexer;
    if (self->remaining > 0) {
        self->replayed++;
        self->remaining--;
    } else {
        self->source->advance(self->source, skip);
    }
    lexer->lookahead = replay_lookahead(self);
}

static void replay_mark_end(TSLexer *lexer) {
    Replay *self = (Replay *)lexer;
    self->source->mark_end(self->source);
}

static uint32_t replay_get_column(TSLexer *lexer) {
    Replay *self = (Replay *)lexer;
    return self->source->get_column(self->source);
}

static bool replay_is_at_included_range_start(const TSLexer *lexer) {
    const Replay *self = (const Replay *)lexer;
    return self->source->is_at_included_range_start(self->source);
}

static bool replay_eof(const TSLexer *lexer) {
    const Replay *self = (const Replay *)lexer;
    return self->remaining == 0 && self->source->eof(self->source);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [LEXEMES]

static void advance(TSLexer *lexer) { lexer->advance(lexer, false); }

static size_t walked(TSLexer *lexer, const char *text) {
    size_t length = 0;
    for (; text[length] != 0 && !lexer->eof(lexer) && lexer->lookahead == (unsigned char)text[length]; length++) {
        advance(lexer);
    }
    return length;
}

static bool skip_comment(TSLexer *lexer) {
    advance(lexer);
    if (lexer->lookahead == '/') {
        while (!lexer->eof(lexer) && lexer->lookahead != '\n') {
            advance(lexer);
        }
        return true;
    }
    if (lexer->lookahead != '*') {
        return true;
    }
    advance(lexer);
    for (int32_t previous = 0; !(previous == '*' && lexer->lookahead == '/');) {
        if (lexer->eof(lexer)) {
            return false;
        }
        previous = lexer->lookahead;
        advance(lexer);
    }
    advance(lexer);
    return true;
}

// --- [HOLES]

static const Form *opened_by(int32_t character) {
    for (const Form *form = FORMS; form < FORMS + sizeof FORMS / sizeof *FORMS; form++) {
        if (character == (unsigned char)form->opening[0]) {
            return form;
        }
    }
    return NULL;
}

static bool closes_hole(int32_t character) {
    for (const Form *form = FORMS; form < FORMS + sizeof FORMS / sizeof *FORMS; form++) {
        if (character == form->closing) {
            return true;
        }
    }
    return false;
}

static bool continues_name(int32_t character) {
    for (const Span *span = TAIL; span < TAIL + sizeof TAIL / sizeof *TAIL; span++) {
        if (character >= span->minimum && character <= span->maximum) {
            return opened_by(character) == NULL;
        }
    }
    return false;
}

static bool skip_balanced(TSLexer *lexer, const Form *form);

static bool skip_quoted(TSLexer *lexer, int32_t quote, const Form *form) {
    advance(lexer);
    while (lexer->lookahead != quote) {
        if (lexer->eof(lexer)) {
            return false;
        }
        size_t length = quote == '`' ? walked(lexer, form->opening) : 0;
        if (length > 0 && form->opening[length] == 0) {
            if (!skip_balanced(lexer, form)) {
                return false;
            }
        } else if (length == 0) {
            if (lexer->lookahead == '\\') {
                advance(lexer);
            }
            advance(lexer);
        }
    }
    advance(lexer);
    return true;
}

static bool skip_enclosed(TSLexer *lexer, const Form *form) {
    int32_t opening = (unsigned char)form->opening[strlen(form->opening) - 1];
    for (unsigned depth = 1;;) {
        int32_t lead = lexer->lookahead;
        if (lexer->eof(lexer)) {
            return false;
        }
        if (lead == '\'' || lead == '"' || lead == '`') {
            if (!skip_quoted(lexer, lead, form)) {
                return false;
            }
        } else if (lead == '/') {
            if (!skip_comment(lexer)) {
                return false;
            }
        } else if (lead == form->closing && depth == 1) {
            return true;
        } else {
            depth = depth + (lead == opening) - (lead == form->closing);
            advance(lexer);
        }
    }
}

static bool skip_balanced(TSLexer *lexer, const Form *form) {
    if (!skip_enclosed(lexer, form)) {
        return false;
    }
    advance(lexer);
    return true;
}

static bool spells_keyword(const char *word, size_t length) {
    for (const char *const *held = WORDS; held < WORDS + sizeof WORDS / sizeof *WORDS; held++) {
        if (strlen(*held) == length && memcmp(*held, word, length) == 0) {
            return true;
        }
    }
    return false;
}

static void skip_glued(TSLexer *lexer) {
    for (bool joined = true; joined;) {
        lexer->mark_end(lexer);
        char word[HOLE_WORD_SIZE];
        size_t length = 0;
        bool spelled = true;
        while (!lexer->eof(lexer) && continues_name(lexer->lookahead)) {
            int32_t letter = lexer->lookahead >= 'A' && lexer->lookahead <= 'Z' ? lexer->lookahead + ('a' - 'A') : lexer->lookahead;
            spelled = spelled && letter < 0x80 && length < HOLE_WORD_SIZE - 1;
            if (spelled) {
                word[length] = (char)letter;
            }
            length++;
            advance(lexer);
        }
        if (length > 0 && spelled && spells_keyword(word, length)) {
            return;
        }
        lexer->mark_end(lexer);
        const Form *form = opened_by(lexer->lookahead);
        joined = form != NULL && form->opening[walked(lexer, form->opening)] == 0 && skip_balanced(lexer, form);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

bool COMPOSITE_SCAN(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    if (valid_symbols[HOLE_END] && closes_hole(lexer->lookahead)) {
        lexer->result_symbol = HOLE_END;
        advance(lexer);
        skip_glued(lexer);
        return true;
    }
    if (!valid_symbols[HOLE]) {
        return dialect_scan(payload, lexer, valid_symbols);
    }
    while (lexer->lookahead == ' ' || (lexer->lookahead >= '\t' && lexer->lookahead <= '\r')) {
        lexer->advance(lexer, true);
    }
    const Form *form = opened_by(lexer->lookahead);
    if (form == NULL) {
        return dialect_scan(payload, lexer, valid_symbols);
    }
    size_t length = walked(lexer, form->opening);
    if (form->opening[length] == 0) {
        lexer->result_symbol = HOLE;
        bool enclosed = skip_enclosed(lexer, form);
        lexer->mark_end(lexer);
        return enclosed;
    }
    Replay replay = {
        .lexer = {.result_symbol = lexer->result_symbol, .advance = replay_advance, .mark_end = replay_mark_end, .get_column = replay_get_column, .is_at_included_range_start = replay_is_at_included_range_start, .eof = replay_eof},
        .source = lexer,
        .replayed = form->opening,
        .remaining = length,
    };
    replay.lexer.lookahead = replay_lookahead(&replay);
    bool found = dialect_scan(payload, &replay.lexer, valid_symbols);
    lexer->result_symbol = replay.lexer.result_symbol;
    return found;
}
