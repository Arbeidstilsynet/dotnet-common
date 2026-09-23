import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { basename, join, resolve } from "node:path";

const [, , sourceArgument, outputArgument] = process.argv;

if (!sourceArgument || !outputArgument) {
  console.error("Usage: node prepare-specs.mjs <source-directory> <output-directory>");
  process.exit(1);
}

const sourceDirectory = resolve(sourceArgument);
const outputDirectory = resolve(outputArgument);

const specifications = [
  { file: "openapi-storage.json" },
  { file: "openapi-events.json" },
  {
    file: "openapi-authentication.json",
    server: "https://platform.tt02.altinn.no/authentication/api/v1",
  },
  { file: "openapi-correspondence-tt02.json" },
  { file: "openapi-app.json", server: "https://dat.apps.tt02.altinn.no" },
  { file: "openapi-dialogporten-tt02.json" },
];

mkdirSync(outputDirectory, { recursive: true });

for (const { file, server } of specifications) {
  const sourcePath = join(sourceDirectory, file);
  const outputPath = join(outputDirectory, basename(file));
  const source = readFileSync(sourcePath, "utf8");
  let prepared = source;

  if (server) {
    const specification = JSON.parse(source);

    if (!Array.isArray(specification.servers) || specification.servers.length === 0) {
      const { openapi, info, ...rest } = specification;
      prepared = `${JSON.stringify(
        { openapi, info, servers: [{ url: server }], ...rest },
        null,
        2,
      )}\n`;
    }
  }

  let existing;

  try {
    existing = readFileSync(outputPath, "utf8");
  } catch (error) {
    if (error.code !== "ENOENT") {
      throw error;
    }
  }

  if (existing !== prepared) {
    writeFileSync(outputPath, prepared, "utf8");
    console.log(`${file}: prepared`);
  }
}
