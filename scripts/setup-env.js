#!/usr/bin/env node
// Kopierar .env.example till .env om .env saknas.
// Körs automatiskt av "npm run start".

const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '..');
const envPath = path.join(root, '.env');
const examplePath = path.join(root, '.env.example');

if (!fs.existsSync(envPath)) {
  if (!fs.existsSync(examplePath)) {
    console.error('❌ .env.example saknas – kan inte skapa .env automatiskt.');
    process.exit(1);
  }
  fs.copyFileSync(examplePath, envPath);
  console.log('✅ .env skapades från .env.example (dev-credentials)');
} else {
  console.log('ℹ️  .env finns redan – hoppar över skapande.');
}

