# Poängtavlan — Design & UX Specification
**Version:** 1.0
**Plattform:** Mobile-first webb (PWA-redo)
**Målgrupp:** Vängrupper som spelar sällskapsspel eller fysiska sporter (dart, minigolf, m.m.)
**Stack:** Angular + ASP.NET Core (eller valfritt ramverk som respekterar detta designsystem)

---

## 1. Produktidentitet

### Vad är Poängtavlan?
En enkel, stilren poängapp för informella sammanhang. Användaren sitter runt ett bord, håller telefonen i handen och vill komma igång på under 30 sekunder. Inga konton, ingen inloggning, inget krångel.

### Varumärkeskaraktär
- **Energisk men ren.** Stark typografi, hög kontrast, avskalad layout.
- **Kinetisk editorial.** Inspirerat av sportmagasin och dark-mode dashboards. Känns levande utan att vara stökig.
- **Lekfull precision.** Inte ett barnspel, inte ett enterprise-verktyg. Exakt mitten: tydlig struktur med ett leende.

### Ton i text (microcopy)
- Svenska genomgående.
- Direkt och kortfattad. "Skapa match" inte "Klicka här för att skapa en ny match".
- Tilläts vara lite fräck när kontexten tillåter (t.ex. 404-sidan: "Domaren blåste av i förtid").
- Aldrig pekpinnar eller förklarande text som användaren inte frågat efter.

---

## 2. Design Tokens

Dessa värden är sanningskällan. Alla komponenter använder dessa variabler. Hårdkodade färger eller avvikande värden är **förbjudet**.

### 2.1 Färgpalett (Dark Mode — primär)

Hela applikationen är dark mode. Light mode är inte planerat i v1.

```css
/* Bakgrunder */
--color-background:              #0c0e11;   /* Djupaste bakgrunden */
--color-surface:                 #0c0e11;   /* Sidbakgrund */
--color-surface-dim:             #0c0e11;
--color-surface-container-lowest:#000000;
--color-surface-container-low:   #111417;   /* Sektion/kort (lägre nivå) */
--color-surface-container:       #171a1d;
--color-surface-container-high:  #1d2024;   /* Inputs, kort */
--color-surface-container-highest:#23262a;  /* Aktiva element, player cards */
--color-surface-bright:          #292c31;
--color-surface-variant:         #23262a;

/* Primär (blå) */
--color-primary:                 #84adff;   /* Huvud-accent, rubriker, aktiv nav */
--color-primary-container:       #6c9fff;
--color-primary-fixed:           #6c9fff;
--color-primary-fixed-dim:       #5091ff;
--color-primary-dim:             #0070ea;
--color-on-primary:              #002d64;
--color-on-primary-fixed:        #000000;
--color-on-primary-container:    #00214e;
--color-on-primary-fixed-variant:#002a60;
--color-inverse-primary:         #005bc1;

/* Sekundär (gul) */
--color-secondary:               #ffd709;   /* Leader-glow, highlights */
--color-secondary-container:     #705d00;
--color-secondary-fixed:         #ffd709;
--color-secondary-fixed-dim:     #efc900;
--color-secondary-dim:           #efc900;
--color-on-secondary:            #5b4b00;
--color-on-secondary-fixed:      #453900;
--color-on-secondary-container:  #fff7e6;
--color-on-secondary-fixed-variant:#665500;

/* Tertiär (vit/off-white) */
--color-tertiary:                #faf9f9;
--color-tertiary-container:      #ebebeb;
--color-tertiary-fixed:          #ffffff;
--color-tertiary-fixed-dim:      #f1f0f0;
--color-tertiary-dim:            #ebebeb;
--color-on-tertiary:             #5e5f5f;
--color-on-tertiary-container:   #565757;
--color-on-tertiary-fixed:       #4f5051;
--color-on-tertiary-fixed-variant:#6c6d6d;

/* Error (röd) */
--color-error:                   #ff716c;
--color-error-container:         #9f0519;
--color-error-dim:               #d7383b;
--color-on-error:                #490006;
--color-on-error-container:      #ffa8a3;

/* Text & outlines */
--color-on-surface:              #f9f9fd;   /* Primär text */
--color-on-surface-variant:      #aaabaf;   /* Sekundär text, labels */
--color-on-background:           #f9f9fd;
--color-outline:                 #747579;   /* Borders, ikoner */
--color-outline-variant:         #46484b;   /* Subtila borders */
--color-inverse-surface:         #f9f9fd;
--color-inverse-on-surface:      #535559;
--color-surface-tint:            #84adff;
```

