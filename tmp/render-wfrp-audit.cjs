// Rebuild the standalone report from its Markdown source.
// Usage: node tmp/render-wfrp-audit.cjs [input.md] [output.html]
const fs = require('node:fs');
const path = require('node:path');
const { pathToFileURL } = require('node:url');

(async () => {
  const { marked } = await import(pathToFileURL('C:/Users/rapha/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/marked/lib/marked.esm.js').href);
  const root = path.resolve(__dirname, '..');
  const input = path.resolve(process.argv[2] || path.join(root, 'docs', 'WFRP.audit.md'));
  const output = path.resolve(process.argv[3] || path.join(root, 'docs', 'WFRP.audit.html'));
  const markdown = fs.readFileSync(input, 'utf8').replace(/^\uFEFF/, '');
  const escapeHtml = value => String(value).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  const plain = html => html.replace(/<[^>]*>/g, '').replace(/&amp;/g, '&').replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&#39;/g, "'");
  const slugs = new Map();
  const slug = text => {
    const base = text.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '') || 'section';
    const count = (slugs.get(base) || 0) + 1;
    slugs.set(base, count);
    return count === 1 ? base : `${base}-${count}`;
  };
  const tokens = marked.lexer(markdown, { gfm: true });
  const titleIndex = tokens.findIndex(token => token.type === 'heading' && token.depth === 1);
  if (titleIndex < 0) throw new Error('The Markdown report must contain an H1 title.');
  const titleToken = tokens.splice(titleIndex, 1)[0];
  const titleHtml = marked.parseInline(titleToken.text);
  const title = plain(titleHtml);
  const toc = [];
  const renderer = new marked.Renderer();
  renderer.heading = function (token) {
    const html = this.parser.parseInline(token.tokens);
    const text = plain(html);
    const id = slug(text);
    if (token.depth === 2) toc.push({ id, text: text.replace(/^\d+\.\s*/, '') });
    return `<h${token.depth} id="${id}" tabindex="-1">${html}<a class="heading-link" href="#${id}" aria-label="Lien vers ${escapeHtml(text)}">#</a></h${token.depth}>\n`;
  };
  const tableRenderer = renderer.table.bind(renderer);
  renderer.table = function (token) {
    return `<div class="table-scroll" role="region" aria-label="Tableau de l’audit, défilement horizontal disponible" tabindex="0">${tableRenderer(token)}</div>\n`;
  };
  let article = marked.parser(tokens, { renderer, gfm: true });
  article = article.replace(/<td>(<strong>)?(P[0-3])(<\/strong>)?(?=\s|<\/td>|[—–:])/g, (full, opening, priority) => `<td><span class="priority priority-${priority.toLowerCase()}">${priority}</span>`);
  const nav = toc.map((item, index) => `<li><a href="#${item.id}"><span class="toc-number" aria-hidden="true">${String(index + 1).padStart(2, '0')}</span><span>${escapeHtml(item.text)}</span></a></li>`).join('\n');
  const sourceHref = path.relative(path.dirname(output), input).split(path.sep).map(encodeURIComponent).join('/');
  const html = `<!doctype html>
<html lang="fr">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta name="color-scheme" content="light">
  <meta name="description" content="Audit de l’application WFRP : produit, architecture, qualité, sécurité et recommandations. Rapport du 19 septembre 2026.">
  <title>${escapeHtml(title)}</title>
  <style>
    :root { --paper:#f6f4ed; --surface:#fffdf8; --ink:#292823; --muted:#716e64; --line:#dedbd0; --accent:#a74f36; --accent-soft:#f0e4da; --serif:Georgia,'Times New Roman',serif; --sans:'Segoe UI',Arial,sans-serif; }
    * { box-sizing:border-box; }
    html { scroll-behavior:smooth; scroll-padding-top:34px; }
    body { margin:0; background:var(--paper); color:var(--ink); font:16px/1.75 var(--sans); -webkit-font-smoothing:antialiased; }
    a { color:var(--accent); text-decoration-thickness:1px; text-underline-offset:3px; }
    a:hover { color:#743321; }
    ::selection { background:#ead0ba; color:#28241f; }
    :focus-visible { outline:2px solid var(--accent); outline-offset:4px; border-radius:3px; }
    .reading-progress { position:fixed; z-index:20; inset:0 auto auto 0; height:3px; width:0; background:var(--accent); transition:width .12s linear; }
    .skip { position:fixed; z-index:30; left:16px; top:-70px; padding:10px 18px; color:var(--ink); background:var(--surface); border:1px solid var(--line); }
    .skip:focus { top:12px; }
    .shell { max-width:1440px; margin:0 auto; padding:0 64px; }
    .masthead { display:flex; align-items:center; justify-content:space-between; gap:24px; min-height:99px; border-bottom:1px solid var(--line); }
    .brand { display:flex; align-items:center; gap:14px; font-size:13px; line-height:1.4; letter-spacing:.04em; }
    .brand-symbol { width:32px; height:32px; color:var(--accent); flex:none; }
    .brand strong { display:block; font-size:15px; letter-spacing:.13em; }
    .brand span { color:var(--muted); font-size:12px; letter-spacing:0; }
    .toolbar { display:flex; align-items:center; gap:18px; }
    .source-link { font-size:12px; color:var(--muted); text-decoration:none; }
    .source-link:hover { color:var(--accent); }
    .print-button { display:flex; align-items:center; gap:8px; border:1px solid #d4d0c5; border-radius:6px; background:transparent; color:var(--ink); font:500 12px var(--sans); padding:10px 14px; cursor:pointer; }
    .print-button:hover { background:var(--surface); border-color:var(--accent); }
    .print-button svg { width:15px; height:15px; }
    .hero { padding:66px 0 55px; max-width:1040px; }
    .eyebrow { display:flex; align-items:center; gap:11px; color:var(--accent); font-size:11px; line-height:1.5; font-weight:600; letter-spacing:.17em; text-transform:uppercase; }
    .eyebrow:before { content:''; display:block; width:27px; height:1px; background:var(--accent); }
    h1 { font:400 clamp(40px,4.8vw,67px)/1.08 var(--serif); letter-spacing:-.045em; margin:25px 0 29px; max-width:1000px; text-wrap:balance; }
    .hero-meta { display:flex; align-items:center; flex-wrap:wrap; gap:10px 23px; color:var(--muted); font-size:12px; }
    .hero-meta span + span { position:relative; }
    .hero-meta span + span:before { content:'·'; position:absolute; left:-14px; }
    .hero-meta code { padding:0; background:none; color:inherit; font-size:11px; }
    .report-layout { display:grid; grid-template-columns:216px minmax(0,1fr); gap:56px; border-top:1px solid var(--line); padding-top:36px; align-items:start; }
    .sidebar { position:sticky; top:30px; padding-bottom:24px; }
    .sidebar-label { margin:0 0 18px; color:var(--muted); font-size:10px; letter-spacing:.16em; font-weight:600; text-transform:uppercase; }
    .toc { margin:0; padding:0; list-style:none; max-height:calc(100vh - 170px); overflow:auto; scrollbar-width:thin; scrollbar-color:#d5cfc3 transparent; }
    .toc li + li { margin-top:3px; }
    .toc a { display:flex; align-items:baseline; gap:11px; padding:8px 10px 8px 0; color:#726e63; font-size:12px; line-height:1.55; text-decoration:none; transition:color .15s; border-right:2px solid transparent; }
    .toc a:hover, .toc a[aria-current="location"] { color:var(--accent); }
    .toc a[aria-current="location"] { border-right-color:var(--accent); font-weight:600; }
    .toc-number { flex:none; font-size:10px; opacity:.7; font-variant-numeric:tabular-nums; }
    .sidebar-foot { margin-top:24px; padding-top:18px; border-top:1px solid var(--line); font-size:10px; line-height:1.7; letter-spacing:.02em; color:var(--muted); }
    .sidebar-foot a { color:inherit; text-decoration:none; }
    .article { min-width:0; padding:0 0 45px; overflow-wrap:break-word; }
    .article > :first-child { margin-top:0; }
    .article p { margin:14px 0 20px; }
    .article h2 { position:relative; margin:58px 0 21px; padding-top:27px; border-top:1px solid var(--line); font:400 32px/1.25 var(--serif); letter-spacing:-.025em; text-wrap:balance; }
    .article h2:first-child { margin-top:0; padding-top:0; border-top:0; }
    .article h3 { margin:34px 0 14px; font:500 22px/1.35 var(--serif); letter-spacing:-.015em; text-wrap:balance; }
    .article h4, .article h5, .article h6 { margin:28px 0 12px; font-size:16px; line-height:1.45; }
    .heading-link { margin-left:9px; font-family:var(--sans); font-size:.6em; color:#9d9485; opacity:0; text-decoration:none; }
    h2:hover .heading-link, h3:hover .heading-link, h4:hover .heading-link, .heading-link:focus { opacity:1; }
    .article strong { font-weight:600; color:#302b24; }
    .article ul, .article ol { padding-left:24px; margin:16px 0 23px; }
    .article li { padding-left:3px; margin:7px 0; }
    .article li::marker { color:var(--accent); }
    .article li p { margin:5px 0; }
    .article li ul, .article li ol { margin:6px 0 12px; }
    .article blockquote { margin:27px 0; padding:17px 23px; background:var(--accent-soft); border-left:3px solid var(--accent); border-radius:0 7px 7px 0; color:#5b483a; }
    .article blockquote p { margin:5px 0; }
    .article hr { border:0; height:1px; background:var(--line); margin:40px 0; }
    .article code { background:#ebe8dd; border-radius:4px; padding:2px 5px; color:#6f442f; font:12px/1.7 Consolas,'Cascadia Code',monospace; overflow-wrap:anywhere; }
    .article pre { padding:22px 24px; border:1px solid var(--line); border-radius:8px; background:#eeeae0; overflow-x:auto; margin:23px 0; font-size:12px; line-height:1.75; }
    .article pre code { padding:0; background:transparent; color:#473d30; white-space:pre; overflow-wrap:normal; }
    .table-scroll { margin:25px 0 30px; overflow-x:auto; border:1px solid var(--line); border-radius:8px; background:var(--surface); }
    .article table { width:100%; border-collapse:collapse; font-size:12px; line-height:1.65; }
    .article thead { background:#eeeade; }
    .article th { color:#534a3d; font-weight:600; text-align:left; padding:13px 15px; border-bottom:1px solid #d5cdbd; vertical-align:top; }
    .article td { padding:13px 15px; vertical-align:top; border-bottom:1px solid #e8e3d7; min-width:110px; }
    .article tbody tr:last-child td { border-bottom:0; }
    .article tbody tr:nth-child(even) { background:#f7f4eb; }
    .article tbody tr:hover { background:#f2eddf; }
    .article td code { font-size:10px; }
    .priority { display:inline-block; border-radius:4px; padding:1px 7px; font-size:10px; font-weight:700; letter-spacing:.02em; }
    .priority-p0 { color:#842e2e; background:#f5dddd; }
    .priority-p1 { color:#8e3927; background:#f5e3d8; }
    .priority-p2 { color:#806125; background:#f2eacd; }
    .priority-p3 { color:#536758; background:#e8ede1; }
    .article img { max-width:100%; height:auto; }
    .footer { display:flex; align-items:center; justify-content:space-between; gap:20px; padding:23px 0 31px; border-top:1px solid var(--line); color:var(--muted); font-size:11px; }
    .footer a { color:inherit; text-decoration:none; }
    .footer a:hover { color:var(--accent); }
    @media (min-width:1500px) { .article { font-size:17px; } }
    @media (max-width:1100px) { .shell { padding:0 36px; } .report-layout { grid-template-columns:185px minmax(0,1fr); gap:35px; } .hero { padding-top:54px; } }
    @media (max-width:800px) {
      .shell { padding:0 24px; } .masthead { min-height:82px; } .hero { padding:43px 0 35px; } .eyebrow { letter-spacing:.1em; font-size:10px; } h1 { font-size:43px; margin:22px 0 24px; }
      .report-layout { display:block; padding-top:22px; } .sidebar { position:static; padding:0 0 23px; margin-bottom:30px; border-bottom:1px solid var(--line); }
      .sidebar-label { margin-bottom:11px; } .toc { display:grid; grid-template-columns:1fr 1fr; max-height:none; gap:3px 22px; } .toc li + li { margin:0; } .toc a { padding:5px 0; font-size:11px; border-right:0; } .sidebar-foot { display:none; }
      .article h2 { margin-top:44px; font-size:29px; } .article h3 { font-size:22px; } .article { font-size:15px; } .article th, .article td { padding:11px 12px; } .article table { min-width:620px; } .heading-link { opacity:.5; }
    }
    @media (max-width:450px) { .shell { padding:0 19px; } .toolbar { gap:10px; } .source-link { font-size:11px; } .print-button { padding:9px; font-size:0; gap:0; } .print-button svg { width:17px; height:17px; } .brand { gap:9px; } .brand-symbol { width:27px; height:27px; } .brand strong { font-size:13px; } .brand span { font-size:10px; } h1 { font-size:38px; } .hero-meta { font-size:11px; gap:8px 20px; } .toc { column-gap:15px; } .toc a { gap:7px; } .footer { align-items:flex-start; font-size:10px; } }
    @media (prefers-reduced-motion:reduce) { html { scroll-behavior:auto; } *, *:before, *:after { transition:none !important; } }
    @page { size:A4; margin:18mm 17mm 20mm; }
    @media print {
      html { scroll-behavior:auto; } body { background:#fff; color:#222; font-size:10pt; line-height:1.5; } .shell { max-width:none; padding:0; } .reading-progress, .skip, .toolbar, .sidebar, .heading-link, .footer a { display:none !important; }
      .masthead { min-height:auto; padding:0 0 13px; } .brand-symbol { width:23px; height:23px; } .brand strong { font-size:11px; } .brand span { font-size:9px; } .hero { padding:27px 0 25px; max-width:none; } .eyebrow { font-size:8px; } h1 { font-size:32pt; margin:15px 0; max-width:none; }
      .hero-meta { font-size:9px; } .report-layout { display:block; padding-top:22px; } .article { font-size:10pt; padding-bottom:15px; } .article h2 { font-size:20pt; margin-top:30px; padding-top:20px; break-after:avoid; } .article h3 { font-size:15pt; margin-top:24px; break-after:avoid; } .article h4 { break-after:avoid; }
      .article p, .article li { orphans:3; widows:3; } .article p { margin:10px 0 14px; } .article a { color:#6b3f30; text-decoration:underline; } .article code { font-size:8pt; background:#f2f0e9; } .article pre { white-space:pre-wrap; overflow:visible; font-size:8pt; padding:12px; } .article pre code { white-space:pre-wrap; overflow-wrap:anywhere; }
      .table-scroll { overflow:visible; border-radius:0; margin:15px 0 20px; } .article table { width:100%; min-width:0; table-layout:fixed; font-size:7.3pt; line-height:1.4; } .article td, .article th { padding:7px 6px; min-width:0; overflow-wrap:anywhere; } .article td code { font-size:6.9pt; } .article tr { break-inside:avoid; } .article thead { display:table-header-group; } .article blockquote { break-inside:avoid; padding:11px 15px; } .footer { font-size:8px; padding:14px 0; } .priority, .article thead, .article blockquote { print-color-adjust:exact; -webkit-print-color-adjust:exact; }
    }
  </style>
</head>
<body id="top">
  <a class="skip" href="#report-content">Aller au rapport</a>
  <div class="reading-progress" aria-hidden="true"></div>
  <div class="shell">
    <header class="masthead">
      <div class="brand">
        <svg class="brand-symbol" viewBox="0 0 36 36" fill="none" aria-hidden="true"><path d="M18 2v32M2 18h32M6.7 6.7l22.6 22.6M6.7 29.3L29.3 6.7M11.9 3.2l12.2 29.6M3.2 11.9l29.6 12.2M3.2 24.1l29.6-12.2M11.9 32.8L24.1 3.2" stroke="currentColor" stroke-width="2"/></svg>
        <div><strong>WFRP</strong><span>Rapport d’application</span></div>
      </div>
      <div class="toolbar">
        <a class="source-link" href="${sourceHref}" download>Source Markdown ↗</a>
        <button class="print-button" type="button" aria-label="Imprimer le rapport ou enregistrer en PDF"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><path d="M6 8V3h12v5M6 17H3V8h18v9h-3M6 14h12v7H6z"/><path d="M17 11h1"/></svg>Imprimer</button>
      </div>
    </header>
    <section class="hero" aria-labelledby="report-title">
      <div class="eyebrow">Audit produit · technique · sécurité</div>
      <h1 id="report-title">${titleHtml}</h1>
      <div class="hero-meta"><span><time datetime="2026-09-19">19 septembre 2026</time></span><span>Révision <code>2acede2</code></span><span>Rapport intégral</span></div>
    </section>
    <div class="report-layout">
      <aside class="sidebar" aria-label="Sommaire du rapport">
        <p class="sidebar-label">Dans ce rapport</p>
        <nav aria-label="Sections de l’audit"><ol class="toc">${nav}</ol></nav>
        <div class="sidebar-foot">WFRP · Septembre 2026<br><a href="${sourceHref}">Consulter le document source</a></div>
      </aside>
      <main class="article" id="report-content" tabindex="-1">${article}</main>
    </div>
    <footer class="footer"><span>WFRP · Audit de l’application · 19.09.2026</span><a href="#top">Retour en haut ↑</a></footer>
  </div>
  <script>
    (() => {
      document.querySelector('.print-button').addEventListener('click', () => window.print());
      const progress = document.querySelector('.reading-progress');
      const links = Array.from(document.querySelectorAll('.toc a'));
      const headings = links.map(link => document.getElementById(link.hash.slice(1))).filter(Boolean);
      let scheduled = false;
      const update = () => {
        scheduled = false;
        const height = document.documentElement.scrollHeight - window.innerHeight;
        progress.style.width = (height > 0 ? Math.min(100, Math.max(0, window.scrollY / height * 100)) : 100) + '%';
        let current = null;
        for (const heading of headings) { if (heading.getBoundingClientRect().top <= 170) current = heading.id; }
        links.forEach(link => { if (link.hash === '#' + current) link.setAttribute('aria-current', 'location'); else link.removeAttribute('aria-current'); });
      };
      const schedule = () => { if (!scheduled) { scheduled = true; requestAnimationFrame(update); } };
      window.addEventListener('scroll', schedule, { passive:true });
      window.addEventListener('resize', schedule, { passive:true });
      update();
    })();
  </script>
</body>
</html>\n`;
  fs.mkdirSync(path.dirname(output), { recursive: true });
  fs.writeFileSync(output, html, 'utf8');
  console.log(JSON.stringify({ input, output, sections: toc.length, bytes: Buffer.byteLength(html) }, null, 2));
})().catch(error => { console.error(error.message); process.exitCode = 1; });
