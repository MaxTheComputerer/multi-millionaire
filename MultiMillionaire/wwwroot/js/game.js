const game = {
    id: "",
    setId: function (id) {
        this.id = id;
        const gameIdElement = document.getElementById("gameId");
        if (gameIdElement) {
            gameIdElement.innerText = `Game ID: ${game.id}`;
        }
    },

    join: {
        host: async function () {
            if (this.validateForm()) {
                await players.setName();
                await connection.invoke("HostGame");
            }
        },

        audience: async function () {
            console.log(this);
            if (this.validateForm()) {
                await players.setName();
                const gameId = document.getElementById("gameIdInput").value.toUpperCase();
                await connection.invoke("JoinGameAudience", gameId);
            }
        },

        spectator: async function () {
            if (this.validateForm()) {
                const gameId = document.getElementById("gameIdInput").value.toUpperCase();
                await connection.invoke("SpectateGame", gameId);
            }
        },

        joinSuccessful: gameId => {
            game.setId(gameId);
            addEventListener("beforeunload", beforeUnloadListener, {capture: true});
            modals.joinGameModal.hide();
        },

        validateForm: () => {
            const nameInput = document.getElementById("playerNameInput");
            const gameIdInput = document.getElementById("gameIdInput");
            return nameInput.reportValidity() && (!gameIdInput || gameIdInput.reportValidity());
        },

        idNotFound: () => {
            show("gameIdNotFoundMessage");
        }
    },

    ended: () => {
        removeEventListener("beforeunload", beforeUnloadListener, {capture: true});
        modals.gameEndedModal.show();
    },

    rounds: {
        fastestFinger: {
            request: async () => await connection.invoke("RequestFastestFinger"),

            fetchQuestion: async () => await connection.invoke("FetchFastestFingerQuestion"),

            showAnswers: answers => {
                setAnswerText("answerA", answers.A);
                setAnswerText("answerB", answers.B);
                setAnswerText("answerC", answers.C);
                setAnswerText("answerD", answers.D);
            },

            onStart: function () {
                console.log(this.input);
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
                    const answerOrder = Array.from(inputs).map(i => i.innerText);

                    await connection.invoke("SubmitFastestFingerAnswer", answerOrder, duration);
                }
            },

            input: {
                cursor: 0,

                unlock: () => {
                    console.log("unlock");
                    unlock("answerA");
                    unlock("answerB");
                    unlock("answerC");
                    unlock("answerD");
                    unlock("fffDeleteBtn");
                    unlock("fffSubmitBtn");
                },

                lock: () => {
                    lock("answerA");
                    lock("answerB");
                    lock("answerC");
                    lock("answerD");
                    lock("fffDeleteBtn");
                    lock("fffSubmitBtn");
                },

                insert: function (letter) {
                    if (this.cursor < 4) {
                        setText(`fffInput${this.cursor++}`, letter);
                        lock(`answer${letter}`);
                    }
                },

                delete: function () {
                    if (this.cursor > 0) {
                        const id = `fffInput${--this.cursor}`;
                        const element = document.getElementById(id);
                        const letter = element.innerText;
                        setText(id, "\u{2002}");
                        unlock(`answer${letter}`);
                    }
                }
            }
        }
    }
}

connection.on("JoinSuccessful", game.join.joinSuccessful);
connection.on("JoinGameIdNotFound", game.join.idNotFound);
connection.on("GameEnded", game.ended);

connection.on("StartFastestFinger", game.rounds.fastestFinger.showAnswers);
connection.on("EnableFastestFingerAnswering", game.rounds.fastestFinger.onStart);