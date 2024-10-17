const { app, BrowserWindow, ipcMain } = require("electron/main");
const NamedPipes = require("named-pipes");
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
  ipcMain.handle("ping", () => "pong");

  ipcMain.handle("open", async () => {
    return new Promise((resolve, reject) => {
      const pipe = NamedPipes.connect("your-pipe-name");

      pipe.on("topic", (str) => {
        console.log(str);
        resolve(str);
      });

      pipe.on("error", (err) => {
        console.error("Pipe connection error:", err);
        reject(err);
      });
    });
  });

  createWindow();
});

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") app.quit();
});
