#include <string.h>
#include <wctype.h>
#define tree_sitter_bash_external_scanner_scan(...) bash_upstream_scan(__VA_ARGS__)
#define tree_sitter_bash_external_scanner_deserialize(...) bash_upstream_deserialize(__VA_ARGS__)
#define tree_sitter_bash_external_scanner_serialize(...) bash_upstream_serialize(__VA_ARGS__)
#define tree_sitter_bash_external_scanner_create(...) bash_upstream_create(__VA_ARGS__)
#define strcmp(left, right) (strcmp(left, right) != 0 || !(lexer->eof(lexer) || lexer->lookahead == '\n'))
#define iswalnum(character) (iswalnum(character) || (valid_symbols[EXPANSION_WORD] && (character) > 0 && (character) < 0x80 && strchr("!#$*-?@_", (int)(character)) != NULL))
#include "tree-sitter-bash/src/scanner.c"
#undef strcmp
#undef iswalnum

// --- [TYPES] ---------------------------------------------------------------------------

enum { TIME_POSIX = ERROR_RECOVERY + 1, TIME_END, CLOSED, EXTGLOB_WORD, BRACE_OPEN, BACKTICK_END, HEREDOC_DUE, QUOTED_BACKTICK, ESCAPED_QUOTE, STRING_SPACE, KEY_OPEN, TOKEN_COUNT };

// --- [CONSTANTS] -----------------------------------------------------------------------

static const char METACHARACTERS[] = " \t\n;&|()<>";
static const char EXPANDED_OPERATORS[] = ";&|)<>";
static const char SPECIAL_PARAMETERS[] = "*@?-";
static const char GLOB_BOUNDS[] = "\"'$`;&|)]";
static const char STRING_BOUNDS[] = "\"$`";
static const char *const CLOSERS[] = {"do", "done", "elif", "else", "fi", "then"};

// --- [MODELS] --------------------------------------------------------------------------

typedef struct {
    Scanner upstream;
    bool quoted;
} Context;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CHARACTERS]

static inline bool ascii(int32_t character) {
    return character > 0 && character < 0x80;
}

static inline bool among(const char *set, int32_t character) {
    return ascii(character) && strchr(set, character) != NULL;
}

static inline bool digit(int32_t character) {
    return ascii(character) && isdigit(character);
}

static inline bool named(int32_t character, bool leading) {
    return character == '_' || (ascii(character) && (isalpha(character) || (!leading && isdigit(character))));
}

static inline bool ends(TSLexer *lexer) {
    return lexer->eof(lexer) || iswspace(lexer->lookahead);
}

static inline bool bounds(TSLexer *lexer) {
    return lexer->eof(lexer) || among(METACHARACTERS, lexer->lookahead);
}

static inline bool blank(TSLexer *lexer) {
    return lexer->lookahead == ' ' || lexer->lookahead == '\t';
}

static inline bool spacing(TSLexer *lexer, const bool *valid_symbols, bool queued) {
    return blank(lexer) || (lexer->lookahead == '\n' && !queued && !valid_symbols[NEWLINE]);
}

static inline bool plain(const bool *valid_symbols) {
    return !valid_symbols[EXPANSION_WORD] && !valid_symbols[REGEX] && !valid_symbols[REGEX_NO_SLASH] && !valid_symbols[REGEX_NO_SPACE];
}

// --- [HEREDOCS]

static uint32_t waiting(const Scanner *scanner) {
    uint32_t index = scanner->heredocs.size;
    while (index > 0 && !scanner->heredocs.contents[index - 1].started) {
        index--;
    }
    while (index < scanner->heredocs.size && scanner->heredocs.contents[index].delimiter.size == 0) {
        index++;
    }
    return index;
}

