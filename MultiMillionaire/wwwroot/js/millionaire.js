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
            });
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

        audience: {
            request: async () => await connection.invoke("RequestAskTheAudience"),

            use: () => {
                const lifeline = document.getElementById("lifeline-audience");
                lifeline.classList.add("used");
            },

            askLive: async () => await connection.invoke("ChooseAudienceToAsk", false),

            askAi: async () => await connection.invoke("ChooseAudienceToAsk", true),

            submit: async letter => await connection.invoke("SubmitAudienceGuess", letter),

            lock: () => {
                disable("answerA");
                disable("answerB");
                disable("answerC");
                disable("answerD");
            },

            setAnswersOnClick: () => {
                ['A', 'B', 'C', 'D'].forEach(letter => {
                    const element = document.getElementById(`answer${letter}`);
                    element.onclick = async () => await millionaire.lifelines.audience.submit(letter);
                })
            },

            resetAnswersOnClick: () => {
                ['A', 'B', 'C', 'D'].forEach(letter => {
                    const element = document.getElementById(`answer${letter}`);
                    element.onclick = () => fastestFinger.input.insert(letter);
                })
            },

            start: async () => await connection.invoke("StartAudienceVoting"),

            dismiss: async () => await connection.invoke("DismissAskTheAudience"),

            graph: {
                getContext: () => {
                    const canvas = document.getElementById("audienceGraph");
                    const ctx = canvas.getContext("2d");
                    ctx.globalCompositeOperation = 'source-over';
                    ctx.shadowBlur = 3;
                    ctx.shadowColor = "#2b88e7";
                    ctx.strokeStyle = "#2b88e7";
                    ctx.lineWidth = 2;
                    return ctx;
                },

                drawGrid: function () {
                    const ctx = this.getContext();
                    let i;
                    for (i = 1.5; i <= 376.5; i = i + 37.5) {
                        ctx.moveTo(1.5, i);
                        ctx.lineTo(301.5, i);
                    }
                    for (i = 1.5; i <= 301.5; i = i + 37.5) {
                        ctx.moveTo(i, 1.5);
                        ctx.lineTo(i, 376.5);
                    }
                    ctx.stroke();
                },

                drawResults: function (percentages) {
                    ['A', 'B', 'C', 'D'].forEach(letter => {
                        const value = percentages[letter] ? percentages[letter] : 0;
                        setText(`audienceResults${letter}`, value + "%");
                    });

                    const ctx = this.getContext();
                    for (const letter of Object.keys(percentages)) {
                        const pos = 375 - Math.round((percentages[letter] / 100) * 375);
                        const xPosition = letter.charCodeAt(0) - 'A'.charCodeAt(0);

                        const grd = ctx.createLinearGradient(0, 1 + pos, 0, 376);
                        grd.addColorStop(0, "#3ddbff");
                        grd.addColorStop(1, "#a45de0");
                        ctx.fillStyle = grd;
                        ctx.fillRect(13.75 + (xPosition * 75), 1 + pos, 50.5, 376);
                    }
                },

                reset: function () {
                    this.getContext().clearRect(0, 0, 303, 378);
                }
            }
        }
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
connection.on("UseAskTheAudience", millionaire.lifelines.audience.use);
connection.on("StartPhoneClock", millionaire.lifelines.phone.onStart);
connection.on("SetAudienceAnswersOnClick", millionaire.lifelines.audience.setAnswersOnClick);
connection.on("ResetAudienceAnswersOnClick", millionaire.lifelines.audience.resetAnswersOnClick);
connection.on("DrawAudienceGraphGrid", millionaire.lifelines.audience.graph.drawGrid.bind(millionaire.lifelines.audience.graph));
connection.on("DrawAudienceGraphResults", millionaire.lifelines.audience.graph.drawResults.bind(millionaire.lifelines.audience.graph));
connection.on("ResetAudienceGraph", millionaire.lifelines.audience.graph.reset.bind(millionaire.lifelines.audience.graph));
connection.on("LockAudienceSubmission", millionaire.lifelines.audience.lock);