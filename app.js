const $ = s => document.querySelector(s),
    esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c])),
    stars = n => { n = Math.round(n) || 0; return '★'.repeat(n) + '☆'.repeat(5 - n) },
    X = '<button type="button" class="x" onclick="this.closest(\'dialog\').close()" aria-label="Close">×</button>',
    MAX_IMG = 2 * 1024 * 1024;
let S = JSON.parse(localStorage.nm || 'null'), cats = [], regs = [], tt, tab = 'Categories';
const save = () => S ? localStorage.nm = JSON.stringify(S) : localStorage.removeItem('nm');
const has = r => S && S.roles.includes(r);

function toast(m, bad) { const t = $('#toast'); t.textContent = m; t.className = 'on' + (bad ? ' bad' : ''); clearTimeout(tt); tt = setTimeout(() => t.className = '', 3500) }
function msg(s, d) {
    if (typeof d === 'string' && d) return d;
    if (Array.isArray(d)) return d.map(e => e.description).join(', ');
    if (d && d.message) return d.message;
    if (d && d.errors) return Object.values(d.errors).flat().join(', ');
    return s === 403 ? "You don't have permission to do that" : s === 404 ? 'Not found' : 'Something went wrong. Try again.'
}
async function api(p, o = {}) {
    const fd = o.body instanceof FormData, h = {};
    if (!fd) h['Content-Type'] = 'application/json';
    if (S) h.Authorization = 'Bearer ' + S.token;
    const r = await fetch('/api/' + p, { ...o, headers: h, body: o.body ? (fd ? o.body : JSON.stringify(o.body)) : undefined });
    if (r.status === 401 && S && p !== 'auth/login') { S = null; save(); nav(); go('browse'); throw Error('Your session expired. Log in again.') }
    const t = await r.text(); let d; try { d = t ? JSON.parse(t) : null } catch { d = t }
    if (!r.ok) { const e = Error(msg(r.status, d)); e.status = r.status; throw e }
    return d
}
const loadLists = async () => { [cats, regs] = await Promise.all([api('Category'), api('Region')]) };
const opts = (l, sel, all) => `<option value="">${all}</option>` + l.map(x => `<option value="${x.id}"${x.id === sel ? ' selected' : ''}>${esc(x.name)}</option>`).join('');
const ico = n => { const p = cats.find(c => c.name === n)?.iconPath; return p ? `<img class="ico" src="${esc(p)}" alt="" onerror="this.remove()">` : '' };
const fill = () => { $('#c').innerHTML = opts(cats, 0, 'All categories'); $('#g').innerHTML = opts(regs, 0, 'All regions') };

function nav() {
    const r = has('Admin') ? 'Admin' : has('Provider') ? 'Provider' : 'User';
    $('#nav').innerHTML = S ? `<span class="role">${r}</span>${has('Provider') ? '' : `<button class="btn ghost" onclick="go('browse')">Home</button>`}${has('Provider') ? `<button class="btn ghost" onclick="go('mine')">My business</button>` : ''}${has('Admin') ? `<button class="btn ghost" onclick="go('admin')">Admin</button>` : ''}<button class="btn ghost" onclick="out()">Log out</button>`
        : `<button class="btn ghost" onclick="openAuth(false)">Log in</button><button class="btn sun" onclick="openAuth(true)">Sign up</button>`
}
function go(v) {
    if (v === 'browse' && has('Provider')) v = 'mine';
    ['browse', 'mine', 'admin'].forEach(k => $('#v-' + k).hidden = k !== v);
    $('#hero').hidden = v !== 'browse';
    if (v === 'mine') mine(); if (v === 'admin') admin(); if (v === 'browse') loadLists().then(() => { fill(); search() }).catch(x => toast(x.message, 1))
}
function out() { S = null; save(); nav(); go('browse') }

