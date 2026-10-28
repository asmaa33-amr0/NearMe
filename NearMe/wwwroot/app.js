const $=s=>document.querySelector(s),
esc=s=>String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c])),
stars=n=>{n=Math.round(n)||0;return '★'.repeat(n)+'☆'.repeat(5-n)},
X='<button type="button" class="x" onclick="this.closest(\'dialog\').close()" aria-label="Close">×</button>';
let S=JSON.parse(localStorage.nm||'null'),cats=[],regs=[],tt,tab='Categories';
const save=()=>S?localStorage.nm=JSON.stringify(S):localStorage.removeItem('nm');
const has=r=>S&&S.roles.includes(r);

function toast(m,bad){const t=$('#toast');t.textContent=m;t.className='on'+(bad?' bad':'');clearTimeout(tt);tt=setTimeout(()=>t.className='',3500)}
function msg(s,d){
  if(typeof d==='string'&&d)return d;
  if(Array.isArray(d))return d.map(e=>e.description).join(', ');
  if(d&&d.message)return d.message;
  if(d&&d.errors)return Object.values(d.errors).flat().join(', ');
  return s===403?"You don't have permission to do that":s===404?'Not found':'Something went wrong. Try again.'}
async function api(p,o={}){
  const h={'Content-Type':'application/json'};if(S)h.Authorization='Bearer '+S.token;
  const r=await fetch('/api/'+p,{...o,headers:h,body:o.body?JSON.stringify(o.body):undefined});
  if(r.status===401&&S&&p!=='auth/login'){S=null;save();nav();go('browse');throw Error('Your session expired. Log in again.')}
  const t=await r.text();let d;try{d=t?JSON.parse(t):null}catch{d=t}
  if(!r.ok){const e=Error(msg(r.status,d));e.status=r.status;throw e}
  return d}
const loadLists=async()=>{[cats,regs]=await Promise.all([api('Category'),api('Region')])};
const opts=(l,sel,all)=>`<option value="">${all}</option>`+l.map(x=>`<option value="${x.id}"${x.id===sel?' selected':''}>${esc(x.name)}</option>`).join('');
const fill=()=>{$('#c').innerHTML=opts(cats,0,'All categories');$('#g').innerHTML=opts(regs,0,'All regions')};


function nav(){
  const r=has('Admin')?'Admin':has('Provider')?'Provider':'User';
  $('#nav').innerHTML=S?`<span class="role">${r}</span><button class="btn ghost" onclick="go('browse')">Home</button>${has('Provider')?`<button class="btn ghost" onclick="go('mine')">My business</button>`:''}${has('Admin')?`<button class="btn ghost" onclick="go('admin')">Admin</button>`:''}<button class="btn ghost" onclick="out()">Log out</button>`
  :`<button class="btn ghost" onclick="openAuth(false)">Log in</button><button class="btn sun" onclick="openAuth(true)">Sign up</button>`}
function go(v){
  ['browse','mine','admin'].forEach(k=>$('#v-'+k).hidden=k!==v);
  $('#hero').hidden=v!=='browse';
  if(v==='mine')mine();if(v==='admin')admin();if(v==='browse'){fill();search()}}
function out(){S=null;save();nav();go('browse')}

function openAuth(r){
  const a=$('#auth');
  a.innerHTML=`${X}<form onsubmit="doAuth(event,${r})"><h2>${r?'Create an account':'Log in'}</h2><br>
  ${r?'<label>Full name<input name="fullName" required maxlength="100"></label>':''}
  <label>Email<input name="email" type="email" required></label>
  <label>Password<input name="password" type="password" required minlength="6"></label>
  ${r?`<fieldset><legend>I'm joining NearMe as</legend>
  <label class="opt"><input type="radio" name="accountType" value="User" checked><b>A customer</b><span>I search for services and review them</span></label>
  <label class="opt"><input type="radio" name="accountType" value="Provider"><b>A service provider</b><span>I have a business and want to be found</span></label></fieldset>`:''}
  <button class="btn primary wide">${r?'Create account':'Log in'}</button>
  <p class="sw">${r?'Already have an account?':'New here?'} <a href="#" onclick="openAuth(${!r});return false">${r?'Log in':'Create an account'}</a></p></form>`;
  if(!a.open)a.showModal()}
