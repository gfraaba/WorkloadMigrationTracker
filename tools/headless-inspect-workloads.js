const { chromium } = require('playwright');
(async () => {
  const appBase = process.env.APP_BASE_URL || 'http://localhost:5049';
  const url = appBase + '/workloads';
  const browser = await chromium.launch();
  const page = await browser.newPage();
  try {
    console.log('Navigating to', url);
    await page.goto(url, { waitUntil: 'networkidle', timeout: 20000 });
    // wait for potential SPA rendering
    await page.waitForTimeout(4000);
    const title = await page.locator('.card-title').first().innerText().catch(() => null);
    const titleHtml = await page.locator('.card-title').first().evaluate(el => el.innerHTML).catch(() => null);
    const bodyText = await page.locator('body').innerText();
    console.log('card-title:', title);
    console.log('card-title innerHTML:', titleHtml);
    console.log('--- body snippet ---');
    console.log(bodyText.split('\n').slice(0,60).join('\n'));
  } catch (e) {
    console.error('Error:', e && e.stack ? e.stack : e);
    process.exitCode = 2;
  } finally {
    await browser.close();
  }
})();
