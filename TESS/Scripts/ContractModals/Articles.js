// Global variables
var $SystemSelect = $("#articlesModal #System-select");
var $classificationSelect = $("#articlesModal #classification-select");
var ctr = "";

// Fuction to fill the classifications select element with options corresponding to the correct
// System.
var fillClassificationSelect = function(System){
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
                $classificationSelect.empty();
                $.each(selectOptions, function(i, selectOption){
                    var $optionEl = $("<option></option>").val(selectOption.Value).html(selectOption.Text);
                    $classificationSelect.append($optionEl);
                    if(i == selectOptions.length-1)
                        fillArticleList($SystemSelect.val(), $classificationSelect.val());
                });
            }
            $classificationSelect.selectpicker('refresh');
        }
    });
    $classificationSelect.selectpicker('refresh');
}

//Hämtar samtliga gällande artiklar och markerar de i kontraktet befintliga 
var fillArticleSearchList = function () {
    var $searchText = $("#art-search");
    console.log($searchText);
    $.ajax({
        "url": serverPrefix + "CustomerContract/GetModulesAll/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "searchtext": $searchText.val(),
            "moduletype": 1, //Articles
            "contracttype": ctr,
            "contractid": contractId
        },
        "success": function (data) {
            if (data.length > 0) {
                var articles = JSON.parse(data);
                var $availableList = $("#articlesModal #available-articles");
                var $selectedList = $("#articlesModal #selected-articles");
                $availableList.empty();
                handleExistingArticle(articles, $availableList, $selectedList);
            }
        }
    });
}

// Function to fill the article list depending on chosen System and classification
var fillArticleList = function(System, classification){
    $.ajax({
        "url": serverPrefix + "CustomerContract/GetModules/",
        "type": "POST",
        "data": {
            "System": System,
            "classification": classification,
            "customer": customerName,
            "contracttype": ctr,
            "contractid": contractId
        },
        "success": function (data) {
            if (data.length > 0) {
                var articles = JSON.parse(data);
                var $availableList = $("#articlesModal #available-articles");
                var $selectedList = $("#articlesModal #selected-articles");
                $availableList.empty();
                handleExistingArticle(articles, $availableList, $selectedList);
            } 
        }
    });
}


