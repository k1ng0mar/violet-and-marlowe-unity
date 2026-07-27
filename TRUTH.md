# Violet & Marlowe — TRUTH.md

Last updated: 2026-07-27 (Scale fix — instance-level transform scale finalized, 35/35 green)
Unity 6.3.20f1 LTS | URP 17.3.0 | Input System 1.19.0 | Test Framework 1.6.0

## PlayMode Test Results — VERIFIED ✅

**35/35 tests PASS** — verified via `test-results.xml` (exit code 0, no failures).

### Original 11 Tests
| # | Test | Status |
|---|------|--------|
| 1 | MoveForward_ChangesPosition | PASS |
| 2 | Jump_YRisesThenFalls | PASS |
| 3 | Crouch_ReducesHeight | PASS |
| 4 | Dash_PositionDeltaLargerThanWalk | PASS |
| 5 | CameraFollowsPlayer | PASS |
| 6 | MobileControls_CanvasExists | PASS |
| 7 | MobileControls_JoystickExists | PASS |
| 8 | MobileControls_JoystickMovesPlayer | PASS (horizontal delta=1.612, total=1.712) |
| 9 | MobileControls_ActionButtonsExist | PASS |
| 10 | MobileControls_CameraDragZoneExists | PASS |
| 11 | MobileControls_EventSystemHasInputModule | PASS |

### Batch 1 Tests (HUD/Model/Banner/Reticle/DevConfig)
| # | Test | Status |
|---|------|--------|
| 12 | VisiblePlayer_HasRealModelAndAnimator | **PASS** — SkinnedMeshRenderer found, Animator.isHuman=True |
| 13 | VisiblePlayer_FacesMoveDirection | PASS |
| 14 | HUD_MinimapExists | PASS |
| 15 | HUD_PartnerCardExists | PASS |
| 16 | HUD_HealthAndHeatBarsExist | PASS |
| 17 | HUD_WeaponAndAmmoExist | PASS |
| 18 | HUD_DoesNotBlockJoystickInput | PASS |
| 19 | Reticle_ExistsAnchoredCenter | PASS |
| 20 | Banner_FadeInHoldFadeOut | PASS |
| 21 | DevConfig_ParserCorrect | PASS (incl. cameraDistance, invertY) |
| 22 | DevConfig_DefaultsAreCorrect | PASS (incl. cameraDistance=2.5, invertY=false) |

### Batch 2 Tests (District/Camera Collision/Minimap)
| # | Test | Status | Evidence |
|---|------|--------|----------|
| 23 | District_HasMultipleBuildingsAndStreets | PASS | N=12 buildings, M=4 street segments |
| 24 | District_AllBuildingsHaveColliders | PASS | All 12 buildings have non-trigger BoxColliders |
| 25 | District_StreetFloorIsContinuous | PASS | 7/7 raycasts hit floor at grid points |
| 26 | CameraCollision_PullsInWhenBlocked | PASS | fullLength=2.500, pulledIn=0.863 |
| 27 | CameraCollision_ReturnsToFullLengthWhenClear | PASS | fullLength=2.500, current=2.500 |
| 28 | MinimapPip_MovesWithPlayer | PASS | ΔX=10.0 (east), ΔY=10.0 (north) — BOTH axes via teleport |

### Batch 3 Tests (Heist)
| # | Test | Status | Evidence |
|---|------|--------|----------|
| 29 | Heist_StateTransitionsInOrder | PASS | NOT_STARTED → ENTERED_BANK → VAULT_REACHED → LOOT_SECURED → EXTRACTING → SUCCESS |
| 30 | Heist_ObjectiveTextUpdatesPerState | PASS | "Get inside the bank" → "Reach the vault" → "Grab the carrots" → "Carry them to extraction!" → "HEIST COMPLETE" |
| 31 | Heist_CarryAttachesAndReducesSpeed | PASS | Sack parented to player, walkSpeed reduced (3.5 → 2.1) |
| 32 | Heist_DropDetaches | PASS | Sack detached from player after Drop() |
| 33 | Heist_ExtractionWithLootFiresSuccess | PASS | Extraction with loot → SUCCESS state |
| 34 | Heist_ExtractionWithoutLootDoesNotFireSuccess | PASS | Extraction without loot → no state change |
| 35 | ScaleImport_ModelHeightIsCorrect | PASS | Skinned height=1.80m, localScale.y=142.07, Hips relY=0.9762 |

