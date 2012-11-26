namespace Tetrahedron
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch
open Microsoft.Xna.Framework.Storage
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Media

    module Vector3 =
        let rotateX angle vector =
            Vector3.Transform(vector, Matrix.CreateRotationX(angle))
            
        let rotateY angle vector =
            Vector3.Transform(vector, Matrix.CreateRotationY(angle))
            
        let rotateZ angle vector =
            Vector3.Transform(vector, Matrix.CreateRotationZ(angle))
            
    module FunctionalHelpers =         
        let inline radians a = MathHelper.ToRadians(a)
        let inline degrees a = MathHelper.ToDegrees(a)
        
    open FunctionalHelpers    
    type TetrahedronGame() as x =
        inherit Game()
        let graphics = new GraphicsDeviceManager(x)
        let mutable basicEffect = Unchecked.defaultof<_>
        let mutable texture = Unchecked.defaultof<_>
        let mutable vertexBuffer = Unchecked.defaultof<_>
        let mutable view = Unchecked.defaultof<_>
        let mutable world = Unchecked.defaultof<_>
        let mutable projection = Unchecked.defaultof<_>
        let mutable effect = Unchecked.defaultof<_>
        do x.Content.RootDirectory <- "Content"

        let generateTetrahedron size = 
            let circumSphereRadius = sqrt (3.f/8.f) * size
            let centerVertexAngle = acos (-1.f/3.f)
            let v1 = Vector3(0.f, circumSphereRadius, 0.f)
            let v2 = v1 |> Vector3.rotateX centerVertexAngle
            let v3 = v2 |> Vector3.rotateY (radians 120.f)
            let v4 = v2 |> Vector3.rotateY (-radians 120.f)
            
            let uv1 = Vector2(0.5f, 1.f - sqrt 0.75f)
            let uv2 = Vector2(0.75f, 1.f - (sqrt 0.75f)/2.f)
            let uv3 = Vector2(0.25f, 1.f - (sqrt 0.75f)/2.f)
            let uv4 = Vector2(0.5f, 1.f)
            let uv5 = Vector2.UnitY
            let uv6 = Vector2.One

            [| VertexPositionTexture(v1, uv1)
               VertexPositionTexture(v3, uv2)
               VertexPositionTexture(v2, uv3)  
               
               VertexPositionTexture(v1, uv2)
               VertexPositionTexture(v4, uv6)
               VertexPositionTexture(v3, uv4)

               VertexPositionTexture(v1, uv3)
               VertexPositionTexture(v2, uv4)
               VertexPositionTexture(v4, uv5)

               VertexPositionTexture(v2, uv3)
               VertexPositionTexture(v3, uv2)
               VertexPositionTexture(v4, uv4) |]  
                                   
        override x.Initialize() = base.Initialize()
        
        /// Load graphics content.
        override x.LoadContent() =
            //load texture
            texture <- x.Content.Load<Texture2D>("Tetrahedron")
            
            //world, view, projection
            world <- Matrix.Identity
            view <- Matrix.CreateLookAt(Vector3(0.f, 0.f, 10.f), Vector3.Zero, Vector3.Up)
            projection <- Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                              x.GraphicsDevice.Viewport.AspectRatio,
                                                              1.f,
                                                              1000.f)
                
            basicEffect <- new BasicEffect(x.GraphicsDevice,
                                           World = world,
                                           View = view,
                                           Projection = projection,
                                           Texture = texture,
                                           TextureEnabled = true)
                                             
            let tetrahedronData = generateTetrahedron 4.5f
            vertexBuffer <- new VertexBuffer(x.GraphicsDevice, 
                                             VertexPositionTexture.VertexDeclaration, 
                                             tetrahedronData.Length, 
                                             BufferUsage.WriteOnly)
            vertexBuffer.SetData(tetrahedronData)
            x.GraphicsDevice.SetVertexBuffer(vertexBuffer)

        /// Allows the game to run logic such as updating the world, checking 
        /// for collisions, gathering input, and playing audio.
        override x.Update(gameTime) = 
            let time = float32 gameTime.ElapsedGameTime.TotalSeconds
            if Keyboard.GetState().IsKeyDown(Keys.Escape) then x.Exit()
             
            if Keyboard.GetState().IsKeyDown(Keys.Left) then 
                basicEffect.World <- basicEffect.World * Matrix.CreateRotationX(time * 5.f)  
                
            if Keyboard.GetState().IsKeyDown(Keys.Right) then 
                basicEffect.World <- basicEffect.World * Matrix.CreateRotationX(-time * 5.f)    
                
            if Keyboard.GetState().IsKeyDown(Keys.Up) then 
                basicEffect.World <- basicEffect.World * Matrix.CreateRotationZ(time * 5.f)  
                
            if Keyboard.GetState().IsKeyDown(Keys.Down) then 
                basicEffect.World <- basicEffect.World * Matrix.CreateRotationZ(-time * 5.f) 
                
            let time = float32 gameTime.TotalGameTime.TotalSeconds

            // Compute camera matrices.
            let rotationz = Matrix.CreateRotationY(time * 1.2f)
            basicEffect.View <- rotationz * Matrix.CreateLookAt(Vector3(0.f, 0.f, 10.f), Vector3.Zero, Vector3.Up)
                    
            base.Update (gameTime)

        /// This is called when the game should draw itself. 
        override x.Draw (gameTime) =
            // Clear the backbuffer
            x.GraphicsDevice.Clear (Color.CornflowerBlue)

            for pass in basicEffect.CurrentTechnique.Passes do
                pass.Apply()
                x.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 4)
                 
            base.Draw (gameTime)