$(document).ready(function () {
    //$("#submit_comment_btn").on('click', function () {
    //    event.preventDefault();
    //    var form = $("#new_comment_form")
    //    console.log(form.serialize())

    //})

    $("#new_comment_form").submit(function (e) {
        e.preventDefault();
        var form = $(this);
        var actionUrl = form.attr('action');  
        $.ajax({
            type: "POST",
            url: actionUrl,
            data: form.serialize(), 
            success: function (data) {
                console.log(data);
                if (data.success)
                    location.reload(true)
            },
            error: function (error) {
                console.log(error)
            }
        });

    });

    $(".delete-form").submit(function (e) {

        e.preventDefault();

        var form = $(this);
        var actionUrl = form.attr('action');

        $.ajax({
            type: "Delete",
            url: actionUrl,
            data: form.serialize(),
            success: function (data) {
                console.log(data);
                if (data.success)
                    location.reload(true)
            },
            error: function (error) {
                console.log(error)
            }
        });

    })

    $("#like-form").submit(function (e) {
        e.preventDefault();

        var form = $(this);
        var actionUrl = form.attr('action');
        console.log($(this).children('.like-btn i'))
        var icon = $(this).find('.like-btn i')
        if (icon.hasClass('fa-light')) {
            icon.removeClass('fa-light')
            icon.addClass('fa-solid')
            icon.addClass('text-primary')

           
        } else {
            icon.addClass('fa-light')
            icon.removeClass('fa-solid')
            icon.removeClass('text-primary')
        }    
       
        $.ajax({
            type: "Post",
            url: actionUrl,
            data: form.serialize(),
            success: function (data) {
                console.log(data);
            },
            error: function (error) {
                console.log(error)
                console.log(error.status)
                if (error.status == 401) {
                    window.location.replace('/login')
                }
            }
        })
    })

    $("#bookmark_btn").on('click', fetchBookmarkData)

    function fetchBookmarkData() {
        var actionUrl = "/Collections/GetAllCollections"
        if ($(this).hasClass('active')) {
            $(this).removeClass('active')
            $(this).parent().children('.collections_container').remove()
        } else {
            $.ajax({
                type: 'Get',
                url: actionUrl,
                success: function (data) {
                    generateBookMarkContainer(data)
                },
                error: function (error) {
                    console.log(error);
                    if (error.status == 401) {
                        window.location.replace('/login')
                    }
                }
            })
        }  
    }

    function generateBookMarkContainer(data) {
        var postId = $("article.post-article").attr('post-id')

        $("#bookmark_btn").addClass('active')

        var container = document.createElement('div')
        $(container).addClass('collections_container')
        $(container).attr('id', 'collections_container')
        generateBackdrop('collections_container');

        var list = document.createElement('ul');
        $(list).addClass('collections_list')

        data.forEach(item => {
            $(list).append(generateCollections(item, postId))
        })

        $(container).append(list);

        $(container).append(
            `<div class="new_collections">
                <button data-toggle="modal" data-target="#create_collection_modal">
                    Thêm thư mục mới
                </button>              
            </div>`
        )

        generateCreateCollectionsModal('create_collection_modal', data)

        $('#bookmark_container').children("#collections_container").empty();

        $('#bookmark_container').append(container);

        
        $("input.bookmark_checkbox").change(function (event) {
            var icon = $(this).parent().children('span.bookmark_item')
            var bookmarked = icon.hasClass('item_bookmarked')
            var collectionsId = $(this).val();

            var collectionsForm = $("form#collections-add-post-form").clone();
            collectionsForm.append(`<input  name="collectionsId" value=${collectionsId} />`)
            var data = collectionsForm.serialize();
            $.ajax({
                type: bookmarked?"DELETE":"POST",
                url: `/collections/${collectionsId}/post`,
                data: data,
                success: function (result) {
                    if (result.success) {
                        if (bookmarked) {
                            icon.removeClass('item_bookmarked')
                            icon.empty();
                        } else {
                            icon.addClass('item_bookmarked')
                            icon.append('<i class="fas fa-check fa-sm"></i>')
                            showAlert("success", "Lưu thành công", "Lưu thành công thông tin bài viết thành công")
                        }
                    } else {
                        showAlert("error", "Không thành công", "Lưu không thành công! Vui lòng thử lại sau.")
                    }
                },
                error: function (error) {
                    showAlert("error", "Không thành công", "Lưu không thành công! Vui lòng thử lại sau.")
                }
            })

            
        })

        

    }

    function generateBackdrop(backdropFor) {
        var backdrop = document.createElement('div')
        $(backdrop).addClass('backdrop')
        $(backdrop).attr('for', backdropFor)
        $(backdrop).on('click', function (event) {
            var targetId = $(this).attr('for')
            $(`#${targetId}`).parent().children('button.bookmark_btn').removeClass('active')
            $(`#${targetId}`).remove();
            $(this).remove()
        })

        $(document).find('body').prepend(backdrop)
    }

    function generateCollections(collections, postId) {
        var active = collections.postCollections.some(c => c.postId == postId)
        var child = collections.childrenCollections
        var item = `
            <li class="collections_item ${child.length > 0 ?'':'pb-3'}">                          
                <label class="mb-0 d-flex flex-nowrap align-items-center" for="col_${collections.id}">
                    <span class="d-flex align-items-center justify-content-center mr-2 bookmark_item ${active ? 'item_bookmarked':''}">
                        ${ active ? '<i class="fas fa-check fa-sm"></i>' : ''}
                    </span>
                    <input  hidden type="checkbox" class="bookmark_checkbox"  name="collectionsId" value="${collections.id}" id="col_${collections.id}"/>
                    <div class="text-nowrap">
                        ${collections.name}</label>
                    </div>
                </label>
                ${child.length > 0 ?
                    `<ul class="pt-3">
                        ${collections.childrenCollections.map(item => generateCollections(item, postId)).join('')}
                    </ul>`: ''
                  }
            </li >`
        return item;
    }

    function generateCreateCollectionsModal(id, data) {
        $(`#${id}`).remove();
        $('.modal-backdrop').remove();

        $(document).find('body').append(
            `<div class="modal fade create_collection_modal" id="${id}" tabindex="-1" aria-labelledby="create_collection_modal" aria-hidden="true">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content ">
                        <div class="modal-header border-0 pb-0">      
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div class="modal-body pt-0">
                            <div class="body-content px-4 px-md-5 py-4">
                                <form id="new-collections-form" action="/collections/create" method="POST">
                                    <div class="body-main-content">
                                        <div class="pb-5">
                                            <h2 class="font-weight-bold">TẠO THƯ MỤC MỚI</h2>
                                        </div>

                                        <div class="form-group pb-4">
                                            <div class="form-control multiple-level-select" display-for="new_collection_parent_collection">
                                                <input hidden name="ParentCollectionsId" value="" />
                                                <div class="multiple-level-select-label">Không có thư mục cha</div>
                                                <ul class="multiple-level-select-data">
                                                    <li class="option">
                                                        <div class="option-label" value="">
                                                            Không có thư mục cha
                                                        </div>         
                                                    </li>
                                                    ${data.map(item => generateCollectionsSelectItem(item)).join('')}        
                                                <ul>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <input class="form-control" type="text" name="Name" placeholder="Tên thư mục">
                                        </div>
                                    </div>
                                    <div class="form-group d-flex flex-row justify-content-center align-items-center">
                                        <button type="button" class="btn btn-outline-secondary rounded-pill mr-2 px-4" data-dismiss="modal" aria-label="Close">
                                            Huỷ
                                        </button>

                                        <button class="btn btn-success rounded-pill px-4" type="submit">Lưu</button>
                                    </div>
                                </form>
                            </div>                                        
                        </div>
                    </div>
                </div>
            </div>`
        )

        $("ul.select li.option").on('click', function (event) {
            event.stopPropagation();
/*            $(this).parents('ul.select').toggle();*/
            var targetId = $(this).parents('ul.select').attr('data-for')
            var value = $(this).attr('value')
            $(`#${targetId}`).val(value)
            console.log($(`#${targetId}`).val())
            

        })

        $(".multiple-level-select").on('click', function (event) {
            event.stopPropagation();
            $(this).attr("tabindex", -1)
            this.focus()   
            $(this).children('ul.multiple-level-select-data').toggle(); 
        })


        $(".multiple-level-select").on('focusout', function (event) {
            event.stopPropagation();
            $(this).children('ul.multiple-level-select-data').hide();
        })

        $(".multiple-level-select li div.option-label").on('click', function (event) {
            event.stopPropagation();
            var value = $(this).attr('value');
            var text = $(this).text();
            var root = $(this).closest('div.multiple-level-select')
            var label = root.children('div.multiple-level-select-label')
            var input = root.children('input')
            input.val(value)
            label.text(text)
            root.children('ul.multiple-level-select-data').toggle()
        })

        $("form#new-collections-form").on('submit', function (event) {
            event.preventDefault();
            var form = $(this)
            var actionUrl = form.attr('action');
            var method = form.attr('method')
            var modal = $(this).closest('div.modal')
            $.ajax({
                type: method,
                url: actionUrl,
                data: form.serialize(),
                success: function (data) {
                    fetchBookmarkData()
                    //var collections = data
                    //console.log(data)
                    //var list = $("ul.collections_list");
                    //console.log(list)
                    //var active = collections.postCollections.some(c => c.postId == postId)
                    //var parentItem = list.find(`input#col_${collections.parentCollectionsId}`).closest('li.collections_item')
                    //parentItem.addClass('pb-3')
                    //parentItem.append(`
                    //    <ul class="pt-3">
                    //        <li class="collections_item">
                    //            <label class="mb-0 d-flex flex-nowrap align-items-center" for="col_${collections.id}">
                    //                <span class="d-flex align-items-center justify-content-center mr-2 bookmark_item ${active ? 'item_bookmarked' : ''}">
                    //                    ${active ? '<i class="fas fa-check fa-sm"></i>' : ''}
                    //                </span>
                    //                <input  hidden type="checkbox" class="bookmark_checkbox"  name="collectionsId" value="${collections.id}" id="col_${collections.id}"/>
                    //                <div class="">
                    //                    ${collections.name}</label>
                    //                </div>
                    //            </label>
                    //        </li> 
                    //    </ul>
                    //`)
                    //$(modal).modal('hide')
                },
                error: function (error) {
                    console.log(error)
                }
            })
        })
    }

    function generateCollectionsSelectItem(collections, level = 0) {
        var child = collections.childrenCollections
        var item = `
            <li class="option">
                <div class="option-label text-nowrap" value=${collections.id}>
                    ${collections.name}
                </div>
                ${child.length > 0 ?
                    `<ul >
                        ${collections.childrenCollections.map(item => generateCollectionsSelectItem(item, level + 1)).join('')}
                    </ul>`: ''
                }
            </li>`
        return item;
    }


    
    
})

