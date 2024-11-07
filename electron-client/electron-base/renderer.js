const main = async () => {
  const meterReadingButton = document.getElementById("meterReading");

  meterReadingButton.onclick = () => {
    setInterval(async () => {
      const response = await versions.meterReading();

      const responseDiv = document.getElementById("responseDiv");
      responseDiv.innerHTML = response;

      function updateGaugeWithRandomValue(response) {
        console.log("Received response:", response);

        const jsonObject = JSON.parse(JSON.parse(response));

        data[0].value = jsonObject.KwhUsed;
        Plotly.react("myDiv", data, layout);
      }

      updateGaugeWithRandomValue(response);
    }, 1000);
  };
};

main();
