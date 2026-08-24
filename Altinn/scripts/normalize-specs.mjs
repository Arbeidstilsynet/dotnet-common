// Adds a servers entry to the OpenAPI specifications that lack one.
//
// Kiota only emits the "baseurl" path parameter for a client when the specification declares a
// server. Without it the generated client compiles but throws at runtime as soon as it tries to
// build a URL:
//
//   PathParameters must contain a value for "baseurl" for the url to be built.
//
// The Altinn app and authentication specifications both omit servers, and Kiota reports this only
// as a warning during generation. The declared value itself is unimportant because registration
// always assigns the base URL resolved for the target environment before the client is built; it
// exists purely so that Kiota wires the path parameter up at all.
//
// This runs as part of the generate scripts so that refreshing a specification from Altinn cannot
// silently reintroduce the problem.

import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const publishDir = join(
  dirname(fileURLToPath(import.meta.url)),
  "..",
  "AT.Common.Altinn.Publish",
);

const specsRequiringServers = [
  {
    file: "openapi-authentication.json",
    url: "https://platform.tt02.altinn.no/authentication/api/v1",
  },
  { file: "openapi-app.json", url: "https://dat.apps.tt02.altinn.no" },
];

let changed = false;

for (const { file, url } of specsRequiringServers) {
  const path = join(publishDir, file);
  const spec = JSON.parse(readFileSync(path, "utf8"));

  if (Array.isArray(spec.servers) && spec.servers.length > 0) {
    console.log(`${file}: already declares a server, leaving unchanged`);
    continue;
  }

  // Reinsert the key so that servers sits next to info, as in the other specifications.
  const { openapi, info, ...rest } = spec;
  const updated = { openapi, info, servers: [{ url }], ...rest };

  writeFileSync(path, `${JSON.stringify(updated, null, 2)}\n`, "utf8");

  console.log(`${file}: added servers entry ${url}`);
  changed = true;
}

console.log(
  changed
    ? "\nSpecifications normalised. Regenerate the affected clients."
    : "\nNo changes required.",
);
