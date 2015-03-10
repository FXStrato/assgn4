var system = setInterval(function () { cpuram() }, 2000);
var spiders = setInterval(function () { crawlerStatus() }, 2000);
var stats = setInterval(function () { getStats() }, 2000);

window.onload = function () {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/initstart",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            document.getElementById("totalurls").innerHTML = msg.d[0];
            document.getElementById("urlten").innerHTML = msg.d[1];
            document.getElementById("queuesize").innerHTML = msg.d[2];
            document.getElementById("tablesize").innerHTML = msg.d[3];
            document.getElementById("errorten").innerHTML = "";
        },
        error: function (msg) {
            alert(msg.d);
        }
    });
    $("#clearqueue").show();
    $("#cleartable").show();
    $("#stopbutton").hide();
    $("#initbutton").click(function () {
        $("#initbutton").hide();
        $("#stopbutton").show();
        $("#clearqueue").hide();
        $("#cleartable").hide();
    });

    $("#stopbutton").click(function () {
        $("#stopbutton").hide();
        $("#clearqueue").show();
        $("#initbutton").show();
        $("#cleartable").show();
    });
}

function cpuram() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/cpuram",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            document.getElementById("cpuram").innerHTML = msg.d;
        },
        error: function (msg) {
            document.getElementById("cpuram").innerHTML = "Error in obtaining CPU and RAM data";
        }
    });
}

function crawlerStatus() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/CrawlerStatus",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            document.getElementById("crawlerstatus").innerHTML = msg.d;
        },
        error: function (msg) {
            document.getElementById("crawlerstatus").innerHTML = "Error in obtaining crawler status";
        }
    });
}

function initialize() {
    var crawler = document.getElementById("url1").value;
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/initializeSpider",
        data: JSON.stringify({ url: crawler }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            document.getElementById("crawlerstatus").innerHTML = msg.d;
        },
        error: function (msg) {
            document.getElementById("crawlerstatus").innerHTML = "Error in initializing crawler";
        }
    });
}

function end() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/endSpider",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            document.getElementById("crawlerstatus").innerHTML = msg.d;
        },
        error: function (msg) {
            document.getElementById("crawlerstatus").innerHTML = "Error in obtaining crawler status";
        }
    });
}


function getStats() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/getStats",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            document.getElementById("totalurls").innerHTML = msg.d[0];
            document.getElementById("urlten").innerHTML = msg.d[1];
            document.getElementById("queuesize").innerHTML = msg.d[2];
            document.getElementById("tablesize").innerHTML = msg.d[3];
            document.getElementById("errorten").innerHTML = msg.d[4];
            document.getElementById("titlecount").innerHTML = msg.d[5];
            document.getElementById("lasttitle").innerHTML = msg.d[6];

        },
        error: function (msg) {
            document.getElementById("errorten").innerHTML = "Error in obtaining stats";
        }
    });
}

function clearqueue() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/clearqueue",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            document.getElementById("queuesize").innerHTML = msg.d;
        },
        error: function (msg) {
            document.getElementById("test").innerHTML = "Error";
        }
    });
}

function getFromTable() {
    var crawler = document.getElementById("indextable").value;
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/GetFromTable",
        data: JSON.stringify({ url: crawler }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            if (msg.d[1] != null) {
                var i = "";
                i += msg.d[0] + "|" + msg.d[1] + "|" + msg.d[2];
                document.getElementById("indexable").innerHTML = i;
            } else {
                document.getElementById("indexable").innerHTML = "Either incorrect search string, or url not found in table.";
            }
        },
        error: function (msg) {
            document.getElementById("indexable").innerHTML = "Error";
        }
    });
}
