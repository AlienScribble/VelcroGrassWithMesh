using AlienScribble;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using VelcroPhysics.Collision.Filtering;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.Joints;
using VelcroPhysics.Factories;
using VelcroPhysics.Utilities;

using con = VelcroPhysics.Utilities.ConvertUnits;

namespace VelcroGrass
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice        gpu;
        SpriteBatch           spriteBatch;
        QuadBatch             quadBatch;        

        // ART 
        Rectangle gras1, gras2, gras3, BigGrass;
        Rectangle pixel;
        Texture2D tex;                             // sprite sheet 

        // PHYSICS
        World           world;
        FixedMouseJoint mouse_joint;        
        Body            player_body;
        Body            big_grass_body;
        //-- grass physics: 
        VelcroChain[]   grassChain;


        // C O N S T R U C T 
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth    = 1024,     PreferredBackBufferHeight = 768,   PreferredBackBufferFormat = SurfaceFormat.Color,
                PreferredDepthStencilFormat = DepthFormat.None,                            SynchronizeWithVerticalRetrace = true,
                GraphicsProfile             = GraphicsProfile.HiDef,
            };
            Content.RootDirectory = "Content";
        }


        // I N I T
        protected override void Initialize()
        {
            gpu         = GraphicsDevice;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            quadBatch   = new QuadBatch(Content, gpu, "QuadEffect", "", null, null); // ( if using a rendertarget for most rendering - change null to its resolution )

            pixel = new Rectangle(382, 0, 1, 1);     quadBatch.PIXEL = pixel;        // need this for line drawing to work in quadbatch
            gras1 = new Rectangle(12, 144, 28, 82);     // fixed these since last time
            gras2 = new Rectangle(80, 144, 28, 82);     // fixed these since last time
            gras3 = new Rectangle(142, 144, 28, 82);    // fixed these since last time
            BigGrass = new Rectangle(0, 0, 380, 122);

            world = new World(new Vector2(0f, 9.8f)); // world physics sim (provide gravity direction)          

            player_body = BodyFactory.CreateCircle(world, con.ToSimUnits(8.0f), 1.0f);
            player_body.Position    = con.ToSimUnits(400, 10);
            player_body.BodyType    = BodyType.Dynamic; // moves
            player_body.Mass        = 0.4f;
            player_body.Restitution = 0.2f;             // bounciness
            player_body.Friction    = 0.4f;             // grip
            player_body.CollisionCategories = Category.Cat1; player_body.CollidesWith = Category.All;
            player_body.FixedRotation = false;

            // attach collider_body to mouse: 
            mouse_joint = JointFactory.CreateFixedMouseJoint(world, player_body, player_body.Position);
            mouse_joint.MaxForce = 500.0f;


            // 大きな 草            
            big_grass_body = BodyFactory.CreateRectangle(world, con.ToSimUnits(BigGrass.Width*0.88f), con.ToSimUnits(BigGrass.Height*0.6f), 1.0f);
            big_grass_body.Position = con.ToSimUnits(300+BigGrass.Width/2f-10, 600+BigGrass.Height/2f);
            big_grass_body.BodyType = BodyType.Static;
            big_grass_body.Restitution = 0.2f;    // bounciness
            big_grass_body.Friction    = 0.8f;    // surface grip
            big_grass_body.Mass        = 1.0f;
            big_grass_body.CollisionCategories = Category.Cat2;
            big_grass_body.CollidesWith        = Category.Cat1;


            // DYNAMIC GRASS (BOTTOM PART): 
            grassChain = new VelcroChain[3];
            grassChain[0] = new VelcroChain(world, quadBatch);
            grassChain[0].MakeGrass(gras1.Width * 0.5f, gras1.Height, new Vector2(350, 600),num_divisions: 3, baseBody: big_grass_body, noGravity: false);
            grassChain[1] = new VelcroChain(world, quadBatch);
            grassChain[1].MakeGrass(gras1.Width * 0.5f, gras1.Height, new Vector2(380, 600), num_divisions: 3, baseBody: big_grass_body, noGravity: false);
            grassChain[2] = new VelcroChain(world, quadBatch);
            grassChain[2].MakeGrass(gras1.Width * 0.5f, gras1.Height, new Vector2(410, 600), num_divisions: 3, baseBody: big_grass_body, noGravity: false);

            base.Initialize();
        }

 
        // L O A D
        protected override void LoadContent()
        {
            tex = Content.Load<Texture2D>("grass");
        }        
        protected override void UnloadContent() {  }


        // U P D A T E
        Vector2 screenpos;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
           
            MouseState ms = Mouse.GetState();           
            mouse_joint.WorldAnchorB = ConvertUnits.ToSimUnits(new Vector2(ms.X, ms.Y)); // next mouse position (-radius)            
            // NOTE: IF SCROLLING IN A WORLD, ADD THE WORLD_OFFSET TO THE COLLISION THINGYS SO IT KNOWS WHERE ON THE MAP THE SCREEN OBJECTS ARE REALLY SUPPOSED TO BE.
            //       [AN OPTIMIZATION IS TO IGNORE UPDATES FOR BODIES THAT ARE TOO FAR OFF-SCREEN (IF NEEDED)]

            //world.Step(0.01666666f); // <-- if assume locked framerate = 60FPS 
            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));

            screenpos = con.ToDisplayUnits(player_body.Position);

            if (screenpos.Y > 768)  player_body.SetTransform(new Vector2(player_body.Position.X, 0f), 0f);
            if (screenpos.Y < 0)    player_body.SetTransform(new Vector2(player_body.Position.X, ConvertUnits.ToSimUnits(768)), 0f);
            if (screenpos.X > 1024) player_body.SetTransform(new Vector2(0f, player_body.Position.Y), 0f);
            if (screenpos.X < 0)    player_body.SetTransform(new Vector2(ConvertUnits.ToSimUnits(1024), player_body.Position.Y), 0f);

            // UPDATE GRASS:
            grassChain[0].UpdateGrass();
            grassChain[1].UpdateGrass();
            grassChain[2].UpdateGrass();
                        
            base.Update(gameTime);
        }


        // D R A W 
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.TransparentBlack);
            quadBatch.Begin(tex, BlendState.NonPremultiplied);

            // draw character first
            quadBatch.Draw(pixel, screenpos - new Vector2(4, 4), new Vector2(0.5f, 0.5f), 9.5f, player_body.Rotation, Color.Red);
            // draw grass
            grassChain[0].Draw(gras1, 1.2f, 0f, 10f, 20f, rotate_base: true);
            grassChain[1].Draw(gras2, 1f,   0f, 0f,  30f, rotate_base: false);
            grassChain[2].Draw(gras3, 1.3f, 0f, 20f, 40f, rotate_base: true);
            // draw big grass as foreground
            quadBatch.Draw(BigGrass, new Vector2(300, 600 - 20), Color.White);              
            
            quadBatch.End(); 
 
            base.Draw(gameTime);
        }
    }
}
