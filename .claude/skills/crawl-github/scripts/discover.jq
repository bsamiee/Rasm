# --- [CONSTANTS] ------------------------------------------------------------------------

def search:
    "query($q: String!, $first: Int!, $after: String) { search(query: $q, type: REPOSITORY, first: $first, after: $after) {"
    + " repositoryCount pageInfo { hasNextPage endCursor } nodes { ... on Repository { nameWithOwner } } } }";

def facts:
    "nameWithOwner description homepageUrl stargazerCount forkCount diskUsage createdAt pushedAt isArchived"
    + " parent { nameWithOwner } licenseInfo { spdxId } repositoryTopics(first: 20) { nodes { topic { name } } }"
    + " languages(first: 8, orderBy: {field: SIZE, direction: DESC}) { totalSize edges { size node { name } } }"
    + " releases(first: 1, orderBy: {field: CREATED_AT, direction: DESC}) { totalCount nodes { tagName publishedAt } }"
    + " issues(states: OPEN) { totalCount } merged: pullRequests(states: MERGED) { totalCount } submodules(first: 50) { nodes { path } }"
    + " defaultBranchRef { name target { ... on Commit {"
    + " oid committedDate statusCheckRollup { state } recent: history(since: $since) { totalCount } } } }"
    + " root: object(expression: \"HEAD:\") { ... on Tree { entries { name type } } }";

# --- [OPERATIONS] -----------------------------------------------------------------------

def names:
    reduce (inputs | scan("github\\.com/([A-Za-z0-9-]+/[\\w.-]+?)(?:\\.git)?(?:[^\\w.-]|\\.?$)|^([A-Za-z0-9-]+/[\\w.-]+)(?:\\t.*)?$") | map(values)[0]) as $name
        ({seen: {}, kept: []}; ($name | ascii_downcase) as $key | if .seen[$key] then . else .seen[$key] = true | .kept += [$name] end)
    | .kept;

def queries($probes):
    ($probes | to_entries | map("p\(.key): object(expression: \("HEAD:" + .value | @json)) { ... on Tree { entries { name type } } ... on Blob { byteSize } }") | join(" ")) as $paths
    | names
    | range(0; length; 10) as $start
    | .[$start:$start + 10]
    | to_entries
    | map((.value / "/") as [$owner, $name] | "r\(.key): repository(owner: \($owner | @json), name: \($name | @json)) { ...Facts }")
    | "query($since: GitTimestamp!) { \(join(" ")) } fragment Facts on Repository { \(facts) \($paths) }";

def entries: map(.name + (if .type == "tree" then "/" else "" end));

def rows($probes):
    (.errors // [] | map(select(.type != "NOT_FOUND").message) | if length > 0 then error(join("; ")) else empty end),
    (.data[]
        | values
        | . as $repo
        | .defaultBranchRef.target as $head
        | {
            repo: .nameWithOwner, stars: .stargazerCount, forks: .forkCount, kb: .diskUsage, created: .createdAt[:10], pushed: .pushedAt[:10],
            branch: .defaultBranchRef.name, commit: $head.oid, committed: $head.committedDate[:10], commits_since: $head.recent.totalCount,
            ci: $head.statusCheckRollup.state, merged_prs: .merged.totalCount, open_issues: .issues.totalCount,
            releases: .releases.totalCount, release: .releases.nodes[0].tagName, released: .releases.nodes[0].publishedAt[:10],
            license: .licenseInfo.spdxId, archived: .isArchived, fork_of: .parent.nameWithOwner,
            langs: (.languages | .totalSize as $total | .edges | map({key: .node.name, value: (.size * 1000 / $total | floor / 10)}) | from_entries),
            topics: [.repositoryTopics.nodes[].topic.name], submodules: [.submodules.nodes[].path], root: (.root.entries // [] | entries),
            paths: ($probes | to_entries | map({key: .value, value: ($repo["p\(.key)"] | if .entries then .entries | entries else .byteSize end)}) | from_entries),
            homepage: .homepageUrl, description
        });

def baseline:
    length as $total
    | (.[0].paths // {} | keys | map("probe\t" + .)) as $probes
    | map((.root | map("root\t" + .))
        + (.paths | to_entries | map(select(.value != null) | "probe\t" + .key))
        + (if .ci then ["fact\tchecks on head commit"] else [] end)
        + (if .releases > 0 then ["fact\trelease"] else [] end)
        + (if .submodules == [] then [] else ["fact\tsubmodules"] end)
        | unique)
    | (add // []) + $probes
    | group_by(.)
    | map({name: .[0], count: (length - (if .[0] | IN($probes[]) then 1 else 0 end))})
    | sort_by(-.count, .name)
    | .[]
    | select(.count > 1 or (.name | startswith("root") | not))
    | "\(.count * 100 / $total | round)%\t\(.count)/\($total)\t\(.name)";

def distinct:
    foreach inputs as $row ({seen: {}}; ($row.repo | ascii_downcase) as $key | .fresh = (.seen[$key] | not) | .seen[$key] = true; select(.fresh) | $row);
