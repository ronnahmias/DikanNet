$(document).ready(function () {
  var x = 'test';

  const hello = name => {
    return `hello ${name}`;
  };

  const log = txt => {
    console.log(txt);
  };

  log(hello(x));
});