import { createStore } from 'vuex';
import { jwtDecode } from 'jwt-decode';
import metronomeSettingsService from '@/services/metronomeSettingsService';
import { getAudioContext } from '@/helpers/audioContextHelper'

const store = createStore({
    state: {
        //audioUrls: ["http://localhost:5173/src/assets/audio/Fingers1.wav",
        //    "http://localhost:5173/src/assets/audio/Fingers2.wav",
        //    "http://localhost:5173/src/assets/audio/Stick.wav"
        //],
        audioUrls: ["audio/Fingers1.wav",
            "audio/Fingers2.wav",
            "audio/Stick.wav"
        ],
        clickBuffers: [],
        allFilesReady: true,
        savedSettingsId: null,
        nickname: '',
        currentPresetTitle: '', // <-- new: title of currently applied preset (demo or saved)
        defaultSettings: {
            sourceMs: 500,
            targetMs: 500,
            interval: null,
            tempo: 120,
            sourceBpm: 120,
            targetBpm: 120,
            minRegularRang: 20,
            maxRegularRang: 280,
            tempoPeriodBpm: 0,
            nextTempo: 0,
            tempoText: 'Allegro',
            beatsPerBar: 4,
            isPlaying: false,
            barsPerPattern: 0,
            isRepeatPattern: false,
            tempoPeriods: 0,
            countTempoPeriod: 1,
            twoDecimalPlaces: '.00',
            isTempoIncreace: true,
            isPlayingPeriod: false,
            isServiceTargetAreDifference: false,
            isReverse: false,
            isReverseBack: false,

            pendingConfirmation: null, // { success: boolean, message: string } or null
        },
        audioContext: null,
        interval: null,
        // Milliseconds start
        sourceMs: 500,
        targetMs: 500,
        intervalTime: 0,
        tempoPeriodMs: 0,
        nextTempoMs: 0,
        // Milliseconds end
        // Bpm start
        tempo: 120,
        sourceBpm: 120,
        targetBpm: 120,
        rangeTempo: [20, 280],
        minRegularRang: 20,
        maxRegularRang: 280,
        tempoPeriodBpm: 0,
        nextTempo: 0,
        // Bpm end
        isOpenSettings: false,
        activeSettingsTab: null, // null | 'auth' | 'settings' | 'presets' | 'saveSettingsAs'
        tempoText: 'Allegro',
        beatsPerBar: 4,
        isPlaying: false,
        
        barsPerPattern: 0,
        isRepeatPattern: false,
        tempoPeriods: 0,
        countTempoPeriod: 1,
        
        twoDecimalPlaces: '.00',
        isTempoIncreace: true,
        isPlayingPeriod: false,

        token: localStorage.getItem('authToken') || null,
        refreshToken: localStorage.getItem('refreshToken') || null,
        isServiceTargetAreDifference: false,
        isReverse: false,
        isReverseBack: false,

        // Add pendingReset to hold { email, token } while user is on reset page/modal
        pendingReset: null
    },
    mutations: {
        setActiveSettingsTab(state, tabName) {
            state.activeSettingsTab = tabName;
        },
        setSavedSettingsId(state, id) {
            state.savedSettingsId = id
        },
        clearSavedSettingsId(state) {
            state.savedSettingsId = null
        },
        setToken(state, token) {
            state.token = token
            if (token) {
                localStorage.setItem('authToken', token)

                // Decode the token to get user information
                const decodedToken = jwtDecode(token)
                const nickname = decodedToken.unique_name || '';
                state.nickname = nickname;
                //localStorage.setItem('nickname', nickname);
            } else {
                localStorage.removeItem('authToken')
                //localStorage.removeItem('nickname')
                state.nickname = '';
            }
            
        },
        setRefreshToken(state, refreshToken) {
            state.refreshToken = refreshToken
            if (refreshToken) {
                localStorage.setItem('refreshToken', refreshToken)
            } else {
                localStorage.removeItem('refreshToken')
            }
        },
        setPendingConfirmation(state, payload) {
            state.pendingConfirmation = payload;
        },
        clearPendingConfirmation(state) {
            state.pendingConfirmation = null;
        },
        clearAuthData(state) {
            state.token = null
            state.refreshToken = null
            localStorage.removeItem('authToken')
            localStorage.removeItem('refreshToken')
        },
        setDefaultSettings(state) {
            state.savedSettingsId = null;
            state.sourceMs = state.defaultSettings.sourceMs;
            state.targetMs = state.defaultSettings.targetMs;
            state.interval = state.defaultSettings.interval;
            state.tempo = state.defaultSettings.tempo;
            state.sourceBpm = state.defaultSettings.sourceBpm;
            state.targetBpm = state.defaultSettings.targetBpm;
            state.minRegularRang = state.defaultSettings.minRegularRang;
            state.maxRegularRang = state.defaultSettings.maxRegularRang;
            state.tempoPeriodBpm = state.defaultSettings.tempoPeriodBpm;
            state.nextTempo = state.defaultSettings.nextTempo;
            state.tempoText = state.defaultSettings.tempoText;
            state.beatsPerBar = state.defaultSettings.beatsPerBar;
            state.isPlaying = state.defaultSettings.isPlaying;
            state.barsPerPattern = state.defaultSettings.barsPerPattern;
            state.isRepeatPattern = state.defaultSettings.isRepeatPattern;
            state.tempoPeriods = state.defaultSettings.tempoPeriods;
            state.countTempoPeriod = state.defaultSettings.countTempoPeriod;
            state.twoDecimalPlaces = state.defaultSettings.twoDecimalPlaces;
            state.isTempoIncreace = state.defaultSettings.isTempoIncreace;
            state.isPlayingPeriod = state.defaultSettings.isPlayingPeriod;
            state.isServiceTargetAreDifference = state.defaultSettings.isServiceTargetAreDifference;
            state.isReverse = state.defaultSettings.isReverse;
            state.isReverseBack = state.defaultSettings.isReverseBack;
        },
        loadPresetSettings(state, preset) {
            state.savedSettingsId = preset.id ?? null; // demo presets should pass null id
            state.currentPresetTitle = (preset.title || preset.Title) ?? '';

            state.sourceMs = Math.round(60000 / preset.sourceBpm);
            state.targetMs = Math.round(60000 / preset.targetBpm);
            state.tempo = preset.sourceBpm

            // Saved key parameters start
            state.sourceBpm = preset.sourceBpm
            state.targetBpm = preset.targetBpm
            state.barsPerPattern = preset.barsPerPattern
            state.tempoPeriods = preset.tempoPeriods
            state.isReverse = preset.isReversed
            state.isRepeatPattern = preset.isRepeatPattern
            state.beatsPerBar = preset.beatsPerBar
            state.isTempoIncreace = preset.isTempoIncreace
            // Saved key parameters end

            state.tempoPeriodBpm = state.defaultSettings.tempoPeriodBpm;
            state.nextTempo = state.defaultSettings.nextTempo;
            state.isPlaying = state.defaultSettings.isPlaying;
            state.countTempoPeriod = state.defaultSettings.countTempoPeriod;
            state.twoDecimalPlaces = state.defaultSettings.twoDecimalPlaces;
            state.isPlayingPeriod = state.defaultSettings.isPlayingPeriod;
            state.isServiceTargetAreDifference = state.defaultSettings.isServiceTargetAreDifference;
            state.isReverseBack = state.defaultSettings.isReverseBack;
        },
        initAudioContextBuffers(state, context) {
            state.audioContext = context;
            state.clickBuffers = new Array(state.audioUrls.length);
        },
        clearAudioContext(state) {
            state.audioContext = null;
        },
        initSounds(state, arrayBuffer) {
            state.clickBuffers = arrayBuffer.slice();
        },
        setAudioFiles(state, response) {
            state.allFilesReady = response;
        },
        updateIntervalTime(state) {
            state.intervalTime = 60000 / state.tempo;
        },
        setIntervalTime(state, newInterval) {
            state.intervalTime = newInterval;
        },
        setIsPlayingPeriod(state, value) {
            state.isPlayingPeriod = value;
        },
        setIntervalId(state, id) {
            state.interval = id;
        },
        updateIntervalMs(state, newValue) {
            if (newValue) {
                state.nextTempoMs = newValue;
            } else {
                if (state.isTempoIncreace) {
                    if (state.isReverseBack) {
                        state.nextTempoMs = state.intervalTime + state.tempoPeriodMs;
                    } else {
                        state.nextTempoMs = state.intervalTime - state.tempoPeriodMs; 
                    }
                } else {
                    if (state.isReverseBack) {
                        state.nextTempoMs = state.intervalTime - state.tempoPeriodMs;
                    } else {
                        state.nextTempoMs = state.intervalTime + state.tempoPeriodMs; 
                    }
                }
            }
        },
        setCurrentTempoBpm(state, newValue) {
            if (newValue) {
                state.nextTempo = newValue;
            } else {
                if (state.isTempoIncreace) {
                    if (state.isReverseBack) {
                        state.nextTempo = state.tempo - state.tempoPeriodBpm;
                    } else {
                        state.nextTempo = state.tempo + state.tempoPeriodBpm;
                    }
                } else {
                    if (state.isReverseBack) {
                        state.nextTempo = state.tempo + state.tempoPeriodBpm;
                    } else {
                        state.nextTempo = state.tempo - state.tempoPeriodBpm;
                    }
                }
            }
        },
        areServiceAndTargetDifference(state) {
            state.isServiceTargetAreDifference = state.sourceBpm !==
                state.targetBpm;
        },
        updateCountTempoPeriod(state, newValue) {
            if (newValue) {
                state.countTempoPeriod = newValue;
            } else {
                ++state.countTempoPeriod;
            }
        },
        updateTempoPeriodBpmMs(state) {
            state.tempoPeriodMs = 0;
            let bpmDifference = state.targetBpm - state.sourceBpm;
            state.isTempoIncreace = bpmDifference > 0;
            state.tempoPeriodBpm = Math.abs(bpmDifference / (state.tempoPeriods - 1));
            state.sourceMs = 60000 / state.sourceBpm;
            state.targetMs = 60000 / state.targetBpm;
            state.tempoPeriodMs = Math.abs((state.targetMs - state.sourceMs) / (state.tempoPeriods - 1));
            state.isReverseBack = false;
            state.countTempoPeriod = 1;
        },
        updateTempoParts(state, periodBpm) {
            if (periodBpm % 1 !== 0) {
                let rounded = periodBpm.toFixed(2);
                let roundedParts = rounded.split('.');
                state.tempo = Number(roundedParts[0]);
                state.twoDecimalPlaces = roundedParts[1] ? "." + roundedParts[1] : ".00";
            } else {
                state.tempo = periodBpm;
                state.twoDecimalPlaces = ".00";
            }
        },
        toggleSettingsPanel(state) {
            state.isOpenSettings = !state.isOpenSettings;
            if (!state.isOpenSettings) { 
                state.activeSettingsTab = null; // Reset on close
            }
        },
        openSettingsPanelWithTab(state, tabName) {
            if (state.isOpenSettings && state.activeSettingsTab === tabName) {
                // Close if same tab is already active
                state.isOpenSettings = false;
                state.activeSettingsTab = null;
            } else {
                state.isOpenSettings = true;
                state.activeSettingsTab = tabName; // Set the active tab
            }
        },
        toggleReverse(state, newValue) {
            state.isReverse = newValue;
        },
        setRegularMinRang(state, newRang) {
            state.minRegularRang = newRang;
        },
        setRegularMaxRang(state, newRang) {
            state.maxRegularRang = newRang;
        },
        setBarsPerPattern(state, numberOfBars) {
            state.barsPerPattern = numberOfBars;
        },
        setRepeatPattern(state, value) {
            state.isRepeatPattern = value;
        },
        setPatternRepeats(state, numberOfRepeats) {
            state.tempoPeriods = numberOfRepeats;
        },
        setRangeSourceTempo(state, newtempo) {
            state.sourceBpm = newtempo;
        },
        increaseBpmBtn(state) {
            state.tempo++;
        },
        setRangeTempo(state, newtempo) {
            state.tempo = newtempo;
        },
        decreaseBpmBtn(state) {
            state.tempo--;
        },
        setRangeTargetTempo(state, newtempo) {
            state.targetBpm = newtempo;
        },
        increaseBeatPerBar(state) {
            if (state.beatsPerBar < 7) {
                state.beatsPerBar++;
            }
        },
        decreaseBeatPerBar(state) {
            if (state.beatsPerBar > 1) {
                state.beatsPerBar--;
            }
        },
        setTempoText(state) {
            if (state.tempo < 40) { state.tempoText = 'Grave' }
            else if (state.tempo >= 40 && state.tempo < 45) { state.tempoText = 'Lento' }
            else if (state.tempo >= 45 && state.tempo < 55) { state.tempoText = 'Largo' }
            else if (state.tempo >= 55 && state.tempo < 65) { state.tempoText = 'Adagio' }
            else if (state.tempo >= 65 && state.tempo < 73) { state.tempoText = 'Adagietto' }
            else if (state.tempo >= 73 && state.tempo < 86) { state.tempoText = 'Andante' }
            else if (state.tempo >= 86 && state.tempo < 98) { state.tempoText = 'Moderato' }
            else if (state.tempo >= 98 && state.tempo < 109) { state.tempoText = 'Allegretto' }
            else if (state.tempo >= 109 && state.tempo < 132) { state.tempoText = 'Allegro' }
            else if (state.tempo >= 132 && state.tempo < 168) { state.tempoText = 'Vivace' }
            else if (state.tempo >= 168 && state.tempo < 200) { state.tempoText = 'Presto' }
            else if (state.tempo >= 200) { state.tempoText = 'Prestissimo' }
        },
        clearSpeedUpInterval(state) {
            clearTimeout(state.interval);
        },
        itPlaying(state) {
            state.isPlaying = true;
        },
        playSound(state, buffer) {
            const source = state.audioContext.createBufferSource();
            source.buffer = buffer;
            source.connect(state.audioContext.destination);
            source.start();
        },
        prePlayClick(state, buffer) {
            const source = state.audioContext.createBufferSource();
            source.buffer = buffer;
            source.connect(state.audioContext.destination);
            source.start();
            source.stop();
        },
        notPlaying(state) {
            state.isPlaying = false;
        },
        // New mutations for reset flow
        setPendingReset(state, payload) {
            // payload: { email: string, token: string }
            state.pendingReset = payload;
        },
        clearPendingReset(state) {
            state.pendingReset = null;
        },
    },
    actions: {
        setDefaultSettings({ state, commit, dispatch }) {
            if (state.isPlaying) {
                dispatch('stopMetronome');
            }

            commit('setDefaultSettings');
        },
        async loadPresetById({ state, commit, dispatch }, id) {
            if (state.isPlaying) {
                dispatch('stopMetronome')
            }
            
            try {
                const preset = await metronomeSettingsService.getUserPresetById(id)
                commit('loadPresetSettings', preset)
                commit('setTempoText');
                commit('setActiveSettingsTab', 'settings')
                commit('setSavedSettingsId', id)
                return true
            } catch (error) { 
                if (import.meta.env.DEV) {
                    console.error('Error loading preset by id: ', error)
                }
                return false
            }
        },
        // Only into actions start
        updateBarsAndPatternsInputs({ state, commit }) { 
            if (state.sourceBpm !== state.targetBpm) {
                if (state.barsPerPattern == 0) {
                    commit('setBarsPerPattern', 2);
                }
                
                if (state.tempoPeriods == 0) {
                    commit('setPatternRepeats', 2);
                }

                commit('updateTempoPeriodBpmMs');

                if (!state.isRepeatPattern) {
                    commit('setRepeatPattern', true);
                }
            } else {
                commit('setPatternRepeats', 0);
            }

            commit('areServiceAndTargetDifference');
        },
        updateRegularBpmMinMax({ state, commit }) { 
            const bpmDifference = state.targetBpm - state.sourceBpm;
            
            if (bpmDifference > 0) {
                commit('setRegularMaxRang', state.rangeTempo[1] - bpmDifference);
                commit('setRegularMinRang', state.rangeTempo[0]);
            } else if (bpmDifference < 0) {
                commit('setRegularMinRang', state.rangeTempo[0] - bpmDifference);
                commit('setRegularMaxRang', state.rangeTempo[1]);
            }
        },
        initAudioContextBuffers({ commit }) {
            const context = getAudioContext();
            if (!context) return;

            commit('initAudioContextBuffers', context);
        },
        closeAudioContext({ state, commit }) {
            if (state.audioContext && state.sudioContext.state !== 'closed') {
                state.audioContext.close();
                commit('clearAudioContext');
            }
        },
        async initSounds({ state, commit, dispatch }) {

            dispatch('initAudioContextBuffers');

            if (state.audioContext && (state.audioUrls.length === state.clickBuffers.length)) {
                let clickBuffer = new Array(state.audioUrls.length);
                for (let i = 0; i < state.audioUrls.length; i++) {
                    const response = await fetch(state.audioUrls[i]);
                    if (!response.ok) {
                        commit('setAudioFiles', response.ok);
                    } else {
                        const arrayBuffer = await response.arrayBuffer();
                        clickBuffer[i] = await state.audioContext.decodeAudioData(arrayBuffer);
                    }
                }

                if (state.allFilesReady) {
                    commit('initSounds', clickBuffer);
                }
            }
        },
        toggleSettingsPanel({ state, commit, dispatch }) {
            if (!state.audioContext) {
                dispatch('initSounds');
            }
            
            commit('toggleSettingsPanel');
        },
        updateBarsPerPattern({ state, commit, dispatch }, numberOfBars) {
            if (state.isPlaying) {
                dispatch('stopMetronome');
            }
            commit('setBarsPerPattern', numberOfBars);
        },
        setRepeatPattern({ state, commit, dispatch }, value) {
            if (state.isPlaying) {
                dispatch('stopMetronome');
            }
            commit('setRepeatPattern', value);
        },
        setPatternRepeats({ state, commit, dispatch }, numberOfPatterns) {
            if (state.isPlaying) {
                dispatch('stopMetronome');
            }

            commit('setPatternRepeats', numberOfPatterns);
            if (state.sourceBpm !== state.targetBpm) {
                commit('updateTempoPeriodBpmMs');
            }
        },
        toggleReverse({ state, commit, dispatch }, newValue) {
            if (state.isPlaying) {
                dispatch('stopMetronome');
            }

            commit('toggleReverse', newValue);
        },
        increaseSourceBpmBtn({ state, dispatch }) {
            if (state.sourceBpm < state.rangeTempo[1]) {
                const newValue = 1 + state.sourceBpm;
                dispatch('updateRangeSourceTempo', newValue);
            }
        },
        updateRangeSourceTempo({ state, commit, dispatch }, newTempo) {
            if (state.isPlaying) {
                dispatch('stopMetronome');
            }
            commit('setRangeSourceTempo', newTempo);
            dispatch('updateRegularBpmMinMax');
            commit('setRangeTempo', newTempo);
            commit('setTempoText');
            dispatch('updateBarsAndPatternsInputs');
            commit('updateTempoPeriodBpmMs');
            commit('updateIntervalTime');
        },
        decreaseSourceBpmBtn({ state, dispatch }) {
            if (state.sourceBpm > state.rangeTempo[0]) {
                const newValue = -1 + state.sourceBpm;
                dispatch('updateRangeSourceTempo', newValue);
            }
        },
        increaseTargetBpmBtn({ state,  dispatch }) {
            if (state.targetBpm < state.rangeTempo[1]) {
                const newValue = 1 + state.targetBpm;
                dispatch('updateRangeTargetTempo', newValue);
            }
        },
        updateRangeTargetTempo({ state, commit, dispatch }, newTempo) {
            if (state.isPlaying) {
                dispatch('stopMetronome');
            }

            commit('setRangeTargetTempo', newTempo);
            dispatch('updateBarsAndPatternsInputs');
            
            dispatch('updateRegularBpmMinMax');

            commit('updateTempoPeriodBpmMs');
            commit('updateIntervalTime');
        },
        decreaseTargetBpmBtn({ state, dispatch }) {
            if (state.targetBpm > state.rangeTempo[0]) {
                const newValue = -1 + state.targetBpm;
                dispatch('updateRangeTargetTempo', newValue);
            }
        },
        stopMetronome({ state, commit }) {
            if (state.isPlaying) {
                commit('clearSpeedUpInterval');
                commit('notPlaying');

                if (state.isServiceTargetAreDifference) {
                    commit('updateTempoParts', state.sourceBpm);
                    commit('setIntervalTime', state.sourceMs);
                    commit('updateIntervalMs', state.sourceMs);
                    commit('setCurrentTempoBpm', state.sourceBpm);
                    commit('updateCountTempoPeriod', 1);
                } else {
                    commit('setIsPlayingPeriod', false);
                }
            }
        },
        prePlayClick({ state, commit }) {
            if (state.isRepeatPattern) {
                commit('prePlayClick', state.clickBuffers[2]);
            } else {
                commit('prePlayClick', state.clickBuffers[0]);
            }
        },
        playMetronome({ state, commit }) {
            if (state.isPlaying) return; // Is already playing

            commit('itPlaying');

            if (state.isServiceTargetAreDifference) {
                if (!state.isPlayingPeriod) {
                    commit('updateTempoPeriodBpmMs');
                    commit('setIsPlayingPeriod', true);
                }
            } else {
                commit('updateIntervalTime');
            }

            let countBeat = 0;
            let countBar = 0;
            let expectedTime = performance.now() + state.intervalTime;

            const tick = () => {
                const now = performance.now();
                const drift = now - expectedTime;
                expectedTime += state.intervalTime;
                
                if (state.barsPerPattern > 0) {
                    if (countBar == 0) {
                        commit('playSound', state.clickBuffers[2]);
                    } else {
                        if (countBeat == 0) {
                            commit('playSound', state.clickBuffers[0]);
                        } else {
                            commit('playSound', state.clickBuffers[1]);
                        }
                    }

                    countBeat++;

                    if (countBeat === state.beatsPerBar) {
                        countBar++;
                        countBeat = 0;
                    }

                    if (countBar === state.barsPerPattern + 1) {
                        countBar = 0;

                        if (state.isServiceTargetAreDifference) {

                            commit('updateCountTempoPeriod');

                            if (state.countTempoPeriod > state.tempoPeriods) {
                                commit('updateCountTempoPeriod', 1);

                                if (state.isReverse) {
                                    if (state.isReverse !== state.isReverseBack) {
                                        state.isReverseBack = !state.isReverseBack;
                                        commit('updateTempoParts', state.targetBpm);
                                        commit('updateIntervalMs', state.targetMs);
                                    } else {
                                        state.isReverseBack = !state.isReverseBack;
                                        commit('updateTempoParts', state.sourceBpm);
                                        commit('updateIntervalMs', state.sourceMs);
                                    }
                                } else {
                                    commit('updateTempoParts', state.sourceBpm);
                                    commit('updateIntervalMs', state.sourceMs);
                                }
                            } else if (state.countTempoPeriod === state.tempoPeriods) {
                                if (state.isReverse) {
                                    if (state.isReverse === state.isReverseBack) {
                                        commit('updateTempoParts', state.sourceBpm);
                                        commit('updateIntervalMs', state.sourceMs);
                                    } else {
                                        commit('updateTempoParts', state.targetBpm);
                                        commit('updateIntervalMs', state.targetMs);
                                    }
                                } else {
                                    commit('updateTempoParts', state.targetBpm);
                                    commit('updateIntervalMs', state.targetMs);
                                }
                            } else {
                                commit('setCurrentTempoBpm');
                                commit('updateTempoParts', state.nextTempo);
                                commit('updateIntervalMs');
                            }
                        }

                        if (!state.isRepeatPattern) {
                            commit('clearSpeedUpInterval');
                            commit('notPlaying');
                        }
                    }
                } else {
                    if (state.beatsPerBar > 1) {
                        if (countBeat == 0) {
                            commit('playSound', state.clickBuffers[0]);
                        } else {
                            commit('playSound', state.clickBuffers[1]);
                        }

                        if (countBeat === state.beatsPerBar - 1) {
                            countBeat = 0;
                        } else {
                            countBeat++;
                        }
                    } else {
                        commit('playSound', state.clickBuffers[1]);
                    }
                }

                if (state.isPlaying) {
                    const nextTimeout = Math.max(0, state.intervalTime - drift);
                    const intervalId = setTimeout(tick, nextTimeout);
                    commit('setIntervalId', intervalId);
                }
            };

            const intervalId = setTimeout(tick, state.intervalTime);
            commit('setIntervalId', intervalId);
        },
        increaseBpmBtn({ state, commit, dispatch }) {
            if (!state.audioContext) {
                dispatch('initSounds');
            }

            if (state.tempo < state.rangeTempo[1]) {
                const newValue = 1 + state.sourceBpm;
                dispatch('updateRangeTempo', newValue);

                if (state.isPlaying) {
                    commit('updateIntervalTime');
                    dispatch('stopMetronome');
                }
            }
        },
        updateRangeTempo({ state, commit, dispatch }, newTempo) {
            if (!state.audioContext) {
                dispatch('initSounds');
            }

            if (state.isPlaying) {
                // Stop Metronome without to switch isPlaying variable
                commit('clearSpeedUpInterval');
            }
            
            if (state.sourceBpm !== state.targetBpm) {
                commit('notPlaying');
                
                const bpmDifference = state.targetBpm - state.sourceBpm;
                if (bpmDifference > 0 && (state.targetBpm <= state.rangeTempo[1] && newTempo <= state.rangeTempo[1])) {
                    commit('setRegularMaxRang', state.rangeTempo[1] - bpmDifference);

                    commit('setRangeTempo', newTempo);
                    commit('setTempoText');

                    commit('setRangeSourceTempo', newTempo);
                    commit('setRangeTargetTempo', newTempo + bpmDifference);
                } else if (bpmDifference < 0 && (state.targetBpm >= state.rangeTempo[0] && newTempo >= state.rangeTempo[0])) {
                    commit('setRegularMinRang', state.rangeTempo[0] - bpmDifference);

                    commit('setRangeTempo', newTempo);
                    commit('setTempoText');

                    commit('setRangeSourceTempo', newTempo);
                    commit('setRangeTargetTempo', newTempo + bpmDifference);
                }

                commit('updateTempoPeriodBpmMs');
                commit('updateIntervalTime');

            } else {
                commit('setRangeTempo', newTempo);
                commit('setTempoText');

                commit('setRangeSourceTempo', newTempo);
                commit('setRangeTargetTempo', newTempo);
            }

            // Let's keep the rithm going
            if (state.isPlaying) {
                commit('notPlaying');
                commit('updateIntervalTime');
                dispatch('playMetronome');
            }
        },
        decreaseBpmBtn({ state, commit, dispatch }) {
            if (!state.audioContext) {
                dispatch('initSounds');
            }

            if (state.tempo > state.rangeTempo[0]) {
                const newValue = -1 + state.sourceBpm;
                dispatch('updateRangeTempo', newValue);

                if (state.isPlaying) {
                    commit('updateIntervalTime');
                    dispatch('stopMetronome');
                }
            }
        }
    },
    getters: {
        isAuthenticated: state => !!state.token,
        getSavedSettingsId: (state) => state.savedSettingsId,
        getNickname: (state) => state.nickname,
        getToken: (state) => state.token,
        getAudioContext: (state) => state.audioContext,
        getInterval: (state) => state.interval,
        getAudioFiles: (state) => state.audioFiles,
        areAllFilesReady: (state) => state.allFilesReady,
        getnextTempo: (state) => state.nextTempo,
        getNextTempoMs: (state) => state.nextTempoMs,
        getRegularMinRang(state) {
            return state.minRegularRang;
        },
        getRegularMaxRang(state) {
            return state.maxRegularRang;
        },
        getSettingsPanel(state) {
            return state.isOpenSettings
        },
        getActiveSettingsTab: state => state.activeSettingsTab,
        getBarsPerPattern(state) {
            return state.barsPerPattern
        },
        isRepeatPattern: (state) => state.isRepeatPattern,
        getPatternRepeats: (state) => state.tempoPeriods,
        getSourceBpm(state) {
            return state.sourceBpm
        },
        getTargetBpm(state) {
            return state.targetBpm
        },
        getTempo(state) {
            return state.tempo
        },
        getMinRangeTempo(state) {
            return state.rangeTempo[0];
        },
        getMaxRangeTempo(state) {
            return state.rangeTempo[1];
        },
        getTempoText(state) {
            return state.tempoText;
        },
        getBeatsPerBar(state) {
            return state.beatsPerBar;
        },
        getPlayState(state) {
            return state.isPlaying;
        },
        getTwoDecimalPlaces(state) {
            return state.twoDecimalPlaces;
        },
        getCurrentPresetTitle(state) { // <-- new getter
            return state.currentPresetTitle || '';
        },
        getIsReverse(state) {
            return state.isReverse;
        },
        getIsServiceAndTargetAreDifference(state) {
            return state.isServiceTargetAreDifference;
        },
        isSettingsChanged(state) {
            return state.sourceMs !== state.defaultSettings.sourceMs ||
                state.targetMs !== state.defaultSettings.targetMs ||
                state.tempo !== state.defaultSettings.tempo ||
                state.sourceBpm !== state.defaultSettings.sourceBpm ||
                state.targetBpm !== state.defaultSettings.targetBpm ||
                state.minRegularRang !== state.defaultSettings.minRegularRang ||
                state.maxRegularRang !== state.defaultSettings.maxRegularRang ||
                state.tempoPeriodBpm !== state.defaultSettings.tempoPeriodBpm ||
                state.nextTempo !== state.defaultSettings.nextTempo ||
                state.tempoText !== state.defaultSettings.tempoText ||
                state.beatsPerBar !== state.defaultSettings.beatsPerBar ||
                state.barsPerPattern !== state.defaultSettings.barsPerPattern ||
                state.isRepeatPattern !== state.defaultSettings.isRepeatPattern ||
                state.tempoPeriods !== state.defaultSettings.tempoPeriods ||
                state.countTempoPeriod !== state.defaultSettings.countTempoPeriod ||
                state.isTempoIncreace !== state.defaultSettings.isTempoIncreace ||
                state.isPlayingPeriod !== state.defaultSettings.isPlayingPeriod ||
                state.isReverse !== state.defaultSettings.isReverse ||
                state.isReverseBack !== state.defaultSettings.isReverseBack;
        }
    },
});

export default store;