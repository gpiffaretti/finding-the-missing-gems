using System;
using bb = Blitz3DSDK;

namespace FMG_Application
{
    public class Player
    {
        public int X_Position { get; set; }
        public int Y_Position { get; set; }
        private int Matrix_X { get; set; }
        private int Matrix_Y { get; set; }
        private int X1 { get; set; }
        private int X2 { get; set; }
        private int Y1 { get; set; }
        private int Y2 { get; set; }
        public int Direction_X { get; set; }
        public int Direction_Y { get; set; }
        private int Frame { get; set; }
        private int X_Speed { get; set; }
        private int Y_Speed { get; set; }
        private int Falling_Speed { get; set; }
        private int Image { get; set; }
        private bool On_Ground { get; set; }
        private bool Climbing { get; set; }
        private Level level;
        public bool ON_EXIT { get; set; }
        public int Points { get; set; }


        //---------------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        public Level Level
        {
            get { return level; }
            set { level = value; Matrix = level.Matrix; }
        }

        private Tile_Type[][] Matrix { get; set; }
        public int Puntos { get; set; }

        private int Image_Run_Left = 0;
        private int Image_Run_Right = 0;
        private int Image_Run_Down_Left = 0;
        private int Image_Run_Down_Right = 0;
        private int Image_Run_Up_Left = 0;
        private int Image_Run_Up_Right = 0;
        private int Image_Idle = 0;
        private int Image_Air_Right = 0;
        private int Image_Air_Left = 0;

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Loads images
        /// </summary>
        public void LoadImages()
        {
            Image_Run_Right = bb.LoadAnimImage("Resources/Player/Run_Right.png", 96, 96, 0, 10);
            Image_Run_Left = bb.LoadAnimImage("Resources/Player/Run_Left.png", 96, 96, 0, 10);
            Image_Run_Down_Left = bb.LoadAnimImage("Resources/Player/Run_Down_Left.png", 96, 96, 0, 10);
            Image_Run_Down_Right = bb.LoadAnimImage("Resources/Player/Run_Down_Right.png", 96, 96, 0, 10);
            Image_Run_Up_Left = bb.LoadAnimImage("Resources/Player/Run_Up_Left.png", 96, 96, 0, 10);
            Image_Run_Up_Right = bb.LoadAnimImage("Resources/Player/Run_Up_Right.png", 96, 96, 0, 10);
            Image_Idle = bb.LoadAnimImage("Resources/Player/Idle.png", 96, 96, 0, 1);
            Image_Air_Left = bb.LoadAnimImage("Resources/Player/Jump.png", 96, 96, 7, 1);


            if (Image_Idle != 0)
            {
                bb.MaskImage(Image_Idle, 0, 0, 0);
                bb.MidHandle(Image_Idle);
            }
            if (Image_Air_Left != 0)
            {
                bb.MaskImage(Image_Air_Left, 0, 0, 0);
                bb.MidHandle(Image_Air_Left);
                Image_Air_Right = bb.CopyImage(Image_Air_Left);
                bb.ScaleImage(Image_Air_Right, -1, 1);
            }
            if (Image_Run_Right != 0)
            {
                bb.MaskImage(Image_Run_Right, 0, 0, 0);
                bb.MidHandle(Image_Run_Right);
            }
            if (Image_Run_Left != 0)
            {
                bb.MaskImage(Image_Run_Left, 0, 0, 0);
                bb.MidHandle(Image_Run_Left);
            }
            if (Image_Run_Down_Left != 0)
            {
                bb.MaskImage(Image_Run_Down_Left, 0, 0, 0);
                bb.MidHandle(Image_Run_Down_Left);
            }
            if (Image_Run_Down_Right != 0)
            {
                bb.MaskImage(Image_Run_Down_Right, 0, 0, 0);
                bb.MidHandle(Image_Run_Down_Right);
            }
            if (Image_Run_Up_Left != 0)
            {
                bb.MaskImage(Image_Run_Up_Left, 0, 0, 0);
                bb.MidHandle(Image_Run_Up_Left);
            }
            if (Image_Run_Up_Right != 0)
            {
                bb.MaskImage(Image_Run_Up_Right, 0, 0, 0);
                bb.MidHandle(Image_Run_Up_Right);
            }
            Image = Image_Air_Right;
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Free images
        /// </summary>
        public void FreeImages()
        {
            bb.FreeImage(Image_Run_Down_Left);
            bb.FreeImage(Image_Run_Down_Right);
            bb.FreeImage(Image_Run_Up_Left);
            bb.FreeImage(Image_Run_Up_Right);
            bb.FreeImage(Image_Run_Left);
            bb.FreeImage(Image_Run_Right);
            bb.FreeImage(Image_Air_Left);
            bb.FreeImage(Image_Air_Right);
            bb.FreeImage(Image_Idle);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Reset matrix
        /// </summary>
        public void resetMatrix()
        {
            Matrix = Level.Matrix;

        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Calculates player position in matrix
        /// </summary>
        public void CalculatePlayerPositionInMatrix()
        {
            int x = X_Position / 64;
            int x_rest = X_Position % 64;
            int y = Y_Position / 48;
            int y_rest = Y_Position % 48;
            if (x_rest >= 32 && x + 1 < Matrix.Length)
            {
                X1 = x;
                X2 = x + 1;
            }
            else
            {
                X1 = x - 1;
                X2 = x;
            }
            if (y_rest >= 24 && y + 1 < Matrix[0].Length)
            {
                Y1 = y;
                Y2 = y + 1;
            }
            else
            {
                Y1 = y - 1;
                Y2 = y;
            }

        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Returns true if (X, Y) is a valid position in the matrix
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool ValidPosition(int x, int y)
        {
            return x > 0 && x < Matrix.Length && y > 0 && y < Matrix[0].Length;
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Returns true if eval is between min and max
        /// </summary>
        /// <param name="eval"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private bool Between(int eval, int min, int max)
        {
            return eval >= min && eval <= max;
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Check for collitions with gems and exit
        /// </summary>
        private void CheckForGemsAndExit()
        {
            if (IsAGemOrExit(X1, Y1) && Between(X_Position, X1*64+5, X1*64+59) && Between(Y_Position, Y1*48, Y1*48+48))
            {
                switch (level.Matrix[X1][Y1])
                {
                    case Tile_Type.BlueGem:
                        level.Left_Blue_Gems--;
                        level.Matrix[X1][Y1] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.GreenGem:
                        level.TimeLeft = level.TimeLeft.Add(new TimeSpan(0, 0, 0, 10));
                        level.Matrix[X1][Y1] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.RedGem:
                        level.Left_Red_Gems--;
                        level.Matrix[X1][Y1] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.Exit:
                        ON_EXIT = true;
                        level.StartExitSound();
                        break;
                }
            }
            if (IsAGemOrExit(X1, Y2) && Between(X_Position, X1*64 + 5, X1*64 + 59) && Between(Y_Position, Y2*48, Y2*48 + 48))
            {
                switch (level.Matrix[X1][Y2])
                {
                    case Tile_Type.BlueGem:
                        level.Left_Blue_Gems--;
                        level.Matrix[X1][Y2] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.GreenGem:
                        level.TimeLeft = level.TimeLeft.Add(new TimeSpan(0, 0, 0, 10));
                        level.Matrix[X1][Y2] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.RedGem:
                        level.Left_Red_Gems--;
                        level.Matrix[X1][Y2] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.Exit:
                        ON_EXIT = true;
                        level.StartExitSound();
                        break;
                }
            }
            if (IsAGemOrExit(X2, Y1) && Between(X_Position, X2*64 + 5, X2*64 + 59) && Between(Y_Position, Y1*48, Y1*48 + 48))
            {
                switch (level.Matrix[X2][Y1])
                {
                    case Tile_Type.BlueGem:
                        level.Left_Blue_Gems--;
                        level.Matrix[X2][Y1] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.GreenGem:
                        level.TimeLeft = level.TimeLeft.Add(new TimeSpan(0, 0, 0, 10));
                        level.Matrix[X2][Y1] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.RedGem:
                        level.Left_Red_Gems--;
                        level.Matrix[X2][Y1] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.Exit:
                        ON_EXIT = true;
                        level.StartExitSound();
                        break;
                }
            }
            if (IsAGemOrExit(X2, Y2) && Between(X_Position, X2*64 + 5, X2*64 + 59) && Between(Y_Position, Y2*48, Y2*48 + 48))
            {
                switch (level.Matrix[X2][Y2])
                {
                    case Tile_Type.BlueGem:
                        level.Left_Blue_Gems--;
                        level.Matrix[X2][Y2] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.GreenGem:
                        level.TimeLeft = level.TimeLeft.Add(new TimeSpan(0, 0, 0, 10));
                        level.Matrix[X2][Y2] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.RedGem:
                        level.Left_Red_Gems--;
                        level.Matrix[X2][Y2] = Tile_Type.Empty;
                        level.StartGemSound();
                        break;
                    case Tile_Type.Exit:
                        ON_EXIT = true;
                        level.StartExitSound();
                        break;
                }
            }
            
        }


        //---------------------------------------------------------------------------------
        /// <summary>
        /// Updates player position and state(running, falling and climbing)
        /// </summary>
        /// <param name="origin"></param>
        public void Update(int origin)
        {
            CheckForGemsAndExit();
            if (On_Ground && !Climbing) // IF PLAYER IS ON GROUND
            {
                #region GRAVITY
                if (ValidPosition(X1, Y2 + 1)) // WALKING OVER TILE ROW
                {

                    if (Direction_X == 1 && (Matrix[X2][Y2 + 1] == Tile_Type.Empty || IsAGemOrExit(X2, Y2 + 1)) && X_Position > (X2 * 64))
                    {
                        On_Ground = false;
                        Image = Image_Air_Right;
                        Frame = 0;
                        X_Position += 3 * X_Speed * Direction_X;
                    }
                    else if (Direction_X == -1 && (Matrix[X1][Y2 + 1] == Tile_Type.Empty || IsAGemOrExit(X1, Y2 + 1)) && X_Position < (X2 * 64))
                    {
                        On_Ground = false;
                        Image = Image_Air_Left;
                        Frame = 0;
                        X_Position += 3 * X_Speed * Direction_X;
                    }
                    else
                    {
                        X_Position += X_Speed * Direction_X;
                    }
                }
                else // WALKING OVER FLOOR
                {
                    X_Position += X_Speed * Direction_X;
                }
                #endregion

                // COLLIDE WITH RIGHT OBSTACLE
                if (Direction_X == 1 && (Matrix[X2][Y2] == Tile_Type.RightRamp || Matrix[X2][Y2] == Tile_Type.Block || Matrix[X2][Y2] == Tile_Type.FixedBlock || Matrix[X2][Y1] == Tile_Type.FixedBlock) && X_Position > X1 * 64 + 32) // COLLIDES WITH RIGHT OBSTACLE
                {
                    Direction_X *= -1;
                    Image = Image_Run_Left;
                }

                // COLLIDE WITH LEFT OBSTACLE
                else if (Direction_X == -1 && ((Matrix[X1][Y2] == Tile_Type.LeftRamp || Matrix[X1][Y2] == Tile_Type.Block || Matrix[X1][Y2] == Tile_Type.FixedBlock) || Matrix[X1][Y1] == Tile_Type.FixedBlock) && X_Position < X2 * 64 + 32) // COLLIDES WITH LEFT OBSTACLE
                {
                    Direction_X *= -1;
                    Image = Image_Run_Right;
                }

                    // START CLIMBING RIGHT UP
                else if (Direction_X == 1 && Matrix[X2][Y2] == Tile_Type.LeftRamp && X_Position > X2 * 64)
                {
                    Climbing = true;
                    X_Position = X2 * 64;
                    Y_Position -= 9;
                    Direction_Y = -1;
                    Image = Image_Run_Up_Right;
                }
                // START CLIMBING LEFT UP
                else if (Direction_X == -1 && Matrix[X1][Y2] == Tile_Type.RightRamp && X_Position < X2 * 64)
                {
                    Climbing = true;
                    X_Position = X2 * 64;
                    Y_Position -= 9;
                    Direction_Y = -1;
                    Image = Image_Run_Up_Left;
                }
                // START CLIMBING RIGHT DOWN
                else if (Direction_X == 1 && ValidPosition(X2, Y2 + 1) && Matrix[X2][Y2 + 1] == Tile_Type.RightRamp && X_Position > X2 * 64 + 20)
                {
                    Climbing = true;
                    X_Position = X2 * 64 + 32;
                    Y_Position += 9;
                    Direction_Y = 1;
                    Image = Image_Run_Down_Right;
                }
                // START CLIMBING LEFT DOWN
                else if (Direction_X == -1 && ValidPosition(X1, Y2 + 1) && Matrix[X1][Y2 + 1] == Tile_Type.LeftRamp && X_Position < X2 * 64 - 20)
                {
                    Climbing = true;
                    X_Position = X2 * 64 - 32;
                    Y_Position += 9;
                    Direction_Y = 1;
                    Image = Image_Run_Down_Left;
                }

                CalculatePlayerPositionInMatrix();
                if (On_Ground) Frame = (Frame + 1) % 10;
            }
            else if (On_Ground && Climbing)
            {
                // CLIMBING RIGHT UP
                if (Direction_X == 1 && Direction_Y == -1 && (Matrix[X2][Y2] == Tile_Type.Block || Matrix[X2][Y2] == Tile_Type.FixedBlock || Matrix[X2][Y2] == Tile_Type.RightRamp) && X_Position > X2 * 64 - 32) //
                {
                    Direction_X *= -1;
                    Direction_Y *= -1;
                    Image = Image_Run_Down_Left;

                }
                else if (Direction_X == 1 && Direction_Y == -1 && (Matrix[X2][Y2] == Tile_Type.Empty || IsAGemOrExit(X2, Y2)) && ValidPosition(X2, Y2 + 1) && (Matrix[X2][Y2 + 1] == Tile_Type.Block || Matrix[X2][Y2 + 1] == Tile_Type.FixedBlock) && X_Position > X2 * 64 - 30 || IsAGemOrExit(X2, Y2))
                {
                    Climbing = false;
                    Image = Image_Run_Right;
                    X_Position = X2 * 64;
                    Y_Position = Y2 * 48;
                }
                else if (Direction_X == 1 && Direction_Y == -1 && (Matrix[X2][Y2] == Tile_Type.Empty || IsAGemOrExit(X2, Y2)) && ValidPosition(X2, Y2 + 1) && (Matrix[X2][Y2 + 1] == Tile_Type.Empty || IsAGemOrExit(X2, Y2 + 1)) && X_Position > X2 * 64)
                {
                    Climbing = false;
                    On_Ground = false;
                    Image = Image_Air_Right;
                    Frame = 0;
                    X_Position += X_Speed * 2;
                }

                // CLIMBING RIGHT DOWN
                else if (Direction_X == 1 && Direction_Y == 1 && (Matrix[X2][Y2] == Tile_Type.Block || Matrix[X2][Y2] == Tile_Type.FixedBlock || Matrix[X2][Y2] == Tile_Type.RightRamp) && X_Position > X2 * 64 - 32)
                {
                    Direction_X *= -1;
                    Direction_Y *= -1;
                    Image = Image_Run_Up_Left;

                }
                else if (Direction_X == 1 && Direction_Y == 1 && ValidPosition(X2, Y2 + 1) && (Matrix[X2][Y2] == Tile_Type.Empty || IsAGemOrExit(X2, Y2)) &&  (Matrix[X2][Y2 + 1] == Tile_Type.Block || Matrix[X2][Y2 + 1] == Tile_Type.FixedBlock) && X_Position > X2 * 64 - 30)
                {
                    Climbing = false;
                    Image = Image_Run_Right;
                    X_Position += X_Speed * Direction_X;
                    Y_Position = Y2 * 48;
                }
                else if (Direction_X == 1 && Direction_Y == 1 && ValidPosition(X2, Y2 + 1) && (Matrix[X2][Y2] == Tile_Type.Empty || IsAGemOrExit(X2, Y2)) && Matrix[X2][Y2 + 1] == Tile_Type.Empty && X_Position > X2 * 64 )
                {
                    Climbing = false;
                    On_Ground = false;
                    Image = Image_Air_Right;
                    Frame = 0;
                    X_Position += X_Speed *2;
                }
                else if (Direction_X == 1 && Direction_Y == 1 && Y_Position > Matrix[0].Length * 48 - 48)
                {
                    Climbing = false;
                    Image = Image_Run_Right;
                    X_Position = X2 * 64;
                    Y_Position = Y2 * 48;
                }
                // CLIMBING LEFT UP
                else if (Direction_X == -1 && Direction_Y == -1 && (Matrix[X1][Y2] == Tile_Type.Block || Matrix[X1][Y2] == Tile_Type.FixedBlock || Matrix[X1][Y2] == Tile_Type.LeftRamp) && X_Position < X2 * 64 + 32) //
                {
                    Direction_X *= -1;
                    Direction_Y *= -1;
                    Image = Image_Run_Down_Right;
                }
                else if (Direction_X == -1 && Direction_Y == -1 && (Matrix[X1][Y2] == Tile_Type.Empty || IsAGemOrExit(X1, Y2)) && ValidPosition(X1, Y2 + 1) && (Matrix[X1][Y2 + 1] == Tile_Type.Block || Matrix[X1][Y2 + 1] == Tile_Type.FixedBlock) && X_Position < X2 * 64 + 30)
                {
                    Climbing = false;
                    Image = Image_Run_Left;
                    X_Position = X2 * 64;
                    Y_Position = Y2 * 48;
                }
                else if (Direction_X == -1 && Direction_Y == -1 && (Matrix[X1][Y2] == Tile_Type.Empty || IsAGemOrExit(X1, Y2)) && ValidPosition(X1, Y2 + 1) && (Matrix[X1][Y2 + 1] == Tile_Type.Empty || IsAGemOrExit(X1, Y2 + 1)) && X_Position < X2 * 64)
                {
                    Climbing = false;
                    On_Ground = false;
                    Image = Image_Air_Left;
                    Frame = 0;
                    X_Position -= X_Speed * 2;
                }
                // CLIMBING LEFT DOWN
                else if (Direction_X == -1 && Direction_Y == 1 && (Matrix[X1][Y2] == Tile_Type.Block || Matrix[X1][Y2] == Tile_Type.FixedBlock || Matrix[X1][Y2] == Tile_Type.RightRamp) && X_Position < X2 * 64 + 32)
                {
                    Direction_X *= -1;
                    Direction_Y *= -1;
                    Image = Image_Run_Up_Right;

                }
                else if (Direction_X == -1 && Direction_Y == 1 && ValidPosition(X2, Y2 + 1) && (Matrix[X1][Y2] == Tile_Type.Empty || IsAGemOrExit(X1, Y2)) && (Matrix[X1][Y2 + 1] == Tile_Type.Block || Matrix[X1][Y2 + 1] == Tile_Type.FixedBlock) && X_Position < X2 * 64 + 30)
                {
                    Climbing = false;
                    Image = Image_Run_Left;
                    X_Position += X_Speed*Direction_X;
                    Y_Position = Y2 * 48;
                }
                else if (Direction_X == -1 && Direction_Y == 1 && ValidPosition(X1, Y2 + 1) && (Matrix[X1][Y2] == Tile_Type.Empty || IsAGemOrExit(X1, Y2)) && Matrix[X1][Y2 + 1] == Tile_Type.Empty && X_Position < X2 * 64)
                {
                    Climbing = false;
                    On_Ground = false;
                    Image = Image_Air_Left;
                    Frame = 0;
                    X_Position -= X_Speed * 2;
                }
                else if (Direction_X == -1 && Direction_Y == 1 && Y_Position > Matrix[0].Length * 48 - 48)
                {
                    Climbing = false;
                    Image = Image_Run_Left;
                    X_Position = X2 * 64;
                    Y_Position = Y2 * 48;
                }
                else
                {
                    X_Position += X_Speed * Direction_X;
                    Y_Position += Y_Speed * Direction_Y;

                }
                CalculatePlayerPositionInMatrix();
                if (On_Ground) Frame = (Frame + 1) % 10;
            }
            else
            { //  IF PLAYER IS IN AIR
                if (ValidPosition(X1, Y2 + 1))
                { // IF THERE ARE TILES BELOW HIM

                    if (Matrix[X1][Y2 + 1] != Tile_Type.Empty || Matrix[X2][Y2 + 1] != Tile_Type.Empty) // IF HE CAN CONTINUE FALLING
                    {
                        // FALL ON A SQUARE BLOCK
                        if ((Matrix[X2][Y2 + 1] == Tile_Type.FixedBlock || Matrix[X2][Y2 + 1] == Tile_Type.Block) && Direction_X == 1 && bb.ImageRectCollide(Image_Air_Right, origin + X_Position, Y_Position, 0, origin + X2 * 64, (Y2 + 1) * 48, 64, 48) == bb.BBTRUE)
                        {
                            On_Ground = true;
                            Y_Position = (Y2 + 1) * 48 - 96 + bb.ImageYHandle(Image);
                            Image = Image_Run_Right;
                        }
                        else if ((Matrix[X1][Y2 + 1] == Tile_Type.FixedBlock || Matrix[X1][Y2 + 1] == Tile_Type.Block) && Direction_X == 1 && bb.ImageRectCollide(Image_Air_Right, origin + X_Position, Y_Position, 0, origin + X1 * 64, (Y2 + 1) * 48, 64, 48) == bb.BBTRUE)
                        {
                            On_Ground = true;
                            Y_Position = (Y2 + 1) * 48 - 96 + bb.ImageYHandle(Image);
                            Image = Image_Run_Right;
                        }
                        else if ((Matrix[X1][Y2 + 1] == Tile_Type.FixedBlock || Matrix[X1][Y2 + 1] == Tile_Type.Block) && Direction_X == -1 && bb.ImageRectCollide(Image_Air_Left, origin + X_Position, Y_Position, 0, origin + X1 * 64, (Y2 + 1) * 48, 64, 48) == bb.BBTRUE)
                        {
                            On_Ground = true;
                            Y_Position = (Y2 + 1) * 48 - 96 + bb.ImageYHandle(Image);
                            Image = Image_Run_Left;
                        }
                        else if ((Matrix[X2][Y2 + 1] == Tile_Type.FixedBlock || Matrix[X2][Y2 + 1] == Tile_Type.Block) && Direction_X == -1 && bb.ImageRectCollide(Image_Air_Left, origin + X_Position, Y_Position, 0, origin + X2 * 64, (Y2 + 1) * 48, 64, 48) == bb.BBTRUE)
                        {
                            On_Ground = true;
                            Y_Position = (Y2 + 1) * 48 - 96 + bb.ImageYHandle(Image);
                            Image = Image_Run_Left;
                        }
                    }
                    if (!On_Ground) Y_Position += Falling_Speed;
                    CalculatePlayerPositionInMatrix();
                }
                else
                { // IF JUST THE FLOOR IS BELOW HIM
                    // HE REACHES THE FLOOR
                    if (Y_Position > Matrix[0].Length * 48 - 48)
                    {
                        On_Ground = true;
                        Y_Position = Matrix[0].Length * 48 - bb.ImageYHandle(Image);
                        if (Direction_X == 1) Image = Image_Run_Right;
                        else Image = Image_Run_Left;
                    }
                    else
                    { // STILL FALLING
                        Y_Position += Y_Speed;
                        CalculatePlayerPositionInMatrix();
                    }
                }
                // FALL ON A RAMP IN RIGHT DIRECTION
                if ((Matrix[X1][Y2] == Tile_Type.LeftRamp) && Direction_X == 1 && (X_Position > X1 * 64 && X_Position < X1 * 64 + 64 && Y_Position > Y2 * 48 - 20))
                {
                    On_Ground = true;
                    Climbing = true;
                    Image = Image_Run_Up_Right;
                    Y_Position = Y_Position - 20;
                    Direction_Y = -1;
                }
                if ((Matrix[X2][Y2] == Tile_Type.LeftRamp) && Direction_X == 1 && (X_Position > X2 * 64 && X_Position < X2 * 64 + 64 && Y_Position > Y2 * 48 - 20))
                {
                    On_Ground = true;
                    Climbing = true;
                    Image = Image_Run_Up_Right;
                    Y_Position = Y_Position - 20;
                    Direction_Y = -1;
                }
                if ((Matrix[X1][Y2] == Tile_Type.RightRamp) && Direction_X == 1 && (X_Position > X1 * 64 && X_Position < X1 * 64 + 64 && Y_Position > Y2 * 48 - 20))
                {
                    On_Ground = true;
                    Climbing = true;
                    Image = Image_Run_Down_Right;
                    Y_Position = Y_Position - 20;
                    Direction_Y = 1;
                }
                if ((Matrix[X2][Y2] == Tile_Type.RightRamp) && Direction_X == 1 && (X_Position > X2 * 64 && X_Position < X2 * 64 + 64 && Y_Position > Y2 * 48 - 20))
                {
                    On_Ground = true;
                    Climbing = true;
                    Image = Image_Run_Down_Right;
                    Y_Position = Y_Position - 20;
                    Direction_Y = 1;
                }
                // FALL ON A RAMP IN LEFT DIRECTION
                if ((Matrix[X1][Y2] == Tile_Type.LeftRamp) && Direction_X == -1 && (X_Position > X1 * 64 && X_Position < X1 * 64 + 64 && Y_Position > Y2 * 48 - 20))
                {
                    On_Ground = true;
                    Climbing = true;
                    Image = Image_Run_Down_Left;
                    Y_Position = Y_Position - 20;
                    Direction_Y = 1;
                }
                if ((Matrix[X2][Y2] == Tile_Type.LeftRamp) && Direction_X == -1 && (X_Position > X2 * 64 && X_Position < X2 * 64 + 64 && Y_Position > Y2 * 48 - 20))
                {
                    On_Ground = true;
                    Climbing = true;
                    Image = Image_Run_Down_Left;
                    Y_Position = Y_Position - 20;
                    Direction_Y = 1;
                }
                if ((Matrix[X1][Y2] == Tile_Type.RightRamp) && Direction_X == -1 && (X_Position > X1 * 64 && X_Position < X1 * 64 + 64 && Y_Position > Y2 * 48 - 20))
                {
                    On_Ground = true;
                    Climbing = true;
                    Image = Image_Run_Up_Left;
                    Y_Position = Y_Position - 20;
                    Direction_Y = -1;
                }
                if ((Matrix[X2][Y2] == Tile_Type.RightRamp) && Direction_X == -1 && (X_Position > X2 * 64 && X_Position < X2 * 64 + 64 && Y_Position > Y2 * 48 - 20))
                {
                    On_Ground = true;
                    Climbing = true;
                    Image = Image_Run_Up_Left;
                    Y_Position = Y_Position - 20;
                    Direction_Y = -1;
                }
            }
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// Return true if in (x,y) position of the matrix there's a gem or an exit tile
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsAGemOrExit(int x, int y)
        {
            return level.Matrix[x][y] == Tile_Type.BlueGem || level.Matrix[x][y] == Tile_Type.GreenGem ||
                   level.Matrix[x][y] == Tile_Type.RedGem || level.Matrix[x][y] == Tile_Type.Exit;
        }


        //----------------------------------------------------------------------------
        /// <summary>
        /// Draws player
        /// </summary>
        /// <param name="origen"></param>
        public void Draw(int origen)
        {
            bb.DrawImage(Image, X_Position + origen, Y_Position, Frame);

        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        public Player()
        {
            Image = Image_Air_Right;
            On_Ground = false;
            ON_EXIT = false;
            Climbing = false;
            X_Speed = 8;
            Y_Speed = 6;
            Falling_Speed = 10;
            Direction_X = 1;
            Direction_Y = 1;
        }



    }
}
