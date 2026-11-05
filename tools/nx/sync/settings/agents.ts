import { parse, patch } from '@decimalturn/toml-patch';
import type { Tree } from '@nx/devkit';
import { Duration, Effect, Option, Predicate, Record, Schema, Struct } from 'effect';
import { NetAddress } from 'effect/net';
import { Identity, Mise, Package } from '#/eng/typescript/manifest';
import { TextFormat } from '#/eng/typescript/text-format';
import { decodeTree, formatted, patched, syncGenerator, treeText } from '#/tools/nx/sync/tree';

// --- [TYPES] ---------------------------------------------------------------------------

type Configured = typeof Mise.schema.Type;
interface Server {
    readonly url?: string;
    readonly command?: readonly [string, ...string[]];
    readonly headers?: Readonly<Record<string, string>>;
    readonly helper?: { readonly secret: string; readonly header: string; readonly value: string } | string;
    readonly startup?: Extract<Duration.Input, string>;
    readonly timeout?: Extract<Duration.Input, string>;
    readonly approval?: 'auto' | 'prompt' | 'writes' | 'approve';
    readonly disabled?: true;
    readonly denied?: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGINS = 'plugins';
const CLAUDE = { servers: '.mcp.json', settings: '.claude/settings.json', catalog: `${PLUGINS}/.claude-plugin/marketplace.json`, name: /[^\w-]/gu } as const;
const CODEX = {
    config: '.codex/config.toml',
    agents: '.codex/agents',
    catalog: `${PLUGINS}/.agents/plugins/marketplace.json`,
    name: /\W/gu,
    entry: { policy: { installation: 'AVAILABLE', authentication: 'ON_INSTALL' }, category: 'Coding' },
    deny: `jq -c '{hookSpecificOutput: {hookEventName: .hook_event_name, permissionDecision: "deny", permissionDecisionReason: "\\(.tool_name) is denied"}}'`,
} as const;
const FRONTMATTER = /^---\n(?<head>.*?)\n---\n(?<body>.*)$/su;
const HOST = '10 minutes';
const BEARER = { header: 'Authorization', value: 'Bearer \\(.)' } as const;
const SERVERS: Readonly<Record<string, Server>> = {
    atelier: { url: 'http://127.0.0.1:7490/' },
    github: {
        url: 'https://api.githubcopilot.com/mcp/',
        helper: { secret: 'GH_PROJECTS_TOKEN', ...BEARER },
        headers: {
            'X-MCP-Toolsets': 'context,repos,issues,pull_requests,actions,git,dependabot,github_support_docs_search',
            'X-MCP-Exclude-Tools': 'delete_repository,delete_file,create_repository,fork_repository,push_files,create_or_update_file',
            'X-MCP-Features': 'file_blame,issue_dependencies',
        },
    },
    exa: { url: 'https://mcp.exa.ai/mcp?tools=web_fetch_exa,web_search_advanced_exa,agent_run', helper: { secret: 'EXA_API_KEY', header: 'x-api-key', value: '\\(.)' } },
    context7: { url: 'https://mcp.context7.com/mcp', helper: { secret: 'CONTEXT7_API_KEY', ...BEARER } },
    deepwiki: { url: 'https://mcp.deepwiki.com/mcp' },
    greptile: { url: 'https://api.greptile.com/mcp', helper: { secret: 'GREPTILE_API_KEY', ...BEARER }, disabled: true },
    openaiDeveloperDocs: { url: 'https://developers.openai.com/mcp' },
    claudeCodeDocs: { url: 'https://code.claude.com/docs/mcp' },
    illustrator: { url: 'http://localhost:18412/v1/mcp', helper: 'eng/scripts/illustrator-token.sh', approval: 'auto' },
    arena: { url: 'https://mcp.are.na/mcp', disabled: true },
    xcode: { command: ['xcrun', 'mcpbridge'] },
    lldb: { command: ['xcrun', 'lldb-mcp'], startup: '20 seconds', timeout: '2 minutes' },
};
const DAEMONS: Readonly<Record<string, Omit<Server, 'url' | 'command'> & { readonly name?: string; readonly path?: string; readonly plugin?: string }>> = {
    'rhino-mcp-platform': { timeout: HOST, denied: ['run_python', 'g1_*'] },
    'blender-lab': { name: 'blender', timeout: HOST, denied: ['*_for_cli'] },
    'mcp-for-blender': { timeout: HOST },
    argus: { plugin: 'argus' },
    'playwright-test': { plugin: 'playwright', path: '/mcp' },
};
const DESCRIPTIONS: Readonly<Record<string, string>> = { rasm: 'Claude Code and Codex plugins of Rasm monorepo', playwright: 'Browser automation and test authoring for agents' };

// --- [MODELS] --------------------------------------------------------------------------

const Rest = [Schema.Record(Schema.String, Schema.Json)] as const;
const Strings = Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const Settings = Schema.fromJsonString(Schema.Struct({ disabledMcpjsonServers: Strings, permissions: Schema.StructWithRest(Schema.Struct({ deny: Strings }), Rest) }));
const Hooks = Schema.Array(Schema.StructWithRest(Schema.Struct({ hooks: Schema.Array(Schema.StructWithRest(Schema.Struct({ command: Schema.optionalKey(Schema.String) }), Rest)) }), Rest));
const CodexConfig = TextFormat('TOML', parse).pipe(
    Schema.decodeTo(
        Schema.StructWithRest(
            Schema.Struct({
                mcp_servers: Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Json)),
                hooks: Schema.StructWithRest(Schema.Struct({ PreToolUse: Hooks.pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }), Rest).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ PreToolUse: [] }))),
            }),
            Rest,
        ),
    ),
);
const AgentFile = Schema.Struct({ head: TextFormat.Yaml.pipe(Schema.decodeTo(Schema.Struct({ name: Schema.String, description: Schema.String, skills: Strings }))), body: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const named = (pattern: RegExp) => (name: string) => name.replace(pattern, '_');
const written = (tree: Tree, file: string, text: string) => Effect.map(formatted(tree, file, text), (next) => ({ file, next }));
const json = (tree: Tree, file: string, value: object) => written(tree, file, JSON.stringify(value));
const helped =
    ({ DOPPLER_PROJECT, DOPPLER_CONFIG }: Configured['env']) =>
    (helper: NonNullable<Server['helper']>) =>
        typeof helper === 'string' ? helper : `doppler secrets get ${helper.secret} --plain --project ${DOPPLER_PROJECT} --config ${DOPPLER_CONFIG} | jq -R '{"${helper.header}": "${helper.value}"}'`;
const served = ([daemon, row]: readonly [string, Configured['daemons'][string]]) => {
    const { name = daemon, path = '/', plugin, ...server } = DAEMONS[daemon] ?? {};
    return typeof row === 'string' ? [] : Option.toArray(Option.map(row.port, (port) => ({ name, plugin, server: { ...server, url: `http://${NetAddress.formatInet(NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, port))}${path}` } })));
};
const denials = ([server, { denied = [] }]: readonly [string, Server]) => denied.map((glob) => ({ claude: `mcp__${named(CLAUDE.name)(server)}__${glob}`, codex: `mcp__${named(CODEX.name)(server)}__${glob.split('*').map(named(CODEX.name)).join('.*')}` }));
const denying = ({ hooks }: (typeof CodexConfig.Type)['hooks']['PreToolUse'][number]) => hooks.some((handler) => handler.command === CODEX.deny);
const claudeRow =
    (helper: ReturnType<typeof helped>) =>
    ({ url, command, headers, helper: held, timeout }: Server) => ({
        ...(command === undefined ? { type: 'http', url, timeout: timeout && Duration.toMillis(timeout) } : { command: command[0], args: command.slice(1) }),
        headers,
        headersHelper: held && helper(held),
    });
const codexRow =
    (helper: ReturnType<typeof helped>) =>
    ({ url, command, headers, helper: held, startup, timeout, approval = 'approve', disabled }: Server) => ({
        default_tools_approval_mode: disabled === true ? undefined : approval,
        url,
        command: command?.[0],
        args: command?.slice(1),
        http_headers_helper: held && helper(held),
        http_headers: headers,
        startup_timeout_sec: startup && Duration.toSeconds(startup),
        tool_timeout_sec: timeout && Duration.toSeconds(timeout),
        enabled: disabled && false,
    });
const plugin = Effect.fnUntraced(function* (tree: Tree, local: ReturnType<typeof served>, name: string) {
    const root = `${PLUGINS}/${name}`;
    const manifest = tree.exists(`${root}/${Package.file}`) ? Option.some(yield* decodeTree(tree, Package.schema, `${root}/${Package.file}`)) : Option.none();
    const owned = local.filter((row) => row.plugin === name);
    return {
        name,
        root,
        description: yield* Option.match(manifest, { onNone: () => Effect.fromNullishOr(DESCRIPTIONS[name]), onSome: (held) => Effect.succeed(held.description) }),
        version: Option.getOrUndefined(Option.flatMap(manifest, Struct.get('version'))),
        servers: Record.fromIterableWith(owned, (row) => [row.name, { type: 'streamable-http', url: row.server.url }]),
        prefixes: owned.map((row) => [`mcp__plugin_${named(CLAUDE.name)(name)}_${named(CLAUDE.name)(row.name)}__`, `mcp__${named(CODEX.name)(row.name)}__`] as const),
        agents: tree.exists(`${root}/agents`) ? tree.children(`${root}/agents`).map((file) => `${root}/agents/${file}`) : [],
        mcpServers: owned.length > 0 && !tree.exists(`${root}/.lsp.json`) ? './mcp.json' : undefined,
    };
});
const agent = Effect.fnUntraced(function* (tree: Tree, prefixes: readonly (readonly [string, string])[], file: string) {
    const {
        head: { name, description, skills },
        body,
    } = yield* Schema.decodeUnknownEffect(AgentFile)(FRONTMATTER.exec(yield* treeText(tree, file))?.groups);
    const codex = named(CODEX.name)(name);
    const target = `${CODEX.agents}/${codex}.toml`;
    const instructions = prefixes.reduce((text, [claude, renamed]) => text.replaceAll(claude, renamed), [...skills.map((skill) => `Read and follow \`${skill}\` before starting.`), body.trim()].join('\n\n'));
    return yield* written(tree, target, patch(tree.read(target, 'utf-8') ?? '', { name: codex, description, developer_instructions: `${instructions}\n` }));
});
const rows = Effect.fnUntraced(function* (tree: Tree) {
    const [{ name: marketplace, author, repository, license }, { env, daemons }, settings, config] = yield* Effect.all([decodeTree(tree, Identity.schema, Identity.file), decodeTree(tree, Mise.schema, Mise.file), decodeTree(tree, Settings, CLAUDE.settings), decodeTree(tree, CodexConfig, CODEX.config)]);
    const helper = helped(env);
    const local = Record.toEntries(daemons).flatMap(served);
    const servers: Readonly<Record<string, Server>> = {
        ...Record.fromIterableWith(
            local.filter((row) => row.plugin === undefined),
            (row) => [row.name, row.server],
        ),
        ...SERVERS,
    };
    const denied = Record.toEntries(servers).flatMap(denials);
    const plugins = yield* Effect.forEach(
        tree.children(PLUGINS).filter((entry) => !(entry.startsWith('.') || tree.isFile(`${PLUGINS}/${entry}`))),
        (name) => plugin(tree, local, name),
    );
    const prefixes = plugins.flatMap(Struct.get('prefixes'));
    const rendered = yield* Effect.forEach(plugins.flatMap(Struct.get('agents')), (file) => agent(tree, prefixes, file));
    const kept = new Set(rendered.map(Struct.get('file')));
    const owner = { name: author };
    const schemas = 'https://agent-plugins.org/schemas/1.0.0';
    return [
        yield* json(tree, CLAUDE.servers, { mcpServers: Record.map(servers, claudeRow(helper)) }),
        yield* patched(tree, CLAUDE.settings, settings, {
            disabledMcpjsonServers: Record.keys(Record.filter(servers, (server) => server.disabled === true)),
            permissions: { ...settings.permissions, deny: [...denied.map(Struct.get('claude')), ...settings.permissions.deny.filter((rule) => !rule.startsWith('mcp__'))] },
        }),
        yield* written(
            tree,
            CODEX.config,
            patch(yield* treeText(tree, CODEX.config), {
                ...config,
                mcp_servers: { ...Record.map(servers, codexRow(helper)), ...Record.filter(config.mcp_servers, (row) => !('url' in row || 'command' in row)) },
                hooks: {
                    ...config.hooks,
                    PreToolUse: [...config.hooks.PreToolUse.filter(Predicate.not(denying)), ...(denied.length > 0 ? [{ matcher: `^(?:${denied.map(Struct.get('codex')).join('|')})$`, hooks: [{ type: 'command', command: CODEX.deny }] }] : [])],
                },
            }),
        ),
        yield* json(tree, CLAUDE.catalog, {
            name: marketplace,
            description: yield* Effect.fromNullishOr(DESCRIPTIONS[marketplace]),
            owner,
            plugins: plugins.map((row) => ({ name: row.name, source: `./${row.name}`, description: row.description, version: row.version, author: owner, mcpServers: row.mcpServers })),
        }),
        yield* json(tree, CODEX.catalog, { name: marketplace, plugins: plugins.map((row) => ({ name: row.name, source: { source: 'local', path: `./${row.name}` }, ...CODEX.entry })) }),
        ...(yield* Effect.forEach(plugins, (row) => json(tree, `${row.root}/plugin.json`, { $schema: `${schemas}/plugin.schema.json`, name: row.name, version: row.version, description: row.description, author: owner, repository, license }))),
        ...(yield* Effect.forEach(
            plugins.filter((row) => !Record.isEmptyRecord(row.servers)),
            (row) => json(tree, `${row.root}/mcp.json`, { $schema: `${schemas}/mcp.schema.json`, mcpServers: row.servers }),
        )),
        ...rendered,
        ...tree
            .children(CODEX.agents)
            .map((file) => `${CODEX.agents}/${file}`)
            .filter((file) => !kept.has(file))
            .map((file) => ({ file })),
    ];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const agents = syncGenerator('Agent configuration differs from mise.toml daemons, plugins and their agents', rows);

// --- [EXPORTS] -------------------------------------------------------------------------

export { agents };
