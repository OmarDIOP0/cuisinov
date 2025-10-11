

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
//document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    console.log("user = "+user)
    if (user == "shop" && location.pathname.toLowerCase() == "/home/bookvalidation") {
        initLocalCommandeValidation(message);
        return;
    }

    var command = JSON.parse(message);
    var fullname = command.userNavigation ? (command.userNavigation.login) : "";
    var emplacement = command.emplacementNavigation ? command.emplacementNavigation.name : "";
    var emplacementId = command.emplacementNavigation ? command.emplacementNavigation.id : 0;
   
    var commandeADistance = command.commandeADistance ? `<span class="badge badge-success" style="font-size:10px">commande à distance.</span>` : '';
    var display = "none";
    if ($(".filters_menu li").first().hasClass("active")) {
        display = "normal";
    } else {
        if ($(`.emplacement_${emplacementId}`).length > 0) {
            display = $(`.emplacement_${emplacementId}`)[0].style.display;
        }
    }

    console.log("display  =  "+display)
    colClass = "col-12";
    if (location.pathname.toLowerCase() == "/commande/index") {
        colClass = "col-md-6 col-lg-4 col-xl-3 col-sm-12 m-1";
    }
    var $grid = $("#messagesList").isotope({
        filter: `.emplacement_${emplacementId}`
    });

    var $itemElement = $(`<div class="card border border-warning shadow-0 mb-3 ${colClass} all emplacement_${emplacementId}  pulse" id="command_${command.id}" style="display:${display};height:fit-content;">
                            <div class="card-header">${fullname}</div>
                            <div class="card-body text-danger">
                               <p> Emplacement: <span class=" bold">${emplacement}</span></p>
                                
                                <div class="d-flex justify-content-between align-items-start">
                                <span class="badge badge-danger text-white" id="time${command.id}"></span>

                                ${commandeADistance}
                                 <a class="btn btn-primary btn-sm float-end btnDelivery"  data-commandid="${command.id}"  href="#!">Livrer</a>
                                </div>
                               <div id='lignesCommande${command.id}'></div>
                            </div>
                        </div>`);
   
    $grid.append($itemElement).isotope('appended', $itemElement)

    $(`#li_filter_${emplacementId}`).trigger("click");
    //init Timer
    if (command.date) {

        setInterval(function () {

            var hour = getTimeDiff(`#time${command.id}`, command.date);

            var dt = new Date(command.date);
            var totalMinutes = Math.floor(((new Date()) - dt) / (1000 * 60));
            if (totalMinutes >= 2) {
                $(`#item${command.id}`).removeClass("pulse");
            }
        }, 1000);

    }
    //End Timer
    if (command.ligneCommandesNavigation) {
        var lenLigne = command.ligneCommandesNavigation.length;
        command.ligneCommandesNavigation.forEach((ligne, index) => {

            $(`#lignesCommande${command.id}`).append(`
                                     

                                                <div class="row d-flex justify-content-between align-items-center text-muted" id="row_${ligne.articleNavigation.id}">
                                             
                                                    <div class="col-md-8 col-lg-8 col-xl-8">
                                                        
                                                        <span class="text-black mb-0" style="font-size:10px">${ligne.articleNavigation.title}</span>
                                                    </div>
                                                    <div class="col-md-4 col-lg-4 col-xl-4 d-flex">
                                                      
                                                        <span 
                                                               class="badge badge-primary" >${ligne.quantite}</span>

                                                    </div>
                                                    
                                                </div>
                                                   <hr style="margin-top:0;margin-bottom:0">
                                        `);
      
        });
    }
  


    $(".btnDelivery").off("cilck").on("click", function () {

        var commandId = $(this).data("commandid");
        if (!commandId || commandId <= 0) return;
        var successHandler = (response) => {
            if (response) {
                if (response.success) {


                    $("#command_" + commandId).remove();
                    var activeFilters = $('.filters_menu li.active');
                    console.log("activeFilters ", activeFilters.length)
                    if (activeFilters.length > 0) {


                        element = activeFilters[0];
                        $(element).addClass('active');
                        var $grid = $("#messagesList");
                        var data = $(element).attr('data-filter');
                        $grid.isotope({
                            filter: data
                        })
                        console.log("refresh")
                    }
                }
            }
        }
        ajaxManager("POST", "/Commande/Delivery", successHandler, errorHandler, { id: commandId });
    });
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    //li.textContent = `${user} says ${message}`;
});

connection.start().then(function () {
   // document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

function sendDataCommand(user,message) {
  
    //var user = "laye";
 return   connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });

}
//document.getElementById("sendButton").addEventListener("click", function (event) {
//    var user = document.getElementById("userInput").value;
//    var message = document.getElementById("messageInput").value;
//    connection.invoke("SendMessage", user, message).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});


