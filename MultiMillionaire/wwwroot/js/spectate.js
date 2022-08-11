"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("/multiplayerHub").build();


connection.on("Message", msg => console.log(msg));

connection.start().then(function () {
    console.log("Connected");
}).catch(function (err) {
    return console.error(err.toString());
});

function addUser() {
    const name = $("#nameInput").val();
    connection.invoke("AddUser", name);
}

function spectateGame() {
    addUser();
    const gameId = $("#gameIdInput").val();
    connection.invoke("SpectateGame", gameId);
}