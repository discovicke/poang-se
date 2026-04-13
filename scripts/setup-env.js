// Kopierar .env.example till .env om .env saknas.
// Kors automatiskt av "npm run start".
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '..');
const envPath = path.join(root, '.env');
const examplePath = path.join(root, '.env.example');
if (!fs.existsSync(envPath)) {
  if (!fs.existsSync(examplePath)) {
    console.error('ERROR: .env.example saknas - kan inte skapa .env automatiskt.');
    process.exit(1);
  }
  fs.copyFileSync(examplePath, envPath);
  console.log('OK: .env skapades fran .env.example (dev-credentials)');
} else {
  console.log('INFO: .env finns redan - hoppar over skapande.');
}