### 2.2 Semantisk användning av färger

| Syfte | Token |
|---|---|
| Bakgrund (sida) | `--color-background` |
| Kort / panel | `--color-surface-container-high` |
| Aktiv nav-item bakgrund | `--color-surface-container-high` |
| Primär knapp (fill) | `--color-primary` |
| Primär knapp (text) | `--color-on-primary-fixed` |
| Input-bakgrund | `--color-surface-container-high` |
| Leader-accent (gul) | `--color-secondary` |
| Error/destruktiv | `--color-error` |
| Etikett / muted text | `--color-on-surface-variant` |
| Dividers | `--color-outline-variant` @ 10-20% opacity |

### 2.3 Typografi

**Maxregel: 2 typsnitt. Aldrig mer.**

| Roll | Font | Vikt(er) | Användning |
|---|---|---|---|
| `font-headline` | Space Grotesk | 700, 900 | Rubriker, logotyp, scores, knappar |
| `font-body` / `font-label` | Manrope | 400, 500, 600, 700 | Brödtext, labels, nav-items, inputs |

**Typskala (Major Third 1.25x, bas 16px):**

| Namn | Storlek | Vikt | Line-height | Letter-spacing | Användning |
|---|---|---|---|---|---|
| `display` | 96–128px | 900 | 1.0 | -0.04em | Stora poängsiffror |
| `headline-lg` | 48–64px | 900 | 1.05 | -0.03em | Sidrubriker (`h1`) |
| `headline-md` | 32px | 700 | 1.1 | -0.02em | Sektionsrubriker (`h2`) |
| `headline-sm` | 20–24px | 700 | 1.2 | -0.01em | Kortrubriker, spelarnamn |
| `body-lg` | 18px | 400 | 1.6 | 0 | Beskrivande text |
| `body-md` | 16px | 400–500 | 1.5 | 0 | Inputs, formulärelement |
| `label-lg` | 14px | 600–700 | 1.4 | +0.05em | Knappar (versaler) |
| `label-sm` | 11–12px | 600 | 1.3 | +0.15–0.2em | Nav-labels, badges (versaler) |

**Regler:**
- ALL CAPS (versaler) kräver alltid `letter-spacing: +0.1em` minimum.
- Poängsiffror använder `font-headline` weight 900, tracking `-0.04em`.
- Aldrig `font-style: italic` utom i hjälptext / placeholder-kontext.

### 2.4 Spacing (8pt grid)

Alla margin, padding och gap-värden MÅSTE vara multiplar av 4px. Primärt multiplar av 8px.

```
4px   xs    — Internt i komponenter (icon gap, badge padding)
8px   sm    — Label → input gap, icon margin
12px  md    — Padding i kompakta element
16px  lg    — Standard padding i kort
24px  xl    — Gap mellan relaterade sektioner
32px  2xl   — Padding i spaciösa kort
48px  3xl   — Gap mellan orelaterade sektioner
64px  4xl   — Sidmarginaler desktop, hero spacing
96px  5xl   — Stor whitespace (hero, 404)
```

**Intern ≤ Extern-regeln:** Padding inuti ett kort måste alltid vara MINDRE ÄN gapet mellan korten. Bryts denna regel ser layouten söndrig ut.

### 2.5 Border-radius

Applikationen använder ett **"rounded" system** — inte helt cirkulärt, men generöst avrundad. Nestade element har alltid MINDRE radius än sin parent.

```css
--radius-xs:   2px;    /* Knappar inuti knappar, tiny badges */
--radius-sm:   4px;    /* Subtle: tags, chips */
--radius-md:   8px;    /* Knappar, inputs (standard) */
--radius-lg:   12px;   /* Nav-items, settings rows */
--radius-xl:   16px;   /* Kort, panels */
--radius-2xl:  24px;   /* Stora kort, glass-card */
--radius-3xl:  32px;   /* Feature cards, hero sections */
--radius-full: 9999px; /* Pills, avatarer, FAB */
```

