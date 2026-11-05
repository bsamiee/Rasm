import { bindings, type ConfigContext, type WorkerConfig } from 'cf/config';

// --- [CONSTANTS] -----------------------------------------------------------------------

const accessBinding = 'ACCESS';
const compatibilityDate = '2026-10-01';
const hostingMode = { edge: 'cloudflare', disposable: 'disposable', server: 'production' } as const;
const ownerBinding = 'OWNER_EMAIL';

// --- [OPERATIONS] ----------------------------------------------------------------------

const disposableName = (name: string) => `${name}-disposable`;
const hostedWorker =
    <const E extends NonNullable<WorkerConfig['env']>>({ name, assets, env, ...worker }: Omit<WorkerConfig, 'compatibilityDate' | 'env'> & { readonly env: E }) =>
    ({ mode }: ConfigContext) => {
        const disposable = mode === hostingMode.disposable;
        const edge = disposable || mode === hostingMode.edge;
        return {
            compatibilityDate,
            observability: { enabled: true, traces: { enabled: true } },
            ...worker,
            name: disposable ? disposableName(name) : name,
            assets: { runWorkerFirst: ['/'], ...assets },
            ...(disposable && { domains: [], triggers: (worker.triggers ?? []).filter(({ type }) => type !== 'fetch') }),
            env: {
                ASSETS: bindings.assets(),
                ...env,
                [ownerBinding]: edge || mode === hostingMode.server ? bindings.secret() : bindings.text('seedy@sites.test'),
                ...(edge && { [accessBinding]: bindings.secret() }),
            },
        };
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { accessBinding, compatibilityDate, disposableName, hostedWorker, hostingMode, ownerBinding };
