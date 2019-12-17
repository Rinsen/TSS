$(document).ready(function () {
    $("#services-modal-button").click(function () {
        $("#servicesModal").appendTo("body").modal("show").find('.modal-content').draggable();
    });
    $("#servicesModal #choose-selected-services").click(function () {
        saveFunction();
    });

    $.ajax({
        "url": serverPrefix + "CustomerContract/GetSelectedServices/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId,
        },
        "success": function (data) {
            var servicesData = JSON.parse(data);
            var length = servicesData.length;
            for (var i = 0; i < length; i++) {
                service = servicesData[i];
                
                services.push({
                    id: service.Code,
                    amount: service.Amount,
                    total: service.Total,
                    desc: service.Alias,
                    Contract_description: service.Contract_Description,
                    Contract_id: service.Contract_id,
                    Module_text_id: service.Module_text_id,
                    Module_type: service.Module_type,
                    Article_number: service.Article_number
                });
                setTotalCost(service.Amount, service.Price);
            }
        }
    });
});

var services = [];
var addService = function(code, cost, amount, desc)
{
    if(typeof amount == "undefined")
        amount = 1;

    amount = parseInt(amount);

    for(var i = 0; i < services.length; i++)
    {
        if(services[i].id == code){
            services[i].amount += amount;
            services[i].total += amount * cost;
            services[i].desc = desc;
            if(services[i].amount <= 0)
            {
                services.splice(i, 1);
            }
            break;
        }
    }

    setTotalCost(amount,cost);

}

var setTotalCost = function (amount, cost) {
    cost = parseFloat(cost);
    $h4 = $("#servicesModal #total-cost-container");
    total = parseInt($h4.html().replace("kr","").replace(/\s+/g, ''));
    total += amount*cost;
    if(total < 0)
        total = 0;
    $h4.html(formatCurrency(total));
}

var elementExist = function($element, $list, attr)
{
    $list = $list.find(".list-group-item");
    for(var i = 0; i < $list.length; i++)
    {
        var $item = $($list.get(i));
        if(typeof $item == "undefined")
            return false;

        if($item.attr(attr) == $element.attr(attr))
            return true;
    }
    return false;
}

var highlightItem = function($item, css)
{
    $item.addClass(css)
    setTimeout(function () {
        $item.removeClass(css);
        $item.addClass("highlight-item-fade");
        setTimeout(function () {
            $item.removeClass("highlight-item-fade");
        }, 600);
    }, 600);
}

var editService = function(span)
{
    $span = $(span);
    $tr = $span.parent().parent();
    $label = $tr.find("label");
    $desc = $tr.find("#description-title");
    var desc = $desc.html();
    var value = $label.html();
    value = value.replace(" kr", "");
    value = value.replace(" ", "");
    
    bootbox.dialog({
        backdrop: false,
        closebutton: false,
        className: "small-modal",
        title: "Edit service",
        message: "<form class='form-horizontal'>                                                                                                \
                    <div class='form-group'>                                                                                                    \
                        <label for='license-text' class='col-sm-2 control-label'>Cost</label>                                                   \
                        <div class='col-sm-10'>                                                                                                 \
                            <input class='form-control' id='cost-text' name='Cost' value='" + value + "'>                                       \
                        </div>                                                                                                                  \
                    </div>                                                                                                                      \
                    <div class='form-group'>                                                                                                    \
                        <label for='alias-text' class='col-sm-2 control-label'>Alias</label>                                                    \
                        <div class='col-sm-10'>                                                                                                 \
                            <input class='form-control' id='alias-text' name='Alias' value='" + desc + "'>                                      \
                        </div>                                                                                                                  \
                    </div>                                                                                                                      \
                    </form>"
            ,
        buttons: {
            close: {
                label: "Close",
                className: "btn-close",
                callback: function () {
                    $(".small-modal").remove();
                }
            },
            success: {
                label: "Save",
                className: "btn-success",
                callback: function () {
                    var $newCost = $("#cost-text");
                    if ($newCost.val() == "") { $newCost.val("0") };
                    newCost = formatCurrency($newCost.val());
                    newAlias = $("#alias-text").val()
                    $button = $tr.find("button");
                    var code = $button.attr("data-code");
                    $label.html(newCost);
                    $desc.html(newAlias);
                    $("#selected-services button[data-code='" + code + "'] label").html(newCost);
                    
                    $button.attr("onclick", "newItem(this, " + $("#cost-text").val() + ");");

                    var code = $button.attr("data-code");
                    length = services.length;

                    for(var i = 0; i < length; i++)
                    {
                        var service = services[i];

                        if(service.id == code)
                        {
                            setTotalCost(service.amount * (-1), value);
                            setTotalCost(service.amount, $("#cost-text").val());

                            service.total = $("#cost-text").val() * service.amount;

                        }
                    }
                    $(".small-modal").remove();
                },
            },
        }
    });
}