**Tailwind-mappning (befintlig config):**
- `rounded` = 2px
- `rounded-lg` = 4px
- `rounded-xl` = 8px
- `rounded-full` = 12px

> ⚠️ OBS: Tailwinds `rounded-full` i nuvarande config är 12px, INTE 9999px. Använd inline-stil `border-radius: 9999px` för äkta pills/cirklar.

### 2.6 Elevation & Shadows (dark mode)

I dark mode skapas djup med **ljusare surfaces**, inte skuggor.

| Nivå | Bakgrundsfärg | Användning |
|---|---|---|
| 0 (grund) | `#0c0e11` | Sidbakgrund |
| 1 | `#111417` | Sekundär bakgrund, sidofält |
| 2 | `#1d2024` | Kort, inputs |
| 3 | `#23262a` | Aktiva element, hover-states |
| 4 | `#292c31` | Topp-layers, tooltips |

**Glöd-effekter (sparsamt):**
```css
/* Leader card — gul glöd */
box-shadow: -4px 0 20px -5px var(--color-secondary);

/* Primary CTA — blå glöd (hover) */
box-shadow: 0 0 40px rgba(132, 173, 255, 0.2);

/* FAB-knapp */
box-shadow: 0 0 40px rgba(132, 173, 255, 0.4);
```

Glöd används ENBART för:
1. Leader/vinnande spelare (gul)
2. Hover-state på primär CTA
3. FAB (Floating Action Button)

### 2.7 Ikoner

**Ikonset:** Material Symbols Outlined
**Variation:** `'FILL' 0, 'wght' 400, 'GRAD' 0, 'opsz' 24`
**Aktiv state:** `'FILL' 1` (ifylld ikon för aktiv nav-item)

```css
.material-symbols-outlined {
  font-variation-settings: 'FILL' 0, 'wght' 400, 'GRAD' 0, 'opsz' 24;
}
```

Alla ikoner i appen använder denna ikon-familj. Blanda ALDRIG ikonset.

---

## 3. Layout & Navigation

### 3.1 Breakpoints

```
Mobile:  < 768px   (md)  — primär designyta
Tablet:  768–1024px       — anpassad desktop-layout börjar
Desktop: > 1024px  (lg)   — full sidebar-layout
```

### 3.2 Shell-struktur

**Mobile:**
```
┌─────────────────────────┐
│  TopAppBar (sticky)     │  h: 56-64px, bg: #0c0e11
├─────────────────────────┤
│                         │
│  Main Content (scroll)  │  flex-1, overflow-y: auto
│                         │
│                         │
├─────────────────────────┤
│  BottomNavBar (sticky)  │  h: 56-64px, bg: #111417
└─────────────────────────┘
```

**Desktop (md+):**
```
┌──────────────────────────────────────────┐
│           TopAppBar (sticky)             │
├───────────┬──────────────────────────────┤
│           │                              │
│  Sidebar  │     Main Content (scroll)    │
│  w: 256px │                              │
│           │                              │
│           │                              │
├───────────┴──────────────────────────────┤
│                  Footer                  │
└──────────────────────────────────────────┘
```

### 3.3 TopAppBar

**Mobile:**
- Vänster: Logo "Poängtavlan" — `font-headline font-black text-primary tracking-tighter uppercase`
- Höger: Ikonknappar (historik, inställningar)
- Bakgrund: `#0c0e11` med `sticky top-0 z-50`
- Höjd: 56px (py-4 px-6)

**Desktop (på Scoreboard-sidan):**
- Vänster: Matchkontext (`Pågående Match` label + matchnamn i `text-primary`)
- Höger: Ikonknappar

**Regler:**
- TopAppBar är alltid sticky, alltid ovan allt (z-50).
- Bakgrunden är alltid `#0c0e11` (INTE surface, trots att de ser likadana ut just nu — separata tokens för framtida flexibilitet).

### 3.4 Sidebar (Desktop only)

- Bredd: 256px, `shrink-0`
- Bakgrund: `#0c0e11`, border-right: `border-[#1d2024]`
- Höjd: full viewport, sticky
- Innehåll: Logo + version, nav-items, "Ny Match"-knapp längst ner
- **Aktiv nav-item:** `bg-surface-container-high text-primary font-bold border-r-4 border-primary`
- **Inaktiv nav-item:** `text-slate-500 hover:text-slate-200 hover:bg-surface-container-low`
- Nav-item padding: `px-4 py-3`

