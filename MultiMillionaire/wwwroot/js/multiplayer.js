"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("/multiplayerHub").build();

function startConnection() {
    connection.start().then(() => {
        console.log("Connected");
        modals.joinGameModal.show();
    }).catch(async () => {
        await sleep(5000);
        startConnection();
    });
}

startConnection();

connection.onclose(err => {
    if (err) {
        console.log("Connection closed with error: " + err);
    } else {
        console.log("Disconnected");
    }
    removeEventListener("beforeunload", beforeUnloadListener, {capture: true});
    modals.disconnectedModal.show();
    sounds.stopAll();
});

connection.on("Message", msg => console.log(msg));

const beforeUnloadListener = (event) => {
    event.preventDefault();
    return event.returnValue = "Are you sure you want to exit the game?";
};
