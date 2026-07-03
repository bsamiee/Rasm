import { Command, type CommandExecutor, FileSystem } from '@effect/platform';
import type { PlatformError } from '@effect/platform/Error';
import { Data, Effect, Option, pipe, Record, Schema } from 'effect';

// --- [TYPES] -----------------------------------------------------------------------------

declare namespace Hermetic {
    type Route = keyof typeof _PAGES;
}

declare namespace K6 {
    type Gate = Schema.Schema.Type<typeof _Gate>;
    type Verdict = Data.TaggedEnum<{
        Passed: { readonly summary: Summary };
        Breached: { readonly summary: Summary };
    }>;
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const _ORIGIN = 'https://rasm.test';

// k6's documented exit contract: 0 = clean, 99 = thresholds breached; every other code is a crash.
const _EXIT = { breach: 99, pass: 0 } as const;

// Hermetic page corpus: deterministic documents fulfilled by route interception — a secure-context
// origin with zero servers, zero network, and zero litter. A new probe surface is a row.
const _PAGES = {
    '/clock': {
        title: 'clock',
        body: `<h1>clock</h1><output data-testid="now"></output>
<script>
const now = document.querySelector('[data-testid=now]');
const tick = () => { now.textContent = new Date().toISOString(); };
tick();
setInterval(tick, 1000);
</script>`,
    },
    '/echo': {
        title: 'echo',
        body: `<output data-testid="wire">idle</output>
<script>
const wire = document.querySelector('[data-testid=wire]');
const lane = new WebSocket('wss://rasm.test/ws');
lane.onopen = () => lane.send('ping');
lane.onmessage = (event) => { wire.textContent = String(event.data); };
lane.onerror = () => { wire.textContent = 'fault'; };
lane.onclose = () => { if (wire.textContent === 'idle') wire.textContent = 'closed'; };
</script>`,
    },
    '/form': {
        title: 'intake',
        body: `<main><h1>intake</h1><form><label for="key">key</label><input id="key" type="text"><button type="submit">submit</button></form></main>`,
    },
    '/panel': {
        title: 'panel',
        body: `<svg width="240" height="160" role="img" aria-label="panel"><rect x="8" y="8" width="104" height="64" fill="#1f6f8b"/><rect x="128" y="8" width="104" height="64" fill="#99a8b2"/><rect x="8" y="88" width="104" height="64" fill="#e8c547"/><rect x="128" y="88" width="104" height="64" fill="#30323d"/></svg>`,
    },
    '/passkey': {
        title: 'passkey',
        body: `<output data-testid="verdict">idle</output><button data-testid="mint">mint</button>
<script>
document.querySelector('[data-testid=mint]').addEventListener('click', async () => {
    const verdict = document.querySelector('[data-testid=verdict]');
    try {
        const credential = await navigator.credentials.create({ publicKey: {
            challenge: new Uint8Array(32),
            rp: { id: 'rasm.test', name: 'rasm' },
            user: { id: new Uint8Array(16), name: 'probe', displayName: 'probe' },
            pubKeyCredParams: [{ type: 'public-key', alg: -7 }],
            authenticatorSelection: { authenticatorAttachment: 'platform', userVerification: 'required' },
            timeout: 1000,
        } });
        verdict.textContent = credential ? 'minted' : 'empty';
    } catch (fault) {
        verdict.textContent = 'refused:' + fault.name;
    }
});
</script>`,
    },
    '/store': {
        title: 'store',
        body: `<output data-testid="held"></output>
<script>
localStorage.setItem('slot', localStorage.getItem('slot') ?? crypto.randomUUID());
document.querySelector('[data-testid=held]').textContent = localStorage.getItem('slot');
</script>`,
    },
} as const;

// --- [MODELS] ----------------------------------------------------------------------------

// Both threshold-gate spellings the summary export has shipped; the verdict authority stays the exit code.
const _Gate = Schema.Union(Schema.Boolean, Schema.Struct({ ok: Schema.Boolean }));

const _Metric = Schema.Struct({
    thresholds: Schema.optionalWith(Schema.Record({ key: Schema.String, value: _Gate }), { default: () => ({}) }),
});

class Summary extends Schema.Class<Summary>('K6Summary')({
    metrics: Schema.Record({ key: Schema.String, value: _Metric }),
}) {
    get gated(): ReadonlyArray<string> {
        return Record.keys(Record.filter(this.metrics, (metric) => Record.size(metric.thresholds) > 0));
    }
}

// --- [ERRORS] ----------------------------------------------------------------------------

class K6Fault extends Data.TaggedError('K6Fault')<{
    readonly reason: 'crashed' | 'summary';
    readonly detail: string;
}> {}

// --- [OPERATIONS] ------------------------------------------------------------------------

const _shell = (page: { readonly title: string; readonly body: string }): string =>
    `<!doctype html><html><head><meta charset="utf-8"><title>${page.title}</title></head><body>${page.body}</body></html>`;

const _summary = Schema.decode(Schema.parseJson(Summary), { errors: 'all' });

const Hermetic = {
    origin: _ORIGIN,
    routes: Record.keys(_PAGES),
    page: (path: string): Option.Option<string> =>
        pipe(
            Option.liftPredicate(path, (candidate): candidate is Hermetic.Route => candidate in _PAGES),
            Option.map((route) => _shell(_PAGES[route])),
        ),
} as const;

const K6 = {
    Summary,
    Verdict: Data.taggedEnum<K6.Verdict>(),
    // Activation probe for the load lane: the binary is a machine fact, never a JS dependency.
    locate: Command.exitCode(Command.make('k6', 'version')).pipe(
        Effect.map((code) => code === 0),
        Effect.orElseSucceed(() => false),
    ),
    // `env` feeds the script's `__ENV` — the one sanctioned parameterization channel; a hardcoded target host in a script is
    // the rejected form. `binary` defaults to the PATH k6 and exists so the subprocess contract stays falsifiable hermetically.
    run: (lane: {
        readonly script: string;
        readonly summary: string;
        readonly env?: Record<string, string>;
        readonly binary?: string;
    }): Effect.Effect<K6.Verdict, K6Fault, CommandExecutor.CommandExecutor | FileSystem.FileSystem> =>
        Effect.gen(function* () {
            const code = yield* Effect.mapError(
                Command.exitCode(
                    pipe(Command.make(lane.binary ?? 'k6', 'run', '--quiet', `--summary-export=${lane.summary}`, lane.script), (spawn) =>
                        lane.env === undefined ? spawn : Command.env(spawn, lane.env),
                    ),
                ),
                (fault: PlatformError) => new K6Fault({ reason: 'crashed', detail: fault.message }),
            );
            // The exit code is the verdict authority and discriminates FIRST: a crashed run reports its crash,
            // never a masking summary-read fault over the file the crash prevented.
            yield* code === _EXIT.pass || code === _EXIT.breach ? Effect.void : new K6Fault({ reason: 'crashed', detail: `exit ${code}` });
            const fs = yield* FileSystem.FileSystem;
            const raw = yield* Effect.mapError(fs.readFileString(lane.summary), (fault) => new K6Fault({ reason: 'summary', detail: fault.message }));
            const summary = yield* Effect.mapError(_summary(raw), (fault) => new K6Fault({ reason: 'summary', detail: fault.message }));
            return code === _EXIT.pass ? K6.Verdict.Passed({ summary }) : K6.Verdict.Breached({ summary });
        }),
} as const;

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Hermetic, K6, K6Fault };
