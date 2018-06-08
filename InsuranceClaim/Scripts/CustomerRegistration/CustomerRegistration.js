$(document).ready(function () {
    $('#DivProductInfo').hide();

    //This functionality only for check numeric value
    $("#phone_number").keypress(function (e) {
        if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
            $("#errmsg").html("Digits Only").show().fadeOut("slow");
            return false;
        }
    });
})