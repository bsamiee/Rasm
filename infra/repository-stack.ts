import { workspaceRoot } from '@nx/devkit';
import { getZeroTrustOrganizationOutput, getZoneOutput, R2Bucket, R2CustomDomain, ZeroTrustAccessIdentityProvider, ZeroTrustOrganization } from '@pulumi/cloudflare';
import { iam, projects, serviceaccount } from '@pulumi/gcp';
import { ActionsEnvironmentVariable, IssueLabels, Repository, RepositoryEnvironment, RepositoryVulnerabilityAlerts, WorkflowRepositoryPermissions } from '@pulumi/github';
import { VpsPostInstallScript } from '@pulumi/hostinger';
import { type Input, interpolate, type Output, jsonStringify, secret } from '@pulumi/pulumi';
import { Acl, DnsPreferences, FederatedIdentity, TailnetSettings } from '@pulumi/tailscale';
import { BranchConfig, Environment, Project, secretssync, Webhook } from '@pulumiverse/doppler';
import { run } from '@rasm/process';
import { Array, Effect, FileSystem, Path, Record, Schema, SchemaGetter, SchemaIssue, String, Struct } from 'effect';
import { Yaml } from 'effect/encoding';
import { hosts } from '#/platform-stack';

// --- [CONSTANTS] -----------------------------------------------------------------------

const palette = { bug: 'd73a4a', enhancement: 'a2eeef' };
const configs = ['dev', 'ci', 'release', 'deploy'] as const;
const settings = {
    visibility: 'public',
    hasIssues: true,
    allowAutoMerge: true,
    allowMergeCommit: false,
    allowUpdateBranch: true,
    deleteBranchOnMerge: true,
    squashMergeCommitTitle: 'PR_TITLE',
    squashMergeCommitMessage: 'PR_BODY',
    securityAndAnalysis: { secretScanning: { status: 'disabled' }, secretScanningPushProtection: { status: 'disabled' } },
} as const;
const identities = {
    release: { accountId: 'chrome-web-store', displayName: 'Chrome Web Store publishing', roles: [], variables: { CHROME_WEB_STORE_PUBLISHER: 'ead12e33-919c-497e-b162-59d21db1294a' } },
    deploy: { accountId: 'deploy', displayName: 'Stack deploys', roles: ['roles/browser', 'roles/serviceusage.serviceUsageAdmin', 'roles/iam.workloadIdentityPoolAdmin', 'roles/iam.serviceAccountAdmin'], variables: {} },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.NonEmptyString, description: Schema.NonEmptyString, repository: Schema.Struct({ url: Schema.TemplateLiteralParser(['git+https://github.com/', Schema.NonEmptyString, '/', Schema.NonEmptyString, '.git']) }) }));
