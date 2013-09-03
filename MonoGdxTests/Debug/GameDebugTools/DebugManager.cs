#region File Description
//-----------------------------------------------------------------------------
// DebugManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

#endregion

namespace TimeRulerLibrary
{
    /// <summary>
    /// DebugManager class that holds graphics resources for debug.
    /// </summary>
    public class DebugManager : DrawableGameComponent
    {
        #region Properties

        /// <summary>
        /// Gets a content manager for debug.
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// Gets a sprite batch for debug.
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// Gets white texture.
        /// </summary>
        public Texture2D WhiteTexture { get; private set; }

        /// <summary>
        /// Gets SpriteFont for debug.
        /// </summary>
        public SpriteFont DebugFont { get; private set; }

        //public Effect SolidColorEffect { get; private set; }
        public BasicEffect BasicEffect { get; private set; }

        #endregion

        #region Initialize

        public DebugManager( Game game )
            : base( game )
        {
            // Added as a Service.
            Game.Services.AddService( typeof( DebugManager ), this );

            Content = new ContentManager( game.Services );
            Content.RootDirectory = "Content/Debug";

            // This component doesn't need be call neither update nor draw.
            this.Enabled = false;
            this.Visible = false;
        }

        protected override void LoadContent()
        {
            // Load debug content.
            SpriteBatch = new SpriteBatch( GraphicsDevice );

            DebugFont = Content.Load<SpriteFont>( "DebugFont" );

            //SolidColorEffect = Content.Load<Effect>("SolidColor");
            BasicEffect = new BasicEffect(GraphicsDevice);

            // Create white texture.
            WhiteTexture = new Texture2D( GraphicsDevice, 1, 1 );
            Color[] whitePixels = new Color[] { Color.White };
            WhiteTexture.SetData<Color>( whitePixels );

            base.LoadContent();
        }

        #endregion
    }
}