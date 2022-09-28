function Modal(id) {
    this.id = id;
    this.get = () => bootstrap.Modal.getOrCreateInstance(`#${this.id}`);
    this.show = () => this.get().show();
    this.hide = () => this.get().hide();
}

const modals = {
    joinGameModal: new Modal('joinGameModal'),
    gameEndedModal: new Modal('gameEndedModal')
}

function show(element, display = "block") {
    if (typeof element === 'string' || element instanceof String) {
        element = document.getElementById(element);
    }
    element.style.display = display;
}

function hide(element) {
    if (typeof element === 'string' || element instanceof String) {
        element = document.getElementById(element);
    }
    element.style.display = "none";
}