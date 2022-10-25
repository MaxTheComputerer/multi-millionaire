const players = {
    roles: {
        Host: 0,
        Audience: 1,
        Spectator: 2
    },

    setName: async () => {
        const name = document.getElementById("playerNameInput").value;
        await connection.send("SetName", name);
    },

    joined: user => {
        players.listPanel.add(user);
    },

    left: user => {
        players.listPanel.remove(user);
    },

    listPanel: {
        add: function (user) {
            const playerList = document.getElementById("playersListPanel").firstElementChild;

            let rightText, rightClass = "";
            if (user.role === players.roles.Host) {
                rightText = "HOST";
            } else {
                rightText = user.score;
                rightClass = "text-orange";
            }
            const className = user.isFastestFingerWinner ? "correct" : "";
            const listElement = this.createListElement(user.connectionId, user.name.toUpperCase(), rightText, rightClass, className);
            playerList.appendChild(listElement);
        },

        remove: user => document.getElementById(`player-list-element_${user.connectionId}`).remove(),

        populate: function (playerList) {
            this.reset();
            for (const player of playerList) {
                this.add(player);
            }
        },

        reset: () => {
            const playerList = document.getElementById("playersListPanel").firstElementChild;
            const listElements = playerList.querySelectorAll(".player-list");
            listElements.forEach(e => e.remove());
        },

        createListElement: (id, text, rightText = "", rightClass = "", className = "", idPrefix = "player-list-element_") => {
            const listElement = document.createElement("div");
            listElement.id = idPrefix + id;
            listElement.className = `row mb-2 mx-0 player-list ${className}`;
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
}

connection.on("PopulatePlayerList", players.listPanel.populate.bind(players.listPanel));
connection.on("PlayerJoined", players.joined);
connection.on("PlayerLeft", players.left);