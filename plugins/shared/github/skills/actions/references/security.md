# [SECURITY]

Steps here join `SKILL.md` [STEPS] for tokens, OIDC, pinning, untrusted events, environments, and publishing, each removing a stored key or an unused write.

## [01]-[TOKENS]

`GITHUB_TOKEN` holds the scopes a workflow or job declares, and an undeclared workflow takes the repository default:
1. Open each workflow with `permissions: {}` and grant each job the scopes its steps use, never `read-all` or `write-all`
2. Hold gate and notify jobs reading `needs` alone at `permissions: {}`
3. Set `persist-credentials: false` on every checkout except the job pushing commits or tags
4. Hand a push token to the pushing step alone through `GIT_CONFIG_COUNT`, `GIT_CONFIG_KEY_0`, and `GIT_CONFIG_VALUE_0` from a masked value
5. Give installer actions taking a token input an empty one inside a job holding a write scope (`jdx/mise-action` with `github_token: ""`)
6. Set the repository default workflow token to read, so a workflow missing `permissions` gets read scopes

Scopes by work:
- Run, job, and cache reads -> `actions: read`
- Cache deletes, reruns, and dispatches -> `actions: write`
- Commits, tags, and releases -> `contents: write`
- Pull request comments -> `pull-requests: write`, issue comments `issues: write`
- Registry pushes to GitHub Packages -> `packages: write`
- Check runs -> `checks: write`
- OIDC exchange -> `id-token: write`, on the job exchanging alone
- Provenance -> `attestations: write`, and `artifact-metadata: write` for registry storage records

## [02]-[OIDC]

Jobs holding `id-token: write` mint a short-lived token whose claims a cloud or registry trusts in place of a stored key:
- Subject -> `sub` reads `repo:<owner>/<repo>:<context>`, the context `ref:<ref>`, `environment:<name>`, or `pull_request`
- Immutable form -> repositories created, renamed, or transferred after mid-2026 take `repo:<owner>@<owner_id>/<repo>@<repo_id>:...`
- Trust policies -> bind `repository_id` or the immutable subject, as a name-based subject breaks on rename
- Opting in -> moving a repository to the immutable form breaks every name-based policy until it changes
- Environments -> a job's `environment:` puts `environment:<name>` in `sub`, the narrowest claim a policy can require
- Called workflows -> tokens minted inside one carry the caller's `sub`, so policies bind `job_workflow_ref` (`<owner>/<repo>/.github/workflows/<file>@<ref>`) to name the workflow allowed to mint
- Audience -> `aud` defaults to the repository owner's URL, and each consumer checks the audience its exchange sets
- Subject template -> `gh api repos/{owner}/{repo}/actions/oidc/customization/sub` reads it, a `PUT` with `include_claim_keys` adds claims such as `job_workflow_ref`
- Empty environment -> `environment: ${{ case(<trusted>, '<name>', '') }}` runs forks and unlabeled runs with no environment and no credentials
- Scoped writes -> a first role assumes a second role with session tags (`custom-tags: '{"sha": "..."}'`), its policy limiting writes to that commit
- Forks -> fork pull requests get no OIDC token and no secrets

## [03]-[PINNING]

Actions run the code their reference names at run time:
1. Pin every third-party and `actions/*` action by full commit SHA with its tag as comment (`uses: <owner>/<action>@<sha> # v<version>`)
2. Resolve each SHA from the tag through `gh api repos/<owner>/<action>/commits/<tag> --jq .sha`, never from memory
3. Move pins with `jactionlint update --apply` or Dependabot, with one cooldown of 3 to 7 days across every updater
4. Reference same-repository actions and reusable workflows as `$/.github/...`, which reads the running commit
5. Pin reusable workflows of other repositories by SHA and container images by digest (`<image>@sha256:<digest>`), as lockfiles lock neither
6. Add `.github/workflows/actions.lock` where transitive actions exist: `gh extension install github/gh-actions-lock --pin v0.5.1`, then `gh actions-lock --no-narrow`, checked by `--verify-local`

Lockfile behavior:
- Locked commits run in place of the reference, checked against `owner_id` and `repo_id` at run time
- Commits only a fork holds, or ones no longer reachable from their tag, are refused
- `./` local actions are never followed, and one calling remote actions blocks until `--migrate-local-actions` rewrites it to `$/`
- Aliases in a `uses:` value are not detected, so anchors cover whole steps or values outside `uses:`
- Repository policies "Require actions to be pinned to a full-length commit SHA" and "Require lockfile" refuse a run before it starts