function openAuth(r) {
    const a = $('#auth');
    a.innerHTML = `${X}<form onsubmit="doAuth(event,${r})"><h2>${r ? 'Create an account' : 'Log in'}</h2><br>
  ${r ? '<label>Full name<input name="fullName" required maxlength="100"></label>' : ''}
  <label>Email<input name="email" type="email" required></label>
  <label>Password<input name="password" type="password" required minlength="6"></label>
  ${r ? `<fieldset><legend>I'm joining NearMe as</legend>
  <label class="opt"><input type="radio" name="accountType" value="User" checked><b>A customer</b><span>I search for services and review them</span></label>
  <label class="opt"><input type="radio" name="accountType" value="Provider"><b>A service provider</b><span>I have a business and want to be found</span></label></fieldset>`: ''}
  <button class="btn primary wide">${r ? 'Create account' : 'Log in'}</button>
  <p class="sw">${r ? 'Already have an account?' : 'New here?'} <a href="#" onclick="openAuth(${!r});return false">${r ? 'Log in' : 'Create an account'}</a></p></form>`;
    if (!a.open) a.showModal()
}
async function doAuth(e, r) {
    e.preventDefault(); const f = Object.fromEntries(new FormData(e.target));
    try {
        if (r) await api('auth/register', { method: 'POST', body: f });
        const d = await api('auth/login', { method: 'POST', body: { email: f.email, password: f.password } });
        S = { token: d.token, roles: d.roles, hasProfile: d.hasProfile }; save();
        $('#auth').close(); nav(); toast('Welcome to NearMe');
        go(has('Admin') ? 'admin' : has('Provider') ? 'mine' : 'browse')
    } catch (x) { toast(x.message, 1) }
}

/* ---------- Customer: search, details, reviews ---------- */
async function search() {
    const q = new URLSearchParams();
    [['name', '#q'], ['categoryId', '#c'], ['regionId', '#g']].forEach(([k, s]) => { const v = $(s).value.trim(); if (v) q.set(k, v) });
    try {
        const l = await api('ProviderProfiles/search?' + q);
        $('#res').innerHTML = l.length ? `<div class="grid">${l.map(card).join('')}</div>` : '<p class="empty">No results. Try another name or clear a filter.</p>'
    } catch (x) { toast(x.message, 1) }
}
const rating = (a, n) => n ? `<b>${stars(a)}</b> ${a.toFixed(1)} (${n} ${n === 1 ? 'review' : 'reviews'})` : 'No reviews yet';
const mapOf = p => (p.latitude || p.longitude) && isFinite(p.latitude) && isFinite(p.longitude) ? `https://www.google.com/maps/search/?api=1&query=${p.latitude},${p.longitude}` : '';
const okMap = u => /^https:\/\/www\.google\.com\/maps\//.test(u || '');
const card = p => `<div class="card" role="button" tabindex="0" onclick="show(${p.id})" onkeydown="if(event.target===this&&(event.key==='Enter'||event.key===' ')){event.preventDefault();show(${p.id})}"><span class="tag">${ico(p.categoryName)}${esc(p.categoryName)}</span><h3>${esc(p.businessName)}</h3><span class="mute">${esc(p.regionName)}</span><span class="mute">${esc(p.address)}</span>${p.contact ? `<span class="mute">${esc(p.contact)}</span>` : ''}<span class="rate">${rating(p.averageRating, p.reviewsCount)}</span>${okMap(mapOf(p)) ? `<a class="btn sun cmap" href="${esc(mapOf(p))}" target="_blank" rel="noopener noreferrer" onclick="event.stopPropagation()">Open in Google Maps</a>` : ''}</div>`;
const reviewItem = r => `<article class="rv"><b>${stars(r.rating)}</b> <span class="mute">${esc(r.userName || 'User')}</span><p>${esc(r.comment)}</p></article>`;
/* only accept a Google Maps link coming from our own API */
const mapBtn = u => okMap(u) ? `<p class="maplink"><a class="btn sun" href="${esc(u)}" target="_blank" rel="noopener noreferrer">Open in Google Maps</a></p>` : '';

const ratingOpts = v => [5, 4, 3, 2, 1].map(n => `<option value="${n}"${n === v ? ' selected' : ''}>${'★'.repeat(n)} ${['', 'Bad', 'Poor', 'Okay', 'Good', 'Excellent'][n]}</option>`).join('');
const reviewForm = (id, my) => `<form class="panel" onsubmit="${my ? `editRev(event,${my.id},${id})` : `rate(event,${id})`}"><h3>${my ? 'Your review' : 'Leave a review'}</h3>${my ? `<p class="mute">Posted on ${new Date(my.createdAt).toLocaleDateString()}</p>` : ''}
  <label>Your rating<select name="rating">${ratingOpts(my ? my.rating : 5)}</select></label>
  <label>Your comment<textarea name="comment" rows="3" maxlength="1000">${esc(my?.comment)}</textarea></label>
  <button class="btn primary">${my ? 'Save changes' : 'Post review'}</button>${my ? `<button type="button" class="del" style="margin-inline-start:14px" onclick="delRev(${my.id},${id})">Delete my review</button>` : ''}</form>`;

