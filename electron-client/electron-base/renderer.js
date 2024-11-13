const main = async () => {
  const meterReadingButton = document.getElementById("meterReading");

  meterReadingButton.onclick = async () => {
    window.meterAPI.startMeterReading();

    window.meterAPI.onMeterReadingData(async (meterData) => {
      const jsonObject = JSON.parse(JSON.parse(meterData));

      // console.log("Meter Reading Data:", jsonObject);

      if (jsonObject.Message.includes("Power grid outage")) {
        console.log("OUTAGE: " + jsonObject);
        const container = document.getElementById(
          `meter-gauge-${jsonObject.SmartMeterID}`
        );
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
        }, 2500);
      }

      data[0].value = jsonObject.KwhUsed;
      Plotly.react(`meter-gauge-${jsonObject.SmartMeterID}`, data, layout);
    });

    // const responseDiv = document.getElementById("responseDiv");
    // responseDiv.innerHTML = response;

    // async function updateGaugeWithRandomValue() {
    //   const response = await versions.meterReading();
    //   console.log(response);
    //   const jsonObject = JSON.parse(JSON.parse(response));
    //   data[0].value = jsonObject.KwhUsed;
    //   Plotly.react("meterGauge", data, layout);
    // }

    // setInterval(() => {
    //   updateGaugeWithRandomValue();
    // }, 1000);
  };
};

main();
