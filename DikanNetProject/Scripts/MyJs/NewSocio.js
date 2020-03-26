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
var familyMem = {
    title: 'family_mem',
    el: '#family_mem_list',
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
    ajax_get_data('/Student/GetFamilyMem', familyMem);//get family mem data

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

    $('#AddFamily_mem').click(function () {

        console.log('add family mem');

        var id = $('#family_mem .__add_warpper input[name="family_mem_id"]').val();
        var name = $('#family_mem .__add_warpper input[name="family_mem_name"]').val();
        var relationship = $('#family_mem .__add_warpper select[name="family_mem_relationship"]').val();
        var gender = $('#family_mem .__add_warpper select[name="family_mem_gender"]').val(); 
        var birthday = $('#family_mem .__add_warpper input[name="family_mem_birthday"]').val(); 

        if (id == '' || name == '' || relationship == '' || gender == '' || birthday == '') return;

        var data = {
            SpId: SP_ID,
            FamilyMemberId: id,
            Name: name,
            Realationship: relationship,
            Gender: gender,
            BirthDay: birthday
        };

        console.log({ data });
        //return;


        var btn = $(this);

        ajax_post_data('/Student/AddEditFamilyMem', data, family_mem , btn, id);
    });

    //edit car click 
    $('#car_list').on('click', '.edit', function () {
        $('#cars .__add_warpper input[name="car_number"]').val($(this).data('car-number'));
        $('#cars .__add_warpper input[name="car_company"]').val($(this).data('company'));
        $('#cars .__add_warpper input[name="car_model"]').val($(this).data('model'));
        $('#cars .__add_warpper input[name="car_year"]').val($(this).data('year'));
        $('#AddCar').html('עדכן');
    })

    //delete car click
    $('#car_list').on('click', '.delete', function () {
        let btn = $(this);
        id = btn.attr('data-id');
        data = { CarNumber: id };
        ajax_delete_data('/Student/DeleteCar', data, car, btn, id);
    })


    //edit family_mem_list click
    $('#family_mem_list').on('click', '.edit', function () {
        $('#family_mem .__add_warpper input[name="family_mem_id"]').val($(this).data('id'));
        $('#family_mem .__add_warpper input[name="family_mem_name"]').val($(this).data('name'));
        $('#family_mem .__add_warpper select[name="family_mem_relationship"]').val($(this).data('relationship'));
        $('#family_mem .__add_warpper select[name="family_mem_gender"]').val($(this).data('gender'));
        $('#family_mem .__add_warpper input[name="family_mem_birthday"]').val($(this).data('birthday'));
        $('#AddCar').html('עדכן');
    })

    //delete family_mem_list click
    $('#family_mem_list').on('click', '.delete', function () {
        let btn = $(this);
        id = btn.attr('data-id');
        data = { CarNumber: id };
        ajax_delete_data('/Student/DeleteFamilyMem', data, familyMem, btn, id);
    })

    function clear_data(pEl) {
        switch (pEl.title) {
            case 'funding':
                $('#fund .__add_warpper input').val('');
                $('#fund .__add_warpper input[name="funding_id"]').val('0');
                break;

            case 'car':
                $('#cars .__add_warpper input').val('');
                break;

            case 'family_mem':
                $('#family_mem .__add_warpper input').val('');
               // $('#family_mem .__add_warpper input').val('');
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
            case 'family_mem':
                func_todo = get_family_mem_item;
                title_of_todo = get_family_mem_title;
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
                <div class="col d-flex flex-column">
                    <span class="font-weight-bold">פעולות</span>
                </div>
            </li>`;
        return li;
    }
    function get_car_item(item) {
        let li =
            `<li class="row d-flex flex-row mb-3" >
                <div class="col d-flex flex-column ">
                    <span class="">${item.CarNumber}</span>
                </div>
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


    function get_family_mem_title() {
        let li =
            `<li class="row d-flex flex-row mb-3">
                <div class="col d-flex flex-column ">
                    <span class="font-weight-bold">ת.ז</span>
                </div>
                <div class="col d-flex flex-column ">
                    <span class="font-weight-bold">שם מלא</span>
                </div>
                <div class="col d-flex flex-column">
                    <span class="font-weight-bold">קשר משפחתי</span>
                </div>
                <div class="col d-flex flex-column">
                    <span class="font-weight-bold">מגדר</span>
                </div>
                <div class="col d-flex flex-column">
                    <span class="font-weight-bold">תאריך לידה</span>
                </div>
            </li>`;
        return li;
    }
    function get_family_mem_item(item) {
        let li =
            `<li class="row d-flex flex-row mb-3" >
                <div class="col d-flex flex-column ">
                    <span class="">${item.FamilyMemberId}</span>
                </div>
                <div class="col d-flex flex-column ">
                    <span class="">${item.Name}</span>
                </div>
                <div class="col d-flex flex-column mx-5">
                    <span class="">${item.Realationship}</span>
                </div>
                <div class="col d-flex flex-column">
                    <span class="">${item.Gender}</span>
                </div>
                <div class="col d-flex flex-column">
                    <span class="">${item.BirthDay}</span>
                </div>
                <div class="col d-flex flex-row justify-content-around">
                    <button class="edit btn btn-warning" data-id="${item.FamilyMemberId}" data-name="${item.Name}" data-relationship="${item.Realationship}" data-gender="${item.Gender}" data-birthDay="${item.BirthDay}">ערוך</button>
                    <button class="delete btn btn-danger" data-id="${item.FamilyMemberId}">מחק</button>
                </div>
            </li>`;
        return li;
    }



});

$(document).ajaxComplete(function () {
    
});

