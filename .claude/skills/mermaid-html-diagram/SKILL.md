---
name: mermaid-html-diagram
description: Generate a self-contained HTML file containing one or more interactive Mermaid diagrams with dark GitHub-style theming, status-based colour coding, and mouse/button pan-and-zoom via svg-pan-zoom.
user-invokable: true
context: inline
---

# Mermaid HTML Diagram

## Purpose

Use this skill to render any Mermaid diagram (flowchart, sequence, ER, state, etc.) as a polished, self-contained HTML file that can be opened directly in a browser. It handles the common pitfalls of embedding Mermaid SVGs in fixed-height containers and wiring up pan-and-zoom correctly.

## Inputs

| Name | Required | Description |
|---|---|---|
| `diagrams` | yes | List of `{ id, title, mermaidSource, height }` objects |
| `outputPath` | yes | Absolute path for the `.html` file |
| `pageTitle` | no | Browser tab title |
| `legend` | no | List of `{ label, fillColour, strokeColour }` items |

## Colour palette — status-based nodes

Use these consistently whenever nodes represent work items with a blocking status:

| Status      | Fill    | Stroke  | Text    | Usage                                    |
|-------------|---------|---------|---------|------------------------------------------|
| DONE        | #2d1f6e | #8957e5 | #d2a8ff | Closed / completed issue                 |
| UNBLOCKED   | #0d3020 | #3fb950 | #7ee787 | Open, all dependencies done — bold text  |
| IN_PROGRESS | #3d2600 | #f0883e | #f0c674 | Partially done, next task ready — bold   |
| BLOCKED     | #2d1010 | #f85149 | #ffa198 | Open with at least one open dependency   |

Subgraph container backgrounds use the same fill/stroke as the group's status but slightly darker.

Add status indicators to node labels:
- DONE: `✅` prefix or suffix
- IN_PROGRESS: `▶` prefix
- BLOCKED: `⛔` prefix

## HTML structure

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <title>{pageTitle}</title>
  <script src="https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.min.js"></script>
  <script src="https://cdn.jsdelivr.net/npm/svg-pan-zoom@3.6.1/dist/svg-pan-zoom.min.js"></script>
  <style>
    /* See Critical CSS section below */
  </style>
</head>
<body>
  <!-- top nav, legend, diagram sections, summary table -->
  <script>
    /* See Critical JS section below */
  </script>
</body>
</html>
```

## Critical CSS

```css
* { box-sizing: border-box; }
body { font-family: system-ui, sans-serif; background: #0d1117; color: #c9d1d9; margin: 0; padding: 20px; }

.mermaid-box {
  width: 100%; overflow: hidden;
  background: #161b22; border: 1px solid #30363d; border-radius: 8px;
  cursor: grab; user-select: none;
  display: block; position: relative;
}
.mermaid-box:active { cursor: grabbing; }
/* Mermaid injects a wrapper div — both it and the SVG must fill the container */
.mermaid-box > .mermaid {
  width: 100% !important; height: 100% !important; display: block !important;
}
.mermaid-box svg {
  display: block !important; width: 100% !important;
  height: 100% !important; max-width: none !important;
}

.diagram-wrap { position: relative; }
.diagram-controls {
  position: absolute; top: 10px; right: 10px; z-index: 10; display: flex; gap: 6px;
}
.diagram-controls button {
  background: rgba(33,38,45,0.92); border: 1px solid #30363d;
  color: #c9d1d9; border-radius: 6px; padding: 5px 12px;
  cursor: pointer; font-size: 0.82rem; backdrop-filter: blur(4px);
}
.diagram-controls button:hover { background: #30363d; border-color: #58a6ff; color: #58a6ff; }
```

## Critical JS — initialisation

Mermaid renders SVGs with **hardcoded pixel `width` and `height` attributes**. If you leave these in place, svg-pan-zoom will compute an incorrect viewport and the diagram will appear cut off or misaligned. The fix sequence is mandatory:

```javascript
mermaid.initialize({ startOnLoad: false, theme: "dark", securityLevel: "loose" });

const pz = {};

document.addEventListener("DOMContentLoaded", async () => {
  await mermaid.run({ querySelector: ".mermaid" });

  const setups = [
    // { key, boxId, inId, outId, rstId }  — one entry per diagram
  ];

  setups.forEach(({ key, boxId, inId, outId, rstId }) => {
    const box = document.getElementById(boxId);
    const svg = box && box.querySelector("svg");
    if (!svg) return;

    // 1. Strip Mermaid's hardcoded pixel dimensions (keep viewBox)
    svg.removeAttribute("width");
    svg.removeAttribute("height");
    svg.style.cssText = "display:block;width:100%;height:100%;max-width:none;";

    // 2. Make the inner .mermaid wrapper div fill the container too
    const inner = svg.closest(".mermaid");
    if (inner) inner.style.cssText = "display:block;width:100%;height:100%;";

    // 3. Initialise — do NOT pass fit/center:true here; call them explicitly after
    pz[key] = svgPanZoom(svg, {
      zoomEnabled: true, controlIconsEnabled: false,
      fit: false, center: false,
      minZoom: 0.04, maxZoom: 25, zoomScaleSensitivity: 0.3,
      mouseWheelZoomEnabled: true, dblClickZoomEnabled: true,
      preventMouseEventsDefault: true,
    });

    // 4. Resize to actual container, then fit and centre
    pz[key].resize();
    pz[key].fit();
    pz[key].center();

    // 5. Wire buttons
    document.getElementById(inId) .addEventListener("click", () => pz[key].zoomIn());
    document.getElementById(outId).addEventListener("click", () => pz[key].zoomOut());
    document.getElementById(rstId).addEventListener("click", () => {
      pz[key].resize(); pz[key].fit(); pz[key].center();
    });
  });

  // 6. Re-fit on window resize
  window.addEventListener("resize", () => {
    Object.values(pz).forEach(p => { try { p.resize(); p.fit(); p.center(); } catch(e){} });
  });
});
```

## Container height guidelines

| Diagram type | Recommended height |
|---|---|
| Simple (< 15 nodes) | `60vh` |
| Medium (15–30 nodes) | `72vh` |
| Large (30+ nodes) | `85vh` |

## Per-diagram section HTML pattern

```html
<div class="diagram-section">
  <h2 id="{id}">{title}</h2>
  <div class="diagram-wrap">
    <div class="diagram-controls">
      <button id="{id}-in">＋</button>
      <button id="{id}-out">－</button>
      <button id="{id}-rst">⟲ Reset</button>
    </div>
    <div class="mermaid-box" id="box-{id}" style="height:{height}">
      <div class="mermaid" id="mmd-{id}">
        {mermaidSource}
      </div>
    </div>
  </div>
</div>
```

## Mermaid init block per diagram

```
%%{init: {"theme": "dark", "flowchart": {"rankDir": "TB", "nodeSpacing": 55, "rankSpacing": 80}}}%%
```

Use `rankDir: TB` (top-to-bottom) for dependency trees and `rankDir: LR` (left-to-right) for wide graphs with many parallel tracks.

## After writing the file

Open it immediately so the result is visible:

```powershell
Start-Process "<outputPath>"
```
