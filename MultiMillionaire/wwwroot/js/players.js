﻿const players = {
    roles: {
        Host: 0,
        Audience: 1,
        Spectator: 2
    },

    setName: async () => {
        const name = document.getElementById("playerNameInput").value;
        await connection.invoke("SetName", name);
    },

    joined: user => {
        players.addToListPanel(user);
    },

    left: user => {
        players.removeFromListPanel(user);
    },

    addToListPanel: user => {
        const playerList = document.getElementById("playersListPanel").firstElementChild;
        let listElement;
        if (user.role === players.roles.Host) {
            listElement = players.createPlayerListElement(user.connectionId, user.name.toUpperCase(), "HOST");
        } else {
            listElement = players.createPlayerListElement(user.connectionId, user.name.toUpperCase(), user.score, "text-orange");
        }
        playerList.appendChild(listElement);
    },

    populateListPanel: playerList => {
        for (const player of playerList) {
            players.addToListPanel(player);
        }
    },

    removeFromListPanel: user => {
        document.getElementById(`player-list-element_${user.connectionId}`).remove();
    },

    createPlayerListElement: (id, text, rightText = "", rightClass = "") => {
        const listElement = document.createElement("div");
        listElement.id = `player-list-element_${id}`;
        listElement.className = "row mb-2 mx-0 player-list";
        listElement.innerHTML =
            '<hr class="col-1 question-line my-auto px-0"/>\n' +
            '<div class="col question">\n' +
            '<span class="answer-diamond me-2">◆</span>\n' +
            `<span class="player-list-text">${text}</span>\n` +
            `<span class="player-list-text player-list-right ms-auto me-1 ${rightClass}">${rightText}</span>\n` +
            '<span class="answer-diamond text-orange">◆</span>\n' +
            '</div>\n' +
            '<hr class="col-1 question-line my-auto px-0"/>\n' +
            '</div>';
        return listElement;
    }
}

connection.on("PopulatePlayerList", players.populateListPanel);
connection.on("PlayerJoined", players.joined);
connection.on("PlayerLeft", players.left);