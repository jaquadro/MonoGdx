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
using MonoGdx.Utils;

namespace MonoGdx.Scene2D
{
    public abstract class SceneAction : IPoolable
    {
        private Actor _actor;
        private Pool _pool;

        public abstract bool Act (float delta);

        public virtual Actor Actor
        {
            get { return _actor; }
            set
            {
                _actor = value;
                if (_actor == null && _pool != null) {
                    _pool.Release(this);
                    _pool = null;
                }
            }
        }

        public Pool Pool
        {
            get { return _pool; }
            set { _pool = value; }
        }

        public virtual void Restart ()
        { }

        public virtual void Reset ()
        {
            Restart();
        }

        public override string ToString ()
        {
            string name = GetType().Name;
            int index = name.LastIndexOf('.');
            if (index != -1)
                name = name.Substring(index + 1);
            if (name.EndsWith("Action"))
                name = name.Substring(0, name.Length - 6);
            return name;
        }
    }
}
