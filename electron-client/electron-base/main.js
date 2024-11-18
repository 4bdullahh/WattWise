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

    const pipeName = "\\\\.\\pipe\\meter-reading";
    let retryAttempts = 0;
    const maxRetries = 100;
    const retryDelay = 2000;

    function connectToPipe() {
      try {
        const client = net.createConnection(pipeName, () => {
          console.log("Connected to the pipe!");

          client.write("meterReading");

          client.on("data", (data) => {
            const message = data.toString();
            console.log("Received from .NET:", message);

            event.sender.send("meterReadingData", message);
          });

          client.on("error", (error) => {
            console.error("Error in named pipe connection:", error.message);
            event.sender.send("meterReadingError", error.message);

            retryConnection();
          });

          client.on("close", () => {
            console.log("Connection closed.");
            retryConnection();
          });
        });

        client.on("error", (error) => {
          console.error("Connection error:", error.message);
          event.sender.send("meterReadingError", error.message);
          retryConnection();
        });
      } catch (error) {
        console.error("An unexpected error occurred:", error.message);
        event.sender.send("meterReadingError", error.message);

        retryConnection();
      }
    }

    function retryConnection() {
      if (retryAttempts < maxRetries) {
        retryAttempts++;
        console.log(
          `Retrying connection (${retryAttempts}/${maxRetries}) in ${
            retryDelay / 1000
          } seconds...`
        );

        setTimeout(() => {
          connectToPipe();
        }, retryDelay);
      } else {
        console.error("Max retries reached. Failed to connect to the pipe.");
        event.sender.send(
          "meterReadingError",
          "Failed to connect to the pipe after multiple attempts."
        );
      }
    }

    connectToPipe();
  });

  createWindow();
});

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") app.quit();
});
