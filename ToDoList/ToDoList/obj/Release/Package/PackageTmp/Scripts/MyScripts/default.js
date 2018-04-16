function changeFavoriteValue(taskId, currentFavoriteIconElem) {
    $.ajax({
        type: "POST",
        url: "changefavoritevalue/",// + taskId,
        data: "{'taskId':'" + taskId + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function () {
        var currentIcon = currentFavoriteIconElem.attr('src');
        if (currentIcon.endsWith('not_favorite_icon.png')) {
            currentFavoriteIconElem.attr('src', '/Content/Images/favorite_icon.png');
        }
        else {
            currentFavoriteIconElem.attr('src', '/Content/Images/not_favorite_icon.png');
        }
    });
}

function changeDoneValue(taskId, currentFavoriteIconElem) {
    $.ajax({
        type: "POST",
        url: "changedonevalue/",// + taskId,
        data: "{'taskId':'" + taskId + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function () {
        var currentIcon = currentFavoriteIconElem.attr('src');
        if (currentIcon.endsWith('undone.png')) {
            currentFavoriteIconElem.attr('src', '/Content/Images/done.png');
        }
        else {
            currentFavoriteIconElem.attr('src', '/Content/Images/undone.png');
        }
    });
}



function deleteTask(taskId, taskElem) {
    $.ajax({
        type: "POST",
        url: "deletetask/",// + taskId,
        data: "{'id':'" + taskId + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function () {
        alert('success');
        taskElem.hide();
    });

}


$(function () {

    $('.folder').each(function () {
        var folderId = $(this).find('span').first().attr('id');
        $(this).click(function () { window.location.href = '/tasks/folder/' + folderId; });
    });

    $('.task').each(function () {
        var taskId = $(this).find('span').first().attr('id');
        $(this).click(function () {
            window.location.href = '/tasks/edit/' + taskId;
        });
    });

    $('.favorite-icon').each(function () {
        var iconElem = $(this);
        var taskId = iconElem.siblings('span').first().attr('id');
        iconElem.click(function (event) {
            event.stopPropagation();
            changeFavoriteValue(taskId, iconElem);
        });
    });

    $('.done-icon').each(function () {
        var iconElem = $(this);
        var taskId = iconElem.siblings('span').first().attr('id');
        iconElem.click(function (event) {
            event.stopPropagation();
            changeDoneValue(taskId, iconElem);
        });
    });

    $('.delete-icon').each(function () {
        var iconElem = $(this);
        var taskElem = iconElem.parents('.task').first();
        var taskId = iconElem.siblings('span').first().attr('id');
        iconElem.click(function (event) {
            event.stopPropagation();
            deleteTask(taskId, taskElem);
        });
    });

});