import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { createProjectFileMapUsingProjectGraph, type ExecutorContext } from '@nx/devkit';
import { type GetZeroTrustOrganizationResult, getZeroTrustOrganization, getZoneOutput, R2Bucket, R2CustomDomain, ZeroTrustAccessIdentityProvider, ZeroTrustOrganization } from '@pulumi/cloudflare';
import { local } from '@pulumi/command';
import { ActionsEnvironmentSecret, ActionsEnvironmentVariable, ActionsRepositoryPermissions, ActionsSecret, IssueLabels, Repository, RepositoryDependabotSecurityUpdates, RepositoryDeployKey, RepositoryEnvironment, RepositoryRuleset, RepositoryVulnerabilityAlerts, WorkflowRepositoryPermissions } from '@pulumi/github';
import { interpolate, jsonStringify, type Output, secret } from '@pulumi/pulumi';
import { Acl, DnsPreferences, FederatedIdentity, TailnetSettings } from '@pulumi/tailscale';
import { PrivateKey } from '@pulumi/tls';
import { Environment, Project, Provider, ServiceToken, secretssync } from '@pulumiverse/doppler';
import { Array, Cause, Config, Effect, FileSystem, Path, Record, Redacted, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { Yaml } from 'effect/encoding';
import { HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { interpolateReleaseConfig } from 'nx/src/command-line/release/utils/interpolate-release-config';
import { run } from '#/command';
import { Deployed } from '#/platform-stack';

// --- [CONSTANTS] -----------------------------------------------------------------------

const palette = { bug: 'd73a4a', enhancement: 'a2eeef' };

// --- [MODELS] --------------------------------------------------------------------------

const Account = Deployed.mapFields(Struct.pick(['DEPLOYED_HOSTS', 'CLOUDFLARE_ACCOUNT_ID']));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.NonEmptyString, description: Schema.NonEmptyString }));
const Origin = Schema.fromJsonString(Schema.Struct({ name: Schema.NonEmptyString, owner: Schema.Struct({ login: Schema.NonEmptyString }) }));
const Login = Schema.fromJsonString(Schema.Struct({ token: Schema.RedactedFromValue(Schema.NonEmptyString), 'api-host': Schema.URLFromString }));
const Integrations = Schema.Struct({ integrations: Schema.Array(Schema.Struct({ slug: Schema.String, name: Schema.String, type: Schema.String })) });
const Form = Schema.String.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => Yaml.parse(text), catch: (cause) => new SchemaIssue.InvalidValue({ expected: 'YAML', cause }, text, options) })),
        encode: SchemaGetter.forbiddenEncoding,
    }),
    Schema.decodeTo(Schema.Struct({ description: Schema.NonEmptyString, labels: Schema.NonEmptyArray(Schema.Literals(Struct.keys(palette))) })),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const delivered = Effect.fnUntraced(function* (repository: Output<string>, { name, owner, project, description, login: { token, 'api-host': host } }: { readonly name: string; readonly owner: string; readonly project: string; readonly description: string; readonly login: typeof Login.Type }) {
    const { integrations } = yield* HttpClient.execute(HttpClientRequest.bearerToken(HttpClientRequest.get(new URL('/v3/integrations', host)), token)).pipe(Effect.flatMap(HttpClientResponse.filterStatusOk), Effect.flatMap(HttpClientResponse.schemaBodyJson(Integrations)));
    const { slug: integration } = yield* Effect.fromOption(
        Array.findFirst(integrations, (row) => row.type === 'github-actions' && row.name === owner),
        () => new Cause.NoSuchElementError(`Doppler holds no github-actions integration named ${owner}`),
    );
    const provider = new Provider('doppler', { dopplerToken: secret(Redacted.value(token)), host: host.origin });
    const doppler = new Project(project, { name: project, description }, { provider, protect: true });
    const held = Record.map(Struct.Record(['dev', 'ci', 'release', 'deploy'], undefined), (_, config) => new Environment(`${project}-${config}`, { project: doppler.name, slug: config, name: config }, { provider, protect: true }).slug);
    const synced = { integration, project: doppler.name, syncTarget: 'repo', repoName: repository, deleteBehavior: 'delete_from_target' };
    const value = new ServiceToken(`${project}-deploy`, { project: doppler.name, config: held.deploy, name, access: 'read' }, { provider }).key;
    const deploying = Record.map(
        Struct.Record(['release', 'deploy'], undefined),
        (_, environment) => new ActionsEnvironmentSecret(`${name}-${environment}-doppler`, { repository, environment: new RepositoryEnvironment(`${name}-${environment}`, { repository, environment }).environment, secretName: 'DOPPLER_TOKEN', value }).environment,
    );
    return {
        dev: held.dev,
        ci: new secretssync.GithubActions(`${project}-ci`, { ...synced, config: held.ci }, { provider }).config,
        release: new secretssync.GithubActions(`${project}-release`, { ...synced, config: held.release, environmentName: deploying.release }, { provider }).config,
        deploy: deploying.deploy,
    };
}, Effect.provide(layerUndici));

