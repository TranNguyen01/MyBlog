function viewPostDetail(event, postId) {
    let post = posts.find(p => p.id == postId)
    let content = `<div class="container post-article" post-id="${post.id}">
            <div class="row justify-content-center">
                <div class="col-12 col-lg-12 col-xl-10 text-left">
                    <h1 class="font-weight-bold">${post.title}</h1>
                    <div class="text-left mt-3 mb-4">
                        <span class="font-weight-bold mr-2">${post.author.lastName} ${post.author.firstName}</span>
                        <time class="text-muted" datetime="${post.createdAt}"></time>
                    </div>
                </div>
            </div>
            <div class="row justify-content-center">
                <div class="col-12 col-lg-12 col-xl-10 mb-3">
                    ${post.content}
                </div> 
            </div>
        </div >`
    $("#post_detail_modal .modal-body").empty();
    $("#post_detail_modal .modal-body").append(content)
}

function viewAcceptPost(event, postId) {
    $("#accept_modal").remove();
    $("body").append(`
        <div class="modal" tabindex="-1" id="accept_modal">
            <div class="modal-dialog">
                <div class="modal-content p-3">
                    <div class="modal-header border-bottom-0">
                        <h5 class="modal-title text-center text-success">Chấp nhận đăng tải bài viết!</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <p class="py-0">Bạn có chắc chắn chấp nhận bài viết!.</p>
                    </div>
                    <div class="modal-footer border-top-0">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Không</button>
                        <button type="button" id="confirm_delete_post" class="btn btn-primary" onClick="acceptPost(event, '${postId}')">Có</button>
                    </div>
                </div>
            </div>
        </div>`
    )
    $('#accept_modal').modal('show');
}

function viewDeclinePost(event, postId) {
    let post = posts.find(p => p.id == postId)
    $.ajax({
        method: "GET",
        url: '/Reason',
        success: function (response) {
            appenDeclinePostModal(response.data, post);
            $('#decline_modal').modal('show');
            addSubmitDeclineEvent();
        },
        error: function (error) {
            console.log(error)
        }
    })
}

function acceptPost(event, postId) {
    const data = {
        postId: postId,
        status: "1"
    }

    $.ajax({
        method: "POST",
        url: "CensorShip",
        data: JSON.stringify(data),
        contentType: "application/json; charset=utf-8",
        traditional: true,
        success: function (response) {
            if (response.code == 0) {
                $('#accept_modal').modal('hide');
                console.log(response.data)
                updatePostCensor(response.data.post)
                showAlert("success", "", `Chấp thuận bài viết thành công!`)
            } else {
                showAlert("danger", "", response.message)
                $('#accept_modal').modal('hide');
            }
        },
        error: function (error) {
            console.log(error)
        }
    })
}

function declinePost(event, postId) {

}

function appenDeclinePostModal(reason, post) {
    $("#decline_modal").remove();
    $("body").append(`
        <div class="modal" tabindex="-1" id="decline_modal">
            <div class="modal-dialog">
                <div class="modal-content p-3">
                    <form id="decline_post_form" method="POST">
                        <div class="modal-header border-bottom-0">
                            <h5 class="modal-title text-center text-danger">Từ chối bài viết!</h5>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div class="modal-body pt-0">
                            <p class="py-0">Từ chối bài viết: <strong>"${post.title}"</strong>. Vui lòng chọn lý do từ chối bài viết này!</p>
                                <input type="text" name="postId" value="${post.id}" hidden/>
                                <input type="text" name="status" value="-1" hidden/>
                                <div class="form-group">
                                    <label for="reasonId" class="font-weight-bold">Lý do từ chối</label>
                                    <select class="form-control" name="reasonId">
                                    ${reason.map(item => `<option value="${item.id}">${item.name}</option>`)}
                                    </select>
                                </div>
                                <div class="form-group">
                                    <label for="comment" class="font-weight-bold">Nhận xét</label>
                                    <textarea class="form-control" name="comment" rows="3"></textarea>
                                </div>
                        </div>
                        <div class="modal-footer border-top-0">
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">Hủy bỏ</button>
                            <button type="summit" id="confirm_delete_post" class="btn btn-danger">Từ chối</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>`
    )
}

function addSubmitDeclineEvent() {
    $("#decline_post_form").on("submit", function (event) {
        event.preventDefault();
        let form = $(this)
        const array = form.serializeArray(); 
        const json = {};
        $.each(array, function () {
            json[this.name] = this.value || "";
        });
        $.ajax({
            method: "POST",
            url: "CensorShip",
            data: JSON.stringify(json),
            contentType: "application/json; charset=utf-8",
            traditional: true,
            success: function (response) {
                if (response.code == 0) {
                    showAlert("success", "", `Từ chối bài viết thành công!`)
                    updatePostCensor(response.data.post)
                    $("#decline_modal").modal("hide")
                } else {
                    showAlert("danger", "", response.message);
                    $("#decline_modal").modal("hide");
                }
            },
            error: function (error) {
                console.log(error)
            }
        })
    })
}

function updatePostCensor(post) {
    $(`tr[key="${post.id}"]`).empty();
    $(`tr[key="${post.id}"]`).append(`
        <td class="align-middle">
            <strong>
                <a href="">${post.title}</a>
            </strong>
        </td>
        <td class="align-middle"> ${`${post.author?.lastName} ${post.author?.firstName}`}</td>
        <td class="align-middle"> ${getFormattedDate(post.createdAt)}</td>
        <td class="align-middle"> ${getFormattedDate(post.lastUpdatedAt)}</td>
        <td class="align-middle">${post.category?.name}</td>
        <td class="align-middle">
            ${post.status == 1 ? `<p class="text-success mb-0">Phê duyệt</p>` : (post.Status == 0 ? `<p class="text-primary mb-0">Chờ phê duyệt</p>` : `<p class="text-danger mb-0">Từ chối</p>` ) }      
        </td>
                        
        <td>
            <div class="d-flex flex-row justify-content-center flex-nowrap alig-items-center border-top-0">
                <button class="icon-btn text-primary p-0" title="detail"
                    onclick="viewPostDetail(event, '${post.id}')"
                    data-toggle="modal"
                    data-target="#post_detail_modal"
                    >
                    <i class="fa-regular fa-magnifying-glass"></i>
                </button>
                <button class="icon-btn text-success p-0 mx-3" 
                    title="accept" 
                    onclick="viewAcceptPost(event, '${post.id}')"
                >
                    <i class="fa-regular fa-circle-check"></i>
                </button>
                <button class="icon-btn text-danger p-0" 
                    title="decline"
                    onclick="viewDeclinePost(event, '${post.id}')">
                    <i class="fa-regular fa-circle-exclamation"></i>
                </button>
            </div>
        </td>
    `);
}

function getFormattedDate(dateStr) {
    let date = new Date(dateStr);
    let year = date.getFullYear();
    let month = (1 + date.getMonth()).toString().padStart(2, '0');
    let day = date.getDate().toString().padStart(2, '0');
    return day + '/' + month + '/' + year;
}

$(document).ready(function () {
    $('#dataTable').DataTable();
})