const Uproject = Schema.fromJsonString(Schema.Struct({ EngineAssociation: Schema.optionalKey(Schema.Unknown) }));
const Release = Schema.String.check(Schema.isPattern(/^[0-9.]+$/u));
const Feed = Schema.TemplateLiteralParser(['SPARKLE_FEED_URL = ', Schema.URLFromString]);
const Form = Schema.String.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => Yaml.parse(text), catch: (cause) => new SchemaIssue.InvalidValue({ expected: 'YAML', cause }, text, options) })),
        encode: SchemaGetter.forbiddenEncoding,
    }),
    Schema.decodeTo(Schema.Struct({ description: Schema.NonEmptyString, labels: Schema.NonEmptyArray(Schema.Literals(Struct.keys(palette))) })),
);
const Policy = Schema.fromJsonString(Schema.Struct({ bindings: Schema.optionalKey(Schema.Array(Schema.Struct({ role: Schema.String, members: Schema.Array(Schema.String) }))) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const identified = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const manifest = yield* Effect.flatMap(fs.readFileString(path.join(workspaceRoot, 'package.json')), Schema.decodeEffect(Manifest));
    const xcconfig = yield* fs.readFileString(path.join(workspaceRoot, '.config/sparkle.xcconfig'));
    const { stdout } = yield* run(['git', 'ls-files', '-z', '--', '*.uproject'], { cwd: workspaceRoot });
    const engines = yield* Effect.forEach(stdout.split('\0').filter(String.isNonEmpty), (project) => Effect.flatMap(fs.readFileString(path.join(workspaceRoot, project)), Schema.decodeEffect(Uproject)));
    const [, { hostname }] = yield* Effect.fromOption(Array.findFirst(xcconfig.split('\n'), (line) => Schema.decodeUnknownOption(Feed)(line.replace('/$()/', '//'))));
    const [, owner, , repository] = manifest.repository.url;
    return { project: manifest.name, description: manifest.description, owner, repository, feed: { domain: hostname, zone: hostname.split('.').slice(1).join('.') }, engines: Array.dedupe(engines.map(Struct.get('EngineAssociation')).filter(Schema.is(Release))).toSorted() };
});

const managed = <E extends string>(name: string, { description, project, secrets = description, environments, branches = {} }: { readonly description: string; readonly project: string; readonly secrets?: string; readonly environments: readonly E[]; readonly branches?: Readonly<Record<string, E>> }) => {
    const { name: repository, fullName, repoId } = new Repository(name, { name, description, ...settings }, { protect: true });
    const doppler = new Project(project, { name: project, description: secrets }, { protect: true }).name;
    const held = Record.map(Struct.Record(environments, undefined), (_, slug) => new Environment(`${project}-${slug}`, { project: doppler, slug, name: slug }, { protect: true }).slug);
    const branched = Record.map(branches, (environment, branch) => new BranchConfig(`${project}-${branch}`, { project: doppler, environment: held[environment], name: branch }, { protect: true }).name);
    return { repository, fullName, repoId, doppler, held, branched };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const repositoryStack = Effect.fnUntraced(function* (
    { repository: name, project, description, feed: updates }: Effect.Success<typeof identified>,
    { CLOUDFLARE_ACCOUNT_ID: accountId, GITHUB_TOKEN: token, TAILSCALE_ENROLMENT_OAUTH_SECRET: enrolling }: Readonly<Record<'CLOUDFLARE_ACCOUNT_ID' | 'GITHUB_TOKEN' | 'TAILSCALE_ENROLMENT_OAUTH_SECRET', string>>,
) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const listed = yield* fs.glob('.github/ISSUE_TEMPLATE/*.yml', { root: workspaceRoot, exclude: ['**/config.yml'] });
    const forms = yield* Effect.forEach(listed, (form) => Effect.flatMap(fs.readFileString(path.join(workspaceRoot, form)), Schema.decodeEffect(Form)));
    const [cloud, organization] = [[project, 'release' satisfies (typeof configs)[number]].join('-'), getZeroTrustOrganizationOutput({ accountId })];
    const { repository, fullName, repoId, doppler, held } = managed(name, { description, project, environments: configs });
    const environments = Record.map(identities, (_, environment) => new RepositoryEnvironment(`${name}-${environment}`, { repository, environment }).environment);
    const synced = { integration: '51df65fc-7097-4d4d-a4ca-308ce2a5918a', project: doppler, syncTarget: 'repo', repoName: repository, deleteBehavior: 'delete_from_target' };
    const bucket = new R2Bucket(`${name}-updates`, { accountId, name: `${name.toLowerCase()}-updates` }, { protect: true });
    const [tags, deployer] = [Array.dedupe(Object.values(hosts).flatMap((row) => row.tags)), 'tag:deploy'];
    const policy = {
        tagOwners: Record.fromIterableWith([...tags, deployer], (tag) => [tag, ['autogroup:admin']]),
        grants: [{ src: ['*'], dst: ['*'], ip: ['*'] }],
        ssh: [{ action: 'accept', src: ['autogroup:member', deployer], dst: tags, users: ['autogroup:nonroot', 'root'] }],
        nodeAttrs: [{ target: ['autogroup:member'], attr: ['funnel'] }],
    };
    const tailnet = new Acl('tailnet', { acl: interpolate`${jsonStringify(policy, undefined, '\t')}\n`, overwriteExistingContent: true });
    const apis = ['serviceusage', 'cloudresourcemanager', 'iam', 'iamcredentials', 'sts', 'chromewebstore'].map((api) => new projects.Service(`${cloud}-${api}`, { project: cloud, service: `${api}.googleapis.com`, disableOnDestroy: false }));
    const pool = new iam.WorkloadIdentityPool(`${cloud}-github`, { project: cloud, workloadIdentityPoolId: 'github', displayName: 'GitHub Actions' }, { dependsOn: apis });
    const issuer = new iam.WorkloadIdentityPoolProvider(`${cloud}-github`, {
        project: cloud,
        workloadIdentityPoolId: pool.workloadIdentityPoolId,
        workloadIdentityPoolProviderId: 'github',
        displayName: name,
        attributeMapping: { 'google.subject': 'assertion.sub', 'attribute.environment': 'assertion.environment' },
        attributeCondition: interpolate`assertion.repository_id == '${repoId}'`,
        oidc: { issuerUri: 'https://token.actions.githubusercontent.com' },
    });
    const joining = new FederatedIdentity(`${name}-deploy`, { description: 'Deploy', issuer: 'https://token.actions.githubusercontent.com', subject: interpolate`repo:${fullName}:environment:${environments.deploy}`, scopes: ['auth_keys'], tags: [deployer] }, { dependsOn: tailnet });
    const published = { release: { SPARKLE_FEED_BUCKET: bucket.name }, deploy: { TAILSCALE_CLIENT_ID: joining.id, TAILSCALE_AUDIENCE: joining.audience } };
    const federated = Record.map(identities, ({ accountId: id, displayName, roles, variables }, environment) => {
        const account = new serviceaccount.Account(`${cloud}-${id}`, { project: cloud, accountId: id, displayName }, { dependsOn: apis });
        return {
            account: account.name,
            federation: new serviceaccount.IAMMember(`${cloud}-${id}-github`, { serviceAccountId: account.name, role: 'roles/iam.workloadIdentityUser', member: interpolate`principalSet://iam.googleapis.com/${pool.name}/attribute.environment/${environments[environment]}` }).member,
            roles: roles.map((role) => new projects.IAMMember(`${cloud}-${id}-${role}`, { project: cloud, role, member: account.member }).role),
            variables: Record.map<string, Input<string>, Output<string>>(
                { ...variables, ...published[environment], GOOGLE_WORKLOAD_IDENTITY_PROVIDER: issuer.name, GOOGLE_SERVICE_ACCOUNT: account.email },
                (value, variableName) => new ActionsEnvironmentVariable(`${name}-${environment}-${variableName}`, { repository, environment: environments[environment], variableName, value }).value,
            ),
        };
    });
    const owners = projects.getIamPolicyOutput({ project: cloud }, { dependsOn: apis }).policyData.apply(Schema.decodeUnknownPromise(Policy));
    return {
        repository: fullName,
        adopted: managed('Parametric_Forge', { description: 'My Nix based repo for nix-darwin configuration, dotfiles, and more', project: 'parametric-forge', secrets: 'macOS machine and Home Manager toolchain secrets', environments: ['dev'], branches: { dev_machine: 'dev' } }),
        configs: {
            ci: new secretssync.GithubActions(`${project}-ci`, { ...synced, config: held.ci }).config,
            release: new secretssync.GithubActions(`${project}-release`, { ...synced, config: held.release, environmentName: environments.release }).config,
            dispatch: new Webhook(`${project}-deploy`, { project: doppler, url: interpolate`https://api.github.com/repos/${fullName}/dispatches`, authentication: { type: 'Bearer', token: secret(token) }, payload: '{"event_type":"deploy"}', enabledConfigs: [held.deploy] }).slug,
        },
        feed: new R2CustomDomain(updates.domain, { accountId, bucketName: bucket.name, domain: updates.domain, enabled: true, zoneId: getZoneOutput({ filter: { name: updates.zone } }).zoneId }).domain,
        alerts: new RepositoryVulnerabilityAlerts(`${name}-vulnerability-alerts`, { repository, enabled: false }).enabled,
        workflowPermissions: new WorkflowRepositoryPermissions(`${name}-workflow-permissions`, { repository, defaultWorkflowPermissions: 'write', canApprovePullRequestReviews: true }).defaultWorkflowPermissions,
        labels: new IssueLabels(`${name}-labels`, { repository, labels: forms.flatMap(({ description: stated, labels }) => labels.map((label) => ({ name: label, color: palette[label], description: stated }))) }).labels,
        tailnet: tailnet.id,
        updates: new TailnetSettings('tailnet', { devicesAutoUpdatesOn: true, httpsEnabled: true, usersApprovalOn: false, aclsExternallyManagedOn: true, aclsExternalLink: interpolate`https://github.com/${fullName}/blob/HEAD/${path.relative(workspaceRoot, import.meta.filename)}` }).devicesAutoUpdatesOn,
        magicDns: new DnsPreferences('tailnet', { magicDns: true }).magicDns,
        enrolment: new VpsPostInstallScript(`${name}-enrolment`, {
            name: `${name} enrolment`,
            content: interpolate`#!/bin/sh\nset -eu\ncurl -fsSL https://tailscale.com/install.sh | sh\nrm -f /etc/apt/sources.list.d/tailscale.list\nsync\ntailscale up --auth-key='${secret(enrolling)}?ephemeral=false&preauthorized=true' --advertise-tags=${tags.join(',')} --ssh\n`,
        }).vpsPostInstallScriptId,
        assets: new Repository(`${name}-Assets`, { name: `${name}-Assets`, description: `Binary assets ${name} holds as a private submodule`, visibility: 'private', allowMergeCommit: false, deleteBranchOnMerge: true }, { protect: true }).fullName,
        access: new ZeroTrustOrganization('access', { accountId, name: organization.name, authDomain: organization.authDomain, strictServiceTokenAuth: true }, { protect: true, aliases: [{ name: organization.name }] }).authDomain,
        google: {
            ...federated,
            impersonation: new serviceaccount.IAMBinding(`${cloud}-${identities.release.accountId}-owners`, {
                serviceAccountId: federated.release.account,
                role: 'roles/iam.serviceAccountTokenCreator',
                members: owners.apply(({ bindings = [] }) => bindings.filter(({ role }) => role === 'roles/owner').flatMap(Struct.get('members'))),
            }).members,
        },
        identityProvider: new ZeroTrustAccessIdentityProvider('onetimepin', { accountId, name: 'One-time PIN', type: 'onetimepin', config: {} }, { protect: true, aliases: [{ name: interpolate`${organization.name}-onetimepin` }] }).id,
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { configs, identified, repositoryStack };