### 3.5 BottomNavBar (Mobile only)

- Höjd: 56px, `sticky bottom-0 z-50`
- Bakgrund: `#111417`, border-top: `1px #1d2024`
- 4 items: Resultat, Spelare, Stats, Inställningar
- Aktiv item: `text-primary`, ikon med `'FILL' 1`
- Inaktiv item: `text-slate-500`
- Label: 10px, uppercase, `font-label`

**OBS:** FAB (Floating Action Button) är INTE en del av BottomNavBar-gridet. Den sitter fixerad `bottom-6 right-6` ovanpå innehållet med `z-[100]`.

### 3.6 FAB (Mobile)

```
- Position: fixed, bottom: 24px, right: 24px
- Storlek: 56x56px (w-14 h-14)
- Form: cirkel (border-radius: 9999px)
- Bakgrund: --color-primary
- Ikon: add (Material Symbols), 32px
- Shadow: box-shadow: 0 0 40px rgba(132,173,255,0.4)
- Active: scale(0.9)
```

---

## 4. Komponenter

### 4.1 Knappar

**Tre nivåer. En primär per sektion. Alltid.**

#### Primary Button
```
bg: --color-primary
text: --color-on-primary-fixed (#000)
font: font-headline font-black uppercase tracking-widest
padding: py-4 px-8 (48px höjd minimum)
radius: --radius-xl (8px i befintlig config)
hover: box-shadow glow-effekt
active: scale(0.98)
transition: all 200ms ease-out
```

Fullbredd på mobile (`w-full`). Maxbredd med auto margin på desktop.

#### Secondary Button (outline/border)
```
bg: transparent
border: 1px solid --color-outline-variant/30
text: --color-on-surface
font: font-headline font-bold uppercase
hover: bg-surface-container-high
active: scale(0.95)
```

#### Icon Button (rund)
```
Storlek: 40x40px (p-2, ikon 24px)
Form: cirkel
bg: transparent
hover: bg-surface-container-high
active: scale(0.95)
```

Touch-target MINST 44x44px (lägg transparent padding om nödvändigt).

### 4.2 Inputs

**Varje input kräver en synlig label ovan. Aldrig placeholder-only.**

```
label:
  font: font-label text-on-surface-variant uppercase tracking-[0.2em] text-xs

input:
  bg: --color-surface-container-high
  border: none (fokus hanteras av ring)
  border-radius: --radius-xl (8px)
  padding: px-6 py-4 (56px höjd)
  font: font-headline text-xl (för primära inputs) / font-body text-lg (standard)
  text: --color-on-surface
  placeholder: --color-outline/40
  focus: ring-2 ring-primary (outline: none)
  transition: all 200ms
```

Label → Input gap: 8px (space-y-3 ≈ 12px, acceptabelt).

### 4.3 Toggle (Vinstvillkor)

```
Struktur: Omslutande div med bg-surface-container-high p-1 rounded-xl
Aktiv tab:
  bg: --color-primary
  text: --color-on-primary-fixed
  font-bold
  box-shadow: shadow-lg
  border-radius: --radius-lg (4px)
Inaktiv tab:
  text: --color-outline-variant
  hover: text-on-surface
```

### 4.4 Toggle Switch (Checkbox)

```
Bredd: 44px (w-11), höjd: 24px (h-6)
Track inaktiv: --color-surface-container-highest
Track aktiv: --color-primary
Thumb: vit, 20x20px (after:h-5 after:w-5)
Transition: translate-x + colors 200ms ease-in-out
```

### 4.5 Player Card

Används i spelarlistan under matchskapande och på scoreboard.

**Kompakt (spelarlista vid skapande):**
```
bg: --color-surface-container-highest
border-left: 4px solid [spelarfärg]
border-radius: --radius-2xl (24px i befintlig = rounded-full 12px → behöver explicit värde)
padding: p-4
gap till avatar + namn: gap-4
Avatar: 40x40px, cirkel, bg = spelarfärg, text = initial
Namn: font-bold text-lg
Ta bort-knapp: text-outline hover:text-error
```

