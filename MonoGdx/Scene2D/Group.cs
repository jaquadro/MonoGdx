/**
 * Copyright 2011-2013 See AUTHORS file.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGdx.Geometry;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D
{
    public class Group : Actor, ICullable
    {
        private RectangleF? _cullingArea;
        private Matrix3 _worldTransform;
        private Matrix _oldBatchTransform;

        public Group ()
        {
            Children = new SnapshotList<Actor>(4);
            IsTransform = true;
        }

        public override void Act (float delta)
        {
            base.Act(delta);

            foreach (Actor actor in Children.Begin())
                actor.Act(delta);
            Children.End();
        }

        public override void Draw (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            if (IsTransform)
                ApplyTransform(spriteBatch, ComputeTransform());

            DrawChildren(spriteBatch, parentAlpha);

            if (IsTransform)
                ResetTransform(spriteBatch);
        }

        protected void DrawChildren (GdxSpriteBatch spriteBatch, float parentAlpha)
        {
            parentAlpha *= Color.A / 255f;

            IList<Actor> actors = Children.Begin();
            if (_cullingArea != null) {
                RectangleF cull = _cullingArea.Value;
                float cullLeft = cull.X;
                float cullRight = cullLeft + cull.Width;
                float cullBottom = cull.Y;
                float cullTop = cullBottom + cull.Height;

                // Draw children only if inside the culling area.
                if (IsTransform) {
                    foreach (Actor child in actors) {
                        if (!child.IsVisible)
                            continue;

                        float cx = child.X;
                        float cy = child.Y;
                        if (cx <= cullRight && cy <= cullTop && cx + child.Width >= cullLeft && cy + child.Height >= cullBottom)
                            child.Draw(spriteBatch, parentAlpha);
                    }
                    spriteBatch.Flush();
                }
                else {
                    // No transform for this group, offset each child.
                    float offsetX = X;
                    float offsetY = Y;
                    X = 0;
                    Y = 0;

                    foreach (Actor child in actors) {
                        if (!child.IsVisible)
                            continue;

                        float cx = child.X;
                        float cy = child.Y;
                        if (cx <= cullRight && cy <= cullTop && cx + child.Width >= cullLeft && cy + child.Height >= cullBottom) {
                            child.X = cx + offsetX;
                            child.Y = cy + offsetY;
                            child.Draw(spriteBatch, parentAlpha);
                            child.X = cx;
                            child.Y = cy;
                        }
                    }

                    X = offsetX;
                    Y = offsetY;
                }
            }
            else {
                // No culling, draw all children
                if (IsTransform) {
                    foreach (Actor child in actors) {
                        if (!child.IsVisible)
                            continue;
                        child.Draw(spriteBatch, parentAlpha);
                    }
                    spriteBatch.Flush();
                }
                else {
                    // No transform for this group, offset each child.
                    float offsetX = X;
                    float offsetY = Y;
                    X = 0;
                    Y = 0;

                    foreach (Actor child in actors) {
                        if (!child.IsVisible)
                            continue;

                        float cx = child.X;
                        float cy = child.Y;

                        child.X = cx + offsetX;
                        child.Y = cy + offsetY;
                        child.Draw(spriteBatch, parentAlpha);
                        child.X = cx;
                        child.Y = cy;
                    }

                    X = offsetX;
                    Y = offsetY;
                }
            }
            Children.End();
        }

        protected void ApplyTransform (GdxSpriteBatch spriteBatch, Matrix transform)
        {
            _oldBatchTransform = spriteBatch.TransformMatrix;
            spriteBatch.TransformMatrix = transform;
        }

        protected Matrix ComputeTransform ()
        {
            Matrix3 localTransform = (OriginX != 0 || OriginY != 0) 
                ? Matrix3.CreateTranslation(OriginX, OriginY) 
                : Matrix3.Identity;

            if (Rotation != 0)
                localTransform = Matrix3.CreateRotation(Rotation) * localTransform;
            if (ScaleX != 1 || ScaleY != 1)
                localTransform = Matrix3.CreateScale(ScaleX, ScaleY) * localTransform;
            if (OriginX != 0 || OriginY != 0)
                localTransform = Matrix3.CreateTranslation(-OriginX, -OriginY) * localTransform;
            localTransform.Translate(X, Y);

            // Find the first parent that transforms
            Group parentGroup = Parent;
            while (parentGroup != null) {
                if (parentGroup.IsTransform)
                    break;
                parentGroup = parentGroup.Parent;
            }

            _worldTransform = (parentGroup != null) ? localTransform * parentGroup._worldTransform : localTransform;

            return _worldTransform.ToMatrix();
        }

        protected void ResetTransform (GdxSpriteBatch spriteBatch)
        {
            spriteBatch.TransformMatrix = _oldBatchTransform;
        }

        public void SetCullingArea (RectangleF cullingArea)
        {
            _cullingArea = cullingArea;
        }

        public override Actor Hit (float x, float y, bool touchable)
        {
            if (touchable && Touchable == Scene2D.Touchable.Disabled)
                return null;

            for (int i = Children.Count - 1; i >= 0; i--) {
                Actor child = Children[i];
                if (!child.IsVisible)
                    continue;

                Vector2 point = child.ParentToLocalCoordinates(new Vector2(x, y));
                Actor hit = child.Hit(point.X, point.Y, touchable);
                if (hit != null)
                    return hit;
            }

            return base.Hit(x, y, touchable);
        }

        protected virtual void ChildrenChanged ()
        { }

        public virtual void AddActor (Actor actor)
        {
            actor.Remove();
            Children.Add(actor);
            actor.Parent = this;
            actor.Stage = Stage;
            ChildrenChanged();
        }

        public virtual void AddActorAt (int index, Actor actor)
        {
            actor.Remove();
            if (index >= Children.Count)
                Children.Add(actor);
            else
                Children.Insert(index, actor);

            actor.Parent = this;
            actor.Stage = Stage;
            ChildrenChanged();
        }

        public virtual void AddActorBefore (Actor actorBefore, Actor actor)
        {
            actor.Remove();
            int index = Children.IndexOf(actorBefore);
            Children.Insert(index, actor);
            actor.Parent = this;
            actor.Stage = Stage;
            ChildrenChanged();
        }

        public virtual void AddActorAfter (Actor actorAfter, Actor actor)
        {
            actor.Remove();
            int index = Children.IndexOf(actorAfter);
            if (index == Children.Count)
                Children.Add(actor);
            else
                Children.Insert(index + 1, actor);

            actor.Parent = this;
            actor.Stage = Stage;
            ChildrenChanged();
        }

        public virtual bool RemoveActor (Actor actor)
        {
            if (!Children.Remove(actor))
                return false;

            if (Stage != null)
                Stage.Unfocus(actor);

            actor.Parent = null;
            actor.Stage = null;
            ChildrenChanged();

            return true;
        }

        public virtual void ClearChildren ()
        {
            foreach (Actor child in Children.Begin()) {
                child.Stage = null;
                child.Parent = null;
            }
            Children.End();
            Children.Clear();
            ChildrenChanged();
        }

        public override void Clear ()
        {
            base.Clear();
            ClearChildren();
        }

        public Actor FindActor (string name)
        {
            foreach (Actor child in Children) {
                if (name == child.Name)
                    return child;
            }

            foreach (Actor child in Children) {
                if (child is Group) {
                    Actor actor = (child as Group).FindActor(name);
                    if (actor != null)
                        return actor;
                }
            }

            return null;
        }

        public override Stage Stage
        {
            get { return base.Stage; }
            protected internal set {
                base.Stage = value;
                foreach (Actor child in Children)
                    child.Stage = value;
            }
        }

        public bool SwapActor (int first, int second)
        {
            if (first < 0 || first >= Children.Count)
                return false;
            if (second < 0 || second >= Children.Count)
                return false;

            Actor child = Children[first];
            Children[first] = Children[second];
            Children[second] = child;

            return true;
        }

        public bool SwapActor (Actor first, Actor second)
        {
            int firstIndex = Children.IndexOf(first);
            int secondIndex = Children.IndexOf(second);

            return SwapActor(firstIndex, secondIndex);
        }

        public SnapshotList<Actor> Children { get; private set; }

        public bool HasChildren
        {
            get { return Children.Count > 0; }
        }

        public bool IsTransform { get; set; }

        public Vector2 LocalToDescendantCoordinates (Actor descendant, Vector2 localCoords)
        {
            if (Parent == null)
                throw new ArgumentException("Child is not a descendant: " + descendant);
            if (Parent != this)
                localCoords = LocalToDescendantCoordinates(Parent, localCoords);

            return descendant.ParentToLocalCoordinates(localCoords);
        }

        public void Print ()
        {
            Print("");
        }

        private void Print (string indent)
        {
            foreach (Actor child in Children.Begin()) {
                Console.WriteLine(indent + child);
                if (child is Group)
                    (child as Group).Print(indent + "|  ");
            }
            Children.End();
        }
    }
}
