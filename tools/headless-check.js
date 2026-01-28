const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');
(async () => {
  const browser = await chromium.launch({ args: ['--no-sandbox'] });
  const page = await browser.newPage();

  page.on('console', msg => {
    console.log('[console]', msg.type(), msg.text());
  });
  page.on('pageerror', err => console.log('[pageerror]', err.toString()));
  page.on('response', resp => {
    const url = resp.url();
    if (url.includes('/_framework/')) {
      console.log('[response]', resp.status(), url);
    }
  });

  const appBase = process.env.APP_BASE_URL || 'http://localhost:5049';
  const outDir = path.join(process.cwd(), '_docs', 'tests');
  const outPath = path.join(outDir, 'headless_screenshot.png');

  console.log('navigating to', appBase);
  try {
    await page.goto(appBase, { waitUntil: 'networkidle', timeout: 15000 });
  } catch (e) {
    console.log('goto error:', e.message);
  }
  // wait a bit for runtime startup
  await page.waitForTimeout(5000);

  // ensure output dir exists
  try { fs.mkdirSync(outDir, { recursive: true }); } catch (e) { /* ignore */ }

  console.log('screenshotting to', outPath);
  try {
    await page.screenshot({ path: outPath, fullPage: true });
  } catch (e) {
    console.log('screenshot error:', e.message);
  }
  await browser.close();
})();
