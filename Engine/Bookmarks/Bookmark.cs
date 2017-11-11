using System;
using System.Collections.Generic;

namespace Engine
{
    public class Bookmark : BookmarkNode, IComparable<Bookmark>
    {
        public string Url
        {
            get;
            set;
        }

        public int LastVisited
        {
            get;
            set;
        }

        public int CountVisits
        {
            get;
            set;
        }

        public List<Tag> Tags
        {
            get;
            set;            
        }

        public Bookmark()
        {
            this.Tags = new List<Tag>();
        }

        public void AddTag(Tag tag)
        {
            this.Tags.Add(tag);
            tag.AddBookmark(this);
        }

        public void RemoveTag(Tag tag)
        {
            this.Tags.Remove(tag);
            tag.RemoveBookmark(this);
        }

        public override BookmarkType Type { get { return BookmarkType.Bookmark; } }

        public override void Accept(IBookmarkVisitor visitor)
        {
            visitor.Visit(this);
        }

        public int CompareTo(Bookmark other)
        {
            return this.Id.CompareTo(other.Id);
        }
    }

    public enum BookmarkType
    {
        Bookmark,
        Directory
    }
}
