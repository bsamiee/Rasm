on marks(mi)
	set out to ""
	tell application "System Events"
		tell process "AdobeAcrobat"
			repeat with m in mi
				set n to name of m
				set mk to ""
				try
					set mk to value of attribute "AXMenuItemMarkChar" of m
				end try
				set en to enabled of m
				set out to out & n & "\t" & mk & "\t" & en & linefeed
			end repeat
		end tell
	end tell
	return out
end marks
tell application "System Events"
	tell process "AdobeAcrobat"
		set fr to frontmost
		set vis to visible
		set names to name of every menu bar item of menu bar 1
		set viewItems to every menu item of menu 1 of menu bar item "View" of menu bar 1
		set shItems to every menu item of menu 1 of menu item "Show/Hide" of menu 1 of menu bar item "View" of menu bar 1
		set rgItems to every menu item of menu 1 of menu item "Rulers & grids" of menu 1 of menu item "Show/Hide" of menu 1 of menu bar item "View" of menu bar 1
		set spItems to every menu item of menu 1 of menu item "Side Panels" of menu 1 of menu item "Show/Hide" of menu 1 of menu bar item "View" of menu bar 1
		set dtItems to every menu item of menu 1 of menu item "Display Theme" of menu 1 of menu bar item "View" of menu bar 1
		set pdItems to every menu item of menu 1 of menu item "Page Display" of menu 1 of menu bar item "View" of menu bar 1
	end tell
end tell
set r to "frontmost=" & fr & " visible=" & vis & linefeed & "MENUBAR: " & (names as text) & linefeed
set r to r & "--VIEW" & linefeed & my marks(viewItems)
set r to r & "--SHOW/HIDE" & linefeed & my marks(shItems)
set r to r & "--RULERS&GRIDS" & linefeed & my marks(rgItems)
set r to r & "--SIDE PANELS" & linefeed & my marks(spItems)
set r to r & "--DISPLAY THEME" & linefeed & my marks(dtItems)
set r to r & "--PAGE DISPLAY" & linefeed & my marks(pdItems)
return r
