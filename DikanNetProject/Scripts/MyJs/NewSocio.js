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

const removeArrayItem = (arr, itemToRemove, itemToRemoveData) => {
    return arr.filter(item => item[itemToRemove] !== +itemToRemoveData)
}

const SP_ID = $('#spid').val();



$(document).ready(function () {


    console.log("new socio");

    ajax_get_data('/Student/GetFundings?SpId=' + SP_ID, fund);//get funding data

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

    //edit fundraiser click 
    $('#fund_list').on('click', '.edit', function () {
        $('#fund .__add_warpper input[name="name_fund"]').val($(this).attr('data-name'));
        $('#fund .__add_warpper input[name="height_fund"]').val($(this).attr('data-height'));
        $('#fund .__add_warpper input[name="year_funding"]').val($(this).attr('data-year'));
        $('#fund .__add_warpper input[name="funding_id"]').val($(this).attr('data-id'));
        $('#AddFund').html('עדכן');
    })

    //delete fundraiser click$
    $('#fund_list').on('click', '.delete', function () {
        let btn = $(this);
        data = btn.attr('data-id');
        $.ajax({
            type: "POST",
            url: '/Student/DeleteFund',
            dataType: "text",
            data: { FundId: data },
            beforeSend: function () {
                btn.attr('disabled', true); // disabel the button
            },
            success: function (res) {
                console.log(data);
                console.log("DELETE OK");
                fund.list = removeArrayItem(fund.list, 'FundingId', data);
                procces_lists(fund);
            },
            failure: function (errMsg) {
                alert(errMsg);
            },
            complete: function () {
                btn.removeAttr("disabled"); // enable the button
            }
        });
    })

   
    
    
    $('#AddFund').click(function () {

        console.log('add funding');

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

        var success = false;
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
                if (funding_id > 0) {
                    fund.list = removeArrayItem(fund.list, 'FundingId', funding_id);
                };
                //console.log(data);
                console.log("OK");
                fund.list.push(data.obj);
                console.log({ fund });

                procces_lists(fund);
                clear_data(fund);
            },
            failure: function (errMsg) {
                alert(errMsg);
            },
            complete: function () {
                btn.html('הוסף');
                btn.removeAttr("disabled"); // enable the button
            }         
        });        
    });

    function clear_data(pEl) {
        switch (pEl.title) {
            case 'funding':
                $('#fund .__add_warpper input[name="name_fund"]').val('');
                $('#fund .__add_warpper input[name="height_fund"]').val('');
                $('#fund .__add_warpper input[name="year_funding"]').val('');
                $('#fund .__add_warpper input[name="funding_id"]').val('0');
                break;
            default:
        }
    }

    function procces_lists(pEl) {
        //console.log('procces_lists');

        let warrper = pEl.el;
        let list = pEl.list;
        let func_todo;
        let title_of_todo;
        switch (pEl.title) {
            case 'funding':
                func_todo = get_fund_item;
                title_of_todo = get_fund_title;
                break;
            default:
        }
        warrper = $(warrper);
        warrper.empty();
        warrper.append(title_of_todo);
        for (let item of list) {
            warrper.append(func_todo(item));
        }
    }

    function get_fund_item(item) {
        let li =
            `<li class="row d-flex flex-row mb-3" >
                <input type="hidden" name="fund_id" value="${item.FundingId}" />
                <div class="col d-flex flex-column ">
                    <span class="">${item.FinancingInstitution}</span>
                </div>
                <div class="col d-flex flex-column mx-5">
                    <span class="">${item.FinancingHeight}</span>
                </div>
                <div class="col d-flex flex-column">
                    <span class="">${item.YearFinancing}</span>
                </div>
                <div class="col d-flex flex-row justify-content-around">
                    <button class="edit btn btn-warning" data-id="${item.FundingId}" data-name="${item.FinancingInstitution}" data-height="${item.FinancingHeight}" data-year="${item.YearFinancing}">ערוך</button>
                    <button class="delete btn btn-danger" data-id="${item.FundingId}">מחק</button>
                </div>
            </li>`;
        return li;
    }
    function get_fund_title() {
        let li =
            `<li class="row d-flex flex-row mb-3" >
                <div class="col d-flex flex-column ">
                    <span class="font-weight-bold">גוף ממן</span>
                </div>
                <div class="col d-flex flex-column mx-5">
                    <span class="font-weight-bold">סכום מימון</span>
                </div>
                <div class="col d-flex flex-column">
                    <span class="font-weight-bold">שנה</span>
                </div>
                <div class="col d-flex flex-column">
                    <span class="font-weight-bold">פעולות</span>
                </div>
            </li>`;
        return li;
    }



});

$(document).ajaxComplete(function () {
    
});

