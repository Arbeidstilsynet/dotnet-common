// Compares the production and TT02 OpenAPI specifications for the APIs that publish one per
// environment.
//
// The generated clients are built from the TT02 specifications because TT02 has so far been a
// strict superset of production: it carries features that have not yet been rolled out. That
// assumption is what makes a single generated client per API safe, so this script fails when it
// stops holding, i.e. when production declares a path or schema that TT02 does not.
//
// TT02-only additions are expected and reported for information only.

import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const publishDir = join(
  dirname(fileURLToPath(import.meta.url)),
  "..",
  "AT.Common.Altinn.Publish",
);

const specPairs = [
  {
    name: "correspondence",
    prod: "openapi-correspondence-prod.json",
    tt02: "openapi-correspondence-tt02.json",
  },
  {
    name: "dialogporten",
    prod: "openapi-dialogporten-prod.json",
    tt02: "openapi-dialogporten-tt02.json",
  },
];

const readSpec = (fileName) =>
  JSON.parse(readFileSync(join(publishDir, fileName), "utf8"));

const keysOf = (value) => Object.keys(value ?? {});

const missingFrom = (expected, actual) => {
  const present = new Set(actual);
  return expected.filter((key) => !present.has(key));
};

let hasProductionOnlyFeatures = false;

for (const { name, prod, tt02 } of specPairs) {
  const prodSpec = readSpec(prod);
  const tt02Spec = readSpec(tt02);

  const prodPaths = keysOf(prodSpec.paths);
  const tt02Paths = keysOf(tt02Spec.paths);
  const prodSchemas = keysOf(prodSpec.components?.schemas);
  const tt02Schemas = keysOf(tt02Spec.components?.schemas);

  const pathsOnlyInProd = missingFrom(prodPaths, tt02Paths);
  const schemasOnlyInProd = missingFrom(prodSchemas, tt02Schemas);
  const pathsOnlyInTt02 = missingFrom(tt02Paths, prodPaths);
  const schemasOnlyInTt02 = missingFrom(tt02Schemas, prodSchemas);

  console.log(`\n${name}`);
  console.log(
    `  paths: ${prodPaths.length} in prod, ${tt02Paths.length} in tt02`,
  );
  console.log(
    `  schemas: ${prodSchemas.length} in prod, ${tt02Schemas.length} in tt02`,
  );

  if (pathsOnlyInTt02.length > 0) {
    console.log(`  tt02-only paths (expected): ${pathsOnlyInTt02.join(", ")}`);
  }
  if (schemasOnlyInTt02.length > 0) {
    console.log(
      `  tt02-only schemas (expected): ${schemasOnlyInTt02.join(", ")}`,
    );
  }

  if (pathsOnlyInProd.length > 0 || schemasOnlyInProd.length > 0) {
    hasProductionOnlyFeatures = true;
    console.error(
      `  ERROR: production declares features missing from tt02, so the generated client cannot reach them.`,
    );
    if (pathsOnlyInProd.length > 0) {
      console.error(`    paths: ${pathsOnlyInProd.join(", ")}`);
    }
    if (schemasOnlyInProd.length > 0) {
      console.error(`    schemas: ${schemasOnlyInProd.join(", ")}`);
    }
  }
}

if (hasProductionOnlyFeatures) {
  console.error(
    "\nThe tt02-is-a-superset-of-production assumption no longer holds. Regenerate the affected " +
      "client from the production specification, or generate one client per environment.",
  );
  process.exit(1);
}

console.log("\nTT02 remains a superset of production.");
