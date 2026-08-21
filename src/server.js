const http = require("http");
const { requestHandler } = require("./app");

const port = Number(process.env.PORT || 3000);
const server = http.createServer(requestHandler);

server.listen(port, () => {
  console.log(`backend-cocoasensorylab listening on port ${port}`);
});
