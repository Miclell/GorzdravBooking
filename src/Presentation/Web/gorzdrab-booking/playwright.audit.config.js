import { defineConfig } from '@playwright/test';
import config from './playwright.config';

export default defineConfig({
  ...config,
  testMatch: '**/*.capture.js',
  workers: 1,
  outputDir: process.env.UI_AUDIT_BASE_URL ? 'test-results/audit-before' : 'test-results/audit-after',
});

