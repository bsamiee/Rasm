# [INFRASTRUCTURE]

The infrastructure program in any language declares every resource outside the repository tree as a typed row it reads by key, one program per application with a stack per environment, and the repository's own program for its settings and its secret store.

## [01]-[PROGRAM]

The repository program declares the store project, its configs, and its tokens, and the repository settings, secrets, and variables, as typed rows the program reads by key:
- The store holds one project, a config per environment, and a branch config named `<environment>_<suffix>` for repository automation
- A runtime secret enters the branch config once, from stdin, through the command the `secrets` skill names
- Service token rows name the config and the access level, and an Actions secret row holds a token's key under the name the workflows read
- The repository row holds the merge, branch, and feature settings, `protect` refuses its deletion, and `archiveOnDestroy` archives it
- Adopt a resource that exists through import in place of creating it, and the row declares the adoption as its resource option
- `up --import` adopts the project, environments, branch configs, and repository
- Tokens and secrets are created, with no import
- Read every credential through the default provider of its package from the environment alone, and the program passes no token
- The repository provider detects the owner from its token

The program's dependencies sit in the root catalog and manifest, and the root `tsconfig.json` includes its files for the root `typecheck` target.

## [02]-[RUN]

`up` applies the rows and `refresh` reads the live state back, and the summary of resource changes is the proof:
- The root `up` and `refresh` targets run the program's entry under `doppler run --project <project> --config <config>`
- The entry runs the stack through the Automation API over a file backend under the state directory the XDG specification names
- Plugins and credentials sit under `.cache/pulumi/`, and the passphrase secrets provider reads `PULUMI_CONFIG_PASSPHRASE` from the environment
- Each run prints the operation's output and the JSON of its resource changes, and a failed select, `up`, or `refresh` prints the diagnostic
- Take each provider and each provisioned runtime, image, and service at its newest release, and pin nothing outside the lockfile

## [03]-[SHARING]

Share nothing between application programs by position, and an application consumes another's output through a published package or a declared output.

## [04]-[ANTI_PATTERNS]

| [INDEX] | [SMELL]                                              | [CORRECT_FORM]                                                          |
| :-----: | :--------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | Second secret route copied from another repository   | One store, the variable in the environment, an error naming unset names |

Use `pulumi` for the program mechanics: resources, adoption through import, state backends, and destroys.
Use `secrets` for where a secret belongs and how a token reaches a process.
