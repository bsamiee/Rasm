#include <stdarg.h>
#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>

#include "tree_sitter/alloc.h"
#include "tree_sitter/parser.h"

#include "components.h"

// --- [CONSTANTS] -----------------------------------------------------------------------

#define EXTERNAL_SCANNER(composite, fn) tree_sitter_##composite##_external_scanner_##fn
#define EXPANDED_SCANNER(composite, fn) EXTERNAL_SCANNER(composite, fn)
#define ABI(fn) EXPANDED_SCANNER(COMPOSITE, fn)

// --- [MODELS] --------------------------------------------------------------------------

typedef struct {
    void *(*create)(void);
    void (*destroy)(void *);
    unsigned (*serialize)(void *, char *);
    void (*deserialize)(void *, const char *, unsigned);
    bool (*scan)(void *, TSLexer *, const bool *);
    uint16_t offset;
    uint16_t count;
} Component;

typedef struct {
    const Component *stateful;
    void *payload;
} Composite;

typedef struct {
    TSLexer lexer;
    TSLexer *source;
    bool committed;
} Probe;

// --- [SERVICES] ------------------------------------------------------------------------

#define COMPONENT(name, offset, count)                                                     \
    void *name##_component_create(void);                                                   \
    void name##_component_destroy(void *);                                                 \
    unsigned name##_component_serialize(void *, char *);                                   \
    void name##_component_deserialize(void *, const char *, unsigned);                     \
    bool name##_component_scan(void *, TSLexer *, const bool *);
COMPONENTS
#undef COMPONENT

#define COMPONENT(name, offset, count)                                                     \
    {name##_component_create, name##_component_destroy, name##_component_serialize,        \
     name##_component_deserialize, name##_component_scan, offset, count},
static const Component components[] = {COMPONENTS};
#undef COMPONENT

static const size_t component_count = sizeof components / sizeof *components;

static TSLexer *source_of(const TSLexer *lexer) { return ((const Probe *)lexer)->source; }

static void probe_advance(TSLexer *lexer, bool skip) {
    Probe *probe = (Probe *)lexer;
    probe->source->advance(probe->source, skip);
    lexer->lookahead = probe->source->lookahead;
    probe->committed = probe->committed || !skip;
}

static void probe_mark_end(TSLexer *lexer) {
    Probe *probe = (Probe *)lexer;
    probe->source->mark_end(probe->source);
    probe->committed = true;
}

static uint32_t probe_get_column(TSLexer *lexer) { return source_of(lexer)->get_column(source_of(lexer)); }

static bool probe_is_at_included_range_start(const TSLexer *lexer) { return source_of(lexer)->is_at_included_range_start(source_of(lexer)); }

static bool probe_eof(const TSLexer *lexer) { return source_of(lexer)->eof(source_of(lexer)); }

static void probe_log(const TSLexer *lexer, const char *format, ...) {
    char message[TREE_SITTER_SERIALIZATION_BUFFER_SIZE];
    va_list arguments;
    va_start(arguments, format);
    vsnprintf(message, sizeof message, format, arguments);
    va_end(arguments);
    source_of(lexer)->log(source_of(lexer), "%s", message);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

static bool holds(const Component *component, const bool *valid) {
    for (uint16_t index = component->offset; index < component->offset + component->count; index++) {
        if (valid[index] && !SHARED[index]) {
            return true;
        }
    }
    return false;
}

static void *payload_of(const Composite *composite, const Component *component) {
    return composite->stateful == component ? composite->payload : NULL;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

void *ABI(create)(void) {
    Composite *composite = ts_calloc(1, sizeof *composite);
    for (size_t index = 0; index < component_count; index++) {
        void *payload = components[index].create();
        if (payload != NULL) {
            composite->stateful = &components[index];
            composite->payload = payload;
        }
    }
    return composite;
}

void ABI(destroy)(void *payload) {
    Composite *composite = payload;
    for (size_t index = 0; index < component_count; index++) {
        components[index].destroy(payload_of(composite, &components[index]));
    }
    ts_free(composite);
}

unsigned ABI(serialize)(void *payload, char *buffer) {
    Composite *composite = payload;
    return composite->stateful == NULL ? 0 : composite->stateful->serialize(composite->payload, buffer);
}

void ABI(deserialize)(void *payload, const char *buffer, unsigned length) {
    Composite *composite = payload;
    if (composite->stateful != NULL) {
        composite->stateful->deserialize(composite->payload, buffer, length);
    }
}

bool ABI(scan)(void *payload, TSLexer *lexer, const bool *valid_symbols) {
    Composite *composite = payload;
    Probe probe = {
        .lexer = {.lookahead = lexer->lookahead, .advance = probe_advance, .mark_end = probe_mark_end, .get_column = probe_get_column, .is_at_included_range_start = probe_is_at_included_range_start, .eof = probe_eof, .log = probe_log},
        .source = lexer,
    };
    for (size_t index = 0; index < component_count && !probe.committed; index++) {
        const Component *component = &components[index];
        if (holds(component, valid_symbols) && component->scan(payload_of(composite, component), &probe.lexer, valid_symbols + component->offset)) {
            lexer->result_symbol = probe.lexer.result_symbol + component->offset;
            return true;
        }
    }
    return false;
}
