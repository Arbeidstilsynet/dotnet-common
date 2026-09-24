import assert from "node:assert/strict";
import test from "node:test";
import { findCompatibilityIssues } from "./check-spec-drift.mjs";

const operation = (schema) => ({
  responses: {
    200: {
      content: { "application/json": { schema } },
    },
  },
});

const spec = ({ method = "get", schema = { type: "string" } } = {}) => ({
  paths: {
    "/items": {
      [method]: operation({ $ref: "#/components/schemas/Item" }),
    },
  },
  components: {
    schemas: {
      Item: {
        type: "object",
        properties: { value: schema },
      },
    },
  },
});

test("accepts TT02-only paths and schema properties", () => {
  const production = spec();
  const tt02 = spec();
  tt02.paths["/preview"] = { post: operation({ type: "string" }) };
  tt02.components.schemas.Item.properties.preview = { type: "boolean" };

  assert.deepEqual(findCompatibilityIssues(production, tt02), []);
});

test("reports a missing production operation with its method and path", () => {
  const production = spec({ method: "post" });
  const tt02 = spec({ method: "get" });

  assert.deepEqual(findCompatibilityIssues(production, tt02), [
    "operation POST /items is missing",
  ]);
});

test("reports nested missing properties and incompatible types", () => {
  const production = spec({
    schema: {
      type: "object",
      properties: {
        nested: { type: "array", items: { type: "integer", format: "int32" } },
        absent: { type: "string" },
      },
    },
  });
  const tt02 = spec({
    schema: {
      type: "object",
      properties: {
        nested: { type: "array", items: { type: "string" } },
      },
    },
  });

  assert.deepEqual(findCompatibilityIssues(production, tt02), [
    "schema Item.value.nested[] has type integer in production, but string in TT02",
    "schema Item.value.nested[] has format int32 in production, but none in TT02",
    "schema Item.value.absent is missing",
  ]);
});
