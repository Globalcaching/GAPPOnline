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
    return $('<div/>').text(value).html().replace(/\n/g, "<br />");
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
    //var localDate = new Date(date); // conversion from UTC to locale
    var localDate = moment.utc(date).toDate();
    return moment(localDate).format('llll');
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

//
// ==========================================================
// SignalR handler to communicate triggers and events
// ==========================================================
//

var SignalRHandler = function (userGuid, hubName, registerOnServerFunctionName, triggerServerFunctionName) {
    // param registerOnServerFunctionName: name of function which accepts array of strings
    // param deregisterOnServerFunctionName: name of function which accepts array of strings
    // param triggerServerFunctionName: name of function which accepts (param 1:) an array of strings, and (param 2:) a string with data

    if (!(this instanceof SignalRHandler)) {
        return new SignalRHandler(userGuid, hubName, registerOnServerFunctionName, triggerServerFunctionName)
    }

    // signalRCallback is of type { groups: [], callbackFunction: function, htmlElement: htmlElement }.
    // on multiple matches on groups the function is only executed ones
    // the same function can only be added ones
    // the callbackFunction accepts one parameter, a string which can contain data
    var callBacks = [];

    // keep reference to registerOnServer calls, to reinvoke if connection is reconnecting automatically
    var registerOnServerCalls = [];

    var asArray = function (stringOrArray) {
        var result = stringOrArray;
        if (Object.prototype.toString.call(stringOrArray) == "[object String]") {
            result = [stringOrArray];
        }
        return result
    }

    // execute callbacks if callback has one of given groups
    // groups can be a string or an array of strings
    this.triggerClient = function (sessionInf, groups, data) {
        if (sessionInf != null) {
            SessionInfo = sessionInf;
        }
        callBacks.forEach(function (cb) {
            var found = asArray(groups).some(function (group) { // usage of 'some' instead of 'forEach' to be able to break
                var index = cb.groups.findIndex(function (c) { return c == group; });
                return index > -1;
            });
            if (found) {
                cb.callbackFunction(data);
            }
        });
    }

    var hub = null;
    var startedDeferred = $.Deferred();
    var startedPromise = startedDeferred.promise();

    this.start = function () {
        hub = $.connection[hubName];
        hub.client[triggerServerFunctionName] = this.triggerClient;
        jQuery(document).ready(function () {
            $.connection.hub.start().done(function () {
                startedDeferred.resolve();
            });
            $.connection.hub.reconnected(function () {
                startedPromise.then(function () {
                    registerOnServerCalls.forEach(function () {
                        hub.server[registerOnServerFunctionName](userGuid);
                    });
                });
            });
        });
    };

    this.start();

    this.stop = function () {
        hub = null;
        registerOnServerCalls = [];
        startedDeferred = $.Deferred();
        startedPromise = startedDeferred.promise();
        $.connection.hub.stop();
    };



    // make this client listening to the server for the given groups
    // groups can be a string or an array of strings
    this.registerOnServer = function (groups) {
        startedPromise.then(function () {
            registerOnServerCalls.push();
            hub.server[registerOnServerFunctionName](userGuid);
        });
    }

    // send a change to the server to possibly trigger other clients
    // groups can be a string or an array of strings
    this.triggerServer = function (groups, data) {
        startedPromise.then(function () {
            hub.server[triggerServerFunctionName](userGuid, asArray(groups), data);
        });
    }

    // Add a function to be called when the server sends signalR triggers.
    // If htmlElement is defined (not required) then all signalR callbacks which are
    // related with this element will automatically be removed when element is removed from the DOM.
    // groups can be a string or an array of strings
    this.registerOnClient = function (groups, callBackFunction, htmlElement) {
        var index = callBacks.findIndex(function (cb) {
            return cb.callbackFunction === callBackFunction;
        });
        if (index == -1) {
            var thisRef = this;
            callBacks.push({ groups: asArray(groups), callbackFunction: callBackFunction, htmlElement: htmlElement });
            if (htmlElement != undefined) {
                $('body').on('DOMNodeRemoved', htmlElement, function (event) {
                    if (event.target === htmlElement || event.target.contains(htmlElement)) {
                        thisRef.deregisterOnClient(callBackFunction);
                    }
                });
            }
        }
    };

    this.deregisterOnClient = function (callBackFunction) {
        var index = callBacks.findIndex(function (cb) {
            return cb.callbackFunction === callBackFunction;
        });
        if (index > -1) {
            callBacks.splice(index, 1);
        }
    }
}