async function show(id) {
    const d = $('#dr'); d.innerHTML = X + '<p class="empty">Loading...</p>'; if (!d.open) d.showModal();
    try {
        const [p, rv, my] = await Promise.all([api('ProviderProfiles/' + id), api('Review/provider/' + id), S ? api('Review/provider/' + id + '/mine').catch(() => null) : null]);
        const others = my ? rv.filter(r => r.id !== my.id) : rv;
        d.innerHTML = `${X}<span class="tag">${ico(p.category.name)}${esc(p.category.name)}</span><h2>${esc(p.businessName)}</h2>
    <p class="mute">${esc(p.region)}, ${esc(p.address)}</p>${p.contact ? `<p class="info"><b>Contact:</b> ${esc(p.contact)}</p>` : ''}${mapBtn(mapOf(p))}<p class="rate">${rating(p.averageRating, p.reviewsCount)}</p>
    ${p.description ? `<p>${esc(p.description)}</p>` : ''}
    ${p.gallery.length ? `<div class="gal">${p.gallery.map(g => `<img src="${esc(g.imageUrl)}" alt="" loading="lazy" onerror="this.remove()">`).join('')}</div>` : ''}
    <h3>Reviews</h3>
    ${S ? reviewForm(id, my) : `<button class="btn primary" onclick="$('#dr').close();openAuth(false)">Log in to leave a review</button>`}
    ${others.length ? others.map(reviewItem).join('') : (my ? '' : '<p class="mute">No reviews yet. Be the first to review.</p>')}`
    } catch (x) { d.close(); toast(x.message, 1) }
}
async function editRev(e, rid, pid) {
    e.preventDefault(); const f = Object.fromEntries(new FormData(e.target));
    try { await api('Review/' + rid, { method: 'PUT', body: { rating: +f.rating, comment: f.comment } }); toast('Review updated'); show(pid); search() }
    catch (x) { toast(x.message, 1) }
}
async function delRev(rid, pid) {
    if (!confirm('Delete your review?')) return;
    try { await api('Review/' + rid, { method: 'DELETE' }); toast('Review deleted'); show(pid); search() }
    catch (x) { toast(x.message, 1) }
}
async function rate(e, id) {
    e.preventDefault(); const f = Object.fromEntries(new FormData(e.target));
    try { await api('Review', { method: 'POST', body: { rating: +f.rating, comment: f.comment, providerProfileId: id } }); toast('Review posted'); show(id); search() }
    catch (x) { toast(x.message, 1) }
}

