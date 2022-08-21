$(document).ready(function () {
    let topNavSearchBtn = $("#top-nav-search-btn")
    topNavSearchBtn.on("click", function () { 
       
        let input = $("#top-nav-search-input");
        if (input.hasClass("expanded")) {
            if (input.val() == "") {
                event.preventDefault();
                input.removeClass("expanded")
                topNavSearchBtn.addClass("text-light")
            } else {

            } 
        } else {
            event.preventDefault();
            input.addClass("expanded")
            topNavSearchBtn.removeClass("text-light")
        }
    })


    let openCateogryBtn = $("#open-category-popup")
    openCateogryBtn.on("click", function () {
        let popup = $("#category-popup");
        if (popup.hasClass("active")) {
            popup.removeClass("active")
            openCateogryBtn.empty();
            openCateogryBtn.append('<i class="fas fa-ellipsis-h fa-2xl"></i>')
            document.body.style.overflow = 'visible'
        } else {    
            popup.addClass("active")
            document.body.style.overflow = 'hidden'
            openCateogryBtn.empty();
            
            openCateogryBtn.append('<i class="fa-solid fa-xmark fa-xl"></i>')
        }
        
    })
})