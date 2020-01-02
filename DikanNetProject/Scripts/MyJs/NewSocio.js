$(document).ready(function () {

    console.log("new socio");
    $.ajax({
        type: "GET",
        url: '/Student/GetFundings',
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data);
        },
        failure: function (errMsg) { alert(errMsg); }
    });

    $('#AddFund').click(function () {
        var fund = {
            FinancingInstitution: "בדיקה"
        }

        $.ajax({
            type: "POST",
            url: '/Student/AddFund',
            dataType: "json",
            data: fund,
            success: function (data) {
                console.log(data);
                console.log("OK");
            },
            failure: function (errMsg) { alert(errMsg); }
        });
    });
});