var jsWebClientPrint = (function () {
  var setA = function () {
    var e_id = "id_" + new Date().getTime();
    if (window.chrome) {
      $("body").append('<a id="' + e_id + '"></a>');
      $("#" + e_id).attr("href", "webclientprintiv:" + arguments[0]);
      var a = $("a#" + e_id)[0];
      var evObj = document.createEvent("MouseEvents");
      evObj.initEvent("click", true, true);
      a.dispatchEvent(evObj);
    } else {
      $("body").append(
        '<iframe name="' +
          e_id +
          '" id="' +
          e_id +
          '" width="1" height="1" style="visibility:hidden;position:absolute" />'
      );
      $("#" + e_id).attr("src", "webclientprintiv:" + arguments[0]);
    }
    setTimeout(function () {
      $("#" + e_id).remove();
    }, 5000);
  };
  return {
    print: function () {
      setA(
        "http://localhost:5000/api/common?clientPrint" +
          (arguments.length == 1 ? "&" + arguments[0] : "")
      );
    },
    getPrinters: function () {
      setA(
        "-getPrinters:http://localhost:5000/WebClientPrintAPI/ProcessRequest?sid=" +
          "cd524d91-9e4a-b526-b13a-73efa844e269"
      );
      var delay_ms =
        typeof wcppGetPrintersDelay_ms === "undefined"
          ? 0
          : wcppGetPrintersDelay_ms;
      if (delay_ms > 0) {
        setTimeout(function () {
          $.get(
            "http://localhost:5000/WebClientPrintAPI/ProcessRequest?getPrinters&sid=" +
              "cd524d91-9e4a-b526-b13a-73efa844e269",
            function (data) {
              if (data.length > 0) {
                wcpGetPrintersOnSuccess(data);
              } else {
                wcpGetPrintersOnFailure();
              }
            }
          );
        }, delay_ms);
      } else {
        var fncGetPrinters = setInterval(
          getClientPrinters,
          wcppGetPrintersTimeoutStep_ms
        );
        var wcpp_count = 0;
        function getClientPrinters() {
          if (wcpp_count <= wcppGetPrintersTimeout_ms) {
            $.get(
              "http://localhost:5000/WebClientPrintAPI/ProcessRequest?getPrinters&sid=" +
                "cd524d91-9e4a-b526-b13a-73efa844e269",
              { _: $.now() },
              function (data) {
                if (data.length > 0) {
                  clearInterval(fncGetPrinters);
                  wcpGetPrintersOnSuccess(data);
                }
              }
            );
            wcpp_count += wcppGetPrintersTimeoutStep_ms;
          } else {
            clearInterval(fncGetPrinters);
            wcpGetPrintersOnFailure();
          }
        }
      }
    },
    getPrintersInfo: function () {
      setA(
        "-getPrintersInfo:http://localhost:5000/WebClientPrintAPI/ProcessRequest?sid=" +
          "cd524d91-9e4a-b526-b13a-73efa844e269"
      );
      var delay_ms =
        typeof wcppGetPrintersDelay_ms === "undefined"
          ? 0
          : wcppGetPrintersDelay_ms;
      if (delay_ms > 0) {
        setTimeout(function () {
          $.get(
            "http://localhost:5000/WebClientPrintAPI/ProcessRequest?getPrintersInfo&sid=" +
              "cd524d91-9e4a-b526-b13a-73efa844e269",
            function (data) {
              if (data.length > 0) {
                wcpGetPrintersOnSuccess(data);
              } else {
                wcpGetPrintersOnFailure();
              }
            }
          );
        }, delay_ms);
      } else {
        var fncGetPrintersInfo = setInterval(
          getClientPrintersInfo,
          wcppGetPrintersTimeoutStep_ms
        );
        var wcpp_count = 0;
        function getClientPrintersInfo() {
          if (wcpp_count <= wcppGetPrintersTimeout_ms) {
            $.get(
              "http://localhost:5000/WebClientPrintAPI/ProcessRequest?getPrintersInfo&sid=" +
                "cd524d91-9e4a-b526-b13a-73efa844e269",
              { _: $.now() },
              function (data) {
                if (data.length > 0) {
                  clearInterval(fncGetPrintersInfo);
                  wcpGetPrintersOnSuccess(data);
                }
              }
            );
            wcpp_count += wcppGetPrintersTimeoutStep_ms;
          } else {
            clearInterval(fncGetPrintersInfo);
            wcpGetPrintersOnFailure();
          }
        }
      }
    },
    getWcppVer: function () {
      setA(
        "-getWcppVersion:http://localhost:5000/WebClientPrintAPI/ProcessRequest?sid=" +
          "cd524d91-9e4a-b526-b13a-73efa844e269"
      );
      var delay_ms =
        typeof wcppGetVerDelay_ms === "undefined" ? 0 : wcppGetVerDelay_ms;
      if (delay_ms > 0) {
        setTimeout(function () {
          $.get(
            "http://localhost:5000/WebClientPrintAPI/ProcessRequest?getWcppVersion&sid=" +
              "cd524d91-9e4a-b526-b13a-73efa844e269",
            function (data) {
              if (data.length > 0) {
                wcpGetWcppVerOnSuccess(data);
              } else {
                wcpGetWcppVerOnFailure();
              }
            }
          );
        }, delay_ms);
      } else {
        var fncWCPP = setInterval(getClientVer, wcppGetVerTimeoutStep_ms);
        var wcpp_count = 0;
        function getClientVer() {
          if (wcpp_count <= wcppGetVerTimeout_ms) {
            $.get(
              "http://localhost:5000/WebClientPrintAPI/ProcessRequest?getWcppVersion&sid=" +
                "cd524d91-9e4a-b526-b13a-73efa844e269",
              { _: $.now() },
              function (data) {
                if (data.length > 0) {
                  clearInterval(fncWCPP);
                  wcpGetWcppVerOnSuccess(data);
                }
              }
            );
            wcpp_count += wcppGetVerTimeoutStep_ms;
          } else {
            clearInterval(fncWCPP);
            wcpGetWcppVerOnFailure();
          }
        }
      }
    },
    send: function () {
      setA.apply(this, arguments);
    },
  };
})();
