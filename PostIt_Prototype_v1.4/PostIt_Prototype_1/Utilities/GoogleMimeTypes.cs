using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.Utilities
{
    public static class GoogleMimeTypes
    {
        public static readonly GoogleMimeType FolderMimeType = new GoogleMimeType("application/vnd.google-apps.folder");
        public static readonly GoogleMimeType FileMimeType = new GoogleMimeType("application/vnd.google-apps.file");
    }

    public class GoogleMimeType 
    {
        private readonly string _value;

        public GoogleMimeType(string value)
        {
            _value = value;
        }

        public static implicit operator string(GoogleMimeType g)
        {
            return g._value;
        }

        public override string ToString()
        {
            return _value;
        }

        public override bool Equals(object obj)
        {
            return obj.ToString().Equals(_value);
        }

        protected bool Equals(GoogleMimeType other)
        {
            return string.Equals(_value, other._value);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }
    }
}
