

function DeleteCommandItem(btn, data) {
    var command = $(btn).closest('div.all');

    command.remove();
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
function GetPendingCommandes(colClass = "col-12") {
    var successCommandeHandler = function (res) {
        if (res) {
            if (res.success) {
                if (!res.data) {
                    $("#noItemsAlert").show();
                    return;
                }
                var len = res.data.length;
                if (len <= 0) {
                    $("#noItemsAlert").show();

                    $('.filters_menu li').off("click").on("click", function () {
                        $('.filters_menu li').removeClass('active');
                        $(this).addClass('active');

                        var data = $(this).attr('data-filter');
                        $grid.isotope({
                            filter: data
                        })
                    });

                    var $grid = $(".grid").isotope({
                        itemSelector: ".all",
                        percentPosition: false,
                        masonry: {
                            columnWidth: ".all"
                        }
                    });
                    $grid.isotope('on', 'layoutComplete', function () {
                        var numItems = $grid.find('.all:not(.isotope-hidden)').length;
                        if (numItems == 0) {
                            $("#noItemsAlert").show();
                        } else {
                            $("#noItemsAlert").hide();
                        }
                    });

                } else {
                    $("#noItemsAlert").hide();
                }
                $("#messagesList").html("");

                res.data.forEach((command, index) => {
                    var fullname = command.userNavigation ? (command.userNavigation.login) : "";
                    var emplacement = command.emplacementNavigation ? command.emplacementNavigation.name : "";
                    var paymentMethod = command.paymentMethodNavigation ? command.paymentMethodNavigation.name : "";
                    var emplacementId = command.emplacementNavigation ? command.emplacementNavigation.id : 0;
                    var nbArticles = command.ligneCommandesNavigation ? command.ligneCommandesNavigation.length : 0;

                    if (location.pathname.toLowerCase() == "/commande/index") {
                        colClass = "col-md-6 col-lg-4 col-xl-3 col-sm-12 m-1";
                    }

                    paymentMethod = paymentMethod == "SOLDE" ? "DOTATION" : paymentMethod;
                    var paymentMethodHtml = paymentMethod ? `
                        <div class="payment-method">
                            <i class="fas fa-credit-card payment-icon"></i>
                            <span class="payment-text">${paymentMethod}</span>
                        </div>
                    ` : '';

                    var commandeADistance = command.commandeADistance ? `
                        <span class="distance-badge">
                            <i class="fas fa-truck"></i>
                            À distance
                        </span>
                    ` : '';

                    var commentaire = command.comment;
                    var commentaireHtml = commentaire ? `
                        <div class="comment-section">
                            <div class="comment-label">
                                <i class="fas fa-comment comment-icon"></i>
                                Commentaire:
                            </div>
                            <p class="comment-text">${commentaire}</p>
                        </div>
                    ` : "";

                    //$("#messagesList").append(`
                    //    <div class="command-card ${colClass} all emplacement_${emplacementId}" id="command_${command.id}">
                    $("#messagesList").append(`
     <div class="command-card col-md-6 col-lg-4 col-xl-3 all emplacement_${emplacementId}" id="command_${command.id}">
                            <div class="card-header">
                                <div class="customer-info">
                                    <i class="fas fa-user customer-icon"></i>
                                    <span class="customer-name">${fullname}</span>
                                </div>
                            </div>
                            
                            <div class="card-body">
                                <div class="location-info">
                                    <i class="fas fa-map-marker-alt location-icon"></i>
                                    <span class="location-text">${emplacement}</span>
                                </div>
                                
                                ${paymentMethodHtml}
                                
                                <div class="command-actions">
                                    <div class="timer-badge" id="time${command.id}"></div>
                                    ${commandeADistance}
                                    <a class="deliver-btn"
                                       data-method="POST"
                                       data-callback="DeleteCommandItem"
                                       data-message="Veuillez confirmer?"
                                       data-url="/Commande/Delivery/${command.id}" 
                                       data-toggle="modal"
                                       data-target="#confirmModal"
                                       data-commandid="${command.id}" 
                                       href="#!">
                                        <i class="fas fa-check deliver-icon"></i>
                                        Livrer
                                    </a>
                                </div>
                                
                                <div class="articles-section">
                                    <div id='lignesCommande${command.id}' class="articles-list"></div>
                                </div>
                                
                                ${commentaireHtml}
                            </div>
                        </div>
                    `);

                    if (command.ligneCommandesNavigation) {
                        command.ligneCommandesNavigation.forEach((ligne, index) => {
                            $(`#lignesCommande${command.id}`).append(`
                                <div class="article-item" id="row_${ligne.articleNavigation.id}">
                                    <span class="article-name">${ligne.articleNavigation.title}</span>
                                    <span class="article-quantity">${ligne.quantite}</span>
                                </div>
                            `);
                        });
                    }

                    if (index == len - 1) {
                        clearInterval(setTimers);
                        setInterval(setTimers, 1000, res.data);

                        try {
                            var itemReveal = Isotope.Item.prototype.reveal;
                            Isotope.Item.prototype.reveal = function () {
                                itemReveal.apply(this, arguments);
                                $(this.element).removeClass('isotope-hidden');
                            };
                            var itemHide = Isotope.Item.prototype.hide;
                            Isotope.Item.prototype.hide = function () {
                                itemHide.apply(this, arguments);
                                $(this.element).addClass('isotope-hidden');
                            };
                        } catch { }

                        $('.filters_menu li').off("click").on("click", function () {
                            $('.filters_menu li').removeClass('active');
                            $(this).addClass('active');
                            var data = $(this).attr('data-filter');
                            $grid.isotope({ filter: data });
                        });

                        var $grid = $(".grid").isotope({
                            itemSelector: ".all",
                            percentPosition: false,
                            masonry: { columnWidth: ".all" }
                        });

                        $grid.isotope('on', 'layoutComplete', function () {
                            var numItems = $grid.find('.all:not(.isotope-hidden)').length;
                            if (numItems == 0) {
                                $("#noItemsAlert").show();
                            } else {
                                $("#noItemsAlert").hide();
                            }
                        });
                    }
                });
            }
        }
    }

    ajaxManager('GET', '/Commande/GetPendingCommandes', successCommandeHandler, errorHandler);
}

// Ajoutez ce CSS dans votre fichier de styles

//function GetPendingCommandes(colClass="col-12") {
//    var successCommandeHandler = function (res) {
//        if (res) {

//            if (res.success) {
//                if (!res.data) {
//                    $("#noItemsAlert").show();
//                    return;
//                }
//                var len = res.data.length;
//                if (len <= 0) {
//                    $("#noItemsAlert").show();

//                    $('.filters_menu li').off("click").on("click", function () {
//                        $('.filters_menu li').removeClass('active');
//                        $(this).addClass('active');

//                        var data = $(this).attr('data-filter');
//                        $grid.isotope({
//                            filter: data
//                        })
//                    });

//                    var $grid = $(".grid").isotope({
//                        itemSelector: ".all",
//                        percentPosition: false,
//                        masonry: {
//                            columnWidth: ".all"
//                        }
//                    });
//                    $grid.isotope('on', 'layoutComplete', function () {
//                        var numItems = $grid.find('.all:not(.isotope-hidden)').length;
//                        if (numItems == 0) {
//                            $("#noItemsAlert").show();

//                        } else {

//                            $("#noItemsAlert").hide();
//                        }
//                    });

//                } else {
//                    $("#noItemsAlert").hide();
//                }
//                $("#messagesList").html("");

//                res.data.forEach((command, index) => {
//                    var fullname = command.userNavigation ? (command.userNavigation.login) : "";
//                    var emplacement = command.emplacementNavigation ? command.emplacementNavigation.name : "";
//                    var paymentMethod = command.paymentMethodNavigation ? command.paymentMethodNavigation.name : "";
//                    var emplacementId = command.emplacementNavigation ? command.emplacementNavigation.id : 0;
//                    var nbArticles = command.ligneCommandesNavigation ? command.ligneCommandesNavigation.length : 0;
//                    if (location.pathname.toLowerCase() == "/commande/index") {
//                        colClass = "col-md-6 col-lg-4 col-xl-3 col-sm-12 m-1";
//                    }
//                    paymentMethod = paymentMethod == "SOLDE" ? "DOTATION" : paymentMethod;
//                    var paymentMethod = paymentMethod ? `<div class=" small my-2"  style="font-size:11px;font-weight:bold"> Moyen de paiement: <span class=" bold text-primary">${paymentMethod}</span></div>` : '';
//                    var commandeADistance = command.commandeADistance ? `<span class="badge badge-success" style="font-size:10px">commande à distance.</span>` : '';
//                    var commentaire = command.comment;
//                    commentaire = commentaire ? ` <div class="col-12 px-0 text-black"><label class="mb-3 text-black fw-bold" style="font-size:11px;font-weight:bold">Commentaire: </label><p style="font-size:11px" class="fw-bold ">${commentaire}</p></div>` : "";
//                   // <span class="card-text">${nbArticles} article(s).</span>
//                    $("#messagesList").append(`
//                        <div class="card border border-warning shadow-0 mb-3 ${colClass} all emplacement_${emplacementId}" id="command_${command.id}" >
//                            <div class="card-header fullname text-center">${fullname}</div>
//                            <div class="card-body ">
//                               <p class="text-danger medium mb-1"> Emplacement: <span class=" bold">${emplacement}</span></p>
//                                 <div class="d-flex justify-content-between align-items-start">
//                                  ${paymentMethod}
//                                 </div>
//                                <div class="d-flex justify-content-between align-items-start">
//                                <span class="badge badge-danger text-white" id="time${command.id}"></span>
//                                ${commandeADistance}
//                                 <a class="btn btn-primary btn-sm float-end btnDelivery"
//                                   data-method="POST"
//                                   data-callback="DeleteCommandItem"
//                                        data-message="Veuillez confirmer?"
//                                        data-url="/Commande/Delivery/${command.id}" data-toggle="modal"
//                                data-target="#confirmModal"
//                                 data-commandid="${command.id}"  href="#!">Livrer</a>
//                                </div>
//                               <div id='lignesCommande${command.id}'></div>
//                               ${commentaire}
//                            </div>
//                        </div>
                            
//                            `);

//                    if (command.ligneCommandesNavigation) {

//                        command.ligneCommandesNavigation.forEach((ligne, index) => {

//                            $(`#lignesCommande${command.id}`).append(`
//                                <div class="row d-flex justify-content-between align-items-center text-muted" id="row_${ligne.articleNavigation.id}">
//                                    <div class="col-md-8 col-lg-8 col-xl-8 col-sm-8">
//                                        <span class="text-black mb-0" style="font-size:10px">${ligne.articleNavigation.title}</span>
//                                    </div>
//                                    <div class="col-md-3 col-lg-3 col-xl-3 col-sm-3 d-flex">
//                                        <span 
//                                                class="badge badge-primary" >${ligne.quantite}</span>
//                                    </div>
//                                </div>
//                                    <hr style="margin-top:0;margin-bottom:0">
//                        `);
//                        });
//                    }
                   

//                    if (index == len - 1) {
//                        clearInterval(setTimers);
//                        setInterval(setTimers, 1000, res.data);
//                        console.log("len = ", len)

//                        try {
//                            // Add hidden class if item hidden
//                            var itemReveal = Isotope.Item.prototype.reveal;
//                            Isotope.Item.prototype.reveal = function () {
//                                itemReveal.apply(this, arguments);
//                                $(this.element).removeClass('isotope-hidden');
//                            };
//                            var itemHide = Isotope.Item.prototype.hide;
//                            Isotope.Item.prototype.hide = function () {
//                                itemHide.apply(this, arguments);
//                                $(this.element).addClass('isotope-hidden');
//                            };

//                        } catch {

//                        }

//                        $('.filters_menu li').off("click").on("click", function () {
//                            console.log("click filter")
//                            $('.filters_menu li').removeClass('active');
//                            $(this).addClass('active');

//                            var data = $(this).attr('data-filter');
//                            $grid.isotope({
//                                filter: data
//                            })
//                        });

//                        var $grid = $(".grid").isotope({
//                            itemSelector: ".all",
//                            percentPosition: false,
//                            masonry: {
//                                columnWidth: ".all"
//                            }
//                        })

//                        $grid.isotope('on', 'layoutComplete', function () {
//                            var numItems = $grid.find('.all:not(.isotope-hidden)').length;
//                            if (numItems == 0) {
//                                $("#noItemsAlert").show();

//                            } else {

//                                $("#noItemsAlert").hide();
//                            }
//                        });
//                    }

//                })

//            }
//        }
//    }

//    ajaxManager('GET', '/Commande/GetPendingCommandes', successCommandeHandler, errorHandler);

//}
