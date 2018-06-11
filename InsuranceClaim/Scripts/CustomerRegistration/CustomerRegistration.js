$(document).ready(function () {
})

function GoToProductDetail(json) {
    debugger;
    if (json == true) {
        window.location.href = '/CustomerRegistration/ProductDetail';
    }
    else {

    }
}

function GoToRiskDetail(json) {
    if (json == true) {
        window.location.href = '/CustomerRegistration/RiskDetail';
    }
   
}

function GoToSummaryDetail(json) {
    if (json == true) {
        window.location.href = '/CustomerRegistration/SummaryDetail';
    }
   
}

function GoToPaymentDetail(json) {
    window.location.href = '/CustomerRegistration/PaymentDetail';
}