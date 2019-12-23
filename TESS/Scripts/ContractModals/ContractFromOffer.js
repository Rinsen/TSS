$(document).ready(function () {
    $("#contractFromOffer-modal-button").click(function () {
        $("#contractFromOfferModal").appendTo("body").modal("show").find('.modal-content').draggable();
    });

    $("#contractFromOfferModal #check-all-modules").click(function () {
        var $inputs = $("#modules-from-open-offer").find(":input");
        if (this.innerHTML == "Check all") {
            $inputs.prop('checked', true);
            this.innerHTML = "Uncheck all";
        }
        else {
            $inputs.prop('checked', false);
            this.innerHTML = "Check all";
        }
    });

    $("#contractFromOfferModal #check-all-services").click(function () {
        var $inputs = $("#services-from-open-offer").find(":input");
        if (this.innerHTML == "Check all") {
            $inputs.prop('checked', true);
            this.innerHTML = "Uncheck all";
        }
        else {
            $inputs.prop('checked', false);
            this.innerHTML = "Check all";
        }
    });

    $("#contractFromOfferModal #choose-selected-things").click(function () {
        var $inputs = $("#contractFromOfferModal #modules-from-open-offer").find("input:checked");
        var length = $inputs.length;
        var moduleList = [];
        for(var i = 0; i < length; i++){
            var obj = {}
            $input = $($inputs[i]);
            obj.Article_number = $input.attr("data-id");
            obj.License = $input.attr("data-license");
            obj.Maintenance = $input.attr("data-maintenance");
            obj.Alias = $input.attr("data-alias");
            obj.Offer_number = $input.attr("data-offer-number");
            moduleList.push(obj);
        }

        $inputs = $("#contractFromOfferModal #services-from-open-offer").find("input:checked");
        length = $inputs.length;
        var serviceList = [];
        for (var j = 0; j < length; j++) {
            id = JSON.parse($($inputs[j]).attr("data-id"));
            serviceList.push(id);
        }

        $.ajax({
            "url": serverPrefix + "CustomerContract/SaveContractFromOffer/",
            "type": "POST",
            "data": {
                "modules": JSON.stringify(moduleList),
                "services": JSON.stringify(serviceList),
                "customer": customerName,
                "contract-id": contractId
            },
            "success": function (data) {
                if (data > 0) {
                    if (typeof updateSelectedItems != "undefined")
                        updateSelectedItems();
                    if (typeof updateServiceSelected != "undefined")
                        updateServiceSelected();
                    $.ajax({
                        "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_ModuleSection",
                        "type": "GET",
                        "success": function (data) {
                            $(".crm-pdf-module-section").html(data);
                            triggerAlert("Successfully added articles from offer.", "success");
                            $("#contractFromOfferModal").modal("hide");
                            location.reload(); //För att uppdatera menyer...
                        }
                    });
                    console.log("success");
                }
                else {
                    triggerAlert("Failed to add articles from offer.", "warning");
                    console.log("failure");
                }
            }
        });

    });
});

