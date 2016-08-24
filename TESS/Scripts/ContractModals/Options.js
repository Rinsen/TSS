$(document).ready(function () {
    $("#options-modal-button").click(function () {
        $("#optionsModal").appendTo("body").modal("show").find('.modal-content').draggable();
    });
    $("#choose-selected-options").click(function () {
        saveOptions();
    });

    $("#opt-search-button").click(function () {
        fillArticleOptSearchList();
    });

    $('#opt-search').keypress(function (e) {
        if (e.keyCode == 13) {
            $('#opt-search-button').click();
            return false;
        }
    });
    $SystemOptionSelect = $("#optionsModal #System-select");
    fillOptionsClassificationSelect($SystemOptionSelect.val());
    $classificationOptionSelect = $("#optionsModal #classification-select");

    // Change content of classificationSelect on change
    $SystemOptionSelect.change(function () {
        var $el = $(this);
        var System = $el.val();
        fillOptionsClassificationSelect(System);
    });

    // Change content of available-articles on change
    $classificationOptionSelect.change(function () {
        var $el = $(this);
        var classification = $el.val();
        var System = $SystemSelect.val();
        fillOptionsArticleList(System, classification);
    });
});

// Global variables
var $SystemOptionSelect = $("#optionsModal #System-select");
var $classificationOptionSelect = $("#optionsModal #classification-select");

// Fuction to fill the classifications select element with options corresponding to the correct
// System.
var fillOptionsClassificationSelect = function (System) {
    console.log("asd");
    $.ajax({
        "url": serverPrefix + "CustomerOffer/JsonData/",
        "type": "POST",
        "data": {
            "requestData": "update_classification_select",
            "System": System,
            "customer": customerName,
            "Area": area

        },
        "success": function (data) {
            if (data.length > 0) {
                var selectOptions = JSON.parse(data);
                $classificationOptionSelect.empty();
                $.each(selectOptions, function (i, $selectOption) {
                    $classificationOptionSelect.append($("<option></option>").val($selectOption.Value).text($selectOption.Text));
                });
                fillOptionsArticleList(System, $classificationOptionSelect.val());
            }
            $classificationOptionSelect.selectpicker('refresh');
        }
    });
}


var fillArticleOptSearchList = function () {
    var $searchText = $("#opt-search");
    console.log("Search");
    $.ajax({
        "url": serverPrefix + "CustomerContract/GetModulesAll/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "searchtext": $searchText.val()
        },
        "success": function (data) {
            if (data.length > 0) {
                var articles = JSON.parse(data);
                var $availableList = $("#optionsModal #available-options");
                var $selectedList = $("#optionsModal #selected-options");
                $availableList.empty();
                handleExistingOption(articles, $availableList, $selectedList);
            }
        }
    });
}

// Function to fill the article list depending on chosen System and classification
var fillOptionsArticleList = function (System, classification) {
    $.ajax({
        "url": serverPrefix + "CustomerContract/GetModules/",
        "type": "POST",
        "data": {
            "System": System,
            "classification": classification,
            "customer": customerName
        },
        "success": function (data) {
            if (data.length > 0) {
                var articles = JSON.parse(data);
                var $availableList = $("#optionsModal #available-options");
                var $selectedList = $("#optionsModal #selected-options");
                $availableList.empty();
                handleExistingOption(articles, $availableList, $selectedList);
            }
        }
    });
}

