"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("/multiplayerHub").build();

async function startConnection(isHost = false) {
    connection.start().then(await onConnected(isHost)).catch(async () => {
        await sleep(5000);
        await startConnection(isHost);
    });
}

async function onConnected(isHost) {
    return async () => {
        console.log("Connected.");

        const connectionCookie = getCookie("millionaire-connection-id");
        if (isHost || connectionCookie == null) {
            console.log("Starting new session.");
            await connection.send("StartNewSession");
            game.join.showModal();
        } else {
            console.log("Resuming session.");
            await connection.send("ResumeGameSession", connectionCookie);
            setCookie("millionaire-connection-id", connection.connectionId);
        }
    }
}

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

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}

function setCookie(name, value) {
    const date = new Date();
    date.setTime(date.getTime() + (24 * 60 * 60 * 1000));
    let expires = "; expires=" + date.toUTCString();
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}

function eraseCookie(name) {
    document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
}