# Violet & Marlowe — TEST_BACKLOG.md

Running list of everything needing ON-DEVICE validation when the phone returns.
Updated as features are built. Do not lose items.

Last updated: 2026-07-24

---

## Core Movement & Camera
- [ ] Touch stick feel — does the virtual joystick respond smoothly? Deadzone comfortable?
- [ ] Touch buttons (Jump/Crouch/Dash) — tap responsiveness, no missed presses
- [ ] Camera swipe sensitivity — 0.25°/px too fast/slow? Needs tuning on real screen?
- [ ] Camera-into-walls — does the camera pull-in feel natural when walking past buildings?
- [ ] invertY toggle — does it actually invert vertical look on device? (wired in code, untested on device)

## Minimap
- [ ] Minimap pip up/down visual — does the pip move up when walking north, down when south?
- [ ] Minimap pip left/right — does it move right when walking east?
- [ ] Pip clamping — does the pip stay within the minimap border when walking far?

## District
- [ ] Full district walk — can you walk all 4 street segments without getting stuck?
- [ ] Spawn plaza clearance — is the spawn area clear of obstacles?
- [ ] Building colliders — can you walk into building walls and get blocked?
- [ ] Crate clusters — are crates visible and do they block movement?

## HUD
- [ ] Objective text readability — is "Get inside the bank" visible and readable?
- [ ] Reticle visibility — are the 3 chevrons + center pip visible against district backgrounds?
- [ ] District banner — does "DISTRICT 1" fade in/hold/fade out correctly on device?
- [ ] Debug overlay — is the debug strip readable at the top edge?
- [ ] Health/heat bars — are they visible and positioned correctly?
- [ ] Partner card — does the MARLOWE card show with teal portrait ring?

## Bank Heist
- [ ] Bank building visible — grey walls, doorway, vault room all render correctly?
- [ ] Bank doorway — can you walk through the doorway into the interior?
- [ ] Vault room — is the vault room behind the divider with the vault door?
- [ ] Carrot sack — is the warm-colored sack visible in the vault?
- [ ] Carry slowdown — does movement feel slower when carrying the sack?
- [ ] Carry visual — does the sack appear attached to the player when carried?
- [ ] Objective text transitions — does the objective text change as you progress?
- [ ] Extraction zone — is the warm floor patch visible outside the bank?
- [ ] Extraction success — does "HEIST COMPLETE" banner show when extracting with loot?
- [ ] Extraction without loot — does nothing happen when entering extraction without the sack?

## Dev Config
- [ ] devsettings.json load — does the config load from StreamingAssets on device?
- [ ] Camera distance — does changing cameraDistance in JSON affect camera on device?
- [ ] Joystick deadzone — does changing joystickDeadzone in JSON affect stick on device?