**Spelarfärger (rotation, 4 stycken):**
```
1. --color-secondary  (#ffd709 gul)
2. --color-primary    (#84adff blå)
3. --color-error      (#ff716c röd)
4. --color-tertiary   (#faf9f9 vit)
```
Vid fler spelare: börja om från index 0.

### 4.6 Scoreboard Cards

**Leader Card (1:a plats):**
```
bg: --color-surface-container-high
border-left: 4px solid --color-secondary
box-shadow: -4px 0 20px -5px --color-secondary (leader-glow)
padding: p-8
hover: translateY(-4px)
transition: all 300ms ease-out
cursor: pointer

Internt:
  - Badge "Leader" + placeringstext
  - Spelarnamn: text-4xl md:text-5xl font-headline font-bold
  - Poäng: text-[6rem] md:text-[8rem] font-headline font-black text-primary leading-none
  - "Klicka för att ändra" hint: text-slate-400 text-xs uppercase
  - Dekorativt bakgrundsikon: opacity-5, position absolute
```

**Rank 2–3 Cards:**
```
bg: --color-surface-container-low
border: 1px solid --color-outline-variant/10
padding: p-6
hover: bg-surface-container-high
transition: all 200ms

Placering: text-xl font-headline font-bold (färgsatt: 2:a = tertiary-fixed-dim, 3:e = slate-400)
Namn: text-xl font-headline font-bold
Poäng: text-5xl font-headline font-bold text-on-surface-variant
```

**Rank 4+:**
```
bg: --color-surface-container-low/50
padding: p-4
flex justify-between items-center
hover: bg-surface-container-high
transition: colors 200ms

Nummer: text-slate-600 font-headline font-bold w-4
Namn: font-headline font-medium
Poäng: font-headline font-bold text-2xl text-on-surface-variant
Meny-knapp: text-slate-500 (more_vert ikon)
```

### 4.7 Glass Card

Används för player-list panelen vid matchskapande.

```css
.glass-card {
  background: rgba(35, 38, 42, 0.6);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(70, 72, 75, 0.15); /* outline-variant/15 */
}
```

### 4.8 Badges / Tags

```
Inline badge (t.ex. "Leader"):
  bg: --color-secondary
  text: --color-on-secondary-fixed
  font: font-black text-[0.65rem] uppercase tracking-widest
  padding: px-2 py-0.5
  border-radius: --radius-xs (2px)

Status-pill (t.ex. "Felmeddelande: ..."):
  bg: --color-surface-container-high
  border: 1px solid --color-outline-variant/10
  text: --color-secondary font-bold text-xs uppercase tracking-[0.2em]
  padding: px-4 py-1.5
  border-radius: full (9999px)
```

---

## 5. Skärmar & Flöden

### 5.1 Skapa Match (`/new`)

**Syfte:** Konfigurera en ny spelomgång. Enkel, snabb.
**Primär action:** Skapa match (stort, tydligt CTA-knapp längst ner)

**Layout (mobile, single column):**
1. Header: rubrik + beskrivning
2. Matchnamn (input)
3. Vinstvillkor (toggle) + Startpoäng (input) — 2-kolumners grid på md+
4. Lägg till spelare (input + knapp)
5. Lås spelarlista (toggle switch)
6. [Mobile CTA] Skapa match

**Layout (desktop, 12-kolumner):**
- Vänster 7 kol: Formuläret
- Höger 5 kol: Glass card med spelarlistan (live preview)
- Desktop CTA: sist i vänsterkolumnen

**UX-principer:**
- Spelarlistan uppdateras live när spelare läggs till.
- "Lägg till"-knappen (person_add) är sekundär primary-container, inte primär — det finns bara en primär per vy.
- Tomma listan visar en tom-state (tom glass card med uppmaning).
- Enter i spelarens namnfält = lägg till spelare.

**Empty state (spelarlistan):**
```
Ikon: groups (material symbols, stor, text-outline/30)
Text: "Inga spelare ännu. Lägg till minst 2 för att börja."
font-label text-xs uppercase tracking-widest text-outline italic
```

### 5.2 Scoreboard / Pågående Match (`/match/:id`)

**Syfte:** Se poängställning i realtid. Klicka på spelare för att ändra poäng.
**Primär action:** Klicka/tappa på spelarens kortcard för att öppna poängredigering.

