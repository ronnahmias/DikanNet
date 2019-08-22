/*************
Version: 1.1;
Last Update: 26/7/19



**************/

$(document).ready(function () {

    //review password function on hover
    $('.review-password').hover(function () {
        $('input#Password').attr('type', 'text');
    },
    function () {
        $('input#Password').attr('type', 'password');
        });

    //review password function on hover
    $('.review-confirm-password').hover(function () {
        $('input#ConfirmPassword').attr('type', 'text');
    },
    function () {
        $('input#ConfirmPassword').attr('type', 'password');
    });

     // added by ron ---
    var carcount = $("#carstdcount").val();

    $("#addCar").click(function () {
        $("#carsdiv").append("<input class='control-label col-md-2 text-box single-line' data-val='true' data-val-number='The field CarNumber must be a number.' data-val-required='השדה CarNumber נדרש.' id='CarStudent1_" + carcount + "__CarNumber' name='CarStudent1[" + carcount + "].CarNumber' type='number' >");
        $("#carsdiv").append("<input class='control-label col-md-2 text-box single-line' data-val='true' data-val-number='The field CarNumber must be a number.' data-val-required='השדה CarNumber נדרש.' id='CarStudent1_" + carcount + "__CarCompany' name='CarStudent1[" + carcount + "].CarCompany' type='number' >");

        carcount++;
    });

    onlyNumbers();
    chosen();
    datepicker();
    choseFile();
    
});

// Function agter ajax calls
$(document).ajaxComplete(function () {
    onlyNumbers();
    chosen();
    datepicker();
    choseFile();

});

// Function for date picker
function datepicker() {
    $('[data-toggle="datepicker"]').attr('readonly', true);
    $('[data-toggle="datepicker"]').datepicker({
        language: 'he-HE'
    });
}

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

//add attrbute onkeypress inly numbers allow
function onlyNumbers(){
    $('.only-numbers').attr('onkeypress', 'return event.charCode >= 48 && event.charCode <=57');
}

// chose file change the style of button
function choseFile() {
    $('input[type="file"]').change(function () {
        console.log("chose file");
        var $this = $(this);
        var fileName = $this.val();
        var $label = $('label[for="' + $this.attr('id') + '"]')[0];
        var $father = $this.parent();
        if (fileName.length == 0) {
            $label.append('<i class="ml-1 material-icons">add_photo_alternate</i>');
            $label.textContent = 'בחר קובץ';
            $father.removeClass("file-selcted");
        }
        else {
            $label.textContent = fileName.toString();
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

/* The function get the checked value(true or false)
 * and the elemnt that need to be show
 * if checked true it will fade in else it will fade out
 */
function showHidenPortion(checked, idElement) {
    if (checked == true) {
        $('#' + idElement).fadeIn();
        $('#' + idElement + ' :input').each(function () {
            $(this).addClass('must');
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
    $('.must').each(function () {
        var $this = $(this);
        var $singleChosen = $this.parent().find('.chosen-single'); //for chosen drop down.
        $this.removeClass('border-danger');
        $singleChosen.removeClass('border-danger');
        var txt = $this.val();
        if (txt.length == 0 || txt == null) {
            ok = false;
            $this.addClass('border-danger');
            $singleChosen.addClass('border-danger');
        }
    })
    return ok;
}


