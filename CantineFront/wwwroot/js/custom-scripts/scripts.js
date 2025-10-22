$("#msg-alert").hide();
//$(".tbProforma").hide();

inputLengthLimit($('#PhoneNumber'))
inputLengthLimit($('#amount'))

const notificationBtn = document.getElementById('btnNotificationPush');

function ajaxManager(type, url, successHandler, errorHandler, data = null) {
    current_effect = 'img';
    run_waitMe(current_effect);

    $.ajax({
        type,
        url,
        data,
        success: successHandler,
        error: errorHandler
    });
}

function genericAjax(type, url, data, successHandler, errorHandler) {
    current_effect = 'img';
    run_waitMe(current_effect);

    $.ajax({
        type,
        url,
        data,
        success: successHandler,
        error: errorHandler
    });
}






function inputLengthLimit(val) {
    val.on("keypress keyup blur", function (event) {
        $(this).val($(this).val().replace(/[^\d].+/, ""));
        $('#addTel').show();
        $('#GetOTP').show();
        if ((event.which < 48 || event.which > 57)) {
            event.preventDefault();
        }
    });
}

function inputNumber(val) {
    val.on("keypress keyup blur", function (event) {
        $(this).val($(this).val().replace(/[^\d].+/, ""));
        if ((event.which < 48 || event.which > 57)) {
            event.preventDefault();
        }
    });
}



function run_waitMe_DOPdfGenerator(effect, text) {
    $('#container').waitMe({

        //none, rotateplane, stretch, orbit, roundBounce, win8, 
        //win8_linear, ios, facebook, rotation, timer, pulse, 
        //progressBar, bouncePulse or img
        effect: effect,

        //place text under the effect (string).
        text: text,

        //background for container (string).
        bg: 'rgba(255,255,255,0.7)',

        //color for background animation and text (string).
        color: '#000',

        //max size
        maxSize: '',

        //wait time im ms to close
        waitTime: -1,

        //url to image
        source: '../../Content/Images/dpw_icon_load.gif',

        //or 'horizontal'
        textPos: 'vertical',

        //font size
        fontSize: '20px',

        // callback
        onClose: function () { }

    });
}

function run_waitMe(effect) {
    $('#container').waitMe({

        //none, rotateplane, stretch, orbit, roundBounce, win8, 
        //win8_linear, ios, facebook, rotation, timer, pulse, 
        //progressBar, bouncePulse or img
        effect: effect,

        //place text under the effect (string).
        text: 'Patientez...',

        //background for container (string).
        bg: 'rgba(255,255,255,0.7)',

        //color for background animation and text (string).
        color: '#000',

        //max size
        maxSize: '',

        //wait time im ms to close
        waitTime: -1,

        //url to image
        source: '../../Content/Images/dpw_icon_load.gif',

        //or 'horizontal'
        textPos: 'vertical',

        //font size
        fontSize: '',

        // callback
        onClose: function () { }

    });
}

function stop_waitMe() {
    $('#container').waitMe('hide')
}



function serializeObject(form) {
    // Create a new FormData object
    const formData = new FormData(form);

    // Create an object to hold the name/value pairs
    const pairs = {};

    // Add each name/value pair to the object
    for (const [name, value] of formData) {
        pairs[name] = value;
    }

    // Return the object
    return pairs;
}

function serializeJSON(form) {
    // Create a new FormData object
    const formData = new FormData(form);

    // Create an object to hold the name/value pairs
    const pairs = {};

    // Add each name/value pair to the object
    //for (let [name, value] of formData) {
    //    pairs[name] = value;
    //}

    formData.forEach(function (value, name) {
        pairs[name] = value;
    })

    // Return the JSON string
    return JSON.stringify(pairs, null, 2);
}

var startLoadingById = function (elementId = "body") {
    try {
        spiner = $("<div class='modalspinner'></div>");
        spiner.css({ "overflow": "hidden", "display": "block" })
        $(elementId).addClass("loading").css({ "position": 'relative' })
        $(elementId).append(spiner);
    } catch (e) {
        console.log(e)
    }

}

var endLoadingById = function (elementId = "body") {

    spiner = $(".modalspinner").clone();
    $(elementId).removeClass("loading");
    $(elementId).find(".modalspinner").remove();

}


function messageToastReload(msg, type, timer, messageTitle = 'Information') {
    $('#container').waitMe('hide')
    messageTitle = type == 'error' || 'Echec' || 'echec' ? 'Avertissement' : messageTitle

    $(window).madWindow("open",
        {
            messageTitle: messageTitle,
            messageContent: msg,
            theme: type + " hanging-close",
            behavior: "topRight",
            closeTimeout: timer,
            //expireDays: 1,
            width: 400,
            hiddenCallback: function () {
                window.location.reload
            },
            closeCallback: function () {
                window.location.reload
            }
        });
}

