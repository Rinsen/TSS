$(document).ready(function () {
    $("#optionsFromActive-modal-button").click(function () {
        $("#optionsFromActiveModal").appendTo("body").modal("show");
    });

    $("#optionsFromActiveModal #check-all-options").click(function () {
        var $inputs = $("#options-from-active-contract").find(":input");
        if (this.innerHTML == "Check all") {
            $inputs.prop('checked', true);
            this.innerHTML = "Uncheck all";
        }
        else {
            $inputs.prop('checked', false);
            this.innerHTML = "Check all";
        }
    });

    $("#optionsFromActiveModal #choose-selected-things").click(function () {
        var $inputs = $("#options-from-active-contract").find("input:checked");
        var length = $inputs.length;
        var optionList = [];
        for (var i = 0; i < length; i++) {
            id = $($inputs[i]).attr("data-id");
            optionList.push(id);
        }

        $.ajax({
            "url": serverPrefix + "CustomerContract/SaveOptionFromActive/",
            "type": "POST",
            "data": {
                "options": JSON.stringify(optionList),
                "customer": customerName,
                "contract-id": contractId,
            },
            "success": function (data) {
                if (data > 0) {
                    if (typeof updateSelectedOptions != "undefined")
                        updateSelectedOptions();
                    $.ajax({
                        "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_ModuleSection",
                        "type": "GET",
                        "success": function (data) {
                            $(".crm-pdf-module-section").html(data);
                            triggerAlert("Successfully added options from contract.", "success");
                            $("#optionsFromActiveModal").modal("hide");
                        }
                    });
                    console.log("success");
                }
                else {
                    triggerAlert("Failed to add options from contract.", "warning");
                    console.log("failure");
                }
            }
        });

    });
});
