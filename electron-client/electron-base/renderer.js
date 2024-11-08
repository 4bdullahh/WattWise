const main = async () => {
  const meterReadingButton = document.getElementById("meterReading");

  meterReadingButton.onclick = async () => {
    window.meterAPI.startMeterReading();

    window.meterAPI.onMeterReadingData((meterData) => {
      const jsonObject = JSON.parse(JSON.parse(meterData));

      console.log("Meter Reading Data:", jsonObject);

      data[0].value = jsonObject.KwhUsed;
      Plotly.react("meterGauge", data, layout);
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
