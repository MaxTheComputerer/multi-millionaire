function getDescendantProp(obj, desc) {
    const arr = desc.split('.');
    while (arr.length) {
        obj = obj[arr.shift()];
    }
    return obj;
}

const sounds = {
    create: (src, loop = false, volume = 1, onend = () => {
    }) => {
        return new Howl({
            src: ["/sounds/" + src],
            loop: loop,
            volume: volume,
            onend: onend
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
        this.getObjectFromCacheOrPath(path, sound => {
            if (!sound.playing()) return;

            const volume = sound.volume();
            sound.once("fade", function () {
                this.stop();
                this.volume(volume);
            });
            sound.fade(sound.volume(), 0, duration);
        });
    },

    pathCache: []
}

const soundLibrary = {
    music: {
        closing: sounds.create("music/closing.mp3")
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
    }
}

connection.on("PlaySound", sounds.play.bind(sounds));
connection.on("StopSound", sounds.stop.bind(sounds));
connection.on("FadeOutSound", sounds.fadeOut.bind(sounds));