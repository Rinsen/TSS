$(document).ready(function () {

    $("#contractHead-modal-button").click(function () {
        
        $("#contractHeadModal").appendTo("body").modal("show").find('.modal-content').draggable();
        loadData();
    });

    $("#save-head-changes").click(function () {
        if ($("#contract-head-modal form").valid()) {
            var head = {}
            head["Buyer"] = $("#buyer3").val();
            head["Address"] = $("#address3").val();
            head["Zip_code"] = $("#zip-code3").val();
            head["Corporate_identity_number"] = $("#corporate-id3").val();
            head["City"] = $("#city3").val();
            head["Contact_person"] = $("#contact-person3").val();
            head["Contract_id"] = $("#contract-id3").val();
            head["Customer"] = $("#customer3").val();
            head["Customer_sign"] = $("#customer_sign3").val();
            head["Our_sign"] = $("#our_sign3").val();

            $.ajax({
                "url": serverPrefix + "CustomerContract/SaveContractHead/",
                "type": "POST",
                "data": {
                    "json": JSON.stringify(head),
                    "customer": customerName,
                    "contract-id": contractId
                },
                "success": function (data) {
                    if (data > 0) {

                        console.log("success");
                        
                        $.ajax({
                            "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_HeaderSection",
                            "type": "GET",
                            "success": function (data) {
                                $(".crm-pdf-header-section").html(data); 
                                $.ajax({
                                    "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_SigningSection",
                                    "type": "GET",
                                    "success": function (data) {
                                        $("#crm-pdf-signing-section").html(data);
                                    }
                                });
                                $("#contractHeadModal").modal("hide");
                                triggerAlert("Successfully updated this contract head", "success");
                            }
                        });
                    }
                    else {
                        console.log("failure");
                        triggerAlert("Something went wrong when trying to update the contract head on the server", "warning");
                    }
                }
            });
        }

        
    });

    $formValidation = $("#contractHeadModal form").validate({
        ignore: ".ignore",
        rules: {
            "Zip_code": {
                required: true,
                maxlength: 6
            },
            "Corporate_identity_number": {
                required: true,
                maxlength: 11
            },
            "City": {
                required: true,
                maxlength: 25
            },
            "Contact_person": {
                required: true,
                maxlength: 100
            },
            "Buyer": {
                maxlength: 255
            },
            "Address": {
                maxlength: 40
            },
            "Customer_sign": {
                maxlength: 50
            },
            "Our_sign": {
                maxlength: 50
            }

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

var loadData = function () {
    
    $("#contract-id3").val(contractId);
    $("#customer3").val(customerName);

    $.ajax({
        "url": serverPrefix + "CustomerContract/GetContractHead/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId
        },
        "success": function (data) {
            if (data != "0") {
                var contractHead = JSON.parse(data);
                $.ajax({
                    "url": serverPrefix + "CustomerContract/GetContactPersons/",
                    "type": "POST",
                    "data": {
                        "customer": customerName,
                        
                    },
                    "success": function (data2) {
                        if (data2 != "0") {


                            var contactPersons = JSON.parse(data2);
                            $("#buyer3").val(contractHead.Buyer);
                            $("#address3").val(contractHead.Address);
                            $("#zip-code3").val(contractHead.Zip_code);
                            $("#corporate-id3").val(contractHead.Corporate_identity_number);
                            $("#city3").val(contractHead.City);
                            $("#customer_sign3").val(contractHead.Customer_sign);
                            $("#our_sign3").val(contractHead.Our_sign);

                            $("#contact-person3").empty();
                            for (var i = 0; i < contactPersons.length; i++) {
                                var person = contactPersons[i];
                                $("#contact-person3").append($("<option></option>").attr("innerHtml", person).html(person));

                            }
                            
                            $("#contact-person3").val(contractHead.Contact_person);
                        }

                    }
                });
              
            }
            
        }
    });
    
};

