
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

    $('#modalConnected').attr('src', '/images/chat/error.png');
    $('#modalConnected').height(15).width(15);
    $('.modal-open').removeClass('modal-open');
    $('#modalChatID').html('closed');

    $('textarea').each(function () {
        this.setAttribute('style', 'flex-grow: 100;resize:none;height:26px;overflow-y:hidden;max-height:72px;');
    }).on('input', function () {
        var lines = this.value.split(/\r?\n/).length;
        console.log(parseInt(lines.toString()) - 1);
        var height = (26 + (22 * (lines - 1)));

        if (height >= 70)
            height = 70;

        this.style.height = height + 'px';
        //this.style.height = (this.scrollHeight) + 'px';

        $('#modalSend').outerHeight(height);
    });

    $('textarea').keyup(function (event) {
        if (event.keyCode == 13) {
            var content = this.value;
            var caret = getCaret(this);
            if (event.shiftKey) {
                this.value = content.substring(0, caret - 1) + "\r\n" + content.substring(caret, content.length);
                event.stopPropagation();
            } else {
                this.value = content.substring(0, caret - 1) + content.substring(caret, content.length);
                $('form').submit();
            }
        }
    });
});


function getCaret(el) {
    if (el.selectionStart) {
        return el.selectionStart;
    } else if (document.selection) {
        el.focus();
        var r = document.selection.createRange();
        if (r == null) {
            return 0;
        }
        var re = el.createTextRange(), rc = re.duplicate();
        re.moveToBookmark(r.getBookmark());
        rc.setEndPoint('EndToStart', re);
        return rc.text.length;
    }
    return 0;
}
/*----- Chat Modal -----*/

var focused = true;

window.onfocus = function () {
    focused = true;
    $('#favicon').attr('href', '/images/icons/BollmanITSolutions_badge.ico');
};

window.onblur = function () {
    focused = false;
};

/*----- SignalR Chat -----*/
const connChatModal = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .build();

connChatModal.serverTimeoutInMilliseconds = 1000 * 60 * 5;

connChatModal.onclose(function () {
    $('#modalConnected').attr('src', '/images/chat/error.png');
    if ($('#modalChatID').html().localeCompare('closed') !== 0) {
        setTimeout(function () {
            console.log("restarting connection...");
            connChatModal.start().catch(err => console.error(err.toString()));
            setTimeout(function () {
                joinGroup();
            }, 5000);
        }, 5000);
    }
});

connChatModal.on('ConnectionID', (message) => {
    $('#modalConnected').attr('src', '/images/chat/success.png');
    $('#modalConnected').height(15).width(15);

    $('#modalChatID').html(message);

    checkConnection();
});

connChatModal.on('GroupMessage', (nick, message) => {
    appendLine(nick, message);

    $('#modalConnected').attr('src', '/images/chat/success.png');
    $('#modalConnected').height(15).width(15);

    if (!focused) {
        $('#favicon').attr('href', '/images/icons/BollmanITSolutions_badge_notify.ico');
    }
    else {
        $('#favicon').attr('href', '/images/icons/BollmanITSolutions_badge.ico');
    }
    $("#modalBody").scrollTop(function () { return this.scrollHeight; });
});

function toggleConnection() {
    if ($('#modalChatID').html().localeCompare('closed') === 0) {
        console.log('modalToggleConn: connect');
        $('#modalChatID').html('open');
        checkConnection();
        $('#modalConnected').attr('src', '/images/chat/success.png');
    }
    else {
        console.log('modalToggleConn: disconnect');
        disconnect();
        $('#modalChatID').html('closed');
    }
}

function checkConnection() {
    var state = connChatModal.connection.connectionState;

    if (state === 4 || state === 2) {
        console.log('restarting connection...');

        let msgElement = document.createElement('em');
        msgElement.innerHTML = 'Connecting to chat...';
        msgElement.style = 'float:left;color:#005A9C;';

        $('#modalList').append(msgElement);
        $('#modalList').append(document.createElement('br'));
        connChatModal.start().catch(err => console.error(err.toString()));
        $('#modalConnected').attr('src', '/images/chat/success.png');
        $('#modalChatID').html('open');

        return 2;
    }

    return 0;
}

function disconnect() {
    connChatModal.stop();
    $('#modalChatID').html('closed');
}

$('#frm-modal-message').on('submit', event => {
    let message = $('#modalMessage').val();
    let group = $('#modalGroup').val();
    let nick = $('#modalHandle').val();

    $('#modalMessage').val('');

    var time = checkConnection();

    var lines = message.split(/\r?\n/)
    message = lines.join('<br/>');

    setTimeout(function () {
        joinGroup();
        connChatModal.invoke('GroupMessage', group, nick, message);
    }, time * 1000);

    event.preventDefault();
});

function joinGroup() {
    let group = $('#modalGroup').val();
    let nick = $('#modalHandle').val();

    connChatModal.invoke('JoinGroup', group, nick);
}

function appendLine(nick, message) {
    let nameElement = document.createElement('strong');
    nameElement.innerText = `${nick}:`;

    let msgElement = document.createElement('em');
    msgElement.innerHTML = '<br/>' + message.replace("<script>", "&lt;script&gt;").replace("<\/script>", "&lt;/script&gt;");

    let li = document.createElement('li');

    li.appendChild(nameElement);
    li.appendChild(msgElement);

    $('#modalList').append(li);
}

$(window).on("unload", function (e) {
    let group = $('input[name=modalGroup]:checked').val();
    let nick = $('#modalHandle').val();

    checkConnection();


});
/*----- SignalR Chat -----*/