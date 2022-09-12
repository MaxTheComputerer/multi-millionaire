function scaleAnswerTexts() {
    $('.answer-text').each(function (i, obj) {
        const element = $(this);
        const scale = Math.min((element.parent().width() - 40) / element.width(), 1);
        element.css('transform', 'scaleX(' + scale + ')');
    });
}

window.addEventListener('resize', scaleAnswerTexts, true);

new bootstrap.Modal('#joinGameModal').show();


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

drawAudienceGrid(document.getElementById("audienceGraph"));