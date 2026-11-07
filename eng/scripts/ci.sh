#!/usr/bin/env bash
set -euo pipefail

# --- [OPERATIONS] ----------------------------------------------------------------------

# --- [LICENSE]
license() {
    if [[ -z $KEY ]]; then
        printf 'available=false\n' >>"$GITHUB_OUTPUT"
        printf '::notice::ImageSharp license is unavailable; licensed checks will be excluded.\n'
        return
    fi
    umask 077
    printf %s "$KEY" >"$RUNNER_TEMP/sixlabors.lic"
    printf 'SixLaborsLicenseFile=%s/sixlabors.lic\n' "$RUNNER_TEMP" >>"$GITHUB_ENV"
    printf 'available=true\n' >>"$GITHUB_OUTPUT"
}

# --- [CACHE]
cache_paths() {
    local database schema prefix cache
    nx show projects >/dev/null
    shopt -s nullglob
    set -- "$NX_WORKSPACE_DATA_DIRECTORY"/*.db
    if [[ $# != 1 ]]; then
        printf 'Expected one Nx database, found %s\n' "$#" >&2
        return 1
    fi
    database=${1##*/}
    schema=${database##*-}
    prefix="tasks-$RUNNER_OS-$RUNNER_ARCH-${schema%.db}-"
    cache=$(jq -er .cacheDirectory nx.json)
    printf 'database=%s\nprefix=%s\nkey=%s%s-%s\npaths<<PATHS\n%s\n%s/*-%s\nPATHS\n' "$database" "$prefix" "$prefix" "$GITHUB_SHA" "$GITHUB_RUN_ATTEMPT" "$cache" "$NX_WORKSPACE_DATA_DIRECTORY" "$schema" >>"$GITHUB_OUTPUT"
}

restore_cache() {
    local path active=${NX_CACHE_DATABASE:?}
    local -a restored=()
    shopt -s nullglob
    for path in "$NX_WORKSPACE_DATA_DIRECTORY"/*-"${active##*-}"; do
        [[ ${path##*/} == "$active" ]] || restored+=("$path")
    done
    case ${#restored[@]} in
    0) ;;
    1) mv -- "${restored[0]}" "$NX_WORKSPACE_DATA_DIRECTORY/$active" ;;
    *)
        printf 'Multiple cached Nx databases match %s\n' "${active##*-}" >&2
        return 1
        ;;
    esac
}

# --- [HOSTS]
rhino_download() {
    local version major url
    version=$(xmllint --xpath 'string(/Project/ItemGroup/PackageVersion[@Include="RhinoCommon"]/@Version)' Directory.Packages.props)
    major=${version%%.*}
    url=$(curl --silent --show-error --fail --head --location --output /dev/null --write-out '%{url_effective}' "https://www.rhino3d.com/www-api/download/direct?slug=rhino-for-mac/$major/latest")
    printf 'url=%s\npath=/Applications/Rhino %s.app\n' "$url" "$major" >>"$GITHUB_OUTPUT"
}

# --- [TARGETS]
run() {
    local excluded images projects targets command=affected
    targets=$(
        IFS=,
        printf '%s' "$*"
    )
    if [[ $NO_PREVIOUS_BUILD == true || $NX_BASE == "$NX_HEAD" ]] || ! git merge-base --is-ancestor "$NX_BASE" "$NX_HEAD"; then
        printf '::notice::No successful CI run on an earlier ancestor. Selecting all eligible projects.\n'
        command=run-many
    fi
    if [[ $RUNNER_OS == macOS ]]; then
        excluded='!tag:host:macos,!tag:host:rhino'
        [[ $RHINO_READY == true ]] || excluded+=',tag:host:rhino'
    else
        excluded='rasm,tag:host:macos,tag:host:rhino,tag:host:claude,tag:host:blender'
    fi
    if [[ $IMAGE_LICENSE != true ]]; then
        images=$(nx graph --file=stdout | jq -er '
            .graph
            | [.dependencies[][]] as $dependencies
            | [.nodes[] | select(.data.tags | index("license:imagesharp")) | .name] | sort
            | {previous: null, current: .}
            | until(.previous == .current;
                .current as $projects
                | {previous: $projects, current: ($projects + [$dependencies[] | select(.target as $target | $projects | index($target)) | .source] | unique)})
            | .current | map(",\(.)") | join("")
        ')
        excluded+="$images"
    fi
    if [[ $RUNNER_OS == macOS ]]; then
        if [[ $command == affected ]]; then
            projects=$(nx show projects --affected --with-target "$targets" --exclude="$excluded" --json | jq -er '["rasm", .[]] | unique | join(",")')
            set -- --projects "$projects"
        else
            set -- --exclude "$excluded,!directory:."
        fi
        exec nx run-many -t "$targets" --configuration ci "$@"
    fi
    exec nx "$command" -t "$targets" --configuration ci --exclude="$excluded"
}

# --- [COMPOSITION] ---------------------------------------------------------------------

case ${1:?} in
license) license ;;
cache-paths) cache_paths ;;
restore-cache) restore_cache ;;
rhino-download) rhino_download ;;
run) run "${@:2}" ;;
*)
    printf 'Unknown CI operation: %s\n' "$1" >&2
    exit 1
    ;;
esac
