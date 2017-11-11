using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Node<T>
    {
        public T Value { get; set; }
        public char Key { get; set; }
        public List<Node<T>> Children { get; set; }
        public Node<T> Parent { get; set; }
        public int Depth { get; set; }
        public Node<T> Next { get; set; }

        public Node(char key, int depth, Node<T> parent)
        {
            Key = key;
            Children = new List<Node<T>>();
            Depth = depth;
            Parent = parent;
        }

        public Node(char key, int depth, Node<T> parent, T value)
        {
            Key = key;
            Children = new List<Node<T>>();
            Depth = depth;
            Parent = parent;
            Value = value;
        }

        public bool IsLeaf()
        {
            return Children.Count == 0;
        }

        public Node<T> FindChildNode(char c)
        {
            foreach (var child in Children)
            {
                if (child.Key == c)
                {
                    return child;
                }
            }

            return null;
        }

        public void AddChildNode(char c, T value)
        {
            
        }

        public void AddChildNode(Node<T> node)
        {
            Node<T> previous = this.Children.LastOrDefault();
            this.Children.Add(node);

            if (previous != null)
            {
                previous.Next = node;
            }
        }

        public void DeleteChildNode(char c)
        {
            for (var i = 0; i < Children.Count; i++)
            {
                if (Children[i].Key == c)
                {
                    Children.RemoveAt(i);
                }
            }
        }
    }

    public class TrieEnumerator<T> : IEnumerator<Node<T>>
    {
        protected Node<T> _root;
        protected Node<T> _parent;
        protected Node<T> _current;
        protected int _index = 0;

        public TrieEnumerator(Node<T> root)
        {
            this._root = root;
            this.Reset();
        }

        public Node<T> Current => this._current;

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            this._root = null;
            this._parent = null;
            this._current = null;
        }

        public bool MoveNext()
        {
            /**
             * Initial case
             */
            if (this._current == this._root)
            {
                if (this._index != 0)
                {
                    return false;
                }
                else
                {
                    this._current = this.findLeftMost(this._root);
                    this._parent = this._current.Parent;
                    this._index++;

                    return true;
                }
            }

            /**
             * Has siblings - go to sibling
             */
            if (this._current.Next != null)
            {
                this._current = this.findLeftMost(this._current.Next);
            }
            /**
             * Special case - prefix is also part of trie
             * ( "ahoj", "ahojky" )
             */
            else if (this._current.Children.Count > 1)
            {
                this._current = this.findLeftMost(this._current.Children[1]);
                this._parent = this._current.Parent;
            }
            else
            {
                var newRoot = this._current.Parent;

                while (newRoot.Next == null && newRoot != this._root)
                {
                    newRoot = newRoot.Parent;
                }

                this._parent = newRoot.Next;
                this._current = this.findLeftMost(this._parent);
            }

            return this._current != null;
        }

        protected Node<T> findLeftMost(Node<T> from)
        {
            if (from == null) { return null;  }

            if (from.IsLeaf())
            {
                return from.Parent;
            }

            return this.findLeftMost(from.Children[0]);
        }

        public void Reset()
        {
            _current = _root; // this.findLeftMost(_root);
            _parent = _current.Parent;
            _index = 0;
        }
    }

    public class DuplicateElementException : Exception
    {
        public DuplicateElementException(string message) : base(message)
        {
        }
    }

    public class Trie<T> : IEnumerable<Node<T>>
    {
        private readonly Node<T> _root;

        public Trie()
        {
            _root = new Node<T>('^', 0, null);
        }

        public int Count { get; protected set; }

        public Node<T> Prefix(string s)
        {
            var currentNode = _root;
            var result = currentNode;

            foreach (var c in s)
            {
                currentNode = currentNode.FindChildNode(c);

                if (currentNode == null)
                {
                    break;
                }

                result = currentNode;
            }

            return result;
        }

        public T Get(string s)
        {
            var prefix = Prefix(s);

            if (prefix.Depth == s.Length && prefix.FindChildNode('$') != null)
            {
                return prefix.Value;
            }
            else
            {
                return default(T);
            }
        }

        public bool Contains(string s)
        {
            var prefix = Prefix(s);
            return prefix.Depth == s.Length && prefix.FindChildNode('$') != null;
        }

        public void InsertRange(List<Tuple<string, T>> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                Insert(items[i].Item1, items[i].Item2);
            }
        }

        public void Insert(string s, T value)
        {
            var commonPrefix = Prefix(s);
            var current = commonPrefix;

            if (current.Depth == s.Length)
            {
                throw new DuplicateElementException("Item already in trie");
            }

            for (var i = current.Depth; i < s.Length; i++)
            {
                var newNode = new Node<T>(s[i], current.Depth + 1, current, value);
                current.AddChildNode(newNode);
                current = newNode;
            }

            current.AddChildNode(new Node<T>('$', current.Depth + 1, current));
            this.Count++;
        }

        public void Delete(string s)
        {
            if (Contains(s))
            {
                var node = Prefix(s).FindChildNode('$');

                while (node.IsLeaf())
                {
                    var parent = node.Parent;
                    parent.DeleteChildNode(node.Key);
                    node = parent;
                }
            }
        }

        public IEnumerator<Node<T>> GetEnumerator()
        {
            return new TrieEnumerator<T>(this._root);
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
