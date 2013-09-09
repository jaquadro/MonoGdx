using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using MonoGdx.Utils;
using Xna = Microsoft.Xna.Framework;

namespace MonoGdx.Scene2D.UI
{
    public class Skin : IDisposable
    {
        private Dictionary<Type, Dictionary<string, object>> _resources = new Dictionary<Type, Dictionary<string, object>>();

        public Skin ()
        { }

        public Skin (GraphicsDevice graphicsDevice, string skinFile)
        {
            string atlasFile = Path.Combine(Path.GetDirectoryName(skinFile), Path.GetFileNameWithoutExtension(skinFile) + ".atlas");
            if (File.Exists(atlasFile)) {
                Atlas = new TextureAtlas(graphicsDevice, atlasFile);
                AddRegions(Atlas);
            }

            Load(graphicsDevice, skinFile);
        }

        public Skin (GraphicsDevice graphicsDevice, string skinFile, TextureAtlas atlas)
        {
            Atlas = atlas;
            AddRegions(atlas);
            Load(graphicsDevice, skinFile);
        }

        public Skin (TextureAtlas atlas)
        {
            Atlas = atlas;
            AddRegions(atlas);
        }

        public TextureAtlas Atlas { get; internal set; }

        public void Load (GraphicsDevice graphicsDevice, string skinFile)
        {
            string root = Path.GetDirectoryName(skinFile);

            try {
                using (TextReader reader = new StreamReader(skinFile)) {
                    LoadFromJson(graphicsDevice, reader, root);
                }
            }
            catch (Exception e) {
                throw new IOException("Error reading file: " + skinFile, e);
            }
        }

        public void AddRegions (TextureAtlas atlas)
        {
            foreach (var region in atlas.Regions)
                Add(region.Name, region, typeof(TextureRegion));
        }

        public void Add (string name, object resource)
        {
            Add(name, resource, resource.GetType());
        }

        public void Add<T> (string name, T resource)
        {
            Add(name, resource, typeof(T));
        }

        public void Add (string name, object resource, Type type)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (resource == null)
                throw new ArgumentNullException("resource");

            Dictionary<string, object> typeResources;
            if (!_resources.TryGetValue(type, out typeResources)) {
                typeResources = new Dictionary<string,object>();
                _resources[type] = typeResources;
            }

            typeResources[name] = resource;
        }

        public T Get<T> ()
        {
            return Get<T>("default");
        }

        public T Get<T> (string name)
        {
            return (T)Get(name, typeof(T));
        }

        public object Get (string name, Type type)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (type == typeof(ISceneDrawable))
                return GetDrawable(name);
            if (type == typeof(TextureRegion))
                return GetRegion(name);
            if (type == typeof(NinePatch))
                return GetPatch(name);
            if (type == typeof(Sprite))
                return GetSprite(name);

            Dictionary<string, object> typeResources;
            if (!_resources.TryGetValue(type, out typeResources))
                throw new Exception("No " + type + " registered with name: " + name);

            object resource;
            if (!typeResources.TryGetValue(name, out resource))
                throw new Exception("No " + type + " registered with name: " + name);