static bool delimiter(Heredoc *heredoc, TSLexer *lexer) {
    lexer->result_symbol = HEREDOC_START;
    while (blank(lexer)) {
        skip(lexer);
    }
    int32_t quote = 0;
    bool found = false;
    while (!lexer->eof(lexer) && (quote != 0 || (lexer->lookahead != '`' && !among(METACHARACTERS, lexer->lookahead)))) {
        const int32_t character = lexer->lookahead;
        if (character >= 0x80) {
            return false;
        }
        advance(lexer);
        if (character == quote) {
            quote = 0;
        } else if (quote == 0 && (character == '\'' || character == '"')) {
            quote = character;
            heredoc->is_raw = true;
        } else if (character == '\\' && ascii(lexer->lookahead) && (quote == 0 || (quote == '"' && among("$`\"\\\n", lexer->lookahead)))) {
            heredoc->is_raw = true;
            array_push(&heredoc->delimiter, (char)lexer->lookahead);
            advance(lexer);
            found = true;
        } else {
            array_push(&heredoc->delimiter, (char)character);
            found = true;
        }
    }
    array_push(&heredoc->delimiter, '\0');
    if (!found) {
        reset_string(&heredoc->delimiter);
    }
    return found;
}

static bool due(Scanner *scanner, TSLexer *lexer, uint32_t index) {
    Heredoc *contents = scanner->heredocs.contents;
    const uint32_t last = scanner->heredocs.size - 1;
    const Heredoc due = contents[index];
    memmove(&contents[index], &contents[index + 1], (last - index) * sizeof(Heredoc));
    contents[last] = due;
    lexer->result_symbol = HEREDOC_DUE;
    lexer->mark_end(lexer);
    return true;
}

static void truncated(Scanner *scanner, uint32_t size) {
    while (scanner->heredocs.size > size) {
        Heredoc heredoc = array_pop(&scanner->heredocs);
        array_delete(&heredoc.delimiter);
        array_delete(&heredoc.current_leading_word);
    }
}

// --- [TOKENS]

static bool literal(TSLexer *lexer) {
    lexer->result_symbol = BARE_DOLLAR;
    advance(lexer);
    lexer->mark_end(lexer);
    return ends(lexer) || among("\")|&;<>", lexer->lookahead);
}

static bool key(TSLexer *lexer) {
    lexer->result_symbol = KEY_OPEN;
    advance(lexer);
    lexer->mark_end(lexer);
    int32_t quote = 0;
    for (uint32_t depth = 1; !lexer->eof(lexer); advance(lexer)) {
        const int32_t character = lexer->lookahead;
        if (character == '\\' && quote != '\'') {
            advance(lexer);
        } else if (quote != 0) {
            quote = character == quote ? 0 : quote;
        } else if (character == '\'' || character == '"') {
            quote = character;
        } else if (character == '[') {
            depth++;
        } else if (character == ']' && --depth == 0) {
            advance(lexer);
            if (lexer->lookahead == '+') {
                advance(lexer);
            }
            return lexer->lookahead == '=';
        } else if (among(METACHARACTERS, character)) {
            return false;
        }
    }
    return false;
}

static bool flag(TSLexer *lexer, const bool *valid_symbols) {
    advance(lexer);
    if (lexer->lookahead == 'p' && valid_symbols[TIME_POSIX]) {
        lexer->result_symbol = TIME_POSIX;
    } else if (lexer->lookahead == '-' && valid_symbols[TIME_END]) {
        lexer->result_symbol = TIME_END;
    } else {
        return false;
    }
    advance(lexer);
    lexer->mark_end(lexer);
    return bounds(lexer);
}

static bool closed(TSLexer *lexer, const bool *valid_symbols) {
    lexer->result_symbol = CLOSED;
    lexer->mark_end(lexer);
    if (lexer->lookahead == '}') {
        advance(lexer);
        return bounds(lexer);
    }
    char word[sizeof "then"] = {0};
    size_t length = 0;
    for (; named(lexer->lookahead, length == 0); length++) {
        if (length < sizeof word - 1) {
            word[length] = (char)lexer->lookahead;
        }
        advance(lexer);
    }
    if (bounds(lexer)) {
        for (size_t index = 0; length < sizeof word && index < sizeof CLOSERS / sizeof *CLOSERS; index++) {
            if (strcmp(word, CLOSERS[index]) == 0) {
                return true;
            }
        }
        return false;
    }
    if (!valid_symbols[VARIABLE_NAME] || length == 0 || !among("=+[", lexer->lookahead)) {
        return false;
    }
    lexer->result_symbol = VARIABLE_NAME;
    lexer->mark_end(lexer);
    if (lexer->lookahead == '+') {
        advance(lexer);
        return lexer->lookahead == '=';
    }
    return true;
}

