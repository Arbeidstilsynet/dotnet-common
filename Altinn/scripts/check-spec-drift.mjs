// Verifies that a client generated from TT02 can safely consume the production API.
// TT02-only additions are allowed, but every production operation and every structure it uses
// must also be represented compatibly in TT02.

import { readFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const httpMethods = new Set([
  "delete",
  "get",
  "head",
  "options",
  "patch",
  "post",
  "put",
  "trace",
]);

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

const keysOf = (value) => Object.keys(value ?? {});

const missingFrom = (expected, actual) => {
  const present = new Set(actual);
  return expected.filter((key) => !present.has(key));
};

const parameterKey = (parameter) => `${parameter?.in}:${parameter?.name}`;

function compareSchema(expected, actual, location, issues) {
  if (!actual) {
    issues.push(`${location} is missing`);
    return;
  }

  if (expected.$ref && expected.$ref !== actual.$ref) {
    issues.push(
      `${location} references ${expected.$ref}, but TT02 references ${actual.$ref ?? "no schema"}`,
    );
    return;
  }

  for (const keyword of ["type", "format"]) {
    if (expected[keyword] && expected[keyword] !== actual[keyword]) {
      issues.push(
        `${location} has ${keyword} ${expected[keyword]} in production, but ${actual[keyword] ?? "none"} in TT02`,
      );
    }
  }

  if (expected.nullable === true && actual.nullable !== true) {
    issues.push(`${location} is nullable in production, but not in TT02`);
  }

  if (expected.enum) {
    const missingValues = expected.enum.filter(
      (value) => !actual.enum?.some((candidate) => Object.is(candidate, value)),
    );
    if (missingValues.length > 0) {
      issues.push(
        `${location} is missing production enum value(s) in TT02: ${missingValues.map(JSON.stringify).join(", ")}`,
      );
    }
  }

  const expectedProperties = expected.properties ?? {};
  const actualProperties = actual.properties ?? {};
  for (const [name, schema] of Object.entries(expectedProperties)) {
    compareSchema(schema, actualProperties[name], `${location}.${name}`, issues);
  }

  const productionRequired = new Set(expected.required ?? []);
  const extraTt02Required = (actual.required ?? []).filter(
    (name) => !productionRequired.has(name),
  );
  if (extraTt02Required.length > 0) {
    issues.push(
      `${location} makes production-optional propert${extraTt02Required.length === 1 ? "y" : "ies"} required in TT02: ${extraTt02Required.join(", ")}`,
    );
  }

  if (expected.items) {
    compareSchema(expected.items, actual.items, `${location}[]`, issues);
  }

  if (
    typeof expected.additionalProperties === "object" &&
    expected.additionalProperties !== null
  ) {
    compareSchema(
      expected.additionalProperties,
      actual.additionalProperties,
      `${location}{additionalProperties}`,
      issues,
    );
  }

  for (const composition of ["allOf", "anyOf", "oneOf"]) {
    if (expected[composition]) {
      if (!Array.isArray(actual[composition])) {
        issues.push(`${location}.${composition} is missing`);
        continue;
      }
      expected[composition].forEach((schema, index) =>
        compareSchema(
          schema,
          actual[composition][index],
          `${location}.${composition}[${index}]`,
          issues,
        ),
      );
    }
  }
}

function compareContent(expected, actual, location, issues) {
  for (const [mediaType, media] of Object.entries(expected ?? {})) {
    if (!actual?.[mediaType]) {
      issues.push(`${location} content type ${mediaType} is missing`);
      continue;
    }
    if (media.schema) {
      compareSchema(
        media.schema,
        actual[mediaType].schema,
        `${location} content ${mediaType}`,
        issues,
      );
    }
  }
}

function compareParameters(expected, actual, location, issues) {
  const actualByKey = new Map((actual ?? []).map((item) => [parameterKey(item), item]));
  const expectedByKey = new Map((expected ?? []).map((item) => [parameterKey(item), item]));

  for (const parameter of expected ?? []) {
    const key = parameterKey(parameter);
    const counterpart = actualByKey.get(key);
    if (!counterpart) {
      issues.push(`${location} parameter ${key} is missing`);
      continue;
    }
    if (parameter.schema) {
      compareSchema(
        parameter.schema,
        counterpart.schema,
        `${location} parameter ${key}`,
        issues,
      );
    }
  }

  for (const parameter of actual ?? []) {
    const key = parameterKey(parameter);
    if (parameter.required === true && !expectedByKey.has(key)) {
      issues.push(`${location} has an additional required TT02 parameter: ${key}`);
    }
  }
}

function compareOperation(expected, actual, location, issues) {
  compareParameters(expected.parameters, actual.parameters, location, issues);

  if (expected.requestBody) {
    if (!actual.requestBody) {
      issues.push(`${location} request body is missing`);
    } else {
      if (actual.requestBody.required === true && expected.requestBody.required !== true) {
        issues.push(`${location} request body is optional in production, but required in TT02`);
      }
      compareContent(
        expected.requestBody.content,
        actual.requestBody.content,
        `${location} request body`,
        issues,
      );
    }
  }

  for (const [status, response] of Object.entries(expected.responses ?? {})) {
    const counterpart = actual.responses?.[status];
    if (!counterpart) {
      issues.push(`${location} response ${status} is missing`);
      continue;
    }
    compareContent(
      response.content,
      counterpart.content,
      `${location} response ${status}`,
      issues,
    );
  }
}

export function findCompatibilityIssues(prodSpec, tt02Spec) {
  const issues = [];
  const tt02Paths = tt02Spec.paths ?? {};

  for (const [path, prodPath] of Object.entries(prodSpec.paths ?? {})) {
    const tt02Path = tt02Paths[path];
    if (!tt02Path) {
      issues.push(`path ${path} is missing`);
      continue;
    }

    compareParameters(prodPath.parameters, tt02Path.parameters, `path ${path}`, issues);
    for (const [method, operation] of Object.entries(prodPath)) {
      if (!httpMethods.has(method.toLowerCase())) {
        continue;
      }
      const counterpart = tt02Path[method];
      if (!counterpart) {
        issues.push(`operation ${method.toUpperCase()} ${path} is missing`);
        continue;
      }
      compareOperation(operation, counterpart, `${method.toUpperCase()} ${path}`, issues);
    }
  }

  const tt02Schemas = tt02Spec.components?.schemas ?? {};
  for (const [name, schema] of Object.entries(prodSpec.components?.schemas ?? {})) {
    compareSchema(schema, tt02Schemas[name], `schema ${name}`, issues);
  }

  return issues;
}

function run() {
  let hasProductionOnlyFeatures = false;

  for (const { name, prod, tt02 } of specPairs) {
    const prodSpec = JSON.parse(readFileSync(join(publishDir, prod), "utf8"));
    const tt02Spec = JSON.parse(readFileSync(join(publishDir, tt02), "utf8"));
    const prodPaths = keysOf(prodSpec.paths);
    const tt02Paths = keysOf(tt02Spec.paths);
    const prodSchemas = keysOf(prodSpec.components?.schemas);
    const tt02Schemas = keysOf(tt02Spec.components?.schemas);
    const pathsOnlyInTt02 = missingFrom(tt02Paths, prodPaths);
    const schemasOnlyInTt02 = missingFrom(tt02Schemas, prodSchemas);
    const issues = findCompatibilityIssues(prodSpec, tt02Spec);

    console.log(`\n${name}`);
    console.log(`  paths: ${prodPaths.length} in prod, ${tt02Paths.length} in tt02`);
    console.log(`  schemas: ${prodSchemas.length} in prod, ${tt02Schemas.length} in tt02`);

    if (pathsOnlyInTt02.length > 0) {
      console.log(`  tt02-only paths (expected): ${pathsOnlyInTt02.join(", ")}`);
    }
    if (schemasOnlyInTt02.length > 0) {
      console.log(`  tt02-only schemas (expected): ${schemasOnlyInTt02.join(", ")}`);
    }

    if (issues.length > 0) {
      hasProductionOnlyFeatures = true;
      console.error(
        "  ERROR: production contains operations or structures not represented compatibly in tt02:",
      );
      issues.forEach((issue) => console.error(`    - ${issue}`));
    }
  }

  if (hasProductionOnlyFeatures) {
    console.error(
      "\nThe tt02-is-a-superset-of-production assumption no longer holds. Regenerate the affected " +
        "client from the production specification, or generate one client per environment.",
    );
    process.exitCode = 1;
    return;
  }

  console.log("\nTT02 remains a compatible superset of production.");
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  run();
}