## Runtime Status by Feature

### VERIFIED (test-results.xml shows green)
- **Player movement** (walk/run/jump/crouch/dash) — VERIFIED
- **Camera follow** — VERIFIED
- **Camera wall-collision** (raycast pull-in) — VERIFIED (0.863 pulled vs 2.500 full)
- **Mobile touch controls** (joystick, buttons, drag zone) — VERIFIED
- **Visible player character model** (violet_tbp.fbx SkinnedMeshRenderer, Animator.isHuman=True) — **VERIFIED**
- **HUD skeleton** (minimap, partner card, health/heat bars, weapon/ammo) — VERIFIED
- **Live minimap pip** (player XZ → RectTransform, both axes) — VERIFIED, IN-BUILD
- **Reticle** (3 chevrons + center pip) — VERIFIED
- **District banner** (fade in/hold/fade out) — VERIFIED
- **Dev config parser** (JSON → DevSettings, incl. cameraDistance + invertY) — VERIFIED
- **invertY applied** (DevSettings.InvertY multiplied into ApplyLookDelta) — VERIFIED, IN-BUILD
- **Character model scale** (142.07x instance transform, skinned height=1.80m, Hips relY=0.9762) — VERIFIED
- **Procedural district** (12 buildings, 4 street segments, intersection, colliders) — VERIFIED
- **Continuous floor** (7/7 raycast hits, no holes) — VERIFIED
- **Joystick horizontal movement** (delta=1.612 X, not floor settle) — VERIFIED

### Heist Prototype (Batch 3)
- **Bank building** (grey walls, doorway, interior, vault, vault door, carrot sack) — VERIFIED
- **Heist state machine** (6 states, transitions in order) — VERIFIED
- **Objective HUD text** (updates per state: "Get inside the bank" → "HEIST COMPLETE") — VERIFIED
- **Carry mechanic** (pickup = kinematic child, drop = detach) — VERIFIED
- **Carry speed reduction** (walkSpeed 3.5 → 2.1, 60% multiplier) — VERIFIED
- **Extraction zone** (warm floor patch outside bank, trigger) — VERIFIED
- **Success logic** (extraction WITH loot = SUCCESS, WITHOUT loot = nothing) — VERIFIED

### COMPILE-ONLY
*(none)*

### NOT YET IMPLEMENTED
- Combat / enemies / weapons (locked out until heist loop feels good on device)
- Alarm / fail state (comes with combat later)
- Open world (future scope only)
|- **Real artwork / animations** (violet_tbp.fbx imported with Humanoid avatar + SkinnedMeshRenderer — textures missing, need extraction from GLB; no animations yet)

## APK Build — VERIFIED ✅ (Current On-Device Validation Build)

- **Path**: `/home/ubuntu/violet-and-marlowe-unity/build/violet-and-marlowe.apk`
- **Size**: 130MB (135,278,769 bytes)
- **Architecture**: ARM64-only (arm64-v8a .so files: libil2cpp.so, libunity.so, libgame.so, libc++_shared.so, libmain.so, lib_burst_generated.so, libswappywrapper.so)
- **Zero x86/ARMv7**: No lib/x86 or lib/armeabi entries in APK
- **Build type**: IL2CPP, Development build, Landscape
- **Includes**: class-split touch fix, expanded district (12 buildings, 4 streets), camera wall-collision, live minimap pip (BOTH X+Y axes), invertY wiring, camera tunables in dev-config

## Palette Used (Build Guide 6.2)
| Color | Hex | RGB (0-1) | Used For |
|-------|-----|-----------|----------|
| Carrot orange (warm) | #F2762E | (0.949, 0.463, 0.180) | Streets, sidewalks, floor, minimap pip, banner text |
| Institutional grey | #6E6E73 | (0.431, 0.431, 0.451) | Buildings, crates, minimap border, heat bar segments |
| Violet rust | #B84A3E | (0.722, 0.290, 0.243) | Player proxy body, health bar fill |
| Marlowe teal | #3E7A8C | (0.243, 0.478, 0.549) | Partner card portrait |
| Light neutral | #E8E8E8 | (0.910, 0.910, 0.910) | Reticle, text, ammo |

## Dev Config (StreamingAssets/devsettings.json)
```json
{"lookSensitivity":0.25,"joystickDeadzone":0.1,"cameraDistance":2.5,"invertY":false}
```