var editArticle = function(editButton){
    var $editButton = $(editButton);
    var $articleBtn = $editButton.parent().parent().parent().find("td button");
    var oldLicenseVal, oldMaintenanceVal;
    if ($articleBtn.data("discount") != '1' || $articleBtn.data("discount-type") == '0') {
        oldLicenseVal = parseFloat($articleBtn.data("license")).toFixed(2);
        oldMaintenanceVal = parseFloat($articleBtn.data("maintenance")).toFixed(2);
    }
    else {
        oldLicenseVal = parseInt($articleBtn.data("license"));
        oldMaintenanceVal = parseInt($articleBtn.data("maintenance"));
    }
    var oldAlias = $articleBtn.data("alias");
    var articleName = $articleBtn.find(".art-nr").next().html();
    var articleNr = $articleBtn.find(".art-nr").html();
    var licenseText = "";
    if (typeof $articleBtn.data("license") != "undefined" && typeof $articleBtn.data("license") != false) {
        licenseText = " <div class='form-group'>                                                                                                        \
                                <label for='license-text' class='col-sm-2 control-label'>License</label>                                                \
                                <div class='col-sm-10'>                                                                   \
                                    <input class='form-control' id='license-text' name='License' value='" + oldLicenseVal + "'>                         \
                                    <input class='form-control' id='license-percent-text' name='License_percent' value='0%'>                          \
                                    <button type='button' class='btn btn-search' id='license-percent-button'>-</button>                  \
                                </div>                                                                                                                  \
                            </div>  ";
    }
    var aliasText = " <div class='form-group'>                                                                                                          \
                                <label for='license-text' class='col-sm-2 control-label'>Alias</label>                                                  \
                                <div class='col-sm-10'>                                                                                                 \
                                    <input class='form-control' id='alias-text' name='Alias' value='" + oldAlias + "'>                                  \
                                </div>                                                                                                                  \
                            </div>  ";
    bootbox.dialog({
        backdrop: false,
        closebutton: false,
        className: "small-modal",
        title: "Edit Article:  " + articleNr + " " + articleName,
        message: "<form class='form-horizontal'>                                                                                                \                                                                                                                    \
                        <div class='form-group'>                                                                                                    \
                            <label for='maintenance-text' class='col-sm-2 control-label'>Maintenance</label>                                        \
                            <div class='col-sm-10'>                                                                   \
                                <input class='form-control' id='maintenance-text' name='Maintenance' value='" + oldMaintenanceVal + "'>             \
                                <input class='form-control' id='maintenance-percent-text' name='Maintenance_percent' value='0%'>                  \
                                <button type='button' class='btn btn-search' id='maintenance-percent-button'>-</button>                  \
                            </div>                                                                                                                  \
                        </div>                                                                                                                      \
                        " + licenseText + "                                                                                                        \
                      " + aliasText + " </form>"
            ,
        buttons: {
            close: {
                label: "Close",
                className: "btn-default",
                callback: function () {
                    $(".small-modal").remove();
                }
            },
            success: {
                label: "Save",
                className: "btn-primary",
                callback: function () {
                    var $licenseEl = $("#license-text");
                    var $maintenanceEl = $("#maintenance-text");
                    var $aliasEl = $("#alias-text");
                    if ($licenseEl.val() == "") { $licenseEl.val("0") };
                    if ($maintenanceEl.val() == "") {$maintenanceEl.val("0")};
                    // Update article data attrs
                    if (typeof $licenseEl != "undefined" && typeof $licenseEl != false) {
                        $articleBtn.attr("data-license", $licenseEl.val());
                        $articleBtn.data("license", $licenseEl.val());
                        if ($articleBtn.data("discount") != '1' || $articleBtn.data("discount-type") == '0')
                            $articleBtn.find(".license").html(formatCurrencyNoKr($licenseEl.val()));
                        else
                            $articleBtn.find(".license").html($licenseEl.val() + "%");
                    }
                    $articleBtn.attr("data-maintenance", $maintenanceEl.val());
                    $articleBtn.data("maintenance", $maintenanceEl.val());
                    if ($articleBtn.data("discount") != '1' || $articleBtn.data("discount-type") == '0')
                        $articleBtn.find(".maintenance").html(formatCurrencyNoKr($maintenanceEl.val()));
                    else
                        $articleBtn.find(".maintenance").html($maintenanceEl.val() + "%");

                    $articleBtn.attr("data-alias", $aliasEl.val());
                    $articleBtn.data("alias", $aliasEl.val());
                    $articleBtn.find(".alias").html($aliasEl.val());


                    $(".small-modal").remove();
                },
            },
        }
    });

    $("#maintenance-percent-button").click(function () {
        var $maintenanceEl = $("#maintenance-text");
        $maintenanceEl.val(calculateValue($("#maintenance-percent-text").val(), $maintenanceEl.val()));
    });

    $("#license-percent-button").click(function () {
        var $licenseEl = $("#license-text");
        $licenseEl.val(calculateValue($("#license-percent-text").val(), $licenseEl.val()));
    });
}

