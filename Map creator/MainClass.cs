using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bb = Blitz3DSDK;
using System.IO;
using System.Xml;

namespace MapCreator
{
    class MainClass
    {
        // ---- GRAPHICS ---------------------------------------
        // -----------------------------------------------------
        const int WIDTH = 1280;
        const int HEIGHT = 720;
        const int DEPTH = 32;
        const int GRAPHIC_MODE = bb.GFX_WINDOWED;

        // ---- BACKGROUND -------------------------------------
        // -----------------------------------------------------
        static int BACKGROUND_L = 0;
        static int BACKGROUND_C = 0;
        static int BACKGROUND_R = 0;

        const int BACKGROUND_WIDTH = 1280;
        const int BACKGROUND_HEIGHT = 720;

        static int BACKGROUND_L_X = 0;
        static int BACKGROUND_C_X = BACKGROUND_WIDTH;
        static int BACKGROUND_R_X = BACKGROUND_WIDTH * 2;

        static int BACKGROUND_SCROLL_SPEED = 10;

        // ---- DATE-TIME CONSTS -------------------------------
        // -----------------------------------------------------
        static DateTime LAG = DateTime.Now;

        // ---- BACKGROUND TILES -------------------------------
        // -----------------------------------------------------
        static int BLOCK = 0;
        static TileType[][] TILES;

        static int HUD;
        static int EXIT;

        // ---- GEMS -------------------------------------------
        // -----------------------------------------------------
        static int BLUE_GEM;
        static int GREEN_GEM;
        static int RED_GEM;

        // ---- PLAYER -----------------------------------------
        // -----------------------------------------------------
        static int PLAYER_RIGHT;
        static int PLAYER_LEFT;
        static int PLAYER_RIGHT_UP; 
        static int PLAYER_RIGHT_DOWN;
        static int PLAYER_LEFT_UP;
        static int PLAYER_LEFT_DOWN;
        static int PLAYER_FLYING_RIGHT;
        static int PLAYER_FLYING_LEFT;
        static int PLAYER_X = 140;
        static int PLAYER_Y = 100;
        static int PLAYER_X_SPEED = 5;
        static int PLAYER_Y_SPEED =7;
        static int PLAYER_ACTUAL_FRAME = 0;
        static int PLAYER_DIR = 1;
        static int PLAYER_X1;
        static int PLAYER_X2;
        static int PLAYER_Y1;
        static int PLAYER_Y2;
        static bool PLAYER_ON_GROUND = false;

        // ---- MAIN -------------------------------------------
        // -----------------------------------------------------
        static int ACTUAL_TILE_TYPE = 0;
        static TileType[] POSSIBLE_TYPES = new TileType[5] {TileType.FixedBlock, TileType.GreenGem, TileType.RedGem, TileType.BlueGem, TileType.Exit};
        static bool EXIT_ALREADY_PLACED = false;

        static void Main(string[] args)
        {
            bb.BeginBlitz3D();
            setGraphicMode();
            bb.SetBuffer(bb.BackBuffer());
            initializeTileMatrix();
            loadImages();
            CalculatePlayerPositionInMatrix();

            DateTime now;
            bool salir = false;
            while (!salir) {

                bb.Cls();

                drawWorld();

                now = DateTime.Now;
                TimeSpan span = now.Subtract(LAG);

                if (span.TotalMilliseconds >= 25)
                {
                    LAG = now;
                    updateWorld();
                }

                if (bb.KeyDown(bb.KEY_RETURN)==bb.BBTRUE && EXIT_ALREADY_PLACED) salir = true;

                bb.Flip();
            }

            freeImages();

            string fileName = string.Empty;
            while (bb.KeyDown(bb.KEY_RETURN) != bb.BBTRUE)
            {
                
            }

            createXMLScenery(fileName);

            bb.EndBlitz3D();
        }

