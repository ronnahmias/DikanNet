﻿/*************
Version: 1.1;
Last Update: 26/7/19



**************/

$(document).ready(function () {
    //add attrbute onkeypress inly numbers allow
    $('.only-numbers').attr('onkeypress', 'return event.charCode >= 48 && event.charCode <=57');

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

    // Function for dropdown lists to chose
    $(".chosen").chosen(
        {
            no_results_text: "אין ערך כזה.",
            rtl: true,
            allow_single_deselect: true,
            width: '100%'
        });
});

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
    if (checked == true)
        $('#' + idElement).fadeIn();
    else $('#' + idElement).fadeOut();
}

/* The function check that all the must filed are filed */
function checkMust() {
    //alert('check Must Function');
    var ok = true;
    $('.must').each(function () {
        var $this = $(this);
        $this.removeClass('border-danger');
        var txt = $this.val();
        if (txt.length == 0 || txt == null) {
            ok = false;
            $this.addClass('border-danger');
        }
    })
    return ok;
}