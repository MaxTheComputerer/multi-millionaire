game.join.audience = async () => {
    if (game.join.validateForm()) {
        await setName();
        const gameId = document.getElementById("gameIdInput").value;
        await connection.invoke("JoinGameAudience", gameId);
    }
}