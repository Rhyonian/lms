(function () {
  const COMMIT_ENDPOINT = window.SCORM_COMMIT_ENDPOINT || '/api/scorm/commit';
  const ATTEMPT_TOKEN =
    window.SCORM_ATTEMPT_TOKEN ||
    new URLSearchParams(window.location.search).get('attempt');

  let initialized = false;
  let terminated = false;
  let lastError = '0';
  const state = {};
  const delta = {};

  function requireToken() {
    if (!ATTEMPT_TOKEN) {
      lastError = '201';
      console.warn('SCORM adapter: attempt token missing.');
      return false;
    }
    return true;
  }

  function commitDelta() {
    if (!requireToken()) {
      return false;
    }
    const payload = {
      attemptToken: ATTEMPT_TOKEN,
      version: 'SCORM_12',
      cmi: delta,
    };
    try {
      const body = JSON.stringify(payload);
      if (navigator.sendBeacon) {
        const blob = new Blob([body], { type: 'application/json' });
        navigator.sendBeacon(COMMIT_ENDPOINT, blob);
      } else {
        fetch(COMMIT_ENDPOINT, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body,
          keepalive: true,
        }).catch((err) => console.warn('SCORM commit failed', err));
      }
      for (const key of Object.keys(delta)) {
        delete delta[key];
      }
      lastError = '0';
      return true;
    } catch (error) {
      console.error('SCORM commit error', error);
      lastError = '101';
      return false;
    }
  }

  function apiGetValue(element) {
    lastError = '0';
    if (!initialized) {
      lastError = '301';
      return '';
    }
    return state[element] ?? '';
  }

  function apiSetValue(element, value) {
    if (!initialized) {
      lastError = '301';
      return 'false';
    }
    state[element] = value;
    delta[element] = value;
    lastError = '0';
    return 'true';
  }

  window.API = {
    LMSInitialize() {
      if (initialized) {
        lastError = '101';
        return 'false';
      }
      initialized = true;
      terminated = false;
      lastError = '0';
      return 'true';
    },
    LMSFinish() {
      if (!initialized || terminated) {
        lastError = '201';
        return 'false';
      }
      commitDelta();
      initialized = false;
      terminated = true;
      lastError = '0';
      return 'true';
    },
    LMSGetValue: apiGetValue,
    LMSSetValue: apiSetValue,
    LMSCommit() {
      if (!initialized) {
        lastError = '301';
        return 'false';
      }
      return commitDelta() ? 'true' : 'false';
    },
    LMSGetLastError() {
      return lastError;
    },
    LMSGetDiagnostic() {
      return lastError;
    },
    LMSGetErrorString() {
      return lastError;
    },
  };
})();
