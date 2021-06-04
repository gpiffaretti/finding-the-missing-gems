using bb = Blitz3DSDK;
using System;

namespace FMG_Application
{
    class Program
    {
        //--GAME CONSTS-------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        const int WIDTH = 1280;
        const int HEIGHT = 720;
        const int DEPTH = 32;
        const int GRAPHIC_MODE = bb.GFX_WINDOWED;

        //--GAME FONTS--------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        const string FONT_NAME = "Gill Sans Ultra Bold";
        const int FONT_HEIGHT = 32;
        const int FONT_BOLD = 1;
        const int FONT_ITALIC = 0;
        const int FONT_UNDERLINED = 1;
        const int FONT_COLOR_RED = 0;
        const int FONT_COLOR_GREEN = 0;
        const int FONT_COLOR_BLUE = 0;


        //--BACKGROUND--------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        static int BACKGROUND_1 = 0;
        static int BACKGROUND_2 = 0;
        static int BACKGROUND_3 = 0;
        static int BACKGROUND_L_X = 0;
        static int BACKGROUND_C_X = WIDTH;
        static int BACKGROUND_R_X = WIDTH * 2;
        //const int BACKGROUND_WIDTH = 1280;
        static int BACKGROUND_SCROLL_SPEED = 20;

        //--HUD IMAGES--------------------------------------------------------------------
        //--------------------------------------------------------------------------------

        static int HUD_BACKGROUND = 0;
        static int HUD_BUTTON_PLAY = 0;
        static int HUD_BAR = 0;
        static int HUD_RESULTS = 0;
        static int HUD_NEXT_STAGE = 0;

        //--PLAYER------------------------------------------------------------------------
        //--------------------------------------------------------------------------------

        private static int PLAYER_INITIAL_X = 260;
        private static int PLAYER_INITIAL_Y = 100;
        private static Player player;

        //--BLOCKS IMAGES-----------------------------------------------------------------
        //--------------------------------------------------------------------------------

        static int BLOCK_0 = 0;
        static int BLOCK_1 = 0;
        static int BLOCK_2 = 0;
        static int BLOCK_3 = 0;

        //--GAME TIME CONSTS---------------------------------------------------------------
        //---------------------------------------------------------------------------------
        static DateTime LAG = DateTime.Now;
        static bool stageFinished = false;
        static bool gameFinished = false;
        static bool stageWon = false;

        //--MOUSE--------------------------------------------------------------------------
        //---------------------------------------------------------------------------------
        private static int MOUSE_X = 0;
        private static int MOUSE_Y = 0;
        private static Tile_Type TILE_ON_MOUSE = 0;
        private static Boolean CLICK_ON_HUD = false;

        static Level level;

        //---------------------------------------------------------------------------------

        [STAThread]
        static void Main()
        {

            bb.BeginBlitz3D();

            setGraphicMode();


            bb.SetBuffer(bb.BackBuffer());
            setTextFont();

            level = new Level();
            player = new Player();
            player.Level = level;

            loadImages();

            level.loadSounds();

            welcomeLoop();

            while (bb.KeyDown(bb.KEY_ESCAPE) == 0 && !gameFinished)
            {

                gameLoop();
                
                if (!stageWon)
                {
                    level.reloadStage();
                    stageWon = false;
                    stageFinished = false;
                    player.resetMatrix();
                    player.ON_EXIT = false;
                    resetBackgroundPosition();
                }
                else if (level.nextStageAvailable())
                {
                    endLevelLoop();
                    level.nextStage();
                    player.resetMatrix();
                    player.ON_EXIT = false;
                    stageFinished = false;
                    stageWon = false;
                    resetBackgroundPosition();
                    
                }
                else
                {
                    endLevelLoop();
                    gameFinished = true;
                }

            }

            
            endGameLoop();

            freeImages();

            bb.EndBlitz3D();

        }

        //---------------------------------------------------------------------------------

