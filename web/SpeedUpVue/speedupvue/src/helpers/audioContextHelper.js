// audioContextHelper.js
// Helper functions for working with the Web Audio API AudioContext

let currentAudioContext = null;

/**
 * Get a singleton AudioContext instance.
 * @returns {AudioContext}
 */
export function getAudioContext() {
    const AudioCtx = window.AudioContext || window.webkitAudioContext || window.mozAudioContext;

    if (!AudioCtx) {
        if (import.meta.env.DEV) {
            console.warn("Your browser doesn't support Web Audio API");
        }
        return null;
    }

    // Close existing context if it's running
    if (currentAudioContext && currentAudioContext.state !== 'closed') {
        currentAudioContext.close().then(() => {
            if (import.meta.env.DEV) {
                console.log('Previous AudioContext closed.');
            }
        }).catch(error => {
            if (import.meta.env.DEV) {
                console.error('Error closing existing AudioContext:', error);
            }
        });
    }

    // Create new context and store reference
    currentAudioContext = new AudioCtx();

    return currentAudioContext;
}

export function getCurrentAudioContext() {
    return currentAudioContext;
}