async function doAuth(e,r){
  e.preventDefault();const f=Object.fromEntries(new FormData(e.target));
  try{
    if(r)await api('auth/register',{method:'POST',body:f});
    const d=await api('auth/login',{method:'POST',body:{email:f.email,password:f.password}});
    S={token:d.token,roles:d.roles,hasProfile:d.hasProfile};save();
    $('#auth').close();nav();toast('Welcome to NearMe');
    go(has('Admin')?'admin':has('Provider')?'mine':'browse')
  }catch(x){toast(x.message,1)}}

/* ---------- Customer: search, details, reviews ---------- */
async function search(){
  const q=new URLSearchParams();
  [['name','#q'],['categoryId','#c'],['regionId','#g']].forEach(([k,s])=>{const v=$(s).value.trim();if(v)q.set(k,v)});
  try{
    const l=await api('ProviderProfiles/search?'+q);
    $('#res').innerHTML=l.length?`<div class="grid">${l.map(card).join('')}</div>`:'<p class="empty">No results. Try another name or clear a filter.</p>'
  }catch(x){toast(x.message,1)}}
const rating=(a,n)=>n?`<b>${stars(a)}</b> ${a.toFixed(1)} (${n} ${n===1?'review':'reviews'})`:'No reviews yet';
const card=p=>`<button class="card" onclick="show(${p.id})"><span class="tag">${esc(p.categoryName)}</span><h3>${esc(p.businessName)}</h3><span class="mute">${esc(p.regionName)}</span><span class="mute">${esc(p.address)}</span><span class="rate">${rating(p.averageRating,p.reviewsCount)}</span></button>`;
const reviewItem=r=>`<article class="rv"><b>${stars(r.rating)}</b> <span class="mute">${esc(r.userName||'User')}</span><p>${esc(r.comment)}</p></article>`;

async function show(id){
  const d=$('#dr');d.innerHTML=X+'<p class="empty">Loading...</p>';if(!d.open)d.showModal();
  try{
    const [p,rv]=await Promise.all([api('ProviderProfiles/'+id),api('Review/provider/'+id)]);
    d.innerHTML=`${X}<span class="tag">${esc(p.category.name)}</span><h2>${esc(p.businessName)}</h2>
    <p class="mute">${esc(p.region)}, ${esc(p.address)}</p><p class="rate">${rating(p.averageRating,p.reviewsCount)}</p>
    ${p.description?`<p>${esc(p.description)}</p>`:''}
    ${p.gallery.length?`<div class="gal">${p.gallery.map(g=>`<img src="${esc(g.imageUrl)}" alt="" loading="lazy" onerror="this.remove()">`).join('')}</div>`:''}
    <h3>Reviews</h3>${rv.length?rv.map(reviewItem).join(''):'<p class="mute">No reviews yet. Be the first to review.</p>'}
    <br>${S?`<form onsubmit="rate(event,${id})"><label>Your rating<select name="rating"><option value="5">★★★★★ Excellent</option><option value="4">★★★★ Good</option><option value="3">★★★ Okay</option><option value="2">★★ Poor</option><option value="1">★ Bad</option></select></label><label>Your comment<textarea name="comment" rows="3" maxlength="1000"></textarea></label><button class="btn primary">Post review</button></form>`
    :`<button class="btn primary" onclick="$('#dr').close();openAuth(false)">Log in to leave a review</button>`}`
  }catch(x){d.close();toast(x.message,1)}}
async function rate(e,id){
  e.preventDefault();const f=Object.fromEntries(new FormData(e.target));
  try{await api('Review',{method:'POST',body:{rating:+f.rating,comment:f.comment,providerProfileId:id}});toast('Review posted');show(id);search()}
  catch(x){toast(x.message,1)}}

/* ---------- Provider: business profile, gallery, reviews ---------- */
async function mine(){
  const v=$('#v-mine');v.innerHTML='<p class="empty">Loading...</p>';
  try{await loadLists()}catch(x){return toast(x.message,1)}
  let p=null;
  try{p=await api('ProviderProfiles/my-profile')}catch(x){if(x.status!==404)toast(x.message,1)}
  const cs=p&&cats.find(c=>c.name===p.category.name)?.id,rs=p&&regs.find(r=>r.name===p.region)?.id;
  v.innerHTML=`<h2>${p?'Your business':'Set up your business'}</h2>
  <p class="mute">${p?'Changes show up for customers right away.':'Fill this in so your business appears in search results.'}</p>
  <form class="panel" onsubmit="saveP(event,${p?p.id:0})">
  <label>Business name<input name="businessName" required maxlength="200" value="${esc(p?.businessName)}"></label>
  <div class="two"><label>Category<select name="categoryId" required>${opts(cats,cs,'Choose a category')}</select></label>
  <label>Region<select name="regionId" required>${opts(regs,rs,'Choose a region')}</select></label></div>
  <label>Address<input name="address" required maxlength="500" value="${esc(p?.address)}"></label>
  <label>Description<textarea name="description" rows="4" maxlength="2000">${esc(p?.description)}</textarea></label>
  <button class="btn primary">${p?'Save changes':'Create business'}</button></form>
  <div id="imgs"></div><div id="myrev"></div>`;
  if(p){imgs(p.id);myReviews(p)}}
