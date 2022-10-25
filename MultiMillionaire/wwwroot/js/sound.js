function getDescendantProp(obj, desc) {
    const arr = desc.split('.');
    while (arr.length) {
        obj = obj[arr.shift()];
    }
    return obj;
}

const sounds = {
    create: (src, loop = false, volume = 1, onend = () => {
    }, onstop = () => {
    }) => {
        return new Howl({
            src: ["/sounds/" + src],
            loop: loop,
            volume: volume,
            onend: onend,
            onstop: onstop
        });
    },

    getObjectFromCacheOrPath: function (path, callback) {
        const cacheItem = this.pathCache[path];
        if (cacheItem) {
            return callback(cacheItem);
        } else {
            const soundObject = getDescendantProp(soundLibrary, path);
            if (soundObject instanceof Howl) {
                this.pathCache[path] = soundObject;
                return callback(soundObject);
            }
        }
    },

    play: function (path, attack = 40) {
        this.getObjectFromCacheOrPath(path, sound => {
            if (sound.playing()) return;
            const volume = sound.volume();
            sound.volume(0);
            sound.play();
            sound.fade(0, volume, attack);
        });
    },

    stop: function (path) {
        this.getObjectFromCacheOrPath(path, sound => {
            if (sound.playing()) sound.stop();
        });
    },

    fadeOut: function (path, duration = 400) {
        this.getObjectFromCacheOrPath(path, async sound => {
            if (!sound.playing()) return;

            const volume = sound.volume();
            sound.once("fade", function () {
                this.stop();
                this.volume(volume);
            });
            await sleep(500);
            sound.fade(sound.volume(), 0, duration);
        });
    },

    isPlaying: function (path) {
        return this.getObjectFromCacheOrPath(path, sound => {
            return sound.playing();
        });
    },

    stopAll: () => Howler.stop(),

    pathCache: []
}

const soundLibrary = {
    music: {
        hotSeat: sounds.create("music/hotseat.mp3"),

        walk: sounds.create("music/walk.mp3", false, 1, async () => {
            await sleep(1000);
            sounds.play("music.closing");
        }),

        gameOver: sounds.create("music/gameover.mp3", false, 1, () => {
            sounds.play("music.closing");
        }),

        closing: sounds.create("music/closing.mp3")
    },

    fastestFinger: {
        start: sounds.create("fastest-finger/start.mp3", true),
        question: sounds.create("fastest-finger/question.mp3", true),
        vote: sounds.create("fastest-finger/vote.mp3"),
        earlyEnd: sounds.create("fastest-finger/end-vote.mp3"),
        answers: {
            background: sounds.create("fastest-finger/answer-reveal-bg.mp3", true),
            init: function () {
                for (let i = 0; i <= 3; i++) {
                    this[i] = sounds.create(`fastest-finger/answer-${i}.mp3`);
                }
                return this;
            }
        }.init(),
        resultsReveal: sounds.create("fastest-finger/results-reveal.mp3"),
        winner: sounds.create("fastest-finger/winner-reveal.mp3")
    },

    questions: {
        lightsDown: {
            1: (oneSixEleven = sounds.create("questions/lights-down/1-6-11.mp3")),
            6: oneSixEleven,
            7: (sevenTwelve = sounds.create("questions/lights-down/7-12.mp3")),
            8: (eightThirteen = sounds.create("questions/lights-down/8-13.mp3")),
            9: (nineFourteen = sounds.create("questions/lights-down/9-14.mp3")),
            10: (tenFifteen = sounds.create("questions/lights-down/10-15.mp3")),
            11: oneSixEleven,
            12: sevenTwelve,
            13: eightThirteen,
            14: nineFourteen,
            15: tenFifteen
        },

        music: {
            init: function () {
                const oneToFive = sounds.create("questions/music/1to5.mp3", true);
                for (let i = 1; i <= 5; i++) {
                    this[i] = oneToFive;
                }
                for (let i = 6; i <= 15; i++) {
                    this[i] = sounds.create(`questions/music/${i}.mp3`, true);
                }
                return this;
            }
        }.init(),

        final: {
            6: (sixEleven = sounds.create("questions/final/6-11.mp3")),
            7: (sevenTwelve = sounds.create("questions/final/7-12.mp3")),
            8: (eightThirteen = sounds.create("questions/final/8-13.mp3")),
            9: (nineFourteen = sounds.create("questions/final/9-14.mp3")),
            10: (tenFifteen = sounds.create("questions/final/10-15.mp3")),
            11: sixEleven,
            12: sevenTwelve,
            13: eightThirteen,
            14: nineFourteen,
            15: tenFifteen
        },

        correct: {
            init: function () {
                const oneToFour = sounds.create("questions/correct/1to4.mp3");
                for (let i = 1; i <= 4; i++) {
                    this[i] = oneToFour;
                }
                for (let i = 5; i <= 15; i++) {
                    this[i] = sounds.create(`questions/correct/${i}.mp3`);
                }
                return this;
            }
        }.init(),

        incorrect: {
            init: function () {
                const oneToFive = sounds.create("questions/incorrect/1to5.mp3");
                for (let i = 1; i <= 5; i++) {
                    this[i] = oneToFive;
                }
                return this;
            },
            6: (sixEleven = sounds.create("questions/incorrect/6-11.mp3")),
            7: (sevenTwelve = sounds.create("questions/incorrect/7-12.mp3")),
            8: (eightThirteen = sounds.create("questions/incorrect/8-13.mp3")),
            9: (nineFourteen = sounds.create("questions/incorrect/9-14.mp3")),
            10: sounds.create("questions/incorrect/10.mp3"),
            11: sixEleven,
            12: sevenTwelve,
            13: eightThirteen,
            14: nineFourteen,
            15: sounds.create("questions/incorrect/15.mp3"),
        }.init()
    },

    lifelines: {
        fiftyFifty: sounds.create("lifelines/50-50.mp3"),

        phone: {
            start: sounds.create("lifelines/phone-start.mp3", true),
            clock: sounds.create("lifelines/phone-clock.mp3", false, 1, (nextFn = async () => await connection.invoke("RequestQuestionMusic")), nextFn),
            earlyEnd: sounds.create("lifelines/phone-early-end.mp3")
        },

        audience: {
            start: sounds.create("lifelines/audience-start.mp3", true),
            vote: sounds.create("lifelines/audience-vote.mp3", true),
            results: sounds.create("lifelines/audience-results.mp3")
        }
    }
}

connection.on("PlaySound", sounds.play.bind(sounds));
connection.on("StopSound", sounds.stop.bind(sounds));
connection.on("FadeOutSound", sounds.fadeOut.bind(sounds));
connection.on("StopAllSounds", sounds.stopAll);