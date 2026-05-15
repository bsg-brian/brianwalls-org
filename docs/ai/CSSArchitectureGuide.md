# CSS Architecture Guide — Design Tokens with Tailwind CSS v4

## Overview

This project uses a **two-tier design token** architecture for all CSS color and theming values, built on top of Tailwind CSS v4. This is a well-established pattern used by design systems like Salesforce Lightning, Adobe Spectrum, GitHub Primer, and Shopify Polaris.

The core idea: separate **what a color is** (primitive) from **what a color does** (semantic alias). Components only ever reference aliases, never primitives or raw values.

---

## Token Tiers

### Tier 1 — Primitive Tokens

Primitive tokens are the raw palette values. They describe the color itself and are defined in Tailwind's `@theme` block. These are the single source of truth for every color value in the system.

**Naming convention:** `--bsg-color-{hue}-{shade}`

```css
@theme {
  /* Blue ramp */
  --bsg-color-blue-100: #E6F1FB;
  --bsg-color-blue-200: #B5D4F4;
  --bsg-color-blue-400: #2196F3;
  --bsg-color-blue-600: #1565C0;
  --bsg-color-blue-800: #0C447C;

  /* Navy */
  --bsg-color-navy-900: #0C1A2E;

  /* Green ramp */
  --bsg-color-green-500: #2D7D46;
  --bsg-color-green-600: #236335;
  --bsg-color-green-700: #1A4D28;

  /* Gray ramp */
  --bsg-color-gray-50:  #F8FAFB;
  --bsg-color-gray-100: #EEF2F6;
  --bsg-color-gray-200: #DCE3EB;
  --bsg-color-gray-400: #8FA0B3;
  --bsg-color-gray-500: #3A5A7C;
  --bsg-color-gray-600: #4A5E73;
  --bsg-color-gray-700: #1E3148;

  /* Slate */
  --bsg-color-slate-300: #B8CCE0;

  /* Base */
  --bsg-color-white: #FFFFFF;

  /* Semantic (status) */
  --bsg-color-red-500: #C62828;
  --bsg-color-amber-500: #D4920A;
}
```

**Rules:**

- Primitives hold raw hex values — they are the only place hex values appear.
- Name them by hue and approximate shade, not by role.
- You can add new shades to any ramp as needed.
- Never reference primitives directly in HTML or component classes.

---

### Tier 2 — Semantic Alias Tokens

Alias tokens reference primitives and describe **what role** the color plays. These are defined in `:root` and are the only tokens referenced by component styles.

**Naming convention:** `--bsg-alias-{category}-{role}`

Categories: `bg`, `text`, `action`, `border`, `brand`, `status`

```css
:root {
  /* ── Backgrounds ── */
  --bsg-alias-bg-primary:     var(--bsg-color-navy-900);
  --bsg-alias-bg-surface:     var(--bsg-color-white);
  --bsg-alias-bg-surface-alt: var(--bsg-color-gray-50);
  --bsg-alias-bg-card:        var(--bsg-color-gray-100);
  --bsg-alias-bg-footer:      var(--bsg-color-navy-900);
  --bsg-alias-bg-input:       var(--bsg-color-white);

  /* ── Text ── */
  --bsg-alias-text-heading:     var(--bsg-color-blue-600);
  --bsg-alias-text-body:        var(--bsg-color-gray-500);
  --bsg-alias-text-muted:       var(--bsg-color-gray-400);
  --bsg-alias-text-on-dark:     var(--bsg-color-white);
  --bsg-alias-text-sub-on-dark: var(--bsg-color-slate-300);
  --bsg-alias-text-on-action:   var(--bsg-color-white);

  /* ── Interactive / Actions ── */
  --bsg-alias-action-primary:       var(--bsg-color-green-500);
  --bsg-alias-action-primary-hover: var(--bsg-color-green-600);
  --bsg-alias-action-link:          var(--bsg-color-blue-400);
  --bsg-alias-action-link-hover:    var(--bsg-color-blue-600);

  /* ── Borders ── */
  --bsg-alias-border-default: var(--bsg-color-gray-200);
  --bsg-alias-border-accent:  var(--bsg-color-blue-400);
  --bsg-alias-border-input:   var(--bsg-color-gray-200);
  --bsg-alias-border-focus:   var(--bsg-color-blue-400);

  /* ── Brand ── */
  --bsg-alias-brand-primary: var(--bsg-color-blue-600);
  --bsg-alias-brand-accent:  var(--bsg-color-blue-400);

  /* ── Status ── */
  --bsg-alias-status-success: var(--bsg-color-green-500);
  --bsg-alias-status-error:   var(--bsg-color-red-500);
  --bsg-alias-status-warning: var(--bsg-color-amber-500);
  --bsg-alias-status-info:    var(--bsg-color-blue-400);
}
```

**Rules:**

- Alias values are always `var(--bsg-color-*)` — never raw hex.
- Component classes only use `var(--bsg-alias-*)` — never primitives.
- If you need a new role, add an alias. Don't reference a primitive directly.
- This separation means you can re-theme the entire site by changing which primitives the aliases point to (e.g., dark mode, client white-labeling).

---

## Component Classes with @apply

Use Tailwind's `@apply` directive to compose utility classes into semantic, reusable component classes. Colors always come from alias tokens via `var()`.

