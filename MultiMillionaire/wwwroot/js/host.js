game.join.host = async () => {
    if (game.join.validateForm()) {
        await setName();
        await connection.invoke("HostGame");
    }
}