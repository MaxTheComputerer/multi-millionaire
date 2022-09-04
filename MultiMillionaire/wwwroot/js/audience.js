function joinAudience() {
    setName();
    const gameId = $("#gameIdInput").val();
    connection.invoke("JoinGameAudience", gameId);
}