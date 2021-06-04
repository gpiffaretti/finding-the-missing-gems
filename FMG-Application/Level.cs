using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using bb = Blitz3DSDK;
using System.IO;
using System.Timers;

namespace FMG_Application
{
    public class Level
    {
        public int InitialTime { get; set; }
        private int intTimeLeft;
        public TimeSpan TimeLeft { get; set; }
        public DateTime TimeController { get; set; }
        public int Left_Blue_Gems { get; set; }
        public int Left_Red_Gems { get; set; }
        public int Blue_Gems { get; set; }
        public int Red_Gems { get; set; }
        public int Green_Gems { get; set; }
        public string Name { get; set; }
        public Tile_Type[][] Matrix { get; set; }

        private int FIXEDBLOCK;
        private int GREENGEM;
        private int BLUEGEM;
        private int REDGEM;
        private int EXIT;
        private int RIGHTRAMP;
        private int LEFTRAMP;
        private int BLOCK;
        private int ACTUAL_STAGE;
        private Stage[] STAGES;

        private int SOUND_EXIT = 0;
        private int SOUND_GEM = 0;
        private int SOUND_MUSIC = 0;
        private int SOUND_POWER_UP = 0;

        private int CHANNEL_MUSIC = 0;
        private int CHANNEL_POWER_UP = 0;