static bool string_space(TSLexer *lexer) {
    while (iswspace(lexer->lookahead)) {
        advance(lexer);
    }
    lexer->mark_end(lexer);
    lexer->result_symbol = STRING_SPACE;
    return among(STRING_BOUNDS, lexer->lookahead);
}

static bool concat(TSLexer *lexer) {
    lexer->result_symbol = CONCAT;
    lexer->mark_end(lexer);
    if (lexer->lookahead != '\\') {
        return true;
    }
    advance(lexer);
    return !ends(lexer);
}

static bool range(TSLexer *lexer) {
    lexer->result_symbol = BRACE_START;
    while (digit(lexer->lookahead)) {
        advance(lexer);
    }
    for (int dot = 0; dot < 2; dot++) {
        if (lexer->lookahead != '.') {
            return false;
        }
        advance(lexer);
    }
    while (digit(lexer->lookahead)) {
        advance(lexer);
    }
    return lexer->lookahead == '}';
}

static bool descriptor(TSLexer *lexer, const bool *valid_symbols) {
    advance(lexer);
    lexer->mark_end(lexer);
    const bool word = valid_symbols[BRACE_OPEN] && !bounds(lexer);
    lexer->result_symbol = BRACE_OPEN;
    if (!named(lexer->lookahead, true)) {
        return word || (valid_symbols[BRACE_START] && range(lexer));
    }
    while (named(lexer->lookahead, false)) {
        advance(lexer);
    }
    if (lexer->lookahead != '}') {
        return word;
    }
    advance(lexer);
    if (lexer->lookahead != '<' && lexer->lookahead != '>') {
        return word;
    }
    lexer->result_symbol = FILE_DESCRIPTOR;
    lexer->mark_end(lexer);
    return true;
}

static bool group(TSLexer *lexer) {
    uint32_t depth = 0;
    do {
        const int32_t character = lexer->lookahead;
        if (lexer->eof(lexer) || character == '\n' || among("$`<>", character)) {
            return false;
        }
        advance(lexer);
        if (character == '\\') {
            advance(lexer);
        } else if (character == '\'' || character == '"') {
            while (!lexer->eof(lexer) && lexer->lookahead != character) {
                if (character == '"' && among("$`", lexer->lookahead)) {
                    return false;
                }
                advance(lexer);
            }
            advance(lexer);
        } else if (character == '(') {
            depth++;
        } else if (character == ')') {
            depth--;
        }
    } while (depth > 0);
    return true;
}

static bool extglob(TSLexer *lexer) {
    lexer->result_symbol = EXTGLOB_WORD;
    bool found = false;
    while (!bounds(lexer)) {
        const int32_t character = lexer->lookahead;
        if (among("$`'\"{}[]", character) || !ascii(character)) {
            break;
        }
        advance(lexer);
        if (character == '\\') {
            advance(lexer);
        } else if (among("?*+@!", character) && lexer->lookahead == '(') {
            if (!group(lexer)) {
                return false;
            }
            found = true;
        }
    }
    lexer->mark_end(lexer);
    return found;
}

static void classed(TSLexer *lexer) {
    advance(lexer);
    if (lexer->lookahead == '^' || lexer->lookahead == '!') {
        advance(lexer);
    }
    if (lexer->lookahead == ']') {
        advance(lexer);
    }
    while (!lexer->eof(lexer) && lexer->lookahead != ']' && lexer->lookahead != '\n') {
        const bool opening = lexer->lookahead == '[';
        advance(lexer);
        if (opening && lexer->lookahead == ':') {
            while (!lexer->eof(lexer) && lexer->lookahead != ']') {
                advance(lexer);
            }
            advance(lexer);
        }
    }
    advance(lexer);
}

