using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;
using MonoGdx.Graphics.G2D;
using MonoGdx.Scene2D.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

            Load(skinFile);
        }

        public Skin (string skinFile, TextureAtlas atlas)
        {
            Atlas = atlas;
            AddRegions(atlas);
            Load(skinFile);
        }

        public Skin (TextureAtlas atlas)
        {
            Atlas = atlas;
            AddRegions(atlas);
        }

        public TextureAtlas Atlas { get; internal set; }

        [TODO]
        public void Load (string skinFile)
        {
            /*try {
                getJsonLoader(skinFile).fromJson(Skin.class, skinFile);
            } catch (SerializationException ex) {
                throw new SerializationException("Error reading file: " + skinFile, ex);
            }*/
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
            if (name == null)
                throw new ArgumentNullException("name");

            // GROSS!!!
            if (typeof(T) == typeof(ISceneDrawable))
                return (T)(object)GetDrawable(name);
            if (typeof(T) == typeof(TextureRegion))
                return (T)(object)GetRegion(name);
            if (typeof(T) == typeof(NinePatch))
                return (T)(object)GetPatch(name);
            if (typeof(T) == typeof(Sprite))
                return (T)(object)GetSprite(name);

            Dictionary<string, object> typeResources;
            if (!_resources.TryGetValue(typeof(T), out typeResources))
                throw new Exception("No " + typeof(T) + " registered with name: " + name);

            object resource;
            if (!typeResources.TryGetValue(name, out resource))
                throw new Exception("No " + typeof(T) + " registered with name: " + name);

            return (T)resource;
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
            Dictionary<string, object> typeResources;
            if (!_resources.TryGetValue(typeof(T), out typeResources))
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

            Texture2D texture = Optional<Texture2D>(name);
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

        //protected Json GetJsonLoader (string skinFile)
        //{
            /*Newtonsoft.Json.JsonSerializer s = new Newtonsoft.Json.JsonSerializer();
            
            using (TextReader reader = new StreamReader(skinFile)) {
                JObject root = JObject.Parse(reader.ReadToEnd());

                var colorProxy = { 

                JObject colorNode = root["com.badlogic.gdx.graphics.Color"] as JObject;
                foreach (JProperty p in colorNode.Properties()) {
                    string hex = p["hex"].ToObject<string>();
                    if (hex != null)
                        Add(p.Name, Xna.Color.FromHex(hex));
                    float 
                }

                root["com.badlogic.gdx.graphics.Color"].Select(t => {
                    string name = (t as JProperty).Name;
                    object value = ((t as JProperty).Value as JValue).Value;

                    if (value is string)
                        Add(name, Get<Xna.Color>(value as string));
                    else {
                        if (t.
                    }
                });
            }*/

            //throw new NotImplementedException();

            /*final Skin skin = this;

            final Json json = new Json() {
                public <T> T readValue (Class<T> type, Class elementType, JsonValue jsonData) {
                    // If the JSON is a string but the type is not, look up the actual value by name.
                    if (jsonData.isString() && !ClassReflection.isAssignableFrom(CharSequence.class, type)) return get(jsonData.asString(), type);
                    return super.readValue(type, elementType, jsonData);
                }
            };
            json.setTypeName(null);
            json.setUsePrototypes(false);

            json.setSerializer(Skin.class, new ReadOnlySerializer<Skin>() {
                public Skin read (Json json, JsonValue typeToValueMap, Class ignored) {
                    for (JsonValue valueMap = typeToValueMap.child(); valueMap != null; valueMap = valueMap.next()) {
                        try {
                            readNamedObjects(json, ClassReflection.forName(valueMap.name()), valueMap);
                        } catch (ReflectionException ex) {
                            throw new SerializationException(ex);
                        }
                    }
                    return skin;
                }

                private void readNamedObjects (Json json, Class type, JsonValue valueMap) {
                    Class addType = type == TintedDrawable.class ? Drawable.class : type;
                    for (JsonValue valueEntry = valueMap.child(); valueEntry != null; valueEntry = valueEntry.next()) {
                        Object object = json.readValue(type, valueEntry);
                        if (object == null) continue;
                        try {
                            add(valueEntry.name(), object, addType);
                        } catch (Exception ex) {
                            throw new SerializationException("Error reading " + ClassReflection.getSimpleName(type) + ": " + valueEntry.name(), ex);
                        }
                    }
                }
            });

            json.setSerializer(BitmapFont.class, new ReadOnlySerializer<BitmapFont>() {
                public BitmapFont read (Json json, JsonValue jsonData, Class type) {
                    String path = json.readValue("file", String.class, jsonData);

                    FileHandle fontFile = skinFile.parent().child(path);
                    if (!fontFile.exists()) fontFile = Gdx.files.internal(path);
                    if (!fontFile.exists()) throw new SerializationException("Font file not found: " + fontFile);

                    // Use a region with the same name as the font, else use a PNG file in the same directory as the FNT file.
                    String regionName = fontFile.nameWithoutExtension();
                    try {
                        TextureRegion region = skin.optional(regionName, TextureRegion.class);
                        if (region != null)
                            return new BitmapFont(fontFile, region, false);
                        else {
                            FileHandle imageFile = fontFile.parent().child(regionName + ".png");
                            if (imageFile.exists())
                                return new BitmapFont(fontFile, imageFile, false);
                            else
                                return new BitmapFont(fontFile, false);
                        }
                    } catch (RuntimeException ex) {
                        throw new SerializationException("Error loading bitmap font: " + fontFile, ex);
                    }
                }
            });

            json.setSerializer(Color.class, new ReadOnlySerializer<Color>() {
                public Color read (Json json, JsonValue jsonData, Class type) {
                    if (jsonData.isString()) return get(jsonData.asString(), Color.class);
                    String hex = json.readValue("hex", String.class, (String)null, jsonData);
                    if (hex != null) return Color.valueOf(hex);
                    float r = json.readValue("r", float.class, 0f, jsonData);
                    float g = json.readValue("g", float.class, 0f, jsonData);
                    float b = json.readValue("b", float.class, 0f, jsonData);
                    float a = json.readValue("a", float.class, 1f, jsonData);
                    return new Color(r, g, b, a);
                }
            });

            json.setSerializer(TintedDrawable.class, new ReadOnlySerializer() {
                public Object read (Json json, JsonValue jsonData, Class type) {
                    String name = json.readValue("name", String.class, jsonData);
                    Color color = json.readValue("color", Color.class, jsonData);
                    return newDrawable(name, color);
                }
            });

            return json;*/
        //}

        /*static private Method FindMethod (Type type, string name)
        {
            throw new NotImplementedException();
        }*/

        public struct TintedDrawable
        {
            public string Name { get; set; }
            public Xna.Color Color { get; set; }
        }
    }
}
