const { app, BrowserWindow, ipcMain } = require("electron/main");
const NamedPipes = require("named-pipes");
const net = require("net");
const path = require("node:path");

const createWindow = () => {
  const win = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
    },
  });

  win.loadFile("index.html");
};

app.whenReady().then(() => {
  ipcMain.handle("open", async () => {
    return new Promise((resolve, reject) => {
      const client = net.createConnection("\\\\.\\pipe\\base-pipe", () => {
        console.log("Connected to .NET named pipe server");

        // Send a message to the .NET server
        client.write("Hello from Electron!");

        console.log(client);
        // Receive data from the server
        client.on("data", (data) => {
          const message = data.toString();
          console.log("Received from .NET:", message);
          resolve(message); // Resolve the IPC with the received message
        });
      });

      client.on("error", (err) => {
        console.error("Error connecting to named pipe:", err);
        reject(err);
      });
    });
  });

  createWindow();
});

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") app.quit();
});
