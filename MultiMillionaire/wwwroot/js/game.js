﻿const game = {
    id: "",
    setId: id => {
        game.id = id;
        const gameIdElement = document.getElementById("gameId");
        if (gameIdElement) {
            gameIdElement.innerText = `Game ID: ${game.id}`;
        }
    },

    join: {
        host: async () => {
            if (game.join.validateForm()) {
                await players.setName();
                await connection.invoke("HostGame");
            }
        },

        audience: async () => {
            if (game.join.validateForm()) {
                await players.setName();
                const gameId = document.getElementById("gameIdInput").value.toUpperCase();
                await connection.invoke("JoinGameAudience", gameId);
            }
        },

        spectator: async () => {
            if (game.join.validateForm()) {
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
    },

    rounds: {
        fastestFinger: {
            request: async () => await connection.invoke("RequestFastestFinger"),

            fetchQuestion: async () => await connection.invoke("FetchFastestFingerQuestion"),

            onStart: answers => {
                console.log(answers);
            }
        }
    }
}

connection.on("JoinSuccessful", game.join.joinSuccessful);
connection.on("JoinGameIdNotFound", game.join.idNotFound);
connection.on("GameEnded", game.ended);

connection.on("StartFastestFinger", game.rounds.fastestFinger.onStart);