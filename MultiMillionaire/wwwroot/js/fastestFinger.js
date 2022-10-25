const fastestFinger = {
    request: async () => await connection.send("RequestFastestFinger"),

    fetchQuestion: async () => await connection.send("FetchFastestFingerQuestion"),

    showAnswers: answers => {
        setAnswerText("answerA", answers.A);
        setAnswerText("answerB", answers.B);
        setAnswerText("answerC", answers.C);
        setAnswerText("answerD", answers.D);
    },

    onStart: function () {
        this.input.unlock();
        this.startTime = Date.now();
    },

    submit: async function () {
        if (this.input.cursor === 4) {
            const endTime = Date.now();
            this.input.lock();
            const duration = (endTime - this.startTime) / 1000.0;

            const element = document.getElementById("fastestFingerInput");
            const inputs = element.querySelectorAll(".fff-answer-input span");
            const answerOrder = Array.from(inputs).map(i => i.textContent);

            await connection.send("SubmitFastestFingerAnswer", answerOrder, duration);
        }
    },

    stopVoteMusic: () => {
        if (sounds.isPlaying("fastestFinger.vote")) {
            sounds.play("fastestFinger.earlyEnd");
        }
        sounds.stop("fastestFinger.vote");
    },

    revealAnswer: (index, letter, answer) => {
        const rowElement = document.getElementById(`fffAnswer${index}`);
        const letterElement = rowElement.querySelector(".answer-letter");
        const textElement = rowElement.querySelector(".answer-text");
        letterElement.textContent = `${letter}:`;
        textElement.textContent = answer;
        rowElement.classList.add("fff-slide-in");
    },

    populateResultsPanel: playerList => {
        const panel = document.getElementById("fffResultsPanel");
        for (const player of playerList) {
            const listElement = players.listPanel.createListElement(player.connectionId, player.name.toUpperCase(), "", "", "", "fff-results-list-element_");
            panel.appendChild(listElement);
        }
    },

    revealCorrectPlayers: async correctPlayerTimes => {
        for (const playerId of Object.keys(correctPlayerTimes).slice().reverse()) {
            const listElement = document.getElementById(`fff-results-list-element_${playerId}`);
            const rightText = listElement.querySelector(".player-list-right");
            listElement.classList.add("correct");
            rightText.textContent = correctPlayerTimes[playerId].toFixed(2);
            await sleep(250);
        }
    },

    highlightWinner: async winner => {
        await flash(`fff-results-list-element_${winner}`, 8, 115, true);
    },

    reset: function () {
        const resultsPanel = document.getElementById("fffResultsPanel");
        resultsPanel.replaceChildren();

        for (let i = 0; i < 4; i++) {
            const rowElement = document.getElementById(`fffAnswer${i}`);
            const letterElement = rowElement.querySelector(".answer-letter");
            const textElement = rowElement.querySelector(".answer-text");
            letterElement.textContent = "";
            textElement.textContent = "";
            rowElement.classList.remove("fff-slide-in");
        }
    },

    input: {
        cursor: 0,

        unlock: () => {
            enable("answerA");
            enable("answerB");
            enable("answerC");
            enable("answerD");
            enable("fffDeleteBtn");
            enable("fffSubmitBtn");
        },

        lock: () => {
            disable("answerA");
            disable("answerB");
            disable("answerC");
            disable("answerD");
            disable("fffDeleteBtn");
            disable("fffSubmitBtn");
        },

        insert: function (letter) {
            if (this.cursor < 4) {
                setText(`fffInput${this.cursor++}`, letter);
                disable(`answer${letter}`);
            }
        },

        delete: function () {
            if (this.cursor > 0) {
                const id = `fffInput${--this.cursor}`;
                const element = document.getElementById(id);
                const letter = element.textContent;
                setText(id, "\u{2002}");
                enable(`answer${letter}`);
            }
        },

        reset: function () {
            while (this.cursor > 0) {
                this.delete();
            }
            this.lock();
        }
    }
}

connection.on("StartFastestFinger", fastestFinger.showAnswers);
connection.on("EnableFastestFingerAnswering", fastestFinger.onStart.bind(fastestFinger));
connection.on("DisableFastestFingerAnswering", fastestFinger.input.lock);
connection.on("StopFastestFingerVoteMusic", fastestFinger.stopVoteMusic);
connection.on("ShowFastestFingerAnswer", fastestFinger.revealAnswer);
connection.on("PopulateFastestFingerResults", fastestFinger.populateResultsPanel);
connection.on("RevealCorrectFastestFingerPlayers", fastestFinger.revealCorrectPlayers);
connection.on("HighlightFastestFingerWinner", fastestFinger.highlightWinner);
connection.on("ResetFastestFinger", fastestFinger.reset.bind(fastestFinger));
connection.on("ResetFastestFingerInput", fastestFinger.input.reset.bind(fastestFinger.input));