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
      nodeIntegration: true,
    },
  });

  win.loadFile("index.html");
};

app.whenReady().then(() => {
  ipcMain.handle("meterReading", async () => {
    console.log("TRYING TO ONNCET");

    return new Promise((resolve, reject) => {
      const client = net.createConnection("\\\\.\\pipe\\meter-reading", () => {
        client.write("meterReading");

        client.on("data", (data) => {
          const message = data.toString();
          // console.log("Received from .NET:", message);
          resolve(message);
        });
      });

      client.on("error", (err) => {
        console.error("Error connecting to named pipe:", err);
        reject(err);
      });
    });
  });

  /* --------------------------------------------- */
  // ipcMain.handle("open", async () => {
  //   return new Promise((resolve, reject) => {
  //     const client = net.createConnection("\\\\.\\pipe\\base-pipe", () => {
  //       console.log("Connected to .NET named pipe server");

  //       client.write("Hello from Electron!");

  //       client.on("data", (data) => {
  //         const message = data.toString();
  //         console.log("Received from .NET:", message);
  //         resolve(message);
  //       });
  //     });

  //     client.on("error", (err) => {
  //       console.error("Error connecting to named pipe:", err);
  //       reject(err);
  //     });
  //   });
  // });

  createWindow();
});

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") app.quit();
});