// Function to disable a button element if it already exists in the selected list.
var handleExistingArticle = function(availableArticles, $availableList, $selectedList){
    var aaLen = availableArticles.length;
    var $artNrs = $selectedList.find("button .art-nr");
    var artNrsLen = $artNrs.length;
    var hasFixedRows = false;
    for (var i = 0; i < aaLen; i++) {
        var article = availableArticles[i];
        var artComm = ((article.Comment == '') ? "Hjälptext saknas" : article.Comment);
        var artClass = article.System + " / " + article.Classification;
        var usedCell = "<td></td>";
        var usedDep = "<td></td>";

        if (article.Expired == true) {
            usedCell = "<td><span class='glyphicon glyphicon-star'></span></td>";
        }
        else if (article.Used == true) {
            usedCell = "<td><span class='glyphicon glyphicon-ok'></span></td>";
        }
        if (article.HasDependencies || (article.Description != null && article.Description.length) > 0 || article.Module_status != "0") {
            var depTitle = "";

            if (article.Module_status_txt.length > 0 && article.Module_status != "0") {
                depTitle = "Restriction:\n";
                depTitle += article.Module_status_txt;
                depTitle += "\n";
            }

            if (article.HasDependencies) {
                if (depTitle.length > 0) {
                    depTitle += "\n\n";
                }

                var depLen = article.Dependencies.length;
                depTitle += "Depends on:\n";

                var depArticle;
                for (var d = 0; d < depLen; d++) {
                    depArticle = article.Dependencies[d];
                    depTitle += " " + depArticle.Article_number + ": " + depArticle.Module + "\n";
                }
            }

            if (article.Description.length > 0) {
                if (depTitle.length > 0) {
                    depTitle += "\n";
                }
                depTitle += "Important info:\n";
                depTitle += article.Description;
            }
            
            usedDep = "<td title='" + depTitle + "'><span class='glyphicon glyphicon-exclamation-sign'></span></td>";
        }
        var $newButton;
        if (article.System == "Lärportal") {
            if (!hasFixedRows) {
                $tr = $availableList.parent().find("table").find("tbody").find("tr");
                console.log($tr);
                $tr.html("<th></th><th></th><th>Art. nr</th><th>Module</th><th>Price category</th>");
                hasFixedRows = true;
            }
            $newButton = $("<button onclick='moveItem(event, this)'                                                 \
                                            class='list-group-item art-nr-" + article.Article_number + "'           \
                                            data-selected='false'                                                   \
                                            data-maintenance='" + article.Price_category + "'                       \
                                            data-status='" + article.Module_status + "'                             \
                                            data-status-text='" + article.Module_status_txt + "'                    \
                                            data-alias='" + article.Module + "'                                     \
                                            data-discount-type='" + article.Discount_type + "'                      \
                                            data-discount='" + article.Discount + "'                                \
                                            data-multiple-select='" + article.Multiple_type + "'                    \
                                            data-read-name-from-module='" + article.Read_name_from_module + "'      \
                                            data-module-text-id='" + article.Module_text_id + "'                    \
                                            data-contract-description='" + article.Contract_Description + "'        \
                                            data-contract-id='" + article.Contract_id + "'                          \
                                            data-automapping='" + article.IncludeDependencies + "'                  \
                                            type='button'>                                                          \
                                    <table>                                                                         \
                                        <tr>                                                                        "
                                        + usedCell + usedDep +
                                           "<td class='art-nr' title='" + artClass + "'>" + article.Article_number + "</td>                  \
                                            <td class='alias' title = '" + artComm + "'>" + article.Module + "</td>                                         \
                                            <td class='maintenance' style='float: right; width:auto;'>" + formatCurrencyNoKr(article.Price_category) + "</td>\
                                        </tr>                                                                       \
                                    </table>                                                                        \
                                </button>");
        }
        else {
            if (!hasFixedRows) {
                $tr = $availableList.parent().find("table").find("tbody").find("tr");
                $tr.html("<th></th><th></th><th>Art. nr</th><th>Module</th><th>License</th><th>Maintenance</th>");
                hasFixedRows = true;
            }
            var button = "";
            button += "<button onclick='moveItem(event, this)'                                             \
                                            class='list-group-item art-nr-" + article.Article_number + "'           \
                                            data-selected='false'                                                   \
                                            data-discount-type='" + article.Discount_type + "'                      \
                                            data-discount='" + article.Discount + "'                                \
                                            data-license='" + article.License + "'                                  \
                                            data-maintenance='" + article.Maintenance + "'                          \
                                            data-status='" + article.Module_status + "'                             \
                                            data-status-text='" + article.Module_status_txt + "'                    \
                                            data-alias='" + article.Module + "'                                     \
                                            data-multiple-select='" + article.Multiple_type + "'                    \
                                            data-read-name-from-module='" + article.Read_name_from_module + "'      \
                                            data-module-text-id='" + article.Module_text_id + "'                    \
                                            data-contract-description='" + article.Contract_Description + "'        \
                                            data-contract-id='" + article.Contract_id + "'                          \
                                            data-automapping='" + article.IncludeDependencies + "'                  \
                                            type='button'>                                                          \
                                    <table>                                                                         \
                                        <tr>                                                                        "
                                        + usedCell + usedDep +
                                       "<td class='art-nr' title='" + artClass + "'>" + article.Article_number + "</td>                  \
                                            <td class='alias' title = '" + artComm + "'>" + article.Module + "</td>                                         \
                                            ";
            if (article.Discount_type != '1') {
                button += "<td class='license'>" + formatCurrencyNoKr(article.License) + "</td>        \
                            <td class='maintenance'>" + formatCurrencyNoKr(article.Maintenance) + "</td>";
            }
            else {
                button += "<td class='license'>" + article.License + "%</td>        \
                            <td class='maintenance'>" + article.Maintenance + "%</td>";
            }

            button += "</tr>                                                                       \
                                    </table>                                                                        \
                                </button>";
            $newButton = $(button);
        }
        var $buttonContainer = $("<table>                                                                                               \
                                        <tr>                                                                                                \
                                            <td style='width: 10px'>                                                                        \
                                                <div style='margin-right: 1em; cursor: pointer'>                                            \
                                                    <span onclick='editArticle(this)' class='glyphicon glyphicon-pencil'></span>            \
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

                if (article.Article_number == selectedButtonArt && article.Multiple_type != "1") {
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
var calculateSums = function(){
    var $selectedArticleButtons = $("#articlesModal #selected-articles button[data-rowtype!=2]");
    var SACLen =  $selectedArticleButtons.length;
    var LicenseTotal = 0,
        MaintenanceTotal = 0;
    var discountArr = new Array();
    for(var i = 0; i < SACLen; i++){
        var $btn = $($selectedArticleButtons[i]);
        var license = (typeof $btn.data("license") != 'undefined' ? $btn.data("license") : 0);
        var maintenance = (typeof $btn.data("maintenance") != 'undefined' ? $btn.data("maintenance") : 0);
        if ($btn.data("discount") == '1'){
            var type = $btn.data("discount-type");
            var discountObj = { "License": license, "Maintenance": maintenance, "Discount_type": type };
            discountArr.push(discountObj);
        }
        else {
            LicenseTotal += parseFloat(license);
            MaintenanceTotal += parseFloat(maintenance);
        }
    }
    var daLenght = discountArr.length;
    var discountL = {"Percentage": 0, "Money": 0};
    var discountM = { "Percentage": 0, "Money": 0 };
    for (var i = 0; i < daLenght; i++) {
        if (discountArr[i].Discount_type == "1") {
            discountL.Percentage += parseInt(discountArr[i].License);
            discountM.Percentage += parseInt(discountArr[i].Maintenance);
        }
        else {
            discountL.Money += parseFloat(discountArr[i].License);
            discountM.Money += parseFloat(discountArr[i].Maintenance);
        }

    }

    if (discountL.Percentage < -100)
        discountL.Percentage = -100;
    if (discountM.Percentage < -100)
        discountM.Percentage = -100;

    LicenseTotal += discountL.Money;
    MaintenanceTotal += discountM.Money;
    LicenseTotal += LicenseTotal * discountL.Percentage / 100;
    MaintenanceTotal += MaintenanceTotal * discountM.Percentage / 100;
    $("#articlesModal #article-license-total").html(formatCurrency(LicenseTotal));
    $("#articlesModal #article-maintenance-total").html(formatCurrency(MaintenanceTotal));
}

// Array to store objects of selected articles
var selectedArticlesArray = [];

var removeArticle = function(arr, articleNr) {
    for (var i = arr.length; i--;) {
        if (arr[i].Article_number == articleNr) {
            arr.splice(i, 1);
        }
    }
}

var updateSelectedItems = function () {
    $availableArticles = $("#articlesModal #available-articles");
    $selectedArticles = $("#articlesModal #selected-articles");
    $.ajax({
        "url": serverPrefix + "CustomerContract/GetSelectedModules/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "contract-id": contractId,
        },
        "success": function (data) {
            var modules = JSON.parse(data);
            var length = modules.length;
            $selectedArticles.html("");
            for (var i = 0; i < length; i++) {
                module = modules[i];
                if (module.System == "Lärportal") {
                    $selectedArticles.append("                                                      \
                        <button onclick='moveItem(event, this)'                                     \
                                type='button'                                                       \
                                class='list-group-item'                                             \
                                data-selected='true'                                                \
                                data-maintenance='" + module.Maintenance + "'                       \
                                data-status='" + module.Module_status + "'                          \
                                data-alias='" + module.Module + "'                                  \
                                data-discount='" + module.Discount + "'                             \
                                data-discount-type='" + module.Discount_type + "'                   \
                                data-multiple-select='" + module.Multiple_type + "'                 \
                                data-read-name-from-module='" + module.Read_name_from_module + "'   \
                                data-automapping='" + module.IncludeDependencies + "'               \
                                data-rowtype='3'>                                                   \
                            <table>                                                                 \
                                <tr>                                                                \
                                    <td class='art-nr'>" + module.Article_number + "</td>           \
                                    <td class='alias'>" + module.Module + "</td>                    \
                                    <td style='float: right; width:auto;'>" + module.Price_category + "</td>                          \
                                </tr>                                                               \
                            </table>                                                                \
                        </button>                                                                   \
                        ");
                }
                else {
                    var html = "";
                    html += "                                                                       \
                        <button onclick='moveItem(event, this)'                                     \
                                type='button'                                                       \
                                class='list-group-item'                                             \
                                data-selected='true'                                                \
                                data-alias='" + module.Module + "'                                  \
                                data-license='" + module.License + "'                               \
                                data-maintenance='" + module.Maintenance + "'                       \
                                data-status='" + module.Module_status + "'                          \
                                data-discount='" + module.Discount + "'                             \
                                data-discount-type='" + module.Discount_type + "'                   \
                                data-automapping='" + module.IncludeDependencies + "'               \
                                data-rowtype='3'>                                                   \
                            <table>                                                                 \
                                <tr>                                                                \
                                    <td class='art-nr'>" + module.Article_number + "</td>";
                    if (module.Rewritten) {
                        if (module.NewArt) {
                            html += "<td class='alias'>New " + module.Module + "</td>";
                        }
                        else if (module.Removed) {
                            html += "<td class='alias'> Del " + module.Module + "</td>";
                        }
                        else {
                            html += "<td class='alias'>" + module.Module + "</td>";
                        }
                    }
                    else {
                        html += "<td class='alias'>" + module.Module + "</td>";
                    }
                    if (module.Discount != '1' || module.Discount_type == '0') {
                        html += "<td>" + formatCurrencyNoKr(module.License) + "</td>                                \
                                    <td>" + formatCurrencyNoKr(module.Maintenance) + "</td>                             \
                                </tr>                                                               \
                            </table>                                                                \
                        </button>                                                                   \
                        ";
                    }
                    else {
                        html += "<td>" + module.License + "%</td>                                \
                                    <td>" + module.Maintenance + "%</td>                             \
                                </tr>                                                               \
                            </table>                                                                \
                        </button>                                                                   \
                        ";
                    }
                    $selectedArticles.append(html);
                }
            }
            calculateSums();
        }
    });
}

var changePrice = function (event, element) {
    console.log("Ändra pris");
};

// Move list item from either available-articles to selected-articles or
// the other way around.
var moveItem = function (event, element) {
    $button = $(element.closest('button'));
    $arttext = $(element);
    //$arttext = $(element.firstElementChild.firstElementChild.firstElementChild.firstElementChild.nextElementSibling);

    $availableArticles = $("#articlesModal #available-articles");
    $selectedArticles = $("#articlesModal #selected-articles");

    var buttonArt = $button.find(".art-nr").html();
    var buttonLicense = $button.data("license");
    var buttonStatus = $button.data("status");
    var buttonStatusTxt = $button.data("status-text");
    var buttonMaintenance = $button.data("maintenance");
    var buttonid = $button.attr("id");
    var buttonHasServiceDependencies = $button.data("automapping");


    if ($button.attr("data-selected") == "false") {

        if (buttonStatus > 0) {
            //Show dialog
            if (!confirm("Article with restriction! Continue?\n\nRestriction:\n" + buttonStatusTxt)) {
                return;
            }
        }

        $newButton = $button.clone();
        // Fix to exclude the "used" checkmark on selected items.
        $($newButton).find("td").get(0).remove();
        // Fix to exclude the "dep" cell on selected items.
        $($newButton).find("td").get(0).remove();
        if ($button.attr("data-multiple-select") != "1") {
            $button.prop("disabled", true);
        }
        $newButton.attr("data-selected", "true");
        $newButton.attr("data-rowtype", "3");
        $newButton.attr("type", "button");

        if ($button.data("discount") != '1' || $button.data("discount-type") == '0') {
            $newButton.find('.license').html(formatCurrency(buttonLicense));
            $newButton.find('.maintenance').html(formatCurrency(buttonMaintenance));
        }
        else {
            $newButton.find('.license').html(buttonLicense + "%");
            $newButton.find('.maintenance').html(buttonMaintenance + "%");
        }

        if (buttonHasServiceDependencies && confirm('Add dependencies automatically to the contract?')) {
            // Save it!
            $newButton.attr("data-automapping", "true");
        } else {
            $newButton.attr("data-automapping", "false");
        }

        $newButton.appendTo($selectedArticles);
        calculateSums();
    } else {
        // Normal rowtype
        if ($button.attr("data-rowtype") == "3") {
            $availableArticles.find(".art-nr-" + buttonArt).prop("disabled", false);
            $button.attr("data-selected", "false");
            $button.remove();
        }
        else {
            // Removed module
            if ($button.attr("data-rowtype") == "2") {
                $button.attr("data-rowtype", "1");
                $arttext.removeClass("RewrittenRemoved");
                $arttext.addClass("RewrittenRewritten");
                var cont = document.getElementById(buttonid);
                var cont1 = cont.innerHTML;
                $button.empty().append(cont1);

                //$.ajax({
                //    "url": serverPrefix + "CustomerContract/ToggleRewritten/",
                //    "type": "POST",
                //    "data": {
                //        "contractid": contractId,
                //        "customer": customerName,
                //        "artnr": buttonArt,
                //    },
                //    "success": function (data) {
                //        if (data > 0) {
                //            console.log("success");
                //        }
                //        else {
                //            console.log("failure");
                //        }
                //    }
                //});
            }
            else {
                // Rewritten module
                if ($button.attr("data-rowtype") == "1") {
                    $button.attr("data-rowtype", "2");
                    $arttext.removeClass("RewrittenRewritten")
                    $arttext.addClass("RewrittenRemoved");
                    var cont = document.getElementById(buttonid);
                    var cont1 = cont.innerHTML;
                    cont.innerHTML = cont1;
                    //$.ajax({
                    //    "url": serverPrefix + "CustomerContract/ToggleRewritten/",
                    //    "type": "POST",
                    //    "data": {
                    //        "contractid": contractId,
                    //        "customer": customerName,
                    //        "artnr": buttonArt,
                    //    },
                    //    "success": function (data) {
                    //        if (data > 0) {
                    //            console.log("success");
                    //        }
                    //        else {
                    //            console.log("failure");
                    //        }
                    //    }
                    //});
                }
            }
        }
        calculateSums();
    }
};

// Function to show a highlight effect on movement between lists
var highlightItem = function ($item, css) {
    $item.addClass(css)
    setTimeout(function () {
        $item.removeClass(css);
        $item.addClass("highlight-item-fade");
        setTimeout(function () {
            $item.removeClass("highlight-item-fade");
        }, 600);
    }, 600);
};

var saveArticlesFunction = function () {
    $("#choose-selected-articles").button('loading');
    var selectedArticlesArray = [];
    var $selectedList = $("#articlesModal #selected-articles button");
    var selectedListLen = $selectedList.length;
    for (var i = 0; i < selectedListLen; i++) {
        var $button = $($selectedList[i]);
        var buttonArt = $button.find(".art-nr").html();
        var buttonLicense = $button.data("license");
        var buttonMaintenance = $button.data("maintenance");
        var buttonAlias = $button.data("alias");
        var buttonRowtype = $button.data("rowtype");
        var buttonDiscount = $button.data("discount");
        var buttonContractDescription = $button.data("contract-description");
        var buttonModuleTextId = $button.data("module-text-id");
        var buttonContractId = $button.data("contract-id");
        var buttonAutoMapping = $button.data("automapping");

        // "Create a new article" to store in an array to use for server side db update.
        var newArticle = {
            "Article_number": buttonArt,
            "Alias": buttonAlias,
            "License": buttonLicense,
            "Maintenance": buttonMaintenance,
            "Rowtype": buttonRowtype,
            "Discount_type": buttonDiscount,
            "Contract_description": buttonContractDescription,
            "Module_text_id": buttonModuleTextId,
            "Contract_id": buttonContractId,
            "Module_type": "A", //Artikel
            "Automapping": buttonAutoMapping
        };

        selectedArticlesArray.push(newArticle);
    }
    $.ajax({
        "url": serverPrefix + "CustomerContract/UpdateContractRows/",
        "type": "POST",
        "data": {
            "Object": JSON.stringify(selectedArticlesArray),
            "customer": customerName,
            "contract-id": contractId,
            "ctrResign": ctrResign,
        },
        dataType: 'text',
        // iOS 6 has a dreadful bug where POST requests are not sent to the
        // server if they are in the cache.
        headers: { 'Cache-Control': 'no-cache' }, // Apple!
        "success": function (data) {
            if (data > 0) {
                //Need to save standard texts to moduleinfo
                $.ajax({
                    "url": serverPrefix + "CustomerContract/SaveModuleInfoTexts/",
                    "type": "POST",
                    "data": {
                        "object": JSON.stringify(selectedArticlesArray)
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
                                    window.location = "ViewPdf?contract-id=" + contractId + "&customer=" + customerName;
                                }
                            });
                        }
                    }
                });

                // Success to update db. Now update the preview section.
                $.ajax({
                    "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_OldModuleSection",
                    "type": "GET",
                    "success": function (data) {
                        $(".crm-pdf-old-module-section").html(data);
                        updateDoneAjax(1);
                    }
                });

                $.ajax({
                    "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_ModuleTerminationSection",
                    "type": "GET",
                    "success": function (data) {
                        $(".crm-pdf-moduletermination-section").html(data);
                        updateDoneAjax(2);
                    }
                });

                //Also update dialogs
                $.ajax({
                    "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_ModuleInfoSection",
                    "type": "GET",
                    "success": function (data) {
                        $(".crm-pdf-module-info-section").html(data);
                        updateDoneAjax(3);
                    }
                });

                window.location = "ViewPdf?contract-id=" + contractId + "&customer=" + customerName;
            }
            else {
                console.log(data);
                $("#choose-selected-articles").button('reset');
                triggerAlert("Failed to add articles", "warning");
            }
        }
    });
};

