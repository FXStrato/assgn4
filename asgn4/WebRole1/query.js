$(document).ready(function () {
    ///Function to do AJAX search.
    function search() {
        var query_value = $('input#search').val();
        query_value = query_value.toLowerCase();
        $('b#search-string').text(query_value);
        $.ajax({
            type: "POST",
            url: "WebService1.asmx/oldsearch",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ str: query_value }),
            datatype: "json",
            cache: false,
            success: function (result) {
                for (var i = 0; i < result.d.length; i++) {
                    var html = "";
                    html += "<ul><li class=\"result\">";
                    html += "<h3>nameString</h3>";
                    html += "</li>";
                    html += "</ul>";
                    html = html.replace("nameString", result.d[i]);
                    result.d[i] = html;
                }
                $("ul#results").html(result.d);
            }
        });
    }
    ///Function to call search() function. Embedded with timeout to allow for user fast typing.
    ///Binded to keyup.
    $("input#search").bind("keyup", function (e) {
        // Set Timeout
        clearTimeout($.data(this, 'timer'));

        // Set Search String
        var search_string = $(this).val();

        // Do Search
        if (search_string == '') {
            $("ul#results").fadeOut();
            $('h4#results-text').fadeOut();
        } else {
            $("ul#results").fadeIn();
            $('h4#results-text').fadeIn();
            $(this).data('timer', setTimeout(search, 50));
        };
    });
});

function result() {
    
    var insert = document.getElementById("search").value;
    var second = document.getElementById("search").value;
    insert = insert.replace(" ", "+");
        $.ajax({
            crossDomain : true, 
            url: "http://ec2-54-148-15-124.us-west-2.compute.amazonaws.com/updatedcall.php?name=" + insert,
            contentType: "application/json; charset=utf-8",
            dataType: "jsonp",
            cache: true,
            success: function (result) {
                console.log(JSON.stringify(result));
                $("#player").html(result.name.toUpperCase() + " : GP =>" + result.GP + " : FGP =>" + result.FGP + " : TPP =>" + result.TPP + " : FTP =>" + result.FTP + " : PPG =>" + result.PPG + "</br>" + "<img src=\"" + result.image + "\"/>");
                //FGP' => $row['FGP'], 'TPP' => $row['TPP'], 'FTP' => $row['FTP'], 'PPG' => $row['PPG'], 'image' => $temp
            },
            error: function (result) {
                $("#player").html("");
            }
        });
        $.ajax({
            type: "POST",
            url: "WebService1.asmx/newsearch",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ prefix: second }),
            dataType: "json",
            cache: true,
            success: function (result) {
                var t = "";
                document.getElementById("results").innerHTML = "";
                for (var i = 0; i < result.d.length; i++) {
                    var s = result.d[i].split('§');
                    t += "<p class=\"display\"><a href=\"" + s[0] + "\">" + s[1] + "</a></p>" + s[0] + "</br> </br>";
                }
                $("#final").html(t);
            },
            error: function (result) {
                alert("Unable to get results back");
            }
        });
}

