"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("/multiplayerHub").build();

connection.on("Message", msg => console.log(msg));

connection.start().then(() => {
    console.log("Connected");
    modals.joinGameModal.show();
}).catch(err => {
    return console.error(err.toString());
});

const beforeUnloadListener = (event) => {
    event.preventDefault();
    return event.returnValue = "Are you sure you want to exit the game?";
};

connection.on("JoinSuccessful", game.join.joinSuccessful);
connection.on("JoinGameIdNotFound", game.join.idNotFound);
connection.on("PopulatePlayerList", players.populateListPanel);
connection.on("PlayerJoined", players.joined);
connection.on("PlayerLeft", players.left);
connection.on("GameEnded", game.ended);

connection.on("SettingsUpdated", settings.updated);