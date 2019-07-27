using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;


namespace space_invaders
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D dot;
        Player player;
        bool bSpacebar = false;
        Texture2D textureAliens;
        Random r = new Random();
        SpriteFont font;

        int time = 0;

        int score = 0;
        bool gameover = false, pause = true, bPKey = false;
        int level = 1, allienspeed = 1;

        List<Bullet> bullets = new List<Bullet>();
        List<Alien> aliens = new List<Alien>();


        int windowWidth = 800, windowHeight = 600;

        class Player
        {
            int hp;
            int speed;
            Vector2 position;
            int width;
            int height;
            Vector2 center;
            int speedShoot;

            public Player()
            {
                hp = 3;
                speed = 5;
                position = new Vector2(360, 570);
                width = 40;
                height = 10;
                center = new Vector2(position.X + (width / 2), position.Y - height);
                speedShoot = 10;
            }

            public Player(int windowWidth, int windowHeight, int shipWidth, int shipHeight)
            {
                hp = 3;
                speed = 1;
                position = new Vector2((windowWidth / 2) - (width / 2), windowHeight - 10);
                width = shipWidth;
                height = shipHeight;
                center = new Vector2(position.X + (width / 2), position.Y - height);

            }

            public void DrawPlayer(SpriteBatch spriteBatch, Texture2D texture, Color color)
            {
                spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y, width, height), color);
                spriteBatch.Draw(texture, new Rectangle((int)position.X + (width / 3), (int)position.Y - height, width / 3, height), color);
            }

            public void MovePlayer(bool moveRight, int windowWidth)
            {
                if (moveRight && position.X + width < windowWidth)
                {
                    position.X += speed;
                }
                else if (!moveRight && position.X > 0)
                {
                    position.X -= speed;
                }
                center = new Vector2(position.X + (width / 2), position.Y - height);
            }

            public Bullet Shoot()
            {
                Bullet bullet = new Bullet(center, -speedShoot, height);
                return bullet;
            }

            public void ChangeHP(int _hp)
            {
                hp += _hp;
            }

            #region Setters

            public void SetPosition(Vector2 pos)
            {
                position = pos;
            }

            #endregion

            #region Getters

            public Vector2 GetPosition()
            {
                return position;
            }

            public Rectangle[] GetHitbox()
            {
                Rectangle[] hitbox = new Rectangle[2];
                hitbox[0] = new Rectangle((int)position.X, (int)position.Y, width, height);
                hitbox[1] = new Rectangle((int)position.X + (width / 3), (int)position.Y - height, width / 3, height);
                return hitbox;
            }

            public int GetHp()
            {
                return hp;
            }

            #endregion

        }

        class Bullet
        {
            Vector2 position;
            int speed;
            int width;
            Vector2 end;
            int size;
            bool exist;

            public Bullet()
            {
                position = new Vector2();
                speed = 0;
                width = 1;
                end = new Vector2();
                size = 0;
                exist = false;
            }

            public Bullet(Vector2 pos, int _speed, int _size)
            {
                position = pos;
                speed = _speed;
                width = 1;
                size = _size;
                end = new Vector2(position.X, position.Y + size);
                exist = true;
            }

            public void DrawBullet(SpriteBatch spriteBatch, Texture2D texture, Color color)
            {
                Rectangle r = new Rectangle((int)position.X, (int)position.Y, (int)(end - position).Length() + width, width);
                Vector2 v = Vector2.Normalize(position - end);
                float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
                if (position.Y > end.Y) angle = MathHelper.TwoPi - angle;
                spriteBatch.Draw(texture, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
            }

            public void MoveBullet(int windowHeight)
            {
                position.Y += speed;
                end.Y += speed;
                if (end.Y < 0)
                    exist = false;
                else if (position.Y > windowHeight)
                    exist = false;
            }

            public int Hit(Player player, Alien alien = null)
            {
                if (speed < 0 && alien != null)
                {
                    Rectangle hitbox = alien.GetHitbox();
                    Rectangle bullet = new Rectangle((int)position.X, (int)position.Y, width, size);
                    if (hitbox.Intersects(bullet))
                    {
                        exist = false;
                        return 1;
                    }
                }
                else if (speed > 0)
                {
                    Rectangle[] hitbox = player.GetHitbox();
                    Rectangle bullet = new Rectangle((int)position.X, (int)position.Y, width, size);
                    if (hitbox[0].Intersects(bullet) || hitbox[1].Intersects(bullet))
                    {
                        exist = false;
                        return 2;
                    }
                }
                return 0;
            }

            public bool GetExist()
            {
                return exist;
            }

            public int GetSpeed()
            {
                return speed;
            }

        }

        class Alien
        {
            Vector2 position;
            int width;
            int height;
            int type;
            int speed;
            bool moveRight;
            int chanceShoot;
            int bulletSpeed;
            public Alien()
            {
                position = new Vector2();
                width = 0;
                height = 0;
                type = 0;
                speed = 0;
                moveRight = true;
                chanceShoot = 0;
                bulletSpeed = 0;
            }

            public Alien(Vector2 _position, int _width, int _height, int _speed, int _chanceShoot, int _bulletSpeed, int _type = 0)
            {
                position = _position;
                width = _width;
                height = _height;
                type = _type;
                speed = _speed;
                moveRight = true;
                chanceShoot = _chanceShoot;
                bulletSpeed = _bulletSpeed;
            }

            public void Draw(SpriteBatch spriteBatch, Texture2D texture, Color color, int time = 0)
            {
                if (type == 0)
                    spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y, width, height),
                        new Rectangle(0, 0, 87, 63), color);
                if (type == 0 && time < 30)
                    spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y, width, height),
                        new Rectangle(89, 0, 177 - 89, 63), color);
            }

            public void Move(int windowWidth)
            {
                if (position.X < 0)
                {
                    moveRight = true;
                    position.Y += height;
                }
                else if (position.X + width > windowWidth)
                {
                    moveRight = false;
                    position.Y += height;
                }

                if (moveRight)
                    position.X += speed;
                else
                    position.X -= speed;
            }

            public Rectangle GetHitbox()
            {
                Rectangle hitbox = new Rectangle((int)position.X, (int)position.Y, width, height);
                return hitbox;
            }

            public Bullet Shoot(Random r)
            {
                Vector2 vec = new Vector2(position.X + (width / 2), position.Y + (height / 2));
                int random = r.Next(0, chanceShoot);
                if (random == 1)
                {
                    return new Bullet(vec, 5, bulletSpeed);
                }
                return null;
            }

        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.ApplyChanges();
            player = new Player();
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 10; j++)
                    aliens.Add(new Alien(new Vector2(j * 50, 40 * (i + 1)), 40, 30, level, 300 - level * 10, 10));
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            dot = new Texture2D(GraphicsDevice, 1, 1);
            dot.SetData<Color>(new Color[] { Color.White });

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            textureAliens = Content.Load<Texture2D>("Spaceinvaders1");
            font = Content.Load<SpriteFont>("SpriteFont1");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            KeyboardState keystate = Keyboard.GetState();
            if (!gameover)
            {
                if (keystate.IsKeyDown(Keys.Left))
                {
                    player.MovePlayer(false, windowWidth);
                    pause = false;
                }
                else if (keystate.IsKeyDown(Keys.Right))
                {
                    player.MovePlayer(true, windowWidth);
                    pause = false;
                }
                if (keystate.IsKeyDown(Keys.Space) && !bSpacebar)
                {
                    bullets.Add(player.Shoot());
                    bSpacebar = true;
                    pause = false;
                }
                if (keystate.IsKeyDown(Keys.P) && !bPKey)
                {
                    pause = !pause;
                    bPKey = true;
                }

                bPKey = keystate.IsKeyDown(Keys.P);
                bSpacebar = keystate.IsKeyDown(Keys.Space);
                if (!pause)
                {
                    //bullet
                    for (int i = 0; i < bullets.Count; i++)
                    {
                        bullets[i].MoveBullet(windowHeight);
                        for (int j = 0; j < aliens.Count; j++)
                        {
                            if (bullets[i].Hit(player, aliens[j]) == 1)
                            {
                                aliens.Remove(aliens[j]);
                                score += 10;
                            }

                        }
                        if (bullets[i].Hit(player) == 2)
                        {
                            player.ChangeHP(-1);
                            pause = true;
                            for (int k = 0; k < bullets.Count;)
                            {
                                bullets.Remove(bullets[k]);
                            }
                            break;
                        }

                        if (!bullets[i].GetExist())
                        {
                            bullets.Remove(bullets[i]);
                        }
                    }

                    //aliens
                    for (int i = 0; i < aliens.Count; i++)
                    {
                        aliens[i].Move(windowWidth);
                        Bullet b = aliens[i].Shoot(r);
                        if (b != null)
                            bullets.Add(b);
                    }

                    time++;
                    if (time == 60)
                        time = 0;
                    if (player.GetHp() <= 0)
                    {
                        gameover = true;
                        pause = false;
                    }
                    else if (aliens.Count == 0)
                    {
                        level++;
                        if (allienspeed < 5)
                            allienspeed++;
                        for (int i = 0; i < 5; i++)
                            for (int j = 0; j < 10; j++)
                                aliens.Add(new Alien(new Vector2(j * 50, 40 * (i + 1)), 40, 30, allienspeed, 300 - level * 10, 10));
                    }

                }
            }
            else
            {
                if (keystate.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            if (!gameover)
            {
                for (int i = 0; i < bullets.Count; i++)
                {
                    bullets[i].DrawBullet(spriteBatch, dot, Color.White);
                }

                for (int i = 0; i < aliens.Count; i++)
                {
                    aliens[i].Draw(spriteBatch, textureAliens, Color.White, time);
                }

                player.DrawPlayer(spriteBatch, dot, Color.Green);

                spriteBatch.DrawString(font, "SCORE: " + score, new Vector2(10, 5), Color.Green);

                spriteBatch.DrawString(font, "LIFES: " + player.GetHp(), new Vector2(200, 5), Color.Red);
            }
            else
            {
                spriteBatch.DrawString(font, "GAME OVER ", new Vector2(300, 200), Color.Red);
                spriteBatch.DrawString(font, "SCORE: " + score, new Vector2(300, 300), Color.Yellow);
            }

            if (pause)
                spriteBatch.DrawString(font, "PAUSE", new Vector2(350, 300), Color.Green);
            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
