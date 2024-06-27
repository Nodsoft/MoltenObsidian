import { defineIntegration } from "astro-integration-kit";

export const integration = defineIntegration({
	name: "package-name",
	setup() {
		return {
			hooks: {},
		};
	},
});
