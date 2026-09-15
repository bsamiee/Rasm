(function(){var o=[];o.push("version="+app.version);o.push("docs="+app.documents.length);
var l=app.languagesWithVendors.everyItem().name;o.push("langCount="+l.length);o.push("langs="+l.join("|"));
var ma=app.menuActions.everyItem().name;var hits=[];for(var i=0;i<ma.length;i++){var n=ma[i];if(/World-Ready|Toolbar|Composer/i.test(n))hits.push(n);}
o.push("menuActionCount="+ma.length);o.push("hits="+hits.join("|"));
o.push("keys="+app.findKeyStrings("Adobe World-Ready Paragraph Composer").join("|"));
return o.join("\n");})()
