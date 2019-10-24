/*************
Version: 2.1;

**************/

$(document).ready(function () {

    var $passInput;
    //review password function on hover
    $('.review-password').hover(function () {
        //console.log('hover in');
        $passInput = $(this).parent().find('input[type="password"]');
        $passInput.attr('type', 'text');
        console.log($passInput);
    },function () {
        $passInput.attr('type', 'password');
    });

    var carcount = $("#carstdcount").val();

    $("#addCar").click(function () {
        $("#carsdiv").append("<input class='control-label col-md-2 text-box single-line' data-val='true' data-val-number='The field CarNumber must be a number.' data-val-required='השדה CarNumber נדרש.' id='CarStudent1_" + carcount + "__CarNumber' name='CarStudent1[" + carcount + "].CarNumber' type='number' >");
        $("#carsdiv").append("<input class='control-label col-md-2 text-box single-line' data-val='true' data-val-number='The field CarNumber must be a number.' data-val-required='השדה CarNumber נדרש.' id='CarStudent1_" + carcount + "__CarCompany' name='CarStudent1[" + carcount + "].CarCompany' type='number' >");
        carcount++;
    });
    docReadyAndAjax();
});

// Function after ajax calls
$(document).ajaxComplete(function () {
    docReadyAndAjax();
    //clear-date;
    $('.clear-date').click(function () {
        $(this).parent().find('input[data-toggle="datepicker"]').val('');
    });
});


function docReadyAndAjax(){
    pophover();
    onlyNumbers();
    onlyHeb();
    chosen();
    //datepicker();
    choseFile();
    onChangeIdValid();
    onChangeEmailValid();
    mustAddSign();
    //watchSaveFile();
}

// pophover init
function pophover() {
    $("[data-toggle='popover']").popover();
    $("[data-toggle='popover']").attr('data-html', 'true');
}

function onChangeIdValid() {
    $('input.id').change(function () {
        var $this = $(this);
        if (validId($this.val()))
            $this.removeClass('border-danger');
        else
            $this.addClass('border-danger');
    });
}

function onChangeEmailValid() {
    $('input.email').change(function () {
        var $this = $(this);
        if (validEmail($this.val()))
            $this.removeClass('border-danger');
        else
            $this.addClass('border-danger');
    });
}
/*
function watchSaveFile() {
    $('.btn-file').click(function () {
        $('#fileModal').modal('show');
        var pathfile = $(this).attr('name');
        var url = '@Url.Action("GetFile","File")' + '?pFilePath=' + pathfile;
        var type = pathfile.split('.').pop();
        //console.log(type);
        if (type == "pdf")
            type = "application/pdf";
        else
            type = "image/" + type;
        $('.embed-file').attr('type', type);
        $('.embed-file').attr('src', url);
    });
}
*/
// Function for date picker
function datepicker() {
    $('[data-toggle="datepicker"]').attr('readonly', true);
    $('[data-toggle="datepicker"]').datepicker({
        language: 'he-HE',
        format: 'dd/mm/yyyy',
    });
    $('[data-toggle="datepicker"]').datepicker('setStartDate', '01/01/1980');
}

$('.clear-date').click(function () {
   $(this).parent().find('input[data-toggle="datepicker"]').val('');
});

// Function for dropdown lists to chose
function chosen() {
    $(".chosen").chosen(
        {
            no_results_text: "אין ערך כזה.",
            rtl: true,
            allow_single_deselect: true,
            width: '100%'
        });
}

//add attribute onkeypress inly numbers allow
function onlyNumbers(){
    $('.only-numbers').attr('onkeypress', 'return event.charCode >= 48 && event.charCode <=57');
    $('input.id').attr('onkeypress', 'return event.charCode >= 48 && event.charCode <=57');
}

//add attribute onkeypress inly numbers allow
function onlyHeb() {
    $('.only-heb').attr('onkeypress', 'return (event.charCode >= 0x590 && event.charCode <=0x5FF) || event.charCode == 32');
}
// chose file change the style of button
function choseFile() {
    $('input[type="file"]').change(function () {
        console.log("chose file");
        var $this = $(this),
            filepath = $this.val(),
            group = filepath.split("\\"),
            fileName = group.pop(),
            $label = $('label[for="' + $this.attr('id') + '"]')[0],
            $father = $this.parent();

        if (fileName.length == 0) {
            $label.append('<i class="ml-1 material-icons">add_photo_alternate</i>');
            $label.textContent = 'בחר קובץ';
            $father.removeClass("file-selcted");
        }
        else {
            $label.textContent = fileName;
            $father.addClass("file-selcted");
        }
    });
}

// validtion for ID
function validId(id) {
    var tot = 0;
    var tz = new String(id);
    for (i = 0; i < 8; i++) {
        x = (((i % 2) + 1) * tz.charAt(i));
        if (x > 9) {
            x = x.toString();
            x = parseInt(x.charAt(0)) + parseInt(x.charAt(1))
        }
        tot += x;
    }
    return ((tot + parseInt(tz.charAt(8))) % 10 == 0);
}


