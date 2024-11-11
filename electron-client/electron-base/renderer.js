const main = async () => {
  const meterReadingButton = document.getElementById("meterReading");

  meterReadingButton.onclick = async () => {
    window.meterAPI.startMeterReading();

    window.meterAPI.onMeterReadingData(async (meterData) => {
      const jsonObject = JSON.parse(JSON.parse(meterData));

      // console.log("Meter Reading Data:", jsonObject);

      data[0].value = jsonObject.KwhUsed;
      Plotly.react(`meter-gauge-${jsonObject.SmartMeterID}`, data, layout);

      // setInterval(() => {
      //   console.log("Waiting 1s");
      // }, 1000);
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
