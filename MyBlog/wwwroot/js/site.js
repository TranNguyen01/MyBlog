$(document).ready(function () {
    function showAlert(type,title, message) {
        let alert = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <strong>${title}</strong> ${message}
                <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
                </button>
           </div>`
        $("#alert-container").append(alert);
    }


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

    $(".check-menu").on('click', "input[type=checkbox]", function (event) {
        $(this).closest("label").toggleClass("active", this.checked);
        
        let menu = $(this).closest("div.check-menu");
        let checkedbox = menu.find("input:checked");
        let textBtn = "";
        checkedbox.each(function () {
            textBtn += textBtn == "" ? $(this).val() :`, ${$(this).val()}`;
        });

        let button = $(this).closest("div.dropdown").children("button.dropdown-toggle");
        button.text(textBtn);
    })

    $("button.submit-role-btn").on('click', function () {
        let form = $(this).closest('tr').find("form");
        var submitBtn = $(this)
        var cancelBtn = $(this).parent().children(".edit-role-btn")
        $.ajax({
            type: "POST",
            url: $(form).attr('action'),
            data: $(form).serialize(),
            success: function (data) {
                console.log(data);
                if (data.success) {
                    showAlert("success", "Thành công!", "Cập nhật thành công") 
                    console.log(cancelBtn)
                    $(cancelBtn).removeClass("editing")
                    $(cancelBtn).empty();
                    $(cancelBtn).append(`<i class="fa-solid fa-pen-to-square"></i>`)
                    $(submitBtn).addClass("d-none")
                    $(submitBtn).closest("tr").find("button.dropdown-toggle").addClass("disabled")
                } else {
                    showAlert("Error", "Không Thành công!", "Cập nhật không thành công")
                }
                    
            },
            error: function (error) {
                console.log(error)
                showAlert("Error", "Không Thành công!", "Cập nhật không thành công")
            }
        });
    })

    $("button.edit-role-btn").on("click", function () {
        if ($(this).hasClass("editing")) {
            $(this).removeClass("editing")
            $(this).empty();
            $(this).append(`<i class="fa-solid fa-pen-to-square"></i>`)
            $(this).parent().children(".submit-role-btn").addClass("d-none")
            $(this).closest("tr").find("button.dropdown-toggle").addClass("disabled")
        } else {
            $(this).addClass("editing")
            $(this).empty();
            $(this).append(`<i class="fa-solid fa-xmark"></i>`)
            $(this).parent().children(".submit-role-btn").removeClass("d-none")
            $(this).closest("tr").find("button.dropdown-toggle").removeClass("disabled")
            
        }

    })

    $(".check-menu").on('click', function (event) {
        event.stopPropagation()
    })
    
})