var editOption = function (editButton) {
    var $editButton = $(editButton);
    var $articleBtn = $editButton.parent().parent().parent().find("td button");
    var oldLicenseVal = parseInt($articleBtn.data("license"));
    var oldMaintenanceVal = parseInt($articleBtn.data("maintenance"));
    var articleName = $articleBtn.find(".art-nr").next().html();
    var articleNr = $articleBtn.find(".art-nr").html();
    var licenseText = "";
    if (typeof $articleBtn.data("license") != "undefined" && typeof $articleBtn.data("license") != false) {
        licenseText = " <div class='form-group'>                                                                                                    \
                                <label for='license-text' class='col-sm-2 control-label'>License</label>                                                \
                                <div class='col-sm-10'>                                                                                                 \
                                    <input class='form-control' id='license-text' name='License' value='" + oldLicenseVal + "'>                         \
                                </div>                                                                                                                  \
                            </div>  ";
    }
    bootbox.dialog({
        backdrop: false,
        closebutton: false,
        className: "small-modal",
        title: "Edit Article:  " + articleNr + " " + articleName,
        message: "<form class='form-horizontal'>                                                                                                \                                                                                                                    \
                        <div class='form-group'>                                                                                                    \
                            <label for='maintenance-text' class='col-sm-2 control-label'>Maintenance</label>                                        \
                            <div class='col-sm-10'>                                                                                                 \
                                <input class='form-control' id='maintenance-text' name='Maintenance' value='" + oldMaintenanceVal + "'>             \
                            </div>                                                                                                                  \
                        </div>                                                                                                                      \
                        " + licenseText + "                                                                                                        \
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
                    var $licenseEl = $("#license-text");
                    var $maintenanceEl = $("#maintenance-text");
                    // Update article data attrs
                    if (typeof $licenseEl != "undefined" && typeof $licenseEl != false) {
                        $articleBtn.attr("data-license", $licenseEl.val());
                        $articleBtn.data("license", $licenseEl.val());
                        $articleBtn.find(".license").html(formatCurrency($licenseEl.val()));
                    }
                    $articleBtn.attr("data-maintenance", $maintenanceEl.val());
                    $articleBtn.data("maintenance", $maintenanceEl.val());
                    $articleBtn.find(".maintenance").html(formatCurrency($maintenanceEl.val()));

                    $(".small-modal").remove();
                },
            },
        }
    });
}