        //---------------------------------------------------------------------------------
        /// <summary>
        ///  constructor
        /// </summary>
        public Level()
        {
            searchForLevels();
            ACTUAL_STAGE = -1;
            nextStage();
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        ///  Loads and sets the next stage
        /// </summary>
        public void nextStage()
        {
            ACTUAL_STAGE++;
            Stage s = STAGES[ACTUAL_STAGE];
            InitialTime = s.time;
            intTimeLeft = InitialTime;
            Name = "Stage " + ACTUAL_STAGE;
            loadLevel(s.stageFile.Name);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Reloads stage
        /// </summary>
        public void reloadStage()
        {
            loadLevel(STAGES[ACTUAL_STAGE].stageFile.Name);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Starts actual stage timer
        /// </summary>
        public void startTimer()
        {
            TimeLeft = new TimeSpan(0, 0, InitialTime);
            TimeController = DateTime.Now;
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Returns true if there is another stage available
        /// </summary>
        /// <returns></returns>
        public bool nextStageAvailable()
        {
            return ACTUAL_STAGE + 1 < STAGES.Length;
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Updates the stage timer
        /// </summary>
        public void updateTimer()
        {
            DateTime now = DateTime.Now;
            TimeLeft = TimeLeft - (now - TimeController);
            intTimeLeft = Convert.ToInt32(TimeLeft.TotalSeconds);
            if (intTimeLeft == 6 && bb.ChannelPlaying(CHANNEL_POWER_UP) == bb.BBFALSE)
                StartPowerUp();

            if (intTimeLeft > 6 && bb.ChannelPlaying(CHANNEL_POWER_UP) == bb.BBTRUE)
                StopPowerUp();
            TimeController = now;
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Searches for .lvl files and sorts them by stage number
        /// </summary>
        public void searchForLevels()
        {
            DirectoryInfo d = new DirectoryInfo(System.Environment.CurrentDirectory);
            FileInfo[] levels = d.GetFiles("*.lvl");
            STAGES = new Stage[levels.Length];
            List<Stage> stageList = new List<Stage>();
            foreach (FileInfo f in levels)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(f.Name);
                XmlNodeList nodeList;
                nodeList = xmlDoc.GetElementsByTagName("scenery");
                XmlNode node = nodeList.Item(0);
                int stage = int.Parse(node.Attributes.Item(0).Value);
                int time = int.Parse(node.Attributes.Item(1).Value);
                stageList.Add(new Stage(f, stage, time));
            }
            stageList.Sort();
            stageList.CopyTo(STAGES);
        }


        //---------------------------------------------------------------------------------
        /// <summary>
        /// Loads sounds
        /// </summary>
        public void loadSounds()
        {
            SOUND_EXIT = bb.LoadSound("Resources/Sounds/ExitReached.ogg");
            SOUND_GEM = bb.LoadSound("Resources/Sounds/GemCollected.ogg");
            SOUND_MUSIC = bb.LoadSound("Resources/Sounds/Music.ogg");
            SOUND_POWER_UP = bb.LoadSound("Resources/Sounds/PowerUp.ogg");

            if (SOUND_MUSIC != 0)
                bb.LoopSound(SOUND_MUSIC);

            if (SOUND_POWER_UP !=0)
            {
                CHANNEL_POWER_UP = bb.PlaySound(SOUND_POWER_UP);
                bb.StopChannel(CHANNEL_POWER_UP);
            }
        }


        //---------------------------------------------------------------------------------
        /// <summary>
        /// Starts exit sound
        /// </summary>
        public void StartExitSound()
        {
            bb.PlaySound(SOUND_EXIT);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Starts GemCollected sound
        /// </summary>
        public void StartGemSound()
        {
            bb.PlaySound(SOUND_GEM);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Starts Music
        /// </summary>
        public void StartMusic()
        {
            CHANNEL_MUSIC = bb.PlaySound(SOUND_MUSIC);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Starts Power up sound
        /// </summary>
        public void StartPowerUp()
        {
            CHANNEL_POWER_UP = bb.PlaySound(SOUND_POWER_UP);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Stops music
        /// </summary>
        public void StopMusic()
        {
            bb.StopChannel(CHANNEL_MUSIC);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Stops Power up sound
        /// </summary>
        public void StopPowerUp()
        {
            bb.StopChannel(CHANNEL_POWER_UP);
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Loads images
        /// </summary>
        public void LoadImages()
        {
            FIXEDBLOCK = bb.LoadImage("Resources/Tiles/Block0.png");
            BLOCK = bb.LoadImage("Resources/Tiles/Block1.png");
            RIGHTRAMP = bb.LoadImage("Resources/Tiles/Block3.png");
            LEFTRAMP = bb.LoadImage("Resources/Tiles/Block2.png");
            EXIT = bb.LoadImage("Resources/Tiles/Exit.png");

            BLUEGEM = bb.LoadImage("Resources/Gems/blue_Gem.png");
            GREENGEM = bb.LoadImage("Resources/Gems/Green_Gem.png");
            REDGEM = bb.LoadImage("Resources/Gems/Red_Gem.png");

            if (FIXEDBLOCK != 0)
            {
                bb.MaskImage(FIXEDBLOCK, 255, 255, 255);
            }
            if (BLOCK != 0)
            {
                bb.MaskImage(BLOCK, 255, 255, 255);
            }
            if (LEFTRAMP != 0)
            {
                bb.MaskImage(LEFTRAMP, 255, 255, 255);
            }
            if (RIGHTRAMP != 0)
            {
                bb.MaskImage(RIGHTRAMP, 255, 255, 255);
            }
            if (EXIT != 0)
            {
                bb.MaskImage(EXIT, 0, 0, 0);
            }
            if (REDGEM != 0)
            {
                bb.MaskImage(REDGEM, 255, 255, 255);
                bb.MidHandle(REDGEM);
            }
            if (BLUEGEM != 0)
            {
                bb.MaskImage(BLUEGEM, 255, 255, 255);
                bb.MidHandle(BLUEGEM);
            }
            if (GREENGEM != 0)
            {
                bb.MaskImage(GREENGEM, 255, 255, 255);
                bb.MidHandle(GREENGEM);
            }
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Frees images
        /// </summary>
        public void FreeImages()
        {
            bb.FreeImage(BLOCK);
            bb.FreeImage(FIXEDBLOCK);
            bb.FreeImage(REDGEM);
            bb.FreeImage(BLUEGEM);
            bb.FreeImage(GREENGEM);
            bb.FreeImage(EXIT);
            bb.FreeImage(RIGHTRAMP);
            bb.FreeImage(LEFTRAMP);
        }


        //---------------------------------------------------------------------------------
        /// <summary>
        /// Draws stage tiles
        /// </summary>
        /// <param name="origin"></param>
        public void Draw(int origin)
        {
            for (int column = 0; column < Matrix.Length; column++)
            {
                for (int row = 0; row < Matrix[0].Length; row++)
                {
                    switch (Matrix[column][row])
                    {
                        case Tile_Type.FixedBlock:
                            {
                                bb.DrawImage(FIXEDBLOCK, origin + 64 * column, 48 * row);
                            } break;
                        case Tile_Type.RedGem:
                            {
                                bb.DrawImage(REDGEM, origin + 64 * column + 32, 48 * row + 24);
                            } break;
                        case Tile_Type.GreenGem:
                            {
                                bb.DrawImage(GREENGEM, origin + 64 * column + 32, 48 * row + 24);
                            } break;
                        case Tile_Type.BlueGem:
                            {
                                bb.DrawImage(BLUEGEM, origin + 64 * column + 32, 48 * row + 24);
                            } break;
                        case Tile_Type.Block:
                            {
                                bb.DrawImage(BLOCK, origin + 64 * column, 48 * row);
                            } break;
                        case Tile_Type.LeftRamp:
                            {
                                bb.DrawImage(LEFTRAMP, origin + 64 * column, 48 * row);
                            } break;
                        case Tile_Type.RightRamp:
                            {
                                bb.DrawImage(RIGHTRAMP, origin + 64 * column, 48 * row);
                            } break;
                        case Tile_Type.Exit:
                            {
                                bb.DrawImage(EXIT, origin + 64 * column, 48 * row);
                            } break;
                    }
                }
            }
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Returns actual stage's number
        /// </summary>
        /// <returns></returns>
        public int currentStage()
        {
            return ACTUAL_STAGE + 1;
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Returns time left
        /// </summary>
        /// <returns></returns>
        public int timeLeft()
        {
            return intTimeLeft;
        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Loads level from .lvl file
        /// </summary>
        /// <param name="path"></param>
        public void loadLevel(string path)
        {
            Blue_Gems = 0;
            Red_Gems = 0;
            Green_Gems = 0;
            Left_Blue_Gems = 0;
            Left_Red_Gems = 0;


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNodeList nodeList;
            nodeList = xmlDoc.GetElementsByTagName("tile");

            Matrix = new Tile_Type[60][];
            for (int x = 0; x < Matrix.Length; x++)
            {
                Matrix[x] = new Tile_Type[13];
                for (int y = 0; y < Matrix[0].Length; y++)
                {
                    Matrix[x][y] = Tile_Type.Empty;
                }
            }

            foreach (XmlNode node in nodeList)
            {
                string type = node.Attributes.Item(0).Value;
                int posX = int.Parse(node.Attributes.Item(1).Value);
                int posY = int.Parse(node.Attributes.Item(2).Value);
                Tile_Type t = 0;
                switch (type)
                {
                    case "FixedBlock":
                        {
                            t = Tile_Type.FixedBlock;
                        } break;
                    case "Exit":
                        {
                            t = Tile_Type.Exit;
                        } break;
                    case "RedGem":
                        {
                            t = Tile_Type.RedGem;
                            Red_Gems++;
                            Left_Red_Gems++;
                        } break;
                    case "GreenGem":
                        {
                            t = Tile_Type.GreenGem;
                            Green_Gems++;
                        } break;
                    case "BlueGem":
                        {
                            t = Tile_Type.BlueGem;
                            Blue_Gems++;
                            Left_Blue_Gems++;
                        } break;
                    default:
                        {
                            t = Tile_Type.Empty;
                        } break;
                }
                Matrix[posX][posY] = t;
            }

        }

        //---------------------------------------------------------------------------------
        /// <summary>
        /// Inner class Stage
        /// Knows its stage number, the .lvl file from where to load the stage, and the initial time
        /// </summary>
        private class Stage : IComparable
        {
            public FileInfo stageFile;
            public int stageNumber;
            public int time;
            public Stage(FileInfo f, int sNumber, int t)
            {
                stageFile = f;
                stageNumber = sNumber;
                time = t;
            }
            public int CompareTo(object obj)
            {
                if (obj is Stage)
                {
                    return this.stageNumber - (obj as Stage).stageNumber;
                }
                else return 0;
            }
        }
    }

    //---------------------------------------------------------------------------------
    /// <summary>
    /// Represents the different tile types possible in the game
    /// </summary>
    public enum Tile_Type
    {
        RedGem, GreenGem, BlueGem, FixedBlock, LeftRamp, RightRamp, Block, Exit, Empty
    }
}
