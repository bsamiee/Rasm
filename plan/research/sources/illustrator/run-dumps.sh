#!/bin/zsh
S="/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/5491787f-d5bf-4f29-95b6-7ed9e2976566/scratchpad/illustrator"
G="$HOME/Library/CloudStorage/GoogleDrive-b.samiee@mzn-group.com/My Drive/03.Digital Asset Database"

run() {
  doc="$1"; out="$2"; mode="$3"
  printf '%s\n%s\n%s\n' "$doc" "$out" "$mode" > "$S/job.txt"
  echo "=== START $(date +%T) $doc"
  start=$(date +%s)
  osascript -e "tell application id \"com.adobe.illustratorBeta\" to do javascript (POSIX file \"$S/dump.jsx\")" 2>&1 | head -5
  echo "=== END $(date +%T) elapsed $(( $(date +%s) - start ))s -> $(ls -la "$out" 2>&1)"
}

run "$G/My Default Profile.ai" "$S/b-my-default-profile-drive-root.json" "deep"
run "$G/05.Software Related Assets/99.Default Profiles/00.Adobe Illustrator/My Default Profile.ai" "$S/c-my-default-profile-fork.json" "deep"
run "$G/05.Software Related Assets/02.Color Swatches/My Color Palette.ai" "$S/d-my-color-palette.json" "deep"
echo "ALL DONE"