## [04]-[UNTRUSTED]

Code from a pull request head runs only where no secret and no write token exists, judged by the trust of each trigger:
- `pull_request` -> fork runs hold a read token and no secrets, the safe place to build and test pull request code
- `pull_request_target` -> base-branch workflow with secrets, blocked by default in public repositories from December 2026
- `pull_request_target` jobs -> check out no head code and write no head value to `GITHUB_ENV`, `GITHUB_OUTPUT`, or `GITHUB_PATH`
- `workflow_run` -> base-branch code with secrets turning a pull request run's artifact into a comment, parsing it as data alone
- Pull request numbers from a `workflow_run` artifact -> looked up by branch through the API, as the pull request author wrote the artifact
- `repository_dispatch` -> every `client_payload` field is caller-set, a number validated against `^[0-9]+$` before it reaches a ref
- Checkout refusals -> `actions/checkout` refuses a fork's head inside `pull_request_target` and `workflow_run` unless `allow-unsafe-pr-checkout`
- Labels -> a label gate on `pull_request_target` opens for anyone with triage rights
- Linter findings on these triggers -> candidates, a finding only where a lower-trust actor reaches a step using the higher scope

Injection rules:
- `case()`, `format()`, and every other expression returning an event field splice it where it lands, so they return it into `env:` alone
- `format('["{0}"]', github.ref_name)` builds JSON from text, where `toJSON(github.ref_name)` escapes it
- Reruns keep their original event payload, so a job reading live labels reads them through `gh api`

## [05]-[ENVIRONMENTS]

Environments hold secrets, variables, protection rules, and the OIDC `environment` claim:
- `environment: { name: <env>, deployment: false }` reads secrets and variables with no deployment object, refusing custom protection rules
- Deployment branch policies restrict which refs enter the environment, `deployment: false` included, the guard on a dispatch run from any branch
- Reviewers and wait timers hold a job until approval
- Free plans apply reviewers and wait timers to public repositories alone
- Dispatch inputs of `type: environment` list the repository's environments, read as `environment: ${{ inputs.<name> }}`
- Registry trusted publishers name an optional environment, matched by the publish job's `environment:`

## [06]-[PUBLISHING]

Registries exchange the job's OIDC token for a short-lived credential, fed to the variable the publish command reads:

| [INDEX] | [REGISTRY]  | [EXCHANGE]                                      | [VARIABLE]             | [LIMITS]                                       |
| :-----: | :---------- | :---------------------------------------------- | :--------------------- | :--------------------------------------------- |
|  [01]   | `npm`       | `npm publish` from npm 11.5.1+ on Node 22.14.0+ | None                   | Hosted runners, provenance by default          |
|  [02]   | PyPI        | `pypa/gh-action-pypi-publish`                   | None                   | Generates PEP 740 attestations                 |
|  [03]   | PyPI        | `uv publish --trusted-publishing always`        | None                   | Uploads existing `*.publish.attestation` files |
|  [04]   | `crates.io` | `rust-lang/crates-io-auth-action`               | `CARGO_REGISTRY_TOKEN` | First version by token, 30-minute life         |
|  [05]   | NuGet       | `NuGet/login` with `user`                       | `NUGET_API_KEY`        | Policy names workflow and environment          |

npm trusted publishing refuses a publish on each of these facts:
- `package.json` `repository.url` differs from the publishing repository
- Publisher allowing `npm stage publish` alone, where the workflow runs `npm publish`
- Publisher unused 2 days after creation expires, so the first publish follows the same day
- Publish job without `actions/setup-node` runs the image's npm, below 11.5.1 on older images

1. Register each registry's trusted publisher for the workflow file and environment that publishes, and set `node-version` in npm publish jobs
2. Grant `id-token: write` on the publish job alone, and `contents: write` on the job creating the release
3. Turn token publishing off per package once its trusted publisher works (npm "disallow tokens", crates.io trusted-publishing-only)
4. Attest release files with `actions/attest` (`subject-path`, or `subject-checksums: <file>`), verified by `gh attestation verify <file> -R <repo>`
5. Build release files under `cache-mode: none`, which the runner exports to every action as `ACTIONS_CACHE_MODE`, as a poisoned entry lands in every artifact
6. Publish immutable releases by uploading every asset to a draft and publishing it last, as tags and assets freeze at publish
