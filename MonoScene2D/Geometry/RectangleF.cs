using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

namespace MonoScene2D.Geometry
{
    [DataContract]
    public struct RectangleF
    {
        private static RectangleF emptyRectangle = new RectangleF();

        [DataMember]
        public float X;

        [DataMember]
        public float Y;

        [DataMember]
        public float Width;

        [DataMember]
        public float Height;

        public static RectangleF Empty
        {
            get { return emptyRectangle; }
        }

        public float Left
        {
            get { return this.X; }
        }

        public float Right
        {
            get { return (this.X + this.Width); }
        }

        public float Top
        {
            get { return this.Y; }
        }

        public float Bottom
        {
            get { return (this.Y + this.Height); }
        }

        public RectangleF(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public static bool operator ==(RectangleF a, RectangleF b)
        {
            return ((a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height));
        }

		public bool Contains(int x, int y)
        {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }

        public bool Contains(float x, float y)
        {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }
		
        public bool Contains(Point value)
        {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        public bool Contains(Vector2 value)
        {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        public bool Contains (Rectangle value)
        {
            return ((((this.X <= value.X) && ((value.X + value.Width) <= (this.X + this.Width))) && (this.Y <= value.Y)) && ((value.Y + value.Height) <= (this.Y + this.Height)));
        }

        public bool Contains(RectangleF value)
        {
            return ((((this.X <= value.X) && ((value.X + value.Width) <= (this.X + this.Width))) && (this.Y <= value.Y)) && ((value.Y + value.Height) <= (this.Y + this.Height)));
        }

        public static bool operator !=(RectangleF a, RectangleF b)
        {
            return !(a == b);
        }

        public void Offset(Vector2 offset)
        {
            X += offset.X;
            Y += offset.Y;
        }

        public void Offset (float offsetX, float offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }
		
		public Vector2 Location
		{
			get 
			{
				return new Vector2(this.X, this.Y);
			}
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}
		
		public Vector2 Center
		{
			get 
			{
				return new Vector2(this.X + (this.Width / 2), this.Y + (this.Height / 2));
			}
		}

        public void Inflate (float horizontalValue, float verticalValue)
        {
            X -= horizontalValue;
            Y -= verticalValue;
            Width += horizontalValue * 2;
            Height += verticalValue * 2;
        }
		
		public bool IsEmpty
        {
            get
            {
                return ((((this.Width == 0) && (this.Height == 0)) && (this.X == 0)) && (this.Y == 0));
            }
        }

        public bool Equals(RectangleF other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return (obj is RectangleF) ? this == ((RectangleF)obj) : false;
        }

        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1} Width:{2} Height:{3}}}", X, Y, Width, Height);
        }

        public override int GetHashCode()
        {
            return ((int)this.X ^ (int)this.Y ^ (int)this.Width ^ (int)this.Height);
        }

        public bool Intersects(RectangleF value)
        {
            return value.Left < Right       && 
                   Left       < value.Right && 
                   value.Top  < Bottom      &&
                   Top        < value.Bottom;            
        }


        public void Intersects(ref RectangleF value, out bool result)
        {
            result = value.Left < Right       && 
                     Left       < value.Right && 
                     value.Top  < Bottom      &&
                     Top        < value.Bottom;
        }

        public static RectangleF Intersect(RectangleF value1, RectangleF value2)
        {
            RectangleF rectangle;
            Intersect(ref value1, ref value2, out rectangle);
            return rectangle;
        }


        public static void Intersect(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
        {
            if (value1.Intersects(value2))
            {
                float right_side = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                float left_side = Math.Max(value1.X, value2.X);
                float top_side = Math.Max(value1.Y, value2.Y);
                float bottom_side = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
                result = new RectangleF(left_side, top_side, right_side - left_side, bottom_side - top_side);
            }
            else
            {
                result = new RectangleF(0, 0, 0, 0);
            }
        }
		
		public static RectangleF Union(RectangleF value1, RectangleF value2)
		{
            float x = Math.Min(value1.X, value2.X);
            float y = Math.Min(value1.Y, value2.Y);
			return new RectangleF(x, y,
			                     Math.Max (value1.Right, value2.Right) - x,
				                     Math.Max (value1.Bottom, value2.Bottom) - y);
		}
		
		public static void Union(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
		{
			result.X = Math.Min (value1.X, value2.X);
			result.Y = Math.Min (value1.Y, value2.Y);
			result.Width = Math.Max (value1.Right, value2.Right) - result.X;
			result.Height = Math.Max (value1.Bottom, value2.Bottom) - result.Y;
		}
    }
}