/* ---------- Provider: business profile, gallery, reviews ---------- */
async function mine() {
    const v = $('#v-mine'); v.innerHTML = '<p class="empty">Loading...</p>';
    try { await loadLists() } catch (x) { return toast(x.message, 1) }
    let p = null;
    try { p = await api('ProviderProfiles/my-profile') } catch (x) { if (x.status !== 404) toast(x.message, 1) }
    const cs = p && cats.find(c => c.name === p.category.name)?.id, rs = p && regs.find(r => r.name === p.region)?.id;
    v.innerHTML = `<h2>${p ? 'Your business' : 'Set up your business'}</h2>
  <p class="mute">${p ? 'Changes show up for customers right away.' : 'Fill this in so your business appears in search results.'}</p>
  <form class="panel" onsubmit="saveP(event,${p ? p.id : 0})">
  <label>Business name<input name="businessName" required maxlength="200" value="${esc(p?.businessName)}"></label>
  <div class="two"><label>Category<select name="categoryId" required>${opts(cats, cs, 'Choose a category')}</select></label>
  <label>Region<select name="regionId" required>${opts(regs, rs, 'Choose a region')}</select></label></div>
  <label>Address<input name="address" required maxlength="500" value="${esc(p?.address)}"></label>
  <label>Contact<input name="contact" maxlength="300" placeholder="Phone, WhatsApp or email" value="${esc(p?.contact)}"></label>
  <div class="two"><label>Latitude<input name="latitude" inputmode="decimal" placeholder="e.g. 29.9602" value="${esc(p?.latitude)}" oninput="splitCoords(this)"></label>
  <label>Longitude<input name="longitude" inputmode="decimal" placeholder="e.g. 31.2569" value="${esc(p?.longitude)}"></label></div>
  <p class="hint">So customers can open your place in Google Maps: right-click your place on the map, click the numbers to copy them, and paste them in Latitude. Or <button type="button" class="btn line" onclick="myLoc(this)">Use my location</button></p>
  <label>Description<textarea name="description" rows="4" maxlength="2000">${esc(p?.description)}</textarea></label>
  <button class="btn primary">${p ? 'Save changes' : 'Create business'}</button>${p ? `<button type="button" class="del" style="margin-inline-start:14px" onclick="delP(${p.id})">Delete business</button>` : ''}</form>
  <div id="imgs"></div><div id="myrev"></div>`;
    if (p) { imgs(p.id); myReviews(p) }
}
/* pasting "29.96, 31.25" into Latitude fills both boxes */
function splitCoords(el) {
    if (!el.value.includes(',')) return;
    const [a, b] = el.value.split(',').map(s => s.trim());
    el.value = a; el.form.longitude.value = b ?? ''
}
function myLoc(b) {
    if (!navigator.geolocation) return toast('Location is not supported in this browser', 1);
    const f = b.closest('form');
    navigator.geolocation.getCurrentPosition(
        p => { f.latitude.value = p.coords.latitude.toFixed(6); f.longitude.value = p.coords.longitude.toFixed(6) },
        () => toast('Could not get your location', 1))
}
async function saveP(e, id) {
    e.preventDefault(); const f = Object.fromEntries(new FormData(e.target)); f.categoryId = +f.categoryId; f.regionId = +f.regionId;
    const lat = f.latitude.trim(), lng = f.longitude.trim();
    if ((lat && isNaN(+lat)) || (lng && isNaN(+lng))) return toast('Latitude and longitude must be numbers', 1);
    f.latitude = lat ? +lat : null; f.longitude = lng ? +lng : null; f.contact = f.contact.trim() || null;
    try { await api('ProviderProfiles' + (id ? '/' + id : ''), { method: id ? 'PUT' : 'POST', body: f }); S.hasProfile = true; save(); toast('Saved'); mine() }
    catch (x) { toast(x.message, 1) }
}
async function delP(id) {
    if (!confirm('Delete your business, its photos and its reviews? This cannot be undone.')) return;
    try { await api('ProviderProfiles/' + id, { method: 'DELETE' }); S.hasProfile = false; save(); toast('Business deleted'); mine() } catch (x) { toast(x.message, 1) }
}
async function imgs(pid) {
    try {
        const l = await api('ProviderImage/provider/' + pid);
        $('#imgs').innerHTML = `<h3>Photos</h3><div class="gal">${l.map(i => `<figure><img src="${esc(i.imageUrl)}" alt="" onerror="this.remove()"><button class="del ed" onclick="editImg(${i.id},${pid})">Edit</button> <button class="del" onclick="delImg(${i.id},${pid})">Delete</button></figure>`).join('')}</div>
    <p class="hint">Up to 10 photos. Upload from your device (jpg, png or webp, max 2 MB) or add an image link.</p>
    <form class="row up" onsubmit="upImg(event,${pid})"><input name="file" type="file" accept="image/png,image/jpeg,image/webp" required><button class="btn primary">Upload photo</button></form>
    <form class="row" onsubmit="addImg(event,${pid})"><input name="imageUrl" type="url" required placeholder="Image URL"><button class="btn line">Add by link</button></form>`
    } catch (x) { toast(x.message, 1) }
}
async function upImg(e, pid) {
    e.preventDefault(); const file = e.target.file.files[0]; if (!file) return;
    if (file.size > MAX_IMG) return toast('File is too large (max 2 MB)', 1);
    const fd = new FormData(); fd.append('file', file);
    try { await api('ProviderImage/upload', { method: 'POST', body: fd }); toast('Photo uploaded'); imgs(pid) } catch (x) { toast(x.message, 1) }
}
async function addImg(e, pid) { e.preventDefault(); try { await api('ProviderImage', { method: 'POST', body: { imageUrl: e.target.imageUrl.value } }); imgs(pid) } catch (x) { toast(x.message, 1) } }
async function editImg(id, pid) {
    try {
        const i = await api('ProviderImage/' + id), u = prompt('Image URL', i.imageUrl); if (!u || u === i.imageUrl) return;
        await api('ProviderImage/' + id, { method: 'PUT', body: { imageUrl: u } }); imgs(pid)
    } catch (x) { toast(x.message, 1) }
}
async function delImg(id, pid) { try { await api('ProviderImage/' + id, { method: 'DELETE' }); imgs(pid) } catch (x) { toast(x.message, 1) } }
async function myReviews(p) {
    try {
        const rv = await api('Review/provider/' + p.id);
        $('#myrev').innerHTML = `<h2>Customer reviews</h2><p class="rate">${rating(p.averageRating, p.reviewsCount)}</p><div class="panel">${rv.length ? rv.map(reviewItem).join('') : '<p class="mute">No reviews yet.</p>'}</div>`
    } catch (x) { toast(x.message, 1) }
}