        private static void endLevelLoop()
        {
            bb.ClsColor(0, 0, 0);
            bb.Color(0, 0, 0);
            bb.SetFont(bb.LoadFont(FONT_NAME, FONT_HEIGHT, FONT_BOLD, FONT_ITALIC));
            while (!clickInRectangle(900, 640, HUD_NEXT_STAGE) && bb.KeyDown(bb.KEY_ESCAPE) == 0 && bb.KeyDown(bb.KEY_RETURN) == 0)
            {
                int time = level.TimeLeft.Seconds;
                int red = level.Red_Gems - level.Left_Red_Gems;
                int blue = level.Blue_Gems - level.Left_Blue_Gems;
                int total = blue*10 + red*20 + time;
                bb.Cls();
                bb.DrawBlock(HUD_RESULTS, 0, 0);
                bb.Text(820, 320, time.ToString(), 780);
                bb.Text(820, 387, red.ToString()+" x 20",780);
                bb.Text(820, 452, blue.ToString()+ " x 10", 780);
                bb.Text(820, 517, total.ToString(), 780);
                bb.DrawImage(HUD_NEXT_STAGE, 900, 640);
                bb.Flip();
            }
        }

        //---------------------------------------------------------------------------------

        private static void endGameLoop()
        {
            DateTime lag = DateTime.Now;
            bb.ClsColor(0, 0, 0);
            bb.Color(255, 255, 255);
            bb.SetFont(bb.LoadFont(FONT_NAME, FONT_HEIGHT, FONT_BOLD, FONT_ITALIC));
            while ((DateTime.Now - lag).TotalMilliseconds < 2100)
            {
                bb.Cls();
                drawGameExitScreen();
                bb.Flip();
            }
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Resets background position
        /// </summary>
        private static void resetBackgroundPosition()
        {
            BACKGROUND_L_X = 0;
            BACKGROUND_C_X = WIDTH;
            BACKGROUND_R_X = WIDTH * 2;
        }


        //---------------------------------------------------------------------------------
        /// <summary>
        /// Loop for each level of the game
        /// </summary>
        private static void gameLoop()
        {
            player.X_Position = PLAYER_INITIAL_X;
            player.Y_Position = PLAYER_INITIAL_Y;
            player.Direction_X = 1;
            player.CalculatePlayerPositionInMatrix();
            level.StartMusic();
            level.startTimer();

            while (!stageFinished)
            {
                bb.Cls();
                drawWorld();

                TimeSpan span = DateTime.Now.Subtract(LAG);
                if (span.TotalMilliseconds >= 40)
                {

                    mouseEvents();

                    LAG = DateTime.Now;
                    updateWorld();
                    int time = level.timeLeft();
                    
                    if (time < 0)
                    {
                        stageFinished = true;
                        stageWon = false;
                    }
                    else if(player.ON_EXIT)
                    {
                        stageFinished = true;
                        stageWon = true;
                    }
                }
                level.updateTimer();

                bb.Flip();

            }
            level.StopMusic();
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Handles mouse events
        /// </summary>
        private static void mouseEvents()
        {
            mouseClick(Blitz3DSDK.MOUSE_BUTTON);

            if (MOUSE_X != 0 && MOUSE_Y != 0)
            {
                if (clickInRectangle(467, 650, BLOCK_1))
                {
                    TILE_ON_MOUSE = Tile_Type.LeftRamp;
                    CLICK_ON_HUD = true;
                }
                else if (clickInRectangle(607, 650, BLOCK_2))
                {
                    TILE_ON_MOUSE = Tile_Type.Block;
                    CLICK_ON_HUD = true;
                }
                else if (clickInRectangle(747, 650, BLOCK_3))
                {
                    TILE_ON_MOUSE = Tile_Type.RightRamp;
                    CLICK_ON_HUD = true;
                }
            }
            if (CLICK_ON_HUD && bb.MouseDown(bb.MOUSE_BUTTON) == bb.BBTRUE)
            {
                mouseClick(Blitz3DSDK.MOUSE_BUTTON);
                MOUSE_X = -BACKGROUND_L_X + bb.MouseX();
                MOUSE_Y = bb.MouseY();

                int x = MOUSE_X / 64;
                int y = MOUSE_Y / 48;

                // if tile is empty
                if (y<13 && level.Matrix[x][y]== Tile_Type.Empty && !(player.X_Position>x*64-20 && player.X_Position < x*64+64+20 && player.Y_Position>y*48-30 && player.Y_Position<y*48+48+30))
                {
                    if (((TILE_ON_MOUSE == Tile_Type.RightRamp || TILE_ON_MOUSE == Tile_Type.LeftRamp) && (level.Matrix[x][y - 2] == Tile_Type.Empty || player.IsAGemOrExit(x,y-2)))) //|| TILE_ON_MOUSE == Tile_Type.Block)
                    {
                        if (y == 12 && level.Matrix[x][y] != Tile_Type.FixedBlock && level.Matrix[x][y - 1] != Tile_Type.Block && level.Matrix[x][y - 1] != Tile_Type.RightRamp && level.Matrix[x][y - 1] != Tile_Type.LeftRamp)
                        {
                            if (TILE_ON_MOUSE == Tile_Type.LeftRamp && level.Matrix[x + 1][y] != Tile_Type.LeftRamp && level.Matrix[x - 1][y] != Tile_Type.LeftRamp && level.Matrix[x - 1][y] != Tile_Type.Block)
                                level.Matrix[x][y] = TILE_ON_MOUSE;
                            else if (TILE_ON_MOUSE == Tile_Type.RightRamp && level.Matrix[x + 1][y] != Tile_Type.RightRamp && level.Matrix[x + 1][y] != Tile_Type.Block && level.Matrix[x - 1][y] != Tile_Type.RightRamp)
                                level.Matrix[x][y] = TILE_ON_MOUSE;
                        }
                        else
                        {

                            if (y < level.Matrix[0].Length && level.Matrix[x][y] != Tile_Type.FixedBlock && level.Matrix[x][y - 1] != Tile_Type.Block && level.Matrix[x][y - 1] != Tile_Type.RightRamp && level.Matrix[x][y - 1] != Tile_Type.LeftRamp)
                            {
                                if (level.Matrix[x][y + 1] == Tile_Type.FixedBlock || level.Matrix[x][y + 1] == Tile_Type.Block)
                                {
                                    if (TILE_ON_MOUSE == Tile_Type.LeftRamp && level.Matrix[x + 1][y] != Tile_Type.LeftRamp && level.Matrix[x - 1][y] != Tile_Type.LeftRamp && level.Matrix[x - 1][y] != Tile_Type.Block)
                                        level.Matrix[x][y] = TILE_ON_MOUSE;
                                    else if (TILE_ON_MOUSE == Tile_Type.RightRamp && level.Matrix[x + 1][y] != Tile_Type.RightRamp && level.Matrix[x + 1][y] != Tile_Type.Block && level.Matrix[x - 1][y] != Tile_Type.RightRamp)
                                        level.Matrix[x][y] = TILE_ON_MOUSE;
                                    else if (TILE_ON_MOUSE == Tile_Type.Block && level.Matrix[x - 1][y] != Tile_Type.RightRamp && level.Matrix[x + 1][y] != Tile_Type.LeftRamp)
                                        level.Matrix[x][y] = TILE_ON_MOUSE;
                                }
                            }
                        }
                    }
                    else if (TILE_ON_MOUSE == Tile_Type.Block)
                    {
                        if (y == 12 && level.Matrix[x][y] != Tile_Type.FixedBlock && level.Matrix[x][y - 1] != Tile_Type.Block && level.Matrix[x][y - 1] != Tile_Type.RightRamp && level.Matrix[x][y - 1] != Tile_Type.LeftRamp)
                        {
                            if (level.Matrix[x + 1][y] != Tile_Type.LeftRamp &&
                                level.Matrix[x - 1][y] != Tile_Type.RightRamp)
                                level.Matrix[x][y] = TILE_ON_MOUSE;
                        }
                        else
                        {

                            if (y < level.Matrix[0].Length && level.Matrix[x][y] != Tile_Type.FixedBlock && level.Matrix[x][y - 1] != Tile_Type.Block && level.Matrix[x][y - 1] != Tile_Type.RightRamp && level.Matrix[x][y - 1] != Tile_Type.LeftRamp)
                            {
                                if (level.Matrix[x][y + 1] == Tile_Type.FixedBlock || level.Matrix[x][y + 1] == Tile_Type.Block)
                                {
                                    if (TILE_ON_MOUSE == Tile_Type.LeftRamp && level.Matrix[x + 1][y] != Tile_Type.LeftRamp && level.Matrix[x - 1][y] != Tile_Type.LeftRamp && level.Matrix[x - 1][y] != Tile_Type.Block)
                                        level.Matrix[x][y] = TILE_ON_MOUSE;
                                    else if (TILE_ON_MOUSE == Tile_Type.RightRamp && level.Matrix[x + 1][y] != Tile_Type.RightRamp && level.Matrix[x + 1][y] != Tile_Type.Block && level.Matrix[x - 1][y] != Tile_Type.RightRamp)
                                        level.Matrix[x][y] = TILE_ON_MOUSE;
                                    else if (TILE_ON_MOUSE == Tile_Type.Block && level.Matrix[x - 1][y] != Tile_Type.RightRamp && level.Matrix[x + 1][y] != Tile_Type.LeftRamp)
                                        level.Matrix[x][y] = TILE_ON_MOUSE;
                                }
                            }
                        }
                    } 
                }


            } 
            if (bb.MouseDown(bb.MOUSE_RIGHTBUTTON) == bb.BBTRUE)
            {

                mouseClick(Blitz3DSDK.MOUSE_RIGHTBUTTON);
                MOUSE_X = -BACKGROUND_L_X + bb.MouseX();
                MOUSE_Y = bb.MouseY();

                int x = MOUSE_X / 64;
                int y = MOUSE_Y / 48;

                if (y < 13 && level.Matrix[x][y] != Tile_Type.RedGem && level.Matrix[x][y] != Tile_Type.BlueGem && level.Matrix[x][y] != Tile_Type.GreenGem)
                {
                    if (y == 12 && level.Matrix[x][y - 1] == Tile_Type.Empty && level.Matrix[x][y] != Tile_Type.FixedBlock)
                    {
                        level.Matrix[x][y] = Tile_Type.Empty;
                    }
                    else if (y < 12 && level.Matrix[x][y] != Tile_Type.FixedBlock && level.Matrix[x][y + 1] != Tile_Type.Empty && (level.Matrix[x][y - 1] != Tile_Type.Block && level.Matrix[x][y - 1] != Tile_Type.LeftRamp && level.Matrix[x][y - 1] != Tile_Type.RightRamp))
                    {
                        level.Matrix[x][y] = Tile_Type.Empty;
                    } 
                }
            }
        }

        //---------------------------------------------------------------------------------

        /// <summary>
        /// Updates world
        /// </summary>
        private static void updateWorld()
        {
            updateBackground();
            player.Update(BACKGROUND_L_X);
        }

        //---------------------------------------------------------------------------------

        /// <summary>
        /// Sets the fonts for the game
        /// </summary>
        private static void setTextFont()
        {
            int font = bb.LoadFont(FONT_NAME, FONT_HEIGHT, FONT_BOLD, FONT_ITALIC);
            bb.SetFont(font);

            bb.Color(FONT_COLOR_RED, FONT_COLOR_GREEN, FONT_COLOR_BLUE);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Welcome loop
        /// </summary>
        private static void welcomeLoop()
        {
            string text = "";
            int asciiCode;
            while (!clickInRectangle(900, 640, HUD_BUTTON_PLAY) && bb.KeyDown(bb.KEY_ESCAPE) == 0 && bb.KeyDown(bb.KEY_RETURN) == 0)
            {
                bb.Cls();
                drawWelcomeScreen();
                asciiCode = bb.GetKey();
                bb.Text(680, 480, text, bb.BBFALSE, bb.BBTRUE);
                if (asciiCode != 0)
                {
                    if (asciiCode == 8 && text.Length > 0)
                        text = text.Remove(text.Length - 1);
                    else
                        text = text + char.ConvertFromUtf32(asciiCode);

                }
                bb.Flip();
            }
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Draws exit message
        /// </summary>
        private static void drawGameExitScreen()
        {
            bb.Text(950, 600, "GAME FINISHED!!!");
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Draws welcome screen
        /// </summary>
        private static void drawWelcomeScreen()
        {
            bb.DrawBlock(HUD_BACKGROUND, 0, 0);
            bb.DrawImage(HUD_BUTTON_PLAY, 900, 640);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Returns True only if de mouse button is down and the click
        /// was done inside the image
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        private static bool clickInRectangle(int x, int y, int image)
        {
            if (Blitz3DSDK.MouseDown(bb.MOUSE_BUTTON) == bb.BBTRUE)
            {
                if (bb.bbMouseX() > x && bb.bbMouseX() < x + bb.ImageWidth(image))
                    if (bb.bbMouseY() > y && bb.bbMouseY() < y + bb.ImageHeight(image))
                        return true;
            }
            return false;
        }

        /// <summary>
        /// Frees the images when finishing the game
        /// </summary>
        private static void freeImages()
        {
            bb.FreeImage(BACKGROUND_1);
            bb.FreeImage(BACKGROUND_2);
            bb.FreeImage(BACKGROUND_3);
            bb.FreeImage(HUD_BACKGROUND);
            bb.FreeImage(HUD_BUTTON_PLAY);
            bb.FreeImage(HUD_BAR);
            bb.FreeImage(HUD_NEXT_STAGE);
            bb.FreeImage(HUD_RESULTS);
            bb.FreeImage(BLOCK_0);
            bb.FreeImage(BLOCK_1);
            bb.FreeImage(BLOCK_2);
            bb.FreeImage(BLOCK_3);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Draws world
        /// </summary>
        private static void drawWorld()
        {
            drawBackground();
            drawHUD();
            level.Draw(BACKGROUND_L_X);
            player.Draw(BACKGROUND_L_X);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Draws background background
        /// </summary>
        /// <returns></returns>
        private static void drawBackground()
        {
            bb.DrawBlock(BACKGROUND_1, BACKGROUND_L_X, 0);
            bb.DrawBlock(BACKGROUND_2, BACKGROUND_C_X, 0);
            bb.DrawBlock(BACKGROUND_3, BACKGROUND_R_X, 0);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Draws the HUD
        /// </summary>
        private static void drawHUD()
        {
            bb.DrawBlock(HUD_BAR, 0, HEIGHT - 96);
            bb.DrawImage(BLOCK_2, 607, 650);
            bb.DrawImage(BLOCK_1, 467, 650);
            bb.DrawImage(BLOCK_3, 747, 650);
            bb.Color(255, 255, 255);


            bb.Text(100, 650, String.Concat(level.Blue_Gems - level.Left_Blue_Gems, "/" + level.Blue_Gems), bb.BBFALSE, bb.BBTRUE);

            bb.Text(100, 687, String.Concat(level.Red_Gems - level.Left_Red_Gems, "/" + level.Red_Gems), bb.BBFALSE, bb.BBTRUE);

            bb.Text(950, 660, "Stage: " + level.currentStage(), bb.BBFALSE, bb.BBTRUE);

            bb.Text(950, 690, "Time left: " + string.Format("{0}:{1}",level.TimeLeft.Minutes, level.TimeLeft.Seconds) + " sec", bb.BBFALSE, bb.BBTRUE);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Updates the background of the game
        /// </summary>
        private static void updateBackground()
        {
            keyEvents();
        }

        //---------------------------------------------------------------------------------

        /// <summary>
        /// check for key events and updates background position if needed
        /// </summary>
        private static void keyEvents()
        {
            // ---- KEYBOARD EVENTS --------------------
            if (bb.KeyDown(bb.KEY_RIGHT) == bb.BBTRUE)
            {
                if ((BACKGROUND_R_X + WIDTH) - BACKGROUND_SCROLL_SPEED >= WIDTH)
                {
                    BACKGROUND_L_X -= BACKGROUND_SCROLL_SPEED;
                    BACKGROUND_C_X -= BACKGROUND_SCROLL_SPEED;
                    BACKGROUND_R_X -= BACKGROUND_SCROLL_SPEED;
                }
                else
                {
                    BACKGROUND_L_X = -WIDTH * 2;
                    BACKGROUND_C_X = -WIDTH;
                    BACKGROUND_R_X = 0;
                }

            }
            if (bb.KeyDown(bb.KEY_LEFT) == bb.BBTRUE)
            {
                if (BACKGROUND_L_X + BACKGROUND_SCROLL_SPEED <= 0)
                {
                    BACKGROUND_L_X += BACKGROUND_SCROLL_SPEED;
                    BACKGROUND_C_X += BACKGROUND_SCROLL_SPEED;
                    BACKGROUND_R_X += BACKGROUND_SCROLL_SPEED;
                }
                else
                {
                    BACKGROUND_L_X = 0;
                    BACKGROUND_C_X = WIDTH;
                    BACKGROUND_R_X = WIDTH * 2;
                }
            }
            int ascii = bb.GetKey();
            if (ascii == 32)
            {
                stageFinished = true;
                stageWon = true;
                player.X_Position = PLAYER_INITIAL_X;
                player.Y_Position = PLAYER_INITIAL_Y;
                bb.FlushKeys();
            }
            if (ascii == 114 || ascii == 82)
            {
                stageFinished = true;
                stageWon = false;
                player.X_Position = PLAYER_INITIAL_X;
                player.Y_Position = PLAYER_INITIAL_Y;
                bb.FlushKeys();
            }
        }

        //---------------------------------------------------------------------------------

        /// <summary>
        /// Loads all the images needed for the game
        /// </summary>
        private static void loadImages()
        {
            player.LoadImages();
            level.LoadImages();
            BACKGROUND_1 = bb.LoadImage("Resources/Backgrounds/Backgrounds1/Background1_0.png");
            BACKGROUND_2 = bb.LoadImage("Resources/Backgrounds/Backgrounds1/Background1_1.png");
            BACKGROUND_3 = bb.LoadImage("Resources/Backgrounds/Backgrounds1/Background1_2.png");
            HUD_BACKGROUND = bb.LoadImage("Resources/HUD/background-HUD.png");
            HUD_BUTTON_PLAY = bb.LoadImage("Resources/HUD/Button-Play.png");
            HUD_RESULTS = Blitz3DSDK.LoadImage("Resources/HUD/background-Results-HUD.png");
            HUD_BAR = bb.LoadImage("Resources/HUD/Bar.png");
            HUD_NEXT_STAGE = Blitz3DSDK.LoadImage("Resources/HUD/Button-NextStage.png");
            BLOCK_0 = bb.LoadImage("Resources/Tiles/Block0.png");
            BLOCK_1 = bb.LoadImage("Resources/Tiles/Block2.png");
            BLOCK_2 = bb.LoadImage("Resources/Tiles/Block1.png");
            BLOCK_3 = bb.LoadImage("Resources/Tiles/Block3.png");

            if (BACKGROUND_1 != 0)
            {
                bb.ResizeImage(BACKGROUND_1, WIDTH, HEIGHT);
                bb.MaskImage(BACKGROUND_1, 255, 0, 0);
            }
            if (BACKGROUND_2 != 0)
            {
                bb.ResizeImage(BACKGROUND_2, WIDTH, HEIGHT);
                bb.MaskImage(BACKGROUND_2, 255, 0, 0);
            }
            if (BACKGROUND_3 != 0)
            {
                bb.ResizeImage(BACKGROUND_3, WIDTH, HEIGHT);
                bb.MaskImage(BACKGROUND_3, 255, 0, 0);
            }
            if (HUD_BACKGROUND != 0)
            {
                bb.ResizeImage(HUD_BACKGROUND, WIDTH, HEIGHT);
                bb.MaskImage(HUD_BACKGROUND, 255, 0, 0);
            }
            if (HUD_BUTTON_PLAY != 0)
            {
                bb.MaskImage(HUD_BUTTON_PLAY, 255, 100, 200);
            }
            if (HUD_BAR != 0)
            {
                bb.ResizeImage(HUD_BAR, WIDTH, bb.ImageHeight(HUD_BAR));
            }
            if (HUD_RESULTS != 0)
            {
                bb.ResizeImage(HUD_RESULTS, WIDTH, HEIGHT);
                bb.MaskImage(HUD_RESULTS, 255, 0, 0);
            }
            if (HUD_NEXT_STAGE != 0)
            {
                bb.MaskImage(HUD_NEXT_STAGE, 255, 100, 200);
            }
            if (BLOCK_0 != 0)
            {
                bb.MaskImage(BLOCK_0, 255, 255, 255);
            }
            if (BLOCK_1 != 0)
            {
                bb.MaskImage(BLOCK_1, 255, 255, 255);
            }
            if (BLOCK_2 != 0)
            {
                bb.MaskImage(BLOCK_2, 255, 255, 255);
            }
            if (BLOCK_3 != 0)
            {
                bb.MaskImage(BLOCK_3, 255, 255, 255);
            }

        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Set the graphics properties for the game
        /// </summary>
        private static void setGraphicMode()
        {
            bb.Graphics(WIDTH, HEIGHT, DEPTH, GRAPHIC_MODE);

            bb.SetBlitz3DTitle("FMG", "¿Seguro desea salir del juego?");

            bb.ClsColor(255, 0, 0);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Sets into X and Y values of cursor position, only if a click was done
        /// otherwise X and Y while be 0
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private static void mouseClick(int mouseButton)
        {
            if (bb.MouseDown(mouseButton) == bb.BBTRUE)
            {
                MOUSE_X = bb.bbMouseX();
                MOUSE_Y = bb.bbMouseY();
            }
            else
            {
                MOUSE_X = 0;
                MOUSE_Y = 0;
            }
        }
    }
}
