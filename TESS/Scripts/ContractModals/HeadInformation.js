
$(document).ready(function () {
    var oldStatus = "";
    $("#head-info-modal-button").click(function () {
        $("#tableItemsModal").appendTo("body").modal("show");
        loadInfo();
        if (contractType == "Huvudavtal") {
            $("#main_contract_id-text").prop("disabled", "disabled");
        }
        else {
            $("#main_contract_id-text").prop("disabled", false);
        }
        if (aktStatus == "Sänt") {
            $("#contract_id-text").prop("readonly", false);
        }
        else {
            $("#contract_id-text").prop("readonly", true);
        }
       if (!webkit)
        {
            $("#valid_from-date").datepicker();
            $("#valid_through-date").datepicker();
            $("#expire-date").datepicker();
            $("#observation-date").datepicker();
        }
    });

    $("#contract_type-text").change(function () {
        
        if ($(this).val() == "Huvudavtal") {
            $("#main_contract_id-text").prop("disabled", "disabled");
        }
        else {
            $("#main_contract_id-text").prop("disabled", false);
        }
    });


    $("#save-table-changes").click(function () {
        if ($("#table-form").valid()) {
            var $ttForm = $("#table-form");
            var $inputs = $ttForm.find(":input");

            var contract = {};
            $inputs.each(function () {
                var $formInput = $(this);
                contract[$formInput.attr("name")] = $formInput.val();
                if ($formInput.attr("name") == "Contract_id") {
                    contractId = $formInput.val();
                }
            });
            $.ajax({
                "url": serverPrefix + "CustomerContract/UpdateTableInfo/",
                "type": "POST",
                "data": {
                    "customer": customerName,
                    "contract-id": oldContractId,
                    "newContract-id": contractId,
                    "json": JSON.stringify(contract),
                    "oldStatus": oldStatus
                },
                "success": function (data) {





                    console.log(data);
                    if (data.charAt(0) == '1') {

                        var timestamp = data.substr(2);
                        console.log("success");

                        //location.load();
                        if (oldContractId == contractId) {
                            location.reload();
                        }
                        else {
                            window.location = "/CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName;
                        }
                        
                        //window.location = "/CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName;
                        //$.ajax({
                        //    "url": "/CustomerContract/ViewPdf?selected-contract=" + ssma_timestamp + "&contract-section=_HeaderSection",
                        //    "type": "GET",
                        //    "success": function (data) {
                        //        $(".crm-pdf-header-section").html(data);
                        //        $("#contractHeadModal").modal("hide");
                        //        triggerAlert("Successfully updated this contract head", "success");
                        //    }
                        //});
                    }
                    else {

                        console.log("failure");
                        triggerAlert("Something went wrong when trying to update the contract info on the server", "warning");
                    }




                }
            });
        }
    });


    $formValidation = $("#tableItemsModal form").validate({
        ignore: ".ignore",
        rules: {
            "Contract_id": {
                required: true,
            },

            "Term_of_notice": {
                digits: true
                //,
                //required: true
            },
            "Valid_from": {
                date: true
            },
            "Valid_through": {
                date: true,
            },
            "Extension": {
                digits: true
                //,
                //required: true
            },
            "Expire": {
                date: true
            },
            "Observation": {
                date: true,
            },


        },
        errorElement: "span",
        wrapper: "a",  // a wrapper around the error message
        errorPlacement: function (error, element) {
            error.next().addClass("tooltips");
            error.addClass("tooltips");
            error.insertAfter(element);
        }
    });

});

var loadInfo = function () {
    $.ajax({
        "url": serverPrefix + "CustomerContract/GetContract/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId,
            
        },
        "success": function (data) {
            if (data != "0") {
                var items = JSON.parse(data);
                var $ttForm = $("#table-form");
                var $inputs = $ttForm.find(":input");
                var dataLen = Object.keys(items).length;
                for (var i = 0; i < dataLen; i++) {
                    $inputs.each(function () {
                        var $formInput = $(this);
                        // Match input name with json property name, if a match set form value to prop value;
                        if ($formInput.attr("name") == Object.keys(items)[i]) {

                            $formInput.val(items[$formInput.attr("name")]);

                            if ($formInput.attr("name") == "Status") {
                                oldStatus = items[$formInput.attr("name")];
                            }

                        }
                    });
                }
            }

        }
    });


    
}