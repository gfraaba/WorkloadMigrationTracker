const { chromium } = require('playwright');
(async () => {
  const appBase = process.env.APP_BASE_URL || 'http://localhost:5049';
  const workloadId = process.env.WORKLOAD_ID || '1';
  const url = appBase + `/workloads/edit/${workloadId}`;
  const browser = await chromium.launch();
  const page = await browser.newPage();

  page.on('console', msg => console.log('[console]', msg.type(), msg.text()));
  page.on('pageerror', err => console.log('[pageerror]', err.toString()));
  page.on('response', resp => console.log('[response]', resp.status(), resp.url()));

  try {
    console.log('Navigating to', url);
    await page.goto(url, { waitUntil: 'networkidle', timeout: 20000 });
    await page.waitForTimeout(3000);

    // Capture workload name field and form visibility
    const name = await page.locator('#name').inputValue().catch(() => null);
    console.log('name field value:', name);

    // dump body snippet
    const bodyText = await page.locator('body').innerText();
    console.log('--- body snippet ---');
    console.log(bodyText.split('\n').slice(0,80).join('\n'));

    await page.screenshot({ path: '_docs/tests/headless_edit_workload.png', fullPage: true });
    console.log('Screenshot saved to _docs/tests/headless_edit_workload.png');
  } catch (e) {
    console.error('Error:', e && e.stack ? e.stack : e);
    process.exitCode = 2;
  } finally {
    await browser.close();
  }
})();
