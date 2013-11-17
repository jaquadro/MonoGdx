using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGdx.Scene2D;
using MonoGdx.Scene2D.UI;
using MonoGdx.Scene2D.Utils;
using NUnit.Framework;

namespace MonoGdxTests.Tests
{
    [TestFixture]
    class TreeTest : GdxTest
    {
        [Test]
        public void Run ()
        {
            using (GdxTestContext context = new GdxTestContext(this)) {
                context.Run();
            }
        }

        private Stage _stage;

        protected override void InitializeCore ()
        {
            ShowDebug = true;

            _stage = new Stage(Context.GraphicsDevice);
            Context.Input.Processor = _stage;

            Skin skin = new Skin(Context.GraphicsDevice, "Data/uiskin.json");

            Table table = new Table();
            table.SetFillParent(true);
            _stage.AddActor(table);

            Tree tree = new Tree(skin);

            TreeNode node1 = new TreeNode(new TextButton("moo1", skin));
            TreeNode node2 = new TreeNode(new TextButton("moo2", skin));
            TreeNode node3 = new TreeNode(new TextButton("moo3", skin));
            TreeNode node4 = new TreeNode(new TextButton("moo4", skin));
            TreeNode node5 = new TreeNode(new TextButton("moo5", skin));

            tree.Add(node1);
            tree.Add(node2);
            node2.Add(node3);
            node3.Add(node4);
            tree.Add(node5);

            (node5.Actor as Button).Clicked += (sender, e) => {
                tree.Remove(node4);
            };

            //node5.Actor.AddListener(new DispatchClickListener() {
            //    OnClicked = (ev, x, y) => { tree.Remove(node4); }
            //});

            table.Add(tree).Configure.Fill().Expand();

            //Debugger.Launch();
        }

        protected override void UpdateCore (GameTime gameTime)
        {
            _stage.Act((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        protected override void DrawCore (GameTime gameTime)
        {
            Context.GraphicsDevice.Clear(Color.Black);
            _stage.Draw();
        }
    }
}