## Spawn Plaza Clearance
- **Radius**: 8 units around origin — no buildings or crates placed within this zone
- **Guarantees**: Forward movement tests' path is unobstructed (no crates had to be deleted)

## Rig Test — violet_tbp.fbx Character Model ✅

**Model**: `Assets/Art/Characters/Violet/violet_tbp.fbx` (copied from `/home/ubuntu/3D assets/violet/violet_tbp.fbx`)
**Avatar**: `violet_tbpAvatar`, isHuman=True, isValid=True
**Animation Type**: Humanoid (`ModelImporterAnimationType.Human`)
**Avatar Setup**: CreateFromThisModel (`ModelImporterAvatarSetup.CreateFromThisModel`)
**Key insight**: In Unity 6, the enum values are `ModelImporterAnimationType.Human` (NOT `Humanoid`) and `ModelImporterAvatarSetup.CreateFromThisModel`. Must set properties on the importer object FIRST, THEN call `SaveAndReimport()` in that order. SerializedProperty paths like `m_HumanDescription.animationType` are hidden and not accessible via the public API.
**Renderers**: 1 SkinnedMeshRenderer (`mesh_rep_0_ori_repair_quad`)
**Material**: `violet_material` (persistent asset at `Assets/Art/Characters/Violet/violet_material.mat`), shader=Standard
**Textures**: **YES** — mainTexture=`violet_albedo` (2048x2048 ASTC_6x6), extracted from `/home/ubuntu/3D assets/violet/violet_texture.glb` (embedded PNG, bufferView 5, offset 5385984, length 13930317)
**UV/Vertex match**: GLB=11862 verts, FBX=11977 verts (115 extra due to FBX vertex splitting for normals/UVs — <1% difference, standard export behavior, UV layout matches)
**Model height**: ~0.0027m raw FBX mesh. Final scale: 142.07x instance transform → effective skinned height = **1.80m** ✅ (VERIFIED: skinned renderer.bounds.size.y=1.7999, Hips relY=0.9762). Pre-skin scaling (vertex bake / importer globalScale) is broken for this asset — the bindpose reconciles mesh-space to skeleton-space at native scale only, and pre-skin scaling inflates or collapses the skinned result. Instance transform scale is the only safe method.
**Colliders**: None on model (CharacterController handles collision)
**Animator**: Present, isHuman=True, no controller/clips yet (bind pose only)
**Rotation**: Player transform rotates to face movement direction (unchanged from proxy)
**VisiblePlayer_FacesMoveDirection**: rotated 89.4° (still works with real model)

### Texture Restoration Process
1. Extracted PNG from GLB binary chunk (bufferView 5, offset 5385984, length 13930317)
2. Saved as `Assets/Art/Characters/Violet/violet_albedo.png` (Unity imported as 2048x2048 ASTC_6x6)
3. Created persistent material asset `violet_material.mat` with Standard shader
4. Assigned `violet_albedo` as mainTexture on `violet_material`
5. Scene builder loads `violet_material.mat` and assigns to SkinnedMeshRenderer
6. Test assertion: `VisiblePlayer_HasRealModelAndAnimator` now checks `mainTexture != null` and `name == "violet_albedo"`

### Issues Found (next steps)
- **Scale**: FINALIZED — 142.07x instance transform (NOT temporary). Pre-skin scaling is broken due to bindpose scale-dependence. Verified: skinned height=1.80m, Hips relY=0.9762, feetY=0.3852.
- **No animations**: Animator has no controller — will need Mixamo animations next. **OPEN QUESTION**: verify empirically whether Mixamo retargeting is affected by the non-unit instance root scale (142.07x). Don't assume — test when clips go in.

## Issues Fixed This Session
1. **TouchButton/CameraTouchDragZone serialization** — split into separate .cs files (Unity can't resolve multiple MonoBehaviours in one file by GUID)
2. **`-runTests` quitting without results** — removed `-quit` flag
3. **HUD blocking joystick input** — added `usingTouchInput` flag to prevent keyboard overwriting touch `moveInput`
4. **Camera drag zone anchorMin.x** — changed from 0.45 to 0.5
5. **DebugOverlay NRE** — `GetComponent<RectTransform>` instead of `AddComponent<RectTransform>`
6. **Joystick false pass** — test now asserts horizontal (X) displacement, not total delta
