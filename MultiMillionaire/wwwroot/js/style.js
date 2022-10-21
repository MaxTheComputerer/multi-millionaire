function getInnerWidth(element) {
    const cs = getComputedStyle(element);
    const paddingX = parseFloat(cs.paddingLeft) + parseFloat(cs.paddingRight);
    const borderX = parseFloat(cs.borderLeftWidth) + parseFloat(cs.borderRightWidth);
    return element.offsetWidth - paddingX - borderX;
}

function scaleToFit(element) {
    const elementWidth = getInnerWidth(element);
    const parentWidth = getInnerWidth(element.parentElement);
    const scale = Math.min((parentWidth - 50) / elementWidth, 1);
    element.style.transform = 'scaleX(' + scale + ')';
}

function scaleAnswerTexts() {
    const elements = document.querySelectorAll('.answer-text');
    Array.from(elements).forEach(element => {
        scaleToFit(element);
    });
}

window.addEventListener('resize', scaleAnswerTexts, true);