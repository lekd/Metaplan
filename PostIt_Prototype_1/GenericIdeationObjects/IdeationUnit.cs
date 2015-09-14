using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericIdeationObjects
{
    public abstract class IdeationUnit
    {
        protected object _content;
        protected float _left;

        public float Left
        {
            get { return _left; }
            set { _left = value; }
        }
        protected float _top;

        public float Top
        {
            get { return _top; }
            set { _top = value; }
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
        public void SetPosition(int left, int top)
        {
            _left = left;
            _top = top;
        }
        public virtual void updateContent(Object newContent)
        {

        }
    }
}
