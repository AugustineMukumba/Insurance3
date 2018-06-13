$(document).ready(function () {
})

function GoToProductDetail(json) { 
    if (json == true) {
        window.location.href = '/CustomerRegistration/ProductDetail';
    }
    else {

    }
}

function GoToRiskDetail(json) {   
    debugger;
    if (json.Status == true) {
        window.location.href = '/CustomerRegistration/RiskDetail/' + json.Id;
    }
    else {
        alert(json.Message);
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