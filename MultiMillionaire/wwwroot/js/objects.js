const game = {
    join: {
        joinSuccessful: () => {
            addEventListener("beforeunload", beforeUnloadListener, {capture: true});
            modals.joinGameModal.hide();
        },

        validateForm: () => {
            const nameInput = document.getElementById("playerNameInput");
            const gameIdInput = document.getElementById("gameIdInput");
            return nameInput.reportValidity() && (!gameIdInput || gameIdInput.reportValidity());
        }
    }
}

const players = {
    joined: playerData => {
        
    }
}

const modals = {
    joinGameModal: new bootstrap.Modal('#joinGameModal')
}