using AlienScribble;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using VelcroPhysics.Collision.Filtering;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.Joints;
using VelcroPhysics.Factories;

using con = VelcroPhysics.Utilities.ConvertUnits;


namespace VelcroGrass {    

    class VelcroChain {
        QuadBatch   quadBatch;
        World       world;
        List<Body>  bods;
        List<WeldJoint> joints;
        List<Vector2> vecs;
        Vector2     sim_pos, base_pivot;
        float       sx, sy;        

        // C O N S T R U C T 
        public VelcroChain(World worldSys, QuadBatch qbatch)
        {
            world  = worldSys;
            quadBatch = qbatch;
            bods   = new List<Body>();
            joints = new List<WeldJoint>();
            vecs   = new List<Vector2>();
        }


        // M A K E   G R A S S
        #region summary:
        /// <summary>Make a vertical chain (like grass) which starts at the bottom.</summary>
        /// <param name="width">    How wide is the image (ie: sourceRect.Width * 0.75f = 75% of source image's width)</param>
        /// <param name="height">   Height of the image</param>
        /// <param name="start_pos">Pivot position in game's world space for the base of the grass or tentacle or whatever it is (in display units)</param>
        /// <param name="num_divisions">Quantity of segments to use</param>
        /// <param name="noGravity">True by default but could add a gravity factor</param>
        /// <param name="baseBody"> What body this grass(or whatever) is welded to</param>
        /// <param name="category"> What category of collider this object is</param>
        /// <param name="collidesWith">What other collider types can this thing collide with</param>
        /// <param name="damp">     Adjust dampening</param>
        /// <param name="hz">       Frequency of calculation</param>
        #endregion 
        public void MakeGrass(float width, float height, Vector2 start_pos, int num_divisions, Body baseBody, bool noGravity = true,
            Category category = Category.Cat3, Category collidesWith = Category.Cat1, float damp = 0.95f, float hz = 16f)
        {
            if (num_divisions < 1) num_divisions = 1;
            sx = start_pos.X; sy = start_pos.Y; base_pivot = start_pos;   // regular start_pos
            sim_pos = con.ToSimUnits(start_pos);                          // set the grass base pivot position (in sim units)                  
            float sim_seg_width  = con.ToSimUnits(width * 0.9f);
            float seg_height     = height / num_divisions;
            float sim_seg_height = con.ToSimUnits(seg_height);      // divide into parts  
            float start_v = seg_height / 2f;                 // mid-point of first segment

            int i = 0;
            while (i < num_divisions) {
                Body bod = BodyFactory.CreateRectangle(world, sim_seg_width, sim_seg_height, 1f);    // make a rectangle collider
                bod.Position = con.ToSimUnits(sx, sy - start_v - i * seg_height);         // use middle for first segment
                bod.BodyType = BodyType.Dynamic;
                bod.Mass        = 0.05f;             // lower number for more stiff & higher number for loose
                bod.Restitution = 0.3f;
                bod.Friction    = 0.3f;
                bod.IgnoreGravity = noGravity;
                bod.CollisionCategories = category; bod.CollisionCategories = collidesWith;
                WeldJoint joint;
                if (i == 0) joint = JointFactory.CreateWeldJoint(world, baseBody, bod, sim_pos, sim_pos, true);
                else joint = JointFactory.CreateWeldJoint(world, bods[i - 1], bod,
                             con.ToSimUnits(new Vector2(sx, sy - seg_height * i)),
                             con.ToSimUnits(new Vector2(sx, sy - seg_height * i)), true);
                joint.CollideConnected = false;
                joint.FrequencyHz  = hz;
                joint.DampingRatio = damp;
                bods.Add(bod);                        // store the body
                joints.Add(joint);                    // store the joint
                Vector2 vec = Vector2.Zero;           // add an empty vec for update to use
                vecs.Add(vec);
                i++;
            }            
        }



        // U P D A T E 
        public void UpdateGrass()
        {
            int i = 0;
            Vector2 pivot = base_pivot;
            while (i < bods.Count) {
                if (i > 0) pivot += vecs[i - 1];                                 // add the vectors together to form a chain of connected bones                
                vecs[i] = (con.ToDisplayUnits(bods[i].Position) - pivot) * 2;    // * 2 because the position is the middle of the segment                
                i++;
            }               
        }



        // D R A W 
        public void Draw(Rectangle source, float scale_width, float sideways_offset, float base_scale, float tip_scale, bool rotate_base)
        {            
            quadBatch.DrawChainedGrass(source, base_pivot, vecs, Color.White, scale_width, sideways_offset, 
                                        tip_scale, base_scale , rotate_base, SpriteEffects.None); 
        }
    }
}