const tailnet = (hosts: typeof Account.Type.DEPLOYED_HOSTS, { name, repository, fullName, environment }: { readonly name: string; readonly repository: Output<string>; readonly fullName: Output<string>; readonly environment: Output<string> }) => {
    const runner = 'tag:ci' satisfies (typeof hosts)[string]['tags'][number];
    const owned = Array.dedupe([...Object.values(hosts).flatMap(({ tags }) => tags), runner]);
    const served = Array.dedupe(
        Object.values(hosts)
            .filter(({ tailnet: { ssh } }) => ssh)
            .flatMap(({ tags }) => tags),
    );
    const identity = new FederatedIdentity(`${name}-deploy`, { issuer: 'https://token.actions.githubusercontent.com', subject: interpolate`repo:${fullName}:environment:${environment}`, scopes: ['all'], tags: [runner] });
    const policy = {
        tagOwners: Record.fromIterableWith(owned, (tag) => [tag, ['autogroup:admin']]),
        grants: [{ src: ['*'], dst: ['*'], ip: ['*'] }],
        ssh: Array.match(served, { onEmpty: Array.empty, onNonEmpty: (dst) => [{ action: 'accept', src: ['autogroup:member', runner], dst, users: ['autogroup:nonroot', 'root'], acceptEnv: ['*'] }] }),
    };
    return {
        policy: new Acl('tailnet', { acl: interpolate`${jsonStringify(policy, undefined, '\t')}\n`, overwriteExistingContent: true }).id,
        settings: new TailnetSettings('tailnet', { devicesAutoUpdatesOn: true }).id,
        magicDns: new DnsPreferences('tailnet', { magicDns: true }).magicDns,
        identity: Record.map({ TAILSCALE_OAUTH_CLIENT_ID: identity.id, TAILSCALE_AUDIENCE: identity.audience, TAILSCALE_TAGS: runner }, (value, variableName) => new ActionsEnvironmentVariable(`${name}-deploy-${variableName}`, { repository, environment, variableName, value }).variableName),
    };
};

const access = (accountId: string, { name, authDomain }: Pick<GetZeroTrustOrganizationResult, 'name' | 'authDomain'>) => ({
    domain: new ZeroTrustOrganization(name, { accountId, name, authDomain, strictServiceTokenAuth: true }, { protect: true }).authDomain,
    identityProvider: new ZeroTrustAccessIdentityProvider(`${name}-onetimepin`, { accountId, name: 'One-time PIN', type: 'onetimepin', config: {} }, { protect: true }).id,
});

const feed = (accountId: string, hosts: typeof Account.Type.DEPLOYED_HOSTS, { name, repository, environment }: { readonly name: string; readonly repository: Output<string>; readonly environment: Output<string> }) => {
    const bucket = new R2Bucket(`${name}-updates`, { accountId, name: `${name.toLowerCase()}-updates` }, { protect: true });
    return {
        feedBucket: new ActionsEnvironmentVariable(`${name}-release-SPARKLE_FEED_BUCKET`, { repository, environment, variableName: 'SPARKLE_FEED_BUCKET', value: bucket.name }).variableName,
        feedDomains: Array.dedupe(Object.values(hosts).map(({ domain }) => domain.name)).map((zone) => new R2CustomDomain(`updates.${zone}`, { accountId, bucketName: bucket.name, domain: `updates.${zone}`, enabled: true, zoneId: getZoneOutput({ filter: { name: zone } }).zoneId }).domain),
    };
};

const assets = (repository: Output<string>, name: string) => {
    const held = new Repository(`${name}-Assets`, { name: `${name}-Assets`, description: `Binary assets ${name} holds as a private submodule`, visibility: 'private', allowMergeCommit: false, deleteBranchOnMerge: true }, { protect: true });
    const key = new PrivateKey(`${name}-assets-deploy`, { algorithm: 'ED25519' });
    return {
        assets: held.fullName,
        assetsDeployKey: new RepositoryDeployKey(`${name}-assets-deploy`, { repository: held.name, title: `${name} CI`, key: key.publicKeyOpenssh, readOnly: true }).title,
        assetsSecret: new ActionsSecret(`${name}-assets-deploy-key`, { repository, secretName: 'ASSETS_DEPLOY_KEY', value: key.privateKeyOpenssh }).secretName,
    };
};

