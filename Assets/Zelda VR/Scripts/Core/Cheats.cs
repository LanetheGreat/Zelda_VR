﻿using Immersio.Utility;
using UnityEngine;
using System.Collections.Generic;

public class Cheats : Singleton<Cheats>
{
    public Collectible forceDroppedItem;        // Forces a specific Collectible to always drop

    public bool cheatingAllowed = false;

    public bool GhostModeIsEnabled { get; private set; }
    public bool FlyingIsEnabled { get; private set; }
    public bool SecretDetectionModeIsEnabled { get; private set; }
    public bool InvincibilityIsEnabled { get; private set; }


    float _maxRunMultiplier = 4;
    int _maxJumpHeight = 8;


    List<KeyCode> _keypadKeys = new List<KeyCode>() {
        KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4,
        KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9
    };
    int GetNumberForKeypadKey(KeyCode key)
    {
        return _keypadKeys.IndexOf(key);
    }


    void Update()
    {
        if (cheatingAllowed)
        {
            ProcessInput();

            //CycleTriforcePieces();
        }
    }

    void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { EquipSword("WoodenSword"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { EquipSword("WhiteSword"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { EquipSword("MagicSword"); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { EquipSword(null); }

        if (ZeldaInput.GetCommand_Trigger(ZeldaInput.Cmd_Trigger.ToggleGodMode)) { ToggleGodMode(); }
        if (ZeldaInput.GetCommand_Trigger(ZeldaInput.Cmd_Trigger.ToggleGhostMode)) { ToggleGhostMode(); }
        if (ZeldaInput.GetCommand_Trigger(ZeldaInput.Cmd_Trigger.ToggleFlying)) { ToggleFlying(); }

        ProcessKeypadInput();
    }
    void ProcessKeypadInput()
    {
        int d = -1;
        foreach (KeyCode key in _keypadKeys)
        {
            if (Input.GetKeyDown(key))
            {
                d = GetNumberForKeypadKey(key);
                break;
            }
        }
        if (d == -1)
        {
            return;
        }

        if (d == 0)
        {
            WarpToTriforceDungeonRoom();
        }
        else if (d > 0)
        {
            WarpToOverworldDungeonEntrance(d);
        }
    }


    bool _godModeEnabled;
    public void ToggleGodMode()
    {
        ToggleGodMode(!_godModeEnabled);
    }
    public void ToggleGodMode(bool enable)
    {
        _godModeEnabled = enable;

        ToggleInvincibility(_godModeEnabled);
        ToggleAirJumping(_godModeEnabled);
        //ToggleMoonMode(_godModeEnabled);
        //ToggleGhostMode(_godModeEnabled);
        SetRunMultiplier(_godModeEnabled ? _maxRunMultiplier : 1);
        SetJumpHeight(_godModeEnabled ? _maxJumpHeight : 0);

        if (_godModeEnabled)
        {
            MaxOutInventory();
            RestorePlayerHealth();
        }
    }


    public void MaxOutInventory()
    {
        Inventory.Instance.MaxOutEverything();
        MaxOutRupees();
    }

    public void ToggleInvincibility()
    {
        ToggleInvincibility(!InvincibilityIsEnabled);
    }
    public void ToggleInvincibility(bool enable)
    {
        InvincibilityIsEnabled = enable;
        if (CommonObjects.Player_C != null)
        {
            CommonObjects.Player_C.IsInvincible = enable;
        }
    }

    public void ToggleGhostMode()
    {
        ToggleGhostMode(!GhostModeIsEnabled);
    }
    public void ToggleGhostMode(bool enable)
    {
        GhostModeIsEnabled = enable;

        int playerLayer = CommonObjects.Player_G.layer;
        int wallLayer = LayerMask.NameToLayer(ZeldaLayers.WALLS);
        int blocksLayer = LayerMask.NameToLayer(ZeldaLayers.BLOCKS);
        int invisibleBlocksLayer = LayerMask.NameToLayer(ZeldaLayers.INVISIBLE_BLOCKS);

        Physics.IgnoreLayerCollision(playerLayer, wallLayer, GhostModeIsEnabled);
        Physics.IgnoreLayerCollision(playerLayer, blocksLayer, GhostModeIsEnabled);
        Physics.IgnoreLayerCollision(playerLayer, invisibleBlocksLayer, GhostModeIsEnabled);

        UpdateGroundPlaneCollision();
    }

    public void ToggleAirJumping()
    {
        ToggleAirJumping(!CommonObjects.Player_C.IsAirJumpingEnabled);
    }
    public void ToggleAirJumping(bool enable)
    {
        CommonObjects.Player_C.IsAirJumpingEnabled = enable;
    }

    public void ToggleFlying()
    {
        ToggleFlying(!CommonObjects.Player_C.IsFlyingEnabled);
    }
    public void ToggleFlying(bool enable)
    {
        FlyingIsEnabled = enable;

        CommonObjects.Player_C.IsFlyingEnabled = enable;

        UpdateGroundPlaneCollision();
    }

    public void ToggleMoonMode()
    {
        ToggleMoonMode(!CommonObjects.Player_C.IsMoonModeEnabled);
    }
    public void ToggleMoonMode(bool enable)
    {
        CommonObjects.Player_C.IsMoonModeEnabled = enable;
    }

    public void ToggleSecretDetectionMode()
    {
        ToggleSecretDetectionMode(!SecretDetectionModeIsEnabled);
    }
    public void ToggleSecretDetectionMode(bool enable)
    {
        SecretDetectionModeIsEnabled = enable;

        WorldInfo world = WorldInfo.Instance;
        if (world.IsOverworld)
        {
            TileMap tileMap = CommonObjects.OverworldTileMap;
            if (tileMap != null) { tileMap.HighlightAllSpecialBlocks(enable); }
        }
        else if (world.IsInDungeon)
        {
            // TODO
        }
    }

    
    public void IncreaseRunMultiplier()
    {
        float newMultipler = CommonObjects.PlayerController_C.RunMultiplier * 2;
        if (newMultipler > _maxRunMultiplier) { newMultipler = 1; }

        SetRunMultiplier(newMultipler);
    }
    public void SetRunMultiplier(float rm)
    {
        CommonObjects.PlayerController_C.RunMultiplier = rm;
    }

    public void IncreaseJumpHeight()
    {
        int newHeight = CommonObjects.Player_C.JumpHeight;
        newHeight = (newHeight == 0) ? 1 : newHeight * 2;

        if (newHeight > _maxJumpHeight) { newHeight = 0; }

        SetJumpHeight(newHeight);
    }
    public void SetJumpHeight(int h)
    {
        CommonObjects.Player_C.JumpHeight = h;
    }

    public void MaxOutRupees()
    {
        SetRupeeCount(Inventory.Instance.GetItem("Rupee").maxCount);
    }
    public void SetRupeeCount(int r)
    {
        Inventory.Instance.RupeeCount = r;
        GameplayHUDViewController.Instance.ForceRupeeCountTextToAmount(r);
    }


    public void ReturnToGroundLevel()
    {
        CommonObjects.Player_C.ReturnToGroundLevel();
    }

    public void RestorePlayerHealth()
    {
        CommonObjects.Player_C.HealthController.RestoreHealth();
    }
    public void DamagePlayer()
    {
        CommonObjects.Player_C.HealthController.TakeDamage(4, gameObject);
    }
    public void KillPlayer()
    {
        CommonObjects.Player_C.HealthController.Kill(gameObject, true);
    }

    public void EquipSword(string swordName)
    {
        CommonObjects.Player_C.Inventory.EquipSword_Cheat(swordName);
    }

    public void WarpToOverworldDungeonEntrance(int dungeonNum)
    {
        Locations.Instance.WarpToOverworldDungeonEntrance(dungeonNum);
    }

    public void WarpToTriforceDungeonRoom()
    {
        if (!WorldInfo.Instance.IsInDungeon) { return; }

        DungeonRoom dr = CommonObjects.CurrentDungeonFactory.GetRoomContainingTriforce();
        if (dr == null) { return; }
        
        Vector3 p = dr.Center;
        CommonObjects.Player_C.PositionXZ = new Vector2(p.x, p.z - 3);
    }


    int _dungeonNum = 1;
    int _count = 0;
    public void CycleTriforcePieces()
    {
        Inventory inv = Inventory.Instance;

        if (++_count > 30)
        {
            _count = 0;
            if (_dungeonNum > 8)
            {
                _dungeonNum = 0;
                for (int i = 1; i <= 8; i++)
                {
                    inv.SetHasTriforcePieceForDungeon(i, false);
                }
            }
            else
            {
                inv.SetHasTriforcePieceForDungeon(_dungeonNum, true);
            }
            _dungeonNum++;
        }
    }


    void UpdateGroundPlaneCollision()
    {
        OverworldTerrainEngine engine = OverworldTerrainEngine.Instance;
        if (engine != null)
        {
            engine.GroundPlaneCollisionEnabled = GhostModeIsEnabled && !FlyingIsEnabled;
        }
    }
}