import { getZeroTrustOrganization, getZoneOutput, R2Bucket, R2CustomDomain, ZeroTrustAccessIdentityProvider, ZeroTrustOrganization } from '@pulumi/cloudflare';
import { ActionsEnvironmentSecret, ActionsEnvironmentVariable, IssueLabels, Repository, RepositoryEnvironment, RepositoryVulnerabilityAlerts, WorkflowRepositoryPermissions } from '@pulumi/github';
import { interpolate, jsonStringify, secret } from '@pulumi/pulumi';
import { Acl, DeviceKey, DnsPreferences, getDeviceOutput, OauthClient, TailnetSettings } from '@pulumi/tailscale';
import { Environment, Project, ServiceToken, secretssync, Webhook } from '@pulumiverse/doppler';
import { Array, Cause, Effect, FileSystem, Path, Record, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { Yaml } from 'effect/encoding';
import { hosts } from '#/platform-stack';

// --- [CONSTANTS] -----------------------------------------------------------------------

const palette = { bug: 'd73a4a', enhancement: 'a2eeef' };
const feed = { setting: 'SPARKLE_FEED_URL', variable: 'SPARKLE_FEED_BUCKET' } as const;
const configs = ['dev', 'ci', 'release', 'deploy'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.NonEmptyString, description: Schema.NonEmptyString, repository: Schema.Struct({ url: Schema.TemplateLiteralParser(['git+https://github.com/', Schema.NonEmptyString, '/', Schema.NonEmptyString, '.git']) }) }));
const Uproject = Schema.fromJsonString(Schema.Struct({ EngineAssociation: Schema.NonEmptyString }));
const Feed = Schema.TemplateLiteralParser([`${feed.setting} = `, Schema.URLFromString]);
const Form = Schema.String.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => Yaml.parse(text), catch: (cause) => new SchemaIssue.InvalidValue({ expected: 'YAML', cause }, text, options) })),
        encode: SchemaGetter.forbiddenEncoding,
    }),
    Schema.decodeTo(Schema.Struct({ description: Schema.NonEmptyString, labels: Schema.NonEmptyArray(Schema.Literals(Struct.keys(palette))) })),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const identified = Effect.fnUntraced(function* (root: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const [manifest, settings, engines] = yield* Effect.all([
        Effect.flatMap(fs.readFileString(path.join(root, 'package.json')), Schema.decodeEffect(Manifest)),
        fs.readFileString(path.join(root, 'Xcode.xcconfig')),
        Effect.flatMap(
            fs.glob('apps/unreal/**/*.uproject', { root }),
            Effect.forEach((project) => Effect.flatMap(fs.readFileString(path.join(root, project)), Schema.decodeEffect(Uproject))),
        ),
    ]);
    const [, { hostname }] = yield* Effect.flatMap(Effect.fromNullishOr(settings.split('\n').find((line) => line.startsWith(feed.setting))), (line) => Schema.decodeUnknownEffect(Feed)(line.replace('/$()/', '//')));
    const [, owner, , repository] = manifest.repository.url;
    return { project: manifest.name, description: manifest.description, owner, repository, feed: { domain: hostname, zone: hostname.split('.').slice(1).join('.') }, engines: Array.dedupe(engines.map(Struct.get('EngineAssociation'))).toSorted() };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const repositoryStack = Effect.fnUntraced(function* (root: string, { repository: name, project, description, feed: updates }: Effect.Success<ReturnType<typeof identified>>, { CLOUDFLARE_ACCOUNT_ID: accountId, GITHUB_TOKEN: token }: { readonly CLOUDFLARE_ACCOUNT_ID: string; readonly GITHUB_TOKEN: string }) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const [forms, organization] = yield* Effect.all([
        Effect.flatMap(
            fs.glob('.github/ISSUE_TEMPLATE/*.yml', { root, exclude: ['**/config.yml'] }),
            Effect.forEach((form) => Effect.flatMap(fs.readFileString(path.join(root, form)), Schema.decodeEffect(Form))),
        ),
        Effect.tryPromise({ try: () => getZeroTrustOrganization({ accountId }), catch: (cause) => new Cause.UnknownError(cause, `Cloudflare account ${accountId} holds no Zero Trust organization`) }),
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
            securityAndAnalysis: { secretScanning: { status: 'disabled' }, secretScanningPushProtection: { status: 'disabled' } },
        },
        { protect: true },
    );
    const doppler = new Project(project, { name: project, description }, { protect: true });
    const held = Record.map(Struct.Record(configs, undefined), (_, config) => new Environment(`${project}-${config}`, { project: doppler.name, slug: config, name: config }, { protect: true }).slug);
    const environments = Record.map(Struct.pick(held, ['release', 'deploy']), (_, environment) => new RepositoryEnvironment(`${name}-${environment}`, { repository, environment }).environment);
    const synced = { integration: '51df65fc-7097-4d4d-a4ca-308ce2a5918a', project: doppler.name, syncTarget: 'repo', repoName: repository, deleteBehavior: 'delete_from_target' };
    const bucket = new R2Bucket(`${name}-updates`, { accountId, name: `${name.toLowerCase()}-updates` }, { protect: true });
    const tags = Array.dedupe(Object.values(hosts).flatMap((row) => row.tags));
    const deployer = 'tag:deploy';
    const policy = { tagOwners: Record.fromIterableWith([...tags, deployer], (tag) => [tag, ['autogroup:admin']]), grants: [{ src: ['*'], dst: ['*'], ip: ['*'] }], ssh: [{ action: 'accept', src: ['autogroup:member', deployer], dst: tags, users: ['autogroup:nonroot', 'root'] }] };
    const tailnet = new Acl('tailnet', { acl: interpolate`${jsonStringify(policy, undefined, '\t')}\n`, overwriteExistingContent: true });
    const joining = new OauthClient(`${name}-deploy`, { scopes: ['auth_keys'], tags: [deployer] }, { dependsOn: tailnet });
    const machines = Record.keys(hosts).map((host) => host.split('.', 1).join(''));
    return {
        repository: fullName,
        configs: {
            ci: new secretssync.GithubActions(`${project}-ci`, { ...synced, config: held.ci }).config,
            release: new secretssync.GithubActions(`${project}-release`, { ...synced, config: held.release, environmentName: environments.release }).config,
            deploy: new ActionsEnvironmentSecret(`${name}-deploy-doppler`, { repository, environment: environments.deploy, secretName: 'DOPPLER_TOKEN', value: new ServiceToken(`${project}-deploy`, { project: doppler.name, config: held.deploy, name, access: 'read' }).key }).environment,
            dispatch: new Webhook(`${project}-deploy`, { project: doppler.name, url: interpolate`https://api.github.com/repos/${fullName}/dispatches`, authentication: { type: 'Bearer', token: secret(token) }, payload: '{"event_type":"deploy"}', enabledConfigs: [held.deploy] }).slug,
        },
        feed: {
            bucket: new ActionsEnvironmentVariable(`${name}-release-${feed.variable}`, { repository, environment: environments.release, variableName: feed.variable, value: bucket.name }).value,
            domain: new R2CustomDomain(updates.domain, { accountId, bucketName: bucket.name, domain: updates.domain, enabled: true, zoneId: getZoneOutput({ filter: { name: updates.zone } }).zoneId }).domain,
        },
        alerts: new RepositoryVulnerabilityAlerts(`${name}-vulnerability-alerts`, { repository, enabled: false }).enabled,
        workflowPermissions: new WorkflowRepositoryPermissions(`${name}-workflow-permissions`, { repository, defaultWorkflowPermissions: 'write', canApprovePullRequestReviews: true }).defaultWorkflowPermissions,
        labels: new IssueLabels(`${name}-labels`, { repository, labels: forms.flatMap(({ description: stated, labels }) => labels.map((label) => ({ name: label, color: palette[label], description: stated }))) }).labels,
        tailnet: tailnet.id,
        deployer: {
            secret: new ActionsEnvironmentSecret(`${name}-deploy-tailscale`, { repository, environment: environments.deploy, secretName: 'TAILSCALE_OAUTH_SECRET', value: joining.key }).secretName,
            ...Record.map({ TAILSCALE_CLIENT_ID: joining.id, TAILSCALE_MACHINES: machines.join(',') }, (value, variableName) => new ActionsEnvironmentVariable(`${name}-deploy-${variableName}`, { repository, environment: environments.deploy, variableName, value }).value),
        },
        updates: new TailnetSettings('tailnet', { devicesAutoUpdatesOn: true, httpsEnabled: true }).devicesAutoUpdatesOn,
        workstation: new DeviceKey('macbook', { deviceId: getDeviceOutput({ hostname: 'macbook' }).nodeId, keyExpiryDisabled: true }).keyExpiryDisabled,
        magicDns: new DnsPreferences('tailnet', { magicDns: true }).magicDns,
        assets: new Repository(`${name}-Assets`, { name: `${name}-Assets`, description: `Binary assets ${name} holds as a private submodule`, visibility: 'private', allowMergeCommit: false, deleteBranchOnMerge: true }, { protect: true }).fullName,
        access: new ZeroTrustOrganization(organization.name, { accountId, name: organization.name, authDomain: organization.authDomain, strictServiceTokenAuth: true }, { protect: true }).authDomain,
        identityProvider: new ZeroTrustAccessIdentityProvider(`${organization.name}-onetimepin`, { accountId, name: 'One-time PIN', type: 'onetimepin', config: {} }, { protect: true }).id,
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { configs, identified, repositoryStack };
