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
  ipcMain.on("startMeterReading", (event) => {
    console.log("TRYING TO CONNECT");

    const client = net.createConnection("\\\\.\\pipe\\meter-reading", () => {
      client.write("meterReading");

      client.on("data", (data) => {
        const message = data.toString();
        console.log("Received from .NET:", message);

        event.sender.send("meterReadingData", message);
      });

      client.on("error", (error) => {
        console.error("Error in named pipe connection:", error);
        event.sender.send("meterReadingError", error.message);
      });
    });

    client.on("end", () => {
      console.log("Connection closed by .NET server.");
    });
  });

  createWindow();
});

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") app.quit();
});
