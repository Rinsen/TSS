﻿
$(document).ready(function () {

    var oldStatus = "";
    $("#head-info-modal-button").click(function () {
        $("#tableItemsModal").appendTo("body").modal("show").find('.modal-content').draggable();
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
       $('.selectpicker').selectpicker('refresh');
    });

    $("#orginfoid-text").change(function (element) {
        //Set default org checkbox depending on OrgInfo-select
        var sel = document.getElementById("orginfoid-text");

        if (sel.selectedIndex > -1) {
            var text = sel.options[sel.selectedIndex].text;

            if (text != null && text.indexOf("Default") > -1) {
                document.getElementById("defaultOrg").checked = true;
            } else {
                document.getElementById("defaultOrg").checked = false;
            }
        } 
    });

    var reCalcObservationDate = function () {
        if (!isNaN($("#term_of_notice-text").val())) {      // Om Term Of Notice är numeriskt (Not icke-numeriskt)
            if (isNaN($("#valid_through-date").val())) {    //Om inte valid_through är numeriskt (dvs ett datum) ska det beräknas
                var myDate = new Date($("#valid_through-date").val());
                myDate.setMonth(myDate.getMonth() - $("#term_of_notice-text").val());
                $("#observation-date").val($.datepicker.formatDate('yy-mm-dd', myDate));
            }
            else {
                if (isNaN($("#expire-date").val())) {    //Om inte Expire är numeriskt (dvs ett datum) ska det beräknas
                    var myDate = new Date($("#expire-date").val());
                    myDate.setMonth(myDate.getMonth() - $("#term_of_notice-text").val());
                    $("#observation-date").val($.datepicker.formatDate('yy-mm-dd', myDate));
                }
            }
        }
    }

    $("#term_of_notice-text").change(function () {
        reCalcObservationDate();
    });

    $("#valid_through-date").change(function () {
        reCalcObservationDate();
    });


    $("#contract_type-text").change(function () {
        
        if ($(this).val() == "Huvudavtal") {
            $("#main_contract_id-text").prop("disabled", "disabled");
        }
        else {
            $("#main_contract_id-text").prop("disabled", false);
            $("#main_contract_id-text").selectpicker('refresh');
        }
    });


    $("#save-table-changes").click(function () {
        if ($("#table-form").valid()) {
            var $ttForm = $("#table-form");
            var $inputs = $ttForm.find(":input");
            var hashtags = "";

            var contract = {};
            $inputs.each(function () {
                var $formInput = $(this);
                contract[$formInput.attr("name")] = $formInput.val();
                if ($formInput.attr("name") == "Contract_id") {
                    contractId = $formInput.val();
                }
                else if($formInput.attr("name") == "Hashtags"){
                    hashtags = $formInput.val();
                }
                else if ($formInput.attr("name") == "Summera") {
                    contract["Summera"] = (($formInput.prop("checked")) ? "1" : "0");
                }
                else if ($formInput.attr("name") == "DefaultOrg") {
                    contract["DefaultOrg"] = (($formInput.prop("checked")) ? "1" : "0");
                }

            });
            $.ajax({
                "url": serverPrefix + "CustomerContract/UpdateTableInfo/",
                "type": "POST",
                "data": {
                    "customer": customerName,
                    "hashtags": hashtags,
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
                            window.location = serverPrefix + "/CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&our_sign=" + our_sign;
                        }
                        
                        //window.location = "/CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName;
                        //$.ajax({
                        //    "url": serverPrefix + "CustomerContract/ViewPdf?selected-contract=" + ssma_timestamp + "&contract-section=_HeaderSection",
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
                maxlength: 100
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
                $inputs.each(function () {
                    var $formInput = $(this);
                    var formInputName = $formInput.attr("name");
                    if (items.hasOwnProperty(formInputName)) {
                        if (formInputName == "Status") {
                            oldStatus = items[formInputName];
                            //$formInput.find('option[value="' + formInputName + '"]').prop("selected", true);
                            $formInput.val(items[formInputName]).change();
                        } else if (formInputName == "Contract_type" || formInputName == "Sign" || formInputName == "Area") {
                            //$("#list option[value=" + selectValue + "]").text();
                            //$formInput.find('option[value="' + formInputName + '"]').prop("selected", true);
                            //$formInput.find('option[value="' + items[formInputName] + '"]').attr("selected", true);
                            $formInput.val(items[formInputName]).change();
                        } else if (formInputName == "Summera") {
                            $formInput.prop("checked", (items[formInputName] == 1 ? true : false));
                        } else if (formInputName == "OrgInfoId") {
                            var sel = document.getElementById("orginfoid-text");
                            if (sel.selectedIndex > -1) {
                                var text = sel.options[sel.selectedIndex].text;

                                if (text != null && text.indexOf("Default") > -1) {
                                    document.getElementById("defaultOrg").checked = true;
                                } else {
                                    document.getElementById("defaultOrg").checked = false;
                                }
                            }

                            $formInput.val(items[formInputName]).change();
                        } else {
                            $formInput.val(items[formInputName]);
                        }
                    }
                    else if (formInputName == "Hashtags") {
                        var hashtags = items["_HashtagList"];
                        var tags = "";
                        var length = hashtags.length;
                        for (var i = 0; i < length; i++) {
                            tag = hashtags[i];
                            tags += "#" + tag + " ";
                        }
                        if (tags.length > 0)
                        {
                            $formInput.val(tags.substr(0, tags.length - 1));
                        } 
                        else
                            $formInput.val("");
                    }
                });
                $('.selectpicker').selectpicker('refresh');
                }
            }
        });


    
}