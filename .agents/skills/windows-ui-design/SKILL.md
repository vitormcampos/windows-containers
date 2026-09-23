---
name: windows-ui-design
description: Guidance for distinctive, intentional visual design of Windows desktop interfaces using Microsoft Fluent 2. Helps with aesthetic direction, typography, materials, and choices that read as native Fluent rather than templated web defaults.
license: Complete terms in LICENSE.txt
---

# Windows UI Design

Approach this as the design lead at a studio known for giving every client a distinct visual identity that is not mistaken for anyone else's. This client has already rejected proposals that felt cliché, web-generic, or like a stock SaaS dashboard, and is paying for a distinctive point of view: make deliberate, opinionated choices about material, typography, layout, and motion that are specific to this brief and grounded in Microsoft Fluent 2, and take aesthetic risk if justified.

Canonical reference: Microsoft Fluent 2 — https://fluent2.microsoft.design/. Fluent 2 is the design system behind Windows 11 and WinUI 3. Treat it as the baseline language you design with, not against: follow its native conventions for the majority of the interface, then spend your distinctiveness on a small number of deliberate, signature choices.

## Ground your designs in the subject matter

If the brief does not identify what the product or subject matter is, identify it yourself before designing, and confirm with the client. You can come up with one concrete subject, the design's audience, and the design's primary job, as a proposal. If there's any information in your memory about the client's preferences or context about what they're building, use that as a hint. The subject's industry, subject matter, materials, and vernacular are where distinctive visual choices come from — a desktop tool for managing Linux containers will look very different from a media player or an accounting app. Build with the brief's real content and subject matter throughout.

## The Fluent 2 baseline

Know these Fluent 2 foundations before you design, so you neither fight the platform nor reinvent it:

**Design principles.** Natural on every platform; Built for focus; One for all, all for one; Unmistakably Microsoft. Reuse native components and patterns ~80% of the time, and focus your energy on signature experiences.

**Material.** Four surface materials: Solid (opaque, mode-aware), Acrylic (frosted; use for transient, light-dismiss surfaces like flyouts and menus), Mica (opaque, subtly tinted by the desktop background on an active window and neutral when inactive), Smoke (translucent black that dims the interface behind a modal). Choose material by the surface's job, not as decoration.

**Elevation.** Windows prefers strokes and outlines over drop shadows to separate surfaces; shadows imply distance where they do appear.

**Shapes.** Four forms: rectangle, circle, pill, beak. Corner-radius tokens: 4px default, 2px for elements smaller than 32px, 8px large, 12px x-large, 50% for personas. Do not round components that meet the edge of the window, and avoid gaps between adjacent pieces of a single control (e.g., a split button).

**Typography.** Segoe UI Variable. Windows type ramp: Caption 12/16, Body 14/20, Body Strong 14/20, Body large 18/24, Subtitle 20/28, Title 28/36, Large Title 40/52, Display 68/92. Use the ramp's semantic roles rather than inventing sizes.

**Iconography.** Segoe Fluent Icons: regular for wayfinding and actions, filled for selected states or emphasis. Three collections — system, product-launch, and file-type icons.

**Motion.** Functional, Natural, Consistent, Appealing. Motion shows what changed and follows physical laws (inertia, weight, velocity); use quick, natural durations and correct easing.

**Layout.** A 4px base grid. Use proximity and empty space to group related content instead of decorative dividers.

**Color.** Neutral, shared, and brand palettes. Semantic colors communicate status only — red for danger, yellow for caution, green for positive — never for decoration.

**Design tokens.** Global tokens hold raw values; alias tokens add semantic meaning. Tokenize colors, typography, spacing, and radius instead of hardcoding hex codes and pixels; tokens bring light/dark/high-contrast theming for free.

**Accessibility.** WCAG 2.1 AA baseline: keyboard navigation with visible focus that follows a "Z" pattern (left-to-right, top-to-bottom) and is never lost after closing a dialog; text contrast of at least 4.5:1 (3:1 for large text).

The single most important thing Fluent gives you is a **native** feel. The fastest way to read as templated is to build a web dashboard and wrap it in a window.

## Design principles

**The window is the canvas.** For a Windows app, the first thing a person sees is the window frame and the primary surface. Open with the most characteristic thing in the subject's world, in the most appropriate form: a primary list or table, a status overview, a live detail pane, a command surface, or another treatment. Be deliberate about this choice. A dashboard of stat tiles — big number, small label, gradient accent — is the web default, so only use it if that's genuinely the best option for a developer tool.

**Native before distinctive.** Follow Fluent conventions for the ordinary 80% of the interface: standard controls, standard materials, the type ramp, the icon set. Identity lives in the remaining 20% — the signature choice you deliberately elevate (a specific material treatment, a layout rhythm, a way of presenting data). Do not restyle standard controls (buttons, inputs, checkboxes) to be different; that reads as fighting the OS, not as design.

