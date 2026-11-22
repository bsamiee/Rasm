import { getZeroTrustOrganization, getZoneOutput, R2Bucket, R2CustomDomain, ZeroTrustAccessIdentityProvider, ZeroTrustOrganization } from '@pulumi/cloudflare';
import { iam, projects, serviceaccount } from '@pulumi/gcp';
import { ActionsEnvironmentSecret, ActionsEnvironmentVariable, IssueLabels, Repository, RepositoryEnvironment, RepositoryVulnerabilityAlerts, WorkflowRepositoryPermissions } from '@pulumi/github';
import { VpsPostInstallScript } from '@pulumi/hostinger';
import { type Input, interpolate, type Output, jsonStringify, secret } from '@pulumi/pulumi';
import { Acl, DnsPreferences, OauthClient, TailnetKey, TailnetSettings } from '@pulumi/tailscale';
import { Environment, Project, ServiceToken, secretssync, Webhook } from '@pulumiverse/doppler';
import { run } from '@rasm/process';
import { Array, Cause, Effect, FileSystem, Path, Record, Schema, SchemaGetter, SchemaIssue, String, Struct } from 'effect';
import { Yaml } from 'effect/encoding';
import { hosts } from '#/platform-stack.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const palette = { bug: 'd73a4a', enhancement: 'a2eeef' };
const feed = { setting: 'SPARKLE_FEED_URL', variable: 'SPARKLE_FEED_BUCKET' } as const;
const configs = ['dev', 'ci', 'release', 'deploy'] as const;
const google = {
    issuer: 'https://token.actions.githubusercontent.com',
    apis: ['serviceusage', 'cloudresourcemanager', 'iam', 'iamcredentials', 'sts', 'chromewebstore'],
    identities: {
        release: { accountId: 'chrome-web-store', displayName: 'Chrome Web Store publishing', roles: [], variables: { CHROME_WEB_STORE_PUBLISHER: 'ead12e33-919c-497e-b162-59d21db1294a' } },
        deploy: { accountId: 'deploy', displayName: 'Stack deploys', roles: ['roles/browser', 'roles/serviceusage.serviceUsageAdmin', 'roles/iam.workloadIdentityPoolAdmin', 'roles/iam.serviceAccountAdmin'], variables: {} },
    },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.NonEmptyString, description: Schema.NonEmptyString, repository: Schema.Struct({ url: Schema.TemplateLiteralParser(['git+https://github.com/', Schema.NonEmptyString, '/', Schema.NonEmptyString, '.git']) }) }));