async function saveP(e,id){
  e.preventDefault();const f=Object.fromEntries(new FormData(e.target));f.categoryId=+f.categoryId;f.regionId=+f.regionId;
  try{await api('ProviderProfiles'+(id?'/'+id:''),{method:id?'PUT':'POST',body:f});S.hasProfile=true;save();toast('Saved');mine()}
  catch(x){toast(x.message,1)}}
async function imgs(pid){
  try{
    const l=await api('ProviderImage/provider/'+pid);
    $('#imgs').innerHTML=`<h3>Photos</h3><div class="gal">${l.map(i=>`<figure><img src="${esc(i.imageUrl)}" alt="" onerror="this.remove()"><button class="del" onclick="delImg(${i.id},${pid})">Delete</button></figure>`).join('')}</div>
    <form class="row" onsubmit="addImg(event,${pid})"><input name="imageUrl" type="url" required placeholder="Image URL"><button class="btn primary">Add photo</button></form>`
  }catch(x){toast(x.message,1)}}
async function addImg(e,pid){e.preventDefault();try{await api('ProviderImage',{method:'POST',body:{imageUrl:e.target.imageUrl.value}});imgs(pid)}catch(x){toast(x.message,1)}}
async function delImg(id,pid){try{await api('ProviderImage/'+id,{method:'DELETE'});imgs(pid)}catch(x){toast(x.message,1)}}
async function myReviews(p){
  try{
    const rv=await api('Review/provider/'+p.id);
    $('#myrev').innerHTML=`<h2>Customer reviews</h2><p class="rate">${rating(p.averageRating,p.reviewsCount)}</p><div class="panel">${rv.length?rv.map(reviewItem).join(''):'<p class="mute">No reviews yet.</p>'}</div>`
  }catch(x){toast(x.message,1)}}

/* ---------- Admin: categories, regions, providers, reviews ---------- */
const T={
  Categories:{get:()=>api('Category'),row:x=>[x.name,'Category',x.id],add:['name','Category name']},
  Regions:{get:()=>api('Region'),row:x=>[x.name,'Region',x.id],add:['regionName','Region name']},
  Providers:{get:()=>api('ProviderProfiles?pageSize=50'),row:x=>[`${x.businessName} (${x.categoryName}, ${x.regionName})`,'ProviderProfiles',x.id]},
  Reviews:{get:()=>api('Review?pageSize=50'),row:x=>[`${stars(x.rating)} ${x.comment||'(no comment)'} by ${x.userName||'User'}`,'Review',x.id]}};
async function admin(t=tab){
  tab=t;const c=T[t];let l;
  try{l=await c.get()}catch(x){return toast(x.message,1)}
  $('#v-admin').innerHTML=`<h2>Admin</h2><div class="tabs">${Object.keys(T).map(k=>`<button class="${k===t?'on':''}" onclick="admin('${k}')">${k}</button>`).join('')}</div>
  <section class="panel"><ul class="li">${l.map(x=>{const[a,p,i]=c.row(x);return `<li><span>${esc(a)}</span><button class="del" onclick="rm('${p}',${i})">Delete</button></li>`}).join('')||'<li class="mute">Nothing here yet</li>'}</ul>
  ${c.add?`<form class="row" onsubmit="add(event,'${c.add[0]}')"><input name="n" required placeholder="${c.add[1]}"><button class="btn primary">Add</button></form>`:''}</section>`}
async function add(e,k){e.preventDefault();try{await api(tab==='Regions'?'Region':'Category',{method:'POST',body:{[k]:e.target.n.value}});admin()}catch(x){toast(x.message,1)}}
async function rm(p,i){
  if(!confirm('Delete this item?'))return;
  try{await api(p+'/'+i,{method:'DELETE'});admin()}catch(x){toast('Could not delete. It may still be in use.',1)}}

$('#find').addEventListener('submit',e=>{e.preventDefault();search()});
(async()=>{nav();try{await loadLists();fill()}catch(x){toast(x.message,1)}search()})();