        private static void CalculatePlayerPositionInMatrix()
        {
            int x = PLAYER_X / 64;
            int x_rest = PLAYER_X % 64;
            int y = PLAYER_Y / 48;
            int y_rest = PLAYER_Y % 48;
            if (x_rest >= 32 && x+1 < TILES.Length)
            {
                PLAYER_X1 = x;
                PLAYER_X2 = x + 1;
            }
            else {
                PLAYER_X1 = x - 1;
                PLAYER_X2 = x;
            }
            if (y_rest >= 24 && y + 1 < TILES[0].Length)
            {
                PLAYER_Y1 = y;
                PLAYER_Y2 = y + 1;
            }
            else
            {
                PLAYER_Y1 = y - 1;
                PLAYER_Y2 = y;
            }
            
        }

        private static void drawWorld()
        {
            drawBackground();
            drawPlayer();
        }

        private static void drawPlayer()
        {
            if (PLAYER_ON_GROUND)
            {
                if (PLAYER_DIR == 1)
                {
                    bb.DrawImage(PLAYER_RIGHT, BACKGROUND_L_X + PLAYER_X, PLAYER_Y, PLAYER_ACTUAL_FRAME);
                }
                else
                {
                    bb.DrawImage(PLAYER_LEFT, BACKGROUND_L_X + PLAYER_X, PLAYER_Y, PLAYER_ACTUAL_FRAME);
                }
            }
            else {
                if (PLAYER_DIR == 1)
                {
                    bb.DrawImage(PLAYER_FLYING_RIGHT, BACKGROUND_L_X + PLAYER_X, PLAYER_Y);
                }
                else
                {
                    bb.DrawImage(PLAYER_FLYING_LEFT, BACKGROUND_L_X + PLAYER_X, PLAYER_Y);
                } 
            }
            
            
        }

        private static void createXMLScenery(string fileName)
        {
            FileStream f = File.Create("level.lvl");

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars= "\t";
            //settings.NewLineChars = "line";
            
            
            XmlWriter writer = XmlWriter.Create(f, settings);
            //writer.WriteStartDocument();
            writer.WriteRaw(string.Format(@"<scenery stage=""{0}"" time=""{1}"" >", 1, 60));

            for (int i = 0; i < TILES.Length; i++)
            {
                for (int j = 0; j < TILES[0].Length; j++)
                {
                    if (TILES[i][j] != TileType.Empty) {
                        writer.WriteRaw(string.Format(@"<tile type=""{0}"" posX=""{1}"" posY=""{2}"" />", TILES[i][j].ToString(), i.ToString(), j.ToString()));
                        
                        
                    }
                }
            }

            writer.WriteRaw("</scenery>");
            //writer.WriteEndDocument();
            writer.Close();
            f.Close();
        }

        private static void initializeTileMatrix()
        {
            int columns = BACKGROUND_WIDTH * 3 / 64;
            int rows = BACKGROUND_HEIGHT / 48 - 2;

            TILES = new TileType[columns][];
            for (int i = 0; i < TILES.Length; i++)
            {
                TILES[i] = new TileType[rows];
                for (int j = 0; j < rows; j++ )
                {
                    if (i == 0 || i == TILES.Length - 1 || j == 0)
                    {
                        TILES[i][j] = TileType.FixedBlock;
                    }
                    else {
                        TILES[i][j] = TileType.Empty;
                    }
                    
                }
            }
            
            
        }

        private static void setGraphicMode()
        {
            bb.Graphics(WIDTH, HEIGHT, DEPTH, GRAPHIC_MODE);
            bb.SetBlitz3DTitle("Find the missing gems", "Are you sure?");
            bb.ClsColor(255, 0, 0);
        }

        private static void processLeftClick() {
            int mouse_x = -BACKGROUND_L_X + bb.MouseX();
            int mouse_y = bb.MouseY();

            // obtener posicion en matriz
            int x = mouse_x / 64;
            int y = mouse_y / 48;

            // si no hay nada pongo un block
            if (y < TILES[0].Length && TILES[x][y] == TileType.Empty) {
                if (POSSIBLE_TYPES[ACTUAL_TILE_TYPE] == TileType.Exit)
                {
                    if (!EXIT_ALREADY_PLACED)
                    {
                        TILES[x][y] = POSSIBLE_TYPES[ACTUAL_TILE_TYPE];
                        EXIT_ALREADY_PLACED = true;
                    }
                }
                else {
                    TILES[x][y] = POSSIBLE_TYPES[ACTUAL_TILE_TYPE];
                }
                
            }
        }