const Uproject = Schema.fromJsonString(Schema.Struct({ EngineAssociation: Schema.optionalKey(Schema.Unknown) }));
const Release = Schema.String.check(Schema.isPattern(/^[0-9.]+$/u));
const Feed = Schema.TemplateLiteralParser([`${feed.setting} = `, Schema.URLFromString]);
const Form = Schema.String.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => Yaml.parse(text), catch: (cause) => new SchemaIssue.InvalidValue({ expected: 'YAML', cause }, text, options) })),
        encode: SchemaGetter.forbiddenEncoding,
    }),
    Schema.decodeTo(Schema.Struct({ description: Schema.NonEmptyString, labels: Schema.NonEmptyArray(Schema.Literals(Struct.keys(palette))) })),
);
const Policy = Schema.fromJsonString(Schema.Struct({ bindings: Schema.optionalKey(Schema.Array(Schema.Struct({ role: Schema.String, members: Schema.Array(Schema.String) }))) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const identified = Effect.fnUntraced(function* (root: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const [manifest, settings, engines] = yield* Effect.all([
        Effect.flatMap(fs.readFileString(path.join(root, 'package.json')), Schema.decodeEffect(Manifest)),
        fs.readFileString(path.join(root, 'Xcode.xcconfig')),
        Effect.flatMap(run(['git', 'ls-files', '-z', '--', '*.uproject'], { cwd: root }), ({ stdout }) => Effect.forEach(stdout.split('\0').filter(String.isNonEmpty), (project) => Effect.flatMap(fs.readFileString(path.join(root, project)), Schema.decodeEffect(Uproject)))),
    ]);
    const [, { hostname }] = yield* Effect.flatMap(Effect.fromNullishOr(settings.split('\n').find((line) => line.startsWith(feed.setting))), (line) => Schema.decodeUnknownEffect(Feed)(line.replace('/$()/', '//')));
    const [, owner, , repository] = manifest.repository.url;
    return { project: manifest.name, description: manifest.description, owner, repository, feed: { domain: hostname, zone: hostname.split('.').slice(1).join('.') }, engines: Array.dedupe(engines.map(Struct.get('EngineAssociation')).filter(Schema.is(Release))).toSorted() };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const repositoryStack = Effect.fnUntraced(function* (root: string, { repository: name, project, description, feed: updates }: Effect.Success<ReturnType<typeof identified>>, { CLOUDFLARE_ACCOUNT_ID: accountId, GITHUB_TOKEN: token }: { readonly CLOUDFLARE_ACCOUNT_ID: string; readonly GITHUB_TOKEN: string }) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const cloud = [project, 'release' satisfies (typeof configs)[number]].join('-');
    const [forms, organization, { bindings = [] }] = yield* Effect.all([
        Effect.flatMap(
            fs.glob('.github/ISSUE_TEMPLATE/*.yml', { root, exclude: ['**/config.yml'] }),
            Effect.forEach((form) => Effect.flatMap(fs.readFileString(path.join(root, form)), Schema.decodeEffect(Form))),
        ),
        Effect.tryPromise({ try: () => getZeroTrustOrganization({ accountId }), catch: (cause) => new Cause.UnknownError(cause, `Cloudflare account ${accountId} holds no Zero Trust organization`) }),
        Effect.flatMap(Effect.tryPromise({ try: () => projects.getIamPolicy({ project: cloud }), catch: (cause) => new Cause.UnknownError(cause, `Google Cloud project ${cloud} answers no IAM policy`) }), ({ policyData }) => Schema.decodeEffect(Policy)(policyData)),
    ]);
    const {
        name: repository,
        fullName,
        repoId,
    } = new Repository(
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
    const policy = {
        tagOwners: Record.fromIterableWith([...tags, deployer], (tag) => [tag, ['autogroup:admin']]),
        grants: [{ src: ['*'], dst: ['*'], ip: ['*'] }],
        ssh: [{ action: 'accept', src: ['autogroup:member', deployer], dst: tags, users: ['autogroup:nonroot', 'root'] }],
        nodeAttrs: [{ target: ['autogroup:member'], attr: ['funnel'] }],
    };
    const tailnet = new Acl('tailnet', { acl: interpolate`${jsonStringify(policy, undefined, '\t')}\n`, overwriteExistingContent: true });
    const joining = new OauthClient(`${name}-deploy`, { description: 'Deploy', scopes: ['auth_keys'], tags: [deployer] }, { dependsOn: tailnet });
    const enrolling = new TailnetKey(`${name}-machines`, { description: 'Machines', reusable: true, preauthorized: true, tags, recreateIfInvalid: 'always' }, { dependsOn: tailnet });
    const machines = Record.keys(hosts).map((host) => host.split('.', 1).join(''));
    const apis = google.apis.map((api) => new projects.Service(`${cloud}-${api}`, { project: cloud, service: `${api}.googleapis.com`, disableOnDestroy: false }));
    const pool = new iam.WorkloadIdentityPool(`${cloud}-github`, { project: cloud, workloadIdentityPoolId: 'github', displayName: 'GitHub Actions' }, { dependsOn: apis });
    const issuer = new iam.WorkloadIdentityPoolProvider(`${cloud}-github`, {
        project: cloud,
        workloadIdentityPoolId: pool.workloadIdentityPoolId,
        workloadIdentityPoolProviderId: 'github',
        displayName: name,
        attributeMapping: { 'google.subject': 'assertion.sub', 'attribute.environment': 'assertion.environment' },
        attributeCondition: interpolate`assertion.repository_id == '${repoId}'`,
        oidc: { issuerUri: google.issuer },
    });
    const federated = Record.map(google.identities, ({ accountId: id, displayName, roles, variables }, environment) => {
        const account = new serviceaccount.Account(`${cloud}-${id}`, { project: cloud, accountId: id, displayName }, { dependsOn: apis });
        return {
            account: account.name,
            federation: new serviceaccount.IAMMember(`${cloud}-${id}-github`, { serviceAccountId: account.name, role: 'roles/iam.workloadIdentityUser', member: interpolate`principalSet://iam.googleapis.com/${pool.name}/attribute.environment/${environments[environment]}` }).member,
            roles: roles.map((role) => new projects.IAMMember(`${cloud}-${id}-${role}`, { project: cloud, role, member: account.member }).role),
            variables: Record.map<string, Input<string>, Output<string>>(
                { ...variables, GOOGLE_WORKLOAD_IDENTITY_PROVIDER: issuer.name, GOOGLE_SERVICE_ACCOUNT: account.email },
                (value, variableName) => new ActionsEnvironmentVariable(`${name}-${environment}-${variableName}`, { repository, environment: environments[environment], variableName, value }).value,
            ),
        };
    });
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
        updates: new TailnetSettings('tailnet', { devicesAutoUpdatesOn: true, httpsEnabled: true, usersApprovalOn: false, aclsExternallyManagedOn: true, aclsExternalLink: interpolate`https://github.com/${fullName}/blob/HEAD/${path.relative(root, import.meta.filename)}` }).devicesAutoUpdatesOn,
        magicDns: new DnsPreferences('tailnet', { magicDns: true }).magicDns,
        enrolment: new VpsPostInstallScript(`${name}-enrolment`, { name: `${name} enrolment`, content: interpolate`#!/bin/sh\nset -eu\ncurl -fsSL https://tailscale.com/install.sh | sh\nrm /etc/apt/sources.list.d/tailscale.list\nsync\ntailscale up --auth-key=${enrolling.key} --ssh\n` }).vpsPostInstallScriptId,
        assets: new Repository(`${name}-Assets`, { name: `${name}-Assets`, description: `Binary assets ${name} holds as a private submodule`, visibility: 'private', allowMergeCommit: false, deleteBranchOnMerge: true }, { protect: true }).fullName,
        access: new ZeroTrustOrganization(organization.name, { accountId, name: organization.name, authDomain: organization.authDomain, strictServiceTokenAuth: true }, { protect: true }).authDomain,
        google: {
            ...federated,
            impersonation: new serviceaccount.IAMBinding(`${cloud}-${google.identities.release.accountId}-owners`, { serviceAccountId: federated.release.account, role: 'roles/iam.serviceAccountTokenCreator', members: bindings.filter(({ role }) => role === 'roles/owner').flatMap(Struct.get('members')) }).members,
        },
        identityProvider: new ZeroTrustAccessIdentityProvider(`${organization.name}-onetimepin`, { accountId, name: 'One-time PIN', type: 'onetimepin', config: {} }, { protect: true }).id,
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { configs, identified, repositoryStack };