doneAjax = [false,false,false, false];
var updateDoneAjax = function(ajaxRequestDone)
{
    doneAjax[ajaxRequestDone] = true;
    var check = true

    for(var i = 0; i < doneAjax.length; i++)
    {
        if(doneAjax[i] == true && check)
            check = true;
        else
            check = false;
    }

    if(check)
    {
        $("#articlesModal").modal("hide");
        triggerAlert("Successfully added articles", "success");
        $("#choose-selected-articles").button('reset');
    }
}

var zeroArticlesFunction = function () {
    var selectedArticlesArray = [];
    var $selectedList = $("#articlesModal #selected-articles button");
    var selectedListLen = $selectedList.length;
    for (var i = 0; i < selectedListLen; i++) {
        var $button = $($selectedList[i]);
        var buttonArt = $button.find(".art-nr").html();
        $button.data("license",0);
        $button.data("maintenance",0);
        $button.attr("data-license", 0);
        $button.attr("data-maintenance", 0);
        $button.find('.maintenance').html(formatCurrencyNoKr(0));
        $button.find('.license').html(formatCurrencyNoKr(0));

    }
    calculateSums();
}

$(document).ready(function () {
    $SystemSelect = $("#articlesModal #System-select");
    fillClassificationSelect($SystemSelect.val());
    $classificationSelect = $("#articlesModal #classification-select");
    ctr = contractType.substring(0,1);

    // Change content of classificationSelect on change
    $SystemSelect.change(function () {
        var $el = $(this);
        var System = $el.val();
        fillClassificationSelect(System);
    });

    // Change content of available-articles on change
    $classificationSelect.change(function () {
        var $el = $(this);
        var classification = $el.val();
        var System = $SystemSelect.val();
        fillArticleList(System, classification);
    });

    $("#search-button").click(function () {
        fillArticleSearchList();
    });

    $('#art-search').keypress(function (e) {
        if (e.keyCode == 13) {
            $('#search-button').click();
            return false;
        }
    });

    $("#articles-modal-button").click(function () {
        $("#articlesModal").appendTo("body").modal("show").find('.modal-content').draggable();
    });

    $("#articlesModal #choose-selected-articles").click(function () {
        saveArticlesFunction();
    });
    $("#articlesModal #sum-zero-button").click(function () {
        zeroArticlesFunction();
    });
});

