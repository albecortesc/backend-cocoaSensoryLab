const platformInfo = {
  name: "backend-cocoaSensoryLab",
  description: "Plataforma para el análisis sensorial de cacao en la Usco.",
  service: "backend",
};

function sendJson(response, statusCode, payload) {
  response.writeHead(statusCode, { "Content-Type": "application/json; charset=utf-8" });
  response.end(JSON.stringify(payload));
}

function requestHandler(request, response) {
  const { method, url } = request;

  if (method === "GET" && url === "/") {
    return sendJson(response, 200, platformInfo);
  }

  if (method === "GET" && url === "/health") {
    return sendJson(response, 200, { status: "ok" });
  }

  return sendJson(response, 404, { error: "Not Found" });
}

module.exports = {
  requestHandler,
  platformInfo,
};
