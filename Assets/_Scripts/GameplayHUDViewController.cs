﻿using UnityEngine;
using Immersio.Utility;


public class GameplayHUDViewController : Singleton<GameplayHUDViewController>
{
    [SerializeField]
    GameplayHUDView _view;
    Inventory _inventory;


    public int vertShiftSpeed = 600;


    float _yBaseOffset = 300;
    public int PausedYVal           // Where to vertically position HUD when game is Paused
    {
        get { return (int)(Screen.height * 0.7f + _yBaseOffset); }    
    }


    void Awake()
    {
        _inventory = Inventory.Instance;
    }


    void Update()
    {
        UpdateView();
    }


    void UpdateView()
    {
        UpdateView_EquippedItemSlots();
        //GUIShowHearts();
        //GUIShowItemCounts();

        /*if (WorldInfo.Instance.IsOverworld)
        {
            GUIShowOverworldMap();
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            GUIShowDungeonMap();

            string text = "LEVEL-" + WorldInfo.Instance.DungeonNum.ToString();
            StereoDrawZeldaFontLabel(new Rect(20, 3, 56, 6), ref text, Color.white);
        }*/
    }

    void UpdateView_EquippedItemSlots()
    {
        Item itemA = _inventory.EquippedItemA;
        Texture texture = (itemA == null) ? null : itemA.GetGuiTexture();
        _view.SetTextureForEquippedItemSlotA(texture);

        Item ItemB = _inventory.EquippedItemB;
        texture = (ItemB == null) ? null : ItemB.GetGuiTexture();
        _view.SetTextureForEquippedItemSlotB(texture);
    }

    /*void GUIShowHearts()
    {
        Item heartContainer = Inventory.Instance.GetItem("HeartContainer");

        int halfHearts = CommonObjects.Player_C.HealthInHalfHearts;
        int fullHearts = (int)(halfHearts * 0.5f);
        bool plusHalfHeart = halfHearts % 2 == 1;
        int maxHearts = heartContainer.count;

        int xMin = 0; //(int)(heartContainer.guiInventoryPosition.x);
        int yMin = 0; //(int)(heartContainer.guiInventoryPosition.y);

        const int MaxHeartsPerRow = 8;
        const int vPad = 0;
        const int hPad = 1;

        int w = _fullHeartImage.width;
        int h = _fullHeartImage.height;
        int x = xMin;
        int y = yMin + h + vPad;     // (we begin on bottom row)

        Texture heartTexture;
        for (int i = 0; i < maxHearts; i++)
        {
            if (i < fullHearts)
            {
                heartTexture = _fullHeartImage;
            }
            else if (i == fullHearts && plusHalfHeart)
            {
                heartTexture = _halfHeartImage;
            }
            else
            {
                heartTexture = _emptyHeartImage;
            }

            //StereoDrawTexture(new Vector2(x, y), ref heartTexture, color);

            x += w + hPad;
            if (i == MaxHeartsPerRow - 1)
            {
                x = xMin;
                y = yMin;    // (move up to first row)
            }
        }
    }*/

    void GUIShowItemCounts()
    {
        int x = 98;
        int w = 18;
        int h = 6;

        Inventory inv = Inventory.Instance;

        // Rupees
        int rupees = inv.GetItem("Rupee").count;
        string text = "x" + rupees.ToString();
        //StereoDrawZeldaFontLabel(new Rect(x, 14, w, h), ref text, color);

        // Keys
        if (inv.GetItem("MagicKey").count > 0)
        {
            text = "xA";
        }
        else
        {
            int numKeys = inv.GetItem("Key").count;
            text = "x" + numKeys.ToString();
        }
        //StereoDrawZeldaFontLabel(new Rect(x, 30, w, h), ref text, color);

        // Bombs
        int bombs = inv.GetItem("Bomb").count;
        text = "x" + bombs.ToString();
        //StereoDrawZeldaFontLabel(new Rect(x, 38, w, h), ref text, color);
    }
    

    void StereoDrawZeldaFontLabel(Rect rect, ref string text, Color color)
    {
        Texture tex = ZeldaFont.Instance.TextureForString(text);
        //StereoDrawTexture(rect, ref tex, color);
        Destroy(tex);
    }


