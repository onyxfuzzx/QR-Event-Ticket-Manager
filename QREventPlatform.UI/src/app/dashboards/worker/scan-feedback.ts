let successAudio: HTMLAudioElement | null = null;
let errorAudio: HTMLAudioElement | null = null;
let audioUnlocked = false;

/**
 * Call ONCE on first user interaction (button click / scanner open)
 */
export function unlockAudio() {
  if (audioUnlocked) return;

  try {
    successAudio = new Audio('/assets/sounds/success.wav');
    errorAudio = new Audio('/assets/sounds/error.wav');

    successAudio.volume = 0.8;
    errorAudio.volume = 0.8;

    // iOS Safari requires a play() during user gesture
    successAudio.play().then(() => {
      successAudio?.pause();
      successAudio!.currentTime = 0;
      audioUnlocked = true;
    }).catch(() => {
      // still unlocked enough for later
      audioUnlocked = true;
    });

  } catch {
    audioUnlocked = false;
  }
}

/**
 * Play scan feedback
 */
export function playScanFeedback(success: boolean) {
  try {
    // 📳 VIBRATION (mobile only)
    if (navigator.vibrate) {
      navigator.vibrate(success ? 200 : [200, 100, 200]);
    }

    // 🔊 SOUND
    const audio = success ? successAudio : errorAudio;

    if (audio) {
      audio.currentTime = 0;
      audio.play().catch(() => {
        // autoplay blocked → ignore safely
      });
    }

  } catch {
    // NEVER break scan flow
  }
}
