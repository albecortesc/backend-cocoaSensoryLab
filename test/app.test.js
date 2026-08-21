const test = require("node:test");
const assert = require("node:assert/strict");
const http = require("http");
const { requestHandler, platformInfo } = require("../src/app");

function createTestServer() {
  return new Promise((resolve) => {
    const server = http.createServer(requestHandler);
    server.listen(0, () => {
      resolve(server);
    });
  });
}

function request(server, path) {
  return new Promise((resolve, reject) => {
    const { port } = server.address();
    const req = http.request(
      {
        hostname: "127.0.0.1",
        method: "GET",
        port,
        path,
      },
      (res) => {
        let body = "";
        res.setEncoding("utf8");
        res.on("data", (chunk) => {
          body += chunk;
        });
        res.on("end", () => {
          resolve({
            statusCode: res.statusCode,
            payload: JSON.parse(body),
          });
        });
      }
    );

    req.on("error", reject);
    req.end();
  });
}

test("GET / returns platform information", async (t) => {
  const server = await createTestServer();
  t.after(() => server.close());

  const response = await request(server, "/");
  assert.equal(response.statusCode, 200);
  assert.deepEqual(response.payload, platformInfo);
});

test("GET /health returns service health", async (t) => {
  const server = await createTestServer();
  t.after(() => server.close());

  const response = await request(server, "/health");
  assert.equal(response.statusCode, 200);
  assert.deepEqual(response.payload, { status: "ok" });
});

test("unknown route returns 404", async (t) => {
  const server = await createTestServer();
  t.after(() => server.close());

  const response = await request(server, "/unknown");
  assert.equal(response.statusCode, 404);
  assert.deepEqual(response.payload, { error: "Not Found" });
});
