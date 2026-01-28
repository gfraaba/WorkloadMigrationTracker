const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');
(async () => {
  const appBase = process.env.APP_BASE_URL || 'http://localhost:5049';
  const url = appBase + '/add-landing-zone/1';
  const outDir = path.join(process.cwd(), '_docs', 'tests');
  const outPath = path.join(outDir, 'headless_add_lz.png');

  const browser = await chromium.launch();
  const page = await browser.newPage();

  page.on('console', msg => console.log('[console]', msg.type(), msg.text()));
  page.on('pageerror', err => console.log('[pageerror]', err.toString()));
  page.on('response', resp => console.log('[response]', resp.status(), resp.url()));

  try {
    console.log('Navigating to', url);
    await page.goto(url, { waitUntil: 'networkidle', timeout: 30000 });

    // Wait for environment options to populate (robust check)
    await page.waitForFunction(() => {
      const opts = document.querySelectorAll('#environment option:not([value="0"])');
      return opts && opts.length > 0;
    }, { timeout: 30000 });

    const envValue = await page.$eval('#environment option:not([value="0"])', el => el.value);
    console.log('Selecting environment id:', envValue);
    await page.selectOption('#environment', envValue);

    // Wait for regions to populate after selecting environment
    await page.waitForFunction(() => {
      const opts = document.querySelectorAll('#region option:not([value="0"])');
      return opts && opts.length > 0;
    }, { timeout: 30000 });

    const regionValue = await page.$eval('#region option:not([value="0"])', el => el.value);
    console.log('Selecting region id:', regionValue);
    await page.selectOption('#region', regionValue);

    // Fill name
    const name = 'lz-auto-' + Date.now();
    await page.fill('#name', name);
    console.log('Filled ResourceGroupName:', name);

    // Submit and capture the POST response
    const [resp] = await Promise.all([
      page.waitForResponse(r => r.url().includes('/api/WorkloadEnvironmentRegions') && (r.request().method() === 'POST'), { timeout: 20000 }),
      page.click('button[type="submit"]')
    ]);

    console.log('POST status:', resp.status());
    try { const text = await resp.text(); console.log('POST body:', text); } catch (e) { console.log('Could not read POST body', e.message); }

    // Ensure output dir exists
    try { fs.mkdirSync(outDir, { recursive: true }); } catch (e) { /* ignore */ }

    // Take screenshot
    await page.screenshot({ path: outPath, fullPage: true });
    console.log('Screenshot saved to', outPath);
  } catch (err) {
    console.error('Script error:', err && err.stack ? err.stack : err);
    process.exitCode = 2;
  } finally {
    await browser.close();
  }
})();
