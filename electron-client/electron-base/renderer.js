const main = async () => {
  const meterReadingButton = document.getElementById("meterReading");

  window.meterAPI.startMeterReading();
  window.meterAPI.onMeterReadingError(async (errorMessage) => {
    alert = document.getElementById("error-alert");
    alert.style.display = "block";
    alert.innerHTML = errorMessage;
  });

  window.meterAPI.onMeterReadingData(async (meterData) => {
    const jsonObject = JSON.parse(JSON.parse(meterData));

    alert = document.getElementById("error-alert");
    alert.style.display = "none";

    const container = document.getElementById(
      `meter-gauge-${jsonObject.SmartMeterID}`
    );

    if (jsonObject.Message.includes("Power grid outage")) {
      const overlayImage = document.createElement("img");
      overlayImage.src = "warning.jpg";
      overlayImage.id = "OutageImg";

      overlayImage.style.position = "absolute";
      overlayImage.style.top = "0";
      overlayImage.style.left = "0";
      overlayImage.style.width = "100%";
      overlayImage.style.height = "100%";
      overlayImage.style.zIndex = "10";

      container.style.position = "relative";
      container.appendChild(overlayImage);
      setTimeout(() => {
        container.removeChild(overlayImage);
      }, 60000);
    }

    const gauge_message = document.getElementById(
      `gauge-message-${jsonObject.SmartMeterID}`
    );

    gauge_message.innerHTML = "";

    message = jsonObject.Message;
    let words = message.split(" ");
    words.splice(3, 0, ":");
    words[words.length - 1] = `<strong>£${words[words.length - 1]}</strong>`;
    message = words.join(" ");
    gauge_message.innerHTML = message;

    data[0].value = jsonObject.KwhUsed;
    Plotly.react(`meter-gauge-${jsonObject.SmartMeterID}`, data, layout);
  });
};

main();
