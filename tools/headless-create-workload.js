const { chromium, request: playwrightRequest } = require('playwright');

(async () => {
  const apiBase = process.env.TEST_API_BASE_URL || process.env.TEST_BASE_URL || 'http://localhost:5005';
  const appBase = process.env.APP_BASE_URL || 'http://localhost:5049';

  const browser = await chromium.launch();
  const page = await browser.newPage();
  try {
    await page.goto(appBase, { waitUntil: 'networkidle' });

    // Use Playwright request API to avoid browser CORS restrictions
    const request = await playwrightRequest.newContext({ baseURL: apiBase });

    // Check existing workloads first
    const get = await request.get('/api/workloads');
    let list = null;
    try { list = await get.json(); } catch (e) { list = null; }

    let post = null;
    let postText = null;

    if (Array.isArray(list) && list.length > 0) {
      console.log('Workloads already exist, skipping create. Count:', list.length);
    } else {
      const payload = {
        WorkloadId: 0,
        Name: 'AutoCreated Workload',
        Description: 'Created by headless-create-workload.js',
        AzureNamePrefix: 'acw',
        LandingZonesCount: 0,
        ResourcesCount: 0,
        PrimaryPOC: 'dev@example.com',
        SecondaryPOC: null,
        WorkloadEnvironmentRegions: []
      };

      post = await request.post('/api/workloads', { data: payload });
      try { postText = await post.text(); } catch (e) { postText = null; }

      // re-fetch list after create
      const get2 = await request.get('/api/workloads');
      try { list = await get2.json(); } catch (e) { /* keep previous value */ }
    }

    const result = {
      postStatus: post ? post.status() : null,
      postBody: postText,
      getStatus: get.status(),
      workloadsCount: Array.isArray(list) ? list.length : null,
      firstWorkload: Array.isArray(list) && list.length > 0 ? list[0] : null
    };

    console.log(JSON.stringify(result, null, 2));
  } catch (err) {
    console.error('Script error:', err && err.stack ? err.stack : err);
    process.exitCode = 2;
  } finally {
    await browser.close();
  }
})();
