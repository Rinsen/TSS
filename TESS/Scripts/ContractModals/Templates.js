$(document).ready(function () {

    var frm = $("#templatesModal form");

    $("#textTemplate-modal-button").click(function () {
        
        if (isMainCont == "True") {
            $("#main-template-number-select").val("current");
            $("#mainTemplatesModal").appendTo("body").modal("show").find('.modal-content').draggable();
            loadTopHeadText();
        }
        else {
            loadSelectData();
            loadTextData();
            $("#templatesModal").appendTo("body").modal("show").find('.modal-content').draggable();
        }
       
    });

    $("#template-number-select").bind("change", function () {
        loadTextData();
    });
    $("#main-template-number-select").bind("change", function () {
        loadPrologText();
        loadEpilogText();
        loadTopHeadText();
        loadModuleText();
    });
    $("#save-template-changes").click(function () {
        if ($("#template-modal form").valid()) {
            var text = {};
            text["Contract_type"] = $("#document-type-text").val();
            text["Title"] = $("#title-text").val();
            //text["Page_head"] = $("#page-head-text").val();
            text["Page_head"] = tinymce.get('page-head-text').getContent();
            //text["Document_foot"] = $("#document-foot-text").val();
            text["Document_foot"] = tinymce.get('document-foot-text').getContent();
            text["Module_info"] = tinymce.get('module-info-text').getContent();
            text["Document_foot_title"] = $("#bodytitle").val(),
            text["Delivery_maint_title"] = tinymce.get("deluhtitle").getContent(),
            //text["Delivery_maint_text"] = $("#deluhtext").val()
            text["Delivery_maint_text"] = tinymce.get('deluhtext').getContent();
            text["Contract_id"] = contractId;
            text["Customer"] = customerName;
            text["Page_foot"] = "";
            text["Document_head"] = "";


            $.ajax({
                "url": serverPrefix + "CustomerContract/SaveContractText/",
                "type": "POST",
                "data": {
                    "customer": customerName,
                    "contract-id": contractId,
                    "json": JSON.stringify(text)
                },
                "success": function (data) {


                    if (data > 0) {

                        console.log("success");
                        $.each(text, function (key, val) {
                            var $updateTarget = $("#template-" + key);
                            if ($updateTarget.length > 0) {
                                $updateTarget.html(val);
                            }
                        })

                        $("#templatesModal").modal("hide");
                        triggerAlert("Successfully updated this contract text", "success");
                    }
                    else {
                        console.log("failure");
                        triggerAlert("Something went wrong when trying to update the contract text on the server", "warning");
                    }


                }
            });
        };
    });

    $("#save-main-template-changes").click(function () {

        $.ajax({
            "url": serverPrefix + "CustomerContract/SaveMainContractText/",
            "type": "POST",
            "data": {
                "customer": customerName,
                "contract-id": contractId,
                "epilog": tinymce.get('epilog-text').getContent(),
                "prolog": tinymce.get('prolog-text').getContent(),
                "tophead": $("#main-top-head").val(),
                "moduleText": tinymce.get('module-text').getContent()

            },
            success: function (data) {
                if (data > 0) {
                    console.log("Sucseess");
                    $(".crm-pdf-main-contract-epilog-section").html(tinymce.get('epilog-text').getContent());
                    $(".crm-pdf-main-contract-header-prolog").html(tinymce.get('prolog-text').getContent());
                    $(".crm-pdf-module-section-text").html(tinymce.get('module-text').getContent());
                    $("#main-top-title-text").html($("#main-top-head").val());
                    $(document).trigger("clear-alerts");
                    $(document).trigger("add-alerts", [
                      {
                          'message': "Successfully updated values",
                          'priority': 'success'
                      }
                    ]);
                    $("#mainTemplatesModal").modal("hide");
                }
                else {
                    $(document).trigger("clear-alerts");
                    $(document).trigger("add-alerts", [
                      {
                          'message': "Something went wrong when trying to update, some data might not been saved",
                          'priority': 'warning'
                      }
                    ]);
                }


            }

        });
    });

    $formValidation = $("#templatesModal form").validate({
        rules: {
            "DeluhTitle": {
                maxlength: 255
            },
            "Document_foot": {
                maxlength: 255
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


var loadTextData = function (templateID) {


    $.ajax({
        "url": serverPrefix + "CustomerContract/GetTemplateText/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId,
            "selected": $("#template-number-select").val(),
        },
        "success": function (data) {
            if (data != "0") {

                var template = JSON.parse(data);
                if (typeof template.Document_type == "undefined") {
                    $("#document-type-text").val(template.Contract_type);
                }
                else {
                    $("#document-type-text").val(template.Document_type);
                }
                
                $("#title-text").val(template.Title);
                $("#page-head-text").val(template.Page_head);
                $("#document-foot-text").val(template.Document_foot);
                $("#module-info-text").val(template.Module_info);
                $("#bodytitle").val(template.Document_foot_title);
                $("#deluhtitle").val(template.Delivery_maint_title);
                $("#deluhtext").val(template.Delivery_maint_text);

                for (var i = 0; i < tinyMCE.editors.length; i++) {
                    var currentED = tinyMCE.editors[i];
                    currentED.setContent($('#' + currentED.id).val());
                }


            }

        }
    });
};


var loadSelectData = function () {


    $.ajax({
        "url": serverPrefix + "CustomerContract/GetTemplates/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId,
            
        },
        "success": function (data) {
            if (data != "0") {
                var numbers = JSON.parse(data);
                $("#template-number-select option").remove("option.added");
                for (var i = 0; i < numbers.length; i++) {
                    var id = numbers[i].Key;
                    var selectVal = numbers[i].Value
                    $("#template-number-select").append($("<option class='added'></option>").attr("value", selectVal).html(id));
                }
                
                $('.selectpicker').selectpicker('refresh');
            }

        }
    });

};

var loadPrologText = function () {
    
    $.ajax({
        "url": serverPrefix + "CustomerContract/GetProlog/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId,
            "from": $("#main-template-number-select").val()
        },
        "success": function (data) {
            if (data != "0") {
     
                tinyMCE.get('prolog-text').setContent(data);

            }

        }
    });

}

var loadModuleText = function () {


    $.ajax({
        "url": serverPrefix + "CustomerContract/GetModuleText/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId,
            "from": $("#main-template-number-select").val()
        },
        "success": function (data) {
            if (data != "0") {

                tinyMCE.get('module-text').setContent(data);


            }

        }
    });
}

var loadEpilogText = function () {

    
    $.ajax({
        "url": serverPrefix + "CustomerContract/GetEpilog/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId,
            "from": $("#main-template-number-select").val()
        },
        "success": function (data) {
            if (data != "0") {
       
                tinyMCE.get('epilog-text').setContent(data);


            }

        }
    });
}

var loadTopHeadText = function () {


    $.ajax({
        "url": serverPrefix + "CustomerContract/GetTopHead/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId,
            "from": $("#main-template-number-select").val()
        },
        "success": function (data) {
            if (data != "0") {

                $("#main-top-head").val(data);


            }

        }
    });
}