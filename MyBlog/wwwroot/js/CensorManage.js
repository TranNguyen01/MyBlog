$(document).ready(function () {
    $.ajax({
        method: "GET",
        url: "/Categories/GetList",
        success: function (data) {
            for (let category of data) {
                $("#categories_table tbody").append(`<tr id="${category.id}" role="button"><td  onclick="getCensor('${category.id}')">${category.name}</td></tr>`)
            }
            if (data.length > 0) {
                getCensor(data[0].id)
            }
        },
        error: function (error) {
            console.log(error)
        }
    })

    $("#add-censor-category-form").on("submit", function (event) {
        event.preventDefault();
        const data = new FormData(event.target);
        const value = Object.fromEntries(data.entries());
        const userIds = data.getAll("userId");
        for (let userId of userIds) {
            value.userId = userId;
            $.ajax({
                method: "Post",
                url: "/Censor",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(value),
                success: function (data) {
                    getCensor(value.categoryId);
                    $('#add_censor_modal').modal('toggle')
                },
                error: function (error) {
                    console.log(error)
                }
            })
        }
    })
})

function getCensor(categoryId) {
    for (let tr of $("#categories_table tbody").children("tr")) {
        let id = $(tr).attr('id')
        if (id == categoryId)
            $(tr).addClass("table-primary")
        else
            $(tr).removeClass("table-primary")
    }
    
    $.ajax({
        method: "GET",
        url: `/Censor/ByCategory/${categoryId}`,
        success: function (result) {
            $("#censors_table").attr("category-id", categoryId);  
            $("#censors_table tbody").empty();
            for (let censor of result.data) {    
                $("#censors_table tbody").append(`
                    <tr id='censor_${censor.id}'>
                        <td class="align-middle">
                            <button 
                                class="icon-btn text-danger px-0"
                                onclick="deleteCensor('${censor.id}', '${categoryId}')"
                            >
                                <i class="fa-solid fa-trash"></i>
                            </button>
                        </td>
                        <td class="align-middle">${censor.firstName} ${censor.firstName}</td>
                    </tr>`
                )
            }
        },
        error: function (error) {
            console.log(error)
        }
    })
}



function showModal(event) {
    let btn = event.target
    console.log($("#add-censor-category-form"))
    let table = $(btn).closest("table")
    let catId = table.attr("category-id");
    $("#add_censor_table tbody").empty();
    $("#add_censor_table tbody").append(`
        <tr class="d-none">
            <td class="align-middle">
                <input name="categoryId" value="${catId}"/>
            </td>
        </tr>
    `)
    $.ajax({
        method: "GET",
        url: `/Censor/Valid?catId=${catId}`,
        success: function (result) {
            for (let censor of result.data) {
                $("#add_censor_table tbody").append(`
                    <tr onclick="checkCensor(event)" role="button" id="newcensor_${censor.id}">
                        <td class="align-middle">
                            <input type="checkbox" name="userId" value="${censor.id}"/>
                        </td>
                        <td class="align-middle">
                            <img class="avatar-md rounded-circle" src="${censor.avatar?.url || 'https://vivureviews.com/wp-content/uploads/2022/08/avatar-vo-danh-9.png'}"/>
                        </td>
                        <td class="align-middle">${censor.lastName} ${censor.firstName}</td> 
                    </tr>
                `)
            }
            console.log(result)
        },
        error: function (error) {
            console.log(error)
        }
    })
}

function checkCensor(event) {
    let checked = $(event.target).parent().find("input:checkbox").attr("checked")
    $(event.target).parent().find("input:checkbox").attr("checked", !checked);
}

function deleteCensor(censor, category) {
    const value = { userId: censor, categoryId: category };
    $.ajax({
        method: "Delete",
        url: "/Censor",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(value),
        success: function (data) {
            getCensor(category)
        },
        error: function (error) {
            console.log(error)
        }
    })
}

