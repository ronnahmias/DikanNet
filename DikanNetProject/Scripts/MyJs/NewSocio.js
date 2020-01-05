$(document).ready(function () {

    console.log("new socio");
    var fund = {
        title: 'funding',
        el: '#fund_list',
        list: []
    };
    var car = {
        title: 'car',
        el: '#car_list',
        list: []
    };

    ajax_get_data('/Student/GetFundings', fund);//get funding data

    /*$.ajax({
        type: "GET",
        url: '/Student/GetFundings',
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data);
            fund.list = data.fund;
            console.log({ fund });
            procces_lists(fund);
        },
        failure: function (errMsg) { alert(errMsg); }
    });*/

    function ajax_get_data(pUrl, pElement) {
        $.ajax({
            type: "GET",
            url: pUrl,
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                console.log(data);
                pElement.list = data.data;
                console.log({ pElement });
                procces_lists(pElement);
            },
            failure: function (errMsg) { alert(errMsg); }
        });
    }

    
    
    $('#AddFund').click(function () {
        var name_fund = $('input[name="name_fund"]').val();
        var height_fund = $('input[name="height_fund"]').val();
        var year_funding = $('input[name="year_funding"]').val();
        
        var data = {
            FinancingHeight: height_fund,
            FinancingInstitution: name_fund,
            YearFinancing: year_funding
        };

        var btn = $(this);
        $.ajax({
            type: "POST",
            url: '/Student/AddFund',
            dataType: "json",
            data: data,
            beforeSend: function () {
                //console.log(btn);
                btn.attr('disabled', true); // disabel the button
            },
            success: function (data) {
                console.log(data);
                console.log("OK");
                fund.list.push(data.obj);
                console.log({ fund });
            },
            failure: function (errMsg) {
                alert(errMsg);
            },
            complete: function () {
                btn.removeAttr("disabled"); // enable the button
            }         
        });
    });

    function procces_lists(pEl) {
        console.log('procces_lists');

        let warrper = pEl.el;
        let list = pEl.list;
        let func_todo;
        switch (pEl.title) {
            case 'funding':
                func_todo = get_fund_item;
                break;
            default:
        }
        warrper = $(warrper);
        warrper.empty();
        for (let item of list) {
            console.log(item);
            warrper.append(func_todo(item));
        }
    }

    function get_fund_item(item) {
        let li = `<li>
                <span>${item.StudentId}</span>
            </li>`;
        return li;
    }
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