const guarded = (repository: Output<string>, { name, fullName, forms, patterns }: { readonly name: string; readonly fullName: Output<string>; readonly forms: readonly (typeof Form.Type)[]; readonly patterns: readonly string[] }) => ({
    securityUpdates: new RepositoryDependabotSecurityUpdates(`${name}-security-updates`, { repository: new RepositoryVulnerabilityAlerts(`${name}-vulnerability-alerts`, { repository }).repository, enabled: false }).enabled,
    allowedActions: new ActionsRepositoryPermissions(`${name}-actions-permissions`, { repository, allowedActions: 'all' }).allowedActions,
    workflowPermissions: new WorkflowRepositoryPermissions(`${name}-workflow-permissions`, { repository, defaultWorkflowPermissions: 'read' }).defaultWorkflowPermissions,
    codeScanning: new local.Command(`${name}-code-scanning`, { interpreter: ['gh', 'api', '--method', 'PATCH', '--raw-field', 'state=configured'], create: interpolate`repos/${fullName}/code-scanning/default-setup` }).stdout,
    labels: new IssueLabels(`${name}-labels`, { repository, labels: forms.flatMap(({ description, labels }) => labels.map((label) => ({ name: label, color: palette[label], description }))) }).labels,
    releaseTags: new RepositoryRuleset(`${name}-release-tags`, {
        repository,
        name: 'release tags',
        target: 'tag',
        enforcement: 'active',
        conditions: { refName: { includes: patterns.map((pattern) => `refs/tags/${interpolateReleaseConfig(pattern, { projectName: '**', version: '**', releaseGroupName: '**' })}`), excludes: [] } },
        rules: { update: true, deletion: true, nonFastForward: true },
    }).rulesetId,
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const repositoryStack = Effect.fnUntraced(function* (context: ExecutorContext, deployed: Readonly<Record<string, string>>) {
    const [fs, path, { DEPLOYED_HOSTS: hosts, CLOUDFLARE_ACCOUNT_ID: accountId }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeUnknownEffect(Account)(deployed)]);
    const { name: stackName, description } = yield* Effect.flatMap(fs.readFileString(path.join(context.root, 'package.json')), Schema.decodeEffect(Manifest));
    const files = yield* Effect.promise(() => createProjectFileMapUsingProjectGraph(context.projectGraph));
    const { groups } = yield* Effect.flatMap(
        Effect.promise(() => createNxReleaseConfig(context.projectGraph, files, context.nxJsonConfiguration.release)),
        ({ nxReleaseConfig }) => Effect.fromNullishOr(nxReleaseConfig),
    );
    return {
        stackName,
        program: Effect.gen(function* () {
            const [{ name, owner }, project, login, forms, organization] = yield* Effect.all([
                Effect.flatMap(run('gh', ['repo', 'view', '--json', 'name,owner']), Schema.decodeEffect(Origin)),
                Config.String('DOPPLER_PROJECT'),
                Effect.flatMap(run('doppler', ['configure', 'get', 'token', 'api-host', '--json']), Schema.decodeEffect(Login)),
                Effect.flatMap(
                    fs.glob('.github/ISSUE_TEMPLATE/*.yml', { root: context.root, exclude: ['**/config.yml'] }),
                    Effect.forEach((form) => Effect.flatMap(fs.readFileString(path.join(context.root, form)), Schema.decodeEffect(Form))),
                ),
                Effect.tryPromise(() => getZeroTrustOrganization({ accountId })),
            ]);
            const { name: repository, fullName } = new Repository(
                name,
                {
                    name,
                    description,
                    visibility: 'public',
                    hasIssues: true,
                    allowMergeCommit: false,
                    allowUpdateBranch: true,
                    deleteBranchOnMerge: true,
                    squashMergeCommitTitle: 'PR_TITLE',
                    squashMergeCommitMessage: 'PR_BODY',
                    securityAndAnalysis: { secretScanning: { status: 'enabled' }, secretScanningPushProtection: { status: 'enabled' } },
                },
                { protect: true },
            );
            const configs = yield* delivered(repository, { name, owner: owner.login, project, description, login });
            return {
                repository: fullName,
                configs,
                tailnet: tailnet(hosts, { name, repository, fullName, environment: configs.deploy }),
                access: access(accountId, organization),
                ...feed(accountId, hosts, { name, repository, environment: configs.release }),
                ...assets(repository, name),
                ...guarded(repository, { name, fullName, forms, patterns: Array.dedupe(Record.collect(groups ?? {}, (_, { releaseTag }) => releaseTag.pattern)) }),
            };
        }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { repositoryStack };
