function GetYourUserProfile(token, onSuccess, onError) {
    myApp.showPleaseWait();
    var jsonData = { "token": token};
    $.ajax({
        type: 'post',
        url: '/LiveAPI/GetYourUserProfile',
        data: jsonData,
        success: function (response) {
            myApp.hidePleaseWait();
            if (onSuccess != null) {
                onSuccess(response);
            }
        },
        error: function (data, errorText) {
            myApp.hidePleaseWait();
            if (onError != null) {
                onError(data, errorText);
            }
        }
    });

}