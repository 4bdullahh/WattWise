const main = async () => {
  const meterReadingButton = document.getElementById("meterReading");
  meterReadingButton.onclick = async () => {
    const response = await versions.meterReading();
    const responseDiv = document.getElementById("responseDiv");
    responseDiv.innerHTML = response;

    function updateGaugeWithRandomValue() {
      const jsonObject = JSON.parse(JSON.parse(response));
      console.log(jsonObject.KwhUsed);
      data[0].value = jsonObject.KwhUsed;
      Plotly.react("myDiv", data, layout);
    }

    setInterval(() => {
      updateGaugeWithRandomValue();
    }, 1000);
  };
};

main();
