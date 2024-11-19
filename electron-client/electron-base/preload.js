const { contextBridge, ipcRenderer } = require("electron");

contextBridge.exposeInMainWorld("meterAPI", {
  startMeterReading: () => ipcRenderer.send("startMeterReading"),
  onMeterReadingData: (callback) =>
    ipcRenderer.on("meterReadingData", (event, data) => callback(data)),
  onMeterReadingError: (callback) =>
    ipcRenderer.on("meterReadingError", (event, error) => callback(error)),
});