/* ---------- Admin: categories, regions, providers, reviews ---------- */
let editing = null, L = [], q = '';
const T = {
    Categories: { res: 'Category', get: () => api('Category'), label: x => x.name, icon: 1, form: 1 },
    Regions: { res: 'Region', get: () => api('Region'), label: x => x.name, form: 1 },
    Providers: { res: 'ProviderProfiles', get: () => api('ProviderProfiles?pageSize=50'), label: x => `${x.businessName} (${x.categoryName}, ${x.regionName})` },
    Reviews: { res: 'Review', get: () => api('Review?pageSize=50'), label: x => `${stars(x.rating)} ${x.comment || '(no comment)'} by ${x.userName || 'User'}` }
};
async function admin(t = tab) {
    if (t !== tab) { editing = null; q = '' }
    tab = t; const c = T[t], e = editing || {};
    try { L = q ? [await api(c.res + '/' + encodeURIComponent(q))].filter(Boolean) : await c.get() } catch (x) { return toast(x.message, 1) }
    $('#v-admin').innerHTML = `<h2>Admin</h2><div class="tabs">${Object.keys(T).map(k => `<button class="${k === t ? 'on' : ''}" onclick="admin('${k}')">${k}</button>`).join('')}</div>
  <section class="panel">${c.form ? `<form class="row" style="margin:0 0 8px" onsubmit="return find(this)"><input name="s" placeholder="Find by exact name" value="${esc(q)}"><button class="btn primary">Find</button>${q ? '<button type="button" class="btn line" onclick="q=\'\';admin()">Clear</button>' : ''}</form>` : ''}
  <ul class="li">${L.map(x => `<li><span>${c.icon && x.iconPath ? `<img class="ico" src="${esc(x.iconPath)}" alt="" onerror="this.remove()">` : ''}${esc(c.label(x))}</span><span>${c.form ? `<button class="del ed" onclick="edit(${x.id})">Edit</button> ` : ''}<button class="del" onclick="rm('${c.res}',${x.id})">Delete</button></span></li>`).join('') || '<li class="mute">Nothing here yet</li>'}</ul></section>
  ${c.form ? `<section class="panel"><h3>${e.id ? 'Edit' : 'Add'} ${t === 'Categories' ? 'category' : 'region'}</h3>
  <form class="row up" onsubmit="saveItem(event)"><input name="n" required maxlength="100" placeholder="Name" value="${esc(e.name)}">${c.icon ? `<input name="icon" maxlength="500" placeholder="Icon link, e.g. https://..." value="${esc(e.iconPath)}"><input name="iconFile" type="file" accept="image/png,image/jpeg,image/webp" title="Or upload an icon from your device">` : ''}
  <button class="btn primary">${e.id ? 'Save changes' : 'Add'}</button>${e.id ? '<button type="button" class="btn line" onclick="editing=null;admin()">Cancel</button>' : ''}</form>${c.icon ? '<p class="hint">Use an icon link, or choose a file to upload (jpg, png or webp, max 2 MB). A chosen file replaces the link.</p>' : ''}</section>` : ''}`
}
function find(f) { q = f.s.value.trim(); admin(); return false }
function edit(id) { editing = L.find(x => x.id === id); admin() }
async function saveItem(e) {
    e.preventDefault(); const f = e.target, c = T[tab], name = f.n.value.trim();
    try {
        let body = { regionName: name };
        if (tab !== 'Regions') {
            let icon = f.icon.value.trim() || null; const file = f.iconFile.files[0];
            if (file) {
                if (file.size > MAX_IMG) return toast('File is too large (max 2 MB)', 1);
                const fd = new FormData(); fd.append('file', file);
                icon = (await api('Category/upload-icon', { method: 'POST', body: fd })).iconPath
            }
            body = { name, iconPath: icon }
        }
        await api(c.res + (editing ? '/' + editing.id : ''), { method: editing ? 'PUT' : 'POST', body }); toast(editing ? 'Updated' : 'Added'); editing = null; admin()
    } catch (x) { toast(x.message, 1) }
}
async function rm(p, i) {
    if (!confirm('Delete this item?')) return;
    try { await api(p + '/' + i, { method: 'DELETE' }); if (editing && editing.id === i) editing = null; admin() } catch (x) { toast(x.status === 409 ? x.message : 'Could not delete. It may still be in use.', 1) }
}

$('#find').addEventListener('submit', e => { e.preventDefault(); search() });
(async () => { nav(); if (has('Provider')) return go('mine'); try { await loadLists(); fill() } catch (x) { toast(x.message, 1) } search() })();