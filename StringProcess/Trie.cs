using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CIExam.Structure;

namespace CIExam.StringProcess
{
    
    //   class Trie2 {
    //     private readonly Trie2[] _children;
    //     private bool _isEnd;
    //
    //     public Trie2() {
    //         _children = new Trie2[26];
    //         _isEnd = false;
    //     }
    //
    //     public void Insert(string word) {
    //         var node = this;
    //         foreach (var index in word.Select(ch => ch - 'a'))
    //         {
    //             node._children[index] ??= new Trie2();
    //             node = node._children[index];
    //         }
    //         node._isEnd = true;
    //     }
    //
    //     public bool Search(string word) {
    //         var node = SearchPrefix(word);
    //         return node is {_isEnd: true};
    //     }
    //
    //     public bool StartsWith(string prefix) {
    //         return SearchPrefix(prefix) != null;
    //     }
    //
    //     private Trie2 SearchPrefix(string prefix) {
    //         var node = this;
    //         foreach (var index in prefix.Select(ch => ch - 'a'))
    //         {
    //             if (node._children[index] == null) {
    //                 return null;
    //             }
    //             node = node._children[index];
    //         }
    //         return node;
    //     }
    // }

    /* 前缀树 是一种树形数据结构，用于高效地存储和检索字符串数据集中的键。这一数据结构有相当多的应用情景，例如自动补完和拼写检查。*/
    public class Trie : IEnumerable<string>
    {
        private readonly MultiTreeNodeNode<char, string> _root = new MultiTreeNodeNode<char, string>();
        /** Initialize your data structure here. */
        public Trie() {
            
        }
        public Trie(IEnumerable<string> words) {
            foreach (var w in words)
            {
                Insert(w);
            }
        }
        /** Inserts a word into the trie. */
        public void Insert(string word)
        {
            var cur = _root;
            var i = 0;
            var n = word.Length;
            while (true)
            {
                if(i >= n)
                    break;
                var ext = cur.ChildrenEnumerator.Select(e => e.Key);
                if (!ext.Contains(word[i]))
                {
                    var nextNode = new MultiTreeNodeNode<char, string>(word[i]);
                    cur.Insert(nextNode);
                    cur = nextNode;
                }
                else
                {
                    foreach (var e in cur.ChildrenEnumerator.Where(e => e.Key == word[i]))
                    {
                        cur = e;
                        break;
                    }
                }
                i++;
            }

            cur.Val = word;

        }
  
    
        /** Returns if the word is in the trie. */
        public bool Search(string word)
        {
            var cur = _root;
            foreach (var t1 in word)
            {
                var ext = cur.ChildrenEnumerator;
                var flag = false;
                foreach (var t in ext.Where(t => t.Key == t1))
                {
                    flag = true;
                    cur = t;
                    break;
                }

                if (!flag)
                    return false;
            }
            
            return cur. Val == word;
        }
    
        /** Returns if there is any word in the trie that starts with the given prefix. */
        public bool StartsWith(string prefix) {
            var cur = _root;
            foreach (var t1 in prefix)
            {
                var ext = cur.ChildrenEnumerator;
                var flag = false;
                foreach (var t in ext.Where(t => t.Key == t1))
                {
                    flag = true;
                    cur = t;
                    break;
                }

                if (!flag)
                    return false;
            }

            return true;
        }

        public IEnumerator<string> GetEnumerator()
        {
            var a = new List<string>();
            _root.PreOrderDfs(delegate(AbstractTreeNode<string> data, object[] objects)
            {
                if(!string.IsNullOrEmpty(data.Data))
                   a.Add(data.Data);
                return null;
            });
            
            return a.GetEnumerator();
        }

        private static void TrieDfs(MultiTreeNodeNode<char, string> root, List<string> ans)
        {
            if (!string.IsNullOrEmpty(root.Val))
            {
                ans.Add(root.Val);
                
            }
            
            foreach (var node in root.ChildrenEnumerator)
            {
                TrieDfs(node, ans);
            }

        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}