**Layout (mobile):**
- TopAppBar: Matchnamn
- Vertikal lista: Leader → Rank 2 → Rank 3 → Rank 4+ rader
- FAB: Lägg till spelare (om inte låst)
- BottomNavBar

**Layout (desktop):**
- Vänster 8 kol: Scoreboard (leader + grid + rader)
- Höger 4 kol: Hantera spelare + snabbval + matchbild

**Interaktion — poängredigering:**
- Klick/tap på en spelarrad öppnar en bottom sheet (mobile) eller modal (desktop).
- Bottom sheet: glider upp från botten, overlay-bakgrund `rgba(0,0,0,0.5)`.
- Modal: max-width 480px, centrerad, overlay-bakgrund `rgba(0,0,0,0.5)`.
- Innehåll: spelarnamn, nuvarande poäng, +/- knappar, numerisk input, "Spara".

**Feedback:**
- Poängsiffran animerar vid ändring: `transform: scale(1.1) → scale(1)` 150ms ease-out.
- Rangordning omräknas direkt (realtid om SignalR, annars optimistisk uppdatering).

### 5.3 404-sida

**Syfte:** Mjuk felsida som matchar appens estetik.

**Layout:**
- Centrerad vertikalt och horisontellt, full viewport
- Stort "404" i `display`-skala, `text-primary`
- Brutalistisk outline-text (offset, 20% opacity) som dekorativt lager
- Pill-badge: felinformation
- Rubrik + förklaringstext
- CTA-knapp: "Tillbaka till startsidan" (rounded-full, primary)
- Dekorativa bilder/element: `hidden lg:block`, abstrakta, grayscale

**CTA-knapp (unik stil, bara på 404):**
```
border-radius: 9999px (rounded-full)
padding: px-10 py-5
font: font-headline font-black text-xl tracking-tight
hover: scale(1.05)
active: scale(0.95)
icon: home (leading) + arrow_forward (trailing, animated på hover)
```

---

## 6. Animationer & Micro-interactions

**Grundregel:** Animera bara `transform` och `opacity`. Aldrig `width`, `height`, `top`, `left`.

### 6.1 Timing-tabell

| Interaktion | Varaktighet | Easing |
|---|---|---|
| Knapptryck (active state) | 100ms | ease-out |
| Hover-transitions | 200ms | ease-out |
| Kortets hover lift | 300ms | ease-out |
| Bottom sheet in | 350ms | cubic-bezier(0.0, 0.0, 0.2, 1) |
| Bottom sheet ut | 250ms | cubic-bezier(0.4, 0.0, 1, 1) |
| Modal in | 200ms | ease-out |
| Poäng-siffra update | 150ms | ease-out |
| Page/list stagger | 50ms per item | ease-out |

### 6.2 Specifika animationer

**Spelarkortet i leader-position:**
```css
transition: transform 300ms ease-out;
hover: translateY(-4px);
```

**Poängsiffran vid uppdatering:**
```css
@keyframes scoreUpdate {
  0%   { transform: scale(1); }
  50%  { transform: scale(1.15); }
  100% { transform: scale(1); }
}
animation: scoreUpdate 300ms ease-out;
```

**Knapp active-state:**
```css
active: transform: scale(0.95) (mobil) eller scale(0.98) (desktop)
```

**Stagger-effekt (spellistan laddas):**
```css
/* Varje kort: animation-delay: index * 60ms */
animation: fadeInUp 300ms ease-out both;

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(16px); }
  to   { opacity: 1; transform: translateY(0); }
}
```

### 6.3 `prefers-reduced-motion`

```css
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```

---

## 7. Responsiv Strategi

### Mobile (< 768px) — Primär designyta

- Single column layout
- BottomNavBar för primär navigation
- FAB för primär action
- Knappar alltid fullbredd i formulärkontext
- Rubrikstorlekar nedskalade: `headline-lg` 40–48px istället för 64px
- Scoreboard: vertikal stack med leader överst
- Alla knappar min 48px höjd (touch-target)
- Inga hover-states på mobile (`:hover` wrappas i `@media (hover: hover)`)

### Desktop (768px+)