        private static void processRightClick() {
            int mouse_x = -BACKGROUND_L_X + bb.MouseX();
            int mouse_y = bb.MouseY();

            // obtener posicion en matriz
            int x = mouse_x / 64;
            int y = mouse_y / 48;

            // si hay un bloque lo quito
            if (y>0 && x>0 && x<TILES.Length-1 && y < TILES[0].Length && TILES[x][y] != TileType.Empty)
            {
                if (TILES[x][y] == TileType.Exit) EXIT_ALREADY_PLACED = false;
                TILES[x][y] = TileType.Empty;
            }
            
        }

        private static void freeImages()
        {
            bb.FreeImage(BACKGROUND_L);
            bb.FreeImage(BACKGROUND_C);
            bb.FreeImage(BACKGROUND_R);
        }

        private static void loadImages()
        {
            BACKGROUND_L = bb.LoadImage("Resources/Backgrounds/Backgrounds1/Background1_0.png");
            BACKGROUND_C = bb.LoadImage("Resources/Backgrounds/Backgrounds1/Background1_1.png");
            BACKGROUND_R = bb.LoadImage("Resources/Backgrounds/Backgrounds1/Background1_2.png");

            BLOCK = bb.LoadImage("Resources/Tiles/Block0.png");
            bb.MaskImage(BLOCK, 255, 255, 255);

            GREEN_GEM = bb.LoadImage("Resources/Gems/Green_Gem.png");
            bb.MaskImage(GREEN_GEM, 255, 255, 255);
            bb.MidHandle(GREEN_GEM);

            BLUE_GEM = bb.LoadImage("Resources/Gems/blue_Gem.png");
            bb.MaskImage(BLUE_GEM, 255, 255, 255);
            bb.MidHandle(BLUE_GEM);

            RED_GEM = bb.LoadImage("Resources/Gems/Red_Gem.png");
            bb.MaskImage(RED_GEM, 255, 255, 255);
            bb.MidHandle(RED_GEM);

            HUD = bb.LoadImage("Resources/HUD/Bar.png");
            bb.ResizeImage(HUD, WIDTH, bb.ImageHeight(HUD));

            EXIT = bb.LoadImage("Resources/Tiles/Exit.png");
            bb.MaskImage(EXIT, 0, 0, 0);

            PLAYER_RIGHT = bb.LoadAnimImage("Resources/Player/Run_Right.png", 96, 96, 0, 10);
            bb.MaskImage(PLAYER_RIGHT, 0, 0, 0);
            bb.MidHandle(PLAYER_RIGHT);

            PLAYER_LEFT = bb.LoadAnimImage("Resources/Player/Run_Left.png", 96, 96, 0, 10);
            bb.MaskImage(PLAYER_LEFT, 0, 0, 0);
            bb.MidHandle(PLAYER_LEFT);

            PLAYER_RIGHT_UP = bb.LoadAnimImage("Resources/Player/Run_Up_Right.png", 96, 96, 0, 10);
            bb.MaskImage(PLAYER_RIGHT_UP, 0, 0, 0);
            bb.MidHandle(PLAYER_RIGHT_UP);

            PLAYER_LEFT_UP = bb.LoadAnimImage("Resources/Player/Run_Up_Left.png", 96, 96, 0, 10);
            bb.MaskImage(PLAYER_LEFT_UP, 0, 0, 0);
            bb.MidHandle(PLAYER_LEFT_UP);

            PLAYER_RIGHT_DOWN = bb.LoadAnimImage("Resources/Player/Run_Down_Right.png", 96, 96, 0, 10);
            bb.MaskImage(PLAYER_RIGHT_DOWN, 0, 0, 0);
            bb.MidHandle(PLAYER_RIGHT_DOWN);

            PLAYER_LEFT_DOWN = bb.LoadAnimImage("Resources/Player/Run_Down_Left.png", 96, 96, 0, 10);
            bb.MaskImage(PLAYER_LEFT_DOWN, 0, 0, 0);
            bb.MidHandle(PLAYER_LEFT_DOWN);

            PLAYER_FLYING_LEFT = bb.LoadAnimImage("Resources/Player/Jump.png", 96, 96, 7, 1);
            bb.MaskImage(PLAYER_FLYING_LEFT, 0, 0, 0);
            bb.MidHandle(PLAYER_FLYING_LEFT);

            PLAYER_FLYING_RIGHT = bb.CopyImage(PLAYER_FLYING_LEFT);
            bb.ScaleImage(PLAYER_FLYING_RIGHT, -1, 1);
            bb.MaskImage(PLAYER_FLYING_RIGHT, 0, 0, 0);
            bb.MidHandle(PLAYER_FLYING_RIGHT);

        }

