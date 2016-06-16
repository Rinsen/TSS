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
            "customer": customerName

        },
        "success": function (data) {
            if (data.length > 0) {
                var classifications = JSON.parse(data);
                var classificationsLen = Object.keys(classifications).length;
                $classificationSelect.empty();
                for(var i = 0 ; i < classificationsLen ; i++){
                    $classificationSelect.append($("<option></option>").attr("value",classifications[i]).html(classifications[i]));           
                }
                fillArticleList(System, $classificationSelect.val());
            }
        }
    });
}

//Hämtar samtliga gällande artiklar och markerar de i kontraktet befintliga 
var fillArticleSearchList = function (System, classification) {
    var $searchText = $("#art-search");
    $.ajax({
        "url": serverPrefix + "CustomerContract/GetModulesAll/",
        "type": "POST",
        "data": {
            "customer": customerName,
            "searchtext": $searchText.val(),
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
            "contracttype": ctr
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

var editSelectArticle = function (editButton) {
    var $editButton = $(editButton);
    var $articleBtn = $editButton.closest("button");
    var oldLicenseVal = parseFloat($articleBtn.data("license")).toFixed(2);
    var oldMaintenanceVal = parseFloat($articleBtn.data("maintenance")).toFixed(2);
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

                    calculateSums();
                    $(".small-modal").remove();
                },
            },
        }
    });
}

