let prevObject = null;

function navigateToTreeElement(jObject) {
    if (prevObject != null) {
        $(prevObject).removeClass("focus");
    }

    $(jObject).addClass("focus");
}

function focusTreeMemberByCurrentHash() {
    let href = location.href;

    if (!href.includes("#")) {
        return;
    }
    
    let target = href.split("#");

    if (target.length > 1) {
        let element = $("li#"+target[1]);

        if (element.length == 0) {
            throw new Error("target doc element not found");
        }
        
        $(element).trigger("click");
        navigateToTreeElement(element);

        prevObject = element;

        while (element.length > 0) {
            if ($(element).attr("aria-expanded") != null) {
                $(element).attr("aria-expanded", "true");
            }
            
            element = $(element).parent();
        }
    }
}

// needs to be window.addEventListener
// instead of $(document).ready()
// because TreeLinks.js doesn't use jquery
// and i need below to run after that thing
// and i'm not touching the code i stole
window.addEventListener('load', function () {
    $("li[doc-file]").each(function() {
        $(this).on("click", null, $(this).attr("doc-file"), 
            function(ev) {
                navigateToTreeElement(this);
                $("#main-doc-content").load(ev.data);
            }
        );
    });

    focusTreeMemberByCurrentHash();

    $(window).on("hashchange", function(ev) {
        focusTreeMemberByCurrentHash();
    });
});