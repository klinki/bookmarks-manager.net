using Microsoft.VisualStudio.TestTools.UnitTesting;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Tests
{
    [TestClass()]
    public class TrieTests
    {
        protected String GetString<T>(Node<T> node)
        {
            string res = "";

            do
            {
                res += node.Key;
                node = node.Parent;
            } while (node != null && node.Key != '^');

            char[] charArr = res.ToCharArray();
            Array.Reverse(charArr);

            return new string(charArr);
        }

        [TestMethod()]
        public void GetEnumeratorTest()
        {
            Trie<int> trie = new Trie<int>();

            trie.Insert("ahoj", 0);
            trie.Insert("ahold", 1);
            trie.Insert("aha", 2);
            trie.Insert("ahmed", 3);
            trie.Insert("asic", 4);
            trie.Insert("baba", 5);
            trie.Insert("bakala", 6);

            foreach (Node<int> node in trie)
            {
                string str = this.GetString(node);
                Console.WriteLine(str);
            }

            var actual = trie.Take(7).Select(item => item.Value).ToArray();
            var expected = new int[] { 0, 1, 2, 3, 4, 5, 6 };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void HandlingDuplicitiesTest()
        {
            Trie<int> trie = new Trie<int>();

            try
            {
                trie.Insert("ahoj", 0);
                trie.Insert("ahoj", 1);
                Assert.Fail("Expected exception to be thrown");
            }
            catch (DuplicateElementException e)
            {
                Assert.AreEqual(1, trie.Count);
            }

            var actual = trie.Take(1).Select(item => item.Value).ToArray();
            var expected = new int[] { 0 };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void HandlingPrefixPartOfTreeTest()
        {
            Trie<int> trie = new Trie<int>();

            trie.Insert("ahoj", 0);
            trie.Insert("ahojky", 1);

            foreach (Node<int> node in trie)
            {
                string str = this.GetString(node);
                Console.WriteLine(str);
            }

            var actual = trie.Take(7).Select(item => item.Value).ToArray();
            var expected = new int[] { 0, 1 };

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
