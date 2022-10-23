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
        await connection.invoke("UpdateSwitchSetting", id, element.checked);
    },

    updateTextSetting: async id => {
        const element = document.getElementById(id);
        await connection.invoke("UpdateTextSetting", id, element.value);
    }
}