$(document).ready(function () {

    const SP_ID = $('#spid').val();

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
        var name_fund = $('#fund .__add_warpper input[name="name_fund"]').val();
        var height_fund = $('#fund .__add_warpper input[name="height_fund"]').val();
        var year_funding = $('#fund .__add_warpper input[name="year_funding"]').val();
        var funding_id = $('#fund .__add_warpper input[name="funding_id"]').val();

        if (funding_id === undefined || funding_id === null) funding_id = 0;

        if (name_fund == '' || height_fund <= 0 || year_funding < 2010) return;

        var data = {
            SpId: SP_ID,
            FundingId: funding_id,
            FinancingHeight: height_fund,
            FinancingInstitution: name_fund,
            YearFinancing: year_funding
        };

        var btn = $(this);
        $.ajax({
            type: "POST",
            url: '/Student/AddEditFund',
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

                procces_lists(fund);
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
        let li =
            `<li class="d-flex flex-row mb-3" >
                <input type="hidden" name="fund_id" value="${item.FundingId}" />
                <div class="d-flex flex-column ">
                    <span class="font-weight-bold">גוף ממן</span>
                    <span class="">${item.FinancingInstitution}</span>
                </div>
                <div class="d-flex flex-column mx-5">
                    <span class="font-weight-bold">סכום מימון</span>
                    <span class="">${item.FinancingHeight}</span>
                </div>
                <div class="d-flex flex-column">
                    <span class="font-weight-bold">שנה</span>
                    <span class="">${item.YearFinancing}</span>
                </div>
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