static bool grouped(TSLexer *lexer) {
    lexer->result_symbol = EXTGLOB_PATTERN;
    uint32_t depth = 0;
    while (!lexer->eof(lexer) && !iswspace(lexer->lookahead) && !(depth == 0 && among(GLOB_BOUNDS, lexer->lookahead))) {
        const int32_t character = lexer->lookahead;
        if (character == '[') {
            classed(lexer);
            continue;
        }
        depth += (character == '(') - (character == ')');
        advance(lexer);
        if (character == '\\' && !lexer->eof(lexer)) {
            advance(lexer);
        }
    }
    lexer->mark_end(lexer);
    return depth == 0;
}

static bool pattern(TSLexer *lexer) {
    lexer->result_symbol = REGEX_NO_SLASH;
    if (lexer->lookahead == '"') {
        return false;
    }
    lexer->mark_end(lexer);
    bool advanced = false;
    bool quoted = false;
    for (uint32_t braces = 0; !lexer->eof(lexer);) {
        const int32_t character = lexer->lookahead;
        if (character == '/') {
            lexer->mark_end(lexer);
            return advanced;
        }
        if (character == '}' && braces == 0 && !quoted) {
            return true;
        }
        braces += (character == '{' && !quoted) - (character == '}' && !quoted);
        quoted = quoted != (character == '\'');
        advance(lexer);
        advanced = true;
        if (character == '\\' && !lexer->eof(lexer) && lexer->lookahead != '[' && lexer->lookahead != '/') {
            advance(lexer);
        }
        if (character == '\\' || quoted || !iswspace(character)) {
            lexer->mark_end(lexer);
        }
    }
    return false;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

void *(tree_sitter_bash_external_scanner_create)(void) {
    Context *context = calloc(1, sizeof *context);
    array_init(&context->upstream.heredocs);
    return context;
}

unsigned(tree_sitter_bash_external_scanner_serialize)(void *payload, char *buffer) {
    const unsigned size = bash_upstream_serialize(payload, buffer);
    if (size == 0) {
        return 0;
    }
    buffer[size] = (char)((Context *)payload)->quoted;
    return size + 1;
}

void(tree_sitter_bash_external_scanner_deserialize)(void *payload, const char *buffer, unsigned length) {
    Scanner *scanner = payload;
    bash_upstream_deserialize(payload, buffer, length);
    ((Context *)payload)->quoted = length > 0 && buffer[length - 1];
    if (length > 0) {
        truncated(scanner, (unsigned char)buffer[3]);
    }
}

bool(tree_sitter_bash_external_scanner_scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    Scanner *scanner = payload;
    Context *context = payload;
    if (!in_error_recovery(valid_symbols) && valid_symbols[HEREDOC_END] && lexer->eof(lexer) && scanner->heredocs.size > 0) {
        truncated(scanner, scanner->heredocs.size - 1);
        lexer->result_symbol = HEREDOC_END;
        lexer->mark_end(lexer);
        return true;
    }
    if (in_error_recovery(valid_symbols)) {
        return bash_upstream_scan(payload, lexer, valid_symbols);
    }
    if (valid_symbols[STRING_SPACE] && iswspace(lexer->lookahead)) {
        return string_space(lexer);
    }
    if (valid_symbols[HEREDOC_BODY_BEGINNING] || valid_symbols[SIMPLE_HEREDOC_BODY] || valid_symbols[HEREDOC_CONTENT] || valid_symbols[HEREDOC_END]) {
        const bool scanned = bash_upstream_scan(payload, lexer, valid_symbols);
        if (scanned && lexer->eof(lexer) && scanner->heredocs.size > 0 && array_back(&scanner->heredocs)->delimiter.size == 0) {
            lexer->mark_end(lexer);
        }
        return scanned;
    }
    if (valid_symbols[HEREDOC_START] && scanner->heredocs.size > 0) {
        return delimiter(array_back(&scanner->heredocs), lexer);
    }
    const uint32_t queue = waiting(scanner);
    const bool queued = queue < scanner->heredocs.size;
    const bool word = valid_symbols[EXTGLOB_WORD] && plain(valid_symbols) && !valid_symbols[VARIABLE_NAME] && !valid_symbols[EXTGLOB_PATTERN];
    const bool closes = valid_symbols[CLOSED] && !valid_symbols[CONCAT];
    const bool head = valid_symbols[FILE_DESCRIPTOR] && !valid_symbols[EXPANSION_WORD] && !valid_symbols[CLOSING_BRACE];
    const bool timed = valid_symbols[TIME_POSIX] || valid_symbols[TIME_END];
    const bool keyed = valid_symbols[KEY_OPEN];
    const bool dollar = valid_symbols[BARE_DOLLAR] && plain(valid_symbols);
    bool separated[TOKEN_COUNT];
    memcpy(separated, valid_symbols, sizeof separated);
    if (spacing(lexer, valid_symbols, queued) && !valid_symbols[EMPTY_VALUE] && (queued || head || closes || timed || keyed || ((word || dollar) && !valid_symbols[CLOSING_BRACE]))) {
        separated[CONCAT] = false;
        while (spacing(lexer, valid_symbols, queued)) {
            skip(lexer);
        }
    }
    separated[VARIABLE_NAME] = valid_symbols[VARIABLE_NAME] && (valid_symbols[EXPANSION_WORD] || !among(SPECIAL_PARAMETERS, lexer->lookahead));
    if (valid_symbols[QUOTED_BACKTICK] && lexer->lookahead == '`') {
        context->quoted = !context->quoted;
        advance(lexer);
        lexer->mark_end(lexer);
        lexer->result_symbol = QUOTED_BACKTICK;
        return true;
    }
    if (valid_symbols[ESCAPED_QUOTE] && context->quoted && lexer->lookahead == '\\') {
        advance(lexer);
        lexer->result_symbol = ESCAPED_QUOTE;
        if (lexer->lookahead != '"') {
            return false;
        }
        advance(lexer);
        lexer->mark_end(lexer);
        return true;
    }
    if (valid_symbols[EXTGLOB_PATTERN] && !valid_symbols[ESAC] && !valid_symbols[CONCAT] && !queued) {
        while (iswspace(lexer->lookahead)) {
            skip(lexer);
        }
        if (lexer->lookahead == '(') {
            return grouped(lexer);
        }
    }
    if (queued && lexer->lookahead == '\n') {
        return due(scanner, lexer, queue);
    }
    if (keyed && lexer->lookahead == '[' && !separated[CONCAT]) {
        return key(lexer);
    }
    if (queued && valid_symbols[BACKTICK_END] && lexer->lookahead == '`') {
        truncated(scanner, queue);
        lexer->result_symbol = BACKTICK_END;
        lexer->mark_end(lexer);
        return true;
    }
    if (timed && lexer->lookahead == '-') {
        return flag(lexer, valid_symbols);
    }
    if (closes && among("}deft", lexer->lookahead)) {
        return closed(lexer, valid_symbols);
    }
    if (head && lexer->lookahead == '{' && !separated[CONCAT]) {
        return descriptor(lexer, valid_symbols);
    }
    if (head && lexer->lookahead == '0' && !separated[CONCAT]) {
        advance(lexer);
        lexer->result_symbol = FILE_DESCRIPTOR;
        lexer->mark_end(lexer);
        return lexer->lookahead == '<' || lexer->lookahead == '>';
    }
    if (!separated[CONCAT] && lexer->lookahead == '$' && dollar) {
        return literal(lexer);
    }
    if (!separated[CONCAT] && word && (named(lexer->lookahead, true) || among("./~?*+@!%^,:=", lexer->lookahead))) {
        return extglob(lexer);
    }
    if (valid_symbols[EXPANSION_WORD] && lexer->lookahead == '(') {
        advance(lexer);
        if (lexer->lookahead == '$') {
            lexer->result_symbol = EXPANSION_WORD;
            lexer->mark_end(lexer);
            return true;
        }
    }
    if (valid_symbols[REGEX_NO_SLASH] && lexer->lookahead != '$') {
        return pattern(lexer);
    }
    return separated[CONCAT] && (lexer->lookahead == '\\' || (valid_symbols[CLOSING_BRACE] && among(EXPANDED_OPERATORS, lexer->lookahead))) ? concat(lexer) : bash_upstream_scan(payload, lexer, separated);
}
