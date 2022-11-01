const game = {
    id: "",
    setId: function (id) {
        this.id = id;
        const gameIdElement = document.getElementById("gameId");
        if (gameIdElement) {
            gameIdElement.textContent = `Game ID: ${game.id}`;
        }
    },

    join: {
        host: async function () {
            if (this.validateForm()) {
                await players.setName();
                await connection.send("HostGame");
            }
        },

        audience: async function () {
            console.log(this);
            if (this.validateForm()) {
                await players.setName();
                const gameId = document.getElementById("gameIdInput").value.toUpperCase();
                await connection.send("JoinGameAudience", gameId);
            }
        },

        spectator: async function () {
            if (this.validateForm()) {
                const gameId = document.getElementById("gameIdInput").value.toUpperCase();
                await connection.send("SpectateGame", gameId);
            }
        },

        joinSuccessful: gameId => {
            game.setId(gameId);
            addEventListener("beforeunload", beforeUnloadListener, {capture: true});
            modals.joinGameModal.hide();
        },

        validateForm: () => {
            const nameInput = document.getElementById("playerNameInput");
            const gameIdInput = document.getElementById("gameIdInput");
            return nameInput.reportValidity() && (!gameIdInput || gameIdInput.reportValidity());
        },

        idNotFound: () => {
            show("gameIdNotFoundMessage");
        }
    },

    ended: () => {
        removeEventListener("beforeunload", beforeUnloadListener, {capture: true});
        modals.gameEndedModal.show();
        sounds.stopAll();
    },

    toasts: {
        showMessage: function (message, duration = 5000) {
            const toastContainer = document.getElementById("toastContainer");
            const toastElement = this.createToastElement(message, duration);

            toastElement.addEventListener("hidden.bs.toast", () => {
                toastContainer.removeChild(toastElement);
            });
            toastContainer.appendChild(toastElement);

            const toastObject = bootstrap.Toast.getOrCreateInstance(toastElement);
            toastObject.show();
            console.log(message);
        },

        createToastElement: (message, duration = 5000) => {
            const toastElement = document.createElement("div");
            toastElement.className = "toast box align-items-center fade";
            toastElement.setAttribute("data-bs-delay", duration.toString());
            toastElement.innerHTML =
                '<div class="toast-header">\n' +
                '<picture class="me-2">' +
                '<source type="image/webp" srcset="/images/webp/logo.webp">\n' +
                '<img src="/images/png/logo.png" alt="Who Wants To Be A Millionaire? logo">\n' +
                '</picture>\n' +
                '<strong class="me-auto">Who Wants To Be A Millionaire?</strong>\n' +
                '<button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>\n' +
                '</div>\n' +
                `<div class="toast-body">${message}</div>\n`;
            return toastElement;
        }
    }
}

connection.on("JoinSuccessful", game.join.joinSuccessful);
connection.on("JoinGameIdNotFound", game.join.idNotFound);
connection.on("GameEnded", game.ended);
connection.on("ShowToastMessage", game.toasts.showMessage.bind(game.toasts));
