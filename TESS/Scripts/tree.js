$.fn.extend({
    treed: function (o) {

        var openedClass = 'glyphicon-minus-sign';
        var closedClass = 'glyphicon-plus-sign';

        if (typeof o != 'undefined') {
            if (typeof o.openedClass != 'undefined') {
                openedClass = o.openedClass;
            }
            if (typeof o.closedClass != 'undefined') {
                closedClass = o.closedClass;
            }
        };

        //initialize each of the top levels
        var tree = $(this);
        tree.addClass("tree");
        tree.find('li').has("ul").each(function () {
            var branch = $(this); //li with children ul
            if (branch.hasClass("feature-item-expanded")) {
                var $listEl = branch;
               
                var icon = $listEl.children('i:first');
                $listEl.children().children().not("span").toggle();
                branch.prepend("<i class='indicator glyphicon " + openedClass + "'></i>");
            }
            else {
            branch.prepend("<i class='indicator glyphicon " + closedClass + "'></i>");
            }
            
            branch.addClass('branch');
            branch.on('click', function (e) {
                if (this == e.target) {
                    var $listEl = $(this);
                    $listEl.toggleClass(function () {
                        if ($(this).parent().is(".feature-item-expanded")) {
                            return "feature-item-not-expanded";
                        } else {
                            return "feature-item-expanded";
                        }
                    });
                    var icon = $listEl.children('i:first');
                    icon.toggleClass(openedClass + " " + closedClass);
                    $listEl.children().children().not("span").toggle();
                }
                $(this).on('tap', function (e) {
                    $(this).closest('li').click();
                    e.preventDefault();
                });
            });
            branch.children().children().not("span").toggle();
        });
        //fire event from the dynamically added icon
        tree.find('.branch .indicator').each(function () {
            $(this).on('click', function () {
                $(this).closest('li').click();
            });
        });
        //fire event to open branch if the li contains an anchor instead of text
        tree.find('.branch>a').each(function () {
            $(this).on('click', function (e) {
                $(this).closest('li').click();
                e.preventDefault();
            });
            $(this).on('tap', function (e) {
                $(this).closest('li').click();
                e.preventDefault();
            });
        });
        //fire event to open branch if the li contains a button instead of text
        tree.find('.branch>button').each(function () {
            $(this).on('click', function (e) {
                $(this).closest('li').click();
                e.preventDefault();
            });
        });
    }
});

//Initialization of treeviews

$('#tree1').treed();
