const { contextBridge, ipcRenderer } = require("electron");

contextBridge.exposeInMainWorld("meterAPI", {
  startMeterReading: () => ipcRenderer.send("startMeterReading"),
  onMeterReadingData: (callback) =>
    ipcRenderer.on("meterReadingData", (event, data) => callback(data)),
  onMeterReadingError: (callback) =>
    ipcRenderer.on("meterReadingError", (event, error) => callback(error)),
});

// contextBridge.exposeInMainWorld("versions", {
//   node: () => process.versions.node,
//   chrome: () => process.versions.chrome,
//   electron: () => process.versions.electron,
//   // ping: () => ipcRenderer.invoke("ping"),
//   // open: () => ipcRenderer.invoke("open"),
//   meterReading: () => ipcRenderer.invoke("meterReading"),
// });
