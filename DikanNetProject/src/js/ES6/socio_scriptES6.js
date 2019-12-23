//DOM varibels

var carviewurl = '@Url.Action("CarsView")'; // cars partial view
var fundviewurl = '@Url.Action("FundView")'; // fund partial view
var fammemviewurl = '@Url.Action("FamilyView")'; // family member partial view
var form = $('form'); // form
var carsdiv = $('#cardiv .cars'); // cars div inside all pratial rows
var funddiv = $('#Funding .funds'); // fund div inside all pratial rows
var fammemdiv = $('#FamMemdiv .fam'); // family member div inside all pratial rows

$(document).ready(function () {
    //if box open is children elment need must class
    $('.box-open').each(function () {
        var $this = $(this);
        //console.log($this);
        if ($this.css('display') != 'none') {
            $this.find(':input').each(function () {
                if (!($(this).hasClass("chosen-search-input")) && $(this).attr('type') != 'hidden')
                    $(this).addClass('must');
            });
        }
    });

    $('input.workset').each(function () {
        if ($(this).is(':checked')) {
            workSet($(this));
        }
    });
});

$('#otherMili').on('change', function () {
    /* אנחנו פונים לאחרון שבקבוצה כי הוא האחר ומשנים לו את הערך כי זה מה שנשלח לשרת
     * אני בודק שהמילה 'אחר' נמצאת אם לא אני אוסיף אותה
     */
    var e = $(this);
    var $input = $('input[name="SocioMod.MilitaryService"]').first();
    $input.prop('checked', true);
    if (!e.val().includes('אחר '))
        e.val('אחר - ' + e.val());
    $input.val(e.val());
});


$('#addCar').click(function () {
    $.get(carviewurl, function (response) {
        carsdiv.last().before(response);
        // Reparse the validator for client side validation
        //form.data('validator', null);
        //$.validator.unobtrusive.parse(form);
    });
});

$(document).on('click', '.del-car', function () {
    var container = $(this).closest('.cars');
    var id = container.find('.id').attr('value');
    if (id) {
        $.ajax({
            url: "/Student/DevareCar",
            dataType: 'text',
            data: { CarNum: id },
            method: "POST",
            success: function () {
                container.remove();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log(xhr.status);
                console.log(thrownError);
                container.remove();
            }
        });
    }
    else {
        // It never existed, so just remove the container
        container.remove();
    }
});

$('#addFund').click(function () {
    $.ajax({
        url: fundviewurl,
        dataType: 'text',
        data: null,
        method: "GET",
        success: function (response) {
            //funddiv.prepend(response);
            funddiv.last().before(response);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            console.log(xhr.status);
            console.log(thrownError);
        }
    });
    //$.get(fundviewurl, function(response) {
    //funddiv.html(response);
    // Reparse the validator for client side validation
    //form.data('validator', null);
    //$.validator.unobtrusive.parse(form);
    // });
});

$(document).on('click', '.del-fund', function () {
    var container = $(this).closest('.funds');
    var id = container.find('.id').val();
    if (id && id != 0) {
        $.ajax({
            url: "/Student/DevareFund",
            dataType: 'text',
            data: { FundId: id },
            method: "POST",
            success: function () {
                container.remove();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log(xhr.status);
                console.log(thrownError);
                container.remove();
            }
        });
    }
    else {
        // It never existed, so just remove the container
        container.remove();
    }
});

$('#addFamMem').click(function () {
    $.get(fammemviewurl, function (response) {
        fammemdiv.last().before(response);
        // Reparse the validator for client side validation
        //form.data('validator', null);
        //$.validator.unobtrusive.parse(form);
    });
});

$(document).on('click', '.del-fammem', function () {
    var container = $(this).closest('.fam');
    var id = container.find('.id').attr('value');
    if (id) {
        $.ajax({
            url: "/Student/DevareFamMem",
            dataType: 'text',
            data: { FamilyId: id },
            method: "POST",
            success: function () {
                container.remove();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log(xhr.status);
                console.log(thrownError);
                container.remove();
            }
        });
    }
    else {
        // It never existed, so just remove the container
        container.remove();
    }
});

$('input.workset').change(function () {
    workSet($(this));
});

function workSet($this) {
    var $container = $($this.attr('data-target'));
    var $children = $container.children();
    var value = $this.val();
    //console.log("This: " + $this + "Value : " + value);
    $children.first().find('.salary-entery').css('display', 'block');
    switch (value) {
        /*three box*/
        case 'שכיר':
            $children.each(function () {
                $(this).css("display", "block");
                $(this).find(':input').each(function () {
                    if (!($(this).hasClass("chosen-search-input")))
                        $(this).addClass('must');
                });
                $(this).find('span.box-title').html('תלוש שכר -');
            });
            $container.css("grid-template-columns", "repeat(3,1fr)");

            break;
        /*one box*/
        case 'עצמאי':
        case 'חבר_קיבוץ':
        case 'לא_עובד':
        case 'נכה':
        case 'נפטר':
        case 'אחר':
            $children.each(function () {
                $(this).css("display", "none");
                $(this).find(':input').each(function () {
                    $(this).removeClass('must');
                });
            });
            $children.first().css("display", "block");
            $children.first().find(':input').each(function () {
                if (!($(this).hasClass("chosen-search-input")))
                    $(this).addClass('must');
            });
            $container.css("grid-template-columns", "700px");
            switch (value) {
                case 'עצמאי':
                    $children.first().find('span.box-title').html('אישור שומת מס הכנסה לשנה קודמת - ');
                    break;
                case 'חבר_קיבוץ':
                    $children.first().find('span.box-title').html('אישור עדכני על ההכנסה לנפש בקיבוץ - ');
                    break;
                case 'לא_עובד':
                    $children.first().find('span.box-title').html('אישור עדכני מביטוח לאומי על גובה האבטלה - ');
                    break;
                case 'נכה':
                    $children.first().find('span.box-title').html('אישור מביטוח לאומי על נכות וגובה הקצבה - ');
                    break;
                case 'נפטר':
                    $children.first().find('span.box-title').html('תעודת פטירה - ');
                    $children.first().find('.salary-entery').css('display', 'none');
                    break;
                case 'אחר':
                    $children.first().find('span.box-title').html('אחר - ');
                    break;
            }
            break;
        /*tow box*/
        case 'פנסיונר':
            $children.each(function () {
                $(this).css("display", "block");
                $(this).find(':input').each(function () {
                    if (!($(this).hasClass("chosen-search-input")))
                        $(this).addClass('must');
                });
                $(this).find('span.box-title').html('תלוש שכר - ');
            });
            $children.last().css("display", "none");
            $children.last().find(':input').each(function () {
                $(this).removeClass('must');
            });
            $container.css("grid-template-columns", "repeat(2,1fr)");
            break;
        default: console.log("ברירת מחדל");
    }
    /*remove must*/
    $children.each(function () {
        $(this).find(':input').each(function () {
            $(this).removeClass('must');
        });
    });
    mustAddSign();
}