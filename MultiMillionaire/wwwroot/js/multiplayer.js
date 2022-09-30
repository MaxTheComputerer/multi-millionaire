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