var editArticle = function(editButton){
    var $editButton = $(editButton);
    var $articleBtn = $editButton.parent().parent().parent().find("td button");
    var oldLicenseVal = Math.round($articleBtn.data("license"), 0);
    var oldMaintenanceVal = Math.round($articleBtn.data("maintenance"), 0);
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
        if (article.Used == true) {
            usedCell = "<td><span class='glyphicon glyphicon-ok'></span></td>";
        }
        var $newButton;
        if (article.System == "Lärportal") {
            if (!hasFixedRows) {
                $tr = $availableList.parent().find("table").find("tbody").find("tr");
                console.log($tr);
                $tr.html("<th></th><th>Art. nr</th><th>Module</th><th>Price category</th>");
                hasFixedRows = true;
            }
            $newButton = $("<button onclick='moveItem(event, this)'                                             \
                                            class='list-group-item art-nr-" + article.Article_number + "'           \
                                            data-selected='false'                                                   \
                                            data-maintenance='" + article.Price_category + "'                       \
                                            type='button'>                                                          \
                                    <table>                                                                         \
                                        <tr>                                                                        "
                                        + usedCell +
                                           "<td class='art-nr' title='" + artClass + "'>" + article.Article_number + "</td>                  \
                                            <td title = '" + artComm + "'>" + article.Module + "</td>                                         \
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
            $newButton = $("<button onclick='moveItem(event, this)'                                             \
                                            class='list-group-item art-nr-" + article.Article_number + "'           \
                                            data-selected='false'                                                   \
                                            data-license='" + article.License + "'                                  \
                                            data-maintenance='" + article.Maintenance + "'                          \
                                            type='button'>                                                          \
                                    <table>                                                                         \
                                        <tr>                                                                        "
                                        + usedCell +
                                           "<td class='art-nr' title='" + artClass + "'>" + article.Article_number + "</td>                  \
                                            <td title = '" + artComm + "'>" + article.Module + "</td>                                         \
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
var calculateSums = function(){
    var $selectedArticleButtons = $("#articlesModal #selected-articles button[data-rowtype!=2]");
    var SACLen =  $selectedArticleButtons.length;
    var LicenseTotal = 0,
        MaintenanceTotal = 0;
    for(var i = 0; i < SACLen; i++){
        var $btn = $($selectedArticleButtons[i]);
        var license = (typeof $btn.data("license") != 'undefined' ? $btn.data("license") : 0);
        var maintenance = (typeof $btn.data("maintenance") != 'undefined' ? $btn.data("maintenance") : 0);
        LicenseTotal += parseFloat(license);
        MaintenanceTotal += parseFloat(maintenance); 
    }
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
            for (var i = 0; i < length; i++) {
                module = modules[i];
                if (module.System == "Lärportal") {
                    $selectedArticles.append("                                                      \
                        <button onclick='moveItem(event, this)'                                     \
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
                        <button onclick='moveItem(event, this)'                                     \
                                type='button'                                                       \
                                class='list-group-item'                                             \
                                data-selected='true'                                                \
                                data-license='" + module.License + "'                               \
                                data-maintenance='" + module.Maintenance + "'>                      \
                            <table>                                                                 \
                                <tr>                                                                \
                                    <td class='art-nr'>" + module.Article_number + "</td>");
                    if (module.Rewritten) {
                        if (module.NewArt) {
                            $selectedArticles.append("<td>New " + module.Article + "</td>");
                        }
                        if (module.Removed) {
                            $selectedArticles.append("<td> Del " + module.Article + "</td>");
                        }
                    }
                    else 
                    {
                        $selectedArticles.append("<td>" + module.Article + "</td>");
                    }
                    $selectedArticles.append("<td>" + module.Article + "</td>                       \
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

var changePrice = function (event, element) {
    console.log("Ändra poris")
}
// Move list item from either available-articles to selected-articles or
// the other way around.
var moveItem = function(event, element){
    $button = $(element.closest('button'));
    $arttext = $(element);
    //$arttext = $(element.firstElementChild.firstElementChild.firstElementChild.firstElementChild.nextElementSibling);

    $availableArticles = $("#articlesModal #available-articles");
    $selectedArticles = $("#articlesModal #selected-articles");

    var buttonArt = $button.find(".art-nr").html();
    var buttonLicense = $button.data("license");
    var buttonMaintenance = $button.data("maintenance");
    var buttonid = $button.attr("id");
    

    if($button.attr("data-selected") == "false") { 
        $newButton = $button.clone();
        // Fix to exclude the "used" checkmark on selected items.
        $($newButton).find("td").get(0).remove();
           
        $button.prop("disabled", true);
        $newButton.attr("data-selected", "true");
        $newButton.attr("data-rowtype", "3");
        $newButton.attr("type", "button");
        $newButton.find('.license').html(formatCurrency(buttonLicense));
        $newButton.find('.maintenance').html(formatCurrency(buttonMaintenance));
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
                //    "url": "/CustomerContract/ToggleRewritten/",
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
                    //    "url": "/CustomerContract/ToggleRewritten/",
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
}
// Function to show a highlight effect on movement between lists
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

var saveArticlesFunction = function () {
    $("#choose-selected-articles").button('loading');
    var selectedArticlesArray = [];
    var $selectedList = $("#articlesModal #selected-articles button");
    var selectedListLen = $selectedList.length;
    for(var i = 0; i < selectedListLen; i++)
    {
        var $button = $($selectedList[i]);
        var buttonArt = $button.find(".art-nr").html();
        var buttonLicense = $button.data("license");
        var buttonMaintenance = $button.data("maintenance");
        var buttonRowtype = $button.data("rowtype");

        // "Create a new article" to store in an array to use for server side db update.
        var newArticle = {
            "Article_number":  buttonArt,
            "License": buttonLicense,
            "Maintenance": buttonMaintenance,
            "Rowtype" : buttonRowtype
        }
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
                // Success to update db. Now update the preview section.
                $.ajax({
                    "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_OldModuleSection",
                    "type": "GET",
                    "success": function (data) {
                        $(".crm-pdf-old-module-section").html(data);
                        updateDoneAjax(0);
                    }
                });

                $.ajax({
                    "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_ModuleTerminationSection",
                    "type": "GET",
                    "success": function (data) {
                        $(".crm-pdf-moduletermination-section").html(data);
                        updateDoneAjax(1);
                    }
                });

                $.ajax({
                    "url": serverPrefix + "CustomerContract/ViewPdf?contract-id=" + contractId + "&customer=" + customerName + "&contract-section=_ModuleSection",
                    "type": "GET",
                    "success": function (data) {
                        $(".crm-pdf-module-section").html(data);
                        updateDoneAjax(2);
                    }
                });
            }
            else{
                console.log(data);
                triggerAlert("Failed to add articles", "warning");
            }
        }
    })
}

doneAjax = [false,false,false];
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
        $button.find('.maintenance').html(formatCurrency(0));
        $button.find('.license').html(formatCurrency(0));

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
        $("#articlesModal").appendTo("body").modal("show");
    });

    $("#articlesModal #choose-selected-articles").click(function () {
        saveArticlesFunction();
    });
    $("#articlesModal #sum-zero-button").click(function () {
        zeroArticlesFunction();
    });
});

