function Modal(id) {
    this.id = id;
    this.get = () => bootstrap.Modal.getOrCreateInstance(`#${this.id}`);
    this.show = () => this.get().show();
    this.hide = () => this.get().hide();
}

const modals = {
    joinGameModal: new Modal('joinGameModal'),
    gameEndedModal: new Modal('gameEndedModal'),
    disconnectedModal: new Modal('disconnectedModal'),
    choosePlayerModal: new Modal('choosePlayerModal'),
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
    element.textContent = text;
}

function setAnswerText(id, text) {
    const element = document.getElementById(id).querySelector(".answer-text");
    element.style.visibility = "hidden";
    element.textContent = text;
    scaleToFit(element);
    element.style.visibility = "visible";
}

function setOnClick(id, methodName, charArg = null) {
    const element = document.getElementById(id);
    element.onclick = charArg
        ? async () => await connection.send(methodName, charArg)
        : async () => await connection.send(methodName);
}

function enable(id) {
    const element = document.getElementById(id);
    element.classList.remove("disabled");
}

function disable(id) {
    const element = document.getElementById(id);
    element.classList.add("disabled");
}

async function flash(id, flashCount = 5, delay = 150, startsOn = false, remainOn = true, className = "correct") {
    const element = document.getElementById(id);
    if (startsOn) {
        element.classList.remove(className);
        await sleep(delay);
        flashCount--;
    }

    for (let i = 0; i < flashCount; i++) {
        element.classList.add(className);
        await sleep(delay);
        element.classList.remove(className);
        await sleep(delay);
    }
    if (remainOn) element.classList.add(className);
}

function setBackground(imageNumber, useRedVariant) {
    document.body.className = "bg-" + (useRedVariant ? "red-" : "") + imageNumber;
}

connection.on("Show", show);
connection.on("Hide", hide);
connection.on("SetText", setText);
connection.on("SetAnswerText", setAnswerText);
connection.on("SetOnClick", setOnClick);
connection.on("Disable", disable);
connection.on("Enable", enable);
connection.on("SetBackground", setBackground);