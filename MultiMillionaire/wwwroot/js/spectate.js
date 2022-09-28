game.join.spectators = async () => {
    if (game.join.validateForm()) {
        const gameId = document.getElementById("gameIdInput").value;
        await connection.invoke("SpectateGame", gameId);
    }
}

modals.gameEndedModal = new bootstrap.Modal('#gameEndedModal');