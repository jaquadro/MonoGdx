using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGdx.Geometry;
using MonoGdx.Utils;

namespace MonoGdx.Scene2D.Actions
{
    public static class ActionRepo
    {
        public static T Action<T> ()
            where T : SceneAction, new()
        {
            T action = Pools<T>.Obtain();
            action.Pool = Pools<T>.Pool;
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
