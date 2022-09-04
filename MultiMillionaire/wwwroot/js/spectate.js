function spectateGame() {
    const gameId = $("#gameIdInput").val();
    connection.invoke("SpectateGame", gameId);
}