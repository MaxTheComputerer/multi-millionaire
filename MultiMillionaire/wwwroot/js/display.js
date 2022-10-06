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

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
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

function setAnswerText(id, text) {
    const element = document.getElementById(id).querySelector(".answer-text");
    element.style.visibility = "hidden";
    element.innerText = text;
    scaleToFit(element);
    element.style.visibility = "visible";
}

function setOnClick(id, methodName) {
    const element = document.getElementById(id);
    element.onclick = async () => await connection.invoke(methodName);
}

function unlock(id) {
    const element = document.getElementById(id);
    element.classList.remove("disabled");
}

function lock(id) {
    const element = document.getElementById(id);
    element.classList.add("disabled");
}

connection.on("Show", show);
connection.on("Hide", hide);
connection.on("SetText", setText);
connection.on("SetOnClick", setOnClick);
connection.on("Lock", lock);
connection.on("Unlock", unlock);