var newItem = function(element, price){

    $button = $(element);
    $availableServices = $("#available-services");
    $selectedServices = $("#selected-services");

    if($button.attr("data-selected") == "false"){
        if(!elementExist($button, $selectedServices, "data-code")){
            $newButton = $button.clone();
            $newButton.attr("data-selected", "true");
            $newButton.prependTo($selectedServices);

            $amountLabel = $newButton.find(".service-amount");
            $amountLabel.html("1");
            $amountLabel.parent().removeClass("highlight-item-red");
            highlightItem($amountLabel.parent(), "highlight-item-blue");

            setTotalCost(1 ,price);
            services.push({
                id: $newButton.attr("data-code"),
                amount: 1,
                total: price,
                desc: $button.find("#description-title").html(),
                Contract_description: $newButton.attr("data-contract-description"),
                Contract_id: $newButton.attr("data-contract-id"),
                Module_text_id: $newButton.attr("data-module-text-id"),
                Module_type: $newButton.attr("data-module-type"),
                Article_number: $newButton.attr("data-code")
            });
        }
        else{
            $selectedButton = $selectedServices.find(".list-group-item[data-code='" + $button.attr("data-code") + "']");

            $amountLabel = $selectedButton.find(".service-amount");
            var amount = parseInt($amountLabel.html());
            amount += 1;

            $amountLabel.html(amount);
            $amountLabel.parent().removeClass("highlight-item-red");
            highlightItem($amountLabel.parent(), "highlight-item-blue");

            $selectedButton.find("#description-title").html($button.find("#description-title").html());

            addService($button.attr("data-code"), price, 1, $button.find("#description-title").html());
        }
    }
    else{
        $amountLabel = $button.find(".service-amount");
        var amount = parseInt($amountLabel.html());
        if(amount > 1){
            amount -= 1;
            $amountLabel.html(amount);
            $amountLabel.parent().removeClass("highlight-item-blue");
            highlightItem($amountLabel.parent(), "highlight-item-red");

            addService($button.attr("data-code"), price, -1, $button.find("#description-title").html());
        }
        else{
            $button.attr("data-selected", "false");
            addService($button.attr("data-code"), price, -1, $button.find("#description-title").html());
            $button.remove();
        }
    }
}


var updateServiceSelected = function () {
    $selectedServices = $("#selected-services");
    $selectedServices.html("");
    $.ajax({
        "url": serverPrefix + "CustomerContract/GetSelectedServices/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId,
        },
        "success": function (data) {
            var servicesData = JSON.parse(data);
            var length = servicesData.length;
            for (var i = 0; i < length; i++) {
                service = servicesData[i];
                $selectedServices.append("                                                                                              \
                <button style='margin-bottom:25px' type='button' onclick='newItem(this, " + service.Price + ")'         \
                        data-code='" + service.Code + "' data-selected='true' class='list-group-item'  >                               \
                    <label>" + formatCurrency(service.Price) + "</label>                                                                \
                    <span class='service-amount'>" + service.Amount + "</span>                                                          \
                    <br />                                                                                                              \
                    " + service.Description + "                                                                                         \
                </button>                                                                                                               \
                ");
                services.push({
                    id: service.Code,
                    amount: service.Amount,
                    total: service.Total,
                    desc: service.Description,
                    Contract_description: service.Contract_Description,
                    Contract_id: service.Contract_id,
                    Module_text_id: service.Module_text_id,
                    Module_type: service.Module_type,
                    Article_number: service.Article_number
                });
                setTotalCost(service.Amount, service.Price);
            }
        }
    });
}

var saveFunction = function(){

    $.ajax({
        "url": serverPrefix + "CustomerContract/UpdateContractConsultantRows/",
        "type": "POST",
        "data": {
            "requestData": "update_view_Service",
            "object": JSON.stringify(services),
            "customer": customerName,
            "contract-id": contractId,
        },
        dataType: 'text',
        // iOS 6 has a dreadful bug where POST requests are not sent to the
        // server if they are in the cache.
        headers: { 'Cache-Control': 'no-cache' }, // Apple!
        "success": function (data) {
            if (data > 0) {
                console.log("success");

                //Need to save standard texts to moduleinfo
                $.ajax({
                    "url": serverPrefix + "CustomerContract/SaveModuleInfoTexts/",
                    "type": "POST",
                    "data": {
                        "object": JSON.stringify(services)
                    },
                    dataType: 'text',
                    // iOS 6 has a dreadful bug where POST requests are not sent to the
                    // server if they are in the cache.
                    headers: { 'Cache-Control': 'no-cache' }, // Apple!
                    "success": function (data) {
                        if (data > 0) {
                            $.ajax({
                                "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_ModuleSection",
                                "type": "GET",
                                "success": function (data) {
                                    $(".crm-pdf-module-section").html(data);
                                    updateDoneAjax(0);
                                }
                            });
                        }

                        window.location = "ViewPdf?contract-id=" + contractId + "&customer=" + customerName;
                    }
                });
            }
            else
                triggerAlert("Failed to add articles", "success");
        }
    })
}
