var BooleanFilterItems = [{ Name: "Yes", Id: "1" }, { Name: "No", Id: "0" }];

function createFilterItemsFromList(list, textForAll) {
    var filterItems = list.map(
        function (item) {
            return { Text: item.Name, Value: item.Id }
        }
    );
    if (textForAll) { // textForAll is either nothing (undefined or null) or something like '@Html.T("-All-")'
        filterItems.splice(0, 0, { Text: textForAll, Value: "" });
    }
    return filterItems;
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function ajax(type, url, data, onSuccess, onError) {
    $.ajax({
        type: type,
        url: url,
        data: data,
        success: function (response) {
            if (onSuccess != null) {
                onSuccess(response);
            }
        },
        error: function (data, errorText) {
            if (onError != null) {
                onError(data, errorText);
            }
        }
    });
}

function guid() {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
          .toString(16)
          .substring(1);
    }
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
}

function deleteRecordItem(url, item, list, callback) {
    $.ajax({
        type: "POST",
        url: url,
        data: item,
        success: function (response) {
            if (list != null) {
                list.refresh();
            }
            if (callback != null) {
                callback(item, true, response);
            }
        },
        error: function (data, errorText) {
            if (callback != null) {
                callback(item, false, errorText);
            }
        }
    });
}

function saveRecordItem(url, item, list, callback) {
    $.ajax({
        type: "POST",
        url: url,
        data: item,
        success: function (response) {
            if (list != null) {
                list.refresh();
            }
            if (callback != null) {
                callback(item, true, response);
            }
        },
        error: function (data, errorText) {
            if (callback != null) {
                callback(item, false, errorText);
            }
        }
    });
}

function checkNameForItem(url, scope) {
    scope.busyNameCheck = true;
    $.ajax({
        type: "POST",
        url: url,
        data: scope.item,
        success: function (response) {
            scope.$apply(function () {
                scope.busyNameCheck = false;
                scope.item.NameIsInvalid = !response;
            });
        },
        error: function (data, errorText) {
            scope.$apply(function () {
                scope.busyNameCheck = false;
            });
        }
    });
}

function checkEmailForItem(url, scope) {
    scope.busyEmailCheck = true;
    $.ajax({
        type: "POST",
        url: url,
        data: scope.item,
        success: function (response) {
            scope.$apply(function () {
                scope.busyEmailCheck = false;
                scope.item.EmailIsInvalid = !response;
            });
        },
        error: function (data, errorText) {
            scope.$apply(function () {
                scope.busyEmailCheck = false;
            });
        }
    });
}

function checkServerBusy(urlAction, list) {
    $.ajax({
        type: "POST",
        url: urlAction,
        success: function (response) {
            if (response.result) {
                //busy
                setTimeout(function () { checkServerBusy(urlAction, list); }, 500);
            }
            else {
                myApp.hidePleaseWait();
                list.getData(1);
            }
        },
        error: function (data, errorText) {
        }
    });
}

function makeRandomId(length) {
    var text = "";
    var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    for (var i = 0; i < length; i++) {
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    }
    return text;
}

// returns date (as string) to use for datetimepicker
function UtcToLocale(date) { // date as received from server (string, UTC)
    var localDate = new Date(date); // conversion from UTC to locale
    return moment(localDate).format('YYYY/MM/DD HH:mm');
}

// returns date (as string) to send to server
function LocaleToUtc(date) { // date as Moment object (for example created by datetimepicker)
    var result = date.toJSON(); // to utc string, including timzone info
    result = result.substring(0, result.length - 1); // remove timezone info (otherwise server will use this, unwanted)
    return result
}

function dateToText(date) { // date as received from server (string, UTC)
    if (date == null) {
        return htmlEncode("-");
    } else {
        return htmlEncode(UtcToLocale(date));
    }
}

