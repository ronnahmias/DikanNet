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

//the function remove item from array
const removeArrayItem = (arr, itemToRemove, itemToRemoveData) => {
    return arr.filter(item => item[itemToRemove] !== itemToRemoveData)
}

// the function procces whitch element need to be deleted
const removeArrayItemProcces = (handler, id) => {
    console.log('removeArrayItemProcces');
    switch (handler.title) {
        case 'funding':
            handler.list = removeArrayItem(handler.list, 'FundingId', +id);
            break;
         case 'car':
            handler.list = removeArrayItem(handler.list, 'CarNumber', id);
             break;
        default:
    }
}

const SP_ID = $('#spid').val();



$(document).ready(function () {


    console.log("new socio");

    ajax_get_data('/Student/GetFundings?SpId=' + SP_ID, fund);//get funding data
    ajax_get_data('/Student/GetCars?SpId=' + SP_ID, car);//get car data

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

    function ajax_post_data(pUrl,pData, pElement, pBtn, pId) {
        $.ajax({
            type: "POST",
            url: pUrl,
            dataType: "json",
            data: pData,
            beforeSend: function () {
                pBtn.attr('disabled', true); // disabel the button
            },
            success: function (data) {
                if (pId > 0) {
                    removeArrayItemProcces(pElement, pId);
                };
                //console.log(data);
                console.log("OK");
                pElement.list.push(data.obj);
                procces_lists(pElement);
                clear_data(pElement);
            },
            failure: function (errMsg) {
                console.log(errMsg);
            },
            complete: function () {
                pBtn.html('הוסף');
                pBtn.removeAttr("disabled"); // enable the button
            }
        });        
    }

    function ajax_delete_data(pUrl, pData, pElement, pBtn, pId) {
        $.ajax({
            type: "POST",
            url: pUrl,
            dataType: "text",
            data: pData,
            beforeSend: function () {
                pBtn.attr('disabled', true); // disabel the button
            },
            success: function (data) {
                //console.log('delete success');
                removeArrayItemProcces(pElement, pId);
                procces_lists(pElement);
            },
            failure: function (errMsg) {
                console.log(errMsg);
            },
            complete: function () {
                pBtn.removeAttr("disabled"); // enable the button
            }
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

    //delete fundraiser click
    $('#fund_list').on('click', '.delete', function () {
        let btn = $(this);
        id = btn.attr('data-id');
        data = { FundId: id };
        ajax_delete_data('/Student/DeleteFund', data, fund, btn, id);       
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
        ajax_post_data('/Student/AddEditFund', data, fund, btn, funding_id);       
    });

    $('#AddCar').click(function () {

        console.log('add car');

        var car_number = $('#cars .__add_warpper input[name="car_number"]').val();
        var car_company = $('#cars .__add_warpper input[name="car_company"]').val();
        var car_model = $('#cars .__add_warpper input[name="car_model"]').val();
        var car_year = $('#cars .__add_warpper input[name="car_year"]').val();

        if (car_number == '' || car_company == '' || car_model == '' || car_year == '') return;

        var data = {
            SpId: SP_ID,
            CarNumber: car_number,
            CarCompany: car_company,
            CarModel: car_model,
            CarYear: car_year
        };


        var btn = $(this);

        ajax_post_data('/Student/AddEditCar', data, car, btn, car_number);
    });

    //edit car click 
    $('#car_list').on('click', '.edit', function () {
        $('#cars .__add_warpper input[name="car_number"]').val($(this).data('car-number'));
        $('#cars .__add_warpper input[name="car_company"]').val($(this).data('company'));
        $('#cars .__add_warpper input[name="car_model"]').val($(this).data('model'));
        $('#cars .__add_warpper input[name="car_year"]').val($(this).data('year'));
        $('#AddCar').html('עדכן');
    })

    //delete fundraiser click
    $('#car_list').on('click', '.delete', function () {
        let btn = $(this);
        id = btn.attr('data-id');
        data = { CarNumber: id };
        ajax_delete_data('/Student/DeleteCar', data, car, btn, id);
    })

    function clear_data(pEl) {
        switch (pEl.title) {
            case 'funding':
                $('#fund .__add_warpper input[name="name_fund"]').val('');
                $('#fund .__add_warpper input[name="height_fund"]').val('');
                $('#fund .__add_warpper input[name="year_funding"]').val('');
                $('#fund .__add_warpper input[name="funding_id"]').val('0');
                break;

            case 'car':
                $('#cars .__add_warpper input[name="car_number"]').val('');
                $('#cars .__add_warpper input[name="car_company"]').val('');
                $('#cars .__add_warpper input[name="car_model"]').val('');
                $('#cars .__add_warpper input[name="car_year"]').val('');
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
            case 'car':
                func_todo = get_car_item;
                title_of_todo = get_car_title;
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

    function get_car_title() {
        let li =
            `<li class="row d-flex flex-row mb-3" >
                <div class="col d-flex flex-column ">
                    <span class="font-weight-bold">מספר רכב</span>
                </div>
                <div class="col d-flex flex-column mx-5">
                    <span class="font-weight-bold">יצרן רכב</span>
                </div>
                <div class="col d-flex flex-column">
                    <span class="font-weight-bold">דגם רכב</span>
                </div>
                <div class="col d-flex flex-column">
                    <span class="font-weight-bold">שנת ייצור</span>
                </div>
            </li>`;
        return li;
    }
    function get_car_item(item) {
        let li =
            `<li class="row d-flex flex-row mb-3" >
                <input type="hidden" name="fund_id" value="${item.CarNumber}" />
                <div class="col d-flex flex-column ">
                    <span class="">${item.CarCompany}</span>
                </div>
                <div class="col d-flex flex-column mx-5">
                    <span class="">${item.CarModel}</span>
                </div>
                <div class="col d-flex flex-column">
                    <span class="">${item.CarYear}</span>
                </div>
                <div class="col d-flex flex-row justify-content-around">
                    <button class="edit btn btn-warning" data-car-number="${item.CarNumber}" data-company="${item.CarCompany}" data-model="${item.CarModel}" data-year="${item.CarYear}">ערוך</button>
                    <button class="delete btn btn-danger" data-id="${item.CarNumber}">מחק</button>
                </div>
            </li>`;
        return li;
    }



});

$(document).ajaxComplete(function () {
    
});