        private static void updateWorld()
        {

            updateBackground();
            updatePlayer();
            
        }

        private static void updatePlayer()
        {
            if (PLAYER_ON_GROUND) // IF PLAYER IS ON GROUND
            {
                if (ValidPosition(PLAYER_X1, PLAYER_Y2 + 1)) // WALKING OVER TILES
                {
                    if (PLAYER_DIR == 1 && TILES[PLAYER_X2][PLAYER_Y2 + 1] == TileType.Empty && bb.ImageRectCollide(PLAYER_LEFT, BACKGROUND_L_X + PLAYER_X, PLAYER_Y, PLAYER_ACTUAL_FRAME, BACKGROUND_L_X + PLAYER_X1 * 64, (PLAYER_Y2 + 1) * 48, 64, 48) == bb.BBFALSE)
                    {
                        PLAYER_ON_GROUND = false;
                        PLAYER_X += 2* PLAYER_X_SPEED * PLAYER_DIR;
                    }
                    else if (PLAYER_DIR == -1 && TILES[PLAYER_X1][PLAYER_Y2 + 1] == TileType.Empty && bb.ImageRectCollide(PLAYER_LEFT, BACKGROUND_L_X + PLAYER_X, PLAYER_Y, PLAYER_ACTUAL_FRAME, BACKGROUND_L_X + PLAYER_X2 * 64, (PLAYER_Y2 + 1) * 48, 64, 48) == bb.BBFALSE)
                    {
                        PLAYER_ON_GROUND = false;
                        PLAYER_X += 2 * PLAYER_X_SPEED * PLAYER_DIR;
                    }
                    else {
                        PLAYER_X += PLAYER_X_SPEED * PLAYER_DIR;
                    }
                }
                else // WALKING OVER FLOOR
                {
                    PLAYER_X += PLAYER_X_SPEED*PLAYER_DIR;
                }
                PLAYER_ACTUAL_FRAME = (PLAYER_ACTUAL_FRAME + 1) % 10;

                if (PLAYER_DIR == 1 && TILES[PLAYER_X2][PLAYER_Y2] == TileType.FixedBlock)
                {
                    if (bb.ImageRectCollide(PLAYER_RIGHT, PLAYER_X, PLAYER_Y, PLAYER_ACTUAL_FRAME, PLAYER_X2 * 64, PLAYER_Y2 * 48, 64, 48) == bb.BBTRUE)
                    {
                        PLAYER_DIR *= -1;
                    }
                }
                else if (PLAYER_DIR == -1 && TILES[PLAYER_X1][PLAYER_Y2] == TileType.FixedBlock)
                {
                    if (bb.ImageRectCollide(PLAYER_RIGHT, PLAYER_X, PLAYER_Y, PLAYER_ACTUAL_FRAME, PLAYER_X1 * 64, PLAYER_Y2 * 48, 64, 48) == bb.BBTRUE)
                    {
                        PLAYER_DIR *= -1;
                    }
                }
                else {
                    CalculatePlayerPositionInMatrix();
                }
                
            }
            else { //  IF PLAYER IS IN AIR
                if (ValidPosition(PLAYER_X1, PLAYER_Y2 + 1))
                { // IF THERE ARE TILES BELOW HIM
                    if (TILES[PLAYER_X1][PLAYER_Y2 + 1] != TileType.Empty || TILES[PLAYER_X2][PLAYER_Y2 + 1] != TileType.Empty) // IF HE CAN CONTINUE FALLING
                    {
                        if (TILES[PLAYER_X2][PLAYER_Y2 + 1] == TileType.FixedBlock && PLAYER_DIR == 1 && bb.ImageRectCollide(PLAYER_FLYING_LEFT, BACKGROUND_L_X + PLAYER_X, PLAYER_Y, 0, BACKGROUND_L_X + PLAYER_X2 * 64, (PLAYER_Y2 + 1) * 48, 64, 48) == bb.BBTRUE)
                        {
                            PLAYER_ON_GROUND = true;
                            PLAYER_Y = (PLAYER_Y2 + 1) * 48 - 96 + bb.ImageYHandle(PLAYER_RIGHT);
                            CalculatePlayerPositionInMatrix();
                        }
                        else if (TILES[PLAYER_X1][PLAYER_Y2 + 1] == TileType.FixedBlock && PLAYER_DIR == -1 && bb.ImageRectCollide(PLAYER_FLYING_LEFT, BACKGROUND_L_X + PLAYER_X, PLAYER_Y, 0, BACKGROUND_L_X + PLAYER_X1 * 64, (PLAYER_Y2 + 1) * 48, 64, 48) == bb.BBTRUE)
                        {
                            PLAYER_ON_GROUND = true;
                            PLAYER_Y = (PLAYER_Y2 + 1) * 48 - 96 + bb.ImageYHandle(PLAYER_LEFT);
                            CalculatePlayerPositionInMatrix();
                        }
                        
                    }
                    PLAYER_Y += PLAYER_Y_SPEED;
                    CalculatePlayerPositionInMatrix();
                }
                else { // IF JUST THE FLOOR IS BELOW HIM
                    // HE REACHES THE FLOOR
                    if (PLAYER_DIR == 1 && bb.ImageRectOverlap(PLAYER_FLYING_RIGHT, BACKGROUND_L_X + PLAYER_X, PLAYER_Y, BACKGROUND_L_X, HEIGHT-96, WIDTH*3, 20) == bb.BBTRUE)
                    { 
                        PLAYER_ON_GROUND = true;
                        PLAYER_Y = (HEIGHT - 96) - 96 + bb.ImageYHandle(PLAYER_LEFT);
                    }
                    else if (PLAYER_DIR == -1 && bb.ImageRectOverlap(PLAYER_FLYING_LEFT, BACKGROUND_L_X + PLAYER_X, PLAYER_Y, BACKGROUND_L_X, HEIGHT - 96, WIDTH * 3, 20) == bb.BBTRUE)
                    {
                        PLAYER_ON_GROUND = true;
                        PLAYER_Y = (HEIGHT - 96) - 96 + bb.ImageYHandle(PLAYER_LEFT);
                    }
                    else { // STILL FALLING
                        PLAYER_Y += PLAYER_Y_SPEED;
                        CalculatePlayerPositionInMatrix();
                    }
                }
            }
            
        }