function initLocalCommandeValidation(messagejsonString) {

    if ($("#formSeted").val() == "true") {
        ToastR("Il y a une commande en attende de validation", "error", 10, "toast-bottom-right");
        return;
    }
    var data = JSON.parse(messagejsonString);
   
    var articlesBooking = data.articles;
    var user = data.user;
    var paymentMethod = data.paymentMethod;
    paymentMethodCode = paymentMethod ? paymentMethod.code : "";
    console.log("paymentMethod = ", paymentMethod)
    if (paymentMethod) {
        $("#PaymentMethodId").val(paymentMethod.id);
        $("#PaymentMethodId").niceSelect('update');
    }
    var montantTotal = data.montantTotal;
    if (paymentMethodCode == "QRCODE") {
        $("#clientDiv").addClass("d-flex").show();
        $("#soldeDiv").addClass("d-flex").show();
        if (user) {
            $("#userId").val(user.id);
            $("#solde").text(`${user.solde} XOF`);
            $("#soldeRestant").val(user.solde);
            $("#client").text(user.login);

        } else {
            ToastR("Utilisateur inconnu du système", "error", 10, "toast-bottom-right");
            return;
        }
    } else {

        $("#clientDiv").removeClass("d-flex").hide();
        $("#soldeDiv").removeClass("d-flex").hide();
    }
    if (paymentMethodCode == "QRCODE") {
        if (montantTotal > user.solde) {
            $("#solde").removeClass("text-warning text-danger text-success").addClass("text-danger");
            ToastR("Le montant total est supérieur à ton solde", "error", 10, "toast-bottom-right");
            $("#messageSolde").text("Solde insuffisant.");
        } else {
            $("#solde").removeClass("text-warning text-danger text-success").addClass("text-success");
            $("#messageSolde").text("");
        }
    } else {
        $("#solde").removeClass("text-warning text-danger text-success").addClass("text-success");
        $("#messageSolde").text("");
    }

    $("#selectedArticlesCount").text(articlesBooking.length);
    $("#totalPrice").text(`${montantTotal} XOF`);
    $("#articleLines").html(``);
    var ARTICLES_IDS = [];
    var leng = articlesBooking.length;
    articlesBooking.forEach((article, index) => {
        ARTICLES_IDS.push(article.id)
        ARTICLES_PRIX_DICT[article.id] = article.prixDeVente;



        $("#articleLines").append(`
                                        <hr class="my-4" id="line_${article.id}">

                                                <div class="row mb-4 d-flex justify-content-between align-items-center" id="row_${article.id}">
                                                    <div class="col-md-2 col-lg-2 col-xl-2">
                                                        <img src="data:image/png;base64,${article.image}" id="imageArticle_${article.id}" onerror="this.src='/images/default.png';" class="zoom rounded-circle" width="50" height="50">
                                                       
                                                    </div>
                                                    <div class="col-md-3 col-lg-3 col-xl-3">
                                                        <h6 class="text-muted">${article.categorie.name}</h6>
                                                        <h6 class="text-black mb-0">${article.title}</h6>
                                                    </div>
                                                    <div class="col-md-3 col-lg-2 col-xl-2 d-flex">
                                                        <button class="btn btn-link px-2 text-muted" type="button"
                                                                onclick="this.parentNode.querySelector('input[type=number]').stepDown();updateTotalPrice()">
                                                            <i class="fa fa-minus"></i>
                                                        </button>

                                                        <input id="quantite_${article.id}" min="0" name="Quantite" value="${article.quantite}" type="number" onchange="updateTotalPrice()"
                                                               class="form-control form-control-sm" />

                                                        <button class="btn btn-link px-2 text-muted" type="button"
                                                                onclick="this.parentNode.querySelector('input[type=number]').stepUp();updateTotalPrice()">
                                                            <i class="fa fa-plus"></i>
                                                        </button>
                                                    </div>
                                                    <div class="col-md-4 col-lg-3 col-xl-3 ">
                                                        <h7 class="mb-0  " style="font-size:0.8rem">${article.quantite} * ${article.prixDeVente} = <span id="prixTotal_${article.id}">${article.quantite*article.prixDeVente}</span> XOF</h7>
                                                    </div>
                                                    <div class="col-md-1 col-lg-1 col-xl-1 text-end">
                                                        <a href="#!" class="text-muted deleteArticleCart" data-articleid="${article.id}"><i class="fa fa-times"></i></a>
                                                    </div>
                                                </div>

    `);


        if (leng - 1 == index) {

            try {
                loadImageSuccessHandler = (res) => {
                    // console.log("images = ",res.id)
                    if (res != null) {
                        var imageId = res.id
                        $(`#imageArticle_${imageId}`).attr("src", `data:image/png;base64,${res.image}`);
                    }

                }
                function doSetTimeout(i) {
                    var id = ARTICLES_IDS[i]
                    setTimeout(function () {
                        console.log("images = ", id)
                        ajaxManager('GET', '/Article/GetArticleImage?id=' + id, loadImageSuccessHandler, (err) => { });
                    }, 10);
                }

                for (var i = 0; i < leng; i++) {

                    doSetTimeout(i);
                }
            } catch (e) {

            }


        }




    });

    $("#formSeted").val("true")
}