using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using InputFunctionality.KeyboardAdapter;
using InputFunctionality.MouseAdapter;
using PlanetSimulation.Interface;
using System;
using PlanetSimulation.PhysicHandler;
using PlanetSimulation.EngineComponents;

namespace PlanetSimulation
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlanetSim : Game
    {
        GraphicsDeviceManager graphics;
        public CollisionHandling CollisionHandler  { get; private set; }
        public GravityHandling GravityHandler { get; private set; }
        public MultiProcessingUnit MultiProcessing { get; private set; }

        Universe m_currentUniverse;
        Universe[] m_universes;
        public Universe CurrentUniverse
        {
            get { return m_currentUniverse; }
            set
            {
                Pause = true;
                Components.Clear();
                m_currentUniverse = value;
                Components.Add(m_currentUniverse);
                CamControl.Camera = m_currentUniverse.Camera;
            }
        }

        public GameTime CurrentGameTime { get; private set; }

        StatusText StatusText { get; set; }
        ManipulationMenu ManipulationMenu { get; set; }
        BuildMenu BuilderMenu { get; set; }
        public CameraControl CamControl { get; set; }

        public MouseAdvanced InputMouse { get; set; }
        public KeyboardAdvanced InputKeyboard { get; set; }

        bool m_pause;
        public bool Pause
        {
            get { return m_pause; }
            set 
            {
                m_pause = value;
                IsMouseVisible = m_pause;
            }
        }

        public SpriteBatch SpriteBatch { get; private set; }
        public SpriteFont DataFont { get; private set; }

        private int ScreenWidth { get; set; }
        private int ScreenHeight { get; set; }

        public PlanetSim(int maxUsedCores = 0, int screenWidth = -1, int screenHeight = -1, bool freeFPS = false) : base()
        {
            graphics = new GraphicsDeviceManager(this);
            InputKeyboard = new KeyboardAdvanced();
            InputMouse = new MouseAdvanced();

            GravityHandler = new GravityHandling();
            CollisionHandler = new CollisionHandling();
            MultiProcessing = new MultiProcessingUnit(this);

            Content.RootDirectory = "Content";
            Pause = true;
            GameGlobals.MaximumProcessorsUsed = maxUsedCores;

            m_universes = new Universe[GameGlobals.MaxUniverseCount];

#if DEBUG
            
#endif
            //graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = !freeFPS;

            if (screenWidth > 0 && screenHeight > 0)
            {
                ScreenWidth = screenWidth;
                ScreenHeight = screenHeight;
            }
        }

        public void CopyUniverse(Universe universe, int slot)
        {
            if (m_universes[slot] == universe)
                return;

            m_universes[slot] = universe.Copy(slot);
        }

        public void QuickLoadUniverse(int slot)
        {
            if (m_universes[slot] == null)
                m_universes[slot] = new Universe(this, slot);

            CurrentUniverse = m_universes[slot];
        }

        public void FileLoadUniverse(int slot)
        {
            Universe createdUniverse = FileAdapter.ReadUniverseFromFile(this, slot);
            if (createdUniverse != null)
            {
                CurrentUniverse = createdUniverse;
                m_universes[slot] = createdUniverse;
            }
        }
       
        public void ResetCurrentUniverse()
        {
            CurrentUniverse.Reset();
            Pause = true;
        }

        public void ResetBuildMenu()
        {
            BuilderMenu.Reset();
        }

        protected override void Initialize()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;
#if !DEBUG
            graphics.IsFullScreen = true;
#endif
            graphics.ApplyChanges();

            InputKeyboard.Initialize();
            InputMouse.Initialize();

            StatusText = new StatusText(this);
            ManipulationMenu = new ManipulationMenu(this);
            CamControl = new CameraControl(this);
            BuilderMenu = new BuildMenu(this);

            m_universes[0] = new Universe(this, 0);
            CurrentUniverse = m_universes[0];

            base.Initialize();
        }

        protected override void LoadContent()
        {
            DataFont = Content.Load<SpriteFont>("DataFont");

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public string GetDistributionName()
        {
            switch (MultiProcessing.Distribution)
            {
                case MultiProcessingUnit.DistributionMode.Sequence:
                    return "Sequence";
                case MultiProcessingUnit.DistributionMode.ParallelLoop:
                    return "Parallel Loop";
                case MultiProcessingUnit.DistributionMode.Modulo:
                    return "Modulo Split";
                case MultiProcessingUnit.DistributionMode.LockedRRT:
                    return "Locked RRT";
                case MultiProcessingUnit.DistributionMode.SyncedRRT:
                    return "Synchronized RRT";
                default:
                    return "Missing distribution name";
            }
        }

        protected override void Update(GameTime gameTime)
        {
            CurrentGameTime = gameTime;

            InputKeyboard.UpdateState();
            InputMouse.UpdateState();

            if (InputKeyboard.KeyNowPressed(Keys.I))
                StatusText.DrawStatus = !StatusText.DrawStatus;

            if (InputKeyboard.KeyNowPressed(Keys.H))
                StatusText.DrawControls = !StatusText.DrawControls;

            StatusText.Update();

            ManipulationMenu.Update();

            if (Pause)
            {
                PausedActions();
            }
            else
            {
                RunningActions();
            }

            CamControl.Update();
        }

        private void PausedActions()
        {
            if (InputKeyboard.KeyNowPressed(Keys.Escape))
                CurrentUniverse.ClearPlanets();

            BuilderMenu.Update();
        }

        private void RunningActions()
        {
            CurrentUniverse.UpdateUniverse();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(20,20,20));

            base.Draw(gameTime);

            StatusText.DrawStatusText();
            SpriteBatch.DrawString(DataFont, "© David Hofer", new Vector2(GameGlobals.PlanetDataListInspacer, GraphicsDevice.Viewport.Height - 40), Color.White);

            SpriteBatch.End();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            MultiProcessing.Close();
            base.OnExiting(sender, args);
        }

    }
}
