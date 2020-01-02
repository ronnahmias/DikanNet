jQuery(document).ready(function () {


    jQuery.ajax({
        type: "GET",
        url: "/Student/GetlFundings",
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var o = new Object();
            o = data;
            console.log(o);
            alert(data);
        },
        failure: function (errMsg) { alert(errMsg); }
    });
});