function getComments(userId, postId) {
    $.ajax({
        type: "GET",
        url: `/comment?postId=${postId}`,
        success: function (result) {
            if (result.code == 0) {
                console.log(result.data.length)
                $("#comment-count").text(result.data.length)
                result.data.forEach(item => {
                    let sub = userId == item.user.id ? ` <div>
                                <form method="DELETE" action="/comment" class="delete-form">
                                    <input name="Id" value="${item.id}" hidden />
                                    <button class="icon-btn text-danger" type="submit">
                                        <i class="fa-solid fa-trash"></i>
                                    </button>
                                </form>
                            </div>`: ""

                    $("#comment-container").append(`
                           <div class="media mb-3">
                                <img src="${item.user.avatar ? item.user.avatar.url : 'https://static.thenounproject.com/png/363640-200.png'}" class="mr-3 rounded-circle avatar-md" alt="${item.user.firstName}">
                                <div class="media-body">
                                    <div class="d-flex flex-row justify-content-between">
                                        <h5 class="mt-0">${item.user.lastName} ${item.user.firstName}</h5>  
                                        ${sub}  
                                    </div>
                                    <p>${item.content}</p>
                                </div>
                            </div>`
                    )
                })

                $(".delete-form").submit(function (e) {

                    e.preventDefault();

                    var form = $(this);
                    var actionUrl = form.attr('action');

                    $.ajax({
                        type: "Delete",
                        url: actionUrl,
                        data: form.serialize(),
                        success: function (data) {
                            console.log(data);
                            if (data.success)
                                location.reload(true)
                        },
                        error: function (error) {
                            console.log(error)
                        }
                    });

                })
            }
        },
        error: function (error) {
        }
    })
}

