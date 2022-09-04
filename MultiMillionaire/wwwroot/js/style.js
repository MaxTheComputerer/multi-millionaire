function scaleAnswerTexts() {
    $('.answer-text').each(function(i, obj) {
        const element = $(this);
        const scale = Math.min((element.parent().width() - 40) / element.width(), 1);
        element.css('transform','scaleX('+ scale +')');
    });
}

window.addEventListener('resize', scaleAnswerTexts, true);