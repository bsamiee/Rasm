set js to "(function(){try{var o={};o.viewerVersion=app.viewerVersion;o.viewerType=app.viewerType;o.menus=app.listMenuItems().length;o.printColorProfiles=app.printColorProfiles;o.numProfiles=Preflight.getNumProfiles();var names=[];for(var i=0;i<o.numProfiles;i++){var p=Preflight.getNthProfile(i);names.push(p?p.name:null);}o.profileNames=names;o.dp=Preflight.getProfileByName('Default Print')!==undefined;o.rasmTrusted=typeof rasmTrusted;o.typeofTF=typeof app.trustedFunction;o.numDocs=app.activeDocs.length;o.fsEscape=app.fs.escapeExits;return JSON.stringify({ok:true,v:o});}catch(e){return JSON.stringify({ok:false,error:String(e),name:e.name,message:e.message});}})()"
with timeout of 60 seconds
	tell application "Adobe Acrobat"
		do script js
	end tell
end timeout
