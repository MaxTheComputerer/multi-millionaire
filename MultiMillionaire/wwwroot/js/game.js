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
                await connection.invoke("HostGame");
            }
        },

        audience: async function () {
            console.log(this);
            if (this.validateForm()) {
                await players.setName();
                const gameId = document.getElementById("gameIdInput").value.toUpperCase();
                await connection.invoke("JoinGameAudience", gameId);
            }
        },

        spectator: async function () {
            if (this.validateForm()) {
                const gameId = document.getElementById("gameIdInput").value.toUpperCase();
                await connection.invoke("SpectateGame", gameId);
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
    }
}

connection.on("JoinSuccessful", game.join.joinSuccessful);
connection.on("JoinGameIdNotFound", game.join.idNotFound);
connection.on("GameEnded", game.ended);
