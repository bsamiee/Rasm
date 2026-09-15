-- dialog-pane — list or select one pane in a preferences dialog's left list.
--
--   osascript dialog-pane.applescript "<process>" "<window>" count
--   osascript dialog-pane.applescript "<process>" "<window>" select <index>
--
-- Prints the pane count, or the name of the pane selected. Adobe dialogs vary in
-- whether the pane list is a table, an outline or a plain list, so all three are
-- tried before giving up.

on paneRows(win)
	-- Each accessor is tried in its own block: an AppleScript list literal
	-- evaluates every element eagerly, so one invalid index would throw before
	-- any later candidate could be reached.
	tell application "System Events"
		try
			set r to every row of outline 1 of scroll area 1 of win
			if (count of r) > 0 then return r
		end try
		try
			set r to every row of table 1 of scroll area 1 of win
			if (count of r) > 0 then return r
		end try
		try
			set r to every UI element of list 1 of scroll area 1 of win
			if (count of r) > 0 then return r
		end try
		try
			set r to every row of table 1 of win
			if (count of r) > 0 then return r
		end try
		try
			set r to every static text of scroll area 1 of win
			if (count of r) > 0 then return r
		end try
		return {}
	end tell
end paneRows

on run argv
	set procName to item 1 of argv
	set winName to item 2 of argv
	set verb to item 3 of argv
	tell application "System Events"
		tell process procName
			try
				set win to window winName
			on error errMsg
				return "ERROR: " & errMsg
			end try
			set theRows to my paneRows(win)
			if verb is "count" then return (count of theRows) as string
			set idx to (item 4 of argv) as integer
			if idx > (count of theRows) then return "ERROR: index out of range"
			set theRow to item idx of theRows
			set rowName to ""
			try
				set rowName to value of static text 1 of theRow
			end try
			if rowName is "" then
				try
					set rowName to value of theRow
				end try
			end if
			if rowName is "" then
				try
					set rowName to name of theRow
				end try
			end if
			try
				set selected of theRow to true
			on error
				try
					click theRow
				on error errMsg
					return "ERROR: " & errMsg
				end try
			end try
			delay 0.4
			return rowName
		end tell
	end tell
end run
