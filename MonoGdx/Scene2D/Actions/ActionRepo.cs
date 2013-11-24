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

using Microsoft.Xna.Framework;
using MonoGdx.Geometry;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.Actions
{
    /// <summary>
    /// Static convenience methods for using pooled actions.
    /// </summary>
    public static class ActionRepo
    {
        public static T Action<T> ()
            where T : SceneAction, new()
        {
            T action = Pools<T>.Obtain();
            action.Pool = Pools<T>.Pool;
            return action;
        }

        public static MoveToAction MoveTo (float x, float y)
        {
            return MoveTo(x, y, 0, null);
        }

        public static MoveToAction MoveTo (float x, float y, float duration)
        {
            return MoveTo(x, y, duration, null);
        }

        public static MoveToAction MoveTo (float x, float y, float duration, Interpolation interpolation)
        {
            MoveToAction action = Action<MoveToAction>();
            action.X = x;
            action.Y = y;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static MoveByAction MoveBy (float amountX, float amountY)
        {
            return MoveBy(amountX, amountY, 0, null);
        }

        public static MoveByAction MoveBy (float amountX, float amountY, float duration)
        {
            return MoveBy(amountX, amountY, duration, null);
        }

        public static MoveByAction MoveBy (float amountX, float amountY, float duration, Interpolation interpolation)
        {
            MoveByAction action = Action<MoveByAction>();
            action.AmountX = amountX;
            action.AmountY = amountY;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static SizeToAction SizeTo (float x, float y)
        {
            return SizeTo(x, y, 0, null);
        }

        public static SizeToAction SizeTo (float x, float y, float duration)
        {
            return SizeTo(x, y, duration, null);
        }

        public static SizeToAction SizeTo (float x, float y, float duration, Interpolation interpolation)
        {
            SizeToAction action = Action<SizeToAction>();
            action.Width = x;
            action.Height = y;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static SizeByAction SizeBy (float amountX, float amountY)
        {
            return SizeBy(amountX, amountY, 0, null);
        }

        public static SizeByAction SizeBy (float amountX, float amountY, float duration)
        {
            return SizeBy(amountX, amountY, duration, null);
        }

        public static SizeByAction SizeBy (float amountX, float amountY, float duration, Interpolation interpolation)
        {
            SizeByAction action = Action<SizeByAction>();
            action.AmountWidth = amountX;
            action.AmountHeight = amountY;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static ScaleToAction ScaleTo (float x, float y)
        {
            return ScaleTo(x, y, 0, null);
        }

        public static ScaleToAction ScaleTo (float x, float y, float duration)
        {
            return ScaleTo(x, y, duration, null);
        }

        public static ScaleToAction ScaleTo (float x, float y, float duration, Interpolation interpolation)
        {
            ScaleToAction action = Action<ScaleToAction>();
            action.X = x;
            action.Y = y;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static ScaleByAction ScaleBy (float amountX, float amountY)
        {
            return ScaleBy(amountX, amountY, 0, null);
        }

        public static ScaleByAction ScaleBy (float amountX, float amountY, float duration)
        {
            return ScaleBy(amountX, amountY, duration, null);
        }

        public static ScaleByAction ScaleBy (float amountX, float amountY, float duration, Interpolation interpolation)
        {
            ScaleByAction action = Action<ScaleByAction>();
            action.AmountX = amountX;
            action.AmountY = amountY;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static RotateToAction RotateTo (float rotation)
        {
            return RotateTo(rotation, 0, null);
        }

        public static RotateToAction RotateTo (float rotation, float duration)
        {
            return RotateTo(rotation, duration, null);
        }

        public static RotateToAction RotateTo (float rotation, float duration, Interpolation interpolation)
        {
            RotateToAction action = Action<RotateToAction>();
            action.Rotation = rotation;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static RotateByAction RotateBy (float rotationAmount)
        {
            return RotateBy(rotationAmount, 0, null);
        }

        public static RotateByAction RotateBy (float rotationAmount, float duration)
        {
            return RotateBy(rotationAmount, duration, null);
        }

        public static RotateByAction RotateBy (float rotationAmount, float duration, Interpolation interpolation)
        {
            RotateByAction action = Action<RotateByAction>();
            action.Amount = rotationAmount;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static ColorAction Color (Color color)
        {
            return Color(color, 0, null);
        }

        public static ColorAction Color (Color color, float duration)
        {
            return Color(color, duration, null);
        }

        public static ColorAction Color (Color color, float duration, Interpolation interpolation)
        {
            ColorAction action = Action<ColorAction>();
            action.EndColor = color;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static AlphaAction Alpha (float a)
        {
            return Alpha(a, 0, null);
        }

        public static AlphaAction Alpha (float a, float duration)
        {
            return Alpha(a, duration, null);
        }

        public static AlphaAction Alpha (float a, float duration, Interpolation interpolation)
        {
            AlphaAction action = Action<AlphaAction>();
            action.Alpha = a;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static AlphaAction FadeOut (float duration)
        {
            return FadeOut(duration, null);
        }

        public static AlphaAction FadeOut (float duration, Interpolation interpolation)
        {
            AlphaAction action = Action<AlphaAction>();
            action.Alpha = 0;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static AlphaAction FadeIn (float duration)
        {
            return FadeIn(duration, null);
        }

        public static AlphaAction FadeIn (float duration, Interpolation interpolation)
        {
            AlphaAction action = Action<AlphaAction>();
            action.Alpha = 1;
            action.Duration = duration;
            action.Interpolation = interpolation;
            return action;
        }

        public static RemoveActorAction RemoveActor ()
        {
            return Action<RemoveActorAction>();
        }

        public static RemoveActorAction RemoveActor (Actor removeActor)
        {
            RemoveActorAction action = Action<RemoveActorAction>();
            action.RemoveActor = removeActor;
            return action;
        }

        public static SequenceAction Sequence (SceneAction action1)
        {
            SequenceAction action = Action<SequenceAction>();
            action.AddAction(action1);
            return action;
        }

        public static SequenceAction Sequence (SceneAction action1, SceneAction action2)
        {
            SequenceAction action = Action<SequenceAction>();
            action.AddAction(action1);
            action.AddAction(action2);
            return action;
        }

        public static SequenceAction Sequence (SceneAction action1, SceneAction action2, SceneAction action3)
        {
            SequenceAction action = Action<SequenceAction>();
            action.AddAction(action1);
            action.AddAction(action2);
            action.AddAction(action3);
            return action;
        }

        public static SequenceAction Sequence (SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4)
        {
            SequenceAction action = Action<SequenceAction>();
            action.AddAction(action1);
            action.AddAction(action2);
            action.AddAction(action3);
            action.AddAction(action4);
            return action;
        }

        public static SequenceAction Sequence (SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4, SceneAction action5)
        {
            SequenceAction action = Action<SequenceAction>();
            action.AddAction(action1);
            action.AddAction(action2);
            action.AddAction(action3);
            action.AddAction(action4);
            action.AddAction(action5);
            return action;
        }

        public static SequenceAction Sequence (params SceneAction[] actions)
        {
            SequenceAction action = Action<SequenceAction>();
            for (int i = 0; i < actions.Length; i++)
                action.AddAction(actions[i]);
            return action;
        }

        public static SequenceAction Sequence ()
        {
            return Action<SequenceAction>();
        }

        public static ParallelAction Parallel (SceneAction action1)
        {
            ParallelAction action = Action<ParallelAction>();
            action.AddAction(action1);
            return action;
        }

        public static ParallelAction Parallel (SceneAction action1, SceneAction action2)
        {
            ParallelAction action = Action<ParallelAction>();
            action.AddAction(action1);
            action.AddAction(action2);
            return action;
        }

        public static ParallelAction Parallel (SceneAction action1, SceneAction action2, SceneAction action3)
        {
            ParallelAction action = Action<ParallelAction>();
            action.AddAction(action1);
            action.AddAction(action2);
            action.AddAction(action3);
            return action;
        }

        public static ParallelAction Parallel (SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4)
        {
            ParallelAction action = Action<ParallelAction>();
            action.AddAction(action1);
            action.AddAction(action2);
            action.AddAction(action3);
            action.AddAction(action4);
            return action;
        }

        public static ParallelAction Parallel (SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4, SceneAction action5)
        {
            ParallelAction action = Action<ParallelAction>();
            action.AddAction(action1);
            action.AddAction(action2);
            action.AddAction(action3);
            action.AddAction(action4);
            action.AddAction(action5);
            return action;
        }

        public static ParallelAction Parallel (params SceneAction[] actions)
        {
            ParallelAction action = Action<ParallelAction>();
            for (int i = 0; i < actions.Length; i++)
                action.AddAction(actions[i]);
            return action;
        }

        public static ParallelAction Parallel ()
        {
            return Action<ParallelAction>();
        }

        public static RemoveListenerAction RemoveListener (EventListener listener, bool capture)
        {
            RemoveListenerAction action = Action<RemoveListenerAction>();
            action.Listener = listener;
            action.Capture = capture;
            return action;
        }

        public static RemoveListenerAction RemoveListener (EventListener listener, bool capture, Actor targetActor)
        {
            RemoveListenerAction action = Action<RemoveListenerAction>();
            action.Listener = listener;
            action.Capture = capture;
            action.TargetActor = targetActor;
            return action;
        }
    }
}
