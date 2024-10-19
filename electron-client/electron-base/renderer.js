const func = async () => {
  const response = await versions.open();
  console.log(response);
};

func();