- Sidebar ersätter BottomNavBar
- FAB ersätts av "Ny Match"-knapp i sidebar
- Scoreboard: 8+4 kolumner grid
- Matchskapande: 7+5 kolumner grid
- Sidmarginaler: px-12 → max-w-4xl mx-auto

---

## 8. Tillgänglighet

Minimumkrav för v1:

| Krav | Implementation |
|---|---|
| Touch targets | Min 44x44px (helst 48px) på alla interaktiva element |
| Färgkontrast | `--color-on-surface` (#f9f9fd) på `#0c0e11` = ~14:1 (godkänt) |
| Fokusring | Synlig `ring-2 ring-primary` på alla interaktiva element vid tangentbordsfokus |
| Ikoners semantik | Ikoner utan text-label får `aria-label` |
| Formulär | Varje input har en `<label>` med `for`-attribut (eller `aria-label`) |
| Felmeddelanden | Feltext ackompanjeras av ikon + röd färg (ej enbart färg) |
| Semantic HTML | `<header>`, `<main>`, `<nav>`, `<footer>`, `<section>`, `<h1>`→`<h3>` korrekt hierarki |
| ARIA live regions | Poänguppdateringar annonseras via `aria-live="polite"` |

---

## 9. Sidor & Routes (v1)

| Route | Sida | Status |
|---|---|---|
| `/` | Redirect → `/new` eller `/match/:id` om aktiv match | Planerad |
| `/new` | Skapa ny match | Mockup klar |
| `/match/:id` | Pågående match / scoreboard | Mockup klar |
| `/history` | Matchhistorik (lista) | Ej mockupad |
| `/players` | Spelarhantering | Ej mockupad |
| `/stats` | Statistik | Ej mockupad |
| `/settings` | Inställningar | Ej mockupad |
| `*` | 404 | Mockup klar |

---

## 10. Komponenthierarki (för implementation)

```
App Shell
├── TopAppBar
├── Sidebar (md+)
├── Main (router-outlet)
│   ├── NewMatchPage
│   │   ├── MatchNameInput
│   │   ├── WinConditionToggle
│   │   ├── StartingScoreInput
│   │   ├── PlayerAddInput
│   │   ├── LockListToggle
│   │   ├── PlayerListCard (glass)
│   │   │   └── PlayerCard (×n)
│   │   └── CreateMatchButton
│   ├── ScoreboardPage
│   │   ├── LeaderCard
│   │   ├── RankCard (×2, rank 2–3)
│   │   ├── PlayerRow (×n, rank 4+)
│   │   ├── PlayerManagementPanel (desktop)
│   │   └── ScoreEditSheet / Modal
│   └── NotFoundPage
├── BottomNavBar (< md)
└── FAB (< md)
```

---

## 11. Shared Patterns & Rules för AI-agenter

Nedan följer specificerade Do/Don't för implementationsagenter:

### DO
- Använd CSS-variablerna (tokens) för alla färger - aldrig hårdkodade hex
- Håll alla spacing-värden på 4px-multiplar (8px primary)
- Sätt `active:scale-95` och `transition-all duration-200` på alla interaktiva element
- Använd `font-headline` (Space Grotesk) för all text som är "hero" eller siffror
- Använd `font-body` / `font-label` (Manrope) för brödtext och UI-labels
- Gör alla labels uppercase med `tracking-[0.2em]`
- Placera primär CTA längst ner på sidan (mobile-first scroll)
- Testa alltid med en tom lista, en spelare och tio spelare

### DON'T
- Blanda ikonset - ENBART Material Symbols Outlined
- Placera mer än en primär (solid fill) knapp per visuell sektion
- Använda `rounded-full` i Tailwind-config (= 12px, inte cirkel) för pills/avatarer — använd explicit `border-radius: 9999px`
- Animera `height`, `width`, `top`, `left`
- Ta bort `backdrop-filter: blur` från glass-card — det är en designidentitet
- Skriva text som inte är svenska (utom tekniska termer)
- Använda `text-white` - använd `text-on-surface` (`#f9f9fd`)
- Använda `bg-black` - använd `bg-surface-container-lowest` (`#000000`)

---

## 12. Changelog

| Version | Datum | Ändring |
|---|---|---|
| 1.0 | 2026-04-21 | Initial spec baserad på tre mockup-skärmar |
