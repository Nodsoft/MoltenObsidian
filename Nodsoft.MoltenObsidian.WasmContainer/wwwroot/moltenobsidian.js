/* jshint esversion: 11 */
/* globals Blazor */
'use strict';

// `document.CurrentScript` cannot be fetched once in an async context, so we need to store it in a variable first.
const currentScript = document.currentScript;
// Base URI is the href minus the filename of the script. This covers all cases, including when the script is loaded from a CDN.
const currentBaseUri = currentScript.src.substring(0, currentScript.src.lastIndexOf('/'));

import("./_framework/blazor.webassembly.js").then(_ => {
	const vaultUri = currentScript.dataset.vaultUri;
	const baseSlug = currentScript.dataset.baseSlug ?? window.location.pathname + window.location.search + window.location.hash ?? '/';
	const elementsSelector = document.getElementsByClassName('moltenobsidian-content')[0];
	
	Blazor.start({
		loadBootResource: function (type, name, defaultUri, integrity) {
			if (type === 'dotnetjs') {
				return `${currentBaseUri}/_framework/${name}`;
			} else {
				return fetch(`${currentBaseUri}/_framework/${name}`, {
					credentials: "omit",
					integrity,
				});
			}
		}

	}).then(() => {
		Blazor.rootComponents.add(elementsSelector, "moltenobsidian-display-remote", {
			baseSlug,
			vaultUri,
		});
	}).catch(console.error);
});