        private static bool ValidPosition(int x, int y) 
        {
            return x > 0 && x < TILES.Length && y > 0 && y < TILES[0].Length;
        }

        private static void updateBackground()
        {
            // ---- MOUSE EVENTS -----------------------
            if (bb.MouseDown(bb.MOUSE_BUTTON) == bb.BBTRUE)
            {
                processLeftClick();
            }
            else if (bb.MouseDown(bb.MOUSE_RIGHTBUTTON) == bb.BBTRUE)
            {
                processRightClick();
            }

            keyEvents();
            if (bb.GetKey() == 32)
            {
                ACTUAL_TILE_TYPE = (ACTUAL_TILE_TYPE + 1) % POSSIBLE_TYPES.Length;
                bb.FlushKeys();
            }
        }

        private static void keyEvents()
        {
            // ---- KEYBOARD EVENTS --------------------
            if (bb.KeyDown(bb.KEY_RIGHT) == bb.BBTRUE)
            {
                if ((BACKGROUND_R_X + BACKGROUND_WIDTH) - BACKGROUND_SCROLL_SPEED >= WIDTH)
                {
                    BACKGROUND_L_X -= BACKGROUND_SCROLL_SPEED;
                    BACKGROUND_C_X -= BACKGROUND_SCROLL_SPEED;
                    BACKGROUND_R_X -= BACKGROUND_SCROLL_SPEED;
                }
                else
                {
                    BACKGROUND_L_X = WIDTH - (BACKGROUND_WIDTH * 3);
                    BACKGROUND_C_X = WIDTH - (BACKGROUND_WIDTH * 2);
                    BACKGROUND_R_X = WIDTH - BACKGROUND_WIDTH;
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
                    BACKGROUND_C_X = BACKGROUND_WIDTH;
                    BACKGROUND_R_X = BACKGROUND_WIDTH * 2;
                }
            }
        }