// Function to disable a button element if it already exists in the selected list.
var handleExistingOption = function (availableArticles, $availableList, $selectedList) {
    var aaLen = availableArticles.length;
    var $artNrs = $selectedList.find("button .art-nr");
    var artNrsLen = $artNrs.length;
    var hasFixedRows = false;
    for (var i = 0; i < aaLen; i++) {
        var article = availableArticles[i];
        var usedCell = "<td></td>";
        if (article.Used == true) {
            usedCell = "<td><span class='glyphicon glyphicon-ok'></span></td>";
        }
        var artComm = ((article.Comment == '') ? "Hjälptext saknas" : article.Comment);
        var artClass = article.System + " / " + article.Classification;
        var $newButton;
        if (article.System == "Lärportal") {
            if (!hasFixedRows) {
                $tr = $availableList.parent().find("table").find("tbody").find("tr");
                console.log($tr);
                $tr.html("<th></th><th>Art. nr</th><th>Module</th><th>Price category</th>");
                hasFixedRows = true;
            }
            $newButton = $("<button onclick='moveOption(event, this)'                                             \
                                            class='list-group-item art-nr-" + article.Article_number + "'           \
                                            data-selected='false'                                                   \
                                            data-maintenance='" + article.Price_category + "'                       \
                                            type='button'>                                                          \
                                    <table>                                                                         \
                                        <tr>                                                                        "
                                        + usedCell +
                                           "<td class='art-nr' title='" + artClass + "'>" + article.Article_number + "</td>                  \
                                            <td title='" + artComm + "'>" + article.Module + "</td>                                         \
                                            <td class='maintenance' style='float: right; width:auto;'>" + formatCurrency(article.Price_category) + "</td>\
                                        </tr>                                                                       \
                                    </table>                                                                        \
                                </button>");
        }
        else {
            if (!hasFixedRows) {
                $tr = $availableList.parent().find("table").find("tbody").find("tr");
                $tr.html("<th></th><th>Art. nr</th><th>Module</th><th>License</th><th>Maintenance</th>");
                hasFixedRows = true;
            }
            $newButton = $("<button onclick='moveOption(event, this)'                                             \
                                            class='list-group-item art-nr-" + article.Article_number + "'           \
                                            data-selected='false'                                                   \
                                            data-license='" + article.License + "'                                  \
                                            data-maintenance='" + article.Maintenance + "'                          \
                                            type='button'>                                                          \
                                    <table>                                                                         \
                                        <tr>                                                                        "
                                        + usedCell +
                                           "<td class='art-nr' title='" + artClass + "'>" + article.Article_number + "</td>                  \
                                            <td title='" + artComm + "'>" + article.Module + "</td>                                         \
                                            <td class='license'>" + formatCurrency(article.License) + "</td>        \
                                            <td class='maintenance'>" + formatCurrency(article.Maintenance) + "</td>\
                                        </tr>                                                                       \
                                    </table>                                                                        \
                                </button>");
        }
        var $buttonContainer = $("<table>                                                                                               \
                                    <tr>                                                                                                \
                                        <td style='width: 10px'>                                                                        \
                                            <div style='margin-right: 1em; cursor: pointer'>                                            \
                                                <span onclick='editOption(this)' class='glyphicon glyphicon-pencil'></span>            \
                                            </div>                                                                                      \
                                        </td>                                                                                           \
                                        <td style='width: auto' class='module-item-container'>                                          \
                                                                                                                                        \
                                        </td>                                                                                           \
                                    </tr>                                                                                               \
                                    </table>");
        var $mic = $buttonContainer.find(".module-item-container");
        if (artNrsLen > 0) {
            for (var j = 0; j < artNrsLen; j++) {
                var selVal = $artNrs.get(j);
                var $selectedButton = $(selVal).parent().parent().parent();
                var selectedButtonArt = $(selVal).html();

                if (article.Article_number == selectedButtonArt) {
                    $mic.append($newButton.prop("disabled", true));

                    $availableList.append($buttonContainer);
                } else {
                    $mic.append($newButton);

                    $availableList.append($buttonContainer);
                }
            }
        } else {
            $mic.append($newButton);

            $availableList.append($buttonContainer);
        }
    }

}


//
var calculateOptionSums = function () {
    var $selectedArticleButtons = $("#selected-options button");
    var SACLen = $selectedArticleButtons.length;
    var LicenseTotal = 0,
        MaintenanceTotal = 0;
    for (var i = 0; i < SACLen; i++) {
        var $btn = $($selectedArticleButtons[i]);
        var license = (typeof $btn.data("license") != 'undefined' ? $btn.data("license") : 0);
        var maintenance = (typeof $btn.data("maintenance") != 'undefined' ? $btn.data("maintenance") : 0);
        LicenseTotal += parseInt(license);
        MaintenanceTotal += parseInt(maintenance);
    }
    $("#options-license-total").html(formatCurrency(LicenseTotal));
    $("#options-maintenance-total").html(formatCurrency(MaintenanceTotal));
}

// Array to store objects of selected articles
var selectedOptionsArray = [];

var removeOption = function (arr, articleNr) {
    for (var i = arr.length; i--;) {
        if (arr[i].Article_number == articleNr) {
            arr.splice(i, 1);
        }
    }
}

