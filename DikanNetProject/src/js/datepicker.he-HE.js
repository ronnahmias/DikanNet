(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined' ? factory(require('jquery')) :
        typeof define === 'function' && define.amd ? define(['jquery'], factory) :
            (factory(global.jQuery));
}(this, (function ($) {
    'use strict';

    $.fn.datepicker.languages['he-HE'] = {
        format: 'dd-MM-yyyy',
        //format: 'yyyy/MM/dd',
        days: ['ראשון', 'שני', 'שלישי', 'רביעי', 'חמישי', 'שיש', 'שבת'],
        daysShort: ['ראשון', 'שני', 'שלישי', 'רביעי', 'חמישי', 'שיש', 'שבת'],
        daysMin: ['א', 'ב', 'ג', 'ד', 'ה', 'ו', 'ש'],
        weekStart: 7,
        months: ['ינואר', 'פברואר', 'מרץ', 'אפריל', 'מאי', 'יוני', 'יולי', 'אוגוסט', 'ספטמבר', 'אוקטובר', 'נובמבר', 'דצמבר'],
        monthsShort: ['ינואר', 'פברואר', 'מרץ', 'אפריל', 'מאי', 'יוני', 'יולי', 'אוגוסט', 'ספטמבר', 'אוקטובר', 'נובמבר', 'דצמבר']
    };
})));
