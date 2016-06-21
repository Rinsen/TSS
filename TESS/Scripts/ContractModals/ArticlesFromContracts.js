$(document).ready(function () {

    $("#modulesFromContracts-modal-button").click(function () {
        $("#modulesFromContractsModal").appendTo("body").modal("show").find('.modal-content').draggable();
    });

    $("#modulesFromContractsModal #check-all-options").click(function () {
        var $inputs = $("#modules-from-contracts").find(":input");
        if (this.innerHTML == "Check all") {
            $inputs.prop('checked', true);
            this.innerHTML = "Uncheck all";
        }
        else {
            $inputs.prop('checked', false);
            this.innerHTML = "Check all";
        }
    });

    $("#modulesFromContractsModal #check-all-contract-services").click(function () {
        var $inputs = $("#services-from-contracts").find(":input");
        if (this.innerHTML == "Check all") {
            $inputs.prop('checked', true);
            this.innerHTML = "Uncheck all";
        }
        else {
            $inputs.prop('checked', false);
            this.innerHTML = "Check all";
        }
    });

    $("#modulesFromContractsModal #choose-selected-things").click(function () {
        var $inputs = $("#modules-from-contracts").find("input:checked");
        var length = $inputs.length;
        var moduleList = [];
        for (var i = 0; i < length; i++) {
            var obj = {}
            $input = $($inputs[i]);
            obj.Article_number = $input.attr("data-id");
            obj.License = $input.attr("data-license");
            obj.Maintenance = $input.attr("data-maintenance");

            moduleList.push(obj);
        }
        $inputs = $("#services-from-contracts").find("input:checked");
        length = $inputs.length;
        var serviceList = [];
        for (var i = 0; i < length; i++) {
            id = JSON.parse($($inputs[i]).attr("data-id"));
            serviceList.push(id);
        }

        $.ajax({
            "url": serverPrefix + "CustomerContract/SaveItemsFromContracts/",
            "type": "POST",
            "data": {
                "modules": JSON.stringify(moduleList),
                "services": JSON.stringify(serviceList),
                "customer": customerName,
                "contract-id": contractId,
            },
            "success": function (data) {
                if (data > 0) {

                    $.ajax({
                        "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_ModuleSection",
                        "type": "GET",
                        "success": function (data) {
                            $(".crm-pdf-module-section").html(data);
                        }
                    });
                    $.ajax({
                        "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_OldModuleSection",
                        "type": "GET",
                        "success": function (data) {
                            $(".crm-pdf-old-module-section").html(data);
                            triggerAlert("Successfully added articles and services from contracts.", "success");
                            $("#modulesFromContractsModal").modal("hide");
                        }
                    });
                    updateSelectedItems();
                    updateServiceSelected();
                    console.log("success");
                }
                else {
                    triggerAlert("Failed to add articles and services from contracts.", "warning");
                    console.log("failure");
                }
            }
        });

    });
});
