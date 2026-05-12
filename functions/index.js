const functions = require('firebase-functions');
const puppeteer = require('puppeteer-core');
const chromium = require('@sparticuz/chromium');
const fs = require('fs');
const path = require('path');

const CRAWLERS = [
  'googlebot', 'bingbot', 'yandexbot', 'facebookexternalhit',
  'twitterbot', 'linkedinbot', 'slackbot', 'baiduspider',
  'duckduckbot', 'applebot', 'semrushbot', 'ahrefsbot',
  'dotbot', 'ia_archiver', 'gptbot', 'claudebot',
];

function isCrawler(ua) {
  if (!ua) return false;
  const lower = ua.toLowerCase();
  return CRAWLERS.some(c => lower.includes(c));
}

async function renderPage(url) {
  const browser = await puppeteer.launch({
    args: chromium.args,
    defaultViewport: { width: 1280, height: 800 },
    executablePath: await chromium.executablePath() || undefined,
    headless: true,
  });

  try {
    const page = await browser.newPage();
    await page.setUserAgent('Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)');

    await page.goto(url, { waitUntil: 'networkidle0', timeout: 25000 });

    // Wait for Blazor content to render
    await page.waitForFunction(
      () => {
        const container = document.querySelector('.lecture-container, .mud-grid, .mud-card');
        return container && container.children.length > 0;
      },
      { timeout: 20000 }
    ).catch(() => {});

    // Extra wait for fonts and images
    await new Promise(r => setTimeout(r, 1000));

    // Wait for network idle again
    await page.waitForNetworkIdle({ idleTime: 500, timeout: 10000 }).catch(() => {});

    const html = await page.content();
    return html;
  } finally {
    await browser.close();
  }
}

exports.prerender = functions
  .runWith({
    memory: '1GB',
    timeoutSeconds: 60,
    minInstances: 0,
  })
  .https.onRequest(async (req, res) => {
    const ua = req.headers['user-agent'] || '';
    const isCrawlerReq = isCrawler(ua);

    functions.logger.info(`Request: ${req.path} | Crawler: ${isCrawlerReq} | UA: ${ua.slice(0, 80)}`);

    // Rewrite internal Firebase Hosting request path
    let requestPath = req.path;
    if (requestPath.startsWith('/functions/')) {
      requestPath = requestPath.replace('/functions', '');
    }

    // Serve static files directly for non-crawlers
    if (!isCrawlerReq) {
      const hostingDir = path.join(__dirname, 'hosting');
      const filePath = path.join(hostingDir, requestPath === '/' ? 'index.html' : requestPath);

      try {
        if (fs.existsSync(filePath) && fs.statSync(filePath).isFile()) {
          const ext = path.extname(filePath).toLowerCase();
          const mimeTypes = {
            '.html': 'text/html; charset=utf-8',
            '.css': 'text/css; charset=utf-8',
            '.js': 'application/javascript; charset=utf-8',
            '.json': 'application/json',
            '.png': 'image/png',
            '.jpg': 'image/jpeg',
            '.jpeg': 'image/jpeg',
            '.svg': 'image/svg+xml',
            '.ico': 'image/x-icon',
            '.webmanifest': 'application/manifest+json',
            '.wasm': 'application/wasm',
            '.dll': 'application/octet-stream',
            '.pdb': 'application/octet-stream',
            '.woff': 'font/woff',
            '.woff2': 'font/woff2',
          };
          res.set('Content-Type', mimeTypes[ext] || 'application/octet-stream');
          res.send(fs.readFileSync(filePath));
          return;
        }
      } catch (e) {
        // Fall through to index.html
      }

      // SPA fallback
      const indexPath = path.join(hostingDir, 'index.html');
      if (fs.existsSync(indexPath)) {
        res.set('Content-Type', 'text/html; charset=utf-8');
        res.send(fs.readFileSync(indexPath));
        return;
      }
    }

    // Crawler: render with Puppeteer
    try {
      const baseUrl = `https://${req.headers['x-forwarded-host'] || req.headers.host || 'bibeli.vercel.app'}`;
      const fullUrl = `${baseUrl}${req.path}${req.url.includes('?') ? req.url.substring(req.url.indexOf('?')) : ''}`;
      const html = await renderPage(fullUrl);
      res.set('Content-Type', 'text/html; charset=utf-8');
      res.set('X-Rendered-By', 'puppeteer');
      res.send(html);
    } catch (err) {
      functions.logger.error('Prerender failed', err);

      // Fallback: serve index.html
      try {
        const indexPath = path.join(__dirname, 'hosting', 'index.html');
        res.set('Content-Type', 'text/html; charset=utf-8');
        res.send(fs.readFileSync(indexPath));
      } catch {
        res.status(500).send('Error rendering page');
      }
    }
  });
