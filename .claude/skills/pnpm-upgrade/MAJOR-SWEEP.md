# Sweeps spanning several majors

## Stack the units

Ship one PR per unit from step 2, ordered safest first: the bulk within-major bump at the bottom, then one major (or peer-locked set of majors) per layer above it. A major needing source changes carries those changes in its own PR, which keeps each layer independently reviewable and revertible.

Build the stack with [gh-stack](../gh-stack/SKILL.md). Insert at the bottom — where a Node floor or other precondition belongs — by unstacking, re-initialising with the new order, then cascade-rebasing.

Run the full step 4 verification on **every** branch, not only the top. Layers rebase onto each other, so a broken lower layer breaks everything above it.

CI runs the frontend job on stacked PRs whose base is another branch, so each layer gets its own real signal.

## Lockfile conflicts

Every layer touches `pnpm-lock.yaml`, so expect a conflict per rebase step. Resolve it by regenerating rather than reading it: take the incoming branch's copy and let pnpm reconcile it against the already-resolved `package.json`.

```bash
git checkout --theirs ui/menu-website/pnpm-lock.yaml
(cd ui/menu-website && pnpm install)
git add ui/menu-website/pnpm-lock.yaml
```

`package.json` conflicts need hands: keep the layer's own bumps *and* whatever an earlier layer introduced on adjacent lines. A bulk bump changing the devDependencies block will collide with a single-line change like `@types/node` sitting inside it.

## Confirm the restructure moved history, not results

A clean rebase reports nothing about correctness. Two checks settle it — the second is the **invariant**: reordering PRs may move a change between commits, and must leave the end state untouched.

```bash
# the change survived every layer, not just the top
for b in <branches>; do echo -n "$b "; git show "$b:ui/menu-website/package.json" | grep '"@types/node"'; done

# the end state is unchanged by the restructure
git diff <old-top-sha> <new-top-branch>
```

An empty second diff means only history moved. Anything else means the rebase changed the result.

Read the per-layer output as a progression: each layer should show its own bump arriving and every earlier layer's still in place.

## Node targeting

`engines.node` and `@types/node` both answer which Node line the project targets, so move them together, at the bottom of the stack — the floor is declared before anything depends on it.

Derive `engines.node` from what the packages declare rather than guessing. Run this from `ui/menu-website` with the final versions installed:

```bash
node -e "
const fs=require('fs'),path=require('path');
const pkg=JSON.parse(fs.readFileSync('package.json','utf8'));
for(const n of [...Object.keys(pkg.dependencies||{}),...Object.keys(pkg.devDependencies||{})].sort()){
  try{const j=JSON.parse(fs.readFileSync(path.join('node_modules',...n.split('/'),'package.json'),'utf8'));
  if(j.engines&&j.engines.node)console.log(n.padEnd(32),j.engines.node);}catch(e){}
}"
```

Take the intersection. Watch for packages restricting themselves to even-numbered LTS majors — `@quasar/app-vite@3` declares `^30 || ^28 || ^26 || ^24 || ^22.22.0`, which excludes odd majors like 25 and 27 from a strict reading.

`@types/node` tracks Node release lines 1:1, so its major matches a line `engines.node` actually supports. Moving it from 25.x to 24.x reads as a downgrade and is the correct direction when 24 is the LTS line being targeted, because 25.x types the non-LTS Current line. Say so in the commit message, or it gets reverted as a mistake.

CI pins `node-version: 22.x` in `.github/workflows/main.yml`. Read the resolved version out of a job log before assuming a raised floor is safe. pnpm leaves `engine-strict` unset, so a mismatch warns rather than fails.
