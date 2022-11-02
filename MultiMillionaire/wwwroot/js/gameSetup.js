const settings = {
    toggle: () => {
        const settingsPanel = document.getElementById("settingsPanel");
        if (settingsPanel.style.display === "none") {
            hide("playersListPanel");
            show("settingsPanel");
        } else {
            hide("settingsPanel");
            show("playersListPanel");
        }
    },

    updateSwitchSetting: async id => {
        const element = document.getElementById(id);
        await connection.send("UpdateSwitchSetting", id, element.checked);
    },

    updateTextSetting: async id => {
        const element = document.getElementById(id);
        await connection.send("UpdateTextSetting", id, element.value);
    }
}

const questionEditor = {
    request: async () => {
        disable("editQuestionsBtn");
        await connection.invoke("RequestQuestionEditor");
        enable("editQuestionsBtn");
    },

    show: () => modals.questionEditorModal.show(),

    regenerate: async questionNumber => {
        setText(`question${questionNumber}`, "Loading...");
        ['A', 'B', 'C', 'D'].forEach(letter => {
            setAnswerText(`question${questionNumber}_answer${letter}`, "...");
        });
        await connection.send("RegenerateQuestion", questionNumber);
    }
}

connection.on("ShowQuestionEditor", questionEditor.show);