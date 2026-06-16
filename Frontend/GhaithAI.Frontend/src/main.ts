import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app';

// ✅ Run BEFORE Angular bootstraps to purge any corrupted localStorage entries.
// Corrupted values (e.g. truncated JSON from a previous crash) cause
// "SyntaxError: Unexpected end of JSON input" in window.onload.
(function sanitizeStorage() {
  const keysToValidate = ['user', 'currentUser', 'authUser', 'profile'];
  for (const key of keysToValidate) {
    try {
      const raw = localStorage.getItem(key);
      if (raw) JSON.parse(raw); // throws if corrupt
    } catch {
      localStorage.removeItem(key);
      localStorage.removeItem('token'); // also clear paired token
      console.warn(`[Bootstrap] Removed corrupted localStorage key: "${key}"`);
    }
  }
})();

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
