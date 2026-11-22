import { type ByteSize, Data, type Duration, type Record, type Schema, String } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Binding = Data.TaggedEnum<{
    sqlite: object;
    postgres: object;
    bucket: {
        readonly abort: Duration.Duration;
        readonly expire: Record.ReadonlyRecord<keyof typeof App.modes, Record.ReadonlyRecord<string, Duration.Duration>>;
        readonly infrequentAccess: Record.ReadonlyRecord<string, Duration.Duration>;
        readonly cors: readonly { readonly origins: readonly string[]; readonly methods: readonly ('GET' | 'PUT' | 'POST' | 'DELETE' | 'HEAD')[]; readonly headers: readonly string[]; readonly exposed: readonly string[]; readonly age: Duration.Duration }[];
    };
    secret: object;
    text: { readonly value: string };
    json: { readonly value: Schema.MutableJson };
}>;
type Target = Data.TaggedEnum<{ Cloudflare: object; Machine: { readonly host: string } }>;

// --- [MODELS] --------------------------------------------------------------------------

class App<const E extends Record.ReadonlyRecord<string, Binding> = Record.ReadonlyRecord<string, Binding>> extends Data.Class<{
    readonly name: string;
    readonly env: E;
    readonly allowed: readonly string[];
    readonly targets: readonly Target[];
    readonly limits: { readonly cpus: number; readonly memory: ByteSize.ByteSize };
}> {
    static readonly Binding = Data.taggedEnum<Binding>();
    static readonly Target = Data.taggedEnum<Target>();
    static readonly modes = { production: { suffix: '', kept: true }, disposable: { suffix: '-disposable', kept: false } } as const;
    static readonly bundle = { environment: 'node', entry: 'index', assets: 'assets', fetcher: 'ASSETS', mode: 'MODE', portVariable: 'PORT', readiness: '/readyz', signing: { endpoint: 'AWS_ENDPOINT_URL_S3', region: 'AWS_REGION', key: 'AWS_ACCESS_KEY_ID', secret: 'AWS_SECRET_ACCESS_KEY' } } as const;
    static readonly resource = (app: App, mode: keyof typeof App.modes, ...variables: readonly string[]) => `${[app.name, ...variables.map((variable) => String.snakeToKebab(variable.toLowerCase()))].join('-')}${App.modes[mode].suffix}`;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { App, type Binding, type Target };