function messageToastRedirect(msg, type, timer, messageTitle = 'Information', redirect) {
    $('#container').waitMe('hide')
    messageTitle = type == 'error' || 'Echec' || 'echec' ? 'Avertissement' : messageTitle

    $(window).madWindow("open",
        {
            messageTitle: messageTitle,
            messageContent: msg,
            theme: type + " hanging-close",
            behavior: "topRight",
            closeTimeout: timer,
            //expireDays: 1,
            width: 400,
            hiddenCallback: function () {
                window.location.href = redirect
            },
            closeCallback: function () {
                window.location.href = redirect
            }
        });
}

function messageToast(msg, type, timer, messageTitle = 'Information') {
    $('#container').waitMe('hide')
    messageTitle = type == 'error' || 'Echec' || 'echec' ? 'Avertissement' : messageTitle

    $(window).madWindow("open",
        {
            messageTitle: messageTitle,
            messageContent: msg,
            theme: type + " hanging-close",
            behavior: "topRight",
            closeTimeout: timer,
            //expireDays: 1,
            width: 400
        });
}

function messageToast(msg, type, timer = 15, messageTitle = 'Information') {

    // Type: 'success', 'loader', 'error', 'warning', 'info'
    $('#container').waitMe('hide')
    messageTitle = type == 'error' || 'Echec' || 'echec' ? 'Avertissement' : messageTitle

    $(window).madWindow("open",
        {
            messageTitle: messageTitle,
            messageContent: msg,
            theme: type + " hanging-close",
            behavior: "topRight",
            closeTimeout: timer,
            //expireDays: 1,
            width: 400
        });
}


function ToastR(msg, type, timer = 5, position = null) {
    if (position) {
        toastr.options.positionClass = position;
    }

    toastr.options.timeOut = timer * 1000; // How long the toast will display without user interaction
    //les positions :'toast-bottom-full-width', 'toast-top-full-width','toast-top-left', 'toast-top-right', 'toast-bottom-right','toast-bottom-left'
    $("#toast-container").remove();
    var types = ["success", "error", "warning", "info"];
    if (types.indexOf(type) >= 0) {
        toastr[type](msg);
    }
}
function messageAlert(title, msg, type, timer) {




    $('#messageTitle').text(title);
    $('#message').html(msg);
    $('#container').waitMe('hide');
    window.scrollTo(0, 50);
    $("#msg-alert").css("display", "normal");
    $('#msg-alert').removeClass().addClass("col-md-12 text-center alert alert-" + type);
    $("#msg-alert").fadeTo((timer * 1000), 500).slideUp(500, function () {
        $("#msg-alert").slideUp(500);
        //window.location.href = "/Request/GetProforma/";
    });
}
function showMessageAlert() {
    var html = '<button type="button" class="close" data-dismiss="alert">x</button>' +
        '<h2 class="alert-heading" > <span id="isSuccess"></span></h2><hr><span id="message"></span>'

    $('#msg-alert').append(html)
    }


function initAddArticleToCartClick() {
        $(".addToCart").off().on("click", function (e) {
            itemcart = $(this).data("itemcart");
            var postData = itemcart;
            console.log("Post data = " + postData)
            var successHandler = (data) => {

                if (data == undefined || data == null || data == "")
                    ToastR("Une erreur a été rencontrée", "error", 10);
                else {

                    if (data.success) {
                        $("#cartBadge").text(data.object);
                        $("#cartBadge").show();
                        $("#cartBadgeFloatCount").text(data.object);
                        $("#cartBadgeFloat").show();
                        $("#cartBadgeFloatClear").show();
                       // ToastR(data.message, "success", 10,"toast-bottom-right");

                    }

                    else {

                        ToastR(data.message, "error", 10);

                    }

                }

            };

            var errorHandler = (err) => {

                ToastR("La suppréssion a échoué", "error", 10);
            };
            ajaxManager("POST", "/Home/AddArticle", successHandler, errorHandler, itemcart);


        }
        );

}


$("#cartBadgeFloatClear").off("click").on("click", function () {

    var successHandler = (data) => {

        if (data == undefined || data == null || data == "")
            ToastR("Une erreur a été rencontrée", "error", 10);
        else {

            if (data.success) {
                try {
                    if (ARTICLES_PRIX_DICT)
                        for (const [id, price] of Object.entries(ARTICLES_PRIX_DICT)) {
                            delete ARTICLES_PRIX_DICT[id];
                            $("#row_" + id).remove();
                            $("#line_" + id).remove();
                        }

                } catch (e) {

                }
              

                $("#cartBadge").text(0);
                $("#cartBadge").hide();
                $("#cartBadgeFloat").hide();
                $("#cartBadgeFloatClear").hide();
                $("#cartBadgeFloatCount").text(0)

                $("#totalPrice").text(`0 XOF`);
                $("#btnCommand").prop("disabled", true);
             
                $("#selectedArticlesCount").text(0)
            }

            else {

                ToastR(data.message, "error", 10);

            }

        }

    };
    var errorHandler = (err) => {

        ToastR("La suppréssion a échoué", "error", 10);
    };

    ajaxManager("DELETE", "/Home/DeleteAllArticleCart", successHandler, errorHandler, {  })


    
});