            return resource;
        }

        public T Optional<T> (string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Dictionary<string, object> typeResources;
            if (!_resources.TryGetValue(typeof(T), out typeResources))
                return default(T);

            object resource;
            typeResources.TryGetValue(name, out resource);
            return (T)resource;
        }

        public bool Has<T> (string name)
        {
            return Has(name, typeof(T));
        }

        public bool Has (string name, Type type)
        {
            Dictionary<string, object> typeResources;
            if (!_resources.TryGetValue(type, out typeResources))
                return false;

            return typeResources.ContainsKey(name);
        }

        public IDictionary<string, T> GetAll<T> ()
        {
            Dictionary<string, object> typeResources;
            _resources.TryGetValue(typeof(T), out typeResources);

            return (IDictionary<string, T>)typeResources;
        }

        public Xna.Color GetColor (string name)
        {
            return Get<Xna.Color>(name);
        }

        public BitmapFont GetFont (string name)
        {
            return Get<BitmapFont>(name);
        }

        public TextureRegion GetRegion (string name)
        {
            TextureRegion region = Optional<TextureRegion>(name);
            if (region != null)
                return region;

            TextureContext texture = Optional<TextureContext>(name);
            if (texture == null)
                throw new Exception("No TextureRegion or Texture registered with name: " + name);

            region = new TextureRegion(texture);
            Add<TextureRegion>(name, region);
            return region;
        }

        public TiledDrawable GetTiledDrawable (string name)
        {
            TiledDrawable tiled = Optional<TiledDrawable>(name);
            if (tiled != null)
                return tiled;

            ISceneDrawable drawable = Optional<ISceneDrawable>(name);
            if (drawable != null) {
                if (!(drawable is TiledDrawable))
                    throw new Exception("Drawable found but is not a TiledDrawable: " + name + ", " + drawable.GetType());
                return drawable as TiledDrawable;
            }

            try {
                tiled = new TiledDrawable(GetRegion(name));
                Add<TiledDrawable>(name, tiled);
                return tiled;
            }
            catch {
                throw new Exception("No TiledDrawable, Drawable, TextureRegion, or Texture registered with name: " + name);
            }
        }

        public NinePatch GetPatch (string name)
        {
            NinePatch patch = Optional<NinePatch>(name);
            if (patch != null)
                return patch;

            try {
                TextureRegion region = GetRegion(name);
                if (region is TextureAtlas.AtlasRegion) {
                    int[] splits = (region as TextureAtlas.AtlasRegion).Splits;
                    if (splits != null) {
                        patch = new NinePatch(region, splits[0], splits[1], splits[2], splits[3]);
                        int[] pads = (region as TextureAtlas.AtlasRegion).Pads;
                        if (pads != null)
                            patch.SetPadding(pads[0], pads[1], pads[2], pads[3]);
                    }
                }

                if (patch == null)
                    patch = new NinePatch(region);

                Add<NinePatch>(name, patch);
                return patch;
            }
            catch {
                throw new Exception("No NinePatch, TextureRegion, or Texture registered with name: " + name);
            }
        }

        public Sprite GetSprite (string name)
        {
            Sprite sprite = Optional<Sprite>(name);
            if (sprite != null)
                return sprite;

            try {
                TextureRegion textureRegion = GetRegion(name);
                if (textureRegion is TextureAtlas.AtlasRegion) {
                    TextureAtlas.AtlasRegion region = textureRegion as TextureAtlas.AtlasRegion;
                    if (region.Rotate || region.PackedWidth != region.OriginalWidth || region.PackedHeight != region.OriginalHeight)
                        sprite = new TextureAtlas.AtlasSprite(region);
                }

                if (sprite == null)
                    sprite = new Sprite(textureRegion);

                Add<Sprite>(name, sprite);
                return sprite;
            }
            catch {
                throw new Exception("No Sprite, TextureRegion, or Texture registered with name: " + name);
            }
        }

        public ISceneDrawable GetDrawable (string name)
        {
            ISceneDrawable drawable = Optional<ISceneDrawable>(name);
            if (drawable != null)
                return drawable;

            drawable = Optional<TiledDrawable>(name);
            if (drawable != null)
                return drawable;

            try {
                TextureRegion textureRegion = GetRegion(name);
                if (textureRegion is TextureAtlas.AtlasRegion) {
                    TextureAtlas.AtlasRegion region = textureRegion as TextureAtlas.AtlasRegion;
                    if (region.Splits != null)
                        drawable = new NinePatchDrawable(GetPatch(name));
                    else if (region.Rotate || region.PackedWidth != region.OriginalWidth || region.PackedHeight != region.OriginalHeight)
                        drawable = new SpriteDrawable(GetSprite(name));
                }

                if (drawable == null)
                    drawable = new TextureRegionDrawable(textureRegion);
            }
            catch { /* ignore */ }

            if (drawable == null) {
                NinePatch patch = Optional<NinePatch>(name);
                if (patch != null)
                    drawable = new NinePatchDrawable(patch);
                else {
                    Sprite sprite = Optional<Sprite>(name);
                    if (sprite != null)
                        drawable = new SpriteDrawable(sprite);
                    else
                        throw new Exception("No Drawable, NinePatch, TextureRegion, Texture, or Sprite registered with name: " + name);
                }
            }

            Add<ISceneDrawable>(name, drawable);
            return drawable;
        }

        public string Find (object resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            Dictionary<string, object> typeResources;
            if (!_resources.TryGetValue(resource.GetType(), out typeResources))
                return null;

            if (!typeResources.ContainsValue(resource))
                return null;

            return typeResources.First(kv => kv.Value == resource).Key;
        }

        public ISceneDrawable NewDrawable (string name)
        {
            return NewDrawable(GetDrawable(name));
        }

        public ISceneDrawable NewDrawable (string name, float r, float g, float b, float a)
        {
            return NewDrawable(GetDrawable(name), new Xna.Color(r, g, b, a));
        }

        public ISceneDrawable NewDrawable (string name, Xna.Color tint)
        {
            return NewDrawable(GetDrawable(name), tint);
        }

        public ISceneDrawable NewDrawable (ISceneDrawable drawable)
        {
            if (drawable is TextureRegionDrawable)
                return new TextureRegionDrawable(drawable as TextureRegionDrawable);
            if (drawable is NinePatchDrawable)
                return new NinePatchDrawable(drawable as NinePatchDrawable);
            if (drawable is SpriteDrawable)
                return new SpriteDrawable(drawable as SpriteDrawable);

            throw new Exception("Unable to copy, unknown drawable type: " + drawable.GetType());
        }

        public ISceneDrawable NewDrawable (ISceneDrawable drawable, float r, float g, float b, float a)
        {
            return NewDrawable(drawable, new Xna.Color(r, g, b, a));
        }

        public ISceneDrawable NewDrawable (ISceneDrawable drawable, Xna.Color tint)
        {
            if (drawable is TextureRegionDrawable) {
                TextureRegion region = (drawable as TextureRegionDrawable).Region;
                Sprite sprite;
                if (region is TextureAtlas.AtlasRegion)
                    sprite = new TextureAtlas.AtlasSprite(region as TextureAtlas.AtlasRegion);
                else
                    sprite = new Sprite(region);

                sprite.Color = tint;
                return new SpriteDrawable(sprite);
            }

            if (drawable is NinePatchDrawable) {
                NinePatchDrawable patchDrawable = new NinePatchDrawable(drawable as NinePatchDrawable);
                patchDrawable.Patch = new NinePatch(patchDrawable.Patch, tint);
                return patchDrawable;
            }

            if (drawable is SpriteDrawable) {
                SpriteDrawable spriteDrawable = new SpriteDrawable(drawable as SpriteDrawable);
                Sprite sprite = spriteDrawable.Sprite;

                if (sprite is TextureAtlas.AtlasSprite)
                    sprite = new TextureAtlas.AtlasSprite(sprite as TextureAtlas.AtlasSprite);
                else
                    sprite = new Sprite(sprite);

                sprite.Color = tint;
                spriteDrawable.Sprite = sprite;
                return spriteDrawable;
            }

            throw new Exception("Unable to copy, unknown drawbale type: " + drawable.GetType());
        }

        public void SetEnabled (Actor actor, bool enabled)
        {
            throw new NotImplementedException();
        }

        public void Dispose ()
        {
            if (Atlas != null)
                Atlas.Dispose();

            foreach (var entry in _resources.Values) {
                foreach (object resource in entry.Values) {
                    if (resource is IDisposable)
                        (resource as IDisposable).Dispose();
                }
            }
        }

        public static Dictionary<string, string> GdxTypeMap = new Dictionary<string, string> {
            { "com.badlogic.gdx.graphics.Color", "MonoGdx.Scene2D.UI.GdxColor" },
            { "Microsoft.Xna.Framework.Color", "MonoGdx.Scene2D.UI.GdxColor" },
            { "com.badlogic.gdx.graphics.g2d.BitmapFont", "MonoGdx.Graphics.G2D.BitmapFont" },
            { "com.badlogic.gdx.scenes.scene2d.ui.Skin$TintedDrawable", "MonoGdx.Scene2D.UI.TintedDrawable" },
            { "com.badlogic.gdx.scenes.scene2d.ui.Button$ButtonStyle", "MonoGdx.Scene2D.UI.ButtonStyle" },
            { "com.badlogic.gdx.scenes.scene2d.ui.TextButton$TextButtonStyle", "MonoGdx.Scene2D.UI.TextButtonStyle" },
            { "com.badlogic.gdx.scenes.scene2d.ui.ScrollPane$ScrollPaneStyle", "MonoGdx.Scene2D.UI.ScrollPaneStyle" },
            //{ "com.badlogic.gdx.scenes.scene2d.ui.SelectBox$SelectBoxStyle", "MonoGdx.Scene2D.UI.SelectBoxStyle" },
            //{ "com.badlogic.gdx.scenes.scene2d.ui.SplitPane$SplitPaneStyle", "MonoGdx.Scene2D.UI.SplitPaneStyle" },
            { "com.badlogic.gdx.scenes.scene2d.ui.Window$WindowStyle", "MonoGdx.Scene2D.UI.WindowStyle" },
            { "com.badlogic.gdx.scenes.scene2d.ui.Slider$SliderStyle", "MonoGdx.Scene2D.UI.SliderStyle" },
            { "com.badlogic.gdx.scenes.scene2d.ui.Label$LabelStyle", "MonoGdx.Scene2D.UI.LabelStyle" },
            //{ "com.badlogic.gdx.scenes.scene2d.ui.TextField$TextFieldStyle", "MonoGdx.Scene2D.UI.TextFieldStyle" },
            { "com.badlogic.gdx.scenes.scene2d.ui.CheckBox$CheckBoxStyle", "MonoGdx.Scene2D.UI.CheckBoxStyle" },
            //{ "com.badlogic.gdx.scenes.scene2d.ui.List$ListStyle", "MonoGdx.Scene2D.UI.ListStyle" },
            //{ "com.badlogic.gdx.scenes.scene2d.ui.Touchpad$TouchpadStyle", "MonoGdx.Scene2D.UI.TouchpadStyle" },
            { "com.badlogic.gdx.scenes.scene2d.ui.Tree$TreeStyle", "MonoGdx.Scene2D.UI.TreeStyle" },
        };

        private void LoadFromJson (GraphicsDevice graphicsDevice, TextReader reader, string rootPath)
        {
            Dictionary<string, object> data = Json.Deserialize(reader) as Dictionary<string, object>;
            if (data == null)
                return;

            foreach (var objectGroup in data) {
                if (!(objectGroup.Value is Dictionary<string, object>))
                    continue;

                string typeName = GdxTypeMap.ContainsKey(objectGroup.Key) ? GdxTypeMap[objectGroup.Key] : objectGroup.Key;
                Type type = Type.GetType(typeName);
                if (type == null)
                    continue;

                foreach (var item in objectGroup.Value as Dictionary<string, object>) {
                    if (type == typeof(GdxColor)) {
                        Color? objVal = LoadColor(item.Value as Dictionary<string, object>);
                        if (objVal != null) {
                            Add<Color>(item.Key, objVal.Value);
                            Add<Nullable<Color>>(item.Key, objVal);
                        }
                        continue;
                    }
                    else if (type == typeof(BitmapFont)) {
                        BitmapFont objVal = LoadBitmapFont(graphicsDevice, item.Value as Dictionary<string, object>, rootPath);
                        if (objVal != null)
                            Add<BitmapFont>(item.Key, objVal);
                        continue;
                    }
                    else if (type == typeof(TintedDrawable)) {
                        ISceneDrawable objVal = LoadTintededDrawable(item.Value as Dictionary<string, object>);
                        if (objVal != null)
                            Add<ISceneDrawable>(item.Key, objVal);
                    }

                    object genObj = LoadObject(graphicsDevice, type, item.Value as Dictionary<string, object>, rootPath);
                    if (genObj != null)
                        Add(item.Key, genObj, type);
                }
            }
        }

        private Color? LoadColor (Dictionary<string, object> data)
        {
            try {
                float a = (float)data["a"];
                float r = (float)data["r"];
                float g = (float)data["g"];
                float b = (float)data["b"];
                
                return new Color(r, g, b, a);
            }
            catch {
                return null;
            }
        }

        private ISceneDrawable LoadTintededDrawable (Dictionary<string, object> data)
        {
            try {
                string name = data["name"] as string;
                Color? color = LoadColor(data["color"] as Dictionary<string, object>);
                return (name != null && color != null) ? NewDrawable(name, color.Value) : null;
            }
            catch {
                return null;
            }
        }

        private BitmapFont LoadBitmapFont (GraphicsDevice device, Dictionary<string, object> data, string rootPath)
        {
            if (!data.ContainsKey("file"))
                throw new IOException("No font file defined");

            string path = Path.Combine(rootPath, data["file"] as string);
            if (!File.Exists(path))
                throw new IOException("Font file not found: " + path);

            string regionName = Path.GetFileNameWithoutExtension(path);
            try {
                TextureRegion region = Optional<TextureRegion>(regionName);
                if (region != null)
                    return new BitmapFont(device, path, region, false);
                else {
                    string imagePath = Path.Combine(Path.GetDirectoryName(path), regionName + ".png");
                    if (File.Exists(imagePath))
                        return new BitmapFont(device, path, imagePath, false);
                    else
                        return new BitmapFont(device, path, false);
                }

            }
            catch (Exception e) {
                throw new IOException("Error loading bitmap font: " + path, e);
            }
        }

        private object LoadObject (GraphicsDevice device, Type type, Dictionary<string, object> data, string rootPath)
        {
            ConstructorInfo constructInfo = type.GetConstructor(Type.EmptyTypes);
            if (constructInfo == null)
                return null;

            object obj = constructInfo.Invoke(null);

            foreach (var dataItem in data) {
                PropertyInfo propInfo = type.GetProperty(dataItem.Key);
                if (propInfo == null) {
                    propInfo = type.GetProperty(dataItem.Key.Substring(0, 1).ToUpper() + dataItem.Key.Substring(1));
                    if (propInfo == null)
                        continue;
                }

                // Reference Type
                if (dataItem.Value is string && propInfo.PropertyType != typeof(string)) {
                    string refName = dataItem.Value as string;
                    propInfo.SetValue(obj, Get(refName, propInfo.PropertyType), null);
                    continue;
                }

                // Specialized Object Type
                if (dataItem.Value is Dictionary<string, object>) {
                    if (propInfo.PropertyType == typeof(Color)) {
                        Color? objVal = LoadColor(dataItem.Value as Dictionary<string, object>);
                        if (objVal != null)
                            propInfo.SetValue(obj, objVal, null);
                        continue;
                    }
                    if (propInfo.PropertyType == typeof(BitmapFont)) {
                        BitmapFont objVal = LoadBitmapFont(device, dataItem.Value as Dictionary<string, object>, rootPath);
                        if (objVal != null)
                            propInfo.SetValue(obj, objVal, null);
                        continue;
                    }
                }

                // Unspecialized Object Type
                if (dataItem.Value is Dictionary<string, object>) {
                    object objVal = LoadObject(device, propInfo.PropertyType, dataItem.Value as Dictionary<string, object>, rootPath);
                    if (objVal != null)
                        propInfo.SetValue(obj, objVal, null);
                    continue;
                }

                // Primitive Type
                try {
                    object objVal = Convert.ChangeType(dataItem.Value, propInfo.PropertyType);
                    if (objVal != null)
                        propInfo.SetValue(obj, objVal, null);
                }
                catch { }
            }

            return obj;
        }
    }

    public struct TintedDrawable
    {
        public string Name { get; set; }
        public Xna.Color Color { get; set; }
    }

    internal class GdxColor
    { }
}
