"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("/multiplayerHub").build();


connection.on("Message", msg => console.log(msg));

connection.start().then(function () {
    console.log("Connected");
}).catch(function (err) {
    return console.error(err.toString());
});

function setName() {
    const name = $("#nameInput").val();
    connection.invoke("SetName", name);
}