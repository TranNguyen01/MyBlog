$(document).ready(function () {
    $("form.remove-post-collections-form").on("submit", function (event) {
        event.preventDefault();
        var form = $(this)
        console.log(form)
        $.ajax({
            method: "Delete",
            url: form.attr("action"),
            data: form.serialize(),
            success: function (data) {
                console.log(data)
                if (data.success) {
                    form.closest("div.post-item").remove();
                }
            },
            error: function (error) {
                console.log(error)
            }
        })
    })


    $("form#rename-collection-form").on('submit', function (event) {
        event.preventDefault();
        var form = $(this);
        console.log(form)
        $.ajax({
            method: "PATCH",
            url: form.attr("action"),
            data: form.serialize(),
            success: function (result) {
                var collectionName = $("a.current-collection-name");
                var oldName = collectionName.text()
                collectionName.text(result.collections.name)
                form.closest("div.modal").modal('hide');
                showAlert("success", "", `Đã đổi tên từ "${oldName.trim()}" thành "${result.collections.name}"`)
            },
            error: function (error) {
                console.log(error)
            }
        })
    })

    $("form#new-collection-form").on('submit', function (event) {
        event.preventDefault();
        var form = $(this);
        $.ajax({
            method: "POST",
            url: form.attr("action"),
            data: form.serialize(),
            success: function (result) {
                console.log(result)
                form.closest("div.modal").modal('hide');
                showAlert("success", "", `Đã tạo thư mục "${result.name}"`)
                var container = $("div.child-collections-container")
                container.removeClass('d-none')
                var item = `        
                    <div class="col-12 col-md-6 col-lg-4 pb-2">
                        <a href="/Collections/${result.id}" class="text-decoration-none">
                            <div class="btn btn-light border border-secondary rounded d-flex flex-row align-items-center">
                                <div class="py-1 px-3">
                                    <i class="fa-duotone fa-folder"></i>
                                </div>
                                <div class="text-muted pr-1 text-truncate">
                                    ${result.name}
                                </div>
                            </div>
                        </a>
                    </div>`
                container.append(item)
            },
            error: function (error) {
                console.log(error)
            }
        })
    })

    $("form#delete-collection-form").on('submit', function (event) {
        event.preventDefault();
        var form = $(this);
        console.log(form)
        $.ajax({
            method: "DELETE",
            url: form.attr("action"),
            data: form.serialize(),
            success: function (result) {
                if (result.success) {
                    window.location.href = result.redirectUrl;
                }
            },
            error: function (error) {
                console.log(error)
            }
        })
    })
})