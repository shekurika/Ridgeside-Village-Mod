﻿using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;

namespace RidgesideVillage
{
    internal static class SpiritRealm
    {
        static IModHelper Helper;
        static IMonitor Monitor;
        static bool IsRenderingFog;
        static Texture2D FogTexture;
        static Vector2 FogPosition;

        const string Relics1 = "BelRelicHints";
        const string Relics2 = "RaeRelicHints";
        internal static void Initialize(IMod ModInstance)
        {
            Helper = ModInstance.Helper;
            Monitor = ModInstance.Monitor;
            FogTexture = Helper.Content.Load<Texture2D>(PathUtilities.NormalizePath("assets/SpiritRealmFog.png"));

            TileActionHandler.RegisterTileAction("RSVWarp", RSVWarp);
            TileActionHandler.RegisterTileAction("DaiaBook", RSVOpenDaiaBook);
            Helper.Events.Player.Warped += OnWarped;
        }

        private static void RSVOpenDaiaBook(string tileActionString, Vector2 position)
        {
            if (Game1.player.mailReceived.Contains(Relics1))
            {
                Game1.playSound("shadowpeep");
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("Daia.BookOpen"));
                OpenBook();
            }
        }

        private static void OpenBook()
        {
            if (Game1.player.mailReceived.Contains(Relics2) == false)
            {
                var responses = new List<Response>
                {
                    new Response("page1", Helper.Translation.Get("Daia.Page1")),
                    new Response("page2", Helper.Translation.Get("Daia.Page2")),
                    new Response("page3", Helper.Translation.Get("Daia.Page3")),
                    new Response("page4", Helper.Translation.Get("Daia.Page4")),
                    new Response("cancel", Helper.Translation.Get("Daia.BookClose")),
                };
                var responseActions = new List<Action>
                {
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint1"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint2"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint3"));
                    },
                    delegate
                    {
                        //Show image here
                    },
                    delegate{}
                };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("Daia.BookPages"), responses, responseActions);
            }
            else
            {
                var responses = new List<Response>
                {
                    new Response("page1", Helper.Translation.Get("Daia.Page1")),
                    new Response("page2", Helper.Translation.Get("Daia.Page2")),
                    new Response("page3", Helper.Translation.Get("Daia.Page3")),
                    new Response("page4", Helper.Translation.Get("Daia.Page4")),
                    new Response("page5", Helper.Translation.Get("Daia.Page5")),
                    new Response("page6", Helper.Translation.Get("Daia.Page6")),
                    new Response("page7", Helper.Translation.Get("Daia.Page7")),
                    new Response("cancel", Helper.Translation.Get("Daia.BookClose")),
                };
                var responseActions = new List<Action>
                {
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint1"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint2"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint3"));
                    },
                    delegate
                    {
                        //Show RSVDaiaPage4.png here
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint4"));
                    },
                    delegate
                    {
                        Game1.activeClickableMenu = new LetterViewerMenu(Helper.Translation.Get("Daia.RelicHint5"));
                    },
                    delegate
                    {
                        //Show RSVDaiaPage7.png here
                    },
                    delegate{}
                };

                Game1.activeClickableMenu = new DialogueBoxWithActions(Helper.Translation.Get("Daia.BookPages"), responses, responseActions);
            }
        }

        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation != null && e.NewLocation.Name.Equals("Custom_Ridgeside_RSVSpiritRealm"))
            {
                e.NewLocation.waterColor.Value = new Color(35, 214, 213, 120);
                if (!IsRenderingFog)
                {
                    IsRenderingFog = true;
                    //Helper.Events.Display.RenderedWorld += DrawFog;
                    FogPosition = new Vector2(0f);
                }
            }
            else if(IsRenderingFog)
            {
                IsRenderingFog = false;
                //Helper.Events.Display.RenderedWorld -= DrawFog;
            }
        }


        static Color color = Color.White * 0.4f;
        private static void DrawFog(object sender, RenderedWorldEventArgs e)
        {

            float TextureSize = 512f;
            //I've no idea what Im doing
            FogPosition -= Game1.getMostRecentViewportMotion();
            FogPosition.X %= TextureSize;
            FogPosition.Y %= TextureSize;
            for (float x = -1000f * Game1.options.zoomLevel + FogPosition.X; x < (float)Game1.graphics.GraphicsDevice.Viewport.Width + TextureSize; x += TextureSize)
            {
                for (float y = -TextureSize + FogPosition.Y; y < (float)(Game1.graphics.GraphicsDevice.Viewport.Height + 128); y += TextureSize)
                {
                    e.SpriteBatch.Draw(FogTexture, new Vector2(x, y), null, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                }
            }
        }

        internal static void RSVWarp(string tileActionString, Vector2 position)
        {
            var split = tileActionString.Split(' ');
            if (split.Length != 4)
            {
                Log.Error($"Error in TileAction {tileActionString} on tile {position} in {Game1.currentLocation.Name}");
                return;
            }
            if (!(int.TryParse(split[2], out int xCoord) && int.TryParse(split[3], out int yCoord))){
                Log.Error($"Error in TileAction {tileActionString} on tile {position} in {Game1.currentLocation.Name}. Couldnt parse coordinates.");
                return;
            }


            Farmer who = Game1.player;
            if(Game1.random.NextDouble() < 0.02)
            {
                who.currentLocation.playSound("cow");                
            }
            else
            {
                who.currentLocation.playSound("wand");
            }
            Game1.displayFarmer = false;
            Game1.player.temporarilyInvincible = true;
            Game1.player.temporaryInvincibilityTimer = -2000;
            Game1.player.freezePause = 1000;
            if (split[1].Equals(Game1.currentLocation.Name))
            {
                Game1.delayedActions.Add(new DelayedAction(1000, delegate
                    { WarpFarmerOnSameMap(xCoord, yCoord); }));
            }
            else
            {
                Game1.flashAlpha = 1f;
                DelayedAction.fadeAfterDelay(delegate { WarpFarmerToDifferentMap(split[1], xCoord, yCoord); }, 1000);
            }
            int j = 0;
            for (int x = who.getTileX() + 8; x >= who.getTileX() - 8; x--)
            {
                who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(x, who.getTileY()) * 64f, Color.White, 8, flipped: false, 50f)
                {
                    layerDepth = 1f,
                    delayBeforeAnimationStart = j * 25,
                    motion = new Vector2(-0.25f, 0f)
                });
                j++;
            }

            //draw stuff

        }

        static void WarpFarmerToDifferentMap(string LocationName, int X, int Y)
        { 
            Game1.warpFarmer(LocationName, X, Y, flip: false);
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
        }
        static void WarpFarmerOnSameMap(int X, int Y)
        {
            Game1.player.position.Value = new Vector2(X, Y) * 64f - new Vector2(0f, Game1.player.Sprite.getHeight() - 48);
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
        }
    }
}