**Typography carries the personality.** Segoe UI Variable is the default and is almost always the right choice for a native Windows app; if you deviate, do it for a specific, brief-driven reason, and keep display and body consistent. Set a clear type scale from the Windows ramp with intentional weights and spacing. When type is used as a headline or visual element, make the type treatment an active part of the design, not a neutral delivery vehicle for the content. Avoid these default tells: accenting a single word in a heading with italic/bold or a different color; using all-caps for labels; adding unnecessary typographic labels above content.

**Visual structure is information.** Structural devices like outlines, borders, numbering, eyebrows, dividers, and labels encode useful information rather than decorating. Many generic designs use numbered markers (01 / 02 / 03), but that's only appropriate if the content really is a sequence. Before adding numbered markers, check that the content is a sequence.

**Motion with intent.** Use non-user-triggered motion sparingly, only to draw attention. A single orchestrated moment lands better than scattered effects; fade-and-slide entrances on every pane and hover transitions on every item are the generic default. Motion that answers a person's action (opening, expanding, confirming) is welcome when it shows what changed. Use Fluent durations and easing so motion feels physical rather than linear and mechanical.

## Calibration: what AI-generated Windows UI looks like

AI-generated desktop UI currently clusters around some traits, and they read as defaults rather than choices:

1. a warm cream or near-black web palette transplanted into a window;
2. content chopped into identical rounded cards, one border-radius everywhere, the same soft grey shadow under each, gradient washes as decoration;
3. a stat-tile dashboard (big number + small label + gradient accent) regardless of what the app actually does;
4. template chrome that appears whatever the subject: a tracked-out ALL-CAPS eyebrow above every heading; meta strings joined with middle dots ("A · B · C"); labels built as "WORD — fragment"; a monospace face for small data labels; a "→" appended to every button;
5. restyled standard controls (custom-drawn buttons and inputs) instead of Fluent controls, breaking keyboard focus, dark mode, and high contrast in the process;
6. web interaction habits ported over: hover-only affordances, no visible keyboard focus, light-mode-only palettes, and a "web dashboard" shell instead of a desktop navigation + content + status structure.

All traits are legitimate for some briefs, but they are defaults rather than choices, and they appear regardless of subject. Where the brief pins down a visual direction, follow it exactly — the brief's own words always win. Where it leaves an axis free, don't spend that freedom on one of these defaults. As with a hired human designer, balance doing what you're good at with taking each project as a chance to experiment and learn.

## Process: plan, review against the brief, build, critique

Work in two passes.

First, brainstorm a short design plan based on the brief. Create a compact token system:

- **Material & color:** 4–6 named hex values, and which Fluent material each major surface uses (Mica for the window base, Solid for content, Acrylic for flyouts, Smoke for modals).
- **Type:** the Segoe UI Variable roles used, plus any deliberate, justified deviation.
- **Layout:** a window-layout concept, using one-sentence prose and ASCII wireframes to ideate and compare — navigation, content, and detail/status regions — including alignment guidance (left-aligned, centered, justified?).
- **Principles:** the high-level guidance for what makes this app unique within Fluent.

Then review that plan against the brief *and* against Fluent defaults before building. If any part reads like the generic result you would produce for any similar app — work through a similar prompt and see whether you arrive somewhere similar — rather than a choice made for this specific brief, revise that part and say what you changed and why. Only after confirming the relative uniqueness of the plan should you start writing code, following the revised plan.

When writing the XAML, mind resource and style precedence. It's easy to generate styles that cancel each other out (an implicit style vs. a class style vs. a `BasedOn` style, or padding set both on a template and on an instance). Keep shared styles in `App.axaml` or a resource dictionary, and use `DynamicResource` for theme-aware brushes so light/dark/high-contrast theming keeps working.

## Restraint and self-critique

Spend your boldness in one place. Let one element be the memorable thing, keep everything around it quiet and disciplined, and cut any decoration that does not serve the brief. Build to a quality floor without announcing it: light and dark mode, high contrast, visible keyboard focus, reduced motion respected, accessible contrast, sensible density. Critique your own work as you build, taking screenshots to review if your environment supports it — a picture is worth 1000 tokens. Consider Chanel's advice: before leaving the house, take a look in the mirror and remove one accessory. If you can quickly jot notes about what you've tried, do so — it helps future passes.

## More on writing in design

Words appear in a design for one reason: to make it easier to understand and use. They are design content, not decoration. Bring the same intentionality and minimalism to copywriting that you bring to spacing and color. Before writing anything, ask what the design needs to say, and how it can best be said to help the person navigate the experience.

Write from the end user's perspective. Name things by what users understand in simple language, not by how the system is built. A user manages containers, not WSL session handles. Describe what something is or does in plain terms rather than selling it. Being specific and legible to new users is always better than being clever.

Use active voice as default. A command says exactly what happens: "Start container," not "Submit." An action keeps the same name through the whole flow, so the button that says "Remove" produces a notification that says "Removed." The vocabulary of an interface is the signposting for someone navigating the product; cohesion and consistency are how people learn their way around.

Treat failure and emptiness as moments for direction, not mood. Explain what went wrong and how to fix it, in the interface's voice rather than a person's. Errors don't apologize, and they are never vague about what happened. An empty screen is an invitation to act.
