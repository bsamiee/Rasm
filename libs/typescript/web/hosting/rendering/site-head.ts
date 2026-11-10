import { Buffer } from 'node:buffer';
import { basename, join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { paint } from '@rasm/mark/paint';
import { generateManifestIconsEntry } from '@vite-pwa/assets-generator/api/generate-manifest-icons-entry';
import { instructions } from '@vite-pwa/assets-generator/api/instructions';
import { minimal2023Preset } from '@vite-pwa/assets-generator/presets/minimal-2023';
import { formatRgb } from 'culori';
import { Array, Cause, Duration, Effect, FileSystem, Function, Match, Option, Record, Result, Schema, Struct, UndefinedOr } from 'effect';
import { type ChildNode, list, parse } from 'postcss';
import { type DevEnvironment, type HtmlTagDescriptor, normalizePath, type Plugin, type ResolvedConfig, type Rolldown } from 'vite';
import type { WebAppManifest } from 'web-app-manifest';
import { HostingError } from '#/app/error';

// --- [TYPES] ---------------------------------------------------------------------------

type Scheme = 'light' | 'dark';

interface Head {
    readonly master: string;
    readonly stylesheet: string;
    readonly fill: string;
    readonly background: string;
    readonly manifest?: Omit<WebAppManifest, 'icons'> & { readonly name: string };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CSS_CALL = /^(?<call>var|light-dark)\((?<parameters>.*)\)$/su;
const DOCUMENT_ROOT = /^(?::root|:host|html)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const WebManifest = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const headTag = (tag: string, attrs: NonNullable<HtmlTagDescriptor['attrs']>): HtmlTagDescriptor => ({ tag, attrs, injectTo: 'head' });
const cascade = (nodes: readonly ChildNode[], scheme: Scheme): readonly (readonly [string, string])[] =>
    nodes
        .filter((node) => (node.type !== 'atrule' || node.name !== 'media' || node.params.replaceAll(/\s/gu, '') === `(prefers-color-scheme:${scheme})`) && (node.type !== 'rule' || node.selectors.every((selector) => DOCUMENT_ROOT.test(selector))))
        .flatMap(
            Match.type<ChildNode>().pipe(
                Match.discriminatorsExhaustive('type')({
                    atrule: ({ nodes: held = [] }) => cascade(held, scheme),
                    comment: () => [],
                    decl: ({ prop, value }) => [[prop, value] as const],
                    rule: ({ nodes: held }) => cascade(held, scheme),
                }),
            ),
        );
const computed = (declared: ReadonlyMap<string, string>, scheme: Scheme, value: string): Option.Option<string> => {
    const token = value.trim();
    const { call, parameters } = CSS_CALL.exec(token)?.groups ?? {};
    const [first, second] = parameters === undefined ? [] : list.comma(parameters);
    const schemed = scheme === 'dark' ? second : first;
    const referenced = call === 'var' ? (UndefinedOr.map(first, (name) => declared.get(name)) ?? second) : schemed;
    return call === undefined ? Option.some(token) : Option.flatMap(Option.fromUndefinedOr(referenced), (next) => computed(declared, scheme, next));
};
const renderHead = Effect.fnUntraced(
    function* ({ root, base }: ResolvedConfig, { master, stylesheet, fill, background, manifest }: Head) {
        const fs = yield* FileSystem.FileSystem;
        const { nodes } = parse(yield* fs.readFileString(join(root, stylesheet)), { from: stylesheet });
        const color = (scheme: Scheme, token: string) => Effect.fromOption(Option.flatMapNullishOr(computed(new Map(cascade(nodes, scheme)), scheme, `var(${token})`), formatRgb), () => new Cause.NoSuchElementError(`${stylesheet} holds no ${scheme} color at ${token}`));
        const [iconColor, chrome, darkChrome] = yield* Effect.all([color('light', fill), color('light', background), color('dark', background)]);
        const painted = yield* paint(join(root, master), { color: iconColor });
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
        const webManifest = 'manifest.webmanifest';
        const href = `${base}${webManifest}`;
        const icons = yield* Effect.all(
            Record.map(
                { ...generated.favicon, ...generated.apple, ...generated.transparent, ...generated.maskable },
                Effect.fnUntraced(function* ({ name, mimeType, buffer }) {
                    return { name, mimeType, source: yield* Effect.tryPromise(buffer) };
                }),
            ),
            { concurrency: 'unbounded' },
        );
        const links = Array.filterMap(Object.values({ ...generated.favicon, ...generated.apple }), ({ linkObject }) => (linkObject === undefined ? Result.failVoid : Result.succeed(headTag('link', Struct.omit(linkObject, ['id'])))));
        const themed =
            chrome === darkChrome
                ? [headTag('meta', { name: 'theme-color', content: chrome }), headTag('meta', { name: 'color-scheme', content: 'only light' })]
                : [headTag('meta', { name: 'theme-color', content: chrome, media: '(prefers-color-scheme: light)' }), headTag('meta', { name: 'theme-color', content: darkChrome, media: '(prefers-color-scheme: dark)' }), headTag('meta', { name: 'color-scheme', content: 'light dark' })];
        const listed =
            manifest === undefined
                ? { assets: {}, tags: [] }
                : {
                      assets: {
                          [href]: {
                              name: webManifest,
                              mimeType: 'application/manifest+json',
                              source: Buffer.from(yield* Effect.orDie(Schema.encodeEffect(WebManifest)({ short_name: manifest.name, display: 'standalone', start_url: base, theme_color: chrome, background_color: chrome, ...manifest, ...generateManifestIconsEntry('object', generated) }))),
                          },
                      },
                      tags: [headTag('link', { rel: 'manifest', href })],
                  };
        return { assets: { ...icons, ...listed.assets }, tags: [...links, ...listed.tags, ...themed] };
    },
    (effect, _, { master }) => effect.pipe(Effect.mapError(HostingError.of('Build', `client environment of ${master}`)), Effect.provide(layer)),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const siteHead = (head: Head): Plugin => {
    let resolved: ResolvedConfig;
    const files = () => [head.master, head.stylesheet].map((file) => normalizePath(join(resolved.root, file)));
    const [rendered, invalidate] = Effect.runSync(
        Effect.cachedInvalidateWithTTL(
            Effect.suspend(() => renderHead(resolved, head)),
            Duration.infinity,
        ),
    );
    return {
        name: '@rasm/hosting:head',
        applyToEnvironment: ({ config }) => config.consumer === 'client',
        configResolved: (config) => {
            resolved = config;
        },
        configureServer: ({ middlewares, watcher }) => {
            watcher.add(files());
            middlewares.use((request, response, next) => {
                Effect.runPromise(rendered)
                    .then(({ assets }) =>
                        Option.match(
                            Option.flatMap(Option.fromNullishOr(request.url), (url) => Record.get(assets, url)),
                            { onNone: () => next(), onSome: ({ mimeType, source }) => response.setHeader('Content-Type', mimeType).end(source) },
                        ),
                    )
                    .catch(next);
            });
        },
        hotUpdate(this: { readonly environment: DevEnvironment }, { file }) {
            if (!files().includes(file)) {
                return;
            }
            Effect.runSync(invalidate);
            this.environment.hot.send({ type: 'full-reload', path: '*', triggeredBy: file });
            return [];
        },
        generateBundle(this: Rolldown.PluginContext) {
            return Effect.runPromise(rendered).then(({ assets }) => {
                Object.values(assets).forEach(({ name, source }) => this.emitFile({ type: 'asset', fileName: name, source }));
            });
        },
        transformIndexHtml: () => Effect.runPromise(rendered).then(Struct.get('tags')),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Head, siteHead };