```css
/* ── Buttons ── */
.btn-primary {
  @apply px-6 py-3 rounded-lg font-semibold transition-colors;
  background-color: var(--bsg-alias-action-primary);
  color: var(--bsg-alias-text-on-action);
}
.btn-primary:hover {
  background-color: var(--bsg-alias-action-primary-hover);
}

.btn-outline {
  @apply px-6 py-3 rounded-lg font-semibold transition-colors bg-transparent;
  color: var(--bsg-alias-text-on-dark);
  border: 2px solid var(--bsg-alias-text-on-dark);
}
.btn-outline:hover {
  background-color: var(--bsg-alias-text-on-dark);
  color: var(--bsg-alias-bg-primary);
}

/* ── Navigation ── */
.nav-bar {
  @apply sticky top-0 z-50 flex items-center justify-between px-6 py-4;
  background-color: var(--bsg-alias-bg-primary);
}
.nav-link {
  @apply text-sm font-medium tracking-wide transition-colors;
  color: var(--bsg-alias-text-sub-on-dark);
}
.nav-link:hover {
  color: var(--bsg-alias-text-on-dark);
}

/* ── Sections ── */
.section-dark {
  @apply py-16 px-4;
  background-color: var(--bsg-alias-bg-primary);
  color: var(--bsg-alias-text-on-dark);
}
.section-light {
  @apply py-16 px-4;
  background-color: var(--bsg-alias-bg-surface-alt);
}

/* ── Cards ── */
.service-card {
  @apply rounded-xl p-8 shadow-sm transition-shadow hover:shadow-md;
  background-color: var(--bsg-alias-bg-card);
  border: 1px solid var(--bsg-alias-border-default);
}

/* ── Typography ── */
.heading-primary {
  @apply text-4xl font-bold mb-4;
  color: var(--bsg-alias-text-on-dark);
}
.heading-section {
  @apply text-3xl font-bold mb-4;
  color: var(--bsg-alias-text-heading);
}
.body-text {
  @apply text-base leading-relaxed;
  color: var(--bsg-alias-text-body);
}

/* ── Forms ── */
.form-card {
  @apply rounded-xl p-8 shadow-md max-w-2xl mx-auto;
  background-color: var(--bsg-alias-bg-surface);
  border: 1px solid var(--bsg-alias-border-default);
}
.form-label {
  @apply block text-sm font-medium mb-1;
  color: var(--bsg-alias-text-body);
}
.form-input {
  @apply w-full px-4 py-2 rounded-lg transition-colors;
  background-color: var(--bsg-alias-bg-input);
  border: 1px solid var(--bsg-alias-border-input);
  color: var(--bsg-alias-text-body);
}
.form-input:focus {
  @apply outline-none ring-2;
  border-color: var(--bsg-alias-border-focus);
  ring-color: var(--bsg-alias-border-focus);
}

/* ── Prose / Long-form Content ── */
.content-wrapper {
  @apply max-w-3xl mx-auto px-6 py-12;
}
.prose-heading {
  @apply text-xl font-bold mt-8 mb-3;
  color: var(--bsg-alias-text-heading);
}
.prose-body {
  @apply text-base leading-relaxed mb-4;
  color: var(--bsg-alias-text-body);
}

/* ── Links ── */
.link {
  @apply underline transition-colors;
  color: var(--bsg-alias-action-link);
}
.link:hover {
  color: var(--bsg-alias-action-link-hover);
}

/* ── Footer ── */
.footer {
  @apply py-12 px-6;
  background-color: var(--bsg-alias-bg-footer);
  color: var(--bsg-alias-text-sub-on-dark);
}
.footer-link {
  @apply text-sm transition-colors;
  color: var(--bsg-alias-text-sub-on-dark);
}
.footer-link:hover {
  color: var(--bsg-alias-text-on-dark);
}
```

---

## Key Principles

1. **Hex values appear exactly once** — in the primitive token definitions.
2. **Alias tokens are the API** — components consume aliases, not primitives.
3. **@apply for structure, var() for color** — Tailwind utilities handle spacing, layout, and typography; CSS custom properties handle all color.
4. **Semantic naming over visual naming** — use `--bsg-alias-text-heading` not `--bsg-alias-text-blue`. The heading might not always be blue.
5. **Adding new tokens** — if a component needs a color that doesn't have an alias, add a new alias that points to an existing primitive. If no suitable primitive exists, add the primitive first.

---

## File Organization (suggested)

```
wwwroot/css/
├── tokens/
│   ├── primitives.css    /* @theme block with raw color values */
│   └── aliases.css       /* :root block with semantic alias tokens */
├── components/
│   ├── buttons.css       /* .btn-primary, .btn-outline */
│   ├── navigation.css    /* .nav-bar, .nav-link */
│   ├── cards.css         /* .service-card, .form-card */
│   ├── forms.css         /* .form-input, .form-label */
│   ├── typography.css    /* .heading-*, .body-text, .prose-* */
│   └── layout.css        /* .section-dark, .section-light, .footer */
└── site.css              /* imports all of the above */
```

---

## Dark Mode (future)

The alias architecture makes dark mode trivial — override aliases in a media query or class, pointing them to different primitives:

```css
@media (prefers-color-scheme: dark) {
  :root {
    --bsg-alias-bg-surface:     var(--bsg-color-gray-700);
    --bsg-alias-bg-surface-alt: var(--bsg-color-navy-900);
    --bsg-alias-bg-card:        var(--bsg-color-gray-600);
    --bsg-alias-text-heading:   var(--bsg-color-blue-200);
    --bsg-alias-text-body:      var(--bsg-color-slate-300);
    --bsg-alias-border-default: var(--bsg-color-gray-600);
  }
}
```

No component classes change. Only the alias → primitive mappings shift.