function getLikes(userId, postId) {
    $.ajax({
        type: "GET",
        url: `/like?postId=${postId}`,
        success: function (result) {
            if (result.code == 0) {
                console.log(result.data.length)
                $("#like-count").text(result.data.length)
                let isActive = result.data.find(item => item.userId == userId)
                if (isActive) {
                    $("#like-active").removeClass('fa-light');
                    $("#like-active").addClass("text-primary fa-solid")
                }
                else $("#like-active").removeClass("text-primary fa-solid")
            }
        },
        error: function (error) {
        }
    })
}

function report(postId, postName, userId) {
    $.ajax({
        method: "GET",
        url: '/Reason',
        success: function (response) {
            appenReportPostModal(response.data, postName, userId);
            $('#report_modal').modal('show');
            addSubmitReportEvent();
        },
        error: function (error) {
            console.log(error)
        }
    })
}

function appenReportPostModal(reason, postName, postId) {
    $("#report_modal").remove();
    $("body").append(`
        <div class="modal" tabindex="-1" id="report_modal">
            <div class="modal-dialog">
                <div class="modal-content p-3">
                    <form id="report_post_form" method="POST">
                        <div class="modal-header border-bottom-0">
                            <h5 class="modal-title text-center text-danger">Báo cáo bài viết!</h5>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div class="modal-body pt-0">
                            <p class="py-0">Báo cáo bài bài viết: <strong>"${postName}"</strong>. Bạn vui lòng cho biết lý do lý do báo cáo bài viết!</p>
                                <input type="text" name="postId" value="${postId}" hidden/>
                                <div class="form-group">
                                    <label for="reasonId" class="font-weight-bold">Lý do báo cáo</label>
                                    <select class="form-control" name="reasonId">
                                    ${reason.map(item => `<option value="${item.id}">${item.name}</option>`)}
                                    </select>
                                </div>
                                <div class="form-group">
                                    <label for="comment" class="font-weight-bold">Chi tiết:</label>
                                    <textarea class="form-control" name="comment" rows="3"></textarea>
                                </div>
                        </div>
                        <div class="modal-footer border-top-0">
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">Hủy bỏ</button>
                            <button type="summit" id="confirm_delete_post" class="btn btn-danger">Báo cáo</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>`
    )
}

function addSubmitReportEvent() {
    $("#report_post_form").on("submit", function (event) {
        event.preventDefault();
        let form = $(this)
        const array = form.serializeArray();
        const json = {};
        $.each(array, function () {
            json[this.name] = this.value || "";
        });
        console.log(json);
        $.ajax({
            method: "POST",
            url: "/Report",
            data: JSON.stringify(json),
            contentType: "application/json; charset=utf-8",
            traditional: true,
            success: function (response) {
                console.log(response)
                if (response.code == 0) {
                    showAlert("success", "", `Báo cáo bài viết thành công!`)
                    $("#report_modal").modal("hide")
                } else {
                    showAlert("danger", "", response.message);
                    $("#report_modal").modal("hide");
                }
            },
            error: function (error) {
                console.log(error)
            }
        })
    })
}