        private static void drawBackground()
        {
            drawBackgroundImages();
            drawBackgroundTiles();
            drawHUD();
        }

        private static void drawHUD()
        {
            bb.DrawImage(HUD, 0, HEIGHT - 96);
            
            switch (POSSIBLE_TYPES[ACTUAL_TILE_TYPE])
            {
                case TileType.FixedBlock:
                    {
                        bb.DrawImage(BLOCK, 610, HEIGHT - 70);
                    } break;
                case TileType.RedGem:
                    {
                        bb.DrawImage(RED_GEM, 630, HEIGHT - 48);
                    } break;
                case TileType.GreenGem:
                    {
                        bb.DrawImage(GREEN_GEM, 630, HEIGHT - 48);
                    } break;
                case TileType.BlueGem:
                    {
                        bb.DrawImage(BLUE_GEM, 630, HEIGHT - 48);
                    } break;
                case TileType.Exit:
                    {
                        bb.DrawImage(EXIT, 610, HEIGHT - 70);
                    } break;
            }
        }


        private static void drawBackgroundTiles()
        {
            for (int column = 0; column < TILES.Length; column++ )
            {
                for (int row = 0; row < TILES[0].Length; row++) { 
                    switch(TILES[column][row])
                    {
                        case TileType.FixedBlock: {
                            bb.DrawImage(BLOCK, BACKGROUND_L_X + 64 * column, 48 * row); 
                        } break;
                        case TileType.RedGem: {
                            bb.DrawImage(RED_GEM, BACKGROUND_L_X + 64 * column +32, 48 * row +24);
                        } break;
                        case TileType.GreenGem: {
                            bb.DrawImage(GREEN_GEM, BACKGROUND_L_X + 64 * column+32, 48 * row+24);
                        } break;
                        case TileType.BlueGem: {
                            bb.DrawImage(BLUE_GEM, BACKGROUND_L_X + 64 * column+32, 48 * row+24);
                        } break;
                        case TileType.Exit: {
                            bb.DrawImage(EXIT, BACKGROUND_L_X + 64 * column, 48 * row);
                        } break;
                    }
                }
            }
        }

        private static void drawBackgroundImages()
        {
            bb.DrawBlock(BACKGROUND_L, BACKGROUND_L_X, 0);
            bb.DrawBlock(BACKGROUND_C, BACKGROUND_C_X, 0);
            bb.DrawBlock(BACKGROUND_R, BACKGROUND_R_X, 0);
        }
    }

    enum TileType{
        FixedBlock, BlueGem, GreenGem, RedGem, Empty, Exit
    }
}
