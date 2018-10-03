// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


/*----- Chat Modal -----*/
$(document).ready(function () {
    $("#collapseModal").click(function () {
        if ($("#modalBody").is(":visible")) {
            $("#modalBody").hide();
            $("#modalFooter").hide();
            $("#collapseModal").html("^");
        }
        else {
            $("#modalBody").show();
            $("#modalFooter").show();
            $("#collapseModal").html("v");
        }

        var chatModal = $("#chatModal");
        chatModal.modal({ show: true, backdrop: false, keyboard: false });

        var modalDialog = $("#modalDialog");
        chatModal.height(modalDialog.height()).width(modalDialog.width());
        chatModal.css({ position: 'fixed', bottom: 0, right: 0, left: 'auto', top: 'auto' });
    });

    var chatModal = $("#chatModal");
    chatModal.modal({ show: true, backdrop: false, keyboard: false });

    var modalDialog = $("#modalDialog");
    chatModal.height(modalDialog.height()).width(modalDialog.width());
    chatModal.css({ position: 'fixed', bottom: 0, right: 0, left: 'auto', top: 'auto' });

    $('#modalConnected').attr('src', '/images/error.svg');
    $('#modalConnected').height(15).width(15);

    console.log(connection.onclose);


});
/*----- Chat Modal -----*/


/*----- SignalR Chat -----*/
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.start().catch(err => console.error(err.toString()));

connection.onclose(function () {
    console.log("closed");
    setTimeout(function () {
        connection.start();
    }, 1000);
    setTimeout(function () {
        joinGroup();
    }, 2000);

});

//connection.hub.reconnecting(function () {
//    notifyUserOfTryingToReconnect(); // Your function to notify user.
//});

connection.on('ConnectionID', (message) => {
    $('#modalConnected').attr('src', '/images/success.svg');
    $('#modalConnected').height(15).width(15);

    $('#modalChatID').innerHTML = message;
});

connection.on('GroupMessage', (nick, message) => {
    console.log(message);
    appendLine(nick, message);
    $('#modalConnected').attr('src', '/images/success.svg');
    $('#modalConnected').height(15).width(15);
});

function disconnect() {
    connection.stop();
}

document.getElementById('frm-modal-message').addEventListener('submit', event => {
    let message = $('#modalMessage').val();
    let group = $('#modalGroup').val();
    let nick = $('#modalHandle').val();

    $('#modalMessage').val('');

    console.log('here');

    joinGroup();

    connection.invoke('GroupMessage', group, nick, message);
    event.preventDefault();
});

function joinGroup() {
    let group = $('#modalGroup').val();
    let nick = $('#modalHandle').val();

    connection.invoke('JoinGroup', group, nick);
}

function appendLine(nick, message) {
    let nameElement = document.createElement('strong');
    nameElement.innerText = `${nick}:`;

    let msgElement = document.createElement('em');
    msgElement.innerHTML = message.replace("<script>", "&lt;script&gt;").replace("<\/script>", "&lt;/script&gt;");

    let li = document.createElement('li');

    //if ($('#modalChatID').innerHTML.toString().localeCompare(connID) == 0) {
    //    li.appendChild(msgElement);
    //    li.style.cssText = "float:right;"; 
    //}
    //else {
        li.appendChild(nameElement);
        li.appendChild(msgElement);
    //}

    $('#modalList').append(li);
};

$(window).on("unload", function (e) {
    let group = $('input[name=modalGroup]:checked').val();
    let nick = $('#modalHandle').val();

    connection.invoke('GroupMessage', group, nick, nick + ' has left the chat.');
    event.preventDefault();
});
/*----- SignalR Chat -----*/