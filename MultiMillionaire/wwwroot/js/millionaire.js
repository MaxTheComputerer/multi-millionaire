const millionaire = {
    request: async () => await connection.invoke("RequestMainGame"),

    noNextPlayer: players => {
        const choosePlayerSelect = document.getElementById("choosePlayerSelect");
        choosePlayerSelect.querySelectorAll("[value]:not([value=''])").forEach(e => e.remove());

        for (const player of players) {
            const optionElement = document.createElement("option");
            optionElement.value = player.connectionId;
            optionElement.textContent = player.name;
            choosePlayerSelect.appendChild(optionElement);
        }

        modals.choosePlayerModal.show();
    },

    setPlayerAndStart: async () => {
        const choosePlayerSelect = document.getElementById("choosePlayerSelect");
        if (choosePlayerSelect.value !== "") {
            await connection.invoke("SetPlayerAndStart", choosePlayerSelect.value);
        }
    },

    letsPlay: async () => await connection.invoke("LetsPlay"),

    answers: {
        submit: async letter => await connection.invoke("SubmitAnswer", letter),

        select: letter => {
            const answerElement = document.getElementById(`answer${letter}`);
            answerElement.classList.add("selected");
        },

        highlightCorrect: async letter => {
            const answerElement = document.getElementById(`answer${letter}`);
            answerElement.classList.add("correct");
        },

        flashCorrect: async letter => {
            await flash(`answer${letter}`);
        },

        resetBackgrounds: () => {
            ["answerA", "answerB", "answerC", "answerD"].forEach(id => {
                const element = document.getElementById(id);
                element.classList.remove("correct", "selected");
            })
        }
    },

    banners: {
        winnings: {
            show: amount => {
                const winningsRow = document.getElementById("winnings");
                winningsRow.querySelector(".winnings-text").textContent = amount;
                hide("questionAndAnswers");
                show("winnings", "flex");
            },

            hide: () => {
                hide("winnings");
                show("questionAndAnswers");
            },
        },

        showTotalPrize: amount => {
            const prizeRow = document.getElementById("totalPrize");
            prizeRow.querySelector(".total-prize-text").textContent = amount;
            hide("questionAndAnswers");
            show("totalPrize", "flex");
        },

        showMillionaire: name => {
            const prizeRow = document.getElementById("millionairePrize");
            prizeRow.querySelector(".millionaire-prize-text").textContent = name.toUpperCase();
            hide("questionAndAnswers");
            show("millionairePrize", "flex");
        }
    },

    moneyTree: {
        set: questionNumber => {
            if (questionNumber > 1) {
                const previousRow = document.getElementById(`tree-${questionNumber - 1}`);
                previousRow.classList.remove("tree-selected");
            }

            const currentRow = document.getElementById(`tree-${questionNumber}`);
            currentRow.classList.add("tree-selected");
        },

        reset: () => {
            const elements = document.querySelectorAll(".tree-selected");
            elements.forEach(e => e.classList.remove("tree-selected"));
        }
    },

    walkAway: async () => await connection.invoke("WalkAway"),

    lifelines: {
        reset: function () {
            ["lifeline-5050", "lifeline-phone", "lifeline-audience"].forEach(id => {
                const element = document.getElementById(id);
                element.classList.remove("used");
            });
            this.phone.reset();
        },

        fiftyFifty: {
            request: async () => await connection.invoke("RequestFiftyFifty"),

            use: answers => {
                const lifeline = document.getElementById("lifeline-5050");
                lifeline.classList.add("used");

                answers.forEach(letter => {
                    const id = `answer${letter}`;
                    disable(id);
                    setAnswerText(id, "\u00a0");
                });
            }
        },

        phone: {
            request: async () => await connection.invoke("RequestPhoneAFriend"),

            use: () => {
                const lifeline = document.getElementById("lifeline-phone");
                lifeline.classList.add("used");
            },

            phoneFriend: async () => await connection.invoke("ChooseWhoToPhone", false),

            phoneAi: async () => await connection.invoke("ChooseWhoToPhone", true),

            startClock: async () => await connection.invoke("PhoneStartClock"),

            onStart: () => {
                const clockElement = document.getElementById("phoneClockImg");
                clockElement.src = "../images/clock.gif";
            },

            dismiss: async () => await connection.invoke("DismissPhoneAFriend"),

            reset: () => {
                const clockElement = document.getElementById("phoneClockImg");
                clockElement.src = "../images/clock.png";
            }
        },

        audience: {}
    }
}

connection.on("NoNextPlayer", millionaire.noNextPlayer);
connection.on("DismissChoosePlayerModal", modals.choosePlayerModal.hide);
connection.on("SelectAnswer", millionaire.answers.select);
connection.on("HighlightCorrectAnswer", millionaire.answers.highlightCorrect);
connection.on("FlashCorrectAnswer", millionaire.answers.flashCorrect);
connection.on("ShowWinnings", millionaire.banners.winnings.show);
connection.on("HideWinnings", millionaire.banners.winnings.hide);
connection.on("ShowTotalPrize", millionaire.banners.showTotalPrize);
connection.on("ShowMillionaireBanner", millionaire.banners.showMillionaire);
connection.on("SetMoneyTree", millionaire.moneyTree.set);
connection.on("ResetMoneyTree", millionaire.moneyTree.reset);
connection.on("ResetAnswerBackgrounds", millionaire.answers.resetBackgrounds);

connection.on("ResetLifelines", millionaire.lifelines.reset.bind(millionaire.lifelines));
connection.on("UseFiftyFifty", millionaire.lifelines.fiftyFifty.use);
connection.on("UsePhoneAFriend", millionaire.lifelines.phone.use);
connection.on("StartPhoneClock", millionaire.lifelines.phone.onStart);