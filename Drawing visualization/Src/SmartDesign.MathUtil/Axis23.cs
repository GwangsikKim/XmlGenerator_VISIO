using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Axis23
    {
        public static readonly Axis23 OXY = new Axis23(Position3.O, Vector3.OZ, Vector3.OX);

        public Axis23(Position3 location, Vector3 zDirection, Vector3 xReferenceDirection)
        {
            Location = location;
            this.zDirection = zDirection;

            if (Vector3.IsParallel(zDirection, xReferenceDirection))
                throw new ArgumentException("두 방향 벡터가 평행합니다.");

            Vector3 projX = zDirection * Vector3.Dot(xReferenceDirection, zDirection);
            Vector3 vecX = xReferenceDirection - projX;
            this.xDirection = Vector3.Normalize(vecX);
            this.yDirection = Vector3.Normalize(Vector3.Cross(this.zDirection, this.xDirection));
        }


        public Position3 Location { get; set; }

        private Vector3 xDirection;
        public Vector3 XDirection
        {
            get { return xDirection; }
            set
            {
                if (Vector3.IsParallel(this.zDirection, value))
                    throw new ArgumentException("두 방향 벡터가 평행합니다.");

                Vector3 projX = this.zDirection * Vector3.Dot(value, this.zDirection);
                Vector3 vecX = value - projX;
                this.xDirection = Vector3.Normalize(vecX);
                this.yDirection = Vector3.Normalize(Vector3.Cross(this.zDirection, this.xDirection));
            }
        }

        private Vector3 yDirection;
        public Vector3 YDirection
        {
            get { return yDirection; }
            set
            {
                if (Vector3.IsParallel(this.zDirection, value))
                    throw new ArgumentException("두 방향 벡터가 평행합니다.");

                Vector3 projY = this.zDirection * Vector3.Dot(value, this.zDirection);
                Vector3 vecY = value - projY;
                this.yDirection = Vector3.Normalize(vecY);
                this.xDirection = Vector3.Normalize(Vector3.Cross(this.yDirection, this.zDirection));
            }
        }

        private Vector3 zDirection;
        public Vector3 ZDirection
        {
            get { return zDirection; }
            set
            {
                if (Vector3.IsEqual(value, this.xDirection))
                {
                    this.xDirection = this.yDirection;
                    this.yDirection = this.zDirection;
                    this.zDirection = value;
                }
                else if (Vector3.IsOpposite(value, this.xDirection))
                {
                    this.xDirection = this.zDirection;
                    this.zDirection = value;
                }
                else
                {
                    Vector3 projX = value * Vector3.Dot(this.xDirection, value);
                    Vector3 vecX = this.xDirection - projX;
                    this.xDirection = Vector3.Normalize(vecX);
                    this.yDirection = Vector3.Normalize(Vector3.Cross(value, this.xDirection));
                    this.zDirection = value;
                }
            }
        }
    }
}