    #region Overworld Map
    /*
        const int MapX = 16, MapY = 12;
        const int MapWidth = 64, MapHeight = 32;

        Color _owMapBgColor = new Color(0.2f, 0.2f, 0.2f);
        Color _owMapSectorColor = Color.green;
        Texture _owMapBgTexture, _owMapSectorTexture;

        void CreateMapTextures()
        {
            _owMapBgTexture = GfxHelper.CreateColoredTexture(_owMapBgColor);
            _owMapSectorTexture = GfxHelper.CreateColoredTexture(_owMapSectorColor);

            _dungeonMapBgTexture = GfxHelper.CreateColoredTexture(_dungeonMapBgColor);
            _dungeonMapSectorTexture = GfxHelper.CreateColoredTexture(_dungeonMapSectorColor);
            _dungeonMapLinkTexture = GfxHelper.CreateColoredTexture(_dungeonMapLinkColor);
            _dungeonMapBossTexture = GfxHelper.CreateColoredTexture(_dungeonMapBossColor);
        }

        void GUIShowOverworldMap(Color color)
        {
            StereoDrawTexture(new Rect(MapX, MapY, MapWidth, MapHeight), ref _owMapBgTexture, color);

            TileMap tileMap = TileProliferator.Instance.tileMap;

            Vector2 sector = tileMap.GetSectorForPosition(CommonObjects.PlayerController_G.transform.position);
            if (sector != WorldInfo.Instance.lostWoodsSector)
            {
                int w = (int)(MapWidth / tileMap.SectorsWide);
                int h = (int)(MapHeight / tileMap.SectorsLong);
                int x = (int)(MapX + w * sector.x);
                int y = (int)(MapY + h * sector.y);
                StereoDrawTexture(new Rect(x, y, w, h), ref _owMapSectorTexture, color);
            }
        }
        */
    #endregion

    #region Dungeon Map
    /*
        const int DungeonSectorWidth = 7, DungeonSectorHeight = 3;
        const int DungeonBossWidth = 3, DungeonBossHeight = 3;
        const int DungeonLinkWidth = 3, DungeonLinkHeight = 3;

        Color _dungeonMapBgColor = Color.black;
        Color _dungeonMapSectorColor = Color.blue;
        Color _dungeonMapBossColor = Color.red;
        Color _dungeonMapLinkColor = Color.green;

        Texture _dungeonMapBgTexture, _dungeonMapSectorTexture, _dungeonMapBossTexture, _dungeonMapLinkTexture;

        void GUIShowDungeonMap(Color color)
        {
            // Background
            StereoDrawTexture(new Rect(MapX, MapY, MapWidth, MapHeight), ref _dungeonMapBgTexture, color);

            int dungeonNum = WorldInfo.Instance.DungeonNum;
            bool hasMap = Inventory.Instance.HasMapForDungeon(dungeonNum);
            bool hasCompass = Inventory.Instance.HasCompassForDungeon(dungeonNum);

            int indent;
            Rect rect;
            foreach (var room in DungeonFactory.Instance.Rooms)
            {
                Vector2 sector = room.GetGridIndices();
                sector.y = DungeonFactory.MaxDungeonLengthInRooms - sector.y - 1;
                int sectorX = (int)(MapX + (DungeonSectorWidth + 1) * sector.x);
                int sectorY = (int)(MapY + (DungeonSectorHeight + 1) * sector.y);

                // Sector
                if (!room.HideOnMap && (hasMap || room.PlayerHasVisited))
                {
                    rect = new Rect(sectorX, sectorY, DungeonSectorWidth, DungeonSectorHeight);
                    StereoDrawTexture(rect, ref _dungeonMapSectorTexture, color);
                }

                // Boss
                if (hasCompass && room.ContainsTriforce)
                {
                    indent = (int)(0.5f * (DungeonSectorWidth - DungeonBossWidth)) + 1;
                    rect = new Rect(sectorX + indent, sectorY, DungeonBossWidth, DungeonBossHeight);
                    StereoDrawTexture(rect, ref _dungeonMapBossTexture, color);
                }

                // Link
                Vector3 playerPos = CommonObjects.PlayerController_G.transform.position;
                if (room == DungeonRoom.GetRoomForPosition(playerPos))
                {
                    indent = (int)(0.5f * (DungeonSectorWidth - DungeonLinkWidth)) + 1;
                    rect = new Rect(sectorX + indent, sectorY, DungeonLinkWidth, DungeonLinkHeight);
                    StereoDrawTexture(rect, ref _dungeonMapLinkTexture, color);
                }
            }
        }
        */
    #endregion


    /*Rect CalcHudRenderArea()
    {
        int w = hudImage.width;
        int h = hudImage.height;
        int x = (int)((Screen.width - w) * 0.5f);

        Vector3 camForward = CommonObjects.PlayerController_C.LineOfSight;

        float dot = Vector3.Dot(camForward, Vector3.up);
        int y = Mathf.RoundToInt(_yBaseOffset + dot * vertShiftSpeed);

        if (Pause.Instance.IsInventoryShowing)
        {
            y = PausedYVal;
        }

        //print("w, h: " + w + ", " + h);
        //print("SCREEN: " + Screen.width + ", " + Screen.height);

        return new Rect(x, y, w, h);
    }*/
}