using System;
using System.Collections.Generic;
using System.Text;

namespace GenericIdeationObjects
{
    public abstract class IdeationUnit
    {
        public enum IdeaUpdateType
        {
            Position,Content
        }
        protected object _content;
        protected float _centerX;

        public float CenterX
        {
            get { return _centerX; }
            set { _centerX = value; }
        }
        protected float _centerY;

        public float CenterY
        {
            get { return _centerY; }
            set { _centerY = value; }
        }
        protected bool _isAvailable;
        protected int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public bool IsAvailable
        {
            get { return _isAvailable; }
            set { _isAvailable = value; }
        }
        public object Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public virtual IdeationUnit CreateNew()
        {
            return null;
        }
        public virtual IdeationUnit Assign()
        {
            return null;
        }
        public virtual bool UpdatePosition(int x, int y)
        {
            return false;
        }
        public virtual IdeationUnit Delete()
        {
            return null;
        }
        public void SetPosition(int centerX, int centerY)
        {
            _centerX = centerX;
            _centerY = centerY;
        }
        public virtual void updateContent(Object newContent)
        {

        }
        public virtual IdeationUnit Clone()
        {
            return null;
        }
    }
}
