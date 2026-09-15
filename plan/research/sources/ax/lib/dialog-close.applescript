-- dialog-close — dismiss a dialog without committing anything.
--
--   osascript dialog-close.applescript "<process>" "<window name>"
--
-- Order matters. An AX-visible dialog is closed by pressing its own Cancel
-- button. A dialog drawn by Adobe's Drover framework exposes no AX button and
-- ignores Escape, but does honour Command-period, the standard Cancel key
-- equivalent. OK, Apply and Save are never pressed.

on run argv
	set procName to item 1 of argv
	set winName to item 2 of argv
	tell application "System Events"
		tell process procName
			repeat with label in {"Cancel", "Close", "Done"}
				try
					click button (label as string) of window winName
					return "clicked " & (label as string)
				end try
			end repeat
		end tell
	end tell
	-- Drover dialog: no AX button, ignores Escape, honours Command-period. The app
	-- must be genuinely frontmost first, and a loaded Adobe app can take seconds to
	-- get there, so activation is confirmed rather than assumed.
	try
		tell application "System Events" to set frontmost of process procName to true
		repeat 20 times
			delay 0.25
			tell application "System Events"
				if (name of first process whose frontmost is true) is procName then exit repeat
			end tell
		end repeat
		delay 0.5
		tell application "System Events" to keystroke "." using command down
		delay 1.5
		return "command-period"
	on error errMsg
		return "ERROR: " & errMsg
	end try
end run
