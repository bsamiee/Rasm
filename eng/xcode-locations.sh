#!/usr/bin/env bash
set -euo pipefail

# --- [CONSTANTS] ------------------------------------------------------------------------

root=$(git -C "$(dirname "$0")" rev-parse --show-toplevel)
output=$(jq -r '.plugins[] | objects | select(.plugin == "@rasm/nx-apple") | .options.outputPath // empty' "$root/nx.json")
style=$([[ -n $output ]] && echo WorkspaceRelativePath || echo Default)
readonly root output style

# --- [COMPOSITION] ----------------------------------------------------------------------

git -C "$root" ls-files -- '*.xcodeproj/project.xcproj' '*.xcodeproj/project.pbxproj' | while read -r file; do
    project=$(dirname "$file")
    settings="$root/$project/project.xcworkspace/xcuserdata/$USER.xcuserdatad/WorkspaceSettings.xcsettings"
    up=$(dirname "$project" | sed -E 's#^\.$##; s#[^/]+#..#g')
    mkdir -p "$(dirname "$settings")"
    [[ -f $settings ]] || plutil -create xml1 "$settings"
    plutil -replace DerivedDataLocationStyle -string "$style" "$settings"
    [[ -z $output ]] || plutil -replace DerivedDataCustomLocation -string "${up:-.}/$output/xcode" "$settings"
    plutil -p "$settings"
done
