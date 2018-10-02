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
});
/*----- Chat Modal -----*/


/*----- SignalR Chat -----*/
const connSignalR = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .build();

connSignalR.start().catch(err => console.error(err.toString()));

connSignalR.on('GroupMessage', (nick, message) => {
    appendLine(nick, message);
    $('#modalConnected').attr('src', '/images/success.svg');
    $('#modalConnected').height(15).width(15);
});

document.getElementById('frm-modal-message').addEventListener('submit', event => {
    let message = $('#modalMessage').val();
    let group = $('#modalGroup').val();
    let nick = $('#modalHandle').val();

    $('#modalMessage').val('');

    console.log('here');

    connSignalR.invoke('GroupMessage', group, nick, message);
    event.preventDefault();
});

function joinGroup() {
    let group = $('#modalGroup').val();
    let nick = $('#modalHandle').val();

    connection.invoke('JoinGroup', group, nick);
    $('#modalConnected').attr('src', '/images/success.svg');
    event.preventDefault();
}

function appendLine(nick, message) {
    let nameElement = document.createElement('strong');
    nameElement.innerText = `${nick}:`;

    let msgElement = document.createElement('em');
    msgElement.innerHTML = message.replace("<script>", "&lt;script&gt;").replace("<\/script>", "&lt;/script&gt;");

    let li = document.createElement('li');
    li.appendChild(nameElement);
    li.appendChild(msgElement);

    $('#modalList').append(li);
};

$(window).on("unload", function (e) {
    let group = $('input[name=modalGroup]:checked').val();
    let nick = $('#modalHandle').val();

    connSignalR.invoke('GroupMessage', group, nick, nick + ' has left the chat.');
    event.preventDefault();
});
/*----- SignalR Chat -----*/