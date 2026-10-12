import { defineConfig } from '@playwright/test';
import config from './playwright.config';

export default defineConfig({
  ...config,
  testMatch: '**/*.integration.js',
  fullyParallel: false,
  workers: 1,
});

