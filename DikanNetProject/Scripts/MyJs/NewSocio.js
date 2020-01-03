$(document).ready(function () {

    console.log("new socio");
    var fund_list;

    $.ajax({
        type: "GET",
        url: '/Student/GetFundings',
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data);
            fund_list = data;
            console.log({ fund_list});
        },
        failure: function (errMsg) { alert(errMsg); }
    });

    
    
    $('#AddFund').click(function () {
        var fund = {
            FinancingInstitution: "בדיקה"
        }
        var btn = $(this);
        $.ajax({
            type: "POST",
            url: '/Student/AddFund',
            dataType: "json",
            data: fund,
            beforeSend: function () {
                console.log(btn);
                btn.attr('disabled', true); // disabel the button
            },
            success: function (data) {
                console.log(data);
                console.log("OK");
            },
            failure: function (errMsg) {
                alert(errMsg);
            },
            complete: function () {
                btn.removeAttr("disabled"); // enable the button
            }         
        });
    });
});
/*
function AJAXCALLDATA(pType, pUrl, pData) {
    $.ajax({
        type: pType,
        url: pUrl,
        dataType: "json",
        data: pData,
        success: function (data) {
            console.log(data);
            console.log("OK");
            return data;
        },
        failure: function (errMsg) { console.log(errMsg); return false; }
    });
}
*/