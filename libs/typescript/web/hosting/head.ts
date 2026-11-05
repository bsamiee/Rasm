import { Buffer } from 'node:buffer';
import { basename, join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { paint } from '@rasm/mark';
import { generateManifestIconsEntry } from '@vite-pwa/assets-generator/api/generate-manifest-icons-entry';
import { instructions } from '@vite-pwa/assets-generator/api/instructions';
import { minimal2023Preset } from '@vite-pwa/assets-generator/presets/minimal-2023';
import { formatRgb } from 'culori';
import { Cause, Effect, FileSystem, Function, Match, Option, Record, Struct } from 'effect';
import { type ChildNode, list, parse } from 'postcss';
import type { HtmlTagDescriptor, Plugin, ResolvedConfig, Rolldown } from 'vite';
import type { WebAppManifest } from 'web-app-manifest';
import { HostingError } from '#/hosting-error';

// --- [TYPES] ---------------------------------------------------------------------------

type Scheme = 'light' | 'dark';

interface Head {
    readonly master: string;
    readonly stylesheet: string;
    readonly ink: string;
    readonly surface: string;
    readonly manifest?: Omit<WebAppManifest, 'icons'> & { readonly name: string };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CSS_CALL = /^(?<call>var|light-dark)\((?<parameters>.*)\)$/su;
const DOCUMENT_ROOT = /^(?::root|:host|html)$/u;
const WEB_MANIFEST = 'manifest.webmanifest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const headTag = (tag: string, attrs: NonNullable<HtmlTagDescriptor['attrs']>): HtmlTagDescriptor => ({ tag, attrs, injectTo: 'head' });
const cascade = (nodes: readonly ChildNode[], scheme: Scheme): readonly (readonly [string, string])[] =>
    nodes.flatMap(
        Match.type<ChildNode>().pipe(
            Match.discriminatorsExhaustive('type')({
                atrule: ({ name, params, nodes: held = [] }) => (name !== 'media' || params.replaceAll(/\s/gu, '') === `(prefers-color-scheme:${scheme})` ? cascade(held, scheme) : []),
                comment: () => [],
                decl: ({ prop, value }) => [[prop, value] as const],
                rule: ({ selectors, nodes: held }) => (selectors.every((selector) => DOCUMENT_ROOT.test(selector)) ? cascade(held, scheme) : []),
            }),
        ),
    );
const computed = (declared: ReadonlyMap<string, string>, scheme: Scheme, value: string): string => {
    const { call, parameters = '' } = CSS_CALL.exec(value.trim())?.groups ?? {};
    const [first = '', second = ''] = list.comma(parameters);
    const schemed = scheme === 'dark' ? second : first;
    const referenced = call === 'var' ? (declared.get(first) ?? second) : schemed;
    return call === undefined ? value : computed(declared, scheme, referenced);
};
const renderHead = Effect.fnUntraced(
    function* ({ root, base }: ResolvedConfig, { master, stylesheet, ink, surface, manifest }: Head) {
        const fs = yield* FileSystem.FileSystem;
        const { nodes } = parse(yield* fs.readFileString(join(root, stylesheet)), { from: stylesheet });
        const color = (scheme: Scheme, token: string) => Effect.fromNullishOr(formatRgb(computed(new Map(cascade(nodes, scheme)), scheme, `var(${token})`))).pipe(Effect.mapError(() => new Cause.NoSuchElementError(`${stylesheet} holds no ${scheme} color at ${token}`)));
        const [inked, chrome, darkChrome] = yield* Effect.all([color('light', ink), color('light', surface), color('dark', surface)]);
        const painted = yield* paint(join(root, master), { color: inked });
        const installed = <A extends { readonly sizes: unknown }>(asset: A) => (manifest === undefined ? { ...asset, sizes: [] } : asset);
        const resizeOptions = { background: chrome, fit: 'contain' as const };
        const generated = yield* Effect.tryPromise(() =>
            instructions({
                imageResolver: Function.constant(Buffer.from(painted)),
                imageName: basename(master),
                preset: { transparent: { ...installed(minimal2023Preset.transparent), padding: 0 }, maskable: { ...installed(minimal2023Preset.maskable), resizeOptions }, apple: { ...minimal2023Preset.apple, padding: 0, resizeOptions } },
                faviconPreset: '2023',
                htmlLinks: { xhtml: false, includeId: false },
                basePath: base,
                resolveSvgName: Function.identity,
            }),
        );
        const href = `${base}${WEB_MANIFEST}`;
        const linked = {
            ...generated.favicon,
            ...generated.apple,
            ...(manifest && {
                [href]: {
                    name: WEB_MANIFEST,
                    mimeType: 'application/manifest+json',
                    buffer: () => Promise.resolve(Buffer.from(JSON.stringify({ short_name: manifest.name, display: 'standalone', start_url: base, theme_color: chrome, background_color: chrome, ...manifest, ...generateManifestIconsEntry('object', generated) }))),
                    linkObject: { id: WEB_MANIFEST, rel: 'manifest', href },
                },
            }),
        };
        const links = Object.values(linked).flatMap(({ linkObject }) => (linkObject === undefined ? [] : [headTag('link', Struct.omit(linkObject, ['id']))]));
        const themed =
            chrome === darkChrome
                ? [headTag('meta', { name: 'theme-color', content: chrome }), headTag('meta', { name: 'color-scheme', content: 'only light' })]
                : [headTag('meta', { name: 'theme-color', content: chrome, media: '(prefers-color-scheme: light)' }), headTag('meta', { name: 'theme-color', content: darkChrome, media: '(prefers-color-scheme: dark)' }), headTag('meta', { name: 'color-scheme', content: 'light dark' })];
        return { assets: { ...linked, ...generated.transparent, ...generated.maskable }, tags: [...links, ...themed] };
    },
    (effect, _, { master }) => effect.pipe(Effect.mapError(HostingError.build('client', master)), Effect.provide(layer)),
);
const emittedFile = ({ name, buffer }: { readonly name: string; readonly buffer: () => Promise<Buffer> }) => Effect.map(Effect.promise(buffer), (source) => ({ type: 'asset' as const, fileName: name, source }));
const emittedFiles = <E>(rendered: Effect.Effect<Effect.Success<ReturnType<typeof renderHead>>, E>) => Effect.flatMap(rendered, ({ assets }) => Effect.forEach(Object.values(assets), emittedFile, { concurrency: 'unbounded' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const siteHead = (head: Head): Plugin => {
    let resolved: ResolvedConfig;
    const rendered = Effect.runSync(Effect.cached(Effect.suspend(() => renderHead(resolved, head))));
    return {
        name: '@rasm/hosting/head',
        applyToEnvironment: ({ config }) => config.consumer === 'client',
        configResolved: (config) => {
            resolved = config;
        },
        configureServer: ({ middlewares }) => {
            middlewares.use((request, response, next) => {
                Effect.gen(function* () {
                    const asset = Option.getOrUndefined(Record.get((yield* rendered).assets, request.url ?? ''));
                    return asset === undefined ? next() : response.setHeader('Content-Type', asset.mimeType).end(yield* Effect.promise(asset.buffer));
                })
                    .pipe(Effect.runPromise)
                    .catch(next);
            });
        },
        generateBundle(this: Rolldown.PluginContext) {
            return Effect.runPromise(emittedFiles(rendered)).then((emitted) => {
                emitted.forEach((file) => this.emitFile(file));
            });
        },
        transformIndexHtml: () => Effect.runPromise(rendered).then(Struct.get('tags')),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Head, siteHead };
