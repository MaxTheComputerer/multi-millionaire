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

function show(id, display = "block") {
    const element = document.getElementById(id);
    element.style.display = display;
}

function hide(id) {
    const element = document.getElementById(id);
    element.style.display = "none";
}

function setText(id, text) {
    const element = document.getElementById(id);
    element.innerText = text;
}

function setOnClick(id, onclick) {
    const element = document.getElementById(id);
    element.onclick = onclick;
}

connection.on("Show", show);
connection.on("Hide", hide);
connection.on("SetText", setText);
connection.on("SetOnClick", setOnClick);