function validPassword(p)
{
    /* validtion for Password
     * Password must contain:
     *      Minimum 8 characters
     *      A lowercase letter
     *      A capital(uppercase)letter
     *      A number
   */
    //minimum characters
    if (p.length < 8) return false;
    //check with regex
    var lowerCase = /[a-z]/g;
    var upperCase = /[A-Z]/g;
    var numbers = /[0-9]/g;
    return (p.match(lowerCase) && p.match(upperCase) && p.match(numbers))?true:false;

}

//validtion for email
function validEmail(email) {
    var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}


function showHidenPortion(checked, idElement) {
/* The function get the checked value(true or false)
* and the elemnt that need to be show
* if checked true it will fade in else it will fade out
*/
    if (checked == true) {
        $('#' + idElement).fadeIn();
        $('#' + idElement + ' :input').each(function () {
            $(this).addClass('must');
            //specficMustSigh($(this));
        });
    }
    else {
        $('#' + idElement).fadeOut();
        $('#' + idElement + ' :input').each(function () {
            $(this).removeClass('must');
        });
    }
}

/* The function check that all the must filed are filed */
function checkMust() {
    var ok = true;
    var HebrewChars = new RegExp("^[\u0590-\u05FF ]*$");
    $('.must').each(function () {
        var $this = $(this);
        var type = $this.attr('type');
        var $singleChosen = $this.parent().find('.chosen-single'); //for chosen drop down.
        $this.removeClass('border-danger');
        $singleChosen.removeClass('border-danger');
        var txt = $this.val();

        //console.log("This: " + type);
        //console.log("Text: " + txt);
        //console.log("OK: " + ok);
        // if its only letters on hebrew
        if ($this.hasClass('only-heb')) {
            //console.log("only hebrew");
            if (!HebrewChars.test(txt) || txt == null || txt.length == 0) {
                ok = false;
                $this.addClass('border-danger');
            }
        }
        else if (type == "file") {
            //console.log("File");
            if (!checkMustFile($this))
                ok = false;
        }
        else if (type == "email")
        {
            //console.log("check email");
            if (!(validEmail(txt))){
                ok = false;
                $this.addClass('border-danger');
            }
        }
        else {
            //console.log("Else");
            if (txt.length == 0 || txt == null) {
                ok = false;
                $this.addClass('border-danger');
                $singleChosen.addClass('border-danger');
            }
        }

    });

    return ok;
}

function checkMustFile($file) {
    ok = true;
    $fileDiv = $file.parent();
    file = $file.val();
    path = $file.attr('path');
    if (path == null)
        path = "";
    if (file.length <= 0 && path.length <= 0) {
        $fileDiv.addClass('bg-red');
        ok = false;
    }
    else
        $fileDiv.removeClass('bg-red');
    
    return ok;
}

function validUpdateStu() {
    //valid all inputs
    var ok = checkMust();
    console.log('must: ' + ok);
    var $btDay = $('#BirthDay');
    //valid date
    if (!dateIsValid($btDay.val(), 1930)) {
        $btDay.addClass('border-danger');
        ok = false;
    }
    else {
        $btDay.removeClass('border-danger');
    }
    console.log('vtd: ' + ok);
    if (ok == false) {
        $('#errorModal').modal('show');
    }
    return ok;
}

/* The function check that date is valid */
function dateIsValid(pDate, pMinYear) {
    console.log('checkDate');
    console.log('Date: ' + pDate);
    var ok = true;
    var c = pDate.split('/');
    console.log('C: ' + c[2]);
    var datet = new Date(pDate);
    //console.log('he: '+ datet.toLocaleDateString("he-IL").split(',')[0]);
    //console.log('us: '+ datet.toLocaleDateString("en-US").split(',')[0]);
    //return false;
    // check year valid
    if (c[2] < pMinYear) {
        ok = false;
    }
    //date = new Date(c[2],c[1],c[0]); //new Date(yyyy, mm, dd);
    if (datet.toString() == "Invalid Date") {
        ok = false;
    }

    return ok;
}

function validPasswordsAndEmail() {
    var email = $('#Email').val();
    return checkPasswordsValid() && validEmail(email);
}

function checkPasswordsValid() // check that password and confirm pass is match in register and resetpass views
{
    $('#ValidMessagePassword').text = "";
    $('input[type=password]').removeClass("border border-danger");

    var pass1 = $('#Password').val();
    var pass2 = $('#ConfirmPassword').val();
    if (pass1 != pass2 || !validPassword(pass1)) {
        $('#ValidMessagePassword').text = "שגיאה, הסיסמא לא תקנית";
        $('input[type=password]').addClass("border border-danger");
        $('#OldPassword').removeClass("border border-danger");
        return false;
    }
    return true;
}

// add sign to must labels
function mustAddSign() {
    $('.must').each(function () {

        if ($(this).attr('path') == null)
            $(this).parent().parent().find('label').addClass('must-sign');
    });
}
/*
// add sign to must labels
function specficMustSigh(e) {
    console.log(e.attr('path'));
    console.log(e.attr('path') == null || e.attr('path') == 'undefined');
    if (e.attr('path') == null || e.attr('path') == 'undefined')
        e.parent().parent().find('label').addClass('must-sign');
}*/