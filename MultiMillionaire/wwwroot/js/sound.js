function getDescendantProp(obj, desc) {
    const arr = desc.split('.');
    while (arr.length) {
        obj = obj[arr.shift()];
    }
    return obj;
}

const sounds = {
    create: function (src, loop = false, volume = 1, onend = () => {
    }, onstop = () => {
    }) {
        return new Howl({
            src: ["/sounds/" + src + ".webm", "/sounds/" + src + ".mp3"],
            loop: loop,
            volume: volume,
            onend: onend,
            onstop: onstop,
            onload: this.onLoad.bind(sounds),
            preload: false
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

    loadCounter: 0,
    loadEvent: new Event("soundsLoaded"),

    onLoad: function () {
        this.loadCounter++;
        if (this.loadCounter === 62) {
            window.dispatchEvent(this.loadEvent);
        }
    },

    load: () => {
        function loadRecurse(obj) {
            for (const key of Object.keys(obj)) {
                const value = obj[key];
                if (value instanceof Howl) {
                    if (value.state() === "unloaded") value.load();
                } else if (typeof value == "object" && value !== null) {
                    loadRecurse(value);
                }
            }
        }

        console.log("start load");
        loadRecurse(soundLibrary);
    },

    play: function (path, attack = 40) {
        this.getObjectFromCacheOrPath(path, sound => {
            if (sound.playing()) return;
            if (sound.state() === "unloaded") sound.load();
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
        hotSeat: sounds.create("music/hotseat"),

        walk: sounds.create("music/walk", false, 1, async () => {
            await sleep(1000);
            sounds.play("music.closing");
        }),

        gameOver: sounds.create("music/gameover", false, 1, () => {
            sounds.play("music.closing");
        }),

        closing: sounds.create("music/closing")
    },

    fastestFinger: {
        start: sounds.create("fastest-finger/start", true),
        question: sounds.create("fastest-finger/question", true),
        vote: sounds.create("fastest-finger/vote"),
        earlyEnd: sounds.create("fastest-finger/end-vote"),
        answers: {
            background: sounds.create("fastest-finger/answer-reveal-bg", true),
            init: function () {
                for (let i = 0; i <= 3; i++) {
                    this[i] = sounds.create(`fastest-finger/answer-${i}`);
                }
                return this;
            }
        }.init(),
        resultsReveal: sounds.create("fastest-finger/results-reveal"),
        winner: sounds.create("fastest-finger/winner-reveal")
    },

    questions: {
        lightsDown: {
            1: (oneSixEleven = sounds.create("questions/lights-down/1-6-11")),
            6: oneSixEleven,
            7: (sevenTwelve = sounds.create("questions/lights-down/7-12")),
            8: (eightThirteen = sounds.create("questions/lights-down/8-13")),
            9: (nineFourteen = sounds.create("questions/lights-down/9-14")),
            10: (tenFifteen = sounds.create("questions/lights-down/10-15")),
            11: oneSixEleven,
            12: sevenTwelve,
            13: eightThirteen,
            14: nineFourteen,
            15: tenFifteen
        },

        music: {
            init: function () {
                const oneToFive = sounds.create("questions/music/1to5", true);
                for (let i = 1; i <= 5; i++) {
                    this[i] = oneToFive;
                }
                for (let i = 6; i <= 15; i++) {
                    this[i] = sounds.create(`questions/music/${i}`, true);
                }
                return this;
            }
        }.init(),

        final: {
            6: (sixEleven = sounds.create("questions/final/6-11")),
            7: (sevenTwelve = sounds.create("questions/final/7-12")),
            8: (eightThirteen = sounds.create("questions/final/8-13")),
            9: (nineFourteen = sounds.create("questions/final/9-14")),
            10: (tenFifteen = sounds.create("questions/final/10-15")),
            11: sixEleven,
            12: sevenTwelve,
            13: eightThirteen,
            14: nineFourteen,
            15: tenFifteen
        },

        correct: {
            init: function () {
                const oneToFour = sounds.create("questions/correct/1to4");
                for (let i = 1; i <= 4; i++) {
                    this[i] = oneToFour;
                }
                for (let i = 5; i <= 15; i++) {
                    this[i] = sounds.create(`questions/correct/${i}`);
                }
                return this;
            }
        }.init(),

        incorrect: {
            init: function () {
                const oneToFive = sounds.create("questions/incorrect/1to5");
                for (let i = 1; i <= 5; i++) {
                    this[i] = oneToFive;
                }
                return this;
            },
            6: (sixEleven = sounds.create("questions/incorrect/6-11")),
            7: (sevenTwelve = sounds.create("questions/incorrect/7-12")),
            8: (eightThirteen = sounds.create("questions/incorrect/8-13")),
            9: (nineFourteen = sounds.create("questions/incorrect/9-14")),
            10: sounds.create("questions/incorrect/10"),
            11: sixEleven,
            12: sevenTwelve,
            13: eightThirteen,
            14: nineFourteen,
            15: sounds.create("questions/incorrect/15"),
        }.init()
    },

    lifelines: {
        fiftyFifty: sounds.create("lifelines/50-50"),

        phone: {
            start: sounds.create("lifelines/phone-start", true),
            clock: sounds.create("lifelines/phone-clock", false, 1, (nextFn = async () => await connection.send("RequestQuestionMusic")), nextFn),
            earlyEnd: sounds.create("lifelines/phone-early-end")
        },

        audience: {
            start: sounds.create("lifelines/audience-start", true),
            vote: sounds.create("lifelines/audience-vote", true),
            results: sounds.create("lifelines/audience-results")
        }
    }
}

connection.on("PlaySound", sounds.play.bind(sounds));
connection.on("StopSound", sounds.stop.bind(sounds));
connection.on("FadeOutSound", sounds.fadeOut.bind(sounds));
connection.on("StopAllSounds", sounds.stopAll);