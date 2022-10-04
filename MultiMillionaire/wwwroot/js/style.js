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


// Ask the audience graph

function drawAudienceGrid(canvas) {
    const ctx = canvas.getContext("2d");
    ctx.globalCompositeOperation = 'source-over';

    ctx.shadowBlur = 3;
    ctx.shadowColor = "#2b88e7";
    ctx.strokeStyle = "#2b88e7";
    ctx.lineWidth = 2;

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
}

// drawAudienceGrid(document.getElementById("audienceGraph"));