var updateSelectedOptions = function () {
    $.ajax({
        "url": serverPrefix + "CustomerContract/GetSelectedOptions/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId,
        },
        "success": function (data) {
            var modules = JSON.parse(data);
            var length = modules.length;
            for (var i = 0; i < length; i++) {
                module = modules[i];
                if (module.System == "Lärportal") {
                    $selectedArticles.append("                                                      \
                        <button onclick='moveOption(event, this)'                                     \
                                type='button'                                                       \
                                class='list-group-item'                                             \
                                data-selected='true'                                                \
                                data-maintenance='" + module.Maintenance + "'>                      \
                            <table>                                                                 \
                                <tr>                                                                \
                                    <td class='art-nr'>" + module.Article_number + "</td>           \
                                    <td>" + module.Article + "</td>                                 \
                                    <td style='float: right; width:auto;'>" + module.Price_category + "</td>                          \
                                </tr>                                                               \
                            </table>                                                                \
                        </button>                                                                   \
                        ");
                }
                else {
                    $selectedArticles.append("                                                      \
                        <button onclick='moveOption(event, this)'                                     \
                                type='button'                                                       \
                                class='list-group-item'                                             \
                                data-selected='true'                                                \
                                data-license='" + module.License + "'                               \
                                data-maintenance='" + module.Maintenance + "'>                      \
                            <table>                                                                 \
                                <tr>                                                                \
                                    <td class='art-nr'>" + module.Article_number + "</td>           \
                                    <td>" + module.Article + "</td>                                 \
                                    <td>" + module.License + "</td>                                 \
                                    <td>" + module.Maintenance + "</td>                             \
                                </tr>                                                               \
                            </table>                                                                \
                        </button>                                                                   \
                        ");
                }
            }
        }
    });
}

// Move list item from either available-articles to selected-articles or
// the other way around.
var moveOption = function (event, element) {
    $button = $(element);

    $availableArticles = $("#optionsModal #available-options");
    $selectedArticles = $("#optionsModal #selected-options");

    var buttonArt = $button.find(".art-nr").html();
    var buttonLicense = $button.data("license");
    var buttonMaintenance = $button.data("maintenance");

    if ($button.attr("data-selected") == "false") {
        $newButton = $button.clone();
        // Fix to exclude the "used" checkmark on selected items.
        $($newButton).find("td").get(0).remove();

        $button.prop("disabled", true);
        $newButton.attr("data-selected", "true");
        $newButton.attr("type", "button");
        $newButton.find('.license').html(formatCurrency(buttonLicense));
        $newButton.find('.maintenance').html(formatCurrency(buttonMaintenance));
        $newButton.appendTo($selectedArticles);
        calculateOptionSums();
    } else {
        $availableArticles.find(".art-nr-" + buttonArt).prop("disabled", false);
        $button.attr("data-selected", "false");
        $button.remove();
        calculateOptionSums();
    }
}
// Function to show a highlight effect on movement between lists
var highlightOption = function ($item, css) {
    $item.addClass(css)
    setTimeout(function () {
        $item.removeClass(css);
        $item.addClass("highlight-item-fade");
        setTimeout(function () {
            $item.removeClass("highlight-item-fade");
        }, 600);
    }, 600);
}

var saveOptions = function () {
    var selectedOptionsArray = [];
    var $selectedList = $("#selected-options button");
    var selectedListLen = $selectedList.length;
    for (var i = 0; i < selectedListLen; i++) {
        var $button = $($selectedList[i]);
        var buttonArt = $button.find(".art-nr").html();
        var buttonLicense = $button.data("license");
        var buttonMaintenance = $button.data("maintenance");

        // "Create a new article" to store in an array to use for server side db update.
        var newArticle = {
            "Article_number": buttonArt,
            "License": buttonLicense,
            "Maintenance": buttonMaintenance
        }
        selectedOptionsArray.push(newArticle);
    }

    $.ajax({
        "url": serverPrefix + "CustomerContract/UpdateContractOptions/",
        "type": "POST",
        "data": {
            "Object": JSON.stringify(selectedOptionsArray),
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
                // Success to update db. Now update the preview section.
                $.ajax({
                    "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_ModuleSection",
                    "type": "GET",
                    "success": function (data) {
                        $(".crm-pdf-module-section").html(data);
                        $("#optionsModal").modal("hide");
                        triggerAlert("Successfully added options", "success");
                    }
                });
            }
            else {
                console.log(data);
                triggerAlert("Failed to add options", "success");
            }
        }
    })
}
