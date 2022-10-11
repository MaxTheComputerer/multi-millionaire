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
connection.on("Lock", lock);
connection.on("Unlock", unlock);
connection.